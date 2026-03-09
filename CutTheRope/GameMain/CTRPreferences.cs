using System;
using System.Globalization;

using CutTheRope.Commons;
using CutTheRope.Framework.Core;

namespace CutTheRope.GameMain
{
    internal sealed class CTRPreferences : Preferences
    {
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
                            int intForKey2 = GetBoxIntForKey(GetBoxForPack(i), GetPackLevelKey(PREFS_SCORE_, i, j));
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

        private static void ResetMusicSound()
        {
            SetBooleanForKey(true, "SOUND_ON", true);
            SetBooleanForKey(true, "MUSIC_ON", true);
        }

        // temporary hack, remove after setting UI is implemented
        private static void SetRpcPreferenceInJson()
        {
            if (!ContainsKey(PREFS_RPC_ENABLED))
            {
                SetBooleanForKey(true, PREFS_RPC_ENABLED, true);
            }
        }

        // temporary hack, remove after setting UI is implemented
        private static void SetUpdateCheckPreferenceInJson()
        {
            if (!ContainsKey(PREFS_UPDATE_CHECK))
            {
                SetBooleanForKey(true, PREFS_UPDATE_CHECK, true);
            }
        }

        public static bool IsUpdateCheckEnabled()
        {
            return GetBooleanForKey(PREFS_UPDATE_CHECK);
        }

        private static bool IsShareware()
        {
            return false;
        }

        public static bool IsSharewareUnlocked()
        {
            bool flag = IsShareware();
            return !flag || (flag && GetBooleanForKey("IAP_SHAREWARE"));
        }

        public static bool IsLiteVersion()
        {
            return false;
        }

        public static bool IsBannersMustBeShown()
        {
            return false;
        }

        public static int GetStarsForPackLevel(int box, int p, int l)
        {
            return GetBoxIntForKey(box, GetPackLevelKey(PREFS_STARS_, p, l));
        }

        public static int GetStarsForPackLevel(int p, int l)
        {
            return GetStarsForPackLevel(GetBoxForPack(p), p, l);
        }

        public static UNLOCKEDSTATE GetUnlockedForPackLevel(int box, int p, int l)
        {
            string unlockedKey = GetPackLevelKey(PREFS_UNLOCKED_, p, l);
            bool isUnlocked = GetBoxBoolForKey(box, unlockedKey);

            if (!isUnlocked)
            {
                return UNLOCKEDSTATE.LOCKED;
            }

            string stateKey = GetPackLevelKey(PREFS_UNLOCKED_STATE_, p, l);
            string stateValue = GetBoxStringForKey(box, stateKey);
            return Enum.TryParse(stateValue, ignoreCase: false, out UNLOCKEDSTATE parsedState) &&
                parsedState != UNLOCKEDSTATE.LOCKED &&
                parsedState != UNLOCKEDSTATE.UNLOCKED
                ? parsedState
                : UNLOCKEDSTATE.UNLOCKED;
        }

        public static UNLOCKEDSTATE GetUnlockedForPackLevel(int p, int l)
        {
            return GetUnlockedForPackLevel(GetBoxForPack(p), p, l);
        }

        public static int GetPacksCount()
        {
            int packs = PackConfig.GetPackCount();
            return IsLiteVersion() ? Math.Min(packs, SharewareFreePacks()) : packs;
        }

        public static int GetBoxForPack(int pack)
        {
            return PackConfig.GetSaveSlot(pack);
        }

        public static int GetLevelsInPackCount(int pack)
        {
            int levels = PackConfig.GetLevelCount(pack);
            return IsLiteVersion() ? Math.Min(levels, SharewareFreeLevels()) : levels;
        }

        public static int GetLevelsInPackCount()
        {
            return PackConfig.MaxLevelsPerPack;
        }

        public static int GetTotalStars()
        {
            if (Application.SharedRootController() is CTRRootController rootController)
            {
                int pack = rootController.GetPack();
                return GetTotalStarsInBox(GetBoxForPack(pack));
            }

            return GetTotalStarsInBox(0);
        }

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

        public static int PackUnlockStars(int n)
        {
            return PackConfig.GetUnlockStars(n);
        }

