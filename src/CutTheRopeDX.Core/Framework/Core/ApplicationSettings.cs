using System;
using System.Collections.Generic;

namespace CutTheRopeDX.Framework.Core
{
    /// <summary>
    /// Provides access to application-level configuration values such as FPS, orientation, and locale.
    /// </summary>
    internal sealed class ApplicationSettings : FrameworkTypes
    {
        /// <summary>
        /// Gets an integer application setting.
        /// </summary>
        /// <param name="s">The setting identifier (see <see cref="AppSettings"/>).</param>
        /// <returns>The configured FPS or orientation value for the supported setting identifiers.</returns>
        /// <exception cref="NotImplementedException">Thrown when the setting identifier is not supported as an integer value.</exception>
        public static int GetInt(int s)
        {
            return s == 5 ? fps : s != 6 ? throw new NotImplementedException() : (int)orientation;
        }

        /// <summary>
        /// Gets a boolean application setting.
        /// </summary>
        /// <param name="s">The setting identifier (see <see cref="AppSettings"/>).</param>
        /// <returns>The configured boolean value for the requested setting, or <see langword="false" /> if the key is not present.</returns>
        public static bool GetBool(int s)
        {
            _ = DEFAULT_APP_SETTINGS.TryGetValue((AppSettings)s, out bool value);
            return value;
        }

        /// <summary>
        /// Gets a string application setting.
        /// </summary>
        /// <param name="s">The setting identifier (see <see cref="AppSettings"/>).</param>
        /// <returns>The locale code if <c>s</c> is <see cref="AppSettings.APP_SETTING_LOCALE"/>, otherwise an empty string.</returns>
        public string GetString(int s)
        {
            return s != (int)AppSettings.APP_SETTING_LOCALE ? "" : locale ?? LanguageHelper.CurrentCode;
        }

        /// <summary>
        /// Sets a string application setting.
        /// </summary>
        /// <param name="sid">The setting identifier (see <see cref="AppSettings"/>).</param>
        /// <param name="str">The string value to set.</param>
        /// <remarks>
        /// Currently only <see cref="AppSettings.APP_SETTING_LOCALE"/> is supported.
        /// Setting the locale also updates <see cref="LanguageHelper.Current"/>.
        /// </remarks>
        public void SetString(int sid, string str)
        {
            if (sid == (int)AppSettings.APP_SETTING_LOCALE)
            {
                locale = str;
                LanguageHelper.Current = LanguageHelper.FromCode(locale);
            }
        }

        /// <summary>
        /// Default target frame rate.
        /// </summary>
        private static readonly int fps = 60;

        /// <summary>
        /// Default application orientation.
        /// </summary>
        private static readonly ORIENTATION orientation = ORIENTATION.LANDSCAPE_LEFT;

        /// <summary>
        /// Current locale override, if one has been explicitly set.
        /// </summary>
        private string locale;

        /// <summary>
        /// Default boolean values for supported application settings.
        /// </summary>
        private static readonly Dictionary<AppSettings, bool> DEFAULT_APP_SETTINGS = new()
        {
            {
                AppSettings.APP_SETTING_INTERACTION_ENABLED,
                true
            },
            {
                AppSettings.APP_SETTING_MULTITOUCH_ENABLED,
                true
            },
            {
                AppSettings.APP_SETTING_STATUSBAR_HIDDEN,
                true
            },
            {
                AppSettings.APP_SETTING_MAIN_LOOP_TIMERED,
                true
            },
            {
                AppSettings.APP_SETTING_FPS_METER_ENABLED,
                true
            },
            {
                AppSettings.APP_SETTING_LOCALIZATION_ENABLED,
                true
            },
            {
                AppSettings.APP_SETTING_RETINA_SUPPORT,
                false
            },
            {
                AppSettings.APP_SETTING_IPAD_RETINA_SUPPORT,
                false
            }
        };

        /// <summary>
        /// Supported screen orientations.
        /// </summary>
        public enum ORIENTATION
        {
            /// <summary>
            /// Portrait orientation.
            /// </summary>
            PORTRAIT,

            /// <summary>
            /// Portrait orientation rotated 180 degrees.
            /// </summary>
            PORTRAIT_UPSIDE_DOWN,

            /// <summary>
            /// Landscape orientation with the device rotated left.
            /// </summary>
            LANDSCAPE_LEFT,

            /// <summary>
            /// Landscape orientation with the device rotated right.
            /// </summary>
            LANDSCAPE_RIGHT
        }

        /// <summary>
        /// Application setting identifiers used by the legacy framework API.
        /// </summary>
        public enum AppSettings
        {
            /// <summary>
            /// Whether user interaction is enabled.
            /// </summary>
            APP_SETTING_INTERACTION_ENABLED,

            /// <summary>
            /// Whether multi-touch input is enabled.
            /// </summary>
            APP_SETTING_MULTITOUCH_ENABLED,

            /// <summary>
            /// Whether the status bar should be hidden.
            /// </summary>
            APP_SETTING_STATUSBAR_HIDDEN,

            /// <summary>
            /// Whether the main loop is timer-driven.
            /// </summary>
            APP_SETTING_MAIN_LOOP_TIMERED,

            /// <summary>
            /// Whether the FPS meter is enabled.
            /// </summary>
            APP_SETTING_FPS_METER_ENABLED,

            /// <summary>
            /// Frames-per-second setting identifier.
            /// </summary>
            APP_SETTING_FPS,

            /// <summary>
            /// Orientation setting identifier.
            /// </summary>
            APP_SETTING_ORIENTATION,

            /// <summary>
            /// Whether localization is enabled.
            /// </summary>
            APP_SETTING_LOCALIZATION_ENABLED,

            /// <summary>
            /// Locale string setting identifier.
            /// </summary>
            APP_SETTING_LOCALE,

            /// <summary>
            /// Whether Retina support is enabled.
            /// </summary>
            APP_SETTING_RETINA_SUPPORT,

            /// <summary>
            /// Whether iPad Retina support is enabled.
            /// </summary>
            APP_SETTING_IPAD_RETINA_SUPPORT,

            /// <summary>
            /// First identifier reserved for custom settings.
            /// </summary>
            APP_SETTINGS_CUSTOM
        }
    }
}
