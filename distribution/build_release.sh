#!/bin/sh

# Linux build requirements:
#   sudo apt install libvlc-dev vlc libx11-dev

VERSION="$1"
if [ -z "$VERSION" ]; then
    echo "Usage: build_release.sh <version>"
    echo "Example: build_release.sh 2.4.10.1"
    exit 1
fi

dotnet publish ../CutTheRope/CutTheRope.csproj -c Release -f net10.0 -r win-x64 -p:VersionPrefix="$VERSION" -p:VersionSuffix= -o ../CutTheRope/bin/Publish/win-x64
dotnet publish ../CutTheRope/CutTheRope.csproj -c Release -f net10.0 -r osx-arm64 -f net10.0 -p:VersionPrefix="$VERSION" -p:VersionSuffix= -o ../CutTheRope/bin/Publish/osx-arm64
dotnet publish ../CutTheRope/CutTheRope.csproj -c Release -f net10.0 -r linux-x64 -p:VersionPrefix="$VERSION" -p:VersionSuffix= -o ../CutTheRope/bin/Publish/linux-x64