        private static string GetPackLevelKey(string prefs, int p, int l)
        {
            return prefs + p.ToString(CultureInfo.InvariantCulture) + "_" + l.ToString(CultureInfo.InvariantCulture);
        }

        public static void SetUnlockedForPackLevel(int box, UNLOCKEDSTATE s, int p, int l)
        {
            string unlockedKey = GetPackLevelKey(PREFS_UNLOCKED_, p, l);
            string stateKey = GetPackLevelKey(PREFS_UNLOCKED_STATE_, p, l);

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

        public static void SetUnlockedForPackLevel(UNLOCKEDSTATE s, int p, int l)
        {
            SetUnlockedForPackLevel(GetBoxForPack(p), s, p, l);
        }

        public static int SharewareFreeLevels()
        {
            return 10;
        }

        public static int SharewareFreePacks()
        {
            return 2;
        }

        public static void SetLastBox(int p)
        {
            SetIntForKey(p, "PREFS_LAST_BOX", true);
        }

        public static void SetLastGamePack(int b)
        {
            SetIntForKey(b, "PREFS_LAST_GAMEPACK", true);
        }

        public static int GetLastGamePack()
        {
            int val = GetIntForKey("PREFS_LAST_GAMEPACK");
            return val >= 0 ? val : 0;
        }

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

        public static bool IsPackPerfect(int p)
        {
            return IsPackPerfect(GetBoxForPack(p), p);
        }

        public static int GetLastBox()
        {
            int val = GetIntForKey("PREFS_LAST_BOX");
            int maxPack = GetPacksCount();
            // If saved pack is out of range, fall back to first pack
            return (val >= 0 && val <= maxPack) ? val : 0;
        }

        public static void GameViewChanged(string _)
        {
        }

        public static int GetScoreForPackLevel(int box, int p, int l)
        {
            return GetBoxIntForKey(box, GetPackLevelKey(PREFS_SCORE_, p, l));
        }

        public static int GetScoreForPackLevel(int p, int l)
        {
            return GetScoreForPackLevel(GetBoxForPack(p), p, l);
        }

        public static void SetScoreForPackLevel(int box, int s, int p, int l)
        {
            SetBoxIntForKey(box, s, GetPackLevelKey(PREFS_SCORE_, p, l), true);
        }

        public static void SetScoreForPackLevel(int s, int p, int l)
        {
            SetScoreForPackLevel(GetBoxForPack(p), s, p, l);
        }

        public static void SetStarsForPackLevel(int box, int s, int p, int l)
        {
            SetBoxIntForKey(box, s, GetPackLevelKey(PREFS_STARS_, p, l), true);
        }

        public static void SetStarsForPackLevel(int s, int p, int l)
        {
            SetStarsForPackLevel(GetBoxForPack(p), s, p, l);
        }

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

        public static int GetTotalStarsInPack(int p)
        {
            return GetTotalStarsInPack(GetBoxForPack(p), p);
        }

        public static void DisablePlayLevelScroll()
        {
            Application.SharedPreferences().playLevelScroll = false;
        }

        internal static bool ShouldPlayLevelScroll()
        {
            return Application.SharedPreferences().playLevelScroll;
        }

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

                SetBoxBoolForKey(box, true, GetPackLevelKey(PREFS_UNLOCKED_, pack, 0), false);
                RemoveBoxKey(box, GetPackLevelKey(PREFS_UNLOCKED_STATE_, pack, 0));
                changed = true;
            }

