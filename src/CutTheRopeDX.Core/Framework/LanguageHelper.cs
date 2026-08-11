using System;
using System.Collections.Generic;
using System.Globalization;

namespace CutTheRopeDX.Framework
{
    /// <summary>
    /// Centralized utility for language-related conversions and current language state.
    /// </summary>
    public static class LanguageHelper
    {
        /// <summary>
        /// Language codes available in the UI language selector.
        /// </summary>
        private static readonly string[] uiLanguageCodes = [
            "en", // English
            "ru", // Russian
            "de", // German
            "fr", // French
            "es", // Spanish
            "it", // Italian
            "nl", // Dutch
            "pt_br", // Portuguese (Brazilian)
            "ko", // Korean
            "ja", // Japanese
            "zh", // Chinese (Simplified)
            "zh_tw" // Chinese (Traditional)
        ];

        /// <summary>
        /// Gets the list of language codes available in the UI.
        /// </summary>
        public static IReadOnlyList<string> UiLanguageCodes => uiLanguageCodes;

        /// <summary>
        /// Gets or sets the current language setting.
        /// </summary>
        public static Language Current { get; set; } = Language.LANGEN;

        /// <summary>
        /// Gets the current language as the game's language code (for example: <c>en</c>, <c>pt_br</c>, <c>zh_tw</c>).
        /// </summary>
        public static string CurrentCode => ToCode(Current);

        /// <summary>
        /// Gets the current language as its integer enum value.
        /// </summary>
        public static int CurrentAsInt => (int)Current;

        /// <summary>
        /// Checks if the given <paramref name="language"/> matches the current language.
        /// </summary>
        /// <param name="language">Language to check.</param>
        /// <returns><see langword="true"/> when <paramref name="language"/> equals <see cref="Current"/>.</returns>
        public static bool IsCurrent(Language language)
        {
            return Current == language;
        }

        /// <summary>
        /// Checks if the current language matches any of the given <paramref name="languages"/>.
        /// </summary>
        /// <param name="languages">Languages to check against.</param>
        /// <returns><see langword="true"/> when any provided language equals <see cref="Current"/>.</returns>
        public static bool IsCurrentAny(params Language[] languages)
        {
            foreach (Language lang in languages)
            {
                if (Current == lang)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if the given <paramref name="language"/> is available in the UI.
        /// </summary>
        /// <param name="language">Language to check.</param>
        /// <returns><see langword="true"/> when the language is present in the UI language list.</returns>
        public static bool IsUiLanguage(Language language)
        {
            return IsUiLanguageCode(ToCode(language));
        }

        /// <summary>
        /// Checks if the given language <paramref name="code"/> is available in the UI.
        /// </summary>
        /// <param name="code">Language code to check.</param>
        /// <returns><see langword="true"/> when <paramref name="code"/> is present in the UI language list.</returns>
        public static bool IsUiLanguageCode(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return false;
            }

            foreach (string uiCode in uiLanguageCodes)
            {
                if (uiCode == code)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the next language code in the UI cycle.
        /// </summary>
        /// <param name="currentCode">Current language code.</param>
        /// <returns>The next UI language code, or the first entry when <paramref name="currentCode"/> is not found.</returns>
        public static string GetNextUiLanguageCode(string currentCode)
        {
            for (int i = 0; i < uiLanguageCodes.Length; i++)
            {
                if (uiLanguageCodes[i] == currentCode)
                {
                    return uiLanguageCodes[(i + 1) % uiLanguageCodes.Length];
                }
            }

            return uiLanguageCodes[0];
        }

        /// <summary>
        /// Converts <paramref name="language"/> to the game's language code.
        /// </summary>
        /// <param name="language">Language to convert.</param>
        /// <returns>The game language code for <paramref name="language"/>.</returns>
        public static string ToCode(Language language)
        {
            return language switch
            {
                Language.LANGEN => "en",
                Language.LANGRU => "ru",
                Language.LANGDE => "de",
                Language.LANGFR => "fr",
                Language.LANGES => "es",
                Language.LANGIT => "it",
                Language.LANGNL => "nl",
                Language.LANGPTBR => "pt_br",
                Language.LANGKO => "ko",
                Language.LANGJA => "ja",
                Language.LANGZH => "zh",
                Language.LANGZHTW => "zh_tw",
                _ => "en",
            };
        }

        /// <summary>
        /// Converts a game language <paramref name="code"/> to a <see cref="Language"/> value.
        /// Returns <see cref="Language.LANGEN"/> for unrecognized codes.
        /// </summary>
        /// <param name="code">Language code to convert.</param>
        /// <returns>The corresponding <see cref="Language"/> value.</returns>
        public static Language FromCode(string code)
        {
            return code switch
            {
                "ru" => Language.LANGRU,
                "de" => Language.LANGDE,
                "fr" => Language.LANGFR,
                "es" => Language.LANGES,
                "it" => Language.LANGIT,
                "nl" => Language.LANGNL,
                "pt_br" => Language.LANGPTBR,
                "ko" => Language.LANGKO,
                "ja" => Language.LANGJA,
                "zh" => Language.LANGZH,
                "zh_tw" => Language.LANGZHTW,
                _ => Language.LANGEN,
            };
        }

        /// <summary>
        /// Detects the appropriate Language from the system's current culture.
        /// </summary>
        /// <returns>A supported <see cref="Language"/> inferred from the current system culture.</returns>
        public static Language FromSystemCulture()
        {
            CultureInfo culture = CultureInfo.CurrentCulture;
            string code = culture.TwoLetterISOLanguageName;

            // Distinguish zh-TW/zh-HK (Traditional) from zh-CN (Simplified)
            if (code == "zh")
            {
                string name = culture.Name;
                return name.StartsWith("zh-TW", StringComparison.OrdinalIgnoreCase) ||
                    name.StartsWith("zh-HK", StringComparison.OrdinalIgnoreCase) ||
                    name.StartsWith("zh-Hant", StringComparison.OrdinalIgnoreCase)
                    ? Language.LANGZHTW
                    : Language.LANGZH;
            }

            // Map Portuguese to Brazilian Portuguese
            if (code == "pt")
            {
                return Language.LANGPTBR;
            }

            Language lang = FromCode(code);

            // Only allow supported languages, otherwise force English
            return IsUiLanguage(lang) ? lang : Language.LANGEN;
        }

        /// <summary>
        /// Gets the English display name for a language <paramref name="code"/> (e.g. "ru" → "Russian").
        /// </summary>
        /// <param name="code">Language code to look up.</param>
        /// <returns>The English display name for <paramref name="code"/>, or the input code when unknown.</returns>
        public static string GetLanguageDisplayName(string code)
        {
            return code switch
            {
                "en" => "English",
                "ru" => "Russian",
                "de" => "German",
                "fr" => "French",
                "es" => "Spanish",
                "it" => "Italian",
                "nl" => "Dutch",
                "pt_br" => "Portuguese",
                "ko" => "Korean",
                "ja" => "Japanese",
                "zh" => "Chinese",
                "zh_tw" => "Tr. Chinese",
                _ => code,
            };
        }

        /* <summary> */
        /* Gets the quad index for the current language's flag icon in MenuExtraButtons. */
        /* </summary> */
        /* public static int GetLanguageFlagQuadIndex()
        {
            return Current switch
            {
                Language.LANGRU => 4, // Russian flag quad
                Language.LANGDE => 5, // German flag quad
                Language.LANGFR => 6, // French flag quad
                Language.LANGEN => 7, // English flag quad
                _ => 15, // Worldwide flag for all other languages
            };
        } */
    }
}
