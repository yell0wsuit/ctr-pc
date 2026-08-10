using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.Helpers;

namespace CutTheRopeDX.Framework.Platform
{
    /// <summary>
    /// Asset loading without a graphics device. Dimensions come from the build-time manifest
    /// (see content/Builder); no texture is ever created because nothing is drawn.
    /// </summary>
    internal sealed class HeadlessAssetPlatform : IAssetPlatform
    {
        private const string ImagesPrefix = "images/";

        private static Dictionary<string, (int W, int H)> manifest;

        /// <inheritdoc />
        public (int W, int H)? ImageDimensions(string contentPath)
        {
            manifest ??= LoadManifest();
            string key = contentPath.Replace('\\', '/');
            if (key.StartsWith(ImagesPrefix, StringComparison.Ordinal))
            {
                key = key[ImagesPrefix.Length..];
            }

            return manifest.TryGetValue(key, out (int W, int H) dims) ? dims : null;
        }

        /// <inheritdoc />
        public ITextureHandle ImageTexture(string contentPath)
        {
            return null;
        }

        /// <inheritdoc />
        public void FreeImage(string contentPath)
        {
            // No texture is ever created headless, so there is nothing to release.
        }

        /// <inheritdoc />
        public FontGeneric Font(string resourceName)
        {
            return new HeadlessFont();
        }

        /// <inheritdoc />
        public void ClearFontCache()
        {
            // No font cache exists headless, so there is nothing to clear.
        }

        private static Dictionary<string, (int W, int H)> LoadManifest()
        {
            string path = Path.Combine(ContentPaths.GetContentRootAbsolute(), "images", "image_dimensions.json");
            Dictionary<string, (int W, int H)> result = [];
            if (!File.Exists(path))
            {
                return result;
            }

            using JsonDocument doc = JsonDocument.Parse(File.ReadAllText(path));
            foreach (JsonProperty entry in doc.RootElement.GetProperty("images").EnumerateObject())
            {
                result[entry.Name] = (
                    entry.Value.GetProperty("w").GetInt32(),
                    entry.Value.GetProperty("h").GetInt32());
            }

            return result;
        }
    }
}
