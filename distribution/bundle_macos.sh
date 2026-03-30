#!/bin/sh
set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

# =========================
# App metadata
# =========================
APP_NAME="CutTheRope-DX"
BUNDLE_ID="page.yell0wsuit.cuttherope.dx"

# =========================
# Project / publish paths
# =========================
PROJECT="$PROJECT_ROOT/CutTheRope/CutTheRope.csproj"
PUBLISH_DIR="$PROJECT_ROOT/CutTheRope/bin/Publish/osx-arm64"
APP_DIR="$PUBLISH_DIR/$APP_NAME.app"
ICON_SOURCE="$PUBLISH_DIR/Resources/CutTheRopeDXIcon.icns"
TEMPLATES_DIR="$SCRIPT_DIR/templates/macos"

# =========================
# Resolve version (from arg or csproj)
# =========================
VERSION="$1"
if [ -z "$VERSION" ]; then
    echo "Error: version is required. Usage: $0 <version>"
    exit 1
fi

echo "=== Building Cut The Rope: DX v$VERSION for macOS ==="

# =========================
# Step 1: Build the application
# =========================
echo "[1/5] Building macOS arm64 release..."
rm -rf "$PUBLISH_DIR"
dotnet publish "$PROJECT" \
    -c Release \
    -f net10.0 \
    -p:PublishAot=true \
    -r osx-arm64 \
    ${1:+-p:VersionPrefix="$1" -p:VersionSuffix=} \
    -o "$PUBLISH_DIR"

# =========================
# Step 2: Create .app bundle
# =========================
echo "[2/5] Creating .app bundle structure..."
mkdir -p "$APP_DIR/Contents/MacOS"
mkdir -p "$APP_DIR/Contents/Resources"

# Copy runtime files
rsync -av \
  --exclude '*.app' \
  --exclude 'content' \
  --exclude 'icons' \
  "$PUBLISH_DIR/" \
  "$APP_DIR/Contents/MacOS/"

# Copy game content
if [ -d "$PUBLISH_DIR/content" ]; then
  rsync -av \
    "$PUBLISH_DIR/content/" \
    "$APP_DIR/Contents/Resources/content/"
else
  echo "Warning: content folder not found"
fi

# Ensure executable bit
chmod +x "$APP_DIR/Contents/MacOS/$APP_NAME"

# Copy app icon
if [ -f "$ICON_SOURCE" ]; then
  cp "$ICON_SOURCE" "$APP_DIR/Contents/Resources/$APP_NAME.icns"
else
  echo "Warning: icon not found at $ICON_SOURCE"
fi

# Write Info.plist
sed -e "s/{{APP_NAME}}/$APP_NAME/g" \
    -e "s/{{BUNDLE_ID}}/$BUNDLE_ID/g" \
    -e "s/{{VERSION}}/$VERSION/g" \
    "$TEMPLATES_DIR/Info.plist" > "$APP_DIR/Contents/Info.plist"

# =========================
# Step 3: Bundle FFmpeg
# =========================
echo "[3/5] Bundling FFmpeg dylibs into Frameworks..."
"$SCRIPT_DIR/bundle_ffmpeg_macos.sh" "$APP_DIR/Contents/Frameworks"

# Codesign the bundled dylibs (required on macOS to avoid crashes)
echo "Codesigning bundled dylibs..."
for dylib in "$APP_DIR/Contents/Frameworks"/*.dylib; do
    codesign --force --sign - "$dylib"
done

# =========================
# Step 4: Finalize
# =========================
echo "[4/5] Finalizing..."

# Dev convenience: remove quarantine attribute
xattr -dr com.apple.quarantine "$APP_DIR" || true

# =========================
# Step 5: Package .7z
# =========================
echo "[5/5] Packaging .7z archive..."

# Ensure 7z is available (brew install 7zip)
if ! command -v 7z &> /dev/null; then
    echo "7z not found. Install with: brew install 7zip"
    exit 1
fi

RELEASE_DIR="$PROJECT_ROOT/CutTheRope/bin/release_github"
mkdir -p "$RELEASE_DIR"
ARCHIVE_NAME="CutTheRopeDX-v${VERSION}-macOS-arm64-ffmpeg.7z"
ARCHIVE_PATH="$RELEASE_DIR/$ARCHIVE_NAME"

# Remove old archive if exists
rm -f "$ARCHIVE_PATH"

(cd "$PUBLISH_DIR" && 7z a -t7z -m0=lzma -mx=9 "$ARCHIVE_PATH" "$APP_NAME.app")

echo ""
echo "=== Build complete! ==="
echo "App bundle: $APP_DIR"
echo "Archive:    $ARCHIVE_PATH"
