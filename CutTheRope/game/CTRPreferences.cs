using CutTheRope.ctr_commons;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.ios;
using System;

namespace CutTheRope.game
{
    // Token: 0x02000077 RID: 119
    internal class CTRPreferences : Preferences
    {
        // Token: 0x06000485 RID: 1157 RVA: 0x0001A2F4 File Offset: 0x000184F4
        public override NSObject init()
        {
            if (base.init() != null)
            {
                if (!this.getBooleanForKey("PREFS_EXIST"))
                {
                    this.setBooleanforKey(true, "PREFS_EXIST", true);
                    this.setIntforKey(0, "PREFS_GAME_STARTS", true);
                    this.setIntforKey(0, "PREFS_LEVELS_WON", true);
                    this.resetToDefaults();
                    this.resetMusicSound();
                    this.firstLaunch = true;
                    this.playLevelScroll = false;
                }
                else
                {
                    if (this.getIntForKey("PREFS_VERSION") < 1)
                    {
                        this.getTotalScore();
                        int i = 0;
                        int packsCount = CTRPreferences.getPacksCount();
                        while (i < packsCount)
                        {
                            int num = 0;
                            int j = 0;
                            int levelsInPackCount = CTRPreferences.getLevelsInPackCount();
                            while (j < levelsInPackCount)
                            {
                                int intForKey2 = this.getIntForKey(CTRPreferences.getPackLevelKey("SCORE_", i, j));
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
                                this.resetToDefaults();
                                this.resetMusicSound();
                                break;
                            }
                            i++;
                        }
                        this.setScoreHash();
                    }
                    this.firstLaunch = false;
                    this.playLevelScroll = false;
                }
                this.setIntforKey(2, "PREFS_VERSION", true);
            }
            return this;
        }

        // Token: 0x06000486 RID: 1158 RVA: 0x0001A3FE File Offset: 0x000185FE
        private void resetMusicSound()
        {
            this.setBooleanforKey(true, "SOUND_ON", true);
            this.setBooleanforKey(true, "MUSIC_ON", true);
        }

        // Token: 0x06000487 RID: 1159 RVA: 0x0001A41A File Offset: 0x0001861A
        private static bool isShareware()
        {
            return false;
        }

        // Token: 0x06000488 RID: 1160 RVA: 0x0001A420 File Offset: 0x00018620
        public static bool isSharewareUnlocked()
        {
            bool flag = CTRPreferences.isShareware();
            return !flag || (flag && Preferences._getBooleanForKey("IAP_SHAREWARE"));
        }

        // Token: 0x06000489 RID: 1161 RVA: 0x0001A447 File Offset: 0x00018647
        public static bool isLiteVersion()
        {
            return false;
        }

        // Token: 0x0600048A RID: 1162 RVA: 0x0001A44A File Offset: 0x0001864A
        public static bool isBannersMustBeShown()
        {
            return false;
        }

        // Token: 0x0600048B RID: 1163 RVA: 0x0001A44D File Offset: 0x0001864D
        public static int getStarsForPackLevel(int p, int l)
        {
            return Preferences._getIntForKey(CTRPreferences.getPackLevelKey("STARS_", p, l));
        }

        // Token: 0x0600048C RID: 1164 RVA: 0x0001A460 File Offset: 0x00018660
        public static UNLOCKED_STATE getUnlockedForPackLevel(int p, int l)
        {
            return (UNLOCKED_STATE)Preferences._getIntForKey(CTRPreferences.getPackLevelKey("UNLOCKED_", p, l));
        }

        // Token: 0x0600048D RID: 1165 RVA: 0x0001A473 File Offset: 0x00018673
        public static int getPacksCount()
        {
            if (!CTRPreferences.isLiteVersion())
            {
                return 11;
            }
            return 2;
        }

