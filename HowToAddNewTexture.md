# TexturePacker JSON Integration Guide

This guide explains how to add and register new textures using TexturePacker's JSON format instead of the legacy XML quad system.

## Overview

The project now supports **TexturePacker JSON atlases** which eliminates the need for manual XML quad definitions. The parser automatically extracts sprite frame coordinates from the JSON metadata.

## File Structure

Each TexturePacker texture requires two files in the `content/` directory:

```text
content/
├── my_texture.json      # TexturePacker metadata (frame coordinates)
└── my_texture.xnb       # Compiled XNB texture file
```

## Step-by-Step: Adding a New Texture

### 1. Export from TexturePacker

In TexturePacker:

1. Configure your sprite sheet
2. Go to **File → Publish** or **Export**
3. Select **JSON Hash** or **JSON Array** format (we recommend the JSON Array format)
4. Save as `my_texture.json`
5. Convert `my_texture.png` to XNB format
6. Move both `my_texture.json` and `my_texture.xnb` to `content/` directory

You can also visit <https://free-tex-packer.com/app/> for packing sprites online.

> [!NOTE]
> Always remember to **uncheck** "Allow rotation" option in TexturePacker
> ![Allow rotation option location](images/texturepacker_allowrotation.png)

**File naming is critical:** The JSON and XNB must share the same base name (e.g., `obj_candy_02.json` and `obj_candy_02.xnb`)

### 2. Add Resource Name Constant

For image/texture, add a new constant to `Resources.Img` class in `Resources.cs`.
For sound or music, add a new constant to `Resources.Snd` class in `Resources.cs`.

```csharp
public static class Resources
{
    public static class Img
    {
        // ... existing constants ...
        public const string MyNewTexture = "my_new_texture";
    }
}
```

> [!WARNING]
>
> - The string constant value must match your JSON and XNB filenames (`my_new_texture.json` + `my_new_texture.xnb`)
> - Use the constant `Resources.Img.MyNewTexture` throughout your code instead of hardcoded strings or numeric IDs

### 3. Register in `TexturePackerRegistry.json`

Edit `content/TexturePackerRegistry.json` and add your texture to the `textures` array:

```json
{
  "textures": [
    {
      "resourceName": "obj_candy_01",
      "atlasPath": "obj_candy_01.json",
      "useAntialias": true,
      "frameOrder": null,
      "centerOffsets": false
    },
    {
      "resourceName": "my_new_texture",
      "atlasPath": "my_new_texture.json",
      "useAntialias": true,
      "frameOrder": null,
      "centerOffsets": false
    }
  ]
}
```

**No code changes needed!** Just add the entry to the JSON and it's automatically loaded.

To replace texture, simply add same `resourceId` number and filename in `TexturePackerRegistry.json`.

### 4. Configuration options

Each texture entry in `TexturePackerRegistry.json` supports the following properties:

| Property | Type | Description |
|----------|------|-------------|
| `resourceName` | `string` | Unique resource name (must match the constant value in `Resources.Img`) |
| `atlasPath` | `string` | Relative path to JSON file (e.g., `"obj_spider.json"`) |
| `useAntialias` | `bool` | Enable antialiasing (default: `true`) |
| `frameOrder` | `string[]` or `null` | Optional: Reorder frames by name (leave `null` for TexturePacker order) |
| `centerOffsets` | `bool` | Calculate offsets from center instead of reading from JSON. Only use this if you have jittering animation (default: `false`) |

## How it works

1. **Resource Load Request** → Game requests texture using `Resources.Img.*` constant
2. **Config Lookup** → `GetTextureAtlasConfig()` checks if resource should use TexturePacker
3. **JSON Parse** → If configured, parser loads JSON and extracts frame rectangles
4. **XNB Load** → Texture loads from matching `.xnb` file (derived from JSON filename)
5. **Frame Application** → Sprite frame coordinates applied to texture quads
6. **Rendering** → Game renders individual frames with correct coordinates

No XML quad definitions or hardcoded numeric IDs needed!

## Troubleshooting

### "texture not found: my_texture"

- **Cause:** JSON exists but XNB is missing or named incorrectly
- **Fix:** Ensure `my_texture.xnb` exists in `content/` with exact same base name as JSON

### "TexturePacker atlas is missing the frames block"

- **Cause:** JSON format is invalid or unrecognized
- **Fix:** Verify JSON is valid TexturePacker export; use JSON linter to check syntax

### Sprite displays but coordinates are wrong

- **Cause:** Frame coordinates in JSON don't match actual sprite sheet
- **Fix:** Re-export from TexturePacker with correct sprite sheet alignment; also make sure the sprite is not rotated

### Frames are in wrong order

- **Cause:** Default TexturePacker ordering doesn't match expected animation sequence
- **Fix:** Use `FrameOrder` property to specify frame sequence:

  ```csharp
  FrameOrder = new[] { "frame_0001", "frame_0000", "frame_0002" }
  ```
