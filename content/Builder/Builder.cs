using System.Text.Json;

using Microsoft.Xna.Framework.Content.Pipeline.Audio;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

using MonoGame.Framework.Content.Pipeline.Builder;

namespace CutTheRopeDX.Content
{
    /// <summary>
    /// Declares the assets processed by the Cut the Rope DX content build.
    /// </summary>
    public sealed class GameContentBuilder : ContentBuilder
    {
        /// <inheritdoc />
        public override IContentCollection GetContentCollection()
        {
            ContentCollection content = new();
            content.SetContentRoot(string.Empty);

            // Build every asset the default importer/processor understands
            // (textures, sounds, songs). Non-buildable file types are handled
            // in later tasks via IncludeCopy / Exclude.
            content.Include<WildcardRule>("**/*.png");

            // Non-premultiplied cursors (the first assets the game loads).
            content.Exclude<WildcardRule>("images/cursor.png");
            content.Exclude<WildcardRule>("images/cursor_active.png");
            content.Include(
                "images/cursor.png",
                contentProcessor: new TextureProcessor { PremultiplyAlpha = false });
            content.Include(
                "images/cursor_active.png",
                contentProcessor: new TextureProcessor { PremultiplyAlpha = false });

            content.Include<WildcardRule>(
                "sounds/*.wav",
                contentProcessor: new SongProcessor { Quality = ConversionQuality.Best });
            content.Include<WildcardRule>(
                "sounds/sfx/*.wav",
                contentProcessor: new SoundEffectProcessor { Quality = ConversionQuality.Best });

            // Copy (do not build) content the game reads as raw files.
            content.IncludeCopy<WildcardRule>("maps/*.*");
            content.IncludeCopy<WildcardRule>("locales/*.*");
            content.IncludeCopy<WildcardRule>("fonts/*.*");
            content.IncludeCopy<WildcardRule>("video_hd/*.*");
            content.IncludeCopy<WildcardRule>("*.xml");
            content.IncludeCopy<WildcardRule>("*.json");
            content.IncludeCopy<WildcardRule>("*.cur");

            // Exclude legacy pipeline artifacts and prebuilt outputs.
            content.Exclude<WildcardRule>("bin/**/*");
            content.Exclude<WildcardRule>("obj/**/*");
            content.Exclude<WildcardRule>("Builder/**/*");
            content.Exclude<WildcardRule>("sounds/*.xnb");

            return content;
        }

        /// <summary>
        /// Writes pixel dimensions for every source image so headless runs can size textures
        /// without a GraphicsDevice. Source PNGs are not copied to the output and the built
        /// XNBs are LZ4-compressed, so this manifest is the only runtime dimension source.
        /// </summary>
        /// <param name="imagesSourceDir">Directory holding the source PNGs.</param>
        /// <param name="imagesOutputDir">Directory the manifest is written to.</param>
        public static void EmitImageDimensionsManifest(string imagesSourceDir, string imagesOutputDir)
        {
            Dictionary<string, ImageSize> images = [];
            foreach (string png in Directory.EnumerateFiles(imagesSourceDir, "*.png", SearchOption.AllDirectories))
            {
                (int w, int h) = ReadPngSize(png);
                string key = Path.GetRelativePath(imagesSourceDir, png)[..^4]
                    .Replace(Path.DirectorySeparatorChar, '/');
                images[key] = new ImageSize(w, h);
            }

            _ = Directory.CreateDirectory(imagesOutputDir);
            string outPath = Path.Combine(imagesOutputDir, "image_dimensions.json");
            File.WriteAllText(outPath, JsonSerializer.Serialize(new ImageDimensionsManifest(images)));
        }

        /// <summary>Reads width/height from a PNG IHDR header (bytes 16-23, big-endian).</summary>
        private static (int Width, int Height) ReadPngSize(string path)
        {
            byte[] header = new byte[24];
            using FileStream fs = File.OpenRead(path);
            if (fs.ReadAtLeast(header, 24, throwOnEndOfStream: false) < 24)
            {
                throw new InvalidDataException("Not a valid PNG: " + path);
            }

            int w = (header[16] << 24) | (header[17] << 16) | (header[18] << 8) | header[19];
            int h = (header[20] << 24) | (header[21] << 16) | (header[22] << 8) | header[23];
            return (w, h);
        }

        /// <summary>One image's pixel dimensions, as serialized into the manifest.</summary>
        /// <param name="W">Width in pixels.</param>
        /// <param name="H">Height in pixels.</param>
        private sealed record ImageSize(
            [property: System.Text.Json.Serialization.JsonPropertyName("w")] int W,
            [property: System.Text.Json.Serialization.JsonPropertyName("h")] int H);

        /// <summary>Root of the image dimensions manifest.</summary>
        /// <param name="Images">Dimensions keyed by resource name relative to <c>images/</c>.</param>
        private sealed record ImageDimensionsManifest(
            [property: System.Text.Json.Serialization.JsonPropertyName("images")]
            Dictionary<string, ImageSize> Images);
    }
}
