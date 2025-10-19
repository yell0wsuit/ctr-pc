using System;
using System.Collections.Generic;
using CutTheRope.ctr_commons;
using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.ios;

namespace CutTheRope.game
{
	// Token: 0x02000079 RID: 121
	internal class CTRRootController : RootController
	{
		// Token: 0x060004AE RID: 1198 RVA: 0x0001B1C5 File Offset: 0x000193C5
		public static void logEvent(NSString s)
		{
			CTRRootController.logEvent(s.ToString());
		}

		// Token: 0x060004AF RID: 1199 RVA: 0x0001B1D2 File Offset: 0x000193D2
		public static void logEvent(string s)
		{
		}

		// Token: 0x060004B0 RID: 1200 RVA: 0x0001B1D4 File Offset: 0x000193D4
		public virtual void setMap(XMLNode map)
		{
			this.loadedMap = map;
		}

		// Token: 0x060004B1 RID: 1201 RVA: 0x0001B1DD File Offset: 0x000193DD
		public virtual XMLNode getMap()
		{
			return this.loadedMap;
		}

		// Token: 0x060004B2 RID: 1202 RVA: 0x0001B1E5 File Offset: 0x000193E5
		public virtual NSString getMapName()
		{
			return this.mapName;
		}

		// Token: 0x060004B3 RID: 1203 RVA: 0x0001B1ED File Offset: 0x000193ED
		public virtual void setMapName(NSString map)
		{
			NSObject.NSREL(this.mapName);
			this.mapName = map;
		}

		// Token: 0x060004B4 RID: 1204 RVA: 0x0001B201 File Offset: 0x00019401
		public virtual void setMapsList(Dictionary<string, XMLNode> l)
		{
		}

		// Token: 0x060004B5 RID: 1205 RVA: 0x0001B203 File Offset: 0x00019403
		public virtual int getPack()
		{
			return this.pack;
		}

		// Token: 0x060004B6 RID: 1206 RVA: 0x0001B20C File Offset: 0x0001940C
		public override NSObject initWithParent(ViewController p)
		{
			if (base.initWithParent(p) != null)
			{
				this.hacked = false;
				this.loadedMap = null;
				CTRResourceMgr ctrresourceMgr = Application.sharedResourceMgr();
				ctrresourceMgr.initLoading();
				ctrresourceMgr.loadPack(ResDataPhoneFull.PACK_STARTUP);
				ctrresourceMgr.loadImmediately();
				StartupController startupController = (StartupController)new StartupController().initWithParent(this);
				this.addChildwithID(startupController, 0);
				NSObject.NSREL(startupController);
				this.viewTransition = -1;
			}
			return this;
		}

		// Token: 0x060004B7 RID: 1207 RVA: 0x0001B272 File Offset: 0x00019472
		public override void activate()
		{
			CTRPreferences.isFirstLaunch();
			base.activate();
			this.activateChild(0);
			Application.sharedCanvas().beforeRender();
			this.activeChild().activeView().draw();
			Application.sharedCanvas().afterRender();
		}

		// Token: 0x060004B8 RID: 1208 RVA: 0x0001B2AB File Offset: 0x000194AB
		public virtual void deleteMenu()
		{
			ResourceMgr resourceMgr = Application.sharedResourceMgr();
			this.deleteChild(1);
			resourceMgr.freePack(ResDataPhoneFull.PACK_MENU);
			GC.Collect();
		}

		// Token: 0x060004B9 RID: 1209 RVA: 0x0001B2C8 File Offset: 0x000194C8
		public virtual void disableGameCenter()
		{
		}

		// Token: 0x060004BA RID: 1210 RVA: 0x0001B2CA File Offset: 0x000194CA
		public virtual void enableGameCenter()
		{
		}

		// Token: 0x060004BB RID: 1211 RVA: 0x0001B2CC File Offset: 0x000194CC
		public override void suspend()
		{
			this.suspended = true;
		}

		// Token: 0x060004BC RID: 1212 RVA: 0x0001B2D5 File Offset: 0x000194D5
		public override void resume()
		{
			if (!this.inCrystal)
			{
				this.suspended = false;
			}
		}

