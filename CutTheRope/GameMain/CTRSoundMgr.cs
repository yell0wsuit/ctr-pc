using System.Linq;

using CutTheRope.Framework.Core;
using CutTheRope.Framework.Media;

using Microsoft.Xna.Framework.Audio;

namespace CutTheRope.GameMain
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

        /// <summary>
        /// Plays a sound effect identified by its resource name.
        /// </summary>
        /// <param name="soundResourceName">Sound resource name.</param>
        public static void PlaySound(string soundResourceName)
        {
            if (Preferences.GetBooleanForKey("SOUND_ON"))
            {
                Application.SharedSoundMgr().PlaySound(soundResourceName);
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

        /// <summary>
        /// Plays a looped sound effect identified by its resource name.
        /// </summary>
        /// <param name="soundResourceName">Sound resource name.</param>
        public static SoundEffectInstance PlaySoundLooped(string soundResourceName)
        {
            return !s_EnableLoopedSounds || !Preferences.GetBooleanForKey("SOUND_ON")
                ? null
                : Application.SharedSoundMgr().PlaySoundLooped(GetResourceId(soundResourceName));
        }

        /// <summary>
        /// Plays a random sound from the provided list of sound resource names.
        /// </summary>
        public static void PlayRandomSound(params string[] soundNames)
        {
            if (soundNames == null || soundNames.Length == 0)
            {
                return;
            }

            string soundName = soundNames[RND_RANGE(0, soundNames.Length - 1)];
            PlaySound(soundName);
        }

        /// <summary>
        /// Plays background music identified by its resource name.
        /// </summary>
        /// <param name="musicResourceName">Music resource name.</param>
        public static void PlayMusic(string musicResourceName)
        {
            if (Preferences.GetBooleanForKey("MUSIC_ON") && !string.IsNullOrWhiteSpace(musicResourceName))
            {
                int musicId = ResourceNameTranslator.ToResourceId(musicResourceName);
                Application.SharedSoundMgr().PlayMusic(musicId);
            }
        }

        /// <summary>
        /// Plays a random music track from the supplied resource names.
        /// </summary>
        /// <param name="musicNames">Candidate music resource names.</param>
        public static void PlayRandomMusic(params string[] musicNames)
        {
            if (musicNames == null)
            {
                return;
            }

            int[] musicIds = [.. musicNames
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(ResourceNameTranslator.ToResourceId)];

            PlayRandomMusic(musicIds);
        }

        public static void PlayRandomMusic(params int[] musicIds)
        {
            if (musicIds == null || musicIds.Length == 0)
            {
                return;
            }

            int num;
            do
            {
                num = musicIds[RND_RANGE(0, musicIds.Length - 1)];
            }
            while (num == prevMusic && musicIds.Length > 1);
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
