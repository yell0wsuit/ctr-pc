# Cut the Rope: DX

## About

*Cut the Rope: DX (Decompiled Extra)* is a fan-made enhancement of the PC version of *Cut the Rope*, originally developed by ZeptoLab. This project aims to improve the original game's codebase, add new features, and enhance the overall gaming experience.

The game's source code is decompiled from the PC version, which aids in the development of this project.

The project is currently led by [yell0wsuit](https://github.com/yell0wsuit).

> [!NOTE]
> This project is not, and will never be affiliated with or endorsed by ZeptoLab. All rights to the original game and its assets belong to ZeptoLab.

### Related project

- [Cut the Rope: H5DX](https://github.com/yell0wsuit/cuttherope-h5dx): a web edition of Cut The Rope, originated from the FirefoxOS version, currently being developed to improve the game's experience further.

## Download

We will publish a stable version on GitHub Releases soon.

## Features

- New Spooky Box, ported from the Windows Phone version. More boxes coming soon (until Lantern Box pack).
- Dynamic level UI count (currenly only less than 25 levels are supported)
- Support loading custom sprites and animations from [TexturePacker](https://www.codeandweb.com/texturepacker) in JSON array format. This allows easier modding and adding new assets.

## Goals

### Short-term goals

- [ ] **Add more boxes**, up to Lantern Box pack.
  - Latter box packs beyond Lantern will not be ported and/or considered.
- [ ] **Cross-platform support**: Switch to cross-platform building.
  - This might be on hold because MonoGame's DesktopGL (OpenGL), a cross-platform renderer, is going to be deprecated in favor of DesktopVK (Vulkan).
- [ ] **Video player**: Implement video to play cutscene for intro and outro. LibVLCSharp is a candidate.

### Long-term goals

- [ ] **Bugs fixing and polish**: Fix bugs, and ensure everything works smoothly.
- [ ] **Code optimization and modernization**: Optimize performance-critical code, and modernize codebase.

## Development & contributing

The development of *Cut the Rope: DX* is an ongoing process, and contributions are welcome! If you'd like to help out, please consider the following:

- **Reporting issues**: If you encounter any bugs or issues, please report them on the [GitHub Issues page](https://github.com/yell0wsuit/cuttherope-dx/issues).
- **Feature requests**: If you have ideas for new features or improvements, feel free to submit a feature request through Issues.
- **Contributing code**: If you're a developer and want to contribute code, please fork the repository and submit a pull request.

### Testing the code

To test the game during the development process, follow these steps:

1. Ensure you have installed [.NET 9](https://dotnet.microsoft.com/en-us/download/dotnet/9.0) installed on your machine.

2. Clone the repository to your PC:

    ```bash
    git clone https://github.com/yell0wsuit/cuttherope-dx.git
    cd cuttherope-dx
    ```

    You can also use [GitHub Desktop](https://desktop.github.com/) for ease of cloning.

3. Run the following commands:

   ```bash
   # Compile to binary build to run the program
   dotnet build

   # Code formatting to make it compilant to .NET code standards
   dotnet format
   ```
