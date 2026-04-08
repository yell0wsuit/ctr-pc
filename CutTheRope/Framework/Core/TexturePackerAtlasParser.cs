using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace CutTheRope.Framework.Core
{
    /// <summary>
    /// Parsed TexturePacker atlas data prepared for the engine's rectangle/offset-based consumers.
    /// </summary>
    internal sealed class ParsedTexturePackerAtlas
    {
        /// <summary>
        /// Parsed frame rectangles in atlas texture coordinates.
        /// </summary>
        public List<CTRRectangle> Rects { get; } = [];

        /// <summary>
        /// Per-frame offsets applied when drawing trimmed sprites.
        /// </summary>
        public List<Vector> Offsets { get; } = [];

        /// <summary>
        /// Original untrimmed source size for each frame.
        /// </summary>
        public List<Vector> SourceSizes { get; } = [];

        /// <summary>
        /// Whether any parsed frame has a non-zero offset.
        /// </summary>
        public bool HasNonZeroOffset { get; set; }

        /// <summary>
        /// Maximum original source width observed across all frames.
        /// </summary>
        public float PreCutWidth { get; set; }

        /// <summary>
        /// Maximum original source height observed across all frames.
        /// </summary>
        public float PreCutHeight { get; set; }
    }

    /// <summary>
    /// Options that control TexturePacker atlas parsing and post-processing.
    /// </summary>
    internal sealed class TexturePackerParserOptions
    {
        /// <summary>
        /// Optional explicit frame ordering applied after parsing.
        /// </summary>
        public IReadOnlyList<string> FrameOrder { get; init; }

        /// <summary>
        /// Whether frame offsets should be normalized to centered trim offsets.
        /// </summary>
        public bool NormalizeOffsetsToCenter { get; init; }
    }

    /// <summary>
    /// Parses TexturePacker JSON atlas data into the engine's runtime atlas representation.
    /// </summary>
    internal static class TexturePackerAtlasParser
    {
        /// <summary>
        /// Parses TexturePacker atlas JSON into rectangle, offset, and source-size data.
        /// </summary>
        /// <param name="json">TexturePacker atlas JSON text.</param>
        /// <param name="options">Optional parsing and post-processing options.</param>
        /// <returns>Parsed atlas data.</returns>
        /// <exception cref="InvalidDataException">Thrown when the JSON is empty or required TexturePacker blocks are missing.</exception>
        public static ParsedTexturePackerAtlas Parse(string json, TexturePackerParserOptions options)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new InvalidDataException("TexturePacker atlas JSON is empty.");
            }

            using JsonDocument document = JsonDocument.Parse(json);
            if (!document.RootElement.TryGetProperty("frames", out JsonElement framesElement))
            {
                throw new InvalidDataException("TexturePacker atlas is missing the frames block.");
            }

            List<FrameEntry> entries = CreateFrameEntries(framesElement);
            if (entries.Count == 0)
            {
                throw new InvalidDataException("TexturePacker atlas does not contain any frames.");
            }

            if (options?.FrameOrder != null && options.FrameOrder.Count > 0)
            {
                entries = OrderFrameEntries(entries, options.FrameOrder);
            }

            ParsedTexturePackerAtlas atlas = new();
            List<(float w, float h)> rectSizes = new(entries.Count);

            foreach (FrameEntry entry in entries)
            {
                ParseFrame(entry, atlas, rectSizes);
            }

            if (options?.NormalizeOffsetsToCenter == true)
            {
                ApplyCenteredOffsets(atlas, rectSizes);
            }

            return atlas;
        }

        /// <summary>
        /// Creates a normalized list of named frame entries from the TexturePacker <c>frames</c> block.
        /// </summary>
        /// <param name="framesElement">JSON element representing the <c>frames</c> block.</param>
        /// <returns>Parsed frame entries.</returns>
        private static List<FrameEntry> CreateFrameEntries(JsonElement framesElement)
        {
            List<FrameEntry> entries = [];
            if (framesElement.ValueKind == JsonValueKind.Array)
            {
                int index = 0;
                foreach (JsonElement frame in framesElement.EnumerateArray())
                {
                    string name = TryGetString(frame, "filename") ?? TryGetString(frame, "name") ?? index.ToString(CultureInfo.InvariantCulture);
                    entries.Add(new FrameEntry(name, frame));
                    index++;
                }
                return entries;
            }

            if (framesElement.ValueKind == JsonValueKind.Object)
            {
                foreach (JsonProperty property in framesElement.EnumerateObject())
                {
                    entries.Add(new FrameEntry(property.Name, property.Value));
                }
                entries.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
            }

            return entries;
        }

        /// <summary>
        /// Reorders parsed frame <paramref name="entries"/> according to an explicit frame-order list.
        /// Unlisted frames remain sorted by name after listed <paramref name="entries"/>.
        /// </summary>
        /// <param name="entries">Frame entries to reorder.</param>
        /// <param name="frameOrder">Preferred frame order.</param>
        /// <returns>Reordered frame <paramref name="entries"/>.</returns>
        private static List<FrameEntry> OrderFrameEntries(List<FrameEntry> entries, IReadOnlyList<string> frameOrder)
        {
            Dictionary<string, int> order = new(StringComparer.Ordinal);
            for (int i = 0; i < frameOrder.Count; i++)
            {
                string name = frameOrder[i];
                if (!string.IsNullOrEmpty(name) && !order.ContainsKey(name))
                {
                    order.Add(name, i);
                }
            }

            entries.Sort((a, b) =>
            {
                bool hasA = order.TryGetValue(a.Name, out int orderA);
                bool hasB = order.TryGetValue(b.Name, out int orderB);
                return hasA && hasB ? orderA.CompareTo(orderB) : hasA ? -1 : hasB ? 1 : string.CompareOrdinal(a.Name, b.Name);
            });

            return entries;
        }

        /// <summary>
        /// Parses a single frame <paramref name="entry"/> and appends its data to the output <paramref name="atlas"/>.
        /// </summary>
        /// <param name="entry">Frame entry to parse.</param>
        /// <param name="atlas">Output atlas receiving parsed frame data.</param>
        /// <param name="rectSizes">List tracking parsed rectangle sizes for later offset normalization.</param>
        private static void ParseFrame(FrameEntry entry, ParsedTexturePackerAtlas atlas, List<(float w, float h)> rectSizes)
        {
            if (!entry.Data.TryGetProperty("frame", out JsonElement frameElement) || frameElement.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidDataException($"TexturePacker frame \"{entry.Name}\" is missing frame coordinates.");
            }

            float x = ReadFloat(frameElement, "x");
            float y = ReadFloat(frameElement, "y");
            float width = ReadFloat(frameElement, "w");
            float height = ReadFloat(frameElement, "h");
            CTRRectangle rect = new(x, y, width, height);
            atlas.Rects.Add(rect);
            rectSizes.Add((rect.w, rect.h));

            bool rotated = entry.Data.TryGetProperty("rotated", out JsonElement rotatedElement) && rotatedElement.ValueKind == JsonValueKind.True;
            if (rotated)
            {
                Console.WriteLine($"TexturePacker frame \"{entry.Name}\" is rotated — rotation is not supported.");
            }

            Vector offset = new(0f, 0f);
            if (entry.Data.TryGetProperty("spriteSourceSize", out JsonElement offsetElement) && offsetElement.ValueKind == JsonValueKind.Object)
            {
                float offsetX = ReadFloat(offsetElement, "x");
                float offsetY = ReadFloat(offsetElement, "y");
                if (offsetX != 0f || offsetY != 0f)
                {
                    atlas.HasNonZeroOffset = true;
                }
                offset = new Vector(offsetX, offsetY);
            }
            atlas.Offsets.Add(offset);

            if (entry.Data.TryGetProperty("sourceSize", out JsonElement sourceSize) && sourceSize.ValueKind == JsonValueKind.Object)
            {
                float sourceWidth = ReadFloat(sourceSize, "w");
                float sourceHeight = ReadFloat(sourceSize, "h");
                atlas.SourceSizes.Add(new Vector(sourceWidth, sourceHeight));
                if (sourceWidth > atlas.PreCutWidth)
                {
                    atlas.PreCutWidth = sourceWidth;
                }
                if (sourceHeight > atlas.PreCutHeight)
                {
                    atlas.PreCutHeight = sourceHeight;
                }
            }
            else
            {
                atlas.SourceSizes.Add(new Vector(rect.w, rect.h));
            }
        }

        /// <summary>
        /// Replaces parsed offsets with centered trim offsets derived from source size and trimmed rectangle size.
        /// </summary>
        /// <param name="atlas">Atlas whose offsets should be updated.</param>
        /// <param name="rectSizes">Trimmed rectangle sizes corresponding to atlas frames.</param>
        private static void ApplyCenteredOffsets(ParsedTexturePackerAtlas atlas, List<(float w, float h)> rectSizes)
        {
            if (atlas.Rects.Count == 0 || rectSizes.Count != atlas.Rects.Count)
            {
                return;
            }

            bool hasOffset = false;
            for (int i = 0; i < atlas.Rects.Count; i++)
            {
                float referenceWidth = i < atlas.SourceSizes.Count && atlas.SourceSizes[i].X > 0f ? atlas.SourceSizes[i].X : rectSizes[i].w;
                float referenceHeight = i < atlas.SourceSizes.Count && atlas.SourceSizes[i].Y > 0f ? atlas.SourceSizes[i].Y : rectSizes[i].h;
                float offsetX = MathF.Round((referenceWidth - rectSizes[i].w) / 2f);
                float offsetY = MathF.Round((referenceHeight - rectSizes[i].h) / 2f);
                atlas.Offsets[i] = new Vector(offsetX, offsetY);
                if (offsetX != 0f || offsetY != 0f)
                {
                    hasOffset = true;
                }
            }

            atlas.HasNonZeroOffset = hasOffset;
        }

        /// <summary>
        /// Attempts to read a string property from a JSON object <paramref name="element"/>.
        /// </summary>
        /// <param name="element">JSON object element to inspect.</param>
        /// <param name="propertyName">Property name to read.</param>
        /// <returns>String property value, or <see langword="null" /> when missing or not a string.</returns>
        private static string TryGetString(JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out JsonElement value) && value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;
        }

        /// <summary>
        /// Reads a numeric property from a JSON object <paramref name="element"/> as a float.
        /// </summary>
        /// <param name="element">JSON object element to inspect.</param>
        /// <param name="propertyName">Property name to read.</param>
        /// <returns>Numeric property value, or <c>0</c> when missing or not numeric.</returns>
        private static float ReadFloat(JsonElement element, string propertyName)
        {
            return !element.TryGetProperty(propertyName, out JsonElement value) || value.ValueKind != JsonValueKind.Number
                ? 0f
                : (float)value.GetDouble();
        }

        /// <summary>
        /// Lightweight pairing of a frame <paramref name="name"/> with its JSON <paramref name="data"/> block.
        /// </summary>
        /// <param name="name">Frame name.</param>
        /// <param name="data">JSON data for the frame.</param>
        private readonly struct FrameEntry(string name, JsonElement data)
        {
            /// <summary>
            /// Gets the frame name.
            /// </summary>
            public string Name { get; } = name;

            /// <summary>
            /// Gets the JSON data block for the frame.
            /// </summary>
            public JsonElement Data { get; } = data;
        }
    }
}