        // Token: 0x0600048E RID: 1166 RVA: 0x0001A480 File Offset: 0x00018680
        public static int getLevelsInPackCount()
        {
            if (!CTRPreferences.isLiteVersion())
            {
                return 25;
            }
            return 9;
        }

        // Token: 0x0600048F RID: 1167 RVA: 0x0001A490 File Offset: 0x00018690
        public static int getTotalStars()
        {
            int num = 0;
            int i = 0;
            int packsCount = CTRPreferences.getPacksCount();
            while (i < packsCount)
            {
                int j = 0;
                int levelsInPackCount = CTRPreferences.getLevelsInPackCount();
                while (j < levelsInPackCount)
                {
                    num += CTRPreferences.getStarsForPackLevel(i, j);
                    j++;
                }
                i++;
            }
            return num;
        }

        // Token: 0x06000490 RID: 1168 RVA: 0x0001A4D0 File Offset: 0x000186D0
        public static int packUnlockStars(int n)
        {
            if (!CTRPreferences.isLiteVersion())
            {
                return CTRPreferences.PACK_UNLOCK_STARS[n];
            }
            return CTRPreferences.PACK_UNLOCK_STARS_LITE[n];
        }

        // Token: 0x06000491 RID: 1169 RVA: 0x0001A4E8 File Offset: 0x000186E8
        private static string getPackLevelKey(string prefs, int p, int l)
        {
            return prefs + p.ToString() + "_" + l.ToString();
        }

        // Token: 0x06000492 RID: 1170 RVA: 0x0001A503 File Offset: 0x00018703
        public static void setUnlockedForPackLevel(UNLOCKED_STATE s, int p, int l)
        {
            Preferences._setIntforKey((int)s, CTRPreferences.getPackLevelKey("UNLOCKED_", p, l), true);
        }

        // Token: 0x06000493 RID: 1171 RVA: 0x0001A518 File Offset: 0x00018718
        public static int sharewareFreeLevels()
        {
            return 10;
        }

        // Token: 0x06000494 RID: 1172 RVA: 0x0001A51C File Offset: 0x0001871C
        public static int sharewareFreePacks()
        {
            return 2;
        }

        // Token: 0x06000495 RID: 1173 RVA: 0x0001A51F File Offset: 0x0001871F
        public static void setLastPack(int p)
        {
            Preferences._setIntforKey(p, "PREFS_LAST_PACK", true);
        }

        // Token: 0x06000496 RID: 1174 RVA: 0x0001A530 File Offset: 0x00018730
        public static bool isPackPerfect(int p)
        {
            int i = 0;
            int levelsInPackCount = CTRPreferences.getLevelsInPackCount();
            while (i < levelsInPackCount)
            {
                if (CTRPreferences.getStarsForPackLevel(p, i) < 3)
                {
                    return false;
                }
                i++;
            }
            return true;
        }

        // Token: 0x06000497 RID: 1175 RVA: 0x0001A55C File Offset: 0x0001875C
        public static int getLastPack()
        {
            int val = Preferences._getIntForKey("PREFS_LAST_PACK");
            return Math.Min(Math.Max(0, val), CTRPreferences.getPacksCount() + 1);
        }

        // Token: 0x06000498 RID: 1176 RVA: 0x0001A587 File Offset: 0x00018787
        public static void gameViewChanged(NSString NameOfView)
        {
        }

        // Token: 0x06000499 RID: 1177 RVA: 0x0001A589 File Offset: 0x00018789
        public static int getScoreForPackLevel(int p, int l)
        {
            return Preferences._getIntForKey("SCORE_" + p.ToString() + "_" + l.ToString());
        }

        // Token: 0x0600049A RID: 1178 RVA: 0x0001A5AD File Offset: 0x000187AD
        public static void setScoreForPackLevel(int s, int p, int l)
        {
            Preferences._setIntforKey(s, "SCORE_" + p.ToString() + "_" + l.ToString(), true);
        }

