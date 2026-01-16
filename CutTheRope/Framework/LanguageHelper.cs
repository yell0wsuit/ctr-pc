using System.Globalization;

namespace CutTheRope.Framework
{
    /// <summary>
    /// Centralized utility for language-related conversions.
    /// </summary>
    public static class LanguageHelper
    {
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
            return FromCode(CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
        }
    }
}
