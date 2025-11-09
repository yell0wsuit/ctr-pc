using CutTheRope.ctr_commons;
using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.ios;
using System;
using System.Collections.Generic;

namespace CutTheRope.game
{
    internal class CTRRootController : RootController
    {
        public static void logEvent(NSString s)
        {
            CTRRootController.logEvent(s.ToString());
        }

        public static void logEvent(string s)
        {
        }

        public virtual void setMap(XMLNode map)
        {
            this.loadedMap = map;
        }

        public virtual XMLNode getMap()
        {
            return this.loadedMap;
        }

        public virtual NSString getMapName()
        {
            return this.mapName;
        }

        public virtual void setMapName(NSString map)
        {
            NSObject.NSREL(this.mapName);
            this.mapName = map;
        }

        public virtual void setMapsList(Dictionary<string, XMLNode> l)
        {
        }

        public virtual int getPack()
        {
            return this.pack;
        }

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

        public override void activate()
        {
            CTRPreferences.isFirstLaunch();
            base.activate();
            this.activateChild(0);
            Application.sharedCanvas().beforeRender();
            this.activeChild().activeView().draw();
            Application.sharedCanvas().afterRender();
        }

        public virtual void deleteMenu()
        {
            ResourceMgr resourceMgr = Application.sharedResourceMgr();
            this.deleteChild(1);
            resourceMgr.freePack(ResDataPhoneFull.PACK_MENU);
            GC.Collect();
        }

        public virtual void disableGameCenter()
        {
        }

        public virtual void enableGameCenter()
        {
        }

        public override void suspend()
        {
            this.suspended = true;
        }

        public override void resume()
        {
            if (!this.inCrystal)
            {
                this.suspended = false;
            }
        }

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
                            menuController3.viewToShow = (this.pack < CTRPreferences.getPacksCount() - 1) ? 5 : 7;
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

        public override void dealloc()
        {
            this.loadedMap = null;
            this.mapName = null;
            base.dealloc();
        }

        public static void checkMapIsValid(NSObject data)
        {
        }

        public static bool isHacked()
        {
            return false;
        }

        public static void setHacked()
        {
            ((CTRRootController)Application.sharedRootController()).hacked = true;
        }

        public static void setInCrystal(bool b)
        {
            ((CTRRootController)Application.sharedRootController()).inCrystal = b;
        }

        public static void openFullVersionPage()
        {
        }

        public virtual void setPack(int p)
        {
            this.pack = p;
        }

        public virtual void setLevel(int l)
        {
            this.level = l;
        }

        public virtual int getLevel()
        {
            return this.level;
        }

        public virtual void setPicker(bool p)
        {
            this.picker = p;
        }

        public virtual bool isPicker()
        {
            return this.picker;
        }

        public virtual void setSurvival(bool s)
        {
            this.survival = s;
        }

        public virtual bool isSurvival()
        {
            return this.survival;
        }

        public static bool isShowGreeting()
        {
            return ((CTRRootController)Application.sharedRootController()).showGreeting;
        }

        public static void setShowGreeting(bool s)
        {
            ((CTRRootController)Application.sharedRootController()).showGreeting = s;
        }

        public static void postAchievementName(string name, string s)
        {
        }

        public static void postAchievementName(NSString name)
        {
            Scorer.postAchievementName(name);
        }

        internal void recreateLoadingController()
        {
            this.deleteChild(2);
            LoadingController c = (LoadingController)new LoadingController().initWithParent(this);
            this.addChildwithID(c, 2);
        }

        public const int NEXT_GAME = 0;

        public const int NEXT_MENU = 1;

        public const int NEXT_PICKER = 2;

        public const int NEXT_PICKER_NEXT_PACK = 3;

        public const int NEXT_PICKER_SHOW_UNLOCK = 4;

        public const int CHILD_START = 0;

        public const int CHILD_MENU = 1;

        public const int CHILD_LOADING = 2;

        public const int CHILD_GAME = 3;

        public int pack;

        private NSString mapName;

        private XMLNode loadedMap;

        private int level;

        private bool picker;

        private bool survival;

        private bool inCrystal;

        private bool showGreeting;

        private bool hacked;
    }
}
