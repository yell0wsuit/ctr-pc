#!/bin/bash

# Build script for creating a .deb package for Cut The Rope: DX
# Usage: `./build_deb.sh [version]` or `bash build_deb.sh [version]`
#
# Requirements:
#   - .NET 10.0 SDK
#   - sudo apt install libvlc-dev vlc libx11-dev

set -e

# Configuration
APP_NAME="cuttherope-dx"
APP_DISPLAY_NAME="Cut The Rope: DX"
ARCHITECTURE="amd64"
MAINTAINER="yell0wsuit"
DESCRIPTION="Cut the Rope: DX, a fan-made enhancement of the PC version of Cut the Rope."

# Directories
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
PROJECT="$PROJECT_ROOT/CutTheRope/CutTheRope.csproj"
BUILD_DIR="$SCRIPT_DIR/deb_build"
PUBLISH_DIR="$PROJECT_ROOT/CutTheRope/bin/Publish/linux-x64"
TEMPLATES_DIR="$SCRIPT_DIR/templates/linux"

# Resolve version (from arg or csproj)
VERSION="$1"
if [ -z "$VERSION" ]; then
    VERSION=$(dotnet msbuild "$PROJECT" \
      -nologo -v:q \
      -getProperty:InformationalVersion \
      -p:Configuration=Release)
fi

DEB_ROOT="$BUILD_DIR/${APP_NAME}_${VERSION}_${ARCHITECTURE}"

echo "=== Building Cut The Rope: DX v$VERSION .deb ==="

# Step 1: Build the application
echo "[1/5] Building Linux x64 release..."
rm -rf "$PUBLISH_DIR"
dotnet publish "$PROJECT" \
    -c Release \
    -r linux-x64 \
    ${1:+-p:VersionPrefix="$1" -p:VersionSuffix=} \
    -o "$PUBLISH_DIR"

# Step 2: Create directory structure
echo "[2/5] Creating .deb directory structure..."
rm -rf "$BUILD_DIR"
mkdir -p "$DEB_ROOT/DEBIAN"
mkdir -p "$DEB_ROOT/usr/bin"
mkdir -p "$DEB_ROOT/usr/share/applications"
mkdir -p "$DEB_ROOT/usr/share/icons/hicolor/512x512/apps"
mkdir -p "$DEB_ROOT/opt/$APP_NAME"

# Step 3: Copy application files
echo "[3/5] Copying application files..."
cp -r "$PUBLISH_DIR"/* "$DEB_ROOT/opt/$APP_NAME/"
chmod +x "$DEB_ROOT/opt/$APP_NAME/CutTheRope-DX"

# Create launcher script
cat > "$DEB_ROOT/usr/bin/$APP_NAME" << 'EOF'
#!/bin/bash
cd /opt/cuttherope-dx
exec ./CutTheRope-DX "$@"
EOF
chmod +x "$DEB_ROOT/usr/bin/$APP_NAME"

# Step 4: Create metadata files
echo "[4/5] Creating package metadata..."

# Control file
sed -e "s/{{APP_NAME}}/$APP_NAME/g" \
    -e "s/{{VERSION}}/$VERSION/g" \
    -e "s/{{ARCHITECTURE}}/$ARCHITECTURE/g" \
    -e "s/{{MAINTAINER}}/$MAINTAINER/g" \
    -e "s/{{DESCRIPTION}}/$DESCRIPTION/g" \
    "$TEMPLATES_DIR/deb.control" > "$DEB_ROOT/DEBIAN/control"

# Desktop entry
sed -e "s/{{APP_DISPLAY_NAME}}/$APP_DISPLAY_NAME/g" \
    -e "s/{{DESCRIPTION}}/$DESCRIPTION/g" \
    -e "s/{{APP_NAME}}/$APP_NAME/g" \
    "$TEMPLATES_DIR/deb.desktop" > "$DEB_ROOT/usr/share/applications/$APP_NAME.desktop"

# Copy icon
if [ -f "$SCRIPT_DIR/icons/CutTheRopeDXIcon_512.png" ]; then
    cp "$SCRIPT_DIR/icons/CutTheRopeDXIcon_512.png" "$DEB_ROOT/usr/share/icons/hicolor/512x512/apps/$APP_NAME.png"
fi

# Post-install script (optional - update icon cache)
cat > "$DEB_ROOT/DEBIAN/postinst" << 'EOF'
#!/bin/bash
if [ -x /usr/bin/gtk-update-icon-cache ]; then
    gtk-update-icon-cache -f -t /usr/share/icons/hicolor 2>/dev/null || true
fi
if [ -x /usr/bin/update-desktop-database ]; then
    update-desktop-database /usr/share/applications 2>/dev/null || true
fi
EOF
chmod +x "$DEB_ROOT/DEBIAN/postinst"

# Post-remove script
cat > "$DEB_ROOT/DEBIAN/postrm" << 'EOF'
#!/bin/bash
if [ -x /usr/bin/gtk-update-icon-cache ]; then
    gtk-update-icon-cache -f -t /usr/share/icons/hicolor 2>/dev/null || true
fi
if [ -x /usr/bin/update-desktop-database ]; then
    update-desktop-database /usr/share/applications 2>/dev/null || true
fi
EOF
chmod +x "$DEB_ROOT/DEBIAN/postrm"

# Step 5: Build .deb package
echo "[5/5] Building .deb package..."
dpkg-deb --build --root-owner-group "$DEB_ROOT"

# Move to Publish folder
mv "$BUILD_DIR/${APP_NAME}_${VERSION}_${ARCHITECTURE}.deb" "$PUBLISH_DIR/"

# Cleanup
rm -rf "$BUILD_DIR"

DEB_FILE="$PUBLISH_DIR/${APP_NAME}_${VERSION}_${ARCHITECTURE}.deb"
DEB_SIZE=$(ls -lh "$DEB_FILE" | awk '{print $5}')

echo ""
echo "=== Build complete! ==="
echo "Package created: $DEB_FILE ($DEB_SIZE)"
echo ""
echo "To install: sudo apt install $PUBLISH_DIR/${APP_NAME}_${VERSION}_${ARCHITECTURE}.deb"
echo "To uninstall: sudo apt remove $APP_NAME"
