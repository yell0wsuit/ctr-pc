#!/bin/bash

# Copies FFmpeg dylibs from Homebrew into a Frameworks directory and rewrites
# install names so the bundle is fully self-contained.
# Usage: ./bundle_ffmpeg_macos.sh <frameworks_dir>
#
# Requires Homebrew FFmpeg: brew install ffmpeg

set -e

FRAMEWORKS_DIR="$1"
if [ -z "$FRAMEWORKS_DIR" ]; then
    echo "Usage: bundle_ffmpeg_macos.sh <frameworks_dir>"
    exit 1
fi

# Locate Homebrew FFmpeg
FFMPEG_LIB=""
for candidate in /opt/homebrew/opt/ffmpeg/lib /usr/local/opt/ffmpeg/lib; do
    if [ -d "$candidate" ]; then
        FFMPEG_LIB="$candidate"
        break
    fi
done

if [ -z "$FFMPEG_LIB" ]; then
    echo "Error: Homebrew FFmpeg not found. Install with: brew install ffmpeg"
    exit 1
fi

echo "Using FFmpeg from $FFMPEG_LIB"
mkdir -p "$FRAMEWORKS_DIR"

# Core FFmpeg libraries needed by FFmpeg.AutoGen
CORE_LIBS="libavcodec libavformat libavutil libswresample libswscale"

# --- Pass 1: Copy core FFmpeg dylibs ---
# Use the unversioned symlink (e.g. libavcodec.dylib -> libavcodec.62.dylib)
# and cp -L to resolve it to the actual file. Name the output by the short
# versioned name that otool references (e.g. libavcodec.62.dylib).
echo "Copying core FFmpeg dylibs..."
for lib in $CORE_LIBS; do
    # Find the short versioned symlink (e.g. libavcodec.62.dylib)
    short=$(basename "$(ls "$FFMPEG_LIB"/$lib.dylib 2>/dev/null)" .dylib)
    versioned_name=$(ls "$FFMPEG_LIB"/$lib.[0-9]*.dylib 2>/dev/null \
        | xargs -I{} basename {} \
        | grep -E "^$lib\.[0-9]+\.dylib$" \
        | head -1)

    if [ -z "$versioned_name" ]; then
        echo "Warning: $lib not found in $FFMPEG_LIB"
        continue
    fi

    echo "  $versioned_name"
    cp -L "$FFMPEG_LIB/$versioned_name" "$FRAMEWORKS_DIR/$versioned_name"
    chmod 755 "$FRAMEWORKS_DIR/$versioned_name"
done

# --- Pass 2: Discover and copy third-party Homebrew dependencies ---
# Scan the core dylibs for references to /opt/homebrew/* that aren't already
# in our Frameworks directory.
echo "Copying third-party dependencies..."
SEEN_FILE=$(mktemp)
trap "rm -f '$SEEN_FILE'" EXIT

# Mark core libs as already handled
ls "$FRAMEWORKS_DIR"/*.dylib 2>/dev/null | xargs -I{} basename {} > "$SEEN_FILE"

for dylib in "$FRAMEWORKS_DIR"/*.dylib; do
    otool -L "$dylib" | tail -n +2 | awk '{print $1}' | grep '^/opt/homebrew/' | while IFS= read -r dep; do
        dep_name=$(basename "$dep")
        if ! grep -qxF "$dep_name" "$SEEN_FILE" 2>/dev/null; then
            echo "$dep_name" >> "$SEEN_FILE"
            real=$(python3 -c "import pathlib; print(pathlib.Path('$dep').resolve())" 2>/dev/null)
            if [ -f "$real" ]; then
                echo "  $dep_name"
                cp -L "$real" "$FRAMEWORKS_DIR/$dep_name"
                chmod 755 "$FRAMEWORKS_DIR/$dep_name"
            fi
        fi
    done
done

# --- Pass 3: Rewrite install names for relocatability ---
echo "Rewriting install names..."
for dylib in "$FRAMEWORKS_DIR"/*.dylib; do
    name=$(basename "$dylib")
    install_name_tool -id "@loader_path/$name" "$dylib" 2>/dev/null

    otool -L "$dylib" | tail -n +2 | awk '{print $1}' | grep '^/opt/homebrew/' | while IFS= read -r dep; do
        dep_name=$(basename "$dep")
        install_name_tool -change "$dep" "@loader_path/$dep_name" "$dylib" 2>/dev/null
    done
done

# Copy FFmpeg license for LGPL compliance
FFMPEG_CELLAR=$(python3 -c "import pathlib; print(pathlib.Path('$FFMPEG_LIB/..').resolve())")
if [ -f "$FFMPEG_CELLAR/LICENSE" ]; then
    cp "$FFMPEG_CELLAR/LICENSE" "$FRAMEWORKS_DIR/FFmpeg-LICENSE.txt"
elif [ -f "$FFMPEG_CELLAR/COPYING.LGPLv2.1" ]; then
    cp "$FFMPEG_CELLAR/COPYING.LGPLv2.1" "$FRAMEWORKS_DIR/FFmpeg-LICENSE.txt"
fi

TOTAL=$(ls "$FRAMEWORKS_DIR"/*.dylib 2>/dev/null | wc -l | tr -d ' ')
echo "Done. Bundled $TOTAL dylibs into $FRAMEWORKS_DIR"
