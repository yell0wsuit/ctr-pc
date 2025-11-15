using System;
using System.Collections.Generic;
using System.Xml.Linq;

using CutTheRope.commons;
using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.platform;

namespace CutTheRope.game
{
    internal sealed class CTRRootController : RootController
    {
        public static void LogEvent(string s)
        {
        }

        public void SetMap(XElement map)
        {
            loadedMap = map;
        }

        public XElement GetMap()
        {
            return loadedMap;
        }

        public string GetMapName()
        {
            return mapName;
        }

        public void SetMapName(string map)
        {
            mapName = map;
        }

        public static void SetMapsList(Dictionary<string, XElement> l)
        {
        }

        public int GetPack()
        {
            return pack;
        }

        public CTRRootController(ViewController parent)
            : base(parent)
        {
            hacked = false;
            loadedMap = null;
            CTRResourceMgr ctrresourceMgr = Application.SharedResourceMgr();
            ctrresourceMgr.InitLoading();
            ctrresourceMgr.LoadPack(PACK_STARTUP);
            ctrresourceMgr.LoadImmediately();
            StartupController startupController = new(this);
            AddChildwithID(startupController, 0);
            viewTransition = -1;
        }

        public override void Activate()
        {
            _ = CTRPreferences.IsFirstLaunch();
            base.Activate();
            ActivateChild(0);
            Application.SharedCanvas().BeforeRender();
            ActiveChild().ActiveView().Draw();
            GLCanvas.AfterRender();
        }

        public void DeleteMenu()
        {
            CTRResourceMgr resourceMgr = Application.SharedResourceMgr();
            DeleteChild(1);
            resourceMgr.FreePack(PACK_MENU);
            GC.Collect();
        }

        public static void DisableGameCenter()
        {
        }

        public static void EnableGameCenter()
        {
        }

        public override void Suspend()
        {
            suspended = true;
        }

        public override void Resume()
        {
            if (!inCrystal)
            {
                suspended = false;
            }
        }

