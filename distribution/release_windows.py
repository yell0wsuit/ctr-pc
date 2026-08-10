#!/usr/bin/env python3
"""Build a Windows release package for Cut the Rope DX.

Windows ships two builds of the game and a launcher that chooses between them.
The graphics backend is fixed when the game is compiled, so the Vulkan and
OpenGL builds reference different MonoGame assemblies exporting the same types.
One process cannot hold both. The OpenGL build exists for machines whose Vulkan
is missing, which on Intel means anything before Skylake.

Both builds nonetheless share one directory. Every publish here is single-file,
so the managed assemblies live inside each executable rather than beside it,
whether or not it was compiled ahead of time. What stays loose is native and
named differently per backend. The only name the two would have fought over is
the executable's, and each is renamed as it is folded in:

    CutTheRope-DX.exe     launcher: probes Vulkan, runs one of the builds below
    ctrdx-vk.exe   Vulkan build      + mgruntime.dll
    ctrdx-gl.exe   OpenGL build      + SDL2.dll, openal.dll, ...
    ffmpeg/  content/     one copy, shared

macOS and Linux ship the game executable on its own, with content beside it,
and are built by their own scripts. Only Windows has hardware old enough to
need the fallback.
"""

import hashlib
import shutil
import subprocess
import sys
import tempfile
import urllib.request
import zipfile
from http.client import HTTPException
from pathlib import Path

try:
    import py7zr
    from tqdm import tqdm
except ImportError:
    print("Required: pip install py7zr tqdm", file=sys.stderr)
    sys.exit(1)

SCRIPT_DIR = Path(__file__).parent
PROJECT_ROOT = SCRIPT_DIR.parent.resolve()
CSPROJ = PROJECT_ROOT / "src" / "CutTheRopeDX.Desktop" / "CutTheRopeDX.Desktop.csproj"
LAUNCHER_CSPROJ = (
    PROJECT_ROOT / "src" / "CutTheRopeDX.Launcher" / "CutTheRopeDX.Launcher.csproj"
)
RELEASE_DIR = PROJECT_ROOT / "src" / "CutTheRopeDX.Desktop" / "bin" / "release_github"

ARCHITECTURES = {
    "x64": {"rid": "win-x64", "btbn": "win64", "label": "x64"},
    "arm64": {"rid": "win-arm64", "btbn": "winarm64", "label": "ARM64"},
}

# Executable names the launcher looks for; must match BackendSelection.
BACKEND_EXECUTABLES = {"VK": "ctrdx-vk", "GL": "ctrdx-gl"}

# Name the game publishes under before it is renamed per backend.
GAME_ASSEMBLY = "CutTheRope-DX"

LAUNCHER_ASSEMBLY = "CutTheRopeDX.Launcher"
LAUNCHER_EXECUTABLE = "CutTheRope-DX"

CONTENT_DIRECTORY = "content"
UNSHIPPED_SUFFIXES = ".pdb"
FFMPEG_DIRECTORY = "ffmpeg"
FFMPEG_DOWNLOAD_ATTEMPTS = 5
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
    csproj: Path,
    out_dir: Path,
    build_options: tuple[str, bool, str],
    extra: tuple[str, ...] = (),
):
    """Publish one project into out_dir, failing the script if the build fails."""
    version, use_aot, runtime_id = build_options
    cmd = [
        "dotnet",
        "publish",
        str(csproj),
        "-c",
        "Release",
        "-f",
        "net10.0",
        "-r",
        runtime_id,
        f"-p:VersionPrefix={version}",
        "-p:VersionSuffix=",
        f"-p:PublishAot={str(use_aot).lower()}",
        "-o",
        str(out_dir),
        *extra,
    ]
    print(f"\n> {' '.join(cmd)}\n")
    result = subprocess.run(cmd, check=False)
    if result.returncode != 0:
        sys.exit(result.returncode)


