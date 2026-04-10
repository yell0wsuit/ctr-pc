using System;
using System.Globalization;

using CutTheRopeDX.Commons;
using CutTheRopeDX.Framework.Core;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Manages game preferences including level progress, scores, stars, pack unlocks, and user settings.
    /// </summary>
    internal sealed class CTRPreferences : Preferences
    {
        /// <summary>
        /// Initializes preferences, performing first-launch setup or migration from older versions as needed.
        /// </summary>
        public CTRPreferences()
        {
            if (!GetBooleanForKey("PREFS_EXIST"))
            {
                SetBooleanForKey(true, "PREFS_EXIST", true);
                SetIntForKey(0, "PREFS_GAME_STARTS", true);
                SetIntForKey(0, "PREFS_LEVELS_WON", true);
                ResetToDefaults();
                ResetMusicSound();
                firstLaunch = true;
                playLevelScroll = false;
            }
            else
            {
                if (GetIntForKey("PREFS_VERSION") < 1)
                {
                    _ = GetTotalScore();
                    int i = 0;
                    int packsCount = GetPacksCount();
                    while (i < packsCount)
                    {
                        int packScoreTotal = 0;
                        int j = 0;
                        int levelsInPackCount = GetLevelsInPackCount(i);
                        while (j < levelsInPackCount)
                        {
                            int intForKey2 = GetBoxIntForKey(GetBoxForPack(i), GetPackLevelKey("SCORE_", i, j));
                            if (intForKey2 > 5999)
                            {
                                packScoreTotal = 150000;
                                break;
                            }
                            packScoreTotal += intForKey2;
                            j++;
                        }
                        if (packScoreTotal > 149999)
                        {
                            ResetToDefaults();
                            ResetMusicSound();
                            break;
                        }
                        i++;
                    }
                    SetScoreHash();
                }
                firstLaunch = false;
                playLevelScroll = false;
            }
            SetIntForKey(2, "PREFS_VERSION", true);
            EnsureSlotEntryPacksUnlocked();
            SetRpcPreferenceInJson(); // temporary hack, remove after setting UI is implemented
            SetUpdateCheckPreferenceInJson(); // temporary hack, remove after setting UI is implemented
        }

        /// <summary>
        /// Resets sound and music preferences to enabled.
        /// </summary>
        private static void ResetMusicSound()
        {
            SetBooleanForKey(true, "SOUND_ON", true);
            SetBooleanForKey(true, "MUSIC_ON", true);
        }

        /// <summary>
        /// Ensures the Discord RPC preference exists until settings UI can create it.
        /// </summary>
        /// <remarks>
        /// Todo: Remove after setting UI is implemented
        /// </remarks>
        private static void SetRpcPreferenceInJson()
        {
            if (!ContainsKey("PREFS_RPC_ENABLED"))
            {
                SetBooleanForKey(true, "PREFS_RPC_ENABLED", true);
            }
        }

        /// <summary>
        /// Ensures the update-check preference exists until settings UI can create it.
        /// </summary>
        /// <remarks>
        /// Todo: Remove after setting UI is implemented
        /// </remarks>
        private static void SetUpdateCheckPreferenceInJson()
        {
            if (!ContainsKey("PREFS_UPDATE_CHECK"))
            {
                SetBooleanForKey(true, "PREFS_UPDATE_CHECK", true);
            }
        }

        /// <summary>
        /// Returns whether automatic update checking is enabled.
        /// </summary>
        /// <returns><see langword="true"/> if update checking is enabled; otherwise, <see langword="false"/>.</returns>
        public static bool IsUpdateCheckEnabled()
        {
            return GetBooleanForKey("PREFS_UPDATE_CHECK");
        }

        /// <summary>
        /// Returns whether the game is a shareware build.
        /// </summary>
        /// <returns>Always <see langword="false"/> in this build.</returns>
        private static bool IsShareware()
        {
            return false;
        }

        /// <summary>
        /// Returns whether the shareware content has been unlocked.
        /// </summary>
        /// <returns><see langword="true"/> if not shareware or if the shareware IAP has been purchased.</returns>
        public static bool IsSharewareUnlocked()
        {
            bool flag = IsShareware();
            return !flag || (flag && GetBooleanForKey("IAP_SHAREWARE"));
        }

        /// <summary>
        /// Returns whether this is the lite (free) version of the game.
        /// </summary>
        /// <returns>Always <see langword="false"/> in this build.</returns>
        public static bool IsLiteVersion()
        {
            return false;
        }

        /// <summary>
        /// Returns whether advertisement banners should be displayed.
        /// </summary>
        /// <returns>Always <see langword="false"/> in this build.</returns>
        public static bool IsBannersMustBeShown()
        {
            return false;
        }

        /// <summary>
        /// Gets the number of stars earned for a specific level in a save slot.
        /// </summary>
        /// <param name="box">The save slot index.</param>
        /// <param name="p">The pack index.</param>
        /// <param name="l">The level index within the pack.</param>
        /// <returns>The number of stars earned (0–3).</returns>
        public static int GetStarsForPackLevel(int box, int p, int l)
        {
            return GetBoxIntForKey(box, GetPackLevelKey("STARS_", p, l));
        }

        /// <summary>
        /// Gets the number of stars earned for a specific level, using the pack's default save slot.
        /// </summary>
        /// <param name="p">The pack index.</param>
        /// <param name="l">The level index within the pack.</param>
        /// <returns>The number of stars earned (0–3).</returns>
        public static int GetStarsForPackLevel(int p, int l)
        {
            return GetStarsForPackLevel(GetBoxForPack(p), p, l);
        }

        /// <summary>
        /// Gets the unlock state of a specific level in a save slot.
        /// </summary>
        /// <param name="box">The save slot index.</param>
        /// <param name="p">The pack index.</param>
        /// <param name="l">The level index within the pack.</param>
        /// <returns>The <see cref="UNLOCKEDSTATE"/> of the level.</returns>
        public static UNLOCKEDSTATE GetUnlockedForPackLevel(int box, int p, int l)
        {
            string unlockedKey = GetPackLevelKey("UNLOCKED_", p, l);
            bool isUnlocked = GetBoxBoolForKey(box, unlockedKey);

            if (!isUnlocked)
            {
                return UNLOCKEDSTATE.LOCKED;
            }

            string stateKey = GetPackLevelKey("UNLOCKED_STATE_", p, l);
            string stateValue = GetBoxStringForKey(box, stateKey);
            return Enum.TryParse(stateValue, ignoreCase: false, out UNLOCKEDSTATE parsedState) &&
                parsedState != UNLOCKEDSTATE.LOCKED &&
                parsedState != UNLOCKEDSTATE.UNLOCKED
                ? parsedState
                : UNLOCKEDSTATE.UNLOCKED;
        }

        /// <summary>
        /// Gets the unlock state of a specific level, using the pack's default save slot.
        /// </summary>
        /// <param name="p">The pack index.</param>
        /// <param name="l">The level index within the pack.</param>
        /// <returns>The <see cref="UNLOCKEDSTATE"/> of the level.</returns>
        public static UNLOCKEDSTATE GetUnlockedForPackLevel(int p, int l)
        {
            return GetUnlockedForPackLevel(GetBoxForPack(p), p, l);
        }

        /// <summary>
        /// Gets the total number of available packs, limited by shareware restrictions if applicable.
        /// </summary>
        /// <returns>The number of packs.</returns>
        public static int GetPacksCount()
        {
            int packs = PackConfig.GetPackCount();
            return IsLiteVersion() ? Math.Min(packs, SharewareFreePacks()) : packs;
        }

        /// <summary>
        /// Gets the save slot index for the specified pack.
        /// </summary>
        /// <param name="pack">The pack index.</param>
        /// <returns>The save slot index.</returns>
        public static int GetBoxForPack(int pack)
        {
            return PackConfig.GetSaveSlot(pack);
        }

        /// <summary>
        /// Gets the number of levels in the specified pack, limited by shareware restrictions if applicable.
        /// </summary>
        /// <param name="pack">The pack index.</param>
        /// <returns>The number of levels.</returns>
        public static int GetLevelsInPackCount(int pack)
        {
            int levels = PackConfig.GetLevelCount(pack);
            return IsLiteVersion() ? Math.Min(levels, SharewareFreeLevels()) : levels;
        }

        /// <summary>
        /// Gets the maximum number of levels per pack.
        /// </summary>
        /// <returns>The maximum level count.</returns>
        public static int GetLevelsInPackCount()
        {
            return PackConfig.MaxLevelsPerPack;
        }

        /// <summary>
        /// Gets the total stars earned in the current pack's save slot.
        /// </summary>
        /// <returns>The total star count.</returns>
        public static int GetTotalStars()
        {
            if (Application.SharedRootController() is CTRRootController rootController)
            {
                int pack = rootController.GetPack();
                return GetTotalStarsInBox(GetBoxForPack(pack));
            }

            return GetTotalStarsInBox(0);
        }

        /// <summary>
        /// Gets the total stars earned across all packs in the specified save slot.
        /// </summary>
        /// <param name="box">The save slot index.</param>
        /// <returns>The total star count.</returns>
        public static int GetTotalStarsInBox(int box)
        {
            int totalStars = 0;
            int i = 0;
            int packsCount = GetPacksCount();
            while (i < packsCount)
            {
                if (GetBoxForPack(i) != box)
                {
                    i++;
                    continue;
                }

                int j = 0;
                int levelsInPackCount = GetLevelsInPackCount(i);
                while (j < levelsInPackCount)
                {
                    totalStars += GetStarsForPackLevel(box, i, j);
                    j++;
                }
                i++;
            }
            return totalStars;
        }

        /// <summary>
        /// Gets the number of stars required to unlock the specified pack.
        /// </summary>
        /// <param name="n">The pack index.</param>
        /// <returns>The star threshold for unlocking.</returns>
        public static int PackUnlockStars(int n)
        {
            return PackConfig.GetUnlockStars(n);
        }

        /// <summary>
        /// Builds a preference key string for a specific pack and level.
        /// </summary>
        /// <param name="prefs">The key prefix (e.g. <c>"STARS_"</c>, <c>"SCORE_"</c>).</param>
        /// <param name="p">The pack index.</param>
        /// <param name="l">The level index within the pack.</param>
        /// <returns>The composite preference key.</returns>
        private static string GetPackLevelKey(string prefs, int p, int l)
        {
            return prefs + p.ToString(CultureInfo.InvariantCulture) + "_" + l.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Sets the unlock state of a specific level in a save slot.
        /// </summary>
        /// <param name="box">The save slot index.</param>
        /// <param name="s">The unlock state to set.</param>
        /// <param name="p">The pack index.</param>
        /// <param name="l">The level index within the pack.</param>
        public static void SetUnlockedForPackLevel(int box, UNLOCKEDSTATE s, int p, int l)
        {
            string unlockedKey = GetPackLevelKey("UNLOCKED_", p, l);
            string stateKey = GetPackLevelKey("UNLOCKED_STATE_", p, l);

            if (s == UNLOCKEDSTATE.LOCKED)
            {
                SetBoxBoolForKey(box, false, unlockedKey, false);
                RemoveBoxKey(box, stateKey);
            }
            else if (s == UNLOCKEDSTATE.UNLOCKED)
            {
                SetBoxBoolForKey(box, true, unlockedKey, false);
                RemoveBoxKey(box, stateKey);
            }
            else
            {
                SetBoxBoolForKey(box, true, unlockedKey, false);
                SetBoxStringForKey(box, s.ToString(), stateKey, false);
            }

            RequestSave();
        }

        /// <summary>
        /// Sets the unlock state of a specific level, using the pack's default save slot.
        /// </summary>
        /// <param name="s">The unlock state to set.</param>
        /// <param name="p">The pack index.</param>
        /// <param name="l">The level index within the pack.</param>
        public static void SetUnlockedForPackLevel(UNLOCKEDSTATE s, int p, int l)
        {
            SetUnlockedForPackLevel(GetBoxForPack(p), s, p, l);
        }

        /// <summary>
        /// Gets the number of free levels available in the shareware version.
        /// </summary>
        /// <returns>The free level count.</returns>
        public static int SharewareFreeLevels()
        {
            return 10;
        }

        /// <summary>
        /// Gets the number of free packs available in the shareware version.
        /// </summary>
        /// <returns>The free pack count.</returns>
        public static int SharewareFreePacks()
        {
            return 2;
        }

        /// <summary>
        /// Saves the last selected save slot index.
        /// </summary>
        /// <param name="p">The save slot index.</param>
        public static void SetLastBox(int p)
        {
            SetIntForKey(p, "PREFS_LAST_BOX", true);
        }

        /// <summary>
        /// Saves the last selected game pack index.
        /// </summary>
        /// <param name="b">The game pack index.</param>
        public static void SetLastGamePack(int b)
        {
            SetIntForKey(b, "PREFS_LAST_GAMEPACK", true);
        }

        /// <summary>
        /// Gets the last selected game pack index, defaulting to 0 if not set.
        /// </summary>
        /// <returns>The game pack index.</returns>
        public static int GetLastGamePack()
        {
            int val = GetIntForKey("PREFS_LAST_GAMEPACK");
            return val >= 0 ? val : 0;
        }

        /// <summary>
        /// Returns whether every level in the pack has 3 stars in the specified save slot.
        /// </summary>
        /// <param name="box">The save slot index.</param>
        /// <param name="p">The pack index.</param>
        /// <returns><see langword="true"/> if all levels have 3 stars; otherwise, <see langword="false"/>.</returns>
        public static bool IsPackPerfect(int box, int p)
        {
            int i = 0;
            int levelsInPackCount = GetLevelsInPackCount(p);
            while (i < levelsInPackCount)
            {
                if (GetStarsForPackLevel(box, p, i) < 3)
                {
                    return false;
                }
                i++;
            }
            return true;
        }

        /// <summary>
        /// Returns whether every level in the pack has 3 stars, using the pack's default save slot.
        /// </summary>
        /// <param name="p">The pack index.</param>
        /// <returns><see langword="true"/> if all levels have 3 stars; otherwise, <see langword="false"/>.</returns>
        public static bool IsPackPerfect(int p)
        {
            return IsPackPerfect(GetBoxForPack(p), p);
        }

        /// <summary>
        /// Gets the last selected save slot index, defaulting to 0 if out of range.
        /// </summary>
        /// <returns>The save slot index.</returns>
        public static int GetLastBox()
        {
            int val = GetIntForKey("PREFS_LAST_BOX");
            int maxPack = GetPacksCount();
            // If saved pack is out of range, fall back to first pack
            return (val >= 0 && val <= maxPack) ? val : 0;
        }

        /// <summary>
        /// Called when the game view changes. Currently a no-op.
        /// </summary>
        /// <param name="_">Unused parameter.</param>
        public static void GameViewChanged(string _)
        {
        }

        /// <summary>
        /// Gets the score for a specific level in a save slot.
        /// </summary>
        /// <param name="box">The save slot index.</param>
        /// <param name="p">The pack index.</param>
        /// <param name="l">The level index within the pack.</param>
        /// <returns>The level score.</returns>
        public static int GetScoreForPackLevel(int box, int p, int l)
        {
            return GetBoxIntForKey(box, GetPackLevelKey("SCORE_", p, l));
        }

        /// <summary>
        /// Gets the score for a specific level, using the pack's default save slot.
        /// </summary>
        /// <param name="p">The pack index.</param>
        /// <param name="l">The level index within the pack.</param>
        /// <returns>The level score.</returns>
        public static int GetScoreForPackLevel(int p, int l)
        {
            return GetScoreForPackLevel(GetBoxForPack(p), p, l);
        }

        /// <summary>
        /// Sets the score for a specific level in a save slot.
        /// </summary>
        /// <param name="box">The save slot index.</param>
        /// <param name="s">The score to set.</param>
        /// <param name="p">The pack index.</param>
        /// <param name="l">The level index within the pack.</param>
        public static void SetScoreForPackLevel(int box, int s, int p, int l)
        {
            SetBoxIntForKey(box, s, GetPackLevelKey("SCORE_", p, l), true);
        }

        /// <summary>
        /// Sets the score for a specific level, using the pack's default save slot.
        /// </summary>
        /// <param name="s">The score to set.</param>
        /// <param name="p">The pack index.</param>
        /// <param name="l">The level index within the pack.</param>
        public static void SetScoreForPackLevel(int s, int p, int l)
        {
            SetScoreForPackLevel(GetBoxForPack(p), s, p, l);
        }

        /// <summary>
        /// Sets the star count for a specific level in a save slot.
        /// </summary>
        /// <param name="box">The save slot index.</param>
        /// <param name="s">The number of stars to set.</param>
        /// <param name="p">The pack index.</param>
        /// <param name="l">The level index within the pack.</param>
        public static void SetStarsForPackLevel(int box, int s, int p, int l)
        {
            SetBoxIntForKey(box, s, GetPackLevelKey("STARS_", p, l), true);
        }

        /// <summary>
        /// Sets the star count for a specific level, using the pack's default save slot.
        /// </summary>
        /// <param name="s">The number of stars to set.</param>
        /// <param name="p">The pack index.</param>
        /// <param name="l">The level index within the pack.</param>
        public static void SetStarsForPackLevel(int s, int p, int l)
        {
            SetStarsForPackLevel(GetBoxForPack(p), s, p, l);
        }

        /// <summary>
        /// Gets the total stars earned across all levels in a pack within the specified save slot.
        /// </summary>
        /// <param name="box">The save slot index.</param>
        /// <param name="p">The pack index.</param>
        /// <returns>The total star count for the pack.</returns>
        public static int GetTotalStarsInPack(int box, int p)
        {
            int starsInPack = 0;
            int i = 0;
            int levelsInPackCount = GetLevelsInPackCount(p);
            while (i < levelsInPackCount)
            {
                starsInPack += GetStarsForPackLevel(box, p, i);
                i++;
            }
            return starsInPack;
        }

        /// <summary>
        /// Gets the total stars earned across all levels in a pack, using the pack's default save slot.
        /// </summary>
        /// <param name="p">The pack index.</param>
        /// <returns>The total star count for the pack.</returns>
        public static int GetTotalStarsInPack(int p)
        {
            return GetTotalStarsInPack(GetBoxForPack(p), p);
        }

        /// <summary>
        /// Disables the level scroll animation on the next level screen display.
        /// </summary>
        public static void DisablePlayLevelScroll()
        {
            Application.SharedPreferences().playLevelScroll = false;
        }

        /// <summary>
        /// Returns whether the level scroll animation should play.
        /// </summary>
        /// <returns><see langword="true"/> if level scroll should play; otherwise, <see langword="false"/>.</returns>
        internal static bool ShouldPlayLevelScroll()
        {
            return Application.SharedPreferences().playLevelScroll;
        }

        /// <summary>
        /// Returns whether the specified pack is the first pack in its save slot.
        /// </summary>
        /// <param name="pack">The pack index.</param>
        /// <returns><see langword="true"/> if no earlier pack shares the same save slot; otherwise, <see langword="false"/>.</returns>
        private static bool IsSlotEntryPack(int pack)
        {
            int box = GetBoxForPack(pack);
            for (int i = 0; i < pack; i++)
            {
                if (GetBoxForPack(i) == box)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Ensures the first level of each slot-entry pack is unlocked so players can always access at least one level per save slot.
        /// </summary>
        private static void EnsureSlotEntryPacksUnlocked()
        {
            bool changed = false;
            for (int pack = 0; pack < GetPacksCount(); pack++)
            {
                if (!IsSlotEntryPack(pack))
                {
                    continue;
                }

                int box = GetBoxForPack(pack);
                if (GetUnlockedForPackLevel(box, pack, 0) != UNLOCKEDSTATE.LOCKED)
                {
                    continue;
                }

                SetBoxBoolForKey(box, true, GetPackLevelKey("UNLOCKED_", pack, 0), false);
                RemoveBoxKey(box, GetPackLevelKey("UNLOCKED_STATE_", pack, 0));
                changed = true;
            }

            if (changed)
            {
                RequestSave();
            }
        }

        /// <summary>
        /// Resets all progress and preferences to their default values.
        /// </summary>
        public static void ResetToDefaults()
        {
            ClearAllBoxData();
            for (int i = 0; i < GetPacksCount(); i++)
            {
                bool unlockFirstLevel = IsSlotEntryPack(i) || (IsShareware() && i < SharewareFreePacks());
                if (!unlockFirstLevel)
                {
                    continue;
                }

                int box = GetBoxForPack(i);
                SetBoxBoolForKey(box, true, GetPackLevelKey("UNLOCKED_", i, 0), false);
            }

            SetIntForKey(0, "PREFS_ROPES_CUT", true);
            SetIntForKey(0, "PREFS_ROPES_SHOOT", true);
            SetIntForKey(0, "PREFS_BUBBLES_POPPED", true);
            SetIntForKey(0, "PREFS_SPIDERS_BUSTED", true);
            SetIntForKey(0, "PREFS_CANDIES_LOST", true);
            SetIntForKey(0, "PREFS_CANDIES_UNITED", true);
            SetIntForKey(0, "PREFS_SOCKS_USED", true);
            SetIntForKey(0, "PREFS_SELECTED_CANDY", true);
            SetIntForKey(0, "PREFS_SELECTED_ROPE", true);
            SetIntForKey(0, "PREFS_SELECTED_OMNOM", true);
            SetIntForKey(0, "PREFS_SELECTED_TRACE", true);
            SetBooleanForKey(false, "PREFS_CANDY_WAS_CHANGED", true);
            SetBooleanForKey(true, "PREFS_GAME_CENTER_ENABLED", true);
            SetIntForKey(0, "PREFS_NEW_DRAWINGS_COUNTER", true);
            SetIntForKey(0, "PREFS_LAST_BOX", true);
            SetIntForKey(0, "PREFS_LAST_GAMEPACK", true);
            SetBooleanForKey(true, "PREFS_WINDOW_FULLSCREEN", true);
            SetBooleanForKey(true, "PREFS_RPC_ENABLED", true);
            SetBooleanForKey(true, "PREFS_UPDATE_CHECK", true);
            CheckForUnlockIAP();
            RequestSave();
            SetScoreHash();
        }

        /// <summary>
        /// If the unlock-all IAP has been purchased, unlocks the first level of every locked pack.
        /// </summary>
        private static void CheckForUnlockIAP()
        {
            if (!GetBooleanForKey("IAP_UNLOCK"))
            {
                return;
            }
            int i = 0;
            int packsCount = GetPacksCount();
            while (i < packsCount)
            {
                int box = GetBoxForPack(i);
                if (GetUnlockedForPackLevel(box, i, 0) == UNLOCKEDSTATE.LOCKED)
                {
                    SetUnlockedForPackLevel(box, UNLOCKEDSTATE.JUSTUNLOCKED, i, 0);
                }
                i++;
            }
        }

        /// <summary>
        /// Calculates the sum of all level scores across every pack and save slot.
        /// </summary>
        /// <returns>The total score.</returns>
        private static int GetTotalScore()
        {
            int totalScore = 0;
            for (int i = 0; i < GetPacksCount(); i++)
            {
                for (int j = 0; j < GetLevelsInPackCount(i); j++)
                {
                    totalScore += GetBoxIntForKey(GetBoxForPack(i), GetPackLevelKey("SCORE_", i, j));
                }
            }
            return totalScore;
        }

        /// <summary>
        /// Computes and stores a SHA-256 hash of the total score for integrity validation.
        /// </summary>
        public static void SetScoreHash()
        {
            string sha256Str = GetSHA256Str(GetTotalScore().ToString(CultureInfo.InvariantCulture));
            SetStringForKey(sha256Str.ToString(), "PREFS_SCORE_HASH", true);
        }

        /// <summary>
        /// Returns whether this is the first time the game has been launched.
        /// </summary>
        /// <returns><see langword="true"/> on first launch; otherwise, <see langword="false"/>.</returns>
        internal static bool IsFirstLaunch()
        {
            return Application.SharedPreferences().firstLaunch;
        }

        /// <summary>
        /// Validates the stored score hash against the current total score.
        /// </summary>
        /// <returns>Always <see langword="true"/> in this build.</returns>
        internal static bool IsScoreHashValid()
        {
            return true;
        }

        /// <summary>ZeptoLab Twitter profile URL.</summary>
        public const string TWITTER_LINK = "https://mobile.twitter.com/zeptolab";

        /// <summary>Cut the Rope Facebook page URL.</summary>
        public const string FACEBOOK_LINK = "http://www.facebook.com/cuttherope";

        /// <summary>Cut the Rope: Experiments Amazon Store listing URL.</summary>
        public const string EXPERIMENTS_LINK = "http://www.amazon.com/gp/mas/dl/android?p=com.zeptolab.ctrexperiments.hd.amazon.paid";

        /// <summary>Manages fetching and caching of remote configuration data.</summary>
        public RemoteDataManager remoteDataManager = new();

        /// <summary>Whether this is the first time the game has been launched.</summary>
        private readonly bool firstLaunch;

        /// <summary>Whether the level scroll animation should play on the next level screen display.</summary>
        private bool playLevelScroll;
    }
}
