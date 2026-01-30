#!/bin/sh

# Linux build requirements:
#   sudo apt install libvlc-dev vlc libx11-dev

dotnet publish ../CutTheRope/CutTheRope.csproj -c Release -f net9.0 -r win-x64 -o ../CutTheRope/bin/Publish/win-x64
dotnet publish ../CutTheRope/CutTheRope.csproj -c Release -f net9.0 -r osx-arm64 -o ../CutTheRope/bin/Publish/osx-arm64
dotnet publish ../CutTheRope/CutTheRope.csproj -c Release -f net9.0 -r linux-x64 -o ../CutTheRope/bin/Publish/linux-x64
