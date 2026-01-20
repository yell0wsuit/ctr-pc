using System.Linq;

using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Media;

using Microsoft.Xna.Framework.Audio;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Game-specific sound manager that wraps <see cref="SoundMgr"/> with user preference checks.
    /// All sound and music playback respects the SOUND_ON and MUSIC_ON user preferences.
    /// </summary>
    internal sealed class CTRSoundMgr : FrameworkTypes
    {
        /// <summary>
        /// Plays a sound effect by its resource ID if sound is enabled.
        /// </summary>
        /// <param name="s">The resource ID of the sound effect to play.</param>
        public static void PlaySound(int s)
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

        /// <summary>
        /// Enables or disables looped sound playback globally.
        /// </summary>
        /// <param name="bEnable">If <c>true</c>, looped sounds are enabled; otherwise, they are stopped and disabled.</param>
        public static void EnableLoopedSounds(bool bEnable)
        {
            s_EnableLoopedSounds = bEnable;
            if (!s_EnableLoopedSounds)
            {
                StopLoopedSounds();
            }
        }

        /// <summary>
        /// Plays a looping sound effect by its resource ID if sound and looped sounds are enabled.
        /// </summary>
        /// <param name="s">The resource ID of the sound effect to loop.</param>
        /// <returns>The sound effect instance for controlling playback, or <c>null</c> if disabled.</returns>
        public static SoundEffectInstance PlaySoundLooped(int s)
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
                : Application.SharedSoundMgr().PlaySoundLooped(ResourceNameTranslator.ToResourceId(soundResourceName));
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
                SoundMgr.PlayMusic(musicId);
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

        /// <summary>
        /// Plays a random music track from the supplied resource IDs, avoiding immediate repetition.
        /// </summary>
        /// <param name="musicIds">Candidate music resource IDs.</param>
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

        /// <summary>
        /// Plays background music by its resource ID if music is enabled.
        /// </summary>
        /// <param name="f">The resource ID of the music track to play.</param>
        public static void PlayMusic(int f)
        {
            if (Preferences.GetBooleanForKey("MUSIC_ON"))
            {
                SoundMgr.PlayMusic(f);
            }
        }

        /// <summary>
        /// Stops all currently playing looped sound effects.
        /// </summary>
        public static void StopLoopedSounds()
        {
            Application.SharedSoundMgr().StopLoopedSounds();
        }

        /// <summary>
        /// Stops all currently playing sound effects.
        /// </summary>
        public static void StopSounds()
        {
            Application.SharedSoundMgr().StopAllSounds();
        }

        /// <summary>
        /// Stops all currently playing sounds and music.
        /// </summary>
        public static void StopAll()
        {
            StopSounds();
            StopMusic();
        }

        /// <summary>
        /// Stops the currently playing background music.
        /// </summary>
        public static void StopMusic()
        {
            SoundMgr.StopMusic();
        }

        /// <summary>
        /// Pauses all sound effects and music playback.
        /// </summary>
        public static void Pause()
        {
            Application.SharedSoundMgr().Pause();
        }

        /// <summary>
        /// Resumes all paused sound effects and music playback.
        /// </summary>
        public static void Unpause()
        {
            Application.SharedSoundMgr().Unpause();
        }

        /// <summary>
        /// Indicates whether looped sound playback is currently enabled.
        /// </summary>
        private static bool s_EnableLoopedSounds = true;

        /// <summary>
        /// Tracks the previously played music ID to avoid immediate repetition in random playback.
        /// </summary>
        private static int prevMusic = -1;
    }
}
