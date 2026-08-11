#!/usr/bin/env python3
"""Build a macOS Tahoe (AVFoundation) DMG release for Cut the Rope DX."""

import shutil
import subprocess
import sys
import tempfile
from pathlib import Path

SCRIPT_DIR = Path(__file__).resolve().parent
PROJECT_ROOT = SCRIPT_DIR.parent
CSPROJ = PROJECT_ROOT / "src" / "CutTheRopeDX.Desktop" / "CutTheRopeDX.Desktop.csproj"
# Kept separate from bundle_macos.sh's osx-arm64 directory: that script builds a
# different product (net10.0 + ffmpeg dylibs) under the same .app name, and
# sharing a publish path lets one build inherit the other's leftovers.
OUTPUT_DIR = (
    PROJECT_ROOT
    / "src"
    / "CutTheRopeDX.Desktop"
    / "bin"
    / "Publish"
    / "osx-arm64-avfoundation"
)
RELEASE_DIR = PROJECT_ROOT / "src" / "CutTheRopeDX.Desktop" / "bin" / "release_github"
APP_BUNDLE = OUTPUT_DIR / "CutTheRope-DX.app"


def run_command(cmd: list[str]) -> None:
    """Run a command and exit if it fails."""
    print(f"> {' '.join(str(arg) for arg in cmd)}\n")

    result = subprocess.run(cmd, check=False)
    if result.returncode != 0:
        sys.exit(result.returncode)


def create_dmg(version: str) -> None:
    """Package only the .app bundle into a compressed macOS disk image."""
    if sys.platform != "darwin":
        print(
            "DMG creation requires macOS and the hdiutil command.",
            file=sys.stderr,
        )
        sys.exit(1)

    if shutil.which("hdiutil") is None:
        print("hdiutil was not found.", file=sys.stderr)
        sys.exit(1)

    if shutil.which("ditto") is None:
        print("ditto was not found.", file=sys.stderr)
        sys.exit(1)

    if not APP_BUNDLE.is_dir():
        print(f"App bundle not found: {APP_BUNDLE}", file=sys.stderr)
        sys.exit(1)

    RELEASE_DIR.mkdir(parents=True, exist_ok=True)

    dmg_name = f"CutTheRopeDX-v{version}-macOS-arm64-avfoundation.dmg"
    dmg_path = RELEASE_DIR / dmg_name
    volume_name = f"Cut the Rope DX {version}"

    print(f"\nPackaging {dmg_name}...")

    # hdiutil packages the contents of the source directory. Place the app
    # inside a temporary directory so the DMG root contains the .app bundle.
    with tempfile.TemporaryDirectory(prefix="cuttherope-dmg-") as temp_dir:
        staging_dir = Path(temp_dir)
        staged_app = staging_dir / APP_BUNDLE.name

        print("Staging app bundle...")
        run_command(
            [
                "ditto",
                str(APP_BUNDLE),
                str(staged_app),
            ]
        )

        # Remove an existing image so a failed rebuild cannot leave stale data.
        dmg_path.unlink(missing_ok=True)

        print("Creating compressed disk image...")
        run_command(
            [
                "hdiutil",
                "create",
                "-volname",
                volume_name,
                "-srcfolder",
                str(staging_dir),
                "-format",
                "UDZO",
                "-imagekey",
                "zlib-level=9",
                "-ov",
                str(dmg_path),
            ]
        )

    if not dmg_path.is_file():
        print(f"DMG was not created: {dmg_path}", file=sys.stderr)
        sys.exit(1)

    size_mb = dmg_path.stat().st_size / (1024 * 1024)
    print(f"Created {dmg_path} ({size_mb:.1f} MB)")


def resolve_options() -> tuple[str, bool]:
    """Take the version and AOT choice from argv, or prompt when interactive."""
    args = [a for a in sys.argv[1:] if a != "--no-aot"]
    use_aot = "--no-aot" not in sys.argv[1:]

    if args:
        return args[0], use_aot

    if not sys.stdin.isatty():
        print("Usage: release_macos_tahoe.py <version> [--no-aot]", file=sys.stderr)
        sys.exit(1)

    version = input("Version (e.g. 2.12.0.1): ").strip()
    if not version:
        print("Version is required.", file=sys.stderr)
        sys.exit(1)

    return version, input("Use NativeAOT? [Y/n]: ").strip().lower() != "n"


def main() -> None:
    version, use_aot = resolve_options()

    cmd = [
        "dotnet",
        "publish",
        str(CSPROJ),
        "-c",
        "Release",
        "-f",
        "net10.0-macos",
        "-r",
        "osx-arm64",
        "-p:ValidateXcodeVersion=false",
        f"-p:VersionPrefix={version}",
        "-p:VersionSuffix=",
        f"-p:PublishAot={str(use_aot).lower()}",
        "-o",
        str(OUTPUT_DIR),
    ]

    print(
        f"\nBuilding v{version} for macOS Tahoe/AVFoundation "
        f"(NativeAOT: {use_aot})..."
    )
    run_command(cmd)

    create_dmg(version)


if __name__ == "__main__":
    main()
