using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.media;
using Microsoft.Xna.Framework.Audio;
using System;

namespace CutTheRope.game
{
    internal class CTRSoundMgr : SoundMgr
    {
        public static void _playSound(int s)
        {
            if (Preferences._getBooleanForKey("SOUND_ON"))
            {
                Application.sharedSoundMgr().playSound(s);
            }
        }

        public static void EnableLoopedSounds(bool bEnable)
        {
            CTRSoundMgr.s_EnableLoopedSounds = bEnable;
            if (!CTRSoundMgr.s_EnableLoopedSounds)
            {
                CTRSoundMgr._stopLoopedSounds();
            }
        }

        public static SoundEffectInstance _playSoundLooped(int s)
        {
            if (CTRSoundMgr.s_EnableLoopedSounds && Preferences._getBooleanForKey("SOUND_ON"))
            {
                return Application.sharedSoundMgr().playSoundLooped(s);
            }
            return null;
        }

        public static void _playRandomMusic(int minId, int maxId)
        {
            int num;
            do
            {
                num = CTRMathHelper.RND_RANGE(minId, maxId);
            }
            while (num == CTRSoundMgr.prevMusic);
            CTRSoundMgr.prevMusic = num;
            CTRSoundMgr._playMusic(num);
        }

        public static void _playMusic(int f)
        {
            if (Preferences._getBooleanForKey("MUSIC_ON"))
            {
                Application.sharedSoundMgr().playMusic(f);
            }
        }

        public static void _stopLoopedSounds()
        {
            Application.sharedSoundMgr().stopLoopedSounds();
        }

        public static void _stopSounds()
        {
            Application.sharedSoundMgr().stopAllSounds();
        }

        public static void _stopAll()
        {
            CTRSoundMgr._stopSounds();
            CTRSoundMgr._stopMusic();
        }

        public static void _stopMusic()
        {
            Application.sharedSoundMgr().stopMusic();
        }

        public static void _pause()
        {
            Application.sharedSoundMgr().pause();
        }

        public static void _unpause()
        {
            Application.sharedSoundMgr().unpause();
        }

        private static bool s_EnableLoopedSounds = true;

        private static int prevMusic = -1;
    }
}