		// Token: 0x060004BD RID: 1213 RVA: 0x0001B2E8 File Offset: 0x000194E8
		public override void onChildDeactivated(int n)
		{
			base.onChildDeactivated(n);
			ResourceMgr resourceMgr = Application.sharedResourceMgr();
			switch (n)
			{
			case 0:
			{
				this.setViewTransition(4);
				LoadingController c2 = (LoadingController)new LoadingController().initWithParent(this);
				this.addChildwithID(c2, 2);
				MenuController menuController2 = (MenuController)new MenuController().initWithParent(this);
				this.addChildwithID(menuController2, 1);
				this.deleteChild(0);
				resourceMgr.freePack(ResDataPhoneFull.PACK_STARTUP);
				menuController2.viewToShow = 0;
				if (Preferences._getBooleanForKey("PREFS_GAME_CENTER_ENABLED"))
				{
					this.enableGameCenter();
				}
				else
				{
					this.disableGameCenter();
				}
				if (Preferences._getBooleanForKey("IAP_BANNERS"))
				{
					FrameworkTypes.AndroidAPI.disableBanners();
				}
				FrameworkTypes._LOG("activate child menu");
				this.activateChild(1);
				return;
			}
			case 1:
			{
				this.deleteMenu();
				resourceMgr.resourcesDelegate = (LoadingController)this.getChild(2);
				int[] array = null;
				switch (this.pack)
				{
				case 0:
					array = ResDataPhoneFull.PACK_GAME_01;
					break;
				case 1:
					array = ResDataPhoneFull.PACK_GAME_02;
					break;
				case 2:
					array = ResDataPhoneFull.PACK_GAME_03;
					break;
				case 3:
					array = ResDataPhoneFull.PACK_GAME_04;
					break;
				case 4:
					array = ResDataPhoneFull.PACK_GAME_05;
					break;
				case 5:
					array = ResDataPhoneFull.PACK_GAME_06;
					break;
				case 6:
					array = ResDataPhoneFull.PACK_GAME_07;
					break;
				case 7:
					array = ResDataPhoneFull.PACK_GAME_08;
					break;
				case 8:
					array = ResDataPhoneFull.PACK_GAME_09;
					break;
				case 9:
					array = ResDataPhoneFull.PACK_GAME_10;
					break;
				case 10:
					array = ResDataPhoneFull.PACK_GAME_11;
					break;
				}
				resourceMgr.initLoading();
				resourceMgr.loadPack(ResDataPhoneFull.PACK_GAME);
				resourceMgr.loadPack(ResDataPhoneFull.PACK_GAME_NORMAL);
				resourceMgr.loadPack(array);
				resourceMgr.startLoading();
				((LoadingController)this.getChild(2)).nextController = 0;
				this.activateChild(2);
				return;
			}
			case 2:
			{
				int nextController = ((LoadingController)this.getChild(2)).nextController;
				if (nextController == 0)
				{
					CTRRootController.setShowGreeting(true);
					GameController c3 = (GameController)new GameController().initWithParent(this);
					this.addChildwithID(c3, 3);
					this.activateChild(3);
					return;
				}
				if (nextController - 1 > 3)
				{
					return;
				}
				MenuController menuController3 = (MenuController)new MenuController().initWithParent(this);
				this.addChildwithID(menuController3, 1);
				resourceMgr.freePack(ResDataPhoneFull.PACK_GAME_COVER_01);
				resourceMgr.freePack(ResDataPhoneFull.PACK_GAME_COVER_02);
				if (!CTRPreferences.isLiteVersion())
				{
					resourceMgr.freePack(ResDataPhoneFull.PACK_GAME_COVER_03);
					resourceMgr.freePack(ResDataPhoneFull.PACK_GAME_COVER_04);
					resourceMgr.freePack(ResDataPhoneFull.PACK_GAME_COVER_05);
					resourceMgr.freePack(ResDataPhoneFull.PACK_GAME_COVER_06);
					resourceMgr.freePack(ResDataPhoneFull.PACK_GAME_COVER_07);
					resourceMgr.freePack(ResDataPhoneFull.PACK_GAME_COVER_08);
					resourceMgr.freePack(ResDataPhoneFull.PACK_GAME_COVER_09);
					resourceMgr.freePack(ResDataPhoneFull.PACK_GAME_COVER_10);
					resourceMgr.freePack(ResDataPhoneFull.PACK_GAME_COVER_11);
				}
				if (FrameworkTypes.IS_WVGA)
				{
					this.setViewTransition(4);
				}
				if (nextController == 1)
				{
					menuController3.viewToShow = 0;
				}
				if (nextController == 2 || nextController == 4)
				{
					menuController3.viewToShow = 6;
				}
				if (nextController == 3)
				{
					menuController3.viewToShow = ((this.pack < CTRPreferences.getPacksCount() - 1) ? 5 : 7);
				}
				this.activateChild(1);
				if (nextController == 3)
				{
					menuController3.showNextPack();
				}
				GC.Collect();
				return;
			}
			case 3:
			{
				SaveMgr.backup();
				GameController gameController = (GameController)this.getChild(3);
				int exitCode = gameController.exitCode;
				GameScene gameScene = (GameScene)gameController.getView(0).getChild(0);
				if (exitCode <= 2)
				{
					this.deleteChild(3);
					resourceMgr.freePack(ResDataPhoneFull.PACK_GAME);
					resourceMgr.freePack(ResDataPhoneFull.PACK_GAME_NORMAL);
					resourceMgr.freePack(ResDataPhoneFull.PACK_GAME_01);
					resourceMgr.freePack(ResDataPhoneFull.PACK_GAME_02);
					if (!CTRPreferences.isLiteVersion())
					{
						resourceMgr.freePack(ResDataPhoneFull.PACK_GAME_03);
						resourceMgr.freePack(ResDataPhoneFull.PACK_GAME_04);
						resourceMgr.freePack(ResDataPhoneFull.PACK_GAME_05);
						resourceMgr.freePack(ResDataPhoneFull.PACK_GAME_06);
						resourceMgr.freePack(ResDataPhoneFull.PACK_GAME_07);
						resourceMgr.freePack(ResDataPhoneFull.PACK_GAME_08);
						resourceMgr.freePack(ResDataPhoneFull.PACK_GAME_09);
						resourceMgr.freePack(ResDataPhoneFull.PACK_GAME_10);
						resourceMgr.freePack(ResDataPhoneFull.PACK_GAME_11);
					}
					resourceMgr.resourcesDelegate = (LoadingController)this.getChild(2);
					resourceMgr.initLoading();
					resourceMgr.loadPack(ResDataPhoneFull.PACK_MENU);
					resourceMgr.startLoading();
					LoadingController loadingController = (LoadingController)this.getChild(2);
					if (exitCode != 0)
					{
						if (exitCode != 1)
						{
							loadingController.nextController = 3;
						}
						else
						{
							loadingController.nextController = 2;
						}
					}
					else
					{
						loadingController.nextController = 1;
					}
					this.activateChild(2);
					GC.Collect();
				}
				return;
			}
			default:
				return;
			}
		}

