using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Helpers;

using Microsoft.Xna.Framework;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Loads and provides access to available Om Nom skins from the manifest.
    /// Slot 0 is always the classic sprite-frame skin (not in the manifest).
    /// Slot 1+ correspond to XML skins in the manifest (index = slot - 1).
    /// </summary>
    internal static class OmNomSkinRegistry
    {
        /// <summary>
        /// File name of the Om Nom skin manifest within the animations content directory.
        /// </summary>
        private const string ManifestFileName = "om_nom_skins.json";

        /// <summary>
        /// XML-based skin definitions loaded from the manifest.
        /// </summary>
        private static readonly List<OmNomSkinDefinition> xmlSkins;

        /// <summary>
        /// Initializes the skin registry from the manifest.
        /// </summary>
        static OmNomSkinRegistry()
        {
            xmlSkins = LoadManifest();
        }

        /// <summary>XML-based skins only (slot 1+). Index 0 here = slot 1.</summary>
        public static IReadOnlyList<OmNomSkinDefinition> XmlSkins => xmlSkins;

        /// <summary>Total skin count including classic slot 0.</summary>
        public static int TotalSkinCount => xmlSkins.Count + 1;

        /// <summary>Gets the currently selected skin index from preferences.</summary>
        /// <returns>The selected skin index, or 0 when the saved value is out of range.</returns>
        public static int GetSelectedSkinIndex()
        {
            int index = Preferences.GetIntForKey("PREFS_SELECTED_OMNOM");
            return index >= 0 && index < TotalSkinCount ? index : 0;
        }

        /// <summary>Whether the given index is the classic sprite-frame skin.</summary>
        /// <param name="skinIndex">Skin slot index to inspect.</param>
        /// <returns><see langword="true"/> when <paramref name="skinIndex"/> is the classic skin slot; otherwise, <see langword="false"/>.</returns>
        public static bool IsClassicSkin(int skinIndex)
        {
            return skinIndex == 0;
        }

        /// <summary>Gets the XML skin definition for a non-classic skin index.</summary>
        /// <param name="skinIndex">Non-classic skin slot index.</param>
        /// <returns>The XML skin definition for <paramref name="skinIndex"/>.</returns>
        public static OmNomSkinDefinition GetXmlSkinDefinition(int skinIndex)
        {
            int xmlIndex = skinIndex - 1;
            return xmlIndex >= 0 && xmlIndex < xmlSkins.Count
                ? xmlSkins[xmlIndex]
                : throw new ArgumentOutOfRangeException(nameof(skinIndex),
                    $"Skin index {skinIndex} is out of range. Valid range: 1-{xmlSkins.Count}.");
        }

        /// <summary>
        /// Loads XML-based skin definitions from the manifest.
        /// </summary>
        /// <returns>The skin definitions loaded from the manifest, or an empty list when the manifest is missing or invalid.</returns>
        private static List<OmNomSkinDefinition> LoadManifest()
        {
            List<OmNomSkinDefinition> skins = [];

            string manifestPath = Path.Combine(
                ContentPaths.AnimationsDirectory, ManifestFileName);

            try
            {
                using Stream stream = TitleContainer.OpenStream(
                    Path.Combine(ContentPaths.RootDirectory, manifestPath));
                using JsonDocument document = JsonDocument.Parse(stream);
                JsonElement root = document.RootElement;

                if (root.ValueKind != JsonValueKind.Array)
                {
                    return skins;
                }

                foreach (JsonElement entry in root.EnumerateArray())
                {
                    OmNomSkinDefinition skin = ParseSkinEntry(entry);
                    if (skin != null)
                    {
                        skins.Add(skin);
                    }
                }
            }
            catch
            {
                // If manifest is missing or invalid, return empty list.
                // Classic skin (slot 0) is always available.
            }

            return skins;
        }

        /// <summary>
        /// Parses a single skin manifest entry.
        /// </summary>
        /// <param name="entry">JSON object that describes an Om Nom skin.</param>
        /// <returns>The parsed skin definition, or <see langword="null"/> when the entry is invalid.</returns>
        internal static OmNomSkinDefinition ParseSkinEntry(JsonElement entry)
        {
            if (entry.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            string id = GetStringProperty(entry, "id");
            string name = GetStringProperty(entry, "name");
            string animationXml = GetStringProperty(entry, "animationXml");
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(animationXml))
            {
                return null;
            }

            string xmlPath = ContentPaths.GetAnimationXmlAbsolutePath(animationXml);

            Dictionary<TargetAnimationState, int> timelineMappings = [];
            if (entry.TryGetProperty("timelines", out JsonElement timelinesElement)
                && timelinesElement.ValueKind == JsonValueKind.Object)
            {
                foreach (JsonProperty property in timelinesElement.EnumerateObject())
                {
                    if (Enum.TryParse(property.Name, ignoreCase: false, out TargetAnimationState state)
                        && property.Value.TryGetInt32(out int timelineId))
                    {
                        timelineMappings[state] = timelineId;
                    }
                }
            }

            Dictionary<int, int> followups = [];
            if (entry.TryGetProperty("followups", out JsonElement followupsElement)
                && followupsElement.ValueKind == JsonValueKind.Object)
            {
                foreach (JsonProperty property in followupsElement.EnumerateObject())
                {
                    if (int.TryParse(property.Name, NumberStyles.Integer, CultureInfo.InvariantCulture, out int fromId)
                        && property.Value.TryGetInt32(out int toId))
                    {
                        followups[fromId] = toId;
                    }
                }
            }

            List<int> idleVariants = [];
            if (entry.TryGetProperty("idleVariants", out JsonElement idleElement)
                && idleElement.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement item in idleElement.EnumerateArray())
                {
                    if (item.TryGetInt32(out int variantId))
                    {
                        idleVariants.Add(variantId);
                    }
                }
            }

            int idleToSleepTrimFrames = 0;
            if (entry.TryGetProperty("idleToSleepTrimFrames", out JsonElement trimElement)
                && trimElement.TryGetInt32(out int parsedTrimFrames))
            {
                idleToSleepTrimFrames = Math.Max(0, parsedTrimFrames);
            }

            List<int> slowTimelineIds = [];
            if (entry.TryGetProperty("slowTimelines", out JsonElement slowTimelinesElement)
                && slowTimelinesElement.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement item in slowTimelinesElement.EnumerateArray())
                {
                    if (item.TryGetInt32(out int timelineId))
                    {
                        slowTimelineIds.Add(timelineId);
                    }
                }
            }

            bool startWithGreeting = false;
            if (entry.TryGetProperty("startWithGreeting", out JsonElement greetingElement)
                && greetingElement.ValueKind == JsonValueKind.True)
            {
                startWithGreeting = true;
            }

            List<string> uniqueSounds = [];
            if (entry.TryGetProperty("uniqueSounds", out JsonElement uniqueSoundsElement)
                && uniqueSoundsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement item in uniqueSoundsElement.EnumerateArray())
                {
                    if (item.ValueKind != JsonValueKind.String)
                    {
                        continue;
                    }

                    string soundIdentifier = item.GetString()?.Trim();
                    if (Resources.TryResolveSoundIdentifier(soundIdentifier, out string soundResourceName))
                    {
                        uniqueSounds.Add(soundResourceName);
                    }
                }
            }

            return new OmNomSkinDefinition(
                id,
                name,
                xmlPath,
                timelineMappings,
                followups,
                [.. idleVariants],
                idleToSleepTrimFrames,
                [.. slowTimelineIds],
                startWithGreeting,
                [.. uniqueSounds]);
        }

        /// <summary>
        /// Reads and trims a string property from a JSON object.
        /// </summary>
        /// <param name="element">JSON object that owns the property.</param>
        /// <param name="propertyName">Property name to read.</param>
        /// <returns>The trimmed string value, or <see langword="null"/> when the property is missing or not a string.</returns>
        private static string GetStringProperty(JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out JsonElement value)
                && value.ValueKind == JsonValueKind.String
                ? value.GetString()?.Trim()
                : null;
        }
    }
}
