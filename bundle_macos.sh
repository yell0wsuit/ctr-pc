#!/bin/sh
set -e

APP_NAME="CutTheRope"
EXEC_NAME="CutTheRope-DX"
BUNDLE_ID="page.yell0wsuit.cuttherope.dx"

VERSION=$(dotnet msbuild CutTheRope/CutTheRope.csproj \
  -nologo -v:q \
  -getProperty:InformationalVersion \
  -p:Configuration=Release \
  -p:TargetFramework=net9.0)

PUBLISH_DIR="CutTheRope/bin/Publish/osx-arm64"
APP_DIR="$PUBLISH_DIR/$APP_NAME.app"

echo "📦 Bundling Cut The Rope: DX v$VERSION"

# Clean old bundle
rm -rf "$APP_DIR"

# Create structure
mkdir -p "$APP_DIR/Contents/MacOS"
mkdir -p "$APP_DIR/Contents/Resources"

# Copy runtime files (exclude .app and content)
rsync -av \
  --exclude '*.app' \
  --exclude 'content' \
  "$PUBLISH_DIR/" \
  "$APP_DIR/Contents/MacOS/"

# Copy game content → Resources
if [ -d "$PUBLISH_DIR/content" ]; then
  rsync -av \
    "$PUBLISH_DIR/content/" \
    "$APP_DIR/Contents/Resources/content/"
else
  echo "⚠️ Warning: content folder not found"
fi

# Ensure executable bit
chmod +x "$APP_DIR/Contents/MacOS/$EXEC_NAME"

# Optional icon
if [ -f "macos/CutTheRope.icns" ]; then
  cp "macos/CutTheRope.icns" "$APP_DIR/Contents/Resources/"
  ICON_KEY="<key>CFBundleIconFile</key><string>CutTheRope</string>"
else
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

# Remove quarantine (dev convenience)
xattr -dr com.apple.quarantine "$APP_DIR" || true

echo "✅ $APP_NAME.app created successfully!"
echo "👉 $APP_DIR"
