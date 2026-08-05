#!/usr/bin/env python3
"""Build a Windows release package for Cut the Rope DX.

Windows ships two builds of the game and a launcher that chooses between them, because the graphics
backend is fixed when the game is compiled: the Vulkan and OpenGL builds reference different MonoGame
assemblies exporting the same types, so one process cannot hold both. The OpenGL build exists for
machines whose Vulkan is missing or software-only, which on Intel means anything before Skylake.

Ahead-of-time compilation folds the managed assemblies into each executable, leaving only native
libraries beside them, and the two backends need disjoint ones: mgruntime for Vulkan, SDL and OpenAL
for OpenGL. Nothing collides, so both builds share one directory:

    CutTheRope-DX.exe     launcher: probes Vulkan, runs one of the builds below
    CutTheRopeDX.vk.exe   Vulkan build      + mgruntime.dll
    CutTheRopeDX.gl.exe   OpenGL build      + SDL2.dll, openal.dll, ...
    ffmpeg/  content/     one copy, shared

Without ahead-of-time compilation the loose managed assemblies do collide: both builds produce a
MonoGame.Framework.dll of the same name and different content. That layout gets a directory per
backend instead, and the launcher accepts either:

    CutTheRope-DX.exe   launcher
    content/            shared; the game looks one directory up for it
    vk/  gl/            a build each, assemblies and all

macOS and Linux ship the game executable on its own, with content beside it, and are built by their
own scripts. Only Windows has hardware old enough to need the fallback.
"""

import shutil
import subprocess
import sys
from pathlib import Path

try:
    import py7zr
    from tqdm import tqdm
except ImportError:
    print("Required: pip install py7zr tqdm", file=sys.stderr)
    sys.exit(1)

SCRIPT_DIR = Path(__file__).parent
PROJECT_ROOT = SCRIPT_DIR / ".."
CSPROJ = PROJECT_ROOT / "CutTheRopeDX" / "CutTheRopeDX.csproj"
LAUNCHER_CSPROJ = PROJECT_ROOT / "CutTheRopeDX.Launcher" / "CutTheRopeDX.Launcher.csproj"
OUTPUT_DIR = PROJECT_ROOT / "CutTheRopeDX" / "bin" / "Publish" / "win-x64"
RELEASE_DIR = PROJECT_ROOT / "CutTheRopeDX" / "bin" / "release_github"

# Directory and executable names the launcher looks for; must match BackendSelection.
BACKEND_DIRECTORIES = {"VK": "vk", "GL": "gl"}
BACKEND_EXECUTABLES = {"VK": "CutTheRopeDX.vk", "GL": "CutTheRopeDX.gl"}

# Name the game publishes under before it is renamed per backend.
GAME_ASSEMBLY = "CutTheRope-DX"

# The launcher builds under its own assembly name so it can sit beside the game at build time without
# colliding with it. Players start this instead.
LAUNCHER_ASSEMBLY = "CutTheRopeDX.Launcher"
LAUNCHER_EXECUTABLE = "CutTheRope-DX"

CONTENT_DIRECTORY = "content"

# Developer artifacts that publish emits beside the binaries and no player has a use for. Excluded when
# the archive is built rather than switched off in the build: NativeAOT's StripSymbols does not suppress
# a symbol file, it moves symbols out of the executable into one, so there is always something to drop.
UNSHIPPED_SUFFIXES = (".pdb", ".xml", ".dSYM")

# Where the game project's MoveFfmpegToSubfolder target leaves the FFmpeg libraries after publish.
FFMPEG_DIRECTORY = "ffmpeg"


def publish(csproj: Path, out_dir: Path, version: str, use_aot: bool, extra: list[str] = None):
    """Publish one project into out_dir, failing the script if the build fails."""
    cmd = [
        "dotnet",
        "publish",
        str(csproj),
        "-c", "Release",
        "-f", "net10.0",
        "-r", "win-x64",
        f"-p:VersionPrefix={version}",
        "-p:VersionSuffix=",
        f"-p:PublishAot={str(use_aot).lower()}",
        "-o", str(out_dir),
        *(extra or []),
    ]
    print(f"\n> {' '.join(cmd)}\n")
    result = subprocess.run(cmd, check=False)
    if result.returncode != 0:
        sys.exit(result.returncode)


def take_shared_directory(staged: Path, name: str) -> None:
    """Move a directory to the output root the first time, and discard later copies.

    Content is built once, for one platform, and both backends copy that same tree into their own
    publish output; the FFmpeg libraries are identical too. Shipping either twice would add several
    hundred megabytes for nothing.
    """
    shared = OUTPUT_DIR / name
    if not staged.is_dir():
        return
    if shared.exists():
        shutil.rmtree(staged)
    else:
        staged.rename(shared)