        // Token: 0x0600049B RID: 1179 RVA: 0x0001A5D3 File Offset: 0x000187D3
        public static void setStarsForPackLevel(int s, int p, int l)
        {
            Preferences._setIntforKey(s, "STARS_" + p.ToString() + "_" + l.ToString(), true);
        }

        // Token: 0x0600049C RID: 1180 RVA: 0x0001A5FC File Offset: 0x000187FC
        public static int getTotalStarsInPack(int p)
        {
            int num = 0;
            int i = 0;
            int levelsInPackCount = CTRPreferences.getLevelsInPackCount();
            while (i < levelsInPackCount)
            {
                num += CTRPreferences.getStarsForPackLevel(p, i);
                i++;
            }
            return num;
        }

        // Token: 0x0600049D RID: 1181 RVA: 0x0001A628 File Offset: 0x00018828
        public static void disablePlayLevelScroll()
        {
            Application.sharedPreferences().playLevelScroll = false;
        }

        // Token: 0x0600049E RID: 1182 RVA: 0x0001A635 File Offset: 0x00018835
        internal static bool shouldPlayLevelScroll()
        {
            return Application.sharedPreferences().playLevelScroll;
        }

        // Token: 0x0600049F RID: 1183 RVA: 0x0001A644 File Offset: 0x00018844
        public void resetToDefaults()
        {
            int i = 0;
            int packsCount = CTRPreferences.getPacksCount();
            while (i < packsCount)
            {
                int j = 0;
                int levelsInPackCount = CTRPreferences.getLevelsInPackCount();
                while (j < levelsInPackCount)
                {
                    int v = (((i == 0 || (CTRPreferences.isShareware() && i < CTRPreferences.sharewareFreePacks())) && j == 0) ? 1 : 0);
                    this.setIntforKey(0, CTRPreferences.getPackLevelKey("SCORE_", i, j), false);
                    this.setIntforKey(0, CTRPreferences.getPackLevelKey("STARS_", i, j), false);
                    this.setIntforKey(v, CTRPreferences.getPackLevelKey("UNLOCKED_", i, j), false);
                    j++;
                }
                i++;
            }
            this.setIntforKey(0, "PREFS_ROPES_CUT", true);
            this.setIntforKey(0, "PREFS_BUBBLES_POPPED", true);
            this.setIntforKey(0, "PREFS_SPIDERS_BUSTED", true);
            this.setIntforKey(0, "PREFS_CANDIES_LOST", true);
            this.setIntforKey(0, "PREFS_CANDIES_UNITED", true);
            this.setIntforKey(0, "PREFS_SOCKS_USED", true);
            this.setIntforKey(0, "PREFS_SELECTED_CANDY", true);
            this.setBooleanforKey(false, "PREFS_CANDY_WAS_CHANGED", true);
            this.setBooleanforKey(true, "PREFS_GAME_CENTER_ENABLED", true);
            this.setIntforKey(0, "PREFS_NEW_DRAWINGS_COUNTER", true);
            this.setIntforKey(0, "PREFS_LAST_PACK", true);
            this.setBooleanforKey(true, "PREFS_WINDOW_FULLSCREEN", true);
            this.checkForUnlockIAP();
            this.savePreferences();
            this.setScoreHash();
        }

        // Token: 0x060004A0 RID: 1184 RVA: 0x0001A77C File Offset: 0x0001897C
        private void checkForUnlockIAP()
        {
            if (!this.getBooleanForKey("IAP_UNLOCK"))
            {
                return;
            }
            int i = 0;
            int packsCount = CTRPreferences.getPacksCount();
            while (i < packsCount)
            {
                if (CTRPreferences.getUnlockedForPackLevel(i, 0) == UNLOCKED_STATE.UNLOCKED_STATE_LOCKED)
                {
                    CTRPreferences.setUnlockedForPackLevel(UNLOCKED_STATE.UNLOCKED_STATE_JUST_UNLOCKED, i, 0);
                }
                i++;
            }
        }

