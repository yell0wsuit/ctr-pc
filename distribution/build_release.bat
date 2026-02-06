@echo off
setlocal

set VERSION=%1
if "%VERSION%"=="" (
    echo Usage: build_release.bat ^<version^>
    echo Example: build_release.bat 2.4.10.1
    exit /b 1
)

dotnet publish ..\CutTheRope\CutTheRope.csproj -c Release -f net10.0 -r win-x64 -p:VersionPrefix=%VERSION% -p:VersionSuffix= -o ..\CutTheRope\bin\Publish\win-x64
dotnet publish ..\CutTheRope\CutTheRope.csproj -c Release -f net10.0 -r osx-arm64 -p:VersionPrefix=%VERSION% -p:VersionSuffix= -o ..\CutTheRope\bin\Publish\osx-arm64
dotnet publish ..\CutTheRope\CutTheRope.csproj -c Release -f net10.0 -r linux-x64 -p:VersionPrefix=%VERSION% -p:VersionSuffix= -o ..\CutTheRope\bin\Publish\linux-x64
