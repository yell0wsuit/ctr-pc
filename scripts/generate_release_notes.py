#!/usr/bin/env python3
"""Generate GitHub release notes markdown for Cut the Rope DX."""

import sys

REPO = "yell0wsuit/cuttherope-dx"
BASE_URL = f"https://github.com/{REPO}/releases/download"


def generate(version: str) -> str:
    tag = f"v{version}"
    dl = f"{BASE_URL}/{tag}"

    files = {
        "win_x64": f"CutTheRopeDX-{tag}-Windows-x64.7z",
        "mac_ffmpeg": f"CutTheRopeDX-{tag}-macOS-arm64-ffmpeg.dmg",
        "mac_avf": f"CutTheRopeDX-{tag}-macOS-arm64-avfoundation.7z",
        "appimage": f"CutTheRope-DX-{tag}-x86_64.AppImage",
        "deb": f"cuttherope-dx_{tag}_amd64.deb",
    }

    md = f"""## Downloads

### 🪟 Windows

- **Windows x64**
  - [{files['win_x64']}]({dl}/{files['win_x64']})

---

### 🍎 macOS (Apple silicon)

- **macOS build with FFmpeg backend**
  Recommended for macOS < 26
  - [{files['mac_ffmpeg']}]({dl}/{files['mac_ffmpeg']})

- **macOS Tahoe (26+) – AVFoundation backend**
  - [{files['mac_avf']}]({dl}/{files['mac_avf']})

> [!Note]
> App downloaded from Internet is usually marked as "can't be opened" / "damaged" due to macOS security.
> Remove it with `xattr -dr com.apple.quarantine <appname.app>`

---

### 🐧 Linux

- **AppImage (x86_64)** – *Recommended*
  - [{files['appimage']}]({dl}/{files['appimage']})

- **Debian / Ubuntu**
  - [{files['deb']}]({dl}/{files['deb']})"""

    return md


def main():
    version = input("Version (without 'v' prefix, e.g. 2.12.0.1): ").strip()
    if not version:
        print("Version is required.", file=sys.stderr)
        sys.exit(1)

    result = generate(version)
    print("\n--- Generated Markdown ---\n")
    print(result)


if __name__ == "__main__":
    main()
