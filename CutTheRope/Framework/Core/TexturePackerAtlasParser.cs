using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace CutTheRope.Framework.Core
{
    internal sealed class ParsedTexturePackerAtlas
    {
        public List<CTRRectangle> Rects { get; } = [];

        public List<Vector> Offsets { get; } = [];

        public bool HasNonZeroOffset { get; set; }

        public float PreCutWidth { get; set; }

        public float PreCutHeight { get; set; }
    }

    internal sealed class TexturePackerParserOptions
    {
        public IReadOnlyList<string> FrameOrder { get; init; }

        public bool NormalizeOffsetsToCenter { get; init; }
    }

    internal static class TexturePackerAtlasParser
    {
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
                if (sourceWidth > atlas.PreCutWidth)
                {
                    atlas.PreCutWidth = sourceWidth;
                }
                if (sourceHeight > atlas.PreCutHeight)
                {
                    atlas.PreCutHeight = sourceHeight;
                }
            }
        }

        private static void ApplyCenteredOffsets(ParsedTexturePackerAtlas atlas, List<(float w, float h)> rectSizes)
        {
            if (atlas.Rects.Count == 0 || rectSizes.Count != atlas.Rects.Count)
            {
                return;
            }

            bool hasOffset = false;
            for (int i = 0; i < atlas.Rects.Count; i++)
            {
                float referenceWidth = atlas.PreCutWidth > 0f ? atlas.PreCutWidth : rectSizes[i].w;
                float referenceHeight = atlas.PreCutHeight > 0f ? atlas.PreCutHeight : rectSizes[i].h;
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

        private static string TryGetString(JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out JsonElement value) && value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;
        }

        private static float ReadFloat(JsonElement element, string propertyName)
        {
            return !element.TryGetProperty(propertyName, out JsonElement value) || value.ValueKind != JsonValueKind.Number
                ? 0f
                : (float)value.GetDouble();
        }

        private readonly struct FrameEntry(string name, JsonElement data)
        {
            public string Name { get; } = name;

            public JsonElement Data { get; } = data;
        }
    }
}
