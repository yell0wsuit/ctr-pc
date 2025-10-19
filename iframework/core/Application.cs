using System;
using System.Globalization;
using CutTheRope.game;
using CutTheRope.iframework.media;
using CutTheRope.iframework.platform;
using CutTheRope.iframework.visual;
using CutTheRope.ios;

namespace CutTheRope.iframework.core
{
	// Token: 0x02000064 RID: 100
	internal class Application : NSObject
	{
		// Token: 0x060003A2 RID: 930 RVA: 0x0001498E File Offset: 0x00012B8E
		public static CTRPreferences sharedPreferences()
		{
			return Application.prefs;
		}

		// Token: 0x060003A3 RID: 931 RVA: 0x00014995 File Offset: 0x00012B95
		public static CTRResourceMgr sharedResourceMgr()
		{
			return Application.resourceMgr;
		}

		// Token: 0x060003A4 RID: 932 RVA: 0x0001499C File Offset: 0x00012B9C
		public static RootController sharedRootController()
		{
			if (Application.root == null)
			{
				Application.root = (CTRRootController)new CTRRootController().initWithParent(null);
			}
			return Application.root;
		}

		// Token: 0x060003A5 RID: 933 RVA: 0x000149BF File Offset: 0x00012BBF
		public static ApplicationSettings sharedAppSettings()
		{
			return Application.appSettings;
		}

		// Token: 0x060003A6 RID: 934 RVA: 0x000149C6 File Offset: 0x00012BC6
		public static GLCanvas sharedCanvas()
		{
			return Application._canvas;
		}

		// Token: 0x060003A7 RID: 935 RVA: 0x000149CD File Offset: 0x00012BCD
		public static SoundMgr sharedSoundMgr()
		{
			if (Application.soundMgr == null)
			{
				Application.soundMgr = new SoundMgr().init();
			}
			return Application.soundMgr;
		}

		// Token: 0x060003A8 RID: 936 RVA: 0x000149EA File Offset: 0x00012BEA
		public static MovieMgr sharedMovieMgr()
		{
			if (Application.movieMgr == null)
			{
				Application.movieMgr = new MovieMgr();
			}
			return Application.movieMgr;
		}

		// Token: 0x060003A9 RID: 937 RVA: 0x00014A02 File Offset: 0x00012C02
		public virtual ApplicationSettings createAppSettings()
		{
			return (ApplicationSettings)new ApplicationSettings().init();
		}

		// Token: 0x060003AA RID: 938 RVA: 0x00014A13 File Offset: 0x00012C13
		public virtual GLCanvas createCanvas()
		{
			return (GLCanvas)new GLCanvas().initWithFrame(new Rectangle(0f, 0f, FrameworkTypes.SCREEN_WIDTH, FrameworkTypes.SCREEN_HEIGHT));
		}

		// Token: 0x060003AB RID: 939 RVA: 0x00014A3D File Offset: 0x00012C3D
		public virtual CTRResourceMgr createResourceMgr()
		{
			return (CTRResourceMgr)new CTRResourceMgr().init();
		}

		// Token: 0x060003AC RID: 940 RVA: 0x00014A4E File Offset: 0x00012C4E
		public virtual SoundMgr createSoundMgr()
		{
			return new SoundMgr().init();
		}

		// Token: 0x060003AD RID: 941 RVA: 0x00014A5A File Offset: 0x00012C5A
		public virtual CTRPreferences createPreferences()
		{
			return (CTRPreferences)new CTRPreferences().init();
		}

		// Token: 0x060003AE RID: 942 RVA: 0x00014A6B File Offset: 0x00012C6B
		public virtual RootController createRootController()
		{
			return (CTRRootController)new CTRRootController().initWithParent(null);
		}

		// Token: 0x060003AF RID: 943 RVA: 0x00014A80 File Offset: 0x00012C80
		public virtual void applicationDidFinishLaunching(UIApplication application)
		{
			Application.appSettings = this.createAppSettings();
			Application.prefs = this.createPreferences();
			if (Application.appSettings.getBool(7))
			{
				string text = Application.sharedPreferences().getStringForKey("PREFS_LOCALE");
				if (text == null || text.Length == 0)
				{
					text = ((CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "ru") ? "ru" : ((CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "de") ? "de" : ((!(CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "fr")) ? "en" : "fr")));
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

		// Token: 0x060003B0 RID: 944 RVA: 0x00014B88 File Offset: 0x00012D88
		public virtual MovieMgr createMovieMgr()
		{
			return new MovieMgr();
		}

		// Token: 0x060003B1 RID: 945 RVA: 0x00014B8F File Offset: 0x00012D8F
		internal static FontGeneric getFont(int fontResID)
		{
			return (FontGeneric)Application.sharedResourceMgr().loadResource(fontResID, ResourceMgr.ResourceType.FONT);
		}

		// Token: 0x060003B2 RID: 946 RVA: 0x00014BA2 File Offset: 0x00012DA2
		internal static Texture2D getTexture(int textureResID)
		{
			return (Texture2D)Application.sharedResourceMgr().loadResource(textureResID, ResourceMgr.ResourceType.IMAGE);
		}

		// Token: 0x060003B3 RID: 947 RVA: 0x00014BB5 File Offset: 0x00012DB5
		internal static NSString getString(int strResID)
		{
			return (NSString)Application.sharedResourceMgr().loadResource(strResID, ResourceMgr.ResourceType.STRINGS);
		}

		// Token: 0x060003B4 RID: 948 RVA: 0x00014BC8 File Offset: 0x00012DC8
		public virtual void updateOrientation()
		{
			FrameworkTypes.PORTRAIT_SCREEN_WIDTH = 2560f;
			FrameworkTypes.PORTRAIT_SCREEN_HEIGHT = 1440f;
			FrameworkTypes.SCREEN_WIDTH = FrameworkTypes.PORTRAIT_SCREEN_WIDTH;
			FrameworkTypes.SCREEN_HEIGHT = FrameworkTypes.PORTRAIT_SCREEN_HEIGHT;
		}

		// Token: 0x0400029A RID: 666
		private static CTRPreferences prefs;

		// Token: 0x0400029B RID: 667
		private static CTRResourceMgr resourceMgr = (CTRResourceMgr)new CTRResourceMgr().init();

		// Token: 0x0400029C RID: 668
		protected static RootController root;

		// Token: 0x0400029D RID: 669
		private static ApplicationSettings appSettings;

		// Token: 0x0400029E RID: 670
		private static GLCanvas _canvas = (GLCanvas)new GLCanvas().initWithFrame(default(Rectangle));

		// Token: 0x0400029F RID: 671
		private static SoundMgr soundMgr;

		// Token: 0x040002A0 RID: 672
		private static MovieMgr movieMgr;
	}
}
