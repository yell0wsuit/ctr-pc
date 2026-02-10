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
    VERSION=$(dotnet msbuild "$PROJECT" \
      -nologo -v:q \
      -getProperty:InformationalVersion \
      -p:Configuration=Release)
fi

echo "=== Building Cut The Rope: DX v$VERSION for macOS ==="

# =========================
# Step 1: Build the application
# =========================
echo "[1/4] Building macOS arm64 release..."
rm -rf "$PUBLISH_DIR"
dotnet publish "$PROJECT" \
    -c Release \
    -f net10.0 \
    -r osx-arm64 \
    ${1:+-p:VersionPrefix="$1" -p:VersionSuffix=} \
    -o "$PUBLISH_DIR"

# =========================
# Step 2: Create .app bundle
# =========================
echo "[2/4] Creating .app bundle structure..."
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
echo "[3/4] Bundling FFmpeg dylibs into Frameworks..."
"$SCRIPT_DIR/bundle_ffmpeg_macos.sh" "$APP_DIR/Contents/Frameworks"

# Codesign the bundled dylibs (required on macOS to avoid crashes)
echo "Codesigning bundled dylibs..."
for dylib in "$APP_DIR/Contents/Frameworks"/*.dylib; do
    codesign --force --sign - "$dylib"
done

# =========================
# Step 4: Finalize
# =========================
echo "[4/4] Finalizing..."

# Dev convenience: remove quarantine attribute
xattr -dr com.apple.quarantine "$APP_DIR" || true

echo ""
echo "=== Build complete! ==="
echo "App bundle created: $APP_DIR"
echo ""
echo "To run: open $APP_DIR"
