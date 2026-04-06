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
    /// <summary>
    /// Top-level game controller that manages the startup → menu → loading → gameplay lifecycle,
    /// resource loading/unloading across transitions, and background prefetch of box-level resources.
    /// </summary>
    internal sealed class CTRRootController : RootController
    {
        /// <summary>
        /// Stub for analytics event logging.
        /// </summary>
        /// <param name="_">Event name.</param>
        /// <remarks>
        /// No-op on PC.
        /// </remarks>
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

            string[] levelResources = LevelResourceScanner.GetRequiredResources(map);
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

        /// <summary>Stub for setting a maps dictionary.</summary>
        /// <param name="_">Maps dictionary.</param>
        /// <remarks>
        /// No-op on PC.
        /// </remarks>
        public static void SetMapsList(Dictionary<string, XElement> _)
        {
        }

        /// <summary>Gets the current pack (box group) index.</summary>
        /// <returns>The zero-based pack index.</returns>
        public int GetPack()
        {
            return pack;
        }

        /// <summary>
        /// Initialises the root controller, loads startup resources, and adds the startup child controller.
        /// </summary>
        /// <param name="parent">Parent view controller that hosts this root controller.</param>
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

        /// <inheritdoc />
        public override void Activate()
        {
            _ = CTRPreferences.IsFirstLaunch();
            base.Activate();
            ActivateChild(0);
            Application.SharedCanvas().BeforeRender();
            ActiveChild().ActiveView().Draw();
            GLCanvas.AfterRender();
        }

        /// <summary>Removes the menu child controller and frees menu resources.</summary>
        public void DeleteMenu()
        {
            CTRResourceMgr resourceMgr = Application.SharedResourceMgr();
            DeleteChild(1);
            Application.SharedMovieMgr().delegateMovieMgrDelegate = null;
            resourceMgr.FreePack(PackMenu);
        }

        /// <summary>Disabling Game Center.</summary>
        /// <remarks>
        /// No-op on PC.
        /// </remarks>
        public static void DisableGameCenter()
        {
        }

        /// <summary>Enabling Game Center.</summary>
        /// <remarks>
        /// No-op on PC.
        /// </remarks>
        public static void EnableGameCenter()
        {
        }

        /// <inheritdoc />
        public override void Suspend()
        {
            suspended = true;
        }

        /// <inheritdoc />
        public override void Resume()
        {
            if (!inCrystal)
            {
                suspended = false;
            }
        }

        /// <inheritdoc />
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
                        string[] levelResources = LevelResourceScanner.GetRequiredResources(loadedMap);
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
                            menuController3.viewToShow = pack < CTRPreferences.GetPacksCount() - 1 ? 5 : (PackConfig.OutroVideo != null ? 7 : 5);
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

        /// <inheritdoc />
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

        /// <summary>Map validation.</summary>
        /// <remarks>
        /// No-op code.
        /// </remarks>
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

        /// <summary>Sets the Chillingo's Crystal overlay state on the shared root controller.</summary>
        /// <param name="b">Whether the Crystal overlay is active.</param>
        public static void SetInCrystal(bool b)
        {
            ((CTRRootController)Application.SharedRootController()).inCrystal = b;
        }

        /// <summary>Stub for opening the full version store page.</summary>
        /// <remarks>
        /// No-op code.
        /// </remarks>
        public static void OpenFullVersionPage()
        {
        }

        /// <summary>Sets the current box index.</summary>
        /// <param name="b">Zero-based box index.</param>
        public void SetBox(int b)
        {
            box = b;
        }

        /// <summary>Gets the current box index.</summary>
        /// <returns>The zero-based box index.</returns>
        public int GetBox()
        {
            return box;
        }

        /// <summary>Sets the current pack (box group) index.</summary>
        /// <param name="p">Zero-based pack index.</param>
        public void SetPack(int p)
        {
            pack = p;
        }

        /// <summary>Sets the current level index within the active box.</summary>
        /// <param name="l">Zero-based level index.</param>
        public void SetLevel(int l)
        {
            level = l;
        }

        /// <summary>Gets the current level index within the active box.</summary>
        /// <returns>The zero-based level index.</returns>
        public int GetLevel()
        {
            return level;
        }

        /// <summary>Sets whether the level picker is active.</summary>
        /// <param name="p">Whether the level picker should be shown.</param>
        public void SetPicker(bool p)
        {
            picker = p;
        }

        /// <summary>Gets whether the level picker is active.</summary>
        /// <returns><see langword="true"/> if the level picker is shown; otherwise <see langword="false"/>.</returns>
        public bool IsPicker()
        {
            return picker;
        }

        /// <summary>Sets whether survival mode is active.</summary>
        /// <param name="s">Whether survival mode is enabled.</param>
        public void SetSurvival(bool s)
        {
            survival = s;
        }

        /// <summary>Gets whether survival mode(?) is active.</summary>
        /// <returns><see langword="true"/> if survival mode is enabled; otherwise <see langword="false"/>.</returns>
        public bool IsSurvival()
        {
            return survival;
        }

        /// <summary>Gets whether the Om Nom greeting animation should play on the next level start.</summary>
        /// <returns><see langword="true"/> if the greeting should be shown.</returns>
        public static bool IsShowGreeting()
        {
            return ((CTRRootController)Application.SharedRootController()).showGreeting;
        }

        /// <summary>Sets whether the Om Nom greeting animation should play on the next level start.</summary>
        /// <param name="s">Whether to show the greeting.</param>
        public static void SetShowGreeting(bool s)
        {
            ((CTRRootController)Application.SharedRootController()).showGreeting = s;
        }

        /// <summary>Stub for posting a named achievement with a value.</summary>
        /// <param name="_">Achievement name.</param>
        /// <param name="_1">Achievement value.</param>
        /// <remarks>
        /// No-op code.
        /// </remarks>
        public static void PostAchievementName(string _, string _1)
        {
        }

        /// <summary>Posts a named achievement to the scorer.</summary>
        /// <param name="name">The achievement identifier.</param>
        public static void PostAchievementName(string name)
        {
            Scorer.PostAchievementName(name);
        }

        /// <summary>Destroys and re-creates the loading controller child (slot 2).</summary>
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

        /// <summary>Exit code: proceed to the next game level.</summary>
        public const int NEXT_GAME = 0;

        /// <summary>Exit code: return to the main menu.</summary>
        public const int NEXT_MENU = 1;

        /// <summary>Exit code: return to the level picker.</summary>
        public const int NEXT_PICKER = 2;

        /// <summary>Exit code: return to the level picker and advance to the next pack.</summary>
        public const int NEXT_PICKER_NEXT_PACK = 3;

        /// <summary>Exit code: return to the level picker and show the unlock animation.</summary>
        public const int NEXT_PICKER_SHOW_UNLOCK = 4;

        /// <summary>Child slot index for the startup controller.</summary>
        public const int CHILD_START = 0;

        /// <summary>Child slot index for the menu controller.</summary>
        public const int CHILD_MENU = 1;

        /// <summary>Child slot index for the loading controller.</summary>
        public const int CHILD_LOADING = 2;

        /// <summary>Child slot index for the game controller.</summary>
        public const int CHILD_GAME = 3;

        /// <summary>Current box index.</summary>
        public int box;

        /// <summary>Current pack (box group) index.</summary>
        public int pack;

        /// <summary>Filename of the currently loaded map.</summary>
        private string mapName;

        /// <summary>Parsed XML of the currently loaded map, or <see langword="null"/>.</summary>
        private XElement loadedMap;

        /// <summary>Current level index within the active box.</summary>
        private int level;

        /// <summary>Whether the level picker is active.</summary>
        private bool picker;

        /// <summary>Whether survival mode is active.</summary>
        private bool survival;

        /// <summary>Whether the Crystal overlay is currently shown.</summary>
        private bool inCrystal;

        /// <summary>Whether the Om Nom greeting should play on the next level start.</summary>
        private bool showGreeting;

        /// <summary>Resource pack loaded during the startup splash screen.</summary>
        private static readonly string[] PackStartup = [
            Resources.Img.ZeptoLabLogoLoading,
            Resources.Img.ZeptoLabLogoAnim,
            null
        ];

        /// <summary>Resource pack loaded for the main menu.</summary>
        private static readonly string[] PackMenu =
        [
            Resources.Img.MenuBgr,
            Resources.Img.MenuPopup,
            Resources.Img.MenuLogo,
            Resources.Img.MenuLevelUi,
            Resources.Img.MenuPackSelection,
            Resources.Img.MenuPackSelection2,
            Resources.Img.MenuExtraButtons,
            Resources.Img.MenuBgrShadow,
            Resources.Img.MenuBgrXmas,
            null
        ];

        /// <summary>Resource pack loaded for gameplay (HUD, candy, spider, etc.).</summary>
        private static readonly string[] PackGame = [
            Resources.Img.MenuButtons,
            Resources.Img.HudUi,
            CandySkinHelper.GetCandyResource(Preferences.GetIntForKey("PREFS_SELECTED_CANDY")),
            Resources.Img.ObjCandyFx,
            Resources.Img.ObjSpider,
            Resources.Img.ConfettiParticles,
            Resources.Img.MenuPause,
            Resources.Img.MenuResults,
            Resources.Fnt.FontNumbersBig,
            null
        ];

        /// <summary>Set of resource names loaded during the current gameplay session, freed on exit.</summary>
        private readonly HashSet<string> sessionResources = [];

        /// <summary>Async task scanning all levels in the current box for their resource union.</summary>
        private Task<HashSet<string>> boxResourceScanTask;

        /// <summary>Pack index that <see cref="boxResourceScanTask"/> is scanning, or −1.</summary>
        private int boxResourceScanPack = -1;

        /// <summary>Timer handle polling <see cref="boxResourceScanTask"/> completion, or −1 if inactive.</summary>
        private int boxScanPollTimer = -1;

        /// <summary>Timer handle draining the background prefetch queue, or −1 if inactive.</summary>
        private int prefetchDrainTimer = -1;
    }
}
