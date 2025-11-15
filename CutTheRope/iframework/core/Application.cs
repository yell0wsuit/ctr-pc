using System.Globalization;

using CutTheRope.game;
using CutTheRope.iframework.media;
using CutTheRope.iframework.platform;
using CutTheRope.iframework.visual;

using Microsoft.Xna.Framework;

namespace CutTheRope.iframework.core
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
                if (text == null || text.Length == 0)
                {
                    text = CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "ru" ? "ru" : CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "de" ? "de" : !(CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "fr") ? "en" : "fr";
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

        internal static FontGeneric GetFont(int fontResID)
        {
            object resource = SharedResourceMgr().LoadResource(fontResID, ResourceMgr.ResourceType.FONT);
            return resource as FontGeneric;
        }

        internal static CTRTexture2D GetTexture(int textureResID)
        {
            object resource = SharedResourceMgr().LoadResource(textureResID, ResourceMgr.ResourceType.IMAGE);
            return resource as CTRTexture2D;
        }

        internal static string GetString(int strResID)
        {
            object resource = SharedResourceMgr().LoadResource(strResID, ResourceMgr.ResourceType.STRINGS);
            return (resource as string) ?? string.Empty;
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
