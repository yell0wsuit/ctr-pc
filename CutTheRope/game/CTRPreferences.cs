using CutTheRope.ctr_commons;
using CutTheRope.iframework.core;
using CutTheRope.ios;
using System;

namespace CutTheRope.game
{
    internal class CTRPreferences : Preferences
    {
        public override NSObject init()
        {
            if (base.init() != null)
            {
                if (!getBooleanForKey("PREFS_EXIST"))
                {
                    setBooleanforKey(true, "PREFS_EXIST", true);
                    setIntforKey(0, "PREFS_GAME_STARTS", true);
                    setIntforKey(0, "PREFS_LEVELS_WON", true);
                    resetToDefaults();
                    resetMusicSound();
                    firstLaunch = true;
                    playLevelScroll = false;
                }
                else
                {
                    if (getIntForKey("PREFS_VERSION") < 1)
                    {
                        getTotalScore();
                        int i = 0;
                        int packsCount = getPacksCount();
                        while (i < packsCount)
                        {
                            int num = 0;
                            int j = 0;
                            int levelsInPackCount = getLevelsInPackCount();
                            while (j < levelsInPackCount)
                            {
                                int intForKey2 = getIntForKey(getPackLevelKey("SCORE_", i, j));
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
                                resetToDefaults();
                                resetMusicSound();
                                break;
                            }
                            i++;
                        }
                        setScoreHash();
                    }
                    firstLaunch = false;
                    playLevelScroll = false;
                }
                setIntforKey(2, "PREFS_VERSION", true);
            }
            return this;
        }

        private void resetMusicSound()
        {
            setBooleanforKey(true, "SOUND_ON", true);
            setBooleanforKey(true, "MUSIC_ON", true);
        }

        private static bool isShareware()
        {
            return false;
        }

        public static bool isSharewareUnlocked()
        {
            bool flag = isShareware();
            return !flag || (flag && _getBooleanForKey("IAP_SHAREWARE"));
        }

        public static bool isLiteVersion()
        {
            return false;
        }

        public static bool isBannersMustBeShown()
        {
            return false;
        }

        public static int getStarsForPackLevel(int p, int l)
        {
            return _getIntForKey(getPackLevelKey("STARS_", p, l));
        }

        public static UNLOCKED_STATE getUnlockedForPackLevel(int p, int l)
        {
            return (UNLOCKED_STATE)_getIntForKey(getPackLevelKey("UNLOCKED_", p, l));
        }

        public static int getPacksCount()
        {
            return !isLiteVersion() ? 11 : 2;
        }

        public static int getLevelsInPackCount()
        {
            return !isLiteVersion() ? 25 : 9;
        }

        public static int getTotalStars()
        {
            int num = 0;
            int i = 0;
            int packsCount = getPacksCount();
            while (i < packsCount)
            {
                int j = 0;
                int levelsInPackCount = getLevelsInPackCount();
                while (j < levelsInPackCount)
                {
                    num += getStarsForPackLevel(i, j);
                    j++;
                }
                i++;
            }
            return num;
        }

        public static int packUnlockStars(int n)
        {
            return !isLiteVersion() ? PACK_UNLOCK_STARS[n] : PACK_UNLOCK_STARS_LITE[n];
        }

        private static string getPackLevelKey(string prefs, int p, int l)
        {
            return prefs + p.ToString() + "_" + l.ToString();
        }

        public static void setUnlockedForPackLevel(UNLOCKED_STATE s, int p, int l)
        {
            _setIntforKey((int)s, getPackLevelKey("UNLOCKED_", p, l), true);
        }

        public static int sharewareFreeLevels()
        {
            return 10;
        }

        public static int sharewareFreePacks()
        {
            return 2;
        }

        public static void setLastPack(int p)
        {
            _setIntforKey(p, "PREFS_LAST_PACK", true);
        }

        public static bool isPackPerfect(int p)
        {
            int i = 0;
            int levelsInPackCount = getLevelsInPackCount();
            while (i < levelsInPackCount)
            {
                if (getStarsForPackLevel(p, i) < 3)
                {
                    return false;
                }
                i++;
            }
            return true;
        }

        public static int getLastPack()
        {
            int val = _getIntForKey("PREFS_LAST_PACK");
            return Math.Min(Math.Max(0, val), getPacksCount() + 1);
        }

        public static void gameViewChanged(NSString NameOfView)
        {
        }