def download_ffmpeg(output_dir: Path, btbn_arch: str) -> None:
    """Download BtbN FFmpeg LGPL shared libraries for one Windows architecture."""
    release_url = "https://github.com/BtbN/FFmpeg-Builds/releases/download/latest"
    archive_name = f"ffmpeg-n8.1-latest-{btbn_arch}-lgpl-shared-8.1.zip"
    ffmpeg_url = f"{release_url}/{archive_name}"
    checksums_url = f"{release_url}/checksums.sha256"
    destination = output_dir / FFMPEG_DIRECTORY
    if destination.is_dir() and any(destination.glob("avcodec-*.dll")):
        print(f"FFmpeg shared libraries already present in {destination}")
        return

    print("\n=== FFmpeg ===")
    print(f"Downloading {ffmpeg_url}")

    with tempfile.TemporaryDirectory() as temp_dir_name:
        temp_dir = Path(temp_dir_name)
        archive_path = temp_dir / archive_name
        checksums_path = temp_dir / "checksums.sha256"

        for attempt in range(1, FFMPEG_DOWNLOAD_ATTEMPTS + 1):
            try:
                urllib.request.urlretrieve(ffmpeg_url, archive_path)
                urllib.request.urlretrieve(checksums_url, checksums_path)

                expected_checksum = None
                for line in checksums_path.read_text(encoding="utf-8").splitlines():
                    parts = line.split(maxsplit=1)
                    if len(parts) == 2 and parts[1].lstrip("*") == archive_name:
                        expected_checksum = parts[0].lower()
                        break

                digest = hashlib.sha256()
                with archive_path.open("rb") as archive_file:
                    for chunk in iter(lambda: archive_file.read(1024 * 1024), b""):
                        digest.update(chunk)
                actual_checksum = digest.hexdigest()

                if expected_checksum is None:
                    raise ValueError(f"No checksum found for {archive_name}")
                if actual_checksum != expected_checksum:
                    raise ValueError(f"Checksum mismatch for {archive_name}")
            except (OSError, HTTPException, ValueError) as error:
                if attempt == FFMPEG_DOWNLOAD_ATTEMPTS:
                    print(
                        f"FFmpeg download or verification failed after "
                        f"{FFMPEG_DOWNLOAD_ATTEMPTS} attempts: {error}",
                        file=sys.stderr,
                    )
                    sys.exit(1)
                print(
                    f"FFmpeg download or verification failed: {error}; "
                    f"retrying ({attempt + 1}/{FFMPEG_DOWNLOAD_ATTEMPTS})..."
                )
            else:
                break

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
                "No FFmpeg shared DLLs found in the downloaded archive",
                file=sys.stderr,
            )
            sys.exit(1)

        destination.mkdir(parents=True, exist_ok=True)
        for dll in dlls:
            shutil.copy2(dll, destination / dll.name)

        license_file = extracted / "LICENSE.txt"
        if license_file.is_file():
            shutil.copy2(license_file, destination / "FFmpeg-LICENSE.txt")

    print(f"FFmpeg shared libraries copied to {destination}")


def take_shared_directory(output_dir: Path, staged: Path, name: str) -> None:
    """Copy a staged shared directory into the output root, then remove it.

    Windows can reject a directory rename when the destination already exists
    or a build step briefly holds the directory. Copying also safely merges an
    existing shared tree without duplicating it in the final package.
    """
    if not staged.is_dir():
        return

    shared = output_dir / name
    shutil.copytree(staged, shared, dirs_exist_ok=True)
    shutil.rmtree(staged)


def fold_in(output_dir: Path, backend: str, staged: Path):
    """Fold one build into the output root, renaming its executable.

    Safe to rename: a single-file executable finds the assemblies bundled
    inside it. An ahead-of-time one is an ordinary native binary. Neither
    refers to its own file name.
    """
    for name in (CONTENT_DIRECTORY, FFMPEG_DIRECTORY):
        take_shared_directory(output_dir, staged / name, name)

    produced = staged / f"{GAME_ASSEMBLY}.exe"
    if not produced.is_file():
        print(f"No {backend} executable at {produced}", file=sys.stderr)
        sys.exit(1)
    produced.rename(output_dir / f"{BACKEND_EXECUTABLES[backend]}.exe")

    # Everything left is native libraries, which the two backends do not share names for.
    for leftover in staged.iterdir():
        destination = output_dir / leftover.name
        if destination.exists():
            if leftover.is_file():
                leftover.unlink()
            else:
                shutil.rmtree(leftover)
        else:
            leftover.rename(destination)
    staged.rmdir()
    print(f"{backend} build placed as {BACKEND_EXECUTABLES[backend]}.exe")


def rename_launcher(output_dir: Path):
    """Give the launcher the name players start.

    Renaming the apphost is safe: it finds its managed assembly by a name
    recorded inside the binary, not by the executable file name.
    """
    published = output_dir / f"{LAUNCHER_ASSEMBLY}.exe"
    if not published.is_file():
        print(f"Launcher not found at {published}", file=sys.stderr)
        sys.exit(1)
    published.replace(output_dir / f"{LAUNCHER_EXECUTABLE}.exe")
    print(f"Launcher published as {LAUNCHER_EXECUTABLE}.exe")


