using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;

using CutTheRope.Commons;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Platform;
using CutTheRope.Helpers;

namespace CutTheRope.GameMain
{
    internal sealed class CTRRootController : RootController
    {
        public static void LogEvent(string _)
        {
        }

        /// <summary>
        /// Stores the currently prepared gameplay map XML on the root controller.
        /// </summary>
        /// <param name="map">The parsed map XML that should be treated as current.</param>
        public void SetMap(XElement map)
        {
            loadedMap = map;
        }

        /// <summary>
        /// Gets the parsed gameplay map XML currently cached on the root controller.
        /// </summary>
        /// <returns>The current map XML, or <see langword="null"/> when no map is loaded.</returns>
        public XElement GetMap()
        {
            return loadedMap;
        }

        /// <summary>
        /// Gets the current map filename tracked for reload and transition flows.
        /// </summary>
        /// <returns>The current map filename, or <see langword="null"/> when none has been assigned.</returns>
        public string GetMapName()
        {
            return mapName;
        }

        /// <summary>
        /// Stores the current map filename tracked for reload and transition flows.
        /// </summary>
        /// <param name="map">The map filename to persist on the root controller.</param>
        public void SetMapName(string map)
        {
            mapName = map;
        }

        /// <summary>
        /// Synchronously ensures the resources required by a map are loaded, then stores the map as current.
        /// </summary>
        /// <param name="map">The parsed map XML to prepare.</param>
        /// <param name="newMapName">Optional map filename to persist on the root controller.</param>
        public void PrepareMapAndEnsureResources(XElement map, string newMapName)
        {
            if (map == null)
            {
                return;
            }

            StopGameplayPrefetch();

            string[] levelResources = LevelResourceScanner.GetRequiredResources(map, pack);
            TrackSessionResources(levelResources);

            CTRResourceMgr resourceMgr = Application.SharedResourceMgr();
            resourceMgr.InitLoading();
            resourceMgr.LoadPack(levelResources);
            resourceMgr.LoadImmediately();

            SetMap(map);
            if (!string.IsNullOrWhiteSpace(newMapName))
            {
                SetMapName(newMapName);
            }

            StartBoxResourceScanIfNeeded();
            QueueOrPollBoxPrefetch();
        }

        public static void SetMapsList(Dictionary<string, XElement> _)
        {
        }

        public int GetPack()
        {
            return pack;
        }

        public CTRRootController(ViewController parent)
            : base(parent)
        {
            loadedMap = null;
            CTRResourceMgr ctrresourceMgr = Application.SharedResourceMgr();
            ctrresourceMgr.InitLoading();
            ctrresourceMgr.LoadPack(PackStartup);
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
            Application.SharedMovieMgr().delegateMovieMgrDelegate = null;
            resourceMgr.FreePack(PackMenu);
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
                        resourceMgr.FreePack(PackStartup);
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
                        LOG();
                        ActivateChild(1);
                        //Show menu presence after loading screen
                        Game1.RPC?.MenuPresence();
                        return;
                    }
                case 1:
                    {
                        DeleteMenu();
                        resourceMgr.resourcesDelegate = (LoadingController)GetChild(2);
                        ResetGameplayResourceSession();
                        EnsureCurrentMapLoaded();
                        string[] levelResources = LevelResourceScanner.GetRequiredResources(loadedMap, pack);
                        TrackSessionResources(levelResources);
                        StartBoxResourceScanIfNeeded();
                        resourceMgr.InitLoading();
                        resourceMgr.LoadPack(PackGame);
                        resourceMgr.LoadPack(PackConfig.GetBoxBackgrounds(pack));
                        resourceMgr.LoadPack(levelResources);
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
                            QueueOrPollBoxPrefetch();
                            return;
                        }
                        if (nextController - 1 > 3)
                        {
                            return;
                        }
                        MenuController menuController3 = new(this);
                        AddChildwithID(menuController3, 1);
                        int packCount = CTRPreferences.GetPacksCount();
                        for (int i = 0; i < packCount; i++)
                        {
                            resourceMgr.FreePack(PackConfig.GetBoxCovers(i));
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
                            StopGameplayPrefetch();
                            DeleteChild(3);
                            resourceMgr.FreePack(PackGame);
                            resourceMgr.FreePack([.. sessionResources]);
                            sessionResources.Clear();
                            int packCount = CTRPreferences.GetPacksCount();
                            for (int i = 0; i < packCount; i++)
                            {
                                resourceMgr.FreePack(PackConfig.GetBoxBackgrounds(i));
                            }
                            resourceMgr.resourcesDelegate = (LoadingController)GetChild(2);
                            resourceMgr.InitLoading();
                            resourceMgr.LoadPack(PackMenu);
                            resourceMgr.StartLoading();
                            LoadingController loadingController = (LoadingController)GetChild(2);
                            loadingController.nextController = exitCode != 0 ? exitCode != 1 ? 3 : 2 : 1;
                            ActivateChild(2);
                            //Show menu presence on exit to menu
                            Game1.RPC?.MenuPresence();
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
                StopGameplayPrefetch();
                loadedMap = null;
                mapName = null;
            }
            base.Dispose(disposing);
        }

