using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

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
        string supportResourceName,
        string[] boxCovers,
        bool earthBg)
    {
        /// <summary>Number of stars required to unlock this pack.</summary>
        public int UnlockStars { get; } = unlockStars;

        /// <summary>String resource names for pack assets.</summary>
        public string[] BoxBackgrounds { get; } = boxBackgrounds;

        /// <summary>String resource name for the support asset.</summary>
        public string SupportResourceName { get; } = supportResourceName;

        /// <summary>String resource names for cover assets.</summary>
        public string[] BoxCovers { get; } = boxCovers;

        /// <summary>Total number of levels in the pack.</summary>
        public int LevelCount { get; } = levelCount;

        /// <summary>Whether this pack uses earth background animations.</summary>
        public bool EarthBg { get; } = earthBg;
    }

    /// <summary>
    /// Loads pack metadata from <c>packs.xml</c> and exposes string resource names.
    /// </summary>
    internal static class PackConfig
    {
        private static readonly string[] EmptyResourceNames = [null];

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

        public static int GetUnlockStars(int pack)
        {
            return pack >= 0 && pack < packs.Count ? packs[pack].UnlockStars : 0;
        }

        public static bool GetEarthBg(int pack)
        {
            return pack >= 0 && pack < packs.Count && packs[pack].EarthBg;
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

                string supportResourceName = ParseResourceName(packElement, "supportResourceName");
                supportResourceName ??= Resources.Img.CharSupports;
                ValidateResourceName(supportResourceName, "supportResourceName");

                string[] boxCovers = ParseResourceNames(packElement, "boxCover");
                RequireResourceNames(boxCovers, "boxCover");
                ValidateResourceNames(boxCovers, "boxCover");

                bool earthBg = ParseBoolAttribute(packElement, "earthBg");

                results.Add(new PackDefinition(
                    unlockStars,
                    levelCount,
                    boxBackgrounds,
                    supportResourceName,
                    boxCovers,
                    earthBg));
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