        // Token: 0x060004A1 RID: 1185 RVA: 0x0001A7BC File Offset: 0x000189BC
        private int getTotalScore()
        {
            int num = 0;
            for (int i = 0; i < CTRPreferences.getPacksCount(); i++)
            {
                for (int j = 0; j < CTRPreferences.getLevelsInPackCount(); j++)
                {
                    num += this.getIntForKey(CTRPreferences.getPackLevelKey("SCORE_", i, j));
                }
            }
            return num;
        }

        // Token: 0x060004A2 RID: 1186 RVA: 0x0001A804 File Offset: 0x00018A04
        public void setScoreHash()
        {
            NSString mD5Str = MathHelper.getMD5Str(NSObject.NSS(this.getTotalScore().ToString()));
            this.setStringforKey(mD5Str.ToString(), "PREFS_SCORE_HASH", true);
        }

        // Token: 0x060004A3 RID: 1187 RVA: 0x0001A83C File Offset: 0x00018A3C
        internal static bool isFirstLaunch()
        {
            return Application.sharedPreferences().firstLaunch;
        }

        // Token: 0x060004A4 RID: 1188 RVA: 0x0001A848 File Offset: 0x00018A48
        public void unlockAllLevels(int stars)
        {
            int i = 0;
            int packsCount = CTRPreferences.getPacksCount();
            while (i < packsCount)
            {
                int j = 0;
                int levelsInPackCount = CTRPreferences.getLevelsInPackCount();
                while (j < levelsInPackCount)
                {
                    this.setIntforKey(1, CTRPreferences.getPackLevelKey("UNLOCKED_", i, j), false);
                    this.setIntforKey(stars, CTRPreferences.getPackLevelKey("STARS_", i, j), false);
                    j++;
                }
                i++;
            }
            this.savePreferences();
        }

        // Token: 0x060004A5 RID: 1189 RVA: 0x0001A8A7 File Offset: 0x00018AA7
        internal bool isScoreHashValid()
        {
            return true;
        }

        // Token: 0x04000318 RID: 792
        public const int VERSION_NUMBER_AT_WHICH_SCORE_HASH_INTRODUCED = 1;

        // Token: 0x04000319 RID: 793
        public const int VERSION_NUMBER = 2;

        // Token: 0x0400031A RID: 794
        public const int MAX_LEVEL_SCORE = 5999;

        // Token: 0x0400031B RID: 795
        public const int MAX_PACK_SCORE = 149999;

        // Token: 0x0400031C RID: 796
        public const int CANDIES_COUNT = 3;

        // Token: 0x0400031D RID: 797
        public const string TWITTER_LINK = "https://mobile.twitter.com/zeptolab";

        // Token: 0x0400031E RID: 798
        public const string FACEBOOK_LINK = "http://www.facebook.com/cuttherope";

        // Token: 0x0400031F RID: 799
        public const string EXPERIMENTS_LINK = "http://www.amazon.com/gp/mas/dl/android?p=com.zeptolab.ctrexperiments.hd.amazon.paid";

        // Token: 0x04000320 RID: 800
        public const int BOXES_CUT_OUT = 0;

        // Token: 0x04000321 RID: 801
        public const int MAX_PACKS = 12;

        // Token: 0x04000322 RID: 802
        public const int MAX_LEVELS_IN_A_PACK = 25;

        // Token: 0x04000323 RID: 803
        public const string PREFS_WINDOW_WIDTH = "PREFS_WINDOW_WIDTH";

        // Token: 0x04000324 RID: 804
        public const string PREFS_WINDOW_HEIGHT = "PREFS_WINDOW_HEIGHT";

        // Token: 0x04000325 RID: 805
        public const string PREFS_WINDOW_FULLSCREEN = "PREFS_WINDOW_FULLSCREEN";

        // Token: 0x04000326 RID: 806
        public const string PREFS_LOCALE = "PREFS_LOCALE";

