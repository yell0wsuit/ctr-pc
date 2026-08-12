# Cut the Rope: DX

<p align="center">
  <img alt="Logo of Cut the Rope: DX" src="./images/CutTheRopeDXLogo.svg"/>
</p>

## About

_Cut the Rope: DX (Decompiled Extra)_ is a fan-made enhancement of the PC version of _Cut the Rope_, originally developed by ZeptoLab. This project aims to improve the original game's codebase, add new features, and enhance the overall gaming experience.

The game's source code is decompiled from the PC version, which serves as the foundation for development and feature expansion.

This project is a part of the [_Cut the Rope Home_](https://ctrhome.github.io/fan-projects/) fan project, and is currently led by [yell0wsuit](https://github.com/yell0wsuit), with help from [contributors](https://github.com/yell0wsuit/cuttherope-dx/graphs/contributors).

The logo is designed by Bingies24 and darealmrcatz.

> [!NOTE]
> This project is not, and will never be affiliated with or endorsed by ZeptoLab. All rights to the original game and its assets belong to ZeptoLab.

### Related projects

- [Cut the Rope DX: Level Editor](https://github.com/yell0wsuit/ctrdx-editor/): a standalone app for creating and editing levels for Cut the Rope: DX.
- [Cut the Rope: H5DX](https://github.com/yell0wsuit/cuttherope-h5dx): a web edition of Cut The Rope, originated from the FirefoxOS version. Deprecated, superseded by this project's browser edition.

## Download

Download the latest release from the [Releases page](https://github.com/yell0wsuit/cuttherope-dx/releases).

## Features

- More boxes beyond DJ Box, from Spooky Box to Mechanical Box.
- Seasonal Christmas theme and decorations, available during December and January.
- Dynamic level UI, supports variable numbers of levels. Currently, the code only supports fewer than 25 levels.
- Support loading custom sprites and animations from [TexturePacker](https://www.codeandweb.com/texturepacker) in JSON array format. This allows easier modding and adding new assets.
- Improved experience and bug fixes over the original PC version.
- Runs in the browser: a WebAssembly build renders through Skia and installs as a PWA, so it plays offline once loaded. Saves live in `localStorage` rather than a file.
- Better save file format. The save file (`ctr_preferences.json`) is stored in a `CutTheRopeDX_SaveData` folder, with the following fallback priority:
    - Next to the game executable (preferred for portability)
    - `Documents/CutTheRopeDX_SaveData` -- if the above is not writable. Usually on macOS with `.app` bundle installation, or some Linux setups.
    - `%LOCALAPPDATA%/CutTheRopeDX_SaveData` (Windows) or equivalent on other platforms

## Goals

### Short-term goals

Please see [issue #68](https://github.com/yell0wsuit/cuttherope-dx/issues/68) for the current short-term goals.

### Long-term goals

- [ ] **Bug fixing and polish**: Fix bugs, and ensure everything works smoothly.
- [ ] **Code optimization and modernization**: Optimize performance-critical code, and modernize codebase.

## Development & contributing

The development of _Cut the Rope: DX_ is an ongoing process, and contributions are welcome! If you'd like to help out, please consider the following:

- **Reporting issues**: If you encounter any bugs or issues, please report them on the [GitHub Issues page](https://github.com/yell0wsuit/cuttherope-dx/issues).
- **Feature requests**: If you have ideas for new features or improvements, feel free to submit a feature request through Issues.
- **Contributing code**: If you're a developer and want to contribute code, please fork the repository and submit a pull request. Make sure to read the contribution guidelines in `CONTRIBUTING.md`.

### Testing the code

Do these steps to test the game while you develop it.

1. Install [.NET 10 or higher](https://dotnet.microsoft.com/en-us/download/dotnet/).

    > [!NOTE]  
    > The `global.json` file sets the minimum SDK version. It uses `rollForward: latestFeature`. Thus a newer 10.0.x SDK also works.
    > If your SDK is older than the minimum, each `dotnet` command stops with a version-mismatch error. Install a newer SDK to correct this.

2. Clone the repository to your computer:

    ```bash
    git clone https://github.com/yell0wsuit/cuttherope-dx.git
    cd cuttherope-dx
    ```

    You can also use [GitHub Desktop](https://desktop.github.com/) to clone the repository.

    > [!NOTE]  
    > This repository does not contain the content assets. The content assets are the images, the audio, the fonts, the cursors and the video. They are in the [ctrdx-assets](https://github.com/yell0wsuit/ctrdx-assets) repository.
    > You do not have to do anything. Your first local build downloads the assets from the most recent `ctrdx-assets` release. The build then extracts the assets into the `content/` directory.
    > CI does not do this, because CI does not compile the content.

3. Build the game with one of these commands.

    > [!NOTE]  
    > The `PublishAot` option has prerequisites. Obey the [AOT prerequisites](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/?tabs=windows%2Cnet8#prerequisites) for your operating system.

    a. Windows

    ```bash
    dotnet publish src\CutTheRopeDX.Desktop\CutTheRopeDX.Desktop.csproj -c Release -f net10.0 -p:PublishAot=true -o .\src\CutTheRopeDX.Desktop\bin\Publish\win-x64
    ```

    b. macOS

    Use this command if you do not use AVFoundation. The game then plays the video with FFmpeg. Install FFmpeg from [Homebrew](https://formulae.brew.sh/formula/ffmpeg) first.

    ```bash
    dotnet publish src/CutTheRopeDX.Desktop/CutTheRopeDX.Desktop.csproj -c Release -f net10.0 -r osx-arm64 -o ./src/CutTheRopeDX.Desktop/bin/Publish/osx-arm64
    ```

    Use this command if you use AVFoundation. AVFoundation needs macOS 26.0 or later, and Xcode.

    ```bash
    dotnet publish src/CutTheRopeDX.Desktop/CutTheRopeDX.Desktop.csproj -c Release -f net10.0-macos -r osx-arm64 -p:PublishAot=true -o ./src/CutTheRopeDX.Desktop/bin/Publish/osx-arm64
    ```

    > [!NOTE]  
    > Change `osx-arm64` to `osx-x64` to build the game for an Intel Mac. We do not know if the game operates correctly on an Intel Mac.

    c. Linux

    ```bash
    dotnet publish src/CutTheRopeDX.Desktop/CutTheRopeDX.Desktop.csproj -c Release -f net10.0 -p:PublishAot=true -o ./src/CutTheRopeDX.Desktop/bin/Publish/linux-x64
    ```

    > [!WARNING]  
    > A native AOT binary from Linux operates only on the same Linux version, or on a newer Linux version.

    If native AOT causes a problem, remove the `-p:PublishAot=true` option. Then build the game again.

    d. Browser (WebAssembly)

    The browser build needs the WebAssembly workload. It also needs its own content. A Python script converts the desktop assets to WebP files, Ogg Vorbis files, WebM videos and subset fonts. Do this conversion before you build the game. The script needs Python 3.11 or a newer version.

    ```bash
    dotnet workload install wasm-tools
    python3 -m pip install pillow fonttools
    dotnet restore content/Builder/CutTheRopeDX.Content.csproj
    python3 scripts/build_web_content.py
    ```

    > [!NOTE]  
    > The `dotnet restore` command puts the correct FFmpeg in your NuGet cache. The script does not use the FFmpeg from the `PATH`. Many FFmpeg builds do not have the `libvorbis` encoder and the `libwebp` encoder. Such a build fails at a later time.

    > [!NOTE]
    > The cutscene conversion is the one exception. The pinned FFmpeg has no video encoder, so this step uses the FFmpeg from your `PATH` and needs the `libvpx-vp9` encoder and the `libopus` encoder. If your FFmpeg does not have them, the script prints a warning and continues. The game then works, but it plays no cutscenes. Add `--require-video` to make this an error instead; the GitHub Pages workflow does this, because a published build must not lose its cutscenes without a word.

    The conversion is incremental. Do the conversion again only after you change an asset. The script reports each file as it converts it: a terminal gets one line that updates in place, and a log file or a CI job gets a line every few seconds. Add `--no-progress` for the stage totals alone.

    Start the game in your browser:

    ```bash
    dotnet run --project src/CutTheRopeDX.Browser/CutTheRopeDX.Browser.csproj
    ```

    Or publish the static site:

    ```bash
    dotnet publish src/CutTheRopeDX.Browser/CutTheRopeDX.Browser.csproj -c Release -o dist
    ```

    The build writes the site to the `dist/wwwroot` directory. The [Deploy Browser to GitHub Pages](.github/workflows/deploy-pages.yml) workflow sends this site to GitHub Pages. You must start this workflow manually.

4. Run the unit tests:

    ```bash
    dotnet test CutTheRopeDX.slnx -p:ExcludeMacOSTarget=true
    ```

## Running a custom level

_Cut the Rope: DX_ can launch straight into a level XML file instead of the normal game, which is intended for level editors and other external tools.

```bash
CutTheRopeDX --level <path-to-level.xml>
```

The path may be absolute or relative, and the file does not need to live under the content directory.

You can also drag an `.xml` level file onto the executable — a bare `.xml` argument is treated the same as `--level <path>`. If both are given, `--level` wins.

### Behavior

- The splash screen and menu are skipped; the level loads and plays immediately.
- No scores, stars, or unlocks are written. A custom run never touches saved progress.
- The pause menu offers **Continue**, **Quit**, and the sound and music toggles.
- The result screen offers **Replay** only.

### Exit codes

- `0` on normal exit.
- Nonzero when the level file is missing, unreadable, or malformed. The reason is printed to stderr and no window is created.

### Hot reloading

Rewriting the level file while the game is running reloads it automatically. If the edited level needs only resources that are already loaded, it restarts in place; otherwise it reloads through the loading screen.

A malformed file is reported on stderr and leaves the running level untouched, so a partial write will not crash the game. Even so, writers should write atomically—write to a temp file, then move it over the target—so the game never reads a half-written file.
