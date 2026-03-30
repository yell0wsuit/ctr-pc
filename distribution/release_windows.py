#!/usr/bin/env python3
"""Build a Windows release package for Cut the Rope DX."""

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
CSPROJ = PROJECT_ROOT / "CutTheRope" / "CutTheRope.csproj"
OUTPUT_DIR = PROJECT_ROOT / "CutTheRope" / "bin" / "Publish" / "win-x64"
RELEASE_DIR = PROJECT_ROOT / "CutTheRope" / "bin" / "release_github"


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


def main():
    version = input("Version (e.g. 2.12.0.1): ").strip()
    if not version:
        print("Version is required.", file=sys.stderr)
        sys.exit(1)

    aot_input = input("Use NativeAOT? [Y/n]: ").strip().lower()
    use_aot = aot_input != "n"

    cmd = [
        "dotnet",
        "publish",
        str(CSPROJ),
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
        str(OUTPUT_DIR),
    ]

    print(f"\nBuilding v{version} (NativeAOT: {use_aot})...")
    print(f"> {' '.join(cmd)}\n")

    result = subprocess.run(cmd, check=False)
    if result.returncode != 0:
        sys.exit(result.returncode)

    package(version)


if __name__ == "__main__":
    main()
