using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;

using CutTheRope.Framework;

using Microsoft.Xna.Framework;

namespace CutTheRope.Helpers
{
    /// <summary>
    /// Manages JSON-based localization strings loaded from per-language files.
    /// </summary>
    internal static class LocalizationManager
    {
        /// <summary>
        /// Per-language localized strings storage.
        /// Structure: languageCode -> (stringKey -> localizedText)
        /// </summary>
        private static readonly Dictionary<string, Dictionary<string, string>> langStrings_ = [];

        private static readonly Lock langStringsLock_ = new();

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

            Dictionary<string, string> strings = GetLanguageStrings(languageCode);
            if (strings != null && strings.TryGetValue(key, out string value))
            {
                return value;
            }

            // Fallback to English
            if (languageCode != "en")
            {
                Dictionary<string, string> enStrings = GetLanguageStrings("en");
                if (enStrings != null && enStrings.TryGetValue(key, out string fallback))
                {
                    return fallback;
                }
            }

            return string.Empty;
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

            Dictionary<string, string> strings = GetLanguageStrings(LanguageHelper.CurrentCode);
            if (strings != null && strings.ContainsKey(key))
            {
                return true;
            }

            Dictionary<string, string> enStrings = GetLanguageStrings("en");
            return enStrings?.ContainsKey(key) ?? false;
        }

        /// <summary>
        /// Ensures localization strings are loaded for English and the current language.
        /// </summary>
        public static void EnsureLoaded()
        {
            _ = GetLanguageStrings("en");
            string current = LanguageHelper.CurrentCode;
            if (current != "en")
            {
                _ = GetLanguageStrings(current);
            }
        }

        /// <summary>
        /// Clears the cached strings, forcing a reload on next access.
        /// </summary>
        public static void ClearCache()
        {
            lock (langStringsLock_)
            {
                langStrings_.Clear();
            }
        }

        private static Dictionary<string, string> GetLanguageStrings(string languageCode)
        {
            lock (langStringsLock_)
            {
                if (langStrings_.TryGetValue(languageCode, out Dictionary<string, string> cached))
                {
                    return cached;
                }
            }

            Dictionary<string, string> loaded = LoadLanguageFile(languageCode);

            lock (langStringsLock_)
            {
                langStrings_[languageCode] = loaded;
            }

            return loaded;
        }

        private static Dictionary<string, string> LoadLanguageFile(string languageCode)
        {
            Dictionary<string, string> result = [];

            try
            {
                string path = ContentPaths.GetStringsPath(languageCode);
                using Stream stream = OpenStream(path);
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

                using JsonDocument doc = JsonDocument.Parse(json);
                foreach (JsonProperty prop in doc.RootElement.EnumerateObject())
                {
                    if (prop.Value.ValueKind == JsonValueKind.String)
                    {
                        result[prop.Name] = prop.Value.GetString() ?? string.Empty;
                    }
                    else if (prop.Value.ValueKind == JsonValueKind.Object)
                    {
                        foreach (JsonProperty nested in prop.Value.EnumerateObject())
                        {
                            if (nested.Value.ValueKind == JsonValueKind.String)
                            {
                                result[nested.Name] = nested.Value.GetString() ?? string.Empty;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load localization strings for '{languageCode}': {ex.Message}");
            }

            return result;
        }

        private static Stream OpenStream(string fileName)
        {
            try
            {
                string contentPath = Path.Combine(ContentPaths.RootDirectory, fileName);
                return TitleContainer.OpenStream(contentPath);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