        public static int getScoreForPackLevel(int p, int l)
        {
            return _getIntForKey("SCORE_" + p.ToString() + "_" + l.ToString());
        }

        public static void setScoreForPackLevel(int s, int p, int l)
        {
            _setIntforKey(s, "SCORE_" + p.ToString() + "_" + l.ToString(), true);
        }

        public static void setStarsForPackLevel(int s, int p, int l)
        {
            _setIntforKey(s, "STARS_" + p.ToString() + "_" + l.ToString(), true);
        }

        public static int getTotalStarsInPack(int p)
        {
            int num = 0;
            int i = 0;
            int levelsInPackCount = getLevelsInPackCount();
            while (i < levelsInPackCount)
            {
                num += getStarsForPackLevel(p, i);
                i++;
            }
            return num;
        }

        public static void disablePlayLevelScroll()
        {
            Application.sharedPreferences().playLevelScroll = false;
        }

        internal static bool shouldPlayLevelScroll()
        {
            return Application.sharedPreferences().playLevelScroll;
        }

        public void resetToDefaults()
        {
            int i = 0;
            int packsCount = getPacksCount();
            while (i < packsCount)
            {
                int j = 0;
                int levelsInPackCount = getLevelsInPackCount();
                while (j < levelsInPackCount)
                {
                    int v = ((i == 0 || (isShareware() && i < sharewareFreePacks())) && j == 0) ? 1 : 0;
                    setIntforKey(0, getPackLevelKey("SCORE_", i, j), false);
                    setIntforKey(0, getPackLevelKey("STARS_", i, j), false);
                    setIntforKey(v, getPackLevelKey("UNLOCKED_", i, j), false);
                    j++;
                }
                i++;
            }
            setIntforKey(0, "PREFS_ROPES_CUT", true);
            setIntforKey(0, "PREFS_BUBBLES_POPPED", true);
            setIntforKey(0, "PREFS_SPIDERS_BUSTED", true);
            setIntforKey(0, "PREFS_CANDIES_LOST", true);
            setIntforKey(0, "PREFS_CANDIES_UNITED", true);
            setIntforKey(0, "PREFS_SOCKS_USED", true);
            setIntforKey(0, "PREFS_SELECTED_CANDY", true);
            setBooleanforKey(false, "PREFS_CANDY_WAS_CHANGED", true);
            setBooleanforKey(true, "PREFS_GAME_CENTER_ENABLED", true);
            setIntforKey(0, "PREFS_NEW_DRAWINGS_COUNTER", true);
            setIntforKey(0, "PREFS_LAST_PACK", true);
            setBooleanforKey(true, "PREFS_WINDOW_FULLSCREEN", true);
            checkForUnlockIAP();
            savePreferences();
            setScoreHash();
        }

        private void checkForUnlockIAP()
        {
            if (!getBooleanForKey("IAP_UNLOCK"))
            {
                return;
            }
            int i = 0;
            int packsCount = getPacksCount();
            while (i < packsCount)
            {
                if (getUnlockedForPackLevel(i, 0) == UNLOCKED_STATE.UNLOCKED_STATE_LOCKED)
                {
                    setUnlockedForPackLevel(UNLOCKED_STATE.UNLOCKED_STATE_JUST_UNLOCKED, i, 0);
                }
                i++;
            }
        }

        private int getTotalScore()
        {
            int num = 0;
            for (int i = 0; i < getPacksCount(); i++)
            {
                for (int j = 0; j < getLevelsInPackCount(); j++)
                {
                    num += getIntForKey(getPackLevelKey("SCORE_", i, j));
                }
            }
            return num;
        }

        public void setScoreHash()
        {
            NSString mD5Str = getMD5Str(NSS(getTotalScore().ToString()));
            setStringforKey(mD5Str.ToString(), "PREFS_SCORE_HASH", true);
        }

        internal static bool isFirstLaunch()
        {
            return Application.sharedPreferences().firstLaunch;
        }

        public void unlockAllLevels(int stars)
        {
            int i = 0;
            int packsCount = getPacksCount();
            while (i < packsCount)
            {
                int j = 0;
                int levelsInPackCount = getLevelsInPackCount();
                while (j < levelsInPackCount)
                {
                    setIntforKey(1, getPackLevelKey("UNLOCKED_", i, j), false);
                    setIntforKey(stars, getPackLevelKey("STARS_", i, j), false);
                    j++;
                }
                i++;
            }
            savePreferences();
        }

        internal bool isScoreHashValid()
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

        private bool firstLaunch;

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