		// Token: 0x060004BE RID: 1214 RVA: 0x0001B71A File Offset: 0x0001991A
		public override void dealloc()
		{
			this.loadedMap = null;
			this.mapName = null;
			base.dealloc();
		}

		// Token: 0x060004BF RID: 1215 RVA: 0x0001B730 File Offset: 0x00019930
		public static void checkMapIsValid(NSObject data)
		{
		}

		// Token: 0x060004C0 RID: 1216 RVA: 0x0001B732 File Offset: 0x00019932
		public static bool isHacked()
		{
			return false;
		}

		// Token: 0x060004C1 RID: 1217 RVA: 0x0001B735 File Offset: 0x00019935
		public static void setHacked()
		{
			((CTRRootController)Application.sharedRootController()).hacked = true;
		}

		// Token: 0x060004C2 RID: 1218 RVA: 0x0001B747 File Offset: 0x00019947
		public static void setInCrystal(bool b)
		{
			((CTRRootController)Application.sharedRootController()).inCrystal = b;
		}

		// Token: 0x060004C3 RID: 1219 RVA: 0x0001B759 File Offset: 0x00019959
		public static void openFullVersionPage()
		{
		}

		// Token: 0x060004C4 RID: 1220 RVA: 0x0001B75B File Offset: 0x0001995B
		public virtual void setPack(int p)
		{
			this.pack = p;
		}