        public override void OnChildDeactivated(int n)
        {
            base.OnChildDeactivated(n);
            CTRResourceMgr resourceMgr = Application.SharedResourceMgr();
            switch (n)
            {
                case 0:
                    {
                        SetViewTransition(4);
                        LoadingController c2 = new(this);
                        AddChildwithID(c2, 2);
                        MenuController menuController2 = new(this);
                        AddChildwithID(menuController2, 1);
                        DeleteChild(0);
                        resourceMgr.FreePack(PACK_STARTUP);
                        menuController2.viewToShow = 0;
                        if (Preferences.GetBooleanForKey("PREFS_GAME_CENTER_ENABLED"))
                        {
                            EnableGameCenter();
                        }
                        else
                        {
                            DisableGameCenter();
                        }
                        if (Preferences.GetBooleanForKey("IAP_BANNERS"))
                        {
                            AndroidAPI.DisableBanners();
                        }
                        LOG("activate child menu");
                        ActivateChild(1);
                        return;
                    }
                case 1:
                    {
                        DeleteMenu();
                        resourceMgr.resourcesDelegate = (LoadingController)GetChild(2);
                        int[] array = null;
                        switch (pack)
                        {
                            case 0:
                                array = PACK_GAME_01;
                                break;
                            case 1:
                                array = PACK_GAME_02;
                                break;
                            case 2:
                                array = PACK_GAME_03;
                                break;
                            case 3:
                                array = PACK_GAME_04;
                                break;
                            case 4:
                                array = PACK_GAME_05;
                                break;
                            case 5:
                                array = PACK_GAME_06;
                                break;
                            case 6:
                                array = PACK_GAME_07;
                                break;
                            case 7:
                                array = PACK_GAME_08;
                                break;
                            case 8:
                                array = PACK_GAME_09;
                                break;
                            case 9:
                                array = PACK_GAME_10;
                                break;
                            case 10:
                                array = PACK_GAME_11;
                                break;
                            default:
                                break;
                        }
                        resourceMgr.InitLoading();
                        resourceMgr.LoadPack(PACK_GAME);
                        resourceMgr.LoadPack(PACK_GAME_NORMAL);
                        resourceMgr.LoadPack(array);
                        resourceMgr.StartLoading();
                        ((LoadingController)GetChild(2)).nextController = 0;
                        ActivateChild(2);
                        return;
                    }
                case 2:
                    {
                        int nextController = ((LoadingController)GetChild(2)).nextController;
                        if (nextController == 0)
                        {
                            SetShowGreeting(true);
                            GameController c3 = new(this);
                            AddChildwithID(c3, 3);
                            ActivateChild(3);
                            return;
                        }
                        if (nextController - 1 > 3)
                        {
                            return;
                        }
                        MenuController menuController3 = new(this);
                        AddChildwithID(menuController3, 1);
                        resourceMgr.FreePack(PACK_GAME_COVER_01);
                        resourceMgr.FreePack(PACK_GAME_COVER_02);
                        if (!CTRPreferences.IsLiteVersion())
                        {
                            resourceMgr.FreePack(PACK_GAME_COVER_03);
                            resourceMgr.FreePack(PACK_GAME_COVER_04);
                            resourceMgr.FreePack(PACK_GAME_COVER_05);
                            resourceMgr.FreePack(PACK_GAME_COVER_06);
                            resourceMgr.FreePack(PACK_GAME_COVER_07);
                            resourceMgr.FreePack(PACK_GAME_COVER_08);
                            resourceMgr.FreePack(PACK_GAME_COVER_09);
                            resourceMgr.FreePack(PACK_GAME_COVER_10);
                            resourceMgr.FreePack(PACK_GAME_COVER_11);
                        }
                        if (IS_WVGA)
                        {
                            SetViewTransition(4);
                        }
                        if (nextController == 1)
                        {
                            menuController3.viewToShow = 0;
                        }
                        if (nextController is 2 or 4)
                        {
                            menuController3.viewToShow = 6;
                        }
                        if (nextController == 3)
                        {
                            menuController3.viewToShow = pack < CTRPreferences.GetPacksCount() - 1 ? 5 : 7;
                        }
                        ActivateChild(1);
                        if (nextController == 3)
                        {
                            menuController3.ShowNextPack();
                        }
                        GC.Collect();
                        return;
                    }
                case 3:
                    {
                        SaveMgr.Backup();
                        GameController gameController = (GameController)GetChild(3);
                        int exitCode = gameController.exitCode;
                        _ = (GameScene)gameController.GetView(0).GetChild(0);
                        if (exitCode <= 2)
                        {
                            DeleteChild(3);
                            resourceMgr.FreePack(PACK_GAME);
                            resourceMgr.FreePack(PACK_GAME_NORMAL);
                            resourceMgr.FreePack(PACK_GAME_01);
                            resourceMgr.FreePack(PACK_GAME_02);
                            if (!CTRPreferences.IsLiteVersion())
                            {
                                resourceMgr.FreePack(PACK_GAME_03);
                                resourceMgr.FreePack(PACK_GAME_04);
                                resourceMgr.FreePack(PACK_GAME_05);
                                resourceMgr.FreePack(PACK_GAME_06);
                                resourceMgr.FreePack(PACK_GAME_07);
                                resourceMgr.FreePack(PACK_GAME_08);
                                resourceMgr.FreePack(PACK_GAME_09);
                                resourceMgr.FreePack(PACK_GAME_10);
                                resourceMgr.FreePack(PACK_GAME_11);
                            }
                            resourceMgr.resourcesDelegate = (LoadingController)GetChild(2);
                            resourceMgr.InitLoading();
                            resourceMgr.LoadPack(PACK_MENU);
                            resourceMgr.StartLoading();
                            LoadingController loadingController = (LoadingController)GetChild(2);
                            loadingController.nextController = exitCode != 0 ? exitCode != 1 ? 3 : 2 : 1;
                            ActivateChild(2);
                            GC.Collect();
                        }
                        return;
                    }
                default:
                    return;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                loadedMap = null;
                mapName = null;
            }
            base.Dispose(disposing);
        }

        public static void CheckMapIsValid(FrameworkTypes data)
        {
        }

        public static bool IsHacked()
        {
            return false;
        }

        public static void SetHacked()
        {
            ((CTRRootController)Application.SharedRootController()).hacked = true;
        }

        public static void SetInCrystal(bool b)
        {
            ((CTRRootController)Application.SharedRootController()).inCrystal = b;
        }

        public static void OpenFullVersionPage()
        {
        }

        public void SetPack(int p)
        {
            pack = p;
        }

        public void SetLevel(int l)
        {
            level = l;
        }

        public int GetLevel()
        {
            return level;
        }

        public void SetPicker(bool p)
        {
            picker = p;
        }

        public bool IsPicker()
        {
            return picker;
        }

        public void SetSurvival(bool s)
        {
            survival = s;
        }

        public bool IsSurvival()
        {
            return survival;
        }

        public static bool IsShowGreeting()
        {
            return ((CTRRootController)Application.SharedRootController()).showGreeting;
        }

        public static void SetShowGreeting(bool s)
        {
            ((CTRRootController)Application.SharedRootController()).showGreeting = s;
        }

        public static void PostAchievementName(string name, string s)
        {
        }

        public static void PostAchievementName(string name)
        {
            Scorer.PostAchievementName(name);
        }

        internal void RecreateLoadingController()
        {
            DeleteChild(2);
            LoadingController c = new(this);
            AddChildwithID(c, 2);
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

        private string mapName;

        private XElement loadedMap;

        private int level;

        private bool picker;

        private bool survival;

        private bool inCrystal;

        private bool showGreeting;

        private bool hacked;
    }
}