def flatten(backend: str, staged: Path):
    """Fold one ahead-of-time build into the output root, renaming its executable.

    Safe to rename because an ahead-of-time executable is an ordinary native binary: unlike the apphost
    a framework-dependent build produces, nothing inside it refers to its own file name.
    """
    for name in (CONTENT_DIRECTORY, FFMPEG_DIRECTORY):
        take_shared_directory(staged / name, name)

    produced = staged / f"{GAME_ASSEMBLY}.exe"
    if not produced.is_file():
        print(f"No {backend} executable at {produced}", file=sys.stderr)
        sys.exit(1)
    produced.rename(OUTPUT_DIR / f"{BACKEND_EXECUTABLES[backend]}.exe")

    # Everything left is native libraries, which the two backends do not share names for.
    for leftover in staged.iterdir():
        destination = OUTPUT_DIR / leftover.name
        if destination.exists():
            leftover.unlink() if leftover.is_file() else shutil.rmtree(leftover)
        else:
            leftover.rename(destination)
    staged.rmdir()
    print(f"{backend} build placed as {BACKEND_EXECUTABLES[backend]}.exe")


def consolidate_directories():
    """Lift content out of the per-backend directories so one copy serves both."""
    for backend, directory in BACKEND_DIRECTORIES.items():
        produced = OUTPUT_DIR / directory / CONTENT_DIRECTORY
        if not produced.is_dir():
            print(f"No content produced by the {backend} build; expected {produced}", file=sys.stderr)
            sys.exit(1)
        take_shared_directory(produced, CONTENT_DIRECTORY)
    print(f"Content shared at {OUTPUT_DIR / CONTENT_DIRECTORY}")


def rename_launcher():
    """Give the launcher the name players start.

    Renaming the apphost is safe: it finds its managed assembly by a name recorded inside the binary,
    not by whatever the executable happens to be called.
    """
    published = OUTPUT_DIR / f"{LAUNCHER_ASSEMBLY}.exe"
    if not published.is_file():
        print(f"Launcher not found at {published}", file=sys.stderr)
        sys.exit(1)
    published.replace(OUTPUT_DIR / f"{LAUNCHER_EXECUTABLE}.exe")
    print(f"Launcher published as {LAUNCHER_EXECUTABLE}.exe")


def is_shipped(path: Path) -> bool:
    """Whether a published file belongs in the archive players download."""
    return not any(
        part.endswith(UNSHIPPED_SUFFIXES) for part in (path.name, *path.relative_to(OUTPUT_DIR).parts)
    )


def package(version: str):
    """Compress the build output into a .7z archive."""
    RELEASE_DIR.mkdir(parents=True, exist_ok=True)
    archive_name = f"CutTheRopeDX-v{version}-Windows-x64.7z"
    archive_path = RELEASE_DIR / archive_name

    files = sorted(f for f in OUTPUT_DIR.rglob("*") if f.is_file() and is_shipped(f))
    dropped = sum(1 for f in OUTPUT_DIR.rglob("*") if f.is_file() and not is_shipped(f))
    if dropped:
        print(f"Excluding {dropped} debug/documentation file(s) from the archive")
    total_size = sum(f.stat().st_size for f in files)

    print(f"\nPackaging {archive_name}...")
    with py7zr.SevenZipFile(archive_path, "w", filters=[{"id": py7zr.FILTER_LZMA, "preset": 9}]) as archive:
        with tqdm(total=total_size, unit="B", unit_scale=True) as pbar:
            for file in files:
                archive.write(file, str(file.relative_to(OUTPUT_DIR)))
                pbar.update(file.stat().st_size)

    size_mb = archive_path.stat().st_size / (1024 * 1024)
    print(f"Created {archive_path} ({size_mb:.1f} MB)")


def resolve_options() -> tuple[str, bool]:
    """Take the version and AOT choice from argv, or prompt when interactive."""
    args = [a for a in sys.argv[1:] if a != "--no-aot"]
    use_aot = "--no-aot" not in sys.argv[1:]

    if args:
        return args[0], use_aot

    if not sys.stdin.isatty():
        print("Usage: release_windows.py <version> [--no-aot]", file=sys.stderr)
        sys.exit(1)

    version = input("Version (e.g. 2.12.0.1): ").strip()
    if not version:
        print("Version is required.", file=sys.stderr)
        sys.exit(1)

    return version, input("Use NativeAOT? [Y/n]: ").strip().lower() != "n"


def main():
    version, use_aot = resolve_options()
    print(f"\nBuilding v{version} (NativeAOT: {use_aot})...")

    # Start from empty: a directory left over from a single-backend build would otherwise put a stale
    # game executable next to the launcher, where it would be packaged and never run.
    if OUTPUT_DIR.exists():
        shutil.rmtree(OUTPUT_DIR)

    # Ahead-of-time builds carry no loose assemblies, so they can share a directory and be told apart by
    # name. Builds that keep their assemblies cannot: both would write MonoGame.Framework.dll.
    for backend, directory in BACKEND_DIRECTORIES.items():
        print(f"\n=== {backend} build ===")
        staged = OUTPUT_DIR / directory
        publish(CSPROJ, staged, version, use_aot, [f"-p:GraphicsBackend={backend}"])
        if use_aot:
            flatten(backend, staged)

    if not use_aot:
        consolidate_directories()

    print("\n=== launcher ===")
    publish(LAUNCHER_CSPROJ, OUTPUT_DIR, version, use_aot)

    rename_launcher()
    package(version)


if __name__ == "__main__":
    main()
