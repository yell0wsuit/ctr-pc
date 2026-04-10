using System;

using CutTheRopeDX.Framework.Media;
using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;
using CutTheRopeDX.Helpers;

namespace CutTheRopeDX.Framework.Core
{
    /// <summary>
    /// Application bootstrap and shared-service access point for core framework systems.
    /// </summary>
    internal class Application : FrameworkTypes
    {
        /// <summary>
        /// Returns the shared preferences instance.
        /// </summary>
        /// <returns>Shared preferences instance.</returns>
        public static CTRPreferences SharedPreferences()
        {
            return prefs;
        }

        /// <summary>
        /// Returns the shared resource manager instance.
        /// </summary>
        /// <returns>Shared resource manager.</returns>
        public static CTRResourceMgr SharedResourceMgr()
        {
            return resourceMgr;
        }

        /// <summary>
        /// Returns the shared root controller, creating it on first access.
        /// </summary>
        /// <returns>Shared root controller.</returns>
        public static RootController SharedRootController()
        {
            root ??= new CTRRootController(null);
            return root;
        }

        /// <summary>
        /// Returns the shared application-settings instance.
        /// </summary>
        /// <returns>Shared application settings.</returns>
        public static ApplicationSettings SharedAppSettings()
        {
            return appSettings;
        }

        /// <summary>
        /// Returns the shared rendering canvas.
        /// </summary>
        /// <returns>Shared canvas instance.</returns>
        public static GLCanvas SharedCanvas()
        {
            return _canvas;
        }

        /// <summary>
        /// Returns the shared sound manager, creating it on first access.
        /// </summary>
        /// <returns>Shared sound manager.</returns>
        public static SoundMgr SharedSoundMgr()
        {
            soundMgr ??= new SoundMgr();
            return soundMgr;
        }

        /// <summary>
        /// Returns the shared movie manager, creating it on first access.
        /// </summary>
        /// <returns>Shared movie manager.</returns>
        public static MovieMgr SharedMovieMgr()
        {
            movieMgr ??= new MovieMgr();
            return movieMgr;
        }

        /// <summary>
        /// Creates the application-settings instance used at startup.
        /// </summary>
        /// <returns>New application-settings instance.</returns>
        public virtual ApplicationSettings CreateAppSettings()
        {
            return new ApplicationSettings();
        }

        /// <summary>
        /// Creates the rendering canvas used by the application.
        /// </summary>
        /// <returns>Initialized canvas instance.</returns>
        public virtual GLCanvas CreateCanvas()
        {
            return new GLCanvas().InitWithFrame();
        }

        /// <summary>
        /// Creates the resource manager used by the application.
        /// </summary>
        /// <returns>New resource manager instance.</returns>
        public virtual CTRResourceMgr CreateResourceMgr()
        {
            return new CTRResourceMgr();
        }

        /// <summary>
        /// Creates the shared sound manager instance.
        /// </summary>
        /// <returns>New sound manager instance.</returns>
        public static SoundMgr CreateSoundMgr()
        {
            return new SoundMgr();
        }

        /// <summary>
        /// Creates the preferences store used by the application.
        /// </summary>
        /// <returns>New preferences instance.</returns>
        public virtual CTRPreferences CreatePreferences()
        {
            return new CTRPreferences();
        }

        /// <summary>
        /// Creates the root controller that will own the active controller stack.
        /// </summary>
        /// <returns>New root controller instance.</returns>
        public virtual RootController CreateRootController()
        {
            return new CTRRootController(null);
        }

        /// <summary>
        /// Performs application startup by creating shared services, loading preferences,
        /// configuring orientation, and activating the root controller.
        /// </summary>
        public virtual void ApplicationDidFinishLaunching()
        {
            appSettings = CreateAppSettings();
            prefs = CreatePreferences();
            if (ApplicationSettings.GetBool(7))
            {
                string text = Preferences.GetStringForKey("PREFS_LOCALE");
                if (string.IsNullOrEmpty(text))
                {
                    text = LanguageHelper.ToCode(LanguageHelper.FromSystemCulture());
                }
                appSettings.SetString(8, text);
            }
            UpdateOrientation();
            IS_IPAD = false;
            IS_RETINA = false;
            root = CreateRootController();
            soundMgr = CreateSoundMgr();
            movieMgr = CreateMovieMgr();
            _canvas.touchDelegate = root;
            root.Activate();
        }

