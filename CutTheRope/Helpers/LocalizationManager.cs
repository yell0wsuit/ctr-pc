using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;

using CutTheRope.Framework;
using CutTheRope.GameMain;

using Microsoft.Xna.Framework;

namespace CutTheRope.Helpers
{
    /// <summary>
    /// Manages JSON-based localization strings.
    /// </summary>
    internal static class LocalizationManager
    {
        /// <summary>
        /// JSON-based localized strings storage.
        /// Structure: stringKey -> languageCode -> localizedText
        /// </summary>
        private static Dictionary<string, Dictionary<string, string>> jsonStrings_;

        private static readonly Lock jsonStringsLock_ = new();

        /// <summary>
        /// Gets a localized string for the given key and language.
        /// </summary>
        /// <param name="key">The string key (e.g., "PLAY", "OPTIONS")</param>
        /// <param name="languageCode">The language code (e.g., "en", "ru", "de", "fr")</param>
        /// <returns>The localized string, or empty string if not found.</returns>
        public static string GetString(string key, string languageCode)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            EnsureStringsLoaded();

            lock (jsonStringsLock_)
            {
                if (jsonStrings_ == null)
                {
                    return string.Empty;
                }

                if (!jsonStrings_.TryGetValue(key, out Dictionary<string, string> translations))
                {
                    return string.Empty;
                }

                // Try to get the requested language
                if (translations.TryGetValue(languageCode, out string value))
                {
                    return value;
                }

                // Fallback to English
                return languageCode != "en" && translations.TryGetValue("en", out string fallback) ? fallback : string.Empty;
            }
        }

        /// <summary>
        /// Gets a localized string for the given key using the current language.
        /// </summary>
        /// <param name="key">The string key (e.g., "PLAY", "OPTIONS")</param>
        /// <returns>The localized string, or empty string if not found.</returns>
        public static string GetString(string key)
        {
            string languageCode = LanguageHelper.CurrentCode;
            return GetString(key, languageCode);
        }

        /// <summary>
        /// Checks if a string key exists in the localization data.
        /// </summary>
        public static bool HasString(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            EnsureStringsLoaded();

            lock (jsonStringsLock_)
            {
                return jsonStrings_?.ContainsKey(key) ?? false;
            }
        }

        /// <summary>
        /// Ensures the localization strings are loaded into memory.
        /// </summary>
        public static void EnsureLoaded()
        {
            EnsureStringsLoaded();
        }

        /// <summary>
        /// Clears the cached strings, forcing a reload on next access.
        /// </summary>
        public static void ClearCache()
        {
            lock (jsonStringsLock_)
            {
                jsonStrings_ = null;
            }
        }

        private static void EnsureStringsLoaded()
        {
            if (jsonStrings_ != null)
            {
                return;
            }

            lock (jsonStringsLock_)
            {
                jsonStrings_ ??= LoadJsonStrings();
            }
        }

        private static Dictionary<string, Dictionary<string, string>> LoadJsonStrings()
        {
            Dictionary<string, Dictionary<string, string>> result = [];

            try
            {
                using Stream stream = OpenStream(ContentPaths.MenuStringsFile);
                if (stream == null)
                {
                    return result;
                }

                using StreamReader reader = new(stream);
                string json = reader.ReadToEnd();

                if (string.IsNullOrEmpty(json))
                {
                    return result;
                }

                result = DeserializeLocalizationJson(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load localization strings: {ex.Message}");
            }

            return result ?? [];
        }

        /// <summary>
        /// Deserializes localization JSON using AOT-safe JsonDocument parsing.
        /// </summary>
        private static Dictionary<string, Dictionary<string, string>> DeserializeLocalizationJson(string json)
        {
            Dictionary<string, Dictionary<string, string>> result = [];

            using JsonDocument doc = JsonDocument.Parse(json);
            foreach (JsonProperty keyProp in doc.RootElement.EnumerateObject())
            {
                Dictionary<string, string> translations = [];

                if (keyProp.Value.ValueKind == JsonValueKind.Object)
                {
                    foreach (JsonProperty langProp in keyProp.Value.EnumerateObject())
                    {
                        if (langProp.Value.ValueKind == JsonValueKind.String)
                        {
                            translations[langProp.Name] = langProp.Value.GetString() ?? string.Empty;
                        }
                    }
                }

                result[keyProp.Name] = translations;
            }

            return result;
        }

        private static Stream OpenStream(string fileName)
        {
            string[] candidates = string.IsNullOrEmpty(ResDataPhoneFull.ContentFolder)
                ? [fileName]
                : [$"{ResDataPhoneFull.ContentFolder}{fileName}", fileName];

            foreach (string candidate in candidates)
            {
                try
                {
                    string contentPath = $"{ContentPaths.RootDirectory}/{candidate}";
                    return TitleContainer.OpenStream(contentPath);
                }
                catch (Exception)
                {
                }
            }

            return null;
        }
    }
}
