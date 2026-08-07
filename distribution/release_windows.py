#!/usr/bin/env python3
"""Build a Windows release package for Cut the Rope DX.

Windows ships two builds of the game and a launcher that chooses between them, because the graphics
backend is fixed when the game is compiled: the Vulkan and OpenGL builds reference different MonoGame
assemblies exporting the same types, so one process cannot hold both. The OpenGL build exists for
machines whose Vulkan is missing, which on Intel means anything before Skylake.

Both builds nonetheless share one directory. Every publish here is single-file, so the managed
assemblies live inside each executable rather than beside it, whether or not it was compiled ahead of
time; what stays loose is native and named differently per backend. So the only name the two would
have fought over is the executable's, and each is renamed as it is folded in:

    CutTheRope-DX.exe     launcher: probes Vulkan, runs one of the builds below
    ctrdx-vk.exe   Vulkan build      + mgruntime.dll
    ctrdx-gl.exe   OpenGL build      + SDL2.dll, openal.dll, ...
    ffmpeg/  content/     one copy, shared

macOS and Linux ship the game executable on its own, with content beside it, and are built by their
own scripts. Only Windows has hardware old enough to need the fallback.
"""

import shutil
import subprocess
import sys
import tempfile
import urllib.request
import zipfile
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
LAUNCHER_CSPROJ = (
    PROJECT_ROOT / "CutTheRopeDX.Launcher" / "CutTheRopeDX.Launcher.csproj"
)
OUTPUT_DIR = PROJECT_ROOT / "CutTheRopeDX" / "bin" / "Publish" / "win-x64"
RELEASE_DIR = PROJECT_ROOT / "CutTheRopeDX" / "bin" / "release_github"

# Executable names the launcher looks for; must match BackendSelection.
BACKEND_EXECUTABLES = {"VK": "ctrdx-vk", "GL": "ctrdx-gl"}

# Name the game publishes under before it is renamed per backend.
GAME_ASSEMBLY = "CutTheRope-DX"

LAUNCHER_ASSEMBLY = "CutTheRopeDX.Launcher"
LAUNCHER_EXECUTABLE = "CutTheRope-DX"

CONTENT_DIRECTORY = "content"
UNSHIPPED_SUFFIXES = ".pdb"
FFMPEG_DIRECTORY = "ffmpeg"
FFMPEG_URL = (
    "https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/"
    "ffmpeg-n8.1-latest-win64-lgpl-shared-8.1.zip"
)
FFMPEG_DLL_GLOBS = (
    "avcodec-*.dll",
    "avdevice-*.dll",
    "avfilter-*.dll",
    "avformat-*.dll",
    "avutil-*.dll",
    "postproc-*.dll",
    "swresample-*.dll",
    "swscale-*.dll",
)


def publish(
    csproj: Path, out_dir: Path, version: str, use_aot: bool, extra: list[str] = None
):
    """Publish one project into out_dir, failing the script if the build fails."""
    cmd = [
        "dotnet",
        "publish",
        str(csproj),
        "-c",
        "Release",
        "-f",
        "net10.0",
        "-r",
        "win-x64",
        f"-p:VersionPrefix={version}",
        "-p:VersionSuffix=",
        f"-p:PublishAot={str(use_aot).lower()}",
        "-o",
        str(out_dir),
        *(extra or []),
    ]
    print(f"\n> {' '.join(cmd)}\n")
    result = subprocess.run(cmd, check=False)
    if result.returncode != 0:
        sys.exit(result.returncode)


