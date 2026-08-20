"""The stage driver shared by every converter.

All four converters do the same four things to a list of files: stamp the source, skip
it when the stamp and the output are both still valid, convert what is left, and record
the new stamp. Only the per-file conversion differs, so it is the only thing a converter
passes in.

Conversions run concurrently, because no output depends on any other. Which pool
depends on where the work happens: an in-process encoder (Pillow, fontTools) is held by
the GIL and needs separate processes, while ffmpeg is already its own process and needs
only a thread to wait on it.
"""

from __future__ import annotations

import os
from collections.abc import Callable, Iterable, Iterator
from concurrent.futures import Executor, ProcessPoolExecutor, ThreadPoolExecutor
from dataclasses import dataclass
from pathlib import Path

from . import manifest, progress


@dataclass(frozen=True)
class Job:
    """One source file, where its converted form belongs, and how it is encoded.

    `settings` goes into the stamp, so changing an encoder setting invalidates the
    output without needing a clean build.
    """

    source: Path
    out_rel: str
    out_path: Path
    settings: str


def _executor(cpu_bound: bool, workers: int) -> Executor:
    if cpu_bound:
        return ProcessPoolExecutor(max_workers=workers)
    return ThreadPoolExecutor(max_workers=workers)


def _worker_count(pending: int, limit: int | None) -> int:
    workers = min(pending, os.cpu_count() or 1)
    if limit is not None:
        workers = min(workers, limit)
    return max(workers, 1)


def run_stage(
    label: str,
    jobs: Iterable[Job],
    convert: Callable[[Job], None],
    entries: dict[str, str],
    report: progress.Reporter = progress.SILENT,
    *,
    cpu_bound: bool = True,
    max_workers: int | None = None,
) -> tuple[int, int]:
    """Runs one conversion stage. Returns (converted, skipped).

    `convert` is handed a job and must write `job.out_path`; its parent directory
    already exists. Under a process pool it runs in a child, so it has to be a
    module-level function, or a partial of one over picklable arguments.
    """
    todo = list(jobs)
    report.start(label, len(todo))
    try:
        pending: list[tuple[Job, str]] = []
        skipped = 0
        for job in todo:
            stamp = manifest.stamp_for(job.source, job.settings)
            if manifest.is_current(entries, job.out_rel, stamp, job.out_path):
                skipped += 1
                report.advance(job.out_rel)
                continue
            pending.append((job, stamp))

        for job, _stamp in pending:
            job.out_path.parent.mkdir(parents=True, exist_ok=True)

        for job, stamp in _convert_all(pending, convert, cpu_bound, max_workers):
            entries[job.out_rel] = stamp
            report.advance(job.out_rel)
    finally:
        report.finish()

    return len(pending), skipped


def _convert_all(
    pending: list[tuple[Job, str]],
    convert: Callable[[Job], None],
    cpu_bound: bool,
    max_workers: int | None,
) -> Iterator[tuple[Job, str]]:
    """Converts every pending job, yielding each one that succeeded, in order.

    A lone job is run inline: spawning a pool to convert one file costs more than the
    conversion, and the common rerun -- one asset edited -- is exactly that case.
    """
    if not pending:
        return

    if len(pending) == 1:
        job, stamp = pending[0]
        convert(job)
        yield job, stamp
        return

    workers = _worker_count(len(pending), max_workers)
    with _executor(cpu_bound, workers) as pool:
        futures = [(job, stamp, pool.submit(convert, job)) for job, stamp in pending]
        for job, stamp, future in futures:
            # Re-raises whatever the worker raised, which aborts the stage and the
            # build. Jobs that already finished keep their stamps; the rest go
            # unrecorded, so the next run retries them.
            future.result()
            yield job, stamp