        /// <summary>
        /// Creates the movie manager used by the application.
        /// </summary>
        /// <returns>New movie manager instance.</returns>
        public virtual MovieMgr CreateMovieMgr()
        {
            return new MovieMgr();
        }

        /// <summary>
        /// Gets a font by its resource name.
        /// </summary>
        /// <param name="fontResourceName">Logical font resource name.</param>
        /// <returns>Loaded font resource, or <see langword="null" /> if loading failed.</returns>
        internal static FontGeneric GetFont(string fontResourceName)
        {
            object resource = SharedResourceMgr().LoadResource(fontResourceName, ResourceMgr.ResourceType.FONT);
            return resource as FontGeneric;
        }

        /// <summary>
        /// Gets a texture by its resource name.
        /// </summary>
        /// <param name="textureResourceName">Logical texture resource name.</param>
        /// <returns>Loaded texture resource.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="textureResourceName"/> is <see langword="null"/> or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the texture could not be loaded.</exception>
        internal static CTRTexture2D GetTexture(string textureResourceName)
        {
            if (string.IsNullOrEmpty(textureResourceName))
            {
                throw new ArgumentException("Texture resource name cannot be null or empty.", nameof(textureResourceName));
            }

            object resource = SharedResourceMgr().LoadResource(textureResourceName, ResourceMgr.ResourceType.IMAGE);

            if (resource is CTRTexture2D texture)
            {
                return texture;
            }

            string localizedName = CTRResourceMgr.HandleLocalizedResource(textureResourceName);
            string resolvedName = string.Equals(textureResourceName, localizedName, StringComparison.Ordinal)
                ? textureResourceName
                : string.IsNullOrEmpty(localizedName)
                    ? textureResourceName
                    : $"{textureResourceName} (localized: {localizedName})";

            throw new InvalidOperationException(
                $"Texture '{resolvedName}' could not be loaded. Ensure the resource name is correct and the JSON+PNG pair exists in content/images/.");
        }

        /// <summary>
        /// Gets a localized string by its key.
        /// </summary>
        /// <param name="stringKey">Localization key to resolve.</param>
        /// <param name="forceEnglish"><see langword="true" /> to always use English; otherwise uses the current language.</param>
        /// <returns>Localized string, or an empty string when the key is <see langword="null"/> or empty.</returns>
        internal static string GetString(string stringKey, bool forceEnglish = false)
        {
            if (string.IsNullOrEmpty(stringKey))
            {
                return string.Empty;
            }

            string languageCode = forceEnglish ? "en" : LanguageHelper.CurrentCode;
            return LocalizationManager.GetString(stringKey, languageCode);
        }

        /// <summary>
        /// Updates the framework's logical screen dimensions for the current orientation.
        /// The base implementation keeps the game in its default landscape layout.
        /// </summary>
        public virtual void UpdateOrientation()
        {
            PORTRAIT_SCREEN_WIDTH = 2560f;
            PORTRAIT_SCREEN_HEIGHT = 1440f;
            SCREEN_WIDTH = PORTRAIT_SCREEN_WIDTH;
            SCREEN_HEIGHT = PORTRAIT_SCREEN_HEIGHT;
        }

        /// <summary>
        /// Shared preferences instance.
        /// </summary>
        private static CTRPreferences prefs;

        /// <summary>
        /// Shared resource manager instance.
        /// </summary>
        private static readonly CTRResourceMgr resourceMgr = new();

        /// <summary>
        /// Shared root controller instance.
        /// </summary>
        protected static RootController root;

        /// <summary>
        /// Shared application-settings instance.
        /// </summary>
        private static ApplicationSettings appSettings;

        /// <summary>
        /// Shared rendering canvas instance.
        /// </summary>
        private static readonly GLCanvas _canvas = new GLCanvas().InitWithFrame();

        /// <summary>
        /// Shared sound manager instance.
        /// </summary>
        private static SoundMgr soundMgr;

        /// <summary>
        /// Shared movie manager instance.
        /// </summary>
        private static MovieMgr movieMgr;
    }
}
