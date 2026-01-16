using System;

using CutTheRope.Framework.Media;
using CutTheRope.Framework.Platform;
using CutTheRope.Framework.Visual;
using CutTheRope.GameMain;
using CutTheRope.Helpers;

using Microsoft.Xna.Framework;

namespace CutTheRope.Framework.Core
{
    internal class Application : FrameworkTypes
    {
        public static CTRPreferences SharedPreferences()
        {
            return prefs;
        }

        public static CTRResourceMgr SharedResourceMgr()
        {
            return resourceMgr;
        }

        public static RootController SharedRootController()
        {
            root ??= new CTRRootController(null);
            return root;
        }

        public static ApplicationSettings SharedAppSettings()
        {
            return appSettings;
        }

        public static GLCanvas SharedCanvas()
        {
            return _canvas;
        }

        public static SoundMgr SharedSoundMgr()
        {
            soundMgr ??= new SoundMgr();
            return soundMgr;
        }

        public static MovieMgr SharedMovieMgr()
        {
            movieMgr ??= new MovieMgr();
            return movieMgr;
        }

        public virtual ApplicationSettings CreateAppSettings()
        {
            return new ApplicationSettings();
        }

        public virtual GLCanvas CreateCanvas()
        {
            return new GLCanvas().InitWithFrame(new Rectangle((int)0f, (int)0f, (int)SCREEN_WIDTH, (int)SCREEN_HEIGHT));
        }

        public virtual CTRResourceMgr CreateResourceMgr()
        {
            return new CTRResourceMgr();
        }

        public virtual SoundMgr CreateSoundMgr()
        {
            return new SoundMgr();
        }

        public virtual CTRPreferences CreatePreferences()
        {
            return new CTRPreferences();
        }

        public virtual RootController CreateRootController()
        {
            return new CTRRootController(null);
        }

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

        public virtual MovieMgr CreateMovieMgr()
        {
            return new MovieMgr();
        }

        /// <summary>
        /// Gets a font by its resource name.
        /// </summary>
        internal static FontGeneric GetFont(string fontResourceName)
        {
            object resource = SharedResourceMgr().LoadResource(fontResourceName, ResourceMgr.ResourceType.FONT);
            return resource as FontGeneric;
        }

        /// <summary>
        /// Gets a texture by its resource name.
        /// </summary>
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
                $"Texture '{resolvedName}' could not be loaded. Ensure the resource name is correct and the asset is registered in TexturePackerRegistry.json.");
        }

        /// <summary>
        /// Gets a localized string by its key.
        /// </summary>
        internal static string GetString(string stringKey, bool forceEnglish = false)
        {
            if (string.IsNullOrEmpty(stringKey))
            {
                return string.Empty;
            }

            string languageCode = forceEnglish ? "en" : LanguageHelper.CurrentCode;
            return LocalizationManager.GetString(stringKey, languageCode);
        }

        public virtual void UpdateOrientation()
        {
            PORTRAIT_SCREEN_WIDTH = 2560f;
            PORTRAIT_SCREEN_HEIGHT = 1440f;
            SCREEN_WIDTH = PORTRAIT_SCREEN_WIDTH;
            SCREEN_HEIGHT = PORTRAIT_SCREEN_HEIGHT;
        }

        private static CTRPreferences prefs;

        private static readonly CTRResourceMgr resourceMgr = new();

        protected static RootController root;

        private static ApplicationSettings appSettings;

        private static readonly GLCanvas _canvas = new GLCanvas().InitWithFrame(default);

        private static SoundMgr soundMgr;

        private static MovieMgr movieMgr;
    }
}
