import sys
from pathlib import Path

import pytest

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))

from webcontent import pipeline, progress


def _job(tmp_path: Path, name: str, settings: str = "v1") -> pipeline.Job:
    source = tmp_path / "content" / f"{name}.txt"
    source.parent.mkdir(parents=True, exist_ok=True)
    source.write_text(name)
    out_rel = f"sub/{name}.out"
    return pipeline.Job(source, out_rel, tmp_path / "out" / out_rel, settings)


def _copy(job: pipeline.Job) -> None:
    job.out_path.write_bytes(job.source.read_bytes())


def test_converts_every_job_and_records_a_stamp(tmp_path):
    jobs = [_job(tmp_path, name) for name in ("a", "b", "c")]
    entries: dict[str, str] = {}

    converted, skipped = pipeline.run_stage(
        "t", jobs, _copy, entries, cpu_bound=False
    )

    assert (converted, skipped) == (3, 0)
    assert [job.out_path.read_text() for job in jobs] == ["a", "b", "c"]
    assert sorted(entries) == ["sub/a.out", "sub/b.out", "sub/c.out"]


def test_second_run_skips_what_is_already_current(tmp_path):
    jobs = [_job(tmp_path, name) for name in ("a", "b")]
    entries: dict[str, str] = {}

    pipeline.run_stage("t", jobs, _copy, entries, cpu_bound=False)
    converted, skipped = pipeline.run_stage(
        "t", jobs, _copy, entries, cpu_bound=False
    )

    assert (converted, skipped) == (0, 2)


def test_a_missing_output_is_rebuilt_even_with_a_matching_stamp(tmp_path):
    jobs = [_job(tmp_path, "a")]
    entries: dict[str, str] = {}
    pipeline.run_stage("t", jobs, _copy, entries, cpu_bound=False)
    jobs[0].out_path.unlink()

    converted, skipped = pipeline.run_stage(
        "t", jobs, _copy, entries, cpu_bound=False
    )

    assert (converted, skipped) == (1, 0)


def test_changed_settings_invalidate_the_output(tmp_path):
    entries: dict[str, str] = {}
    pipeline.run_stage("t", [_job(tmp_path, "a")], _copy, entries, cpu_bound=False)

    converted, _ = pipeline.run_stage(
        "t", [_job(tmp_path, "a", settings="v2")], _copy, entries, cpu_bound=False
    )

    assert converted == 1


def test_a_lone_job_runs_inline(tmp_path):
    """A closure cannot be pickled, so this only passes without a process pool.

    Spawning one costs more than the conversion, and the common rerun -- one asset
    edited -- is exactly this case.
    """
    job = _job(tmp_path, "a")
    written = []

    def convert(target: pipeline.Job) -> None:
        written.append(target.out_rel)
        _copy(target)

    converted, _ = pipeline.run_stage("t", [job], convert, {}, cpu_bound=True)

    assert converted == 1
    assert written == ["sub/a.out"]


def test_a_failing_job_aborts_the_stage_and_records_no_stamp(tmp_path):
    jobs = [_job(tmp_path, name) for name in ("a", "b")]
    entries: dict[str, str] = {}

    def convert(job: pipeline.Job) -> None:
        raise RuntimeError("encoder said no")

    with pytest.raises(RuntimeError, match="encoder said no"):
        pipeline.run_stage("t", jobs, convert, entries, cpu_bound=False)

    assert entries == {}


def test_progress_counts_skipped_and_converted_against_one_total(tmp_path):
    class Recorder(progress.Reporter):
        def __init__(self) -> None:
            self.total = 0
            self.names: list[str] = []

        def start(self, label: str, total: int) -> None:
            self.total = total

        def advance(self, name: str) -> None:
            self.names.append(name)

    jobs = [_job(tmp_path, name) for name in ("a", "b")]
    entries: dict[str, str] = {}
    pipeline.run_stage("t", jobs[:1], _copy, entries, cpu_bound=False)

    report = Recorder()
    pipeline.run_stage("t", jobs, _copy, entries, report, cpu_bound=False)

    assert report.total == 2
    assert sorted(report.names) == ["sub/a.out", "sub/b.out"]


def test_worker_count_never_exceeds_the_work_or_the_limit():
    assert pipeline._worker_count(1, None) == 1
    assert pipeline._worker_count(1000, 2) == 2
    assert pipeline._worker_count(3, 1000) == 3
