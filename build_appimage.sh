#!/bin/bash

# Build script for creating an AppImage for Cut The Rope: DX
# Usage: `./build_appimage.sh` or `bash build_appimage.sh`
#
# Requirements:
#   - .NET 9.0 SDK
#   - wget (for downloading appimagetool if not present)

set -e

# Configuration
APP_NAME="CutTheRope-DX"
APP_ID="page.yell0wsuit.cuttherope.dx"
APP_DISPLAY_NAME="Cut The Rope: DX"
EXEC_NAME="CutTheRope-DX"
DESCRIPTION="Cut the Rope: DX, a fan-made enhancement of the PC version of Cut the Rope."

# Directories
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT="CutTheRope/CutTheRope.csproj"
BUILD_DIR="$SCRIPT_DIR/appimage_build"
PUBLISH_DIR="$SCRIPT_DIR/CutTheRope/bin/Publish/linux-x64"
APPDIR="$BUILD_DIR/$APP_NAME.AppDir"
TOOLS_DIR="$SCRIPT_DIR/tools"

# Resolve version from csproj
VERSION=$(dotnet msbuild "$PROJECT" \
  -nologo -v:q \
  -getProperty:InformationalVersion \
  -p:Configuration=Release \
  -p:TargetFramework=net9.0)

echo "=== Building Cut The Rope: DX v$VERSION AppImage ==="

# Step 1: Build the application
echo "[1/5] Building Linux x64 release..."
rm -rf "$PUBLISH_DIR"
dotnet publish "$PROJECT" \
    -c Release \
    -f net9.0 \
    -r linux-x64 \
    -o "$PUBLISH_DIR"

# Step 2: Create AppDir structure
echo "[2/5] Creating AppDir structure..."
rm -rf "$BUILD_DIR"
mkdir -p "$APPDIR/usr/bin"
mkdir -p "$APPDIR/usr/share/applications"
mkdir -p "$APPDIR/usr/share/icons/hicolor/512x512/apps"

# Step 3: Copy application files
echo "[3/5] Copying application files..."

# Copy all published files to usr/bin (this includes the executable, content, etc.)
cp -r "$PUBLISH_DIR"/* "$APPDIR/usr/bin/"
chmod +x "$APPDIR/usr/bin/$EXEC_NAME"

# Create AppRun script
cat > "$APPDIR/AppRun" << 'EOF'
#!/bin/bash
SELF=$(readlink -f "$0")
HERE=${SELF%/*}
export PATH="${HERE}/usr/bin:${PATH}"
cd "${HERE}/usr/bin"
exec "./__EXEC_NAME__" "$@"
EOF
sed -i "s/__EXEC_NAME__/$EXEC_NAME/g" "$APPDIR/AppRun"
chmod +x "$APPDIR/AppRun"

# Step 4: Create metadata files
echo "[4/5] Creating metadata files..."

# Desktop entry (in root of AppDir for appimagetool)
cat > "$APPDIR/$APP_NAME.desktop" << EOF
[Desktop Entry]
Name=$APP_DISPLAY_NAME
Comment=$DESCRIPTION
Exec=$EXEC_NAME
Icon=$APP_NAME
Terminal=false
Type=Application
Categories=Game;
Keywords=puzzle;game;cut;rope;omnom;
X-AppImage-Name=$APP_DISPLAY_NAME
X-AppImage-Version=$VERSION
EOF

# Also copy to standard location
cp "$APPDIR/$APP_NAME.desktop" "$APPDIR/usr/share/applications/"

# Copy icon to root of AppDir (required by appimagetool)
if [ -f "$SCRIPT_DIR/CutTheRope/icons/CutTheRopeIcon_512.png" ]; then
    cp "$SCRIPT_DIR/CutTheRope/icons/CutTheRopeIcon_512.png" "$APPDIR/$APP_NAME.png"
    cp "$SCRIPT_DIR/CutTheRope/icons/CutTheRopeIcon_512.png" "$APPDIR/usr/share/icons/hicolor/512x512/apps/$APP_NAME.png"
    # Create .DirIcon symlink (optional but nice for file managers)
    ln -sf "$APP_NAME.png" "$APPDIR/.DirIcon"
else
    echo "Warning: Icon not found at CutTheRope/icons/CutTheRopeIcon_512.png"
fi

# Create AppStream metadata (optional but recommended)
mkdir -p "$APPDIR/usr/share/metainfo"
cat > "$APPDIR/usr/share/metainfo/$APP_ID.metainfo.xml" << EOF
<?xml version="1.0" encoding="UTF-8"?>
<component type="desktop-application">
  <id>$APP_ID</id>
  <name>$APP_DISPLAY_NAME</name>
  <summary>A fan-made enhancement of the PC version of Cut the Rope</summary>
  <metadata_license>CC-BY-SA-4.0</metadata_license>
  <developer id="$APP_ID">
    <name>yell0wsuit</name>
  </developer>
  <description>
    <p>Cut the Rope: DX is a fan-made enhancement of the PC version of Cut the Rope, featuring improved graphics, additional content, and quality-of-life improvements.</p>
  </description>
  <launchable type="desktop-id">$APP_NAME.desktop</launchable>
  <url type="homepage">https://github.com/yell0wsuit/cuttherope-dx</url>
  <provides>
    <binary>$EXEC_NAME</binary>
  </provides>
  <content_rating type="oars-1.1"/>
  <releases>
    <release version="$VERSION" date="$(date +%Y-%m-%d)"/>
  </releases>
</component>
EOF

# Step 5: Build AppImage
echo "[5/5] Building AppImage..."

# Download appimagetool if not available
APPIMAGETOOL="$TOOLS_DIR/appimagetool-x86_64.AppImage"
if [ ! -f "$APPIMAGETOOL" ]; then
    echo "Downloading appimagetool..."
    mkdir -p "$TOOLS_DIR"
    wget -q --show-progress -O "$APPIMAGETOOL" \
        "https://github.com/AppImage/appimagetool/releases/download/continuous/appimagetool-x86_64.AppImage"
    chmod +x "$APPIMAGETOOL"
fi

# Build the AppImage
ARCH=x86_64 "$APPIMAGETOOL" "$APPDIR" "$PUBLISH_DIR/${APP_NAME}-${VERSION}-x86_64.AppImage"

# Cleanup build directory
rm -rf "$BUILD_DIR"

APPIMAGE_FILE="$PUBLISH_DIR/${APP_NAME}-${VERSION}-x86_64.AppImage"
APPIMAGE_SIZE=$(ls -lh "$APPIMAGE_FILE" | awk '{print $5}')

echo ""
echo "=== Build complete! ==="
echo "AppImage created: $APPIMAGE_FILE ($APPIMAGE_SIZE)"
echo ""
echo "To run: chmod +x $APPIMAGE_FILE && $APPIMAGE_FILE"
echo "Or simply double-click the AppImage file in your file manager."
