using System.Collections.Generic;
using System.Globalization;

namespace CutTheRope.Framework
{
    /// <summary>
    /// Centralized utility for language-related conversions and current language state.
    /// </summary>
    public static class LanguageHelper
    {
        // Available languages code for the UI
        // More languages go here
        private static readonly string[] uiLanguageCodes = [
            "en", // English
            "ru", // Russian
            "de", // German
            "fr" // French
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
        /// Gets the current language as an ISO 639-1 two-letter code.
        /// </summary>
        public static string CurrentCode => ToCode(Current);

        /// <summary>
        /// Gets the current language as its integer enum value.
        /// </summary>
        public static int CurrentAsInt => (int)Current;

        /// <summary>
        /// Checks if the given language matches the current language.
        /// </summary>
        public static bool IsCurrent(Language language)
        {
            return Current == language;
        }

        /// <summary>
        /// Checks if the current language matches any of the given languages.
        /// </summary>
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
        /// Checks if the given language is available in the UI.
        /// </summary>
        public static bool IsUiLanguage(Language language)
        {
            return IsUiLanguageCode(ToCode(language));
        }

        /// <summary>
        /// Checks if the given language code is available in the UI.
        /// </summary>
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
        /// Converts a Language enum to its ISO 639-1 two-letter code.
        /// </summary>
        public static string ToCode(Language language)
        {
            return language switch
            {
                Language.LANGEN => "en",
                Language.LANGRU => "ru",
                Language.LANGDE => "de",
                Language.LANGFR => "fr",
                Language.LANGZH => "zh",
                Language.LANGJA => "ja",
                _ => "en",
            };
        }

        /// <summary>
        /// Converts an ISO 639-1 two-letter code to a Language enum.
        /// Returns LANGEN for unrecognized codes.
        /// </summary>
        public static Language FromCode(string code)
        {
            return code switch
            {
                "ru" => Language.LANGRU,
                "de" => Language.LANGDE,
                "fr" => Language.LANGFR,
                "zh" => Language.LANGZH,
                "ja" => Language.LANGJA,
                _ => Language.LANGEN,
            };
        }

        /// <summary>
        /// Detects the appropriate Language from the system's current culture.
        /// </summary>
        public static Language FromSystemCulture()
        {
            string code = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            Language lang = FromCode(code);

            // Only allow supported languages, otherwise force English
            return IsUiLanguage(lang) ? lang : Language.LANGEN;
        }
    }
}
