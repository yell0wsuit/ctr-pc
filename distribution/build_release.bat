@echo off
setlocal

set VERSION=%1
if "%VERSION%"=="" (
    echo Usage: build_release.bat ^<version^>
    echo Example: build_release.bat 2.4.10.1
    exit /b 1
)

set WIN_OUT=..\CutTheRopeDX\bin\Publish\win-x64

rem Windows ships both graphics backends plus a launcher that picks between them; the OpenGL build is
rem for machines whose Vulkan is missing or software-only.
rem
rem These publishes keep their managed assemblies, so both builds would write a MonoGame.Framework.dll
rem of the same name and different content: they get a directory each, and the launcher accepts that
rem layout. release_windows.py compiles ahead of time instead, which folds the assemblies into the
rem executables and lets both share one directory. Use it for anything shipped.
if exist "%WIN_OUT%" rmdir /s /q "%WIN_OUT%"

dotnet publish ..\CutTheRopeDX\CutTheRopeDX.csproj -c Release -f net10.0 -r win-x64 -p:GraphicsBackend=VK -p:VersionPrefix=%VERSION% -p:VersionSuffix= -o "%WIN_OUT%\vk" || exit /b 1
dotnet publish ..\CutTheRopeDX\CutTheRopeDX.csproj -c Release -f net10.0 -r win-x64 -p:GraphicsBackend=GL -p:VersionPrefix=%VERSION% -p:VersionSuffix= -o "%WIN_OUT%\gl" || exit /b 1
dotnet publish ..\CutTheRopeDX.Launcher\CutTheRopeDX.Launcher.csproj -c Release -f net10.0 -r win-x64 -p:VersionPrefix=%VERSION% -p:VersionSuffix= -o "%WIN_OUT%" || exit /b 1

rem One content copy serves both builds: the game looks beside its own executable first and one
rem directory up second.
move "%WIN_OUT%\vk\content" "%WIN_OUT%\content" >nul || exit /b 1
if exist "%WIN_OUT%\gl\content" rmdir /s /q "%WIN_OUT%\gl\content"

rem Players start the launcher, which builds under its own assembly name so it does not collide with
rem the game. Renaming the apphost is safe; it finds its assembly by an embedded name.
move "%WIN_OUT%\CutTheRopeDX.Launcher.exe" "%WIN_OUT%\CutTheRope-DX.exe" >nul || exit /b 1

rem macOS and Linux ship the game executable on its own, with content beside it: no launcher, no
rem backend directories, because neither has hardware old enough to need the OpenGL fallback.
dotnet publish ..\CutTheRopeDX\CutTheRopeDX.csproj -c Release -f net10.0 -r osx-arm64 -p:VersionPrefix=%VERSION% -p:VersionSuffix= -o ..\CutTheRopeDX\bin\Publish\osx-arm64
dotnet publish ..\CutTheRopeDX\CutTheRopeDX.csproj -c Release -f net10.0 -r linux-x64 -p:VersionPrefix=%VERSION% -p:VersionSuffix= -o ..\CutTheRopeDX\bin\Publish\linux-x64
