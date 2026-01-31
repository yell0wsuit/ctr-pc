# Cut the Rope: DX

## About

*Cut the Rope: DX (Decompiled Extra)* is a fan-made enhancement of the PC version of *Cut the Rope*, originally developed by ZeptoLab. This project aims to improve the original game's codebase, add new features, and enhance the overall gaming experience.

The game's source code is decompiled from the PC version, which serves as the foundation for development and feature expansion.

The project is currently led by [yell0wsuit](https://github.com/yell0wsuit), with help from [contributors](https://github.com/yell0wsuit/cuttherope-dx/graphs/contributors).

> [!NOTE]
> This project is not, and will never be affiliated with or endorsed by ZeptoLab. All rights to the original game and its assets belong to ZeptoLab.

### Related project

- [Cut the Rope: H5DX](https://github.com/yell0wsuit/cuttherope-h5dx): a web edition of Cut The Rope, originated from the FirefoxOS version, currently being developed to improve the game's experience further.

## Download

Download the latest release from the [Releases page](https://github.com/yell0wsuit/cuttherope-dx/releases).

Please note that the game is only available on Windows 10 and later. More platforms are planned in the future.

## Features

- More boxes beyond DJ Box, from Spooky Box to Mechanical Box.
- Seasonal Christmas theme and decorations, available during December and January.
- **Dynamic level UI**, supports variable numbers of levels. Currently, the code only supports fewer than 25 levels.
- Support loading custom sprites and animations from [TexturePacker](https://www.codeandweb.com/texturepacker) in JSON array format. This allows easier modding and adding new assets.
- Improved experience and bug fixes over the original PC version.
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

The development of *Cut the Rope: DX* is an ongoing process, and contributions are welcome! If you'd like to help out, please consider the following:

- **Reporting issues**: If you encounter any bugs or issues, please report them on the [GitHub Issues page](https://github.com/yell0wsuit/cuttherope-dx/issues).
- **Feature requests**: If you have ideas for new features or improvements, feel free to submit a feature request through Issues.
- **Contributing code**: If you're a developer and want to contribute code, please fork the repository and submit a pull request. Make sure to read the contribution guidelines in `CONTRIBUTING.md`.

### Testing the code

To test the game during the development process, follow these steps:

1. Ensure you have [latest .NET](https://dotnet.microsoft.com/en-us/download/dotnet/) installed on your machine.

2. Clone the repository to your PC:

    ```bash
    git clone https://github.com/yell0wsuit/cuttherope-dx.git
    cd cuttherope-dx
    ```

    You can also use [GitHub Desktop](https://desktop.github.com/) for ease of cloning.

3. For most cases, you can build the game using the following commands:

    ```bash
    # Windows
    dotnet build CutTheRope\CutTheRope.csproj -c Release -o .\CutTheRope\bin\Publish\win-x64

      # Alternative Windows build with native AOT for potential performance improvement
      # Only applicable for Windows 11 and later
      dotnet publish CutTheRope\CutTheRope.csproj -c Release -p:PublishAot=true -o .\CutTheRope\bin\Publish\win-x64


    # macOS
    dotnet publish CutTheRope\CutTheRope.csproj -c Release -r osx-arm64 -o .\CutTheRope\bin\Publish\osx-arm64

    # Linux
    dotnet publish CutTheRope\CutTheRope.csproj -c Release -r linux-x64 -o .\CutTheRope\bin\Publish\linux-x64
    ```