def is_shipped(output_dir: Path, path: Path) -> bool:
    """Whether a published file belongs in the archive players download."""
    return not any(
        part.endswith(UNSHIPPED_SUFFIXES) for part in path.relative_to(output_dir).parts
    )


def package(output_dir: Path, version: str, arch_label: str):
    """Compress the build output into a .7z archive."""
    RELEASE_DIR.mkdir(parents=True, exist_ok=True)
    archive_name = f"CutTheRopeDX-v{version}-Windows-{arch_label}.7z"
    archive_path = RELEASE_DIR / archive_name

    published = sorted(f for f in output_dir.rglob("*") if f.is_file())
    files = [f for f in published if is_shipped(output_dir, f)]
    if len(published) != len(files):
        print(
            f"Excluding {len(published) - len(files)} "
            "debug/documentation file(s) from the archive"
        )
    sizes = [f.stat().st_size for f in files]

    print(f"\nPackaging {archive_name}...")
    with py7zr.SevenZipFile(
        archive_path, "w", filters=[{"id": py7zr.FILTER_LZMA, "preset": 9}]
    ) as archive:
        with tqdm(total=sum(sizes), unit="B", unit_scale=True) as pbar:
            for file, size in zip(files, sizes, strict=True):
                archive.write(file, str(file.relative_to(output_dir)))
                pbar.update(size)

    size_mb = archive_path.stat().st_size / (1024 * 1024)
    print(f"Created {archive_path} ({size_mb:.1f} MB)")


def resolve_options() -> tuple[str, bool, str]:
    """Take version, AOT choice, and target architecture from argv or prompts."""
    args = sys.argv[1:]
    use_aot = "--no-aot" not in args
    args = [a for a in args if a != "--no-aot"]

    arch = "x64"
    if "--arch" in args:
        index = args.index("--arch")
        try:
            arch = args[index + 1].lower()
        except IndexError:
            print("--arch requires x64 or arm64", file=sys.stderr)
            sys.exit(2)
        del args[index : index + 2]

    # Accept "arm" as a convenient alias, but Windows/.NET and BtbN call it ARM64.
    if arch == "arm":
        arch = "arm64"
    if arch not in ARCHITECTURES:
        print("Architecture must be x64 or arm64", file=sys.stderr)
        sys.exit(2)

    if args:
        return args[0], use_aot, arch

    if not sys.stdin.isatty():
        print(
            "Usage: release_windows.py <version> [--arch x64|arm64] [--no-aot]",
            file=sys.stderr,
        )
        sys.exit(1)

    version = input("Version (e.g. 2.12.0.1): ").strip()
    if not version:
        print("Version is required.", file=sys.stderr)
        sys.exit(1)

    use_aot = input("Use NativeAOT? [Y/n]: ").strip().lower() != "n"
    return version, use_aot, arch


def main():
    """Build and package the selected Windows architecture."""
    version, use_aot, arch = resolve_options()
    config = ARCHITECTURES[arch]
    runtime_id = config["rid"]
    btbn_arch = config["btbn"]
    arch_label = config["label"]
    output_dir = PROJECT_ROOT / "src" / "CutTheRopeDX.Desktop" / "bin" / "Publish" / runtime_id
    build_options = (version, use_aot, runtime_id)

    print(f"\nBuilding v{version} for {runtime_id} " f"(NativeAOT: {use_aot})...")

    if output_dir.exists():
        shutil.rmtree(output_dir)

    for position, backend in enumerate(BACKEND_EXECUTABLES):
        print(f"\n=== {backend} build ===")
        staged = output_dir / f"staging-{backend}"
        options = (f"-p:GraphicsBackend={backend}",)
        if position > 0:
            options += ("-p:DeployContent=false", "-p:RunMGCB=false")
        publish(CSPROJ, staged, build_options, options)
        fold_in(output_dir, backend, staged)

    print("\n=== launcher ===")
    publish(LAUNCHER_CSPROJ, output_dir, build_options)

    rename_launcher(output_dir)
    download_ffmpeg(output_dir, btbn_arch)
    package(output_dir, version, arch_label)


if __name__ == "__main__":
    main()
