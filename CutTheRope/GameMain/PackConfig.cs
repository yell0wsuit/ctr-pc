using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Helpers;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Immutable pack description using string resource names.
    /// </summary>
    internal sealed class PackDefinition(
        int unlockStars,
        int levelCount,
        string[] boxBackgrounds,
        int boxBackgroundP2Y,
        string supportResourceName,
        string[] boxCovers,
        RGBAColor boxHoleBgColor,
        string[] musicPack,
        string[] musicList,
        bool earthBg,
        Vector? earthBgPosition,
        string boxLabelText)
    {
        /// <summary>Number of stars required to unlock this pack.</summary>
        public int UnlockStars { get; } = unlockStars;

        /// <summary>String resource names for pack assets.</summary>
        public string[] BoxBackgrounds { get; } = boxBackgrounds;

        /// <summary>Y position for secondary background (p2) in long levels. 0 means no p2.</summary>
        public int BoxBackgroundP2Y { get; } = boxBackgroundP2Y;

        /// <summary>String resource name for the support asset.</summary>
        public string SupportResourceName { get; } = supportResourceName;

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

        /// <summary>Whether this pack uses earth background animations.</summary>
        public bool EarthBg { get; } = earthBg;

        /// <summary>Position for earth background animation (null uses default).</summary>
        public Vector? EarthBgPosition { get; } = earthBgPosition;

        /// <summary>Localization key for optional box label text (e.g., "the hardest one").</summary>
        public string BoxLabelText { get; } = boxLabelText;
    }

    /// <summary>
    /// Loads pack metadata from <c>packs.xml</c> and exposes string resource names.
    /// </summary>
    internal static class PackConfig
    {
        private static readonly string[] EmptyResourceNames = [null];

        /// <summary>Default box color when not specified in packs.xml (dark gray: 45, 45, 53).</summary>
        private static readonly RGBAColor DefaultBoxHoleBgColor = RGBAColor.MakeRGBA(45 / 255f, 45 / 255f, 53 / 255f, 1f);

        private static readonly List<PackDefinition> packs;

        static PackConfig()
        {
            packs = LoadFromXml();
            MaxLevelsPerPack = packs.Count > 0 ? packs.Max(p => p.LevelCount) : 0;
        }

        public static IReadOnlyList<PackDefinition> Packs => packs;

        public static int MaxLevelsPerPack { get; }

        public static int GetPackCount()
        {
            return packs.Count;
        }

        public static int GetLevelCount(int pack)
        {
            return pack >= 0 && pack < packs.Count ? packs[pack].LevelCount : 0;
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
                ? throw new InvalidDataException($"packs.xml is missing boxCover for pack {pack}.")
                : coverResourceName;
        }

        public static string GetSupportResourceName(int pack)
        {
            return pack >= 0 && pack < packs.Count ? packs[pack].SupportResourceName : null;
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

        public static RGBAColor GetBoxHoleBgColor(int pack)
        {
            return pack >= 0 && pack < packs.Count ? packs[pack].BoxHoleBgColor : DefaultBoxHoleBgColor;
        }

        public static string GetBoxLabelText(int pack)
        {
            return pack >= 0 && pack < packs.Count ? packs[pack].BoxLabelText : null;
        }

        private static List<PackDefinition> LoadFromXml()
        {
            XElement root = XElementExtensions.LoadContentXml("packs.xml");
            List<PackDefinition> results = [];

            if (root == null)
            {
                return results;
            }

            foreach (XElement packElement in root.Elements("pack"))
            {
                int unlockStars = ParseIntAttribute(packElement, "unlockStars");
                int levelCount = ParseLevelCount(packElement);

                string[] boxBackgrounds = ParseResourceNames(packElement, "boxBackground");
                RequireResourceNames(boxBackgrounds, "boxBackground");
                ValidateResourceNames(boxBackgrounds, "boxBackground");

                int boxBackgroundP2Y = ParseIntAttribute(packElement, "boxBackgroundP2Y");

                string supportResourceName = ParseResourceName(packElement, "supportResourceName");
                supportResourceName ??= Resources.Img.CharSupports;
                ValidateResourceName(supportResourceName, "supportResourceName");

                string[] boxCovers = ParseResourceNames(packElement, "boxCover");
                RequireResourceNames(boxCovers, "boxCover");
                ValidateResourceNames(boxCovers, "boxCover");

                RGBAColor boxHoleBgColor = ParseColorAttribute(packElement, "boxHoleBgColor");

                string[] musicPack = ParseResourceNames(packElement, "musicPack");

                string[] musicList = ParseResourceNames(packElement, "musicList");
                ValidateResourceNames(musicList, "musicList");

                bool earthBg = ParseBoolAttribute(packElement, "earthBg");

                Vector? earthBgPosition = ParseVectorAttribute(packElement, "earthBgPosition");

                string boxLabelText = ParseResourceName(packElement, "boxLabelText");

                results.Add(new PackDefinition(
                    unlockStars,
                    levelCount,
                    boxBackgrounds,
                    boxBackgroundP2Y,
                    supportResourceName,
                    boxCovers,
                    boxHoleBgColor,
                    musicPack,
                    musicList,
                    earthBg,
                    earthBgPosition,
                    boxLabelText));
            }

            return results;
        }

        private static int ParseIntAttribute(XElement element, string attributeName, int defaultValue = 0)
        {
            string value = element.AttributeAsNSString(attributeName);
            return string.IsNullOrWhiteSpace(value) ? defaultValue : int.Parse(value, CultureInfo.InvariantCulture);
        }

        private static bool ParseBoolAttribute(XElement element, string attributeName, bool defaultValue = false)
        {
            string value = element.AttributeAsNSString(attributeName);
            return string.IsNullOrWhiteSpace(value) ? defaultValue : bool.Parse(value);
        }

        private static Vector? ParseVectorAttribute(XElement element, string attributeName)
        {
            string value = element.AttributeAsNSString(attributeName);
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            string[] parts = value.Split(',');
            if (parts.Length >= 2)
            {
                float x = float.Parse(parts[0].Trim(), CultureInfo.InvariantCulture);
                float y = float.Parse(parts[1].Trim(), CultureInfo.InvariantCulture);
                return new Vector(x, y);
            }

            return null;
        }

        private static RGBAColor ParseColorAttribute(XElement element, string attributeName)
        {
            string value = element.AttributeAsNSString(attributeName);
            if (string.IsNullOrWhiteSpace(value))
            {
                return DefaultBoxHoleBgColor;
            }

            string[] parts = value.Split(',');
            if (parts.Length >= 3)
            {
                float r = int.Parse(parts[0].Trim(), CultureInfo.InvariantCulture) / 255f;
                float g = int.Parse(parts[1].Trim(), CultureInfo.InvariantCulture) / 255f;
                float b = int.Parse(parts[2].Trim(), CultureInfo.InvariantCulture) / 255f;
                float a = parts.Length >= 4 ? int.Parse(parts[3].Trim(), CultureInfo.InvariantCulture) / 255f : 1f;
                return RGBAColor.MakeRGBA(r, g, b, a);
            }

            return DefaultBoxHoleBgColor;
        }

        private static int ParseLevelCount(XElement element)
        {
            string attributeValue = element.AttributeAsNSString("levelCount");
            if (!string.IsNullOrWhiteSpace(attributeValue))
            {
                return int.Parse(attributeValue, CultureInfo.InvariantCulture);
            }

            string elementValue = element.Element("levelCount")?.Value;
            return string.IsNullOrWhiteSpace(elementValue) ? 0 : int.Parse(elementValue, CultureInfo.InvariantCulture);
        }

        private static string ParseResourceName(XElement element, string attributeName)
        {
            string value = element.AttributeAsNSString(attributeName);
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static string[] ParseResourceNames(XElement element, string attributeName)
        {
            string value = element.AttributeAsNSString(attributeName);
            if (string.IsNullOrWhiteSpace(value))
            {
                return EmptyResourceNames;
            }

            List<string> names = [.. value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(part => part.Trim())];
            names.Add(null);
            return [.. names];
        }

        private static void ValidateResourceNames(IEnumerable<string> resourceNames, string context)
        {
            foreach (string resourceName in resourceNames)
            {
                if (resourceName == null)
                {
                    continue; // Preserve sentinel semantics
                }

                ValidateResourceName(resourceName, context);
            }
        }

        private static void RequireResourceNames(string[] resourceNames, string context)
        {
            if (resourceNames.Length == 0 || string.IsNullOrWhiteSpace(resourceNames[0]))
            {
                throw new InvalidDataException($"packs.xml is missing required {context}.");
            }
        }

        private static void ValidateResourceName(string resourceName, string context)
        {
            if (!Resources.IsValidResourceName(resourceName))
            {
                throw new InvalidDataException($"packs.xml contains unknown resource name '{resourceName}' in '{context}'.");
            }
        }
    }
}
