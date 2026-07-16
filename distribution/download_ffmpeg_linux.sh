#!/bin/bash

# Downloads prebuilt FFmpeg 8.1 LGPL shared libraries for Linux x64 from BtbN/FFmpeg-Builds.
# Usage: ./download_ffmpeg_linux.sh <output_dir>
#
# The shared libraries (.so files) and LICENSE are copied into <output_dir>/ffmpeg/.
# The resolver checks the ffmpeg/ subfolder relative to the app base directory.
#
# Source: https://github.com/BtbN/FFmpeg-Builds

set -e

OUTPUT_DIR="$1"
if [ -z "$OUTPUT_DIR" ]; then
    echo "Usage: download_ffmpeg_linux.sh <output_dir>"
    exit 1
fi

FFMPEG_URL="https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-n8.1-latest-linux64-lgpl-shared-8.1.tar.xz"
ARCHIVE_NAME="ffmpeg-linux64-lgpl-shared.tar.xz"
FFMPEG_SUBDIR="$OUTPUT_DIR/ffmpeg"

# Check if FFmpeg libraries already exist
if [ -d "$FFMPEG_SUBDIR" ] && [ -n "$(ls -A "$FFMPEG_SUBDIR"/lib*.so* 2>/dev/null)" ]; then
    echo "FFmpeg shared libraries already present in $FFMPEG_SUBDIR"
    echo "Skipping download..."
    exit 0
fi

TEMP_DIR=$(mktemp -d)

cleanup() {
    rm -rf "$TEMP_DIR"
}
trap cleanup EXIT

echo "Downloading FFmpeg shared libraries..."
# Add timeout and retry options for reliability
if ! wget --timeout=30 --tries=3 -q --show-progress -O "$TEMP_DIR/$ARCHIVE_NAME" "$FFMPEG_URL"; then
    echo "Error: Failed to download FFmpeg from $FFMPEG_URL"
    echo "Please check your internet connection and try again."
    exit 1
fi

echo "Extracting shared libraries..."
tar -xf "$TEMP_DIR/$ARCHIVE_NAME" -C "$TEMP_DIR"

# The archive extracts to a directory like ffmpeg-n8.1-latest-linux64-lgpl-shared-8.1/
EXTRACTED_DIR=$(find "$TEMP_DIR" -maxdepth 1 -type d -name 'ffmpeg-*' | head -1)
if [ -z "$EXTRACTED_DIR" ]; then
    echo "Error: could not find extracted FFmpeg directory"
    exit 1
fi

mkdir -p "$FFMPEG_SUBDIR"

# Copy all shared libraries (follow symlinks to get the actual files)
cp -L "$EXTRACTED_DIR"/lib/lib*.so* "$FFMPEG_SUBDIR/"
# Remove pkgconfig files if they got included
rm -f "$FFMPEG_SUBDIR"/*.pc 2>/dev/null || true

# Copy license for LGPL compliance
if [ -f "$EXTRACTED_DIR/LICENSE.txt" ]; then
    cp "$EXTRACTED_DIR/LICENSE.txt" "$FFMPEG_SUBDIR/FFmpeg-LICENSE.txt"
fi

echo "FFmpeg shared libraries copied to $FFMPEG_SUBDIR"
ls -lh "$FFMPEG_SUBDIR"/lib*.so* 2>/dev/null || true