def download_ffmpeg() -> None:
    """Download FFmpeg LGPL shared libraries into the package ffmpeg directory."""
    destination = OUTPUT_DIR / FFMPEG_DIRECTORY
    if destination.is_dir() and any(destination.glob("avcodec-*.dll")):
        print(f"FFmpeg shared libraries already present in {destination}")
        return

    print("\n=== FFmpeg ===")
    print(f"Downloading {FFMPEG_URL}")

    with tempfile.TemporaryDirectory() as temp_dir_name:
        temp_dir = Path(temp_dir_name)
        archive_path = temp_dir / "ffmpeg-win64-lgpl-shared.zip"
        urllib.request.urlretrieve(FFMPEG_URL, archive_path)

        with zipfile.ZipFile(archive_path) as archive:
            archive.extractall(temp_dir / "extracted")

        roots = [path for path in (temp_dir / "extracted").iterdir() if path.is_dir()]
        if len(roots) != 1:
            print("Could not find the extracted FFmpeg directory", file=sys.stderr)
            sys.exit(1)

        extracted = roots[0]
        bin_dir = extracted / "bin"
        if not bin_dir.is_dir():
            print(f"FFmpeg bin directory not found at {bin_dir}", file=sys.stderr)
            sys.exit(1)

        dlls = sorted(
            {dll for pattern in FFMPEG_DLL_GLOBS for dll in bin_dir.glob(pattern)}
        )
        if not dlls:
            print(
                "No FFmpeg shared DLLs found in the downloaded archive", file=sys.stderr
            )
            sys.exit(1)

        destination.mkdir(parents=True, exist_ok=True)
        for dll in dlls:
            shutil.copy2(dll, destination / dll.name)

        license_file = extracted / "LICENSE.txt"
        if license_file.is_file():
            shutil.copy2(license_file, destination / "FFmpeg-LICENSE.txt")

    print(f"FFmpeg shared libraries copied to {destination}")


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


def fold_in(backend: str, staged: Path):
    """Fold one build into the output root, renaming its executable.

    Safe to rename: a single-file executable finds the assemblies bundled inside it, and an
    ahead-of-time one is an ordinary native binary. Neither refers to its own file name.
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
        part.endswith(UNSHIPPED_SUFFIXES) for part in path.relative_to(OUTPUT_DIR).parts
    )


def package(version: str):
    """Compress the build output into a .7z archive."""
    RELEASE_DIR.mkdir(parents=True, exist_ok=True)
    archive_name = f"CutTheRopeDX-v{version}-Windows-x64.7z"
    archive_path = RELEASE_DIR / archive_name

    published = sorted(f for f in OUTPUT_DIR.rglob("*") if f.is_file())
    files = [f for f in published if is_shipped(f)]
    if len(published) != len(files):
        print(
            f"Excluding {len(published) - len(files)} debug/documentation file(s) from the archive"
        )
    sizes = [f.stat().st_size for f in files]

    print(f"\nPackaging {archive_name}...")
    with py7zr.SevenZipFile(
        archive_path, "w", filters=[{"id": py7zr.FILTER_LZMA, "preset": 9}]
    ) as archive:
        with tqdm(total=sum(sizes), unit="B", unit_scale=True) as pbar:
            for file, size in zip(files, sizes, strict=True):
                archive.write(file, str(file.relative_to(OUTPUT_DIR)))
                pbar.update(size)

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

    # Published one at a time into a staging directory, then folded into the root, so that the second
    # build cannot overwrite the first's executable before it has been renamed.
    #
    # Only the first carries content. Both backends read the same tree, and the package holds one copy, so
    # building and copying it again for the second would move some 350MB twice over to be deleted here.
    for position, backend in enumerate(BACKEND_EXECUTABLES):
        print(f"\n=== {backend} build ===")
        staged = OUTPUT_DIR / f"staging-{backend}"
        options = [f"-p:GraphicsBackend={backend}"]
        if position > 0:
            options += ["-p:DeployContent=false", "-p:RunMGCB=false"]
        publish(CSPROJ, staged, version, use_aot, options)
        fold_in(backend, staged)

    print("\n=== launcher ===")
    publish(LAUNCHER_CSPROJ, OUTPUT_DIR, version, use_aot)

    rename_launcher()
    download_ffmpeg()
    package(version)


if __name__ == "__main__":
    main()
