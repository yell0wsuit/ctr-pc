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
        public static CTRPreferences sharedPreferences()
        {
            return prefs;
        }

        public static CTRResourceMgr sharedResourceMgr()
        {
            return resourceMgr;
        }

        public static RootController sharedRootController()
        {
            if (root == null)
            {
                root = (CTRRootController)new CTRRootController().initWithParent(null);
            }
            return root;
        }

        public static ApplicationSettings sharedAppSettings()
        {
            return appSettings;
        }

        public static GLCanvas sharedCanvas()
        {
            return _canvas;
        }

        public static SoundMgr sharedSoundMgr()
        {
            if (soundMgr == null)
            {
                soundMgr = new SoundMgr().init();
            }
            return soundMgr;
        }

        public static MovieMgr sharedMovieMgr()
        {
            if (movieMgr == null)
            {
                movieMgr = new MovieMgr();
            }
            return movieMgr;
        }

        public virtual ApplicationSettings createAppSettings()
        {
            return (ApplicationSettings)new ApplicationSettings().init();
        }

        public virtual GLCanvas createCanvas()
        {
            return (GLCanvas)new GLCanvas().initWithFrame(new Rectangle((int)0f, (int)0f, (int)SCREEN_WIDTH, (int)SCREEN_HEIGHT));
        }

        public virtual CTRResourceMgr createResourceMgr()
        {
            return (CTRResourceMgr)new CTRResourceMgr().init();
        }

        public virtual SoundMgr createSoundMgr()
        {
            return new SoundMgr().init();
        }

        public virtual CTRPreferences createPreferences()
        {
            return (CTRPreferences)new CTRPreferences().init();
        }

        public virtual RootController createRootController()
        {
            return (CTRRootController)new CTRRootController().initWithParent(null);
        }

        public virtual void applicationDidFinishLaunching(UIApplication application)
        {
            appSettings = createAppSettings();
            prefs = createPreferences();
            if (appSettings.getBool(7))
            {
                string text = sharedPreferences().getStringForKey("PREFS_LOCALE");
                if (text == null || text.Length == 0)
                {
                    text = (CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "ru") ? "ru" : ((CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "de") ? "de" : ((!(CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "fr")) ? "en" : "fr"));
                }
                appSettings.setString(8, NSS(text));
            }
            updateOrientation();
            IS_IPAD = false;
            IS_RETINA = false;
            root = createRootController();
            soundMgr = createSoundMgr();
            movieMgr = createMovieMgr();
            _canvas.touchDelegate = root;
            root.activate();
        }

        public virtual MovieMgr createMovieMgr()
        {
            return new MovieMgr();
        }

        internal static FontGeneric getFont(int fontResID)
        {
            return (FontGeneric)sharedResourceMgr().loadResource(fontResID, ResourceMgr.ResourceType.FONT);
        }

        internal static CTRTexture2D getTexture(int textureResID)
        {
            return (CTRTexture2D)sharedResourceMgr().loadResource(textureResID, ResourceMgr.ResourceType.IMAGE);
        }

        internal static NSString getString(int strResID)
        {
            return (NSString)sharedResourceMgr().loadResource(strResID, ResourceMgr.ResourceType.STRINGS);
        }

        public virtual void updateOrientation()
        {
            PORTRAIT_SCREEN_WIDTH = 2560f;
            PORTRAIT_SCREEN_HEIGHT = 1440f;
            SCREEN_WIDTH = PORTRAIT_SCREEN_WIDTH;
            SCREEN_HEIGHT = PORTRAIT_SCREEN_HEIGHT;
        }

        private static CTRPreferences prefs;

        private static readonly CTRResourceMgr resourceMgr = (CTRResourceMgr)new CTRResourceMgr().init();

        protected static RootController root;

        private static ApplicationSettings appSettings;

        private static readonly GLCanvas _canvas = (GLCanvas)new GLCanvas().initWithFrame(default(Rectangle));

        private static SoundMgr soundMgr;

        private static MovieMgr movieMgr;
    }
}
