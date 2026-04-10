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
PROJECT="$PROJECT_ROOT/CutTheRopeDX/CutTheRopeDX.csproj"
PUBLISH_DIR="$PROJECT_ROOT/CutTheRopeDX/bin/Publish/osx-arm64"
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

printf "Use NativeAOT? [Y/n]: "
read -r AOT_INPUT
case "$AOT_INPUT" in
    [nN]) USE_AOT="false" ;;
    *)    USE_AOT="true" ;;
esac

echo "=== Building Cut The Rope: DX v$VERSION for macOS (NativeAOT: $USE_AOT) ==="

# =========================
# Step 1: Build the application
# =========================
echo "[1/5] Building macOS arm64 release..."
rm -rf "$PUBLISH_DIR"
dotnet publish "$PROJECT" \
    -c Release \
    -f net10.0 \
    -p:PublishAot="$USE_AOT" \
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

# =========================
# Step 4: Finalize
# =========================
echo "[4/5] Finalizing..."

# Dev convenience: remove quarantine attribute
xattr -dr com.apple.quarantine "$APP_DIR" || true

# Ad-hoc codesign the entire .app bundle (deep signs all binaries and dylibs)
echo "Codesigning .app bundle..."
codesign --force --deep --sign - "$APP_DIR"

# =========================
# Step 5: Package .dmg
# =========================
echo "[5/5] Packaging .dmg archive..."

RELEASE_DIR="$PROJECT_ROOT/CutTheRopeDX/bin/release_github"
mkdir -p "$RELEASE_DIR"
ARCHIVE_NAME="CutTheRopeDX-v${VERSION}-macOS-arm64-ffmpeg.dmg"
ARCHIVE_PATH="$RELEASE_DIR/$ARCHIVE_NAME"

# Remove old archive if exists
rm -f "$ARCHIVE_PATH"

hdiutil create -volname "$APP_NAME" -srcfolder "$APP_DIR" -ov -format UDZO "$ARCHIVE_PATH"

echo ""
echo "=== Build complete! ==="
echo "App bundle: $APP_DIR"
echo "DMG:        $ARCHIVE_PATH"
