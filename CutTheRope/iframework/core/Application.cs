using CutTheRope.game;
using CutTheRope.iframework.media;
using CutTheRope.iframework.platform;
using CutTheRope.iframework.visual;
using CutTheRope.ios;
using Microsoft.Xna.Framework;
using System.Globalization;

namespace CutTheRope.iframework.core
{
    internal class Application : NSObject
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
            root ??= (CTRRootController)new CTRRootController().InitWithParent(null);
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
            soundMgr ??= new SoundMgr().Init();
            return soundMgr;
        }

        public static MovieMgr SharedMovieMgr()
        {
            movieMgr ??= new MovieMgr();
            return movieMgr;
        }

        public virtual ApplicationSettings CreateAppSettings()
        {
            return (ApplicationSettings)new ApplicationSettings().Init();
        }

        public virtual GLCanvas CreateCanvas()
        {
            return (GLCanvas)new GLCanvas().InitWithFrame(new Rectangle((int)0f, (int)0f, (int)SCREEN_WIDTH, (int)SCREEN_HEIGHT));
        }

        public virtual CTRResourceMgr CreateResourceMgr()
        {
            return (CTRResourceMgr)new CTRResourceMgr().Init();
        }

        public virtual SoundMgr CreateSoundMgr()
        {
            return new SoundMgr().Init();
        }

        public virtual CTRPreferences CreatePreferences()
        {
            return (CTRPreferences)new CTRPreferences().Init();
        }

        public virtual RootController CreateRootController()
        {
            return (CTRRootController)new CTRRootController().InitWithParent(null);
        }

        public virtual void ApplicationDidFinishLaunching(UIApplication application)
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
                appSettings.SetString(8, NSS(text));
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
            return (FontGeneric)SharedResourceMgr().LoadResource(fontResID, ResourceMgr.ResourceType.FONT);
        }

        internal static CTRTexture2D GetTexture(int textureResID)
        {
            return (CTRTexture2D)SharedResourceMgr().LoadResource(textureResID, ResourceMgr.ResourceType.IMAGE);
        }

        internal static NSString GetString(int strResID)
        {
            return (NSString)SharedResourceMgr().LoadResource(strResID, ResourceMgr.ResourceType.STRINGS);
        }

        public virtual void UpdateOrientation()
        {
            PORTRAIT_SCREEN_WIDTH = 2560f;
            PORTRAIT_SCREEN_HEIGHT = 1440f;
            SCREEN_WIDTH = PORTRAIT_SCREEN_WIDTH;
            SCREEN_HEIGHT = PORTRAIT_SCREEN_HEIGHT;
        }

        private static CTRPreferences prefs;

        private static readonly CTRResourceMgr resourceMgr = (CTRResourceMgr)new CTRResourceMgr().Init();

        protected static RootController root;

        private static ApplicationSettings appSettings;

        private static readonly GLCanvas _canvas = (GLCanvas)new GLCanvas().InitWithFrame(default(Rectangle));

        private static SoundMgr soundMgr;

        private static MovieMgr movieMgr;
    }
}
