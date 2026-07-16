using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;

using CutTheRopeDX.Framework;

namespace CutTheRopeDX.Helpers
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

        /// <summary>
        /// Lock protecting concurrent access to <see cref="langStrings_"/>.
        /// </summary>
        private static readonly Lock langStringsLock_ = new();

        /// <summary>
        /// Gets a localized string for the given key and language.
        /// </summary>
        /// <param name="key">The string key (e.g., <c>PLAY</c>, <c>OPTIONS</c>)</param>
        /// <param name="languageCode">The language code (e.g., <c>en</c>, <c>ru</c>, <c>de</c>, <c>fr</c>)</param>
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
        /// <param name="key">The string key to look up.</param>
        /// <returns><see langword="true"/> when the key exists in the current language or English fallback; otherwise <see langword="false"/>.</returns>
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

        /// <summary>
        /// Returns the cached string dictionary for the given language, loading it from disk if necessary.
        /// </summary>
        /// <param name="languageCode">The language code to retrieve.</param>
        /// <returns>The cached or newly loaded string dictionary for the requested language.</returns>
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

        /// <summary>
        /// Loads and parses a localization JSON file for the given language.
        /// </summary>
        /// <param name="languageCode">The language code (e.g., "en", "ru").</param>
        /// <returns>A dictionary containing all parsed localized strings for the language.</returns>
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

        /// <summary>
        /// Opens a content stream for the given file name, returning <see langword="null"/> on failure.
        /// </summary>
        /// <param name="fileName">Relative path within the content root.</param>
        /// <returns>An open content stream, or <see langword="null"/> if opening fails.</returns>
        private static Stream OpenStream(string fileName)
        {
            try
            {
                return ContentPaths.OpenStream(fileName);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