		// Token: 0x060004C5 RID: 1221 RVA: 0x0001B764 File Offset: 0x00019964
		public virtual void setLevel(int l)
		{
			this.level = l;
		}

		// Token: 0x060004C6 RID: 1222 RVA: 0x0001B76D File Offset: 0x0001996D
		public virtual int getLevel()
		{
			return this.level;
		}

		// Token: 0x060004C7 RID: 1223 RVA: 0x0001B775 File Offset: 0x00019975
		public virtual void setPicker(bool p)
		{
			this.picker = p;
		}

		// Token: 0x060004C8 RID: 1224 RVA: 0x0001B77E File Offset: 0x0001997E
		public virtual bool isPicker()
		{
			return this.picker;
		}

		// Token: 0x060004C9 RID: 1225 RVA: 0x0001B786 File Offset: 0x00019986
		public virtual void setSurvival(bool s)
		{
			this.survival = s;
		}

		// Token: 0x060004CA RID: 1226 RVA: 0x0001B78F File Offset: 0x0001998F
		public virtual bool isSurvival()
		{
			return this.survival;
		}

		// Token: 0x060004CB RID: 1227 RVA: 0x0001B797 File Offset: 0x00019997
		public static bool isShowGreeting()
		{
			return ((CTRRootController)Application.sharedRootController()).showGreeting;
		}

		// Token: 0x060004CC RID: 1228 RVA: 0x0001B7A8 File Offset: 0x000199A8
		public static void setShowGreeting(bool s)
		{
			((CTRRootController)Application.sharedRootController()).showGreeting = s;
		}

		// Token: 0x060004CD RID: 1229 RVA: 0x0001B7BA File Offset: 0x000199BA
		public static void postAchievementName(string name, string s)
		{
		}

		// Token: 0x060004CE RID: 1230 RVA: 0x0001B7BC File Offset: 0x000199BC
		public static void postAchievementName(NSString name)
		{
			Scorer.postAchievementName(name);
		}

		// Token: 0x060004CF RID: 1231 RVA: 0x0001B7C4 File Offset: 0x000199C4
		internal void recreateLoadingController()
		{
			this.deleteChild(2);
			LoadingController c = (LoadingController)new LoadingController().initWithParent(this);
			this.addChildwithID(c, 2);
		}

		// Token: 0x0400036E RID: 878
		public const int NEXT_GAME = 0;

		// Token: 0x0400036F RID: 879
		public const int NEXT_MENU = 1;

		// Token: 0x04000370 RID: 880
		public const int NEXT_PICKER = 2;

		// Token: 0x04000371 RID: 881
		public const int NEXT_PICKER_NEXT_PACK = 3;

		// Token: 0x04000372 RID: 882
		public const int NEXT_PICKER_SHOW_UNLOCK = 4;

		// Token: 0x04000373 RID: 883
		public const int CHILD_START = 0;

		// Token: 0x04000374 RID: 884
		public const int CHILD_MENU = 1;

		// Token: 0x04000375 RID: 885
		public const int CHILD_LOADING = 2;

		// Token: 0x04000376 RID: 886
		public const int CHILD_GAME = 3;

		// Token: 0x04000377 RID: 887
		public int pack;

		// Token: 0x04000378 RID: 888
		private NSString mapName;

		// Token: 0x04000379 RID: 889
		private XMLNode loadedMap;

		// Token: 0x0400037A RID: 890
		private int level;

		// Token: 0x0400037B RID: 891
		private bool picker;

		// Token: 0x0400037C RID: 892
		private bool survival;

		// Token: 0x0400037D RID: 893
		private bool inCrystal;

		// Token: 0x0400037E RID: 894
		private bool showGreeting;

		// Token: 0x0400037F RID: 895
		private bool hacked;
	}
}