        // Token: 0x04000327 RID: 807
        public const string PREFS_IS_EXIST = "PREFS_EXIST";

        // Token: 0x04000328 RID: 808
        public const string PREFS_SOUND_ON = "SOUND_ON";

        // Token: 0x04000329 RID: 809
        public const string PREFS_MUSIC_ON = "MUSIC_ON";

        // Token: 0x0400032A RID: 810
        public const string PREFS_SCORE_ = "SCORE_";

        // Token: 0x0400032B RID: 811
        public const string PREFS_STARS_ = "STARS_";

        // Token: 0x0400032C RID: 812
        public const string PREFS_UNLOCKED_ = "UNLOCKED_";

        // Token: 0x0400032D RID: 813
        public const string PREFS_DRAWINGS_ = "DRAWINGS_";

        // Token: 0x0400032E RID: 814
        public const string PREFS_NEW_DRAWINGS_COUNTER = "PREFS_NEW_DRAWINGS_COUNTER";

        // Token: 0x0400032F RID: 815
        public const string PREFS_ROPES_CUT = "PREFS_ROPES_CUT";

        // Token: 0x04000330 RID: 816
        public const string PREFS_BUBBLES_POPPED = "PREFS_BUBBLES_POPPED";

        // Token: 0x04000331 RID: 817
        public const string PREFS_SPIDERS_BUSTED = "PREFS_SPIDERS_BUSTED";

        // Token: 0x04000332 RID: 818
        public const string PREFS_CANDIES_LOST = "PREFS_CANDIES_LOST";

        // Token: 0x04000333 RID: 819
        public const string PREFS_CANDIES_UNITED = "PREFS_CANDIES_UNITED";

        // Token: 0x04000334 RID: 820
        public const string PREFS_SOCKS_USED = "PREFS_SOCKS_USED";

        // Token: 0x04000335 RID: 821
        public const string PREFS_LAST_PACK = "PREFS_LAST_PACK";

        // Token: 0x04000336 RID: 822
        public const string PREFS_CLICK_TO_CUT = "PREFS_CLICK_TO_CUT";

        // Token: 0x04000337 RID: 823
        public const string PREFS_CANDY_WAS_CHANGED = "PREFS_CANDY_WAS_CHANGED";

        // Token: 0x04000338 RID: 824
        public const string PREFS_SELECTED_CANDY = "PREFS_SELECTED_CANDY";

        // Token: 0x04000339 RID: 825
        public const string PREFS_GAME_CENTER_ENABLED = "PREFS_GAME_CENTER_ENABLED";

        // Token: 0x0400033A RID: 826
        public const string PREFS_SCORE_HASH = "PREFS_SCORE_HASH";

        // Token: 0x0400033B RID: 827
        public const string PREFS_VERSION = "PREFS_VERSION";

        // Token: 0x0400033C RID: 828
        public const string PREFS_GAME_STARTS = "PREFS_GAME_STARTS";

        // Token: 0x0400033D RID: 829
        public const string PREFS_LEVELS_WON = "PREFS_LEVELS_WON";

        // Token: 0x0400033E RID: 830
        public const string PREFS_IAP_UNLOCK = "IAP_UNLOCK";

        // Token: 0x0400033F RID: 831
        public const string PREFS_IAP_BANNERS = "IAP_BANNERS";

        // Token: 0x04000340 RID: 832
        public const string PREFS_IAP_SHAREWARE = "IAP_SHAREWARE";

        // Token: 0x04000341 RID: 833
        public const string acBronzeScissors = "677900534";

        // Token: 0x04000342 RID: 834
        public const string acSilverScissors = "681508185";

        // Token: 0x04000343 RID: 835
        public const string acGoldenScissors = "681473653";

        // Token: 0x04000344 RID: 836
        public const string acRopeCutter = "681461850";

        // Token: 0x04000345 RID: 837
        public const string acRopeCutterManiac = "681457931";

