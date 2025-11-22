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

A stable release will be available on GitHub Releases soon. Stay tuned!

## Features

- New **Spooky Box**, ported from the Windows Phone version. More boxes coming soon (up to Lantern Box).
- **Dynamic level UI**, supports variable numbers of levels. Currently the code only support less than 25 levels.
- Support loading custom sprites and animations from [TexturePacker](https://www.codeandweb.com/texturepacker) in JSON array format. This allows easier modding and adding new assets.

## Goals

### Short-term goals

- [ ] **Add more boxes**, up to Lantern Box pack.
  - Later packs are **not** planned.
- [ ] **Cross-platform support**: Switch to cross-platform building.
  - This might be on hold due to MonoGame DesktopGL → DesktopVK (Vulkan) transition.
- [ ] **Video player**: Implement video player for intro and outro cutscenes, likely via LibVLCSharp.

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
   # Build the executable
   dotnet build

   # Format code according to .NET standards
   dotnet format
   ```
