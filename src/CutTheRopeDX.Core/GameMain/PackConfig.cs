using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Helpers;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Immutable pack description using string resource names.
    /// </summary>
    /// <param name="unlockStars">Number of stars required to unlock this pack.</param>
    /// <param name="levelCount">Total number of levels in the pack.</param>
    /// <param name="saveSlot">Save slot index used to route this pack's progress file.</param>
    /// <param name="packSpritesheet">Resource name for the spritesheet containing this pack's box sprite.</param>
    /// <param name="packQuadIndex">Quad index within <paramref name="packSpritesheet"/> for this pack's box sprite.</param>
    /// <param name="boxBackgrounds">Resource names for the pack background assets.</param>
    /// <param name="boxBackgroundP2Y">Y position for the secondary background in long levels, or 0 when unused.</param>
    /// <param name="sittingPlatform">Quad index used for the support platform.</param>
    /// <param name="boxCovers">Resource names for the pack cover assets.</param>
    /// <param name="boxHoleBgColor">Background color used behind the box hole in the pack selection menu.</param>
    /// <param name="musicPack">Resource names for pack-specific music.</param>
    /// <param name="musicList">Resource names for level music in this pack.</param>
    /// <param name="earthBg">Whether this pack uses earth background animations.</param>
    /// <param name="earthBgPosition">Position override for earth background animation, or <see langword="null"/> to use the default.</param>
    /// <param name="boxLabelText">Localization key for optional box label text.</param>
    /// <param name="packName">Localized box pack name.</param>
    /// <param name="ghostGrabColor">Optional ghost grab circle color override, or <see langword="null"/> to use the default.</param>
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

        /// <summary>Position for earth background animation, or <see langword="null"/> to use the default.</summary>
        public Vector? EarthBgPosition { get; } = earthBgPosition;

        /// <summary>Localization key for optional box label text (e.g., "the hardest one").</summary>
        public string BoxLabelText { get; } = boxLabelText;

        /// <summary>Optional ghost grab circle color override, or <see langword="null"/> to use the default.</summary>
        public RGBAColor? GhostGrabColor { get; } = ghostGrabColor;
    }

    /// <summary>
    /// Loads pack metadata from JSON packs files and save routing from <c>packlist.json</c>.
    /// </summary>
    internal static class PackConfig
    {
        /// <summary>
        /// Entry from the pack list configuration that identifies a pack config file and save slot.
        /// </summary>
        /// <param name="ConfigFileName">Pack configuration file name.</param>
        /// <param name="SaveSlot">Save slot index used by packs loaded from the configuration file.</param>
        private readonly record struct PackListEntry(string ConfigFileName, int SaveSlot);

        /// <summary>
        /// The configuration file for original <em>Cut the Rope</em> game.
        /// </summary>
        private const string DefaultPacksConfigFile = "ctroriginal_packs.json";

        /// <summary>
        /// The master configuration list for game pack.
        /// </summary>
        private const string PackListConfigFile = "packlist.json";

        /// <summary>Sentinel resource list used when a pack has no resource names.</summary>
        private static readonly string[] EmptyResourceNames = [null];

        /// <summary>Default box color when not specified in pack config (dark gray: 45, 45, 53).</summary>
        private static readonly RGBAColor DefaultBoxHoleBgColor = RGBAColor.MakeRGBA(45 / 255f, 45 / 255f, 53 / 255f, 1f);

        /// <summary>Loaded pack definitions in display order.</summary>
        private static readonly List<PackDefinition> packs;

        /// <summary>Number of packs that contain playable levels.</summary>
        private static readonly int playablePackCount;

        /// <summary>Video filename for the intro movie without extension, or <see langword="null"/> to skip.</summary>
        public static string IntroVideo { get; private set; }

        /// <summary>Video filename for the outro/completion movie without extension, or <see langword="null"/> to skip.</summary>
        public static string OutroVideo { get; private set; }

        /// <summary>
        /// Initializes pack definitions and derived pack counts from configuration files.
        /// </summary>
        static PackConfig()
        {
            List<PackListEntry> packListEntries = LoadPackListEntries();
            packs = LoadPacksFromEntries(packListEntries);
            playablePackCount = packs.Count(p => p.LevelCount > 0);
            MaxLevelsPerPack = packs.Count > 0 ? packs.Max(p => p.LevelCount) : 0;
        }

        /// <summary>
        /// Gets all loaded pack definitions, including non-playable placeholder packs.
        /// </summary>
        public static IReadOnlyList<PackDefinition> Packs => packs;

        /// <summary>
        /// Gets the largest level count across all loaded packs.
        /// </summary>
        public static int MaxLevelsPerPack { get; }

        /// <summary>
        /// Gets the number of packs that contain playable levels.
        /// </summary>
        /// <returns>The playable pack count.</returns>
        public static int GetPackCount()
        {
            return playablePackCount;
        }

        /// <summary>
        /// Gets the level count for a pack.
        /// </summary>
        /// <param name="pack">Target pack index.</param>
        /// <returns>The pack's level count, or 0 when <paramref name="pack"/> is out of range.</returns>
        public static int GetLevelCount(int pack)
        {
            return pack >= 0 && pack < packs.Count ? packs[pack].LevelCount : 0;
        }

        /// <summary>
        /// Gets the save slot used by a pack.
        /// </summary>
        /// <param name="pack">Target pack index.</param>
        /// <returns>The pack's save slot, or 0 when <paramref name="pack"/> is out of range.</returns>
        public static int GetSaveSlot(int pack)
        {
            return pack >= 0 && pack < packs.Count ? packs[pack].SaveSlot : 0;
        }

        /// <summary>
        /// Gets the background resource names for a pack.
        /// </summary>
        /// <param name="pack">Target pack index.</param>
        /// <returns>The pack background resource names, or the empty resource sentinel when <paramref name="pack"/> is out of range.</returns>
        public static string[] GetBoxBackgrounds(int pack)
        {
            return pack >= 0 && pack < packs.Count ? packs[pack].BoxBackgrounds : EmptyResourceNames;
        }

        /// <summary>
        /// Gets the secondary background Y position for a pack.
        /// </summary>
        /// <param name="pack">Target pack index.</param>
        /// <returns>The secondary background Y position, or 0 when unused or when <paramref name="pack"/> is out of range.</returns>
        public static int GetBoxBackgroundP2Y(int pack)
        {
            return pack >= 0 && pack < packs.Count ? packs[pack].BoxBackgroundP2Y : 0;
        }

        /// <summary>
        /// Gets the cover resource names for a pack.
        /// </summary>
        /// <param name="pack">Target pack index.</param>
        /// <returns>The pack cover resource names, or the empty resource sentinel when <paramref name="pack"/> is out of range.</returns>
        public static string[] GetBoxCovers(int pack)
        {
            return pack >= 0 && pack < packs.Count ? packs[pack].BoxCovers : EmptyResourceNames;
        }

        /// <summary>
        /// Returns the first available cover resource name for a pack.
        /// </summary>
        /// <param name="pack">Target pack index.</param>
        /// <returns>The first non-empty cover resource name.</returns>
        public static string GetBoxCoverOrDefault(int pack)
        {
            string coverResourceName = GetBoxCovers(pack).FirstOrDefault(name => !string.IsNullOrWhiteSpace(name));

            return string.IsNullOrWhiteSpace(coverResourceName)
                ? throw new InvalidDataException($"pack config is missing boxCover for pack {pack}.")
                : coverResourceName;
        }

        /// <summary>
        /// Gets the support platform quad index for a pack.
        /// </summary>
        /// <param name="pack">Target pack index.</param>
        /// <returns>The support platform quad index, or 0 when <paramref name="pack"/> is out of range.</returns>
        public static int GetSittingPlatform(int pack)
        {
            return pack >= 0 && pack < packs.Count ? packs[pack].SittingPlatform : 0;
        }

        /// <summary>
        /// Gets the pack-specific music resource names for a pack.
        /// </summary>
        /// <param name="pack">Target pack index.</param>
        /// <returns>The pack music resource names, or the empty resource sentinel when <paramref name="pack"/> is out of range.</returns>
        public static string[] GetMusicPack(int pack)
        {
            return pack >= 0 && pack < packs.Count ? packs[pack].MusicPack : EmptyResourceNames;
        }

        /// <summary>
        /// Gets the first available pack-specific music resource name for a pack.
        /// </summary>
        /// <param name="pack">Target pack index.</param>
        /// <returns>The first non-empty pack music resource name, or <see langword="null"/> when none exists.</returns>
        public static string GetMusicPackOrDefault(int pack)
        {
            string[] musicPack = GetMusicPack(pack);
            return musicPack.FirstOrDefault(name => !string.IsNullOrWhiteSpace(name));
        }

        /// <summary>
        /// Gets the level music resource names for a pack.
        /// </summary>
        /// <param name="pack">Target pack index.</param>
        /// <returns>The level music resource names, or the empty resource sentinel when <paramref name="pack"/> is out of range.</returns>
        public static string[] GetMusicList(int pack)
        {
            return pack >= 0 && pack < packs.Count ? packs[pack].MusicList : EmptyResourceNames;
        }

        /// <summary>
        /// Gets non-empty level music resource names for a pack.
        /// </summary>
        /// <param name="pack">Target pack index.</param>
        /// <returns>The filtered level music resource names.</returns>
        public static string[] GetMusicListOrDefault(int pack)
        {
            string[] musicList = GetMusicList(pack);
            return [.. musicList.Where(name => !string.IsNullOrWhiteSpace(name))];
        }

        /// <summary>
        /// Gets the star count required to unlock a pack.
        /// </summary>
        /// <param name="pack">Target pack index.</param>
        /// <returns>The required star count, or 0 when <paramref name="pack"/> is out of range.</returns>
        public static int GetUnlockStars(int pack)
        {
            return pack >= 0 && pack < packs.Count ? packs[pack].UnlockStars : 0;
        }

        /// <summary>
        /// Gets whether a pack uses earth background animations.
        /// </summary>
        /// <param name="pack">Target pack index.</param>
        /// <returns><see langword="true"/> when the pack uses earth background animations; otherwise, <see langword="false"/>.</returns>
        public static bool GetEarthBg(int pack)
        {
            return pack >= 0 && pack < packs.Count && packs[pack].EarthBg;
        }

        /// <summary>
        /// Gets the earth background position override for a pack.
        /// </summary>
        /// <param name="pack">Target pack index.</param>
        /// <returns>The earth background position override, or <see langword="null"/> when none is configured or <paramref name="pack"/> is out of range.</returns>
        public static Vector? GetEarthBgPosition(int pack)
        {
            return pack >= 0 && pack < packs.Count ? packs[pack].EarthBgPosition : null;
        }

        /// <summary>
        /// Gets the ghost grab circle color override for a pack.
        /// </summary>
        /// <param name="pack">Target pack index.</param>
        /// <returns>The ghost grab color override, or <see langword="null"/> when none is configured or <paramref name="pack"/> is out of range.</returns>
        public static RGBAColor? GetGhostGrabColor(int pack)
        {
            return pack >= 0 && pack < packs.Count ? packs[pack].GhostGrabColor : null;
        }

        /// <summary>
        /// Gets the box hole background color for a pack.
        /// </summary>
        /// <param name="pack">Target pack index.</param>
        /// <returns>The configured box hole background color, or the default color when <paramref name="pack"/> is out of range.</returns>
        public static RGBAColor GetBoxHoleBgColor(int pack)
        {
            return pack >= 0 && pack < packs.Count ? packs[pack].BoxHoleBgColor : DefaultBoxHoleBgColor;
        }

        /// <summary>
        /// Gets the optional box label text key for a pack.
        /// </summary>
        /// <param name="pack">Target pack index.</param>
        /// <returns>The box label text key, or <see langword="null"/> when none is configured or <paramref name="pack"/> is out of range.</returns>
        public static string GetBoxLabelText(int pack)
        {
            return pack >= 0 && pack < packs.Count ? packs[pack].BoxLabelText : null;
        }

        /// <summary>
        /// Gets the localized pack name key for a pack.
        /// </summary>
        /// <param name="pack">Target pack index.</param>
        /// <returns>The pack name key, or <see langword="null"/> when none is configured or <paramref name="pack"/> is out of range.</returns>
        public static string GetPackName(int pack)
        {
            return pack >= 0 && pack < packs.Count ? packs[pack].PackName : null;
        }

        /// <summary>
        /// Gets the pack selection spritesheet resource name for a pack.
        /// </summary>
        /// <param name="pack">Target pack index.</param>
        /// <returns>The pack spritesheet resource name, or <see langword="null"/> when <paramref name="pack"/> is out of range.</returns>
        public static string GetPackSpritesheet(int pack)
        {
            return pack >= 0 && pack < packs.Count ? packs[pack].PackSpritesheet : null;
        }

        /// <summary>
        /// Gets the pack selection quad index for a pack.
        /// </summary>
        /// <param name="pack">Target pack index.</param>
        /// <returns>The pack quad index, or 0 when <paramref name="pack"/> is out of range.</returns>
        public static int GetPackQuadIndex(int pack)
        {
            return pack >= 0 && pack < packs.Count ? packs[pack].PackQuadIndex : 0;
        }

        /// <summary>
        /// Returns the index of the first non-playable pack entry (coming soon placeholder), or -1 if none.
        /// </summary>
        /// <returns>The coming-soon pack index, or -1 when no placeholder pack exists.</returns>
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

        /// <summary>
        /// Loads pack list entries from the master configuration file.
        /// </summary>
        /// <returns>The configured pack list entries, or a single default entry when no master list exists.</returns>
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

        /// <summary>
        /// Parses and appends one pack list entry.
        /// </summary>
        /// <param name="entryElement">JSON object containing pack list entry data.</param>
        /// <param name="entries">Destination collection for parsed entries.</param>
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

        /// <summary>
        /// Loads pack definitions from each configured pack file.
        /// </summary>
        /// <param name="packListEntries">Pack list entries that identify pack files and save slots.</param>
        /// <returns>The loaded pack definitions.</returns>
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

        /// <summary>
        /// Resolves the pack array from a pack configuration root.
        /// </summary>
        /// <param name="root">Root JSON element from the pack configuration file.</param>
        /// <param name="configFileName">Configuration file name used in error messages.</param>
        /// <returns>The JSON array that contains pack definitions.</returns>
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
        /// <param name="id">Spritesheet ID from the pack configuration.</param>
        /// <returns>The resolved spritesheet resource name.</returns>
        private static string ResolvePackSpritesheetId(string id)
        {
            return id switch
            {
                "1" => Resources.Img.MenuPackSelection,
                "2" => Resources.Img.MenuPackSelection2,
                _ => Resources.Img.MenuPackSelection,
            };
        }

        /// <summary>
        /// Normalizes a pack configuration name to a JSON file name.
        /// </summary>
        /// <param name="packsConfigName">Raw pack configuration name.</param>
        /// <returns>The normalized JSON file name, or the default file name when <paramref name="packsConfigName"/> is empty.</returns>
        private static string NormalizePacksConfigFileName(string packsConfigName)
        {
            if (string.IsNullOrWhiteSpace(packsConfigName))
            {
                return DefaultPacksConfigFile;
            }

            string normalized = packsConfigName.Trim();
            return normalized.EndsWith(".json", StringComparison.OrdinalIgnoreCase) ? normalized : $"{normalized}.json";
        }

        /// <summary>
        /// Attempts to load a JSON root element from the content directory.
        /// </summary>
        /// <param name="fileName">JSON file name relative to the content root directory.</param>
        /// <param name="root">Loaded root element when the load succeeds.</param>
        /// <returns><see langword="true"/> when the file was loaded and parsed; otherwise, <see langword="false"/>.</returns>
        private static bool TryLoadJsonRoot(string fileName, out JsonElement root)
        {
            root = default;

            if (string.IsNullOrWhiteSpace(fileName))
            {
                return false;
            }

            try
            {
                using Stream stream = ContentPaths.OpenStream(fileName);
                using JsonDocument document = JsonDocument.Parse(stream);
                root = document.RootElement.Clone();
                return true;
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine($"Failed to load '{fileName}': {exception}");
                return false;
            }
        }

        /// <summary>
        /// Parses an integer property from a JSON element.
        /// </summary>
        /// <param name="element">JSON object that owns the property.</param>
        /// <param name="propertyName">Property name to parse.</param>
        /// <param name="defaultValue">Value returned when the property is missing.</param>
        /// <param name="fileName">Configuration file name used in error messages.</param>
        /// <returns>The parsed integer value, or <paramref name="defaultValue"/> when the property is missing.</returns>
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

        /// <summary>
        /// Parses a boolean property from a JSON element.
        /// </summary>
        /// <param name="element">JSON object that owns the property.</param>
        /// <param name="propertyName">Property name to parse.</param>
        /// <param name="defaultValue">Value returned when the property is missing.</param>
        /// <param name="fileName">Configuration file name used in error messages.</param>
        /// <returns>The parsed boolean value, or <paramref name="defaultValue"/> when the property is missing.</returns>
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

        /// <summary>
        /// Parses a required string property from a JSON element.
        /// </summary>
        /// <param name="element">JSON object that owns the property.</param>
        /// <param name="propertyName">Property name to parse.</param>
        /// <param name="fileName">Configuration file name used in error messages.</param>
        /// <returns>The parsed string value.</returns>
        private static string ParseRequiredString(JsonElement element, string propertyName, string fileName)
        {
            string value = ParseStringProperty(element, propertyName);
            return string.IsNullOrWhiteSpace(value)
                ? throw new InvalidDataException($"{fileName} is missing required property '{propertyName}'.")
                : value;
        }

        /// <summary>
        /// Parses an optional string property from a JSON element.
        /// </summary>
        /// <param name="element">JSON object that owns the property.</param>
        /// <param name="propertyName">Property name to parse.</param>
        /// <returns>The trimmed string value, or <see langword="null"/> when the property is missing, <see langword="null"/>, or not a string.</returns>
        private static string ParseStringProperty(JsonElement element, string propertyName)
        {
            return !element.TryGetProperty(propertyName, out JsonElement value) || value.ValueKind == JsonValueKind.Null
                ? null
                : value.ValueKind == JsonValueKind.String
                ? value.GetString()?.Trim()
                : null;
        }

        /// <summary>
        /// Parses comma-separated or array-based resource names from a JSON element.
        /// </summary>
        /// <param name="element">JSON object that owns the property.</param>
        /// <param name="propertyName">Property name to parse.</param>
        /// <returns>The parsed resource names with a trailing sentinel, or the empty resource sentinel when the property is missing.</returns>
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

        /// <summary>
        /// Parses a vector property from a string or array JSON value.
        /// </summary>
        /// <param name="element">JSON object that owns the property.</param>
        /// <param name="propertyName">Property name to parse.</param>
        /// <param name="fileName">Configuration file name used in error messages.</param>
        /// <returns>The parsed vector, or <see langword="null"/> when the property is missing or <see langword="null"/>.</returns>
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

        /// <summary>
        /// Parses an optional color property from a JSON element.
        /// </summary>
        /// <param name="element">JSON object that owns the property.</param>
        /// <param name="propertyName">Property name to parse.</param>
        /// <returns>The parsed color, or <see langword="null"/> when the property is missing or <see langword="null"/>.</returns>
        private static RGBAColor? ParseNullableColorProperty(JsonElement element, string propertyName)
        {
            return !element.TryGetProperty(propertyName, out JsonElement value) || value.ValueKind == JsonValueKind.Null
                ? null
                : ParseColorProperty(element, propertyName);
        }

        /// <summary>
        /// Parses a color property from a string or array JSON value.
        /// </summary>
        /// <param name="element">JSON object that owns the property.</param>
        /// <param name="propertyName">Property name to parse.</param>
        /// <returns>The parsed color, or the default box hole background color when the property is missing or invalid.</returns>
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

        /// <summary>
        /// Parses RGBA color channels from string parts.
        /// </summary>
        /// <param name="parts">Color channel parts in red, green, blue, and optional alpha order.</param>
        /// <returns>The parsed color, or the default box hole background color when too few channels are provided.</returns>
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

        /// <summary>
        /// Validates resource names while preserving the trailing <see langword="null"/> sentinel.
        /// </summary>
        /// <param name="resourceNames">Resource names to validate.</param>
        /// <param name="context">Pack configuration property name used in error messages.</param>
        /// <param name="packsConfigFile">Configuration file name used in error messages.</param>
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

        /// <summary>
        /// Ensures that a playable pack has at least one resource name for a required field.
        /// </summary>
        /// <param name="resourceNames">Resource names to inspect.</param>
        /// <param name="context">Pack configuration property name used in error messages.</param>
        /// <param name="packsConfigFile">Configuration file name used in error messages.</param>
        private static void RequireResourceNames(string[] resourceNames, string context, string packsConfigFile)
        {
            if (resourceNames.Length == 0 || string.IsNullOrWhiteSpace(resourceNames[0]))
            {
                throw new InvalidDataException($"{packsConfigFile} is missing required {context}.");
            }
        }

        /// <summary>
        /// Validates a single resource name against the resource registry.
        /// </summary>
        /// <param name="resourceName">Resource name to validate.</param>
        /// <param name="context">Pack configuration property name used in error messages.</param>
        /// <param name="packsConfigFile">Configuration file name used in error messages.</param>
        private static void ValidateResourceName(string resourceName, string context, string packsConfigFile)
        {
            if (!Resources.IsValidResourceName(resourceName))
            {
                throw new InvalidDataException($"{packsConfigFile} contains unknown resource name '{resourceName}' in '{context}'.");
            }
        }
    }
}
