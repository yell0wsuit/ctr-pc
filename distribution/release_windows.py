#!/usr/bin/env python3
"""Build a Windows release package for Cut the Rope DX."""

import subprocess
import sys
from pathlib import Path

SCRIPT_DIR = Path(__file__).parent
CSPROJ = SCRIPT_DIR / ".." / "CutTheRope" / "CutTheRope.csproj"
OUTPUT_DIR = SCRIPT_DIR / ".." / "CutTheRope" / "bin" / "Publish" / "win-x64"


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
    sys.exit(result.returncode)


if __name__ == "__main__":
    main()
