using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;

using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Helpers;

using Microsoft.Xna.Framework;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Immutable pack description using string resource names.
    /// </summary>
    internal sealed class PackDefinition(
        int unlockStars,
        int levelCount,
        int saveSlot,
        string packSpritesheet,
        int packQuadIndex,
        string[] boxBackgrounds,
        int boxBackgroundP2Y,
        int sittingPlatform,
        string[] boxCovers,
        RGBAColor boxHoleBgColor,
        string[] musicPack,
        string[] musicList,
        bool earthBg,
        Vector? earthBgPosition,
        string boxLabelText,
        string packName,
        RGBAColor? ghostGrabColor
    )
    {
        /// <summary>Number of stars required to unlock this pack.</summary>
        public int UnlockStars { get; } = unlockStars;

        /// <summary>Resource ID for the spritesheet containing this pack's box sprite.</summary>
        public string PackSpritesheet { get; } = packSpritesheet;

        /// <summary>Quad index within <see cref="PackSpritesheet"/> for this pack's box sprite.</summary>
        public int PackQuadIndex { get; } = packQuadIndex;

        /// <summary>The localized box pack name.</summary>
        public string PackName { get; } = packName;

        /// <summary>String resource names for pack assets.</summary>
        public string[] BoxBackgrounds { get; } = boxBackgrounds;

        /// <summary>Y position for secondary background (p2) in long levels. 0 means no p2.</summary>
        public int BoxBackgroundP2Y { get; } = boxBackgroundP2Y;

        /// <summary>Quad index in <see cref="Resources.Img.CharSupports"/> used for the support platform.</summary>
        public int SittingPlatform { get; } = sittingPlatform;

        /// <summary>String resource names for cover assets.</summary>
        public string[] BoxCovers { get; } = boxCovers;

        /// <summary>Box background color for pack selection menu.</summary>
        public RGBAColor BoxHoleBgColor { get; } = boxHoleBgColor;

        /// <summary>String resource names for the music to play in this pack.</summary>
        public string[] MusicPack { get; } = musicPack;

        /// <summary>String resource names for the music to play in this pack.</summary>
        public string[] MusicList { get; } = musicList;

        /// <summary>Total number of levels in the pack.</summary>
        public int LevelCount { get; } = levelCount;

        /// <summary>Save slot index used to route this pack's progress file.</summary>
        public int SaveSlot { get; } = saveSlot;

        /// <summary>Whether this pack uses earth background animations.</summary>
        public bool EarthBg { get; } = earthBg;

        /// <summary>Position for earth background animation (null uses default).</summary>
        public Vector? EarthBgPosition { get; } = earthBgPosition;

        /// <summary>Localization key for optional box label text (e.g., "the hardest one").</summary>
        public string BoxLabelText { get; } = boxLabelText;

        /// <summary>Optional ghost grab circle color override. When null, the default color is used.</summary>
        public RGBAColor? GhostGrabColor { get; } = ghostGrabColor;
    }

    /// <summary>
    /// Loads pack metadata from JSON packs files and save routing from <c>packlist.json</c>.
    /// </summary>
    internal static class PackConfig
    {
        private readonly record struct PackListEntry(string ConfigFileName, int SaveSlot);

        /// <summary>
        /// The configuration file for original <em>Cut the Rope</em> game.
        /// </summary>
        private const string DefaultPacksConfigFile = "ctroriginal_packs.json";

        /// <summary>
        /// The master configuration list for game pack.
        /// </summary>
        private const string PackListConfigFile = "packlist.json";
        private static readonly string[] EmptyResourceNames = [null];

        /// <summary>Default box color when not specified in pack config (dark gray: 45, 45, 53).</summary>
        private static readonly RGBAColor DefaultBoxHoleBgColor = RGBAColor.MakeRGBA(45 / 255f, 45 / 255f, 53 / 255f, 1f);

        private static readonly List<PackDefinition> packs;
        private static readonly int playablePackCount;

        /// <summary>Video filename for the intro movie (without extension), or null to skip.</summary>
        public static string IntroVideo { get; private set; }

        /// <summary>Video filename for the outro/completion movie (without extension), or null to skip.</summary>
        public static string OutroVideo { get; private set; }

        static PackConfig()
        {
            List<PackListEntry> packListEntries = LoadPackListEntries();
            packs = LoadPacksFromEntries(packListEntries);
            playablePackCount = packs.Count(p => p.LevelCount > 0);
            MaxLevelsPerPack = packs.Count > 0 ? packs.Max(p => p.LevelCount) : 0;
        }

        public static IReadOnlyList<PackDefinition> Packs => packs;

        public static int MaxLevelsPerPack { get; }

        public static int GetPackCount()
        {
            return playablePackCount;
        }

        public static int GetLevelCount(int pack)
        {
            return pack >= 0 && pack < packs.Count ? packs[pack].LevelCount : 0;
        }

        public static int GetSaveSlot(int pack)
        {
            return pack >= 0 && pack < packs.Count ? packs[pack].SaveSlot : 0;
        }

        public static string[] GetBoxBackgrounds(int pack)
        {
            return pack >= 0 && pack < packs.Count ? packs[pack].BoxBackgrounds : EmptyResourceNames;
        }

        public static int GetBoxBackgroundP2Y(int pack)
        {
            return pack >= 0 && pack < packs.Count ? packs[pack].BoxBackgroundP2Y : 0;
        }

        public static string[] GetBoxCovers(int pack)
        {
            return pack >= 0 && pack < packs.Count ? packs[pack].BoxCovers : EmptyResourceNames;
        }

        /// <summary>
        /// Returns the first available cover resource name for a pack.
        /// </summary>
        /// <param name="pack">Target pack index.</param>
        public static string GetBoxCoverOrDefault(int pack)
        {
            string coverResourceName = GetBoxCovers(pack).FirstOrDefault(name => !string.IsNullOrWhiteSpace(name));

            return string.IsNullOrWhiteSpace(coverResourceName)
                ? throw new InvalidDataException($"pack config is missing boxCover for pack {pack}.")
                : coverResourceName;
        }

        public static int GetSittingPlatform(int pack)
        {
            return pack >= 0 && pack < packs.Count ? packs[pack].SittingPlatform : 0;
        }

        public static string[] GetMusicPack(int pack)
        {
            return pack >= 0 && pack < packs.Count ? packs[pack].MusicPack : EmptyResourceNames;
        }

        public static string GetMusicPackOrDefault(int pack)
        {
            string[] musicPack = GetMusicPack(pack);
            return musicPack.FirstOrDefault(name => !string.IsNullOrWhiteSpace(name));
        }

        public static string[] GetMusicList(int pack)
        {
            return pack >= 0 && pack < packs.Count ? packs[pack].MusicList : EmptyResourceNames;
        }

        public static string[] GetMusicListOrDefault(int pack)
        {
            string[] musicList = GetMusicList(pack);
            return [.. musicList.Where(name => !string.IsNullOrWhiteSpace(name))];
        }

        public static int GetUnlockStars(int pack)
        {
            return pack >= 0 && pack < packs.Count ? packs[pack].UnlockStars : 0;
        }

        public static bool GetEarthBg(int pack)
        {
            return pack >= 0 && pack < packs.Count && packs[pack].EarthBg;
        }

        public static Vector? GetEarthBgPosition(int pack)
        {
            return pack >= 0 && pack < packs.Count ? packs[pack].EarthBgPosition : null;
        }

        public static RGBAColor? GetGhostGrabColor(int pack)
        {
            return pack >= 0 && pack < packs.Count ? packs[pack].GhostGrabColor : null;
        }

        public static RGBAColor GetBoxHoleBgColor(int pack)
        {
            return pack >= 0 && pack < packs.Count ? packs[pack].BoxHoleBgColor : DefaultBoxHoleBgColor;
        }

        public static string GetBoxLabelText(int pack)
        {
            return pack >= 0 && pack < packs.Count ? packs[pack].BoxLabelText : null;
        }

        public static string GetPackName(int pack)
        {
            return pack >= 0 && pack < packs.Count ? packs[pack].PackName : null;
        }

        public static string GetPackSpritesheet(int pack)
        {
            return pack >= 0 && pack < packs.Count ? packs[pack].PackSpritesheet : null;
        }

        public static int GetPackQuadIndex(int pack)
        {
            return pack >= 0 && pack < packs.Count ? packs[pack].PackQuadIndex : 0;
        }

        /// <summary>
        /// Returns the index of the first non-playable pack entry (coming soon placeholder), or -1 if none.
        /// </summary>
        public static int GetComingSoonPackIndex()
        {
            for (int i = packs.Count - 1; i >= 0; i--)
            {
                if (packs[i].LevelCount == 0)
                {
                    return i;
                }
            }

            return -1;
        }

        private static List<PackListEntry> LoadPackListEntries()
        {
            if (!TryLoadJsonRoot(PackListConfigFile, out JsonElement root))
            {
                return [new PackListEntry(DefaultPacksConfigFile, 0)];
            }

            List<PackListEntry> entries = [];

            switch (root.ValueKind)
            {
                case JsonValueKind.Array:
                    foreach (JsonElement item in root.EnumerateArray())
                    {
                        AddPackListEntry(item, entries);
                    }
                    break;
                case JsonValueKind.Object:
                    if (root.TryGetProperty("entries", out JsonElement entriesArray) && entriesArray.ValueKind == JsonValueKind.Array)
                    {
                        foreach (JsonElement item in entriesArray.EnumerateArray())
                        {
                            AddPackListEntry(item, entries);
                        }
                    }
                    else
                    {
                        AddPackListEntry(root, entries);
                    }
                    break;
                case JsonValueKind.Undefined:
                case JsonValueKind.String:
                case JsonValueKind.Number:
                case JsonValueKind.True:
                case JsonValueKind.False:
                case JsonValueKind.Null:
                default:
                    throw new InvalidDataException($"{PackListConfigFile} root must be an object or array.");
            }

            if (entries.Count == 0)
            {
                entries.Add(new PackListEntry(DefaultPacksConfigFile, 0));
            }

            return entries;
        }

        private static void AddPackListEntry(JsonElement entryElement, ICollection<PackListEntry> entries)
        {
            if (entryElement.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidDataException($"{PackListConfigFile} entry must be an object.");
            }

            string configName = ParseRequiredString(entryElement, "boxDataName", PackListConfigFile);
            int saveSlot = ParseIntProperty(entryElement, "saveSlot", 0, PackListConfigFile);
            if (saveSlot < 0)
            {
                throw new InvalidDataException(
                    $"{PackListConfigFile} entry '{configName}' has invalid saveSlot '{saveSlot}'. saveSlot must be >= 0."
                );
            }

            entries.Add(new PackListEntry(NormalizePacksConfigFileName(configName), saveSlot));

            IntroVideo ??= ParseStringProperty(entryElement, "introVideo");
            OutroVideo ??= ParseStringProperty(entryElement, "outroVideo");
        }

        private static List<PackDefinition> LoadPacksFromEntries(IEnumerable<PackListEntry> packListEntries)
        {
            List<PackDefinition> results = [];

            foreach (PackListEntry packListEntry in packListEntries)
            {
                if (!TryLoadJsonRoot(packListEntry.ConfigFileName, out JsonElement root))
                {
                    throw new InvalidDataException($"Failed to load pack config file '{packListEntry.ConfigFileName}'.");
                }

                JsonElement packsArray = ResolvePacksArray(root, packListEntry.ConfigFileName);

                foreach (JsonElement packElement in packsArray.EnumerateArray())
                {
                    if (packElement.ValueKind != JsonValueKind.Object)
                    {
                        throw new InvalidDataException($"{packListEntry.ConfigFileName} contains a non-object pack entry.");
                    }

                    int unlockStars = ParseIntProperty(packElement, "unlockStars", 0, packListEntry.ConfigFileName);
                    int levelCount = ParseIntProperty(packElement, "levelCount", 0, packListEntry.ConfigFileName);
                    bool isPlayable = levelCount > 0;

                    string packSpritesheetRaw = ParseStringProperty(packElement, "packSpritesheet");
                    string packSpritesheet = ResolvePackSpritesheetId(packSpritesheetRaw);
                    int packQuadIndex = ParseIntProperty(packElement, "packQuadIndex", 0, packListEntry.ConfigFileName);

                    string[] boxBackgrounds = ParseResourceNames(packElement, "boxBackground");
                    if (isPlayable)
                    {
                        RequireResourceNames(boxBackgrounds, "boxBackground", packListEntry.ConfigFileName);
                    }
                    ValidateResourceNames(boxBackgrounds, "boxBackground", packListEntry.ConfigFileName);

                    int boxBackgroundP2Y = ParseIntProperty(packElement, "boxBackgroundP2Y", 0, packListEntry.ConfigFileName);

                    int sittingPlatform = ParseIntProperty(packElement, "sittingPlatform", 0, packListEntry.ConfigFileName);

                    string[] boxCovers = ParseResourceNames(packElement, "boxCover");
                    if (isPlayable)
                    {
                        RequireResourceNames(boxCovers, "boxCover", packListEntry.ConfigFileName);
                    }
                    ValidateResourceNames(boxCovers, "boxCover", packListEntry.ConfigFileName);

                    RGBAColor boxHoleBgColor = ParseColorProperty(packElement, "boxHoleBgColor");

                    string[] musicPack = ParseResourceNames(packElement, "musicPack");

                    string[] musicList = ParseResourceNames(packElement, "musicList");
                    ValidateResourceNames(musicList, "musicList", packListEntry.ConfigFileName);

                    bool earthBg = ParseBoolProperty(packElement, "earthBg", false, packListEntry.ConfigFileName);

                    Vector? earthBgPosition = ParseVectorProperty(packElement, "earthBgPosition", packListEntry.ConfigFileName);

                    string boxLabelText = ParseStringProperty(packElement, "boxLabelText");

                    string packName = ParseStringProperty(packElement, "packName");

                    RGBAColor? ghostGrabColor = ParseNullableColorProperty(packElement, "ghostGrabColor");

                    results.Add(
                        new PackDefinition(
                            unlockStars,
                            levelCount,
                            packListEntry.SaveSlot,
                            packSpritesheet,
                            packQuadIndex,
                            boxBackgrounds,
                            boxBackgroundP2Y,
                            sittingPlatform,
                            boxCovers,
                            boxHoleBgColor,
                            musicPack,
                            musicList,
                            earthBg,
                            earthBgPosition,
                            boxLabelText,
                            packName,
                            ghostGrabColor
                            )
                        );
                }
            }

            return results;
        }

        private static JsonElement ResolvePacksArray(JsonElement root, string configFileName)
        {
            return root.ValueKind == JsonValueKind.Array
                ? root
                : root.ValueKind == JsonValueKind.Object &&
                root.TryGetProperty("packs", out JsonElement packsProperty) &&
                packsProperty.ValueKind == JsonValueKind.Array
                ? packsProperty
                : throw new InvalidDataException($"{configFileName} root must be a packs array or object with 'packs'.");
        }

        /// <summary>
        /// Maps a shorthand spritesheet ID (e.g. "1", "2") to its full resource name.
        /// Falls back to <see cref="Resources.Img.MenuPackSelection"/> for unrecognized or empty values.
        /// </summary>
        private static string ResolvePackSpritesheetId(string id)
        {
            return id switch
            {
                "1" => Resources.Img.MenuPackSelection,
                "2" => Resources.Img.MenuPackSelection2,
                _ => Resources.Img.MenuPackSelection,
            };
        }

        private static string NormalizePacksConfigFileName(string packsConfigName)
        {
            if (string.IsNullOrWhiteSpace(packsConfigName))
            {
                return DefaultPacksConfigFile;
            }

            string normalized = packsConfigName.Trim();
            return normalized.EndsWith(".json", StringComparison.OrdinalIgnoreCase) ? normalized : $"{normalized}.json";
        }

        private static bool TryLoadJsonRoot(string fileName, out JsonElement root)
        {
            root = default;

            if (string.IsNullOrWhiteSpace(fileName))
            {
                return false;
            }

            try
            {
                using Stream stream = TitleContainer.OpenStream(Path.Combine(ContentPaths.RootDirectory, fileName));
                using JsonDocument document = JsonDocument.Parse(stream);
                root = document.RootElement.Clone();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static int ParseIntProperty(JsonElement element, string propertyName, int defaultValue, string fileName)
        {
            if (!element.TryGetProperty(propertyName, out JsonElement value))
            {
                return defaultValue;
            }

            switch (value.ValueKind)
            {
                case JsonValueKind.Number:
                    if (value.TryGetInt32(out int intValue))
                    {
                        return intValue;
                    }

                    if (value.TryGetInt64(out long longValue))
                    {
                        return (int)longValue;
                    }
                    break;
                case JsonValueKind.String:
                    string strValue = value.GetString();
                    if (!string.IsNullOrWhiteSpace(strValue) && int.TryParse(strValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedInt))
                    {
                        return parsedInt;
                    }
                    break;
                case JsonValueKind.Undefined:
                case JsonValueKind.Object:
                case JsonValueKind.Array:
                case JsonValueKind.True:
                case JsonValueKind.False:
                case JsonValueKind.Null:
                default:
                    throw new InvalidDataException($"{fileName} has invalid integer value for '{propertyName}'.");
            }

            throw new InvalidDataException($"{fileName} has invalid integer value for '{propertyName}'.");
        }

        private static bool ParseBoolProperty(
            JsonElement element,
            string propertyName,
            bool defaultValue,
            string fileName
        )
        {
            bool ThrowInvalid()
            {
                throw new InvalidDataException(
                    $"{fileName} has invalid boolean value for '{propertyName}'."
                );
            }

            return !element.TryGetProperty(propertyName, out JsonElement value)
                ? defaultValue
                : value.ValueKind switch
                {
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,

                    JsonValueKind.String
                        when bool.TryParse(value.GetString(), out bool parsed)
                            => parsed,

                    JsonValueKind.Undefined or
                    JsonValueKind.Object or
                    JsonValueKind.Array or
                    JsonValueKind.Number or
                    JsonValueKind.Null or
                    JsonValueKind.String
                        => ThrowInvalid(),
                    _ => ThrowInvalid(),
                };
        }

        private static string ParseRequiredString(JsonElement element, string propertyName, string fileName)
        {
            string value = ParseStringProperty(element, propertyName);
            return string.IsNullOrWhiteSpace(value)
                ? throw new InvalidDataException($"{fileName} is missing required property '{propertyName}'.")
                : value;
        }

        private static string ParseStringProperty(JsonElement element, string propertyName)
        {
            return !element.TryGetProperty(propertyName, out JsonElement value) || value.ValueKind == JsonValueKind.Null
                ? null
                : value.ValueKind == JsonValueKind.String
                ? value.GetString()?.Trim()
                : null;
        }

        private static string[] ParseResourceNames(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out JsonElement value) || value.ValueKind == JsonValueKind.Null)
            {
                return EmptyResourceNames;
            }

            List<string> names = [];

            switch (value.ValueKind)
            {
                case JsonValueKind.String:
                    names.AddRange(value.GetString()?.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(part => part.Trim()) ?? []);
                    break;
                case JsonValueKind.Array:
                    foreach (JsonElement item in value.EnumerateArray())
                    {
                        if (item.ValueKind == JsonValueKind.String)
                        {
                            string name = item.GetString()?.Trim();
                            if (!string.IsNullOrWhiteSpace(name))
                            {
                                names.Add(name);
                            }
                        }
                    }
                    break;
                case JsonValueKind.Undefined:
                case JsonValueKind.Object:
                case JsonValueKind.Number:
                case JsonValueKind.True:
                case JsonValueKind.False:
                case JsonValueKind.Null:
                default:
                    break;
            }

            names.Add(null);
            return [.. names];
        }

        private static Vector? ParseVectorProperty(JsonElement element, string propertyName, string fileName)
        {
            if (!element.TryGetProperty(propertyName, out JsonElement value) || value.ValueKind == JsonValueKind.Null)
            {
                return null;
            }

            switch (value.ValueKind)
            {
                case JsonValueKind.String:
                    string[] parts = value.GetString()?.Split(',') ?? [];
                    if (parts.Length >= 2)
                    {
                        float x = float.Parse(parts[0].Trim(), CultureInfo.InvariantCulture);
                        float y = float.Parse(parts[1].Trim(), CultureInfo.InvariantCulture);
                        return new Vector(x, y);
                    }
                    break;
                case JsonValueKind.Array:
                    float[] coords = [.. value.EnumerateArray().Select(item => item.GetSingle())];
                    if (coords.Length >= 2)
                    {
                        return new Vector(coords[0], coords[1]);
                    }
                    break;
                case JsonValueKind.Undefined:
                case JsonValueKind.Object:
                case JsonValueKind.Number:
                case JsonValueKind.True:
                case JsonValueKind.False:
                case JsonValueKind.Null:
                default:
                    throw new InvalidDataException($"{fileName} has invalid vector value for '{propertyName}'.");
            }

            throw new InvalidDataException($"{fileName} has invalid vector value for '{propertyName}'.");
        }

        private static RGBAColor? ParseNullableColorProperty(JsonElement element, string propertyName)
        {
            return !element.TryGetProperty(propertyName, out JsonElement value) || value.ValueKind == JsonValueKind.Null
                ? null
                : ParseColorProperty(element, propertyName);
        }

        private static RGBAColor ParseColorProperty(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out JsonElement value) || value.ValueKind == JsonValueKind.Null)
            {
                return DefaultBoxHoleBgColor;
            }

            switch (value.ValueKind)
            {
                case JsonValueKind.String:
                    string[] parts = value.GetString()?.Split(',') ?? [];
                    return ParseColorParts(parts);
                case JsonValueKind.Array:
                    List<string> arrayParts = [];
                    foreach (JsonElement item in value.EnumerateArray())
                    {
                        arrayParts.Add(item.ToString());
                    }
                    return ParseColorParts([.. arrayParts]);
                case JsonValueKind.Undefined:
                case JsonValueKind.Object:
                case JsonValueKind.Number:
                case JsonValueKind.True:
                case JsonValueKind.False:
                case JsonValueKind.Null:
                default:
                    return DefaultBoxHoleBgColor;
            }
        }

        private static RGBAColor ParseColorParts(string[] parts)
        {
            if (parts.Length < 3)
            {
                return DefaultBoxHoleBgColor;
            }

            float r = int.Parse(parts[0].Trim(), CultureInfo.InvariantCulture) / 255f;
            float g = int.Parse(parts[1].Trim(), CultureInfo.InvariantCulture) / 255f;
            float b = int.Parse(parts[2].Trim(), CultureInfo.InvariantCulture) / 255f;
            float a = parts.Length >= 4 ? int.Parse(parts[3].Trim(), CultureInfo.InvariantCulture) / 255f : 1f;
            return RGBAColor.MakeRGBA(r, g, b, a);
        }

        private static void ValidateResourceNames(IEnumerable<string> resourceNames, string context, string packsConfigFile)
        {
            foreach (string resourceName in resourceNames)
            {
                if (resourceName == null)
                {
                    continue; // Preserve sentinel semantics
                }

                ValidateResourceName(resourceName, context, packsConfigFile);
            }
        }

        private static void RequireResourceNames(string[] resourceNames, string context, string packsConfigFile)
        {
            if (resourceNames.Length == 0 || string.IsNullOrWhiteSpace(resourceNames[0]))
            {
                throw new InvalidDataException($"{packsConfigFile} is missing required {context}.");
            }
        }

        private static void ValidateResourceName(string resourceName, string context, string packsConfigFile)
        {
            if (!Resources.IsValidResourceName(resourceName))
            {
                throw new InvalidDataException($"{packsConfigFile} contains unknown resource name '{resourceName}' in '{context}'.");
            }
        }
    }
}
