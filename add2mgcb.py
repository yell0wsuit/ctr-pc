"""
Script to automatically detect and add assets to MonoGame's ContentManager
"""

import re
from pathlib import Path

CONTENT_DIR = Path(__file__).parent / "content"
MGCB_FILE = CONTENT_DIR / "content.mgcb"

# Asset type groups and their importer/processor configs
IMAGE_EXTENSIONS = [".png", ".jpg", ".jpeg"]
SOUND_EXTENSIONS = [".wav"]

ASSET_CONFIGS = {
    "image": {
        "extensions": IMAGE_EXTENSIONS,
        "importer": "TextureImporter",
        "processor": "TextureProcessor",
        "params": [
            "/processorParam:ColorKeyColor=255,0,255,255",
            "/processorParam:ColorKeyEnabled=False",
            "/processorParam:GenerateMipmaps=False",
            "/processorParam:PremultiplyAlpha=True",
            "/processorParam:ResizeToPowerOfTwo=False",
            "/processorParam:MakeSquare=False",
            "/processorParam:TextureFormat=Color",
        ],
    },
    "sound": {
        "extensions": SOUND_EXTENSIONS,
        "importer": "WavImporter",
        "processor": "SoundEffectProcessor",
        "params": ["/processorParam:Quality=Best"],
    },
}

# Flatten for easy lookup
ASSET_TYPES = {}
for asset_config in ASSET_CONFIGS.values():
    for extension in asset_config["extensions"]:
        ASSET_TYPES[extension] = {
            "importer": asset_config["importer"],
            "processor": asset_config["processor"],
            "params": asset_config["params"],
        }


def get_existing_assets():
    """Parse content.mgcb and extract all registered assets."""
    if not MGCB_FILE.exists():
        return set()

    existing = set()
    with open(MGCB_FILE, "r", encoding="utf-8") as f:
        for line in f:
            match = re.match(r"^#begin (.+)$", line)
            if match:
                existing.add(match.group(1))
    return existing


def find_asset_files():
    """Find all asset files in content directory."""
    assets = set()
    for file_ext in ASSET_TYPES:
        for file in CONTENT_DIR.rglob(f"*{file_ext}"):
            # Get relative path from content dir
            rel_path = file.relative_to(CONTENT_DIR)
            rel_str = str(rel_path).replace("\\", "/")

            # Only include sounds from sounds/sfx/ directory
            if file_ext in SOUND_EXTENSIONS:
                if not rel_str.startswith("sounds/sfx/"):
                    continue

            assets.add(rel_str)
    return assets


def generate_entry(asset_name):
    """Generate MGCB entry for an asset."""
    file_ext = Path(asset_name).suffix.lower()
    config = ASSET_TYPES.get(file_ext)

    if not config:
        return None

    lines = [
        f"#begin {asset_name}",
        f"/importer:{config['importer']}",
        f"/processor:{config['processor']}",
    ]
    lines.extend(config["params"])
    lines.append(f"/build:{asset_name}")
    lines.append("")  # Empty line separator

    return "\n".join(lines)


def main():
    """Main entry point for the script."""
    existing = get_existing_assets()
    found = find_asset_files()

    new_assets = found - existing

    if not new_assets:
        print("nothing new")
        return

    print(f"found {len(new_assets)} new asset(s). adding in")

    # Append new entries
    with open(MGCB_FILE, "a", encoding="utf-8") as f:
        for asset in sorted(new_assets):
            entry = generate_entry(asset)
            if entry:
                f.write("\n" + entry)
                print(f"  + {asset}")


if __name__ == "__main__":
    main()
