#!/usr/bin/env python3
"""Build a Windows release package for Cut the Rope DX.

Windows ships two builds of the game and a launcher that chooses between them, because the graphics
backend is fixed when the game is compiled: the Vulkan and OpenGL builds reference different MonoGame
assemblies exporting the same types, so one process cannot hold both. The OpenGL build exists for
machines whose Vulkan is missing or software-only, which on Intel means anything before Skylake.

    CutTheRope-DX.exe   launcher: probes Vulkan, runs one of the builds below
    content/            built once, read by whichever build runs
    vk/                 Vulkan build
    gl/                 OpenGL build

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

# Directory names the launcher looks in; must match BackendSelection.
BACKEND_DIRECTORIES = {"VK": "vk", "GL": "gl"}

# The launcher builds under its own assembly name so it can sit beside the game at build time without
# colliding with it. Players start this instead.
LAUNCHER_ASSEMBLY = "CutTheRopeDX.Launcher"
LAUNCHER_EXECUTABLE = "CutTheRope-DX"

CONTENT_DIRECTORY = "content"


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


def consolidate_content():
    """Lift the built content out of the backend directories so one copy serves both.

    Both builds produce identical content bar one byte naming the platform it was built for, which
    MonoGame does not enforce, so shipping it twice would only add several hundred megabytes. The game
    resolves content beside its own executable first and one directory up second, which is what makes
    the shared copy reachable from inside vk/ and gl/.
    """
    shared = OUTPUT_DIR / CONTENT_DIRECTORY
    if shared.exists():
        shutil.rmtree(shared)

    for backend, directory in BACKEND_DIRECTORIES.items():
        produced = OUTPUT_DIR / directory / CONTENT_DIRECTORY
        if not produced.is_dir():
            print(f"No content produced by the {backend} build; expected {produced}", file=sys.stderr)
            sys.exit(1)
        if shared.exists():
            shutil.rmtree(produced)
        else:
            produced.rename(shared)

    print(f"Content shared at {shared}")


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


def package(version: str):
    """Compress the build output into a .7z archive."""
    RELEASE_DIR.mkdir(parents=True, exist_ok=True)
    archive_name = f"CutTheRopeDX-v{version}-Windows-x64.7z"
    archive_path = RELEASE_DIR / archive_name

    files = sorted(f for f in OUTPUT_DIR.rglob("*") if f.is_file())
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

    for backend, directory in BACKEND_DIRECTORIES.items():
        print(f"\n=== {backend} build ===")
        publish(CSPROJ, OUTPUT_DIR / directory, version, use_aot, [f"-p:GraphicsBackend={backend}"])

    print("\n=== launcher ===")
    publish(LAUNCHER_CSPROJ, OUTPUT_DIR, version, use_aot)

    consolidate_content()
    rename_launcher()
    package(version)


if __name__ == "__main__":
    main()
