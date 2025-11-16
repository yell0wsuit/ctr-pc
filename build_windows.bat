@echo off
cd CutTheRope.DesktopGL
dotnet publish -p:PublishProfile=Windows64 --runtime win-x64
dotnet publish -p:PublishProfile=Linux64 --runtime linux-x64