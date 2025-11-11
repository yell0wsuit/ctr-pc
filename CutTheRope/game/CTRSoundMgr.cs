using CutTheRope.iframework.core;
using CutTheRope.iframework.media;
using Microsoft.Xna.Framework.Audio;

namespace CutTheRope.game
{
    internal sealed class CTRSoundMgr : SoundMgr
    {
        public static new void PlaySound(int s)
        {
            if (Preferences.GetBooleanForKey("SOUND_ON"))
            {
                Application.SharedSoundMgr().PlaySound(s);
            }
        }

        public static void EnableLoopedSounds(bool bEnable)
        {
            s_EnableLoopedSounds = bEnable;
            if (!s_EnableLoopedSounds)
            {
                StopLoopedSounds();
            }
        }

        public static new SoundEffectInstance PlaySoundLooped(int s)
        {
            return s_EnableLoopedSounds && Preferences.GetBooleanForKey("SOUND_ON") ? Application.SharedSoundMgr().PlaySoundLooped(s) : null;
        }

        public static void PlayRandomMusic(int minId, int maxId)
        {
            int num;
            do
            {
                num = RND_RANGE(minId, maxId);
            }
            while (num == prevMusic);
            prevMusic = num;
            PlayMusic(num);
        }

        public static new void PlayMusic(int f)
        {
            if (Preferences.GetBooleanForKey("MUSIC_ON"))
            {
                Application.SharedSoundMgr().PlayMusic(f);
            }
        }

        public static new void StopLoopedSounds()
        {
            Application.SharedSoundMgr().StopLoopedSounds();
        }

        public static void StopSounds()
        {
            Application.SharedSoundMgr().StopAllSounds();
        }

        public static void StopAll()
        {
            StopSounds();
            StopMusic();
        }

        public static new void StopMusic()
        {
            Application.SharedSoundMgr().StopMusic();
        }

        public static new void Pause()
        {
            Application.SharedSoundMgr().Pause();
        }

        public static new void Unpause()
        {
            Application.SharedSoundMgr().Unpause();
        }

        private static bool s_EnableLoopedSounds = true;

        private static int prevMusic = -1;
    }
}
