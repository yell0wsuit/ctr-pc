using CutTheRope.game;
using CutTheRope.iframework.media;
using CutTheRope.iframework.platform;
using CutTheRope.iframework.visual;
using CutTheRope.ios;
using System;
using System.Globalization;

namespace CutTheRope.iframework.core
{
    internal class Application : NSObject
    {
        public static CTRPreferences sharedPreferences()
        {
            return Application.prefs;
        }

        public static CTRResourceMgr sharedResourceMgr()
        {
            return Application.resourceMgr;
        }

        public static RootController sharedRootController()
        {
            if (Application.root == null)
            {
                Application.root = (CTRRootController)new CTRRootController().initWithParent(null);
            }
            return Application.root;
        }

        public static ApplicationSettings sharedAppSettings()
        {
            return Application.appSettings;
        }

        public static GLCanvas sharedCanvas()
        {
            return Application._canvas;
        }

        public static SoundMgr sharedSoundMgr()
        {
            if (Application.soundMgr == null)
            {
                Application.soundMgr = new SoundMgr().init();
            }
            return Application.soundMgr;
        }

        public static MovieMgr sharedMovieMgr()
        {
            if (Application.movieMgr == null)
            {
                Application.movieMgr = new MovieMgr();
            }
            return Application.movieMgr;
        }

        public virtual ApplicationSettings createAppSettings()
        {
            return (ApplicationSettings)new ApplicationSettings().init();
        }

        public virtual GLCanvas createCanvas()
        {
            return (GLCanvas)new GLCanvas().initWithFrame(new Rectangle(0f, 0f, FrameworkTypes.SCREEN_WIDTH, FrameworkTypes.SCREEN_HEIGHT));
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
            Application.appSettings = this.createAppSettings();
            Application.prefs = this.createPreferences();
            if (Application.appSettings.getBool(7))
            {
                string text = Application.sharedPreferences().getStringForKey("PREFS_LOCALE");
                if (text == null || text.Length == 0)
                {
                    text = (CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "ru") ? "ru" : ((CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "de") ? "de" : ((!(CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "fr")) ? "en" : "fr"));
                }
                Application.appSettings.setString(8, NSObject.NSS(text));
            }
            this.updateOrientation();
            FrameworkTypes.IS_IPAD = false;
            FrameworkTypes.IS_RETINA = false;
            Application.root = this.createRootController();
            Application.soundMgr = this.createSoundMgr();
            Application.movieMgr = this.createMovieMgr();
            Application._canvas.touchDelegate = Application.root;
            Application.root.activate();
        }

        public virtual MovieMgr createMovieMgr()
        {
            return new MovieMgr();
        }

        internal static FontGeneric getFont(int fontResID)
        {
            return (FontGeneric)Application.sharedResourceMgr().loadResource(fontResID, ResourceMgr.ResourceType.FONT);
        }

        internal static CTRTexture2D getTexture(int textureResID)
        {
            return (CTRTexture2D)Application.sharedResourceMgr().loadResource(textureResID, ResourceMgr.ResourceType.IMAGE);
        }

        internal static NSString getString(int strResID)
        {
            return (NSString)Application.sharedResourceMgr().loadResource(strResID, ResourceMgr.ResourceType.STRINGS);
        }

        public virtual void updateOrientation()
        {
            FrameworkTypes.PORTRAIT_SCREEN_WIDTH = 2560f;
            FrameworkTypes.PORTRAIT_SCREEN_HEIGHT = 1440f;
            FrameworkTypes.SCREEN_WIDTH = FrameworkTypes.PORTRAIT_SCREEN_WIDTH;
            FrameworkTypes.SCREEN_HEIGHT = FrameworkTypes.PORTRAIT_SCREEN_HEIGHT;
        }

        private static CTRPreferences prefs;

        private static CTRResourceMgr resourceMgr = (CTRResourceMgr)new CTRResourceMgr().init();

        protected static RootController root;

        private static ApplicationSettings appSettings;

        private static GLCanvas _canvas = (GLCanvas)new GLCanvas().initWithFrame(default(Rectangle));

        private static SoundMgr soundMgr;

        private static MovieMgr movieMgr;
    }
}