        // Token: 0x04000346 RID: 838
        public const string acUltimateRopeCutter = "1058248892";

        // Token: 0x04000347 RID: 839
        public const string acQuickFinger = "681464917";

        // Token: 0x04000348 RID: 840
        public const string acMasterFinger = "681508316";

        // Token: 0x04000349 RID: 841
        public const string acBubblePopper = "681513183";

        // Token: 0x0400034A RID: 842
        public const string acBubbleMaster = "1058345234";

        // Token: 0x0400034B RID: 843
        public const string acSpiderBuster = "681486608";

        // Token: 0x0400034C RID: 844
        public const string acSpiderTamer = "1058341284";

        // Token: 0x0400034D RID: 845
        public const string acWeightLoser = "681497443";

        // Token: 0x0400034E RID: 846
        public const string acCalorieMinimizer = "1058341297";

        // Token: 0x0400034F RID: 847
        public const string acTummyTeaser = "1058281905";

        // Token: 0x04000350 RID: 848
        public const string acRomanticSoul = "1432722351";

        // Token: 0x04000351 RID: 849
        public const string acMagician = "1515796440";

        // Token: 0x04000352 RID: 850
        public const string acCardboardBox = "681486798";

        // Token: 0x04000353 RID: 851
        public const string acCardboardPerfect = "1058364368";

        // Token: 0x04000354 RID: 852
        public const string acFabricBox = "681462993";

        // Token: 0x04000355 RID: 853
        public const string acFabricPerfect = "1058328727";

        // Token: 0x04000356 RID: 854
        public const string acFoilBox = "681520253";

        // Token: 0x04000357 RID: 855
        public const string acFoilPerfect = "1058324751";

        // Token: 0x04000358 RID: 856
        public const string acGiftBox = "681512374";

        // Token: 0x04000359 RID: 857
        public const string acGiftPerfect = "1058327768";

        // Token: 0x0400035A RID: 858
        public const string acCosmicBox = "1058310903";

        // Token: 0x0400035B RID: 859
        public const string acCosmicPerfect = "1058407145";

        // Token: 0x0400035C RID: 860
        public const string acValentineBox = "1432721430";

        // Token: 0x0400035D RID: 861
        public const string acValentinePerfect = "1432760157";

        // Token: 0x0400035E RID: 862
        public const string acMagicBox = "1515813296";

        // Token: 0x0400035F RID: 863
        public const string acMagicPerfect = "1515793567";

        // Token: 0x04000360 RID: 864
        public const string acToyBox = "1991474812";

        // Token: 0x04000361 RID: 865
        public const string acToyPerfect = "1991641832";

        // Token: 0x04000362 RID: 866
        public const string acToolBox = "1321820679";

        // Token: 0x04000363 RID: 867
        public const string acToolPerfect = "1335599628";

        // Token: 0x04000364 RID: 868
        public const string acBuzzBox = "23523272771";

        // Token: 0x04000365 RID: 869
        public const string acBuzzPerfect = "99928734496";

        // Token: 0x04000366 RID: 870
        public const string acDJBox = "com.zeptolab.ctr.djboxcompleted";

        // Token: 0x04000367 RID: 871
        public const string acDJPerfect = "com.zeptolab.ctr.djboxperfect";

        // Token: 0x04000368 RID: 872
        public RemoteDataManager remoteDataManager = new();

        // Token: 0x04000369 RID: 873
        private bool firstLaunch;

        // Token: 0x0400036A RID: 874
        private bool playLevelScroll;

        // Token: 0x0400036B RID: 875
        private static int[] PACK_UNLOCK_STARS_LITE = new int[]
        {
            0, 20, 80, 170, 240, 300, 350, 400, 450, 500,
            550
        };

        // Token: 0x0400036C RID: 876
        private static int[] PACK_UNLOCK_STARS = new int[]
        {
            0, 30, 80, 170, 240, 300, 350, 400, 450, 500,
            550
        };
    }
}
