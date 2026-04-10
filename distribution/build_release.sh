#!/bin/sh

# Requirements:
#   wget, tar, xz-utils (for downloading FFmpeg on Linux)

VERSION="$1"
if [ -z "$VERSION" ]; then
    echo "Usage: build_release.sh <version>"
    echo "Example: build_release.sh 2.4.10.1"
    exit 1
fi

dotnet publish ../CutTheRopeDX/CutTheRopeDX.csproj -c Release -f net10.0 -r win-x64 -p:VersionPrefix="$VERSION" -p:VersionSuffix= -o ../CutTheRopeDX/bin/Publish/win-x64
dotnet publish ../CutTheRopeDX/CutTheRopeDX.csproj -c Release -f net10.0 -r osx-arm64 -p:VersionPrefix="$VERSION" -p:VersionSuffix= -o ../CutTheRopeDX/bin/Publish/osx-arm64
dotnet publish ../CutTheRopeDX/CutTheRopeDX.csproj -c Release -f net10.0 -r linux-x64 -p:VersionPrefix="$VERSION" -p:VersionSuffix= -o ../CutTheRopeDX/bin/Publish/linux-x64