            if (changed)
            {
                RequestSave();
            }
        }

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
                SetBoxBoolForKey(box, true, GetPackLevelKey(PREFS_UNLOCKED_, i, 0), false);
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
            SetBooleanForKey(false, "PREFS_CANDY_WAS_CHANGED", true);
            SetBooleanForKey(true, "PREFS_GAME_CENTER_ENABLED", true);
            SetIntForKey(0, "PREFS_NEW_DRAWINGS_COUNTER", true);
            SetIntForKey(0, "PREFS_LAST_BOX", true);
            SetIntForKey(0, "PREFS_LAST_GAMEPACK", true);
            SetBooleanForKey(true, "PREFS_WINDOW_FULLSCREEN", true);
            SetBooleanForKey(true, PREFS_RPC_ENABLED, true);
            SetBooleanForKey(true, PREFS_UPDATE_CHECK, true);
            CheckForUnlockIAP();
            RequestSave();
            SetScoreHash();
        }

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

        private static int GetTotalScore()
        {
            int totalScore = 0;
            for (int i = 0; i < GetPacksCount(); i++)
            {
                for (int j = 0; j < GetLevelsInPackCount(i); j++)
                {
                    totalScore += GetBoxIntForKey(GetBoxForPack(i), GetPackLevelKey(PREFS_SCORE_, i, j));
                }
            }
            return totalScore;
        }

        public static void SetScoreHash()
        {
            string sha256Str = GetSHA256Str(GetTotalScore().ToString(CultureInfo.InvariantCulture));
            SetStringForKey(sha256Str.ToString(), "PREFS_SCORE_HASH", true);
        }

        internal static bool IsFirstLaunch()
        {
            return Application.SharedPreferences().firstLaunch;
        }

        public static void UnlockAllLevels(int stars)
        {
            int i = 0;
            int packsCount = GetPacksCount();
            while (i < packsCount)
            {
                int j = 0;
                int levelsInPackCount = GetLevelsInPackCount(i);
                while (j < levelsInPackCount)
                {
                    int box = GetBoxForPack(i);
                    SetBoxBoolForKey(box, true, GetPackLevelKey(PREFS_UNLOCKED_, i, j), false);
                    RemoveBoxKey(box, GetPackLevelKey(PREFS_UNLOCKED_STATE_, i, j));
                    SetBoxIntForKey(box, stars, GetPackLevelKey(PREFS_STARS_, i, j), false);
                    j++;
                }
                i++;
            }
            RequestSave();
        }

        internal static bool IsScoreHashValid()
        {
            return true;
        }

        public const int VERSION_NUMBER_AT_WHICH_SCORE_HASH_INTRODUCED = 1;

        public const int VERSION_NUMBER = 2;

        public const int MAX_LEVEL_SCORE = 5999;

        public const int MAX_PACK_SCORE = 149999;

        public const int CANDIES_COUNT = 3;

        public const string TWITTER_LINK = "https://mobile.twitter.com/zeptolab";

        public const string FACEBOOK_LINK = "http://www.facebook.com/cuttherope";

        public const string EXPERIMENTS_LINK = "http://www.amazon.com/gp/mas/dl/android?p=com.zeptolab.ctrexperiments.hd.amazon.paid";

        public const int BOXES_CUT_OUT = 0;

        public const int MAX_PACKS = 12;

        public const int MAX_LEVELS_IN_A_PACK = 25;

        public const string PREFS_WINDOW_WIDTH = "PREFS_WINDOW_WIDTH";

        public const string PREFS_WINDOW_HEIGHT = "PREFS_WINDOW_HEIGHT";

        public const string PREFS_WINDOW_FULLSCREEN = "PREFS_WINDOW_FULLSCREEN";

        public const string PREFS_LOCALE = "PREFS_LOCALE";

        public const string PREFS_PREFER_OLD_FONT_SYSTEM = "PREFS_PREFER_OLD_FONT_SYSTEM";

        public const string PREFS_IS_EXIST = "PREFS_EXIST";

        public const string PREFS_RPC_ENABLED = "PREFS_RPC_ENABLED";

        public const string PREFS_UPDATE_CHECK = "PREFS_UPDATE_CHECK";

        public const string PREFS_SOUND_ON = "SOUND_ON";

        public const string PREFS_MUSIC_ON = "MUSIC_ON";

        public const string PREFS_SCORE_ = "SCORE_";

        public const string PREFS_STARS_ = "STARS_";

        public const string PREFS_UNLOCKED_ = "UNLOCKED_";

        public const string PREFS_UNLOCKED_STATE_ = "UNLOCKED_STATE_";

        public const string PREFS_DRAWINGS_ = "DRAWINGS_";

        public const string PREFS_NEW_DRAWINGS_COUNTER = "PREFS_NEW_DRAWINGS_COUNTER";

        public const string PREFS_ROPES_CUT = "PREFS_ROPES_CUT";

        public const string PREFS_ROPES_SHOOT = "PREFS_ROPES_SHOOT";

        public const string PREFS_BUBBLES_POPPED = "PREFS_BUBBLES_POPPED";

        public const string PREFS_SPIDERS_BUSTED = "PREFS_SPIDERS_BUSTED";

        public const string PREFS_CANDIES_LOST = "PREFS_CANDIES_LOST";

        public const string PREFS_CANDIES_UNITED = "PREFS_CANDIES_UNITED";

        public const string PREFS_SOCKS_USED = "PREFS_SOCKS_USED";

        public const string PREFS_LAST_BOX = "PREFS_LAST_BOX";

        public const string PREFS_LAST_GAMEPACK = "PREFS_LAST_GAMEPACK";

        public const string PREFS_CLICK_TO_CUT = "PREFS_CLICK_TO_CUT";

        public const string PREFS_CANDY_WAS_CHANGED = "PREFS_CANDY_WAS_CHANGED";

        public const string PREFS_SELECTED_CANDY = "PREFS_SELECTED_CANDY";

        public const string PREFS_SELECTED_ROPE = "PREFS_SELECTED_ROPE";

        public const string PREFS_SELECTED_OMNOM = "PREFS_SELECTED_OMNOM";

        public const string PREFS_GAME_CENTER_ENABLED = "PREFS_GAME_CENTER_ENABLED";

        public const string PREFS_SCORE_HASH = "PREFS_SCORE_HASH";

        public const string PREFS_VERSION = "PREFS_VERSION";

        public const string PREFS_GAME_STARTS = "PREFS_GAME_STARTS";

        public const string PREFS_LEVELS_WON = "PREFS_LEVELS_WON";

        public const string PREFS_IAP_UNLOCK = "IAP_UNLOCK";

        public const string PREFS_IAP_BANNERS = "IAP_BANNERS";

        public const string PREFS_IAP_SHAREWARE = "IAP_SHAREWARE";

        public const string acBronzeScissors = "677900534";

        public const string acSilverScissors = "681508185";

        public const string acGoldenScissors = "681473653";

        public const string acRopeCutter = "681461850";

        public const string acRopeCutterManiac = "681457931";

        public const string acUltimateRopeCutter = "1058248892";

        public const string acQuickFinger = "681464917";

        public const string acMasterFinger = "681508316";

        public const string acBubblePopper = "681513183";

        public const string acBubbleMaster = "1058345234";

        public const string acSpiderBuster = "681486608";

        public const string acSpiderTamer = "1058341284";

        public const string acWeightLoser = "681497443";

        public const string acCalorieMinimizer = "1058341297";

        public const string acTummyTeaser = "1058281905";

        public const string acRomanticSoul = "1432722351";

        public const string acMagician = "1515796440";

        public const string acCardboardBox = "681486798";

        public const string acCardboardPerfect = "1058364368";

        public const string acFabricBox = "681462993";

        public const string acFabricPerfect = "1058328727";

        public const string acFoilBox = "681520253";

        public const string acFoilPerfect = "1058324751";

        public const string acGiftBox = "681512374";

        public const string acGiftPerfect = "1058327768";

        public const string acCosmicBox = "1058310903";

        public const string acCosmicPerfect = "1058407145";

        public const string acValentineBox = "1432721430";

        public const string acValentinePerfect = "1432760157";

        public const string acMagicBox = "1515813296";

        public const string acMagicPerfect = "1515793567";

        public const string acToyBox = "1991474812";

        public const string acToyPerfect = "1991641832";

        public const string acToolBox = "1321820679";

        public const string acToolPerfect = "1335599628";

        public const string acBuzzBox = "23523272771";

        public const string acBuzzPerfect = "99928734496";

        public const string acDJBox = "com.zeptolab.ctr.djboxcompleted";

        public const string acDJPerfect = "com.zeptolab.ctr.djboxperfect";

        public const string acSpookyBox = "com.zeptolab.ctr.spookyboxcompleted";

        public const string acSpookyPerfect = "com.zeptolab.ctr.spookyboxperfect";

        public RemoteDataManager remoteDataManager = new();

        private readonly bool firstLaunch;

        private bool playLevelScroll;
    }
}