        public static void CheckMapIsValid()
        {
        }

        //public static bool IsHacked()
        //{
        //    return false;
        //}

        //public static void SetHacked()
        //{
        //}

        public static void SetInCrystal(bool b)
        {
            ((CTRRootController)Application.SharedRootController()).inCrystal = b;
        }

        public static void OpenFullVersionPage()
        {
        }

        public void SetBox(int b)
        {
            box = b;
        }

        public int GetBox()
        {
            return box;
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

        public static void PostAchievementName(string _, string _1)
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

        /// <summary>
        /// Loads the current map XML from disk when only the pack/level identity is known.
        /// </summary>
        private void EnsureCurrentMapLoaded()
        {
            if (loadedMap != null)
            {
                return;
            }

            string currentMapName = mapName;
            if (string.IsNullOrWhiteSpace(currentMapName) && pack >= 0 && level >= 0 && pack < PackConfig.GetPackCount() && level < PackConfig.GetLevelCount(pack))
            {
                currentMapName = LevelsList.LEVEL_NAMES[pack, level];
                mapName = currentMapName;
            }

            if (string.IsNullOrWhiteSpace(currentMapName))
            {
                return;
            }

            loadedMap = ContentPaths.LoadXml(Path.Combine(ContentPaths.MapsDirectory, currentMapName));
        }

        /// <summary>
        /// Adds resource identifiers to the set that will be freed when gameplay ends.
        /// </summary>
        /// <param name="resources">Gameplay resources to track for session cleanup.</param>
        private void TrackSessionResources(IEnumerable<string> resources)
        {
            if (resources == null)
            {
                return;
            }

            foreach (string resourceName in resources)
            {
                if (!string.IsNullOrWhiteSpace(resourceName))
                {
                    _ = sessionResources.Add(resourceName);
                }
            }
        }

        /// <summary>
        /// Clears session-scoped loading state before starting a fresh gameplay resource session.
        /// </summary>
        private void ResetGameplayResourceSession()
        {
            StopGameplayPrefetch();
            sessionResources.Clear();
            boxResourceScanTask = null;
            boxResourceScanPack = -1;
        }

        /// <summary>
        /// Starts the asynchronous scan that discovers the union of resources used across the current box.
        /// </summary>
        private void StartBoxResourceScanIfNeeded()
        {
            if (pack < 0)
            {
                return;
            }

            if (boxResourceScanTask != null && boxResourceScanPack == pack && !boxResourceScanTask.IsFaulted && !boxResourceScanTask.IsCanceled)
            {
                return;
            }

            boxResourceScanPack = pack;
            boxResourceScanTask = Task.Run(() => LevelResourceScanner.GetBoxResources(pack));
        }

        /// <summary>
        /// Starts gameplay prefetch immediately if the box scan is done, or polls until scan results are ready.
        /// </summary>
        private void QueueOrPollBoxPrefetch()
        {
            if (GetChild(CHILD_GAME) == null)
            {
                return;
            }

            if (boxResourceScanTask == null)
            {
                return;
            }

            if (boxResourceScanTask.IsCompletedSuccessfully)
            {
                StopBoxScanPollTimer();
                QueueRemainingBoxResourcesForPrefetch(boxResourceScanTask.Result);
                return;
            }

            if (boxScanPollTimer < 0)
            {
                boxScanPollTimer = TimerManager.Schedule(static obj => ((CTRRootController)obj).PollBoxResourceScan(), this, 0.25f);
            }
        }

        /// <summary>
        /// Polls the asynchronous box scan task and queues prefetch work once it completes successfully.
        /// </summary>
        private void PollBoxResourceScan()
        {
            if (boxResourceScanTask == null)
            {
                StopBoxScanPollTimer();
                return;
            }

            if (!boxResourceScanTask.IsCompleted)
            {
                return;
            }

            StopBoxScanPollTimer();
            if (boxResourceScanTask.IsCompletedSuccessfully)
            {
                QueueRemainingBoxResourcesForPrefetch(boxResourceScanTask.Result);
            }
        }

        /// <summary>
        /// Queues the subset of whole-box resources that were not already loaded for the current session.
        /// </summary>
        /// <param name="boxResources">The full resource union discovered for the current box.</param>
        private void QueueRemainingBoxResourcesForPrefetch(HashSet<string> boxResources)
        {
            if (boxResources == null || boxResources.Count == 0)
            {
                return;
            }

            HashSet<string> remainingResources = [.. boxResources];
            remainingResources.ExceptWith(sessionResources);
            if (remainingResources.Count == 0)
            {
                return;
            }

            CTRResourceMgr resourceMgr = Application.SharedResourceMgr();
            resourceMgr.QueuePrefetchPack(remainingResources);

            if (prefetchDrainTimer < 0)
            {
                prefetchDrainTimer = TimerManager.Schedule(static obj => ((CTRRootController)obj).DrainPrefetchQueue(), this, 1f / 60f);
            }
        }

        /// <summary>
        /// Advances background gameplay prefetch by at most one queued resource.
        /// </summary>
        private void DrainPrefetchQueue()
        {
            CTRResourceMgr resourceMgr = Application.SharedResourceMgr();
            if (resourceMgr.PrefetchNextResource(out string loadedName))
            {
                if (!string.IsNullOrWhiteSpace(loadedName))
                {
                    _ = sessionResources.Add(loadedName);
                }
            }
            else if (!resourceMgr.HasPendingPrefetchResources())
            {
                StopPrefetchDrainTimer();
            }
        }

        /// <summary>
        /// Stops all gameplay-prefetch timers and clears any queued prefetch work.
        /// </summary>
        private void StopGameplayPrefetch()
        {
            StopBoxScanPollTimer();
            StopPrefetchDrainTimer();
            Application.SharedResourceMgr().ClearPrefetchQueue();
        }

        /// <summary>
        /// Stops the timer that waits for whole-box scan completion.
        /// </summary>
        private void StopBoxScanPollTimer()
        {
            if (boxScanPollTimer >= 0)
            {
                TimerManager.StopTimer(boxScanPollTimer);
                boxScanPollTimer = -1;
            }
        }

        /// <summary>
        /// Stops the timer that drains the silent gameplay-prefetch queue.
        /// </summary>
        private void StopPrefetchDrainTimer()
        {
            if (prefetchDrainTimer >= 0)
            {
                TimerManager.StopTimer(prefetchDrainTimer);
                prefetchDrainTimer = -1;
            }
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

        public int box;

        public int pack;

        private string mapName;

        private XElement loadedMap;

        private int level;

        private bool picker;

        private bool survival;

        private bool inCrystal;

        private bool showGreeting;

        private static readonly string[] PackStartup = [
            Resources.BackgroundImg.ZeptolabNoLink,
            Resources.Img.LoaderbarFull,
            null
        ];

        private static readonly string[] PackMenu =
        [
            Resources.Img.MenuBgr,
            Resources.Img.MenuPopup,
            Resources.Img.MenuLogo,
            Resources.Img.MenuLevelSelection,
            Resources.Img.MenuPackSelection,
            Resources.Img.MenuPackSelection2,
            Resources.Img.MenuExtraButtons,
            Resources.Img.MenuBgrShadow,
            Resources.Img.MenuBgrXmas,
            null
        ];

        private static readonly string[] PackGame = [
            Resources.Img.MenuButtonShort,
            Resources.Img.HudButtons,
            CandySkinHelper.GetCandyResource(Preferences.GetIntForKey(CTRPreferences.PREFS_SELECTED_CANDY)),
            Resources.Img.ObjCandyFx,
            Resources.Img.ObjSpider,
            Resources.Img.ConfettiParticles,
            Resources.Img.MenuPause,
            Resources.Img.MenuResult,
            Resources.Fnt.FontNumbersBig,
            Resources.Img.HudButtonsEn,
            Resources.Img.MenuResultEn,
            null
        ];

        private readonly HashSet<string> sessionResources = [];

        private Task<HashSet<string>> boxResourceScanTask;

        private int boxResourceScanPack = -1;

        private int boxScanPollTimer = -1;

        private int prefetchDrainTimer = -1;
    }
}
