using System;
using System.Globalization;

using CutTheRope.commons;
using CutTheRope.iframework.core;

namespace CutTheRope.game
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
                        int num = 0;
                        int j = 0;
                        int levelsInPackCount = GetLevelsInPackCount();
                        while (j < levelsInPackCount)
                        {
                            int intForKey2 = GetIntForKey(GetPackLevelKey("SCORE_", i, j));
                            if (intForKey2 > 5999)
                            {
                                num = 150000;
                                break;
                            }
                            num += intForKey2;
                            j++;
                        }
                        if (num > 149999)
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
        }

        private static void ResetMusicSound()
        {
            SetBooleanForKey(true, "SOUND_ON", true);
            SetBooleanForKey(true, "MUSIC_ON", true);
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

        public static int GetStarsForPackLevel(int p, int l)
        {
            return GetIntForKey(GetPackLevelKey("STARS_", p, l));
        }

        public static UNLOCKEDSTATE GetUnlockedForPackLevel(int p, int l)
        {
            return (UNLOCKEDSTATE)GetIntForKey(GetPackLevelKey("UNLOCKED_", p, l));
        }

        public static int GetPacksCount()
        {
            return !IsLiteVersion() ? 11 : 2;
        }

        public static int GetLevelsInPackCount()
        {
            return !IsLiteVersion() ? 25 : 9;
        }

        public static int GetTotalStars()
        {
            int num = 0;
            int i = 0;
            int packsCount = GetPacksCount();
            while (i < packsCount)
            {
                int j = 0;
                int levelsInPackCount = GetLevelsInPackCount();
                while (j < levelsInPackCount)
                {
                    num += GetStarsForPackLevel(i, j);
                    j++;
                }
                i++;
            }
            return num;
        }

        public static int PackUnlockStars(int n)
        {
            return !IsLiteVersion() ? PACK_UNLOCK_STARS[n] : PACK_UNLOCK_STARS_LITE[n];
        }

        private static string GetPackLevelKey(string prefs, int p, int l)
        {
            return prefs + p.ToString(CultureInfo.InvariantCulture) + "_" + l.ToString(CultureInfo.InvariantCulture);
        }

        public static void SetUnlockedForPackLevel(UNLOCKEDSTATE s, int p, int l)
        {
            SetIntForKey((int)s, GetPackLevelKey("UNLOCKED_", p, l), true);
        }

        public static int SharewareFreeLevels()
        {
            return 10;
        }

        public static int SharewareFreePacks()
        {
            return 2;
        }

        public static void SetLastPack(int p)
        {
            SetIntForKey(p, "PREFS_LAST_PACK", true);
        }

        public static bool IsPackPerfect(int p)
        {
            int i = 0;
            int levelsInPackCount = GetLevelsInPackCount();
            while (i < levelsInPackCount)
            {
                if (GetStarsForPackLevel(p, i) < 3)
                {
                    return false;
                }
                i++;
            }
            return true;
        }

        public static int GetLastPack()
        {
            int val = GetIntForKey("PREFS_LAST_PACK");
            return Math.Min(Math.Max(0, val), GetPacksCount() + 1);
        }

        public static void GameViewChanged(string NameOfView)
        {
        }

        public static int GetScoreForPackLevel(int p, int l)
        {
            return GetIntForKey("SCORE_" + p.ToString(CultureInfo.InvariantCulture) + "_" + l.ToString(CultureInfo.InvariantCulture));
        }

        public static void SetScoreForPackLevel(int s, int p, int l)
        {
            SetIntForKey(s, "SCORE_" + p.ToString(CultureInfo.InvariantCulture) + "_" + l.ToString(CultureInfo.InvariantCulture), true);
        }

        public static void SetStarsForPackLevel(int s, int p, int l)
        {
            SetIntForKey(s, "STARS_" + p.ToString(CultureInfo.InvariantCulture) + "_" + l.ToString(CultureInfo.InvariantCulture), true);
        }

        public static int GetTotalStarsInPack(int p)
        {
            int num = 0;
            int i = 0;
            int levelsInPackCount = GetLevelsInPackCount();
            while (i < levelsInPackCount)
            {
                num += GetStarsForPackLevel(p, i);
                i++;
            }
            return num;
        }

        public static void DisablePlayLevelScroll()
        {
            Application.SharedPreferences().playLevelScroll = false;
        }

        internal static bool ShouldPlayLevelScroll()
        {
            return Application.SharedPreferences().playLevelScroll;
        }

        public static void ResetToDefaults()
        {
            int i = 0;
            int packsCount = GetPacksCount();
            while (i < packsCount)
            {
                int j = 0;
                int levelsInPackCount = GetLevelsInPackCount();
                while (j < levelsInPackCount)
                {
                    int v = (i == 0 || (IsShareware() && i < SharewareFreePacks())) && j == 0 ? 1 : 0;
                    SetIntForKey(0, GetPackLevelKey("SCORE_", i, j), false);
                    SetIntForKey(0, GetPackLevelKey("STARS_", i, j), false);
                    SetIntForKey(v, GetPackLevelKey("UNLOCKED_", i, j), false);
                    j++;
                }
                i++;
            }
            SetIntForKey(0, "PREFS_ROPES_CUT", true);
            SetIntForKey(0, "PREFS_BUBBLES_POPPED", true);
            SetIntForKey(0, "PREFS_SPIDERS_BUSTED", true);
            SetIntForKey(0, "PREFS_CANDIES_LOST", true);
            SetIntForKey(0, "PREFS_CANDIES_UNITED", true);
            SetIntForKey(0, "PREFS_SOCKS_USED", true);
            SetIntForKey(0, "PREFS_SELECTED_CANDY", true);
            SetBooleanForKey(false, "PREFS_CANDY_WAS_CHANGED", true);
            SetBooleanForKey(true, "PREFS_GAME_CENTER_ENABLED", true);
            SetIntForKey(0, "PREFS_NEW_DRAWINGS_COUNTER", true);
            SetIntForKey(0, "PREFS_LAST_PACK", true);
            SetBooleanForKey(true, "PREFS_WINDOW_FULLSCREEN", true);
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
                if (GetUnlockedForPackLevel(i, 0) == UNLOCKEDSTATE.LOCKED)
                {
                    SetUnlockedForPackLevel(UNLOCKEDSTATE.JUSTUNLOCKED, i, 0);
                }
                i++;
            }
        }

        private static int GetTotalScore()
        {
            int num = 0;
            for (int i = 0; i < GetPacksCount(); i++)
            {
                for (int j = 0; j < GetLevelsInPackCount(); j++)
                {
                    num += GetIntForKey(GetPackLevelKey("SCORE_", i, j));
                }
            }
            return num;
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
                int levelsInPackCount = GetLevelsInPackCount();
                while (j < levelsInPackCount)
                {
                    SetIntForKey(1, GetPackLevelKey("UNLOCKED_", i, j), false);
                    SetIntForKey(stars, GetPackLevelKey("STARS_", i, j), false);
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

        public const string PREFS_IS_EXIST = "PREFS_EXIST";

        public const string PREFS_SOUND_ON = "SOUND_ON";

        public const string PREFS_MUSIC_ON = "MUSIC_ON";

        public const string PREFS_SCORE_ = "SCORE_";

        public const string PREFS_STARS_ = "STARS_";

        public const string PREFS_UNLOCKED_ = "UNLOCKED_";

        public const string PREFS_DRAWINGS_ = "DRAWINGS_";

        public const string PREFS_NEW_DRAWINGS_COUNTER = "PREFS_NEW_DRAWINGS_COUNTER";

        public const string PREFS_ROPES_CUT = "PREFS_ROPES_CUT";

        public const string PREFS_BUBBLES_POPPED = "PREFS_BUBBLES_POPPED";

        public const string PREFS_SPIDERS_BUSTED = "PREFS_SPIDERS_BUSTED";

        public const string PREFS_CANDIES_LOST = "PREFS_CANDIES_LOST";

        public const string PREFS_CANDIES_UNITED = "PREFS_CANDIES_UNITED";

        public const string PREFS_SOCKS_USED = "PREFS_SOCKS_USED";

        public const string PREFS_LAST_PACK = "PREFS_LAST_PACK";

        public const string PREFS_CLICK_TO_CUT = "PREFS_CLICK_TO_CUT";

        public const string PREFS_CANDY_WAS_CHANGED = "PREFS_CANDY_WAS_CHANGED";

        public const string PREFS_SELECTED_CANDY = "PREFS_SELECTED_CANDY";

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

        public RemoteDataManager remoteDataManager = new();

        private readonly bool firstLaunch;

        private bool playLevelScroll;

        private static readonly int[] PACK_UNLOCK_STARS_LITE =
[
    0, 20, 80, 170, 240, 300, 350, 400, 450, 500,
            550
];

        private static readonly int[] PACK_UNLOCK_STARS =
[
    0, 30, 80, 170, 240, 300, 350, 400, 450, 500,
            550
];
    }
}
