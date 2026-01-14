#!/bin/sh
set -e

# =========================
# App metadata
# =========================
APP_NAME="CutTheRope"
EXEC_NAME="CutTheRope-DX"
BUNDLE_ID="page.yell0wsuit.cuttherope.dx"
ICON_NAME="CutTheRope"

# =========================
# Project / publish paths
# =========================
PROJECT="CutTheRope/CutTheRope.csproj"
PUBLISH_DIR="CutTheRope/bin/Publish/osx-arm64"
APP_DIR="$PUBLISH_DIR/$APP_NAME.app"
ICON_SOURCE="$PUBLISH_DIR/icons/CutTheRopeIcon.icns"

# =========================
# Resolve version from csproj
# =========================
VERSION=$(dotnet msbuild "$PROJECT" \
  -nologo -v:q \
  -getProperty:InformationalVersion \
  -p:Configuration=Release \
  -p:TargetFramework=net9.0)

echo "=== Building Cut The Rope: DX v$VERSION for macOS ==="

# =========================
# Step 1: Build the application
# =========================
echo "[1/3] Building macOS arm64 release..."
rm -rf "$PUBLISH_DIR"
dotnet publish "$PROJECT" \
    -c Release \
    -f net9.0 \
    -r osx-arm64 \
    -o "$PUBLISH_DIR"

# =========================
# Step 2: Create .app bundle
# =========================
echo "[2/3] Creating .app bundle structure..."
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
chmod +x "$APP_DIR/Contents/MacOS/$EXEC_NAME"

# Copy app icon
if [ -f "$ICON_SOURCE" ]; then
  cp "$ICON_SOURCE" "$APP_DIR/Contents/Resources/$ICON_NAME.icns"
  ICON_KEY="<key>CFBundleIconFile</key><string>$ICON_NAME</string>"
else
  echo "Warning: icon not found at $ICON_SOURCE"
  ICON_KEY=""
fi

# Write Info.plist
cat > "$APP_DIR/Contents/Info.plist" <<EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN"
 "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
  <key>CFBundleExecutable</key>
  <string>$EXEC_NAME</string>

  <key>CFBundleIdentifier</key>
  <string>$BUNDLE_ID</string>

  <key>CFBundleName</key>
  <string>$APP_NAME</string>

  <key>CFBundleVersion</key>
  <string>$VERSION</string>

  <key>CFBundleShortVersionString</key>
  <string>$VERSION</string>

  <key>CFBundlePackageType</key>
  <string>APPL</string>

  <key>NSHighResolutionCapable</key>
  <true/>
  $ICON_KEY
</dict>
</plist>
EOF

# =========================
# Step 3: Finalize
# =========================
echo "[3/3] Finalizing..."

# Dev convenience: remove quarantine attribute
xattr -dr com.apple.quarantine "$APP_DIR" || true

echo ""
echo "=== Build complete! ==="
echo "App bundle created: $APP_DIR"
echo ""
echo "To run: open $APP_DIR"
