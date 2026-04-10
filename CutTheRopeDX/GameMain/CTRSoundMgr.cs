using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Media;

using Microsoft.Xna.Framework.Audio;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Game-specific sound manager that wraps <see cref="SoundMgr"/> with user preference checks.
    /// All sound and music playback respects the SOUND_ON and MUSIC_ON user preferences.
    /// </summary>
    internal sealed class CTRSoundMgr : FrameworkTypes
    {
        /// <summary>
        /// Plays a sound effect identified by its resource name.
        /// </summary>
        /// <param name="soundResourceName">The sound resource name to play.</param>
        public static void PlaySound(string soundResourceName)
        {
            if (!string.IsNullOrWhiteSpace(soundResourceName)
                && Preferences.GetBooleanForKey("SOUND_ON"))
            {
                Application.SharedSoundMgr().PlaySound(soundResourceName);
            }
        }

        /// <summary>
        /// Plays an Om Nom sound, swapping to a skin-specific variant when available.
        /// </summary>
        /// <param name="soundResourceName">The base sound resource name to resolve and play.</param>
        public static void PlayOmNomSound(string soundResourceName)
        {
            if (soundResourceName is Resources.Snd.MonsterExcited or Resources.Snd.MonsterGreeting
                && RND_RANGE(0, 1) == 0)
            {
                return;
            }

            PlaySound(OmNomSoundResolver.ResolveSelectedSkinSoundResource(soundResourceName));
        }

        /// <summary>
        /// Enables or disables looped sound playback globally.
        /// </summary>
        /// <param name="bEnable">If <see langword="true" />, looped sounds are enabled; otherwise, they are stopped and disabled.</param>
        public static void EnableLoopedSounds(bool bEnable)
        {
            s_EnableLoopedSounds = bEnable;
            if (!s_EnableLoopedSounds)
            {
                StopLoopedSounds();
            }
        }

        /// <summary>
        /// Plays a looped sound effect identified by its resource name.
        /// </summary>
        /// <param name="soundResourceName">The sound resource name to loop.</param>
        /// <returns>The looping <see cref="SoundEffectInstance"/>, or <see langword="null"/> if looped sounds are disabled.</returns>
        public static SoundEffectInstance PlaySoundLooped(string soundResourceName)
        {
            return !s_EnableLoopedSounds || !Preferences.GetBooleanForKey("SOUND_ON")
                ? null
                : Application.SharedSoundMgr().PlaySoundLooped(soundResourceName);
        }

        /// <summary>
        /// Plays a random sound from the provided list of sound resource names.
        /// </summary>
        /// <param name="soundNames">One or more sound resource names to choose from.</param>
        public static void PlayRandomSound(params string[] soundNames)
        {
            if (soundNames == null || soundNames.Length == 0)
            {
                return;
            }

            int validCount = 0;
            for (int i = 0; i < soundNames.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(soundNames[i]))
                {
                    validCount++;
                }
            }

            if (validCount == 0)
            {
                return;
            }

            string[] validSoundNames = new string[validCount];
            int validIndex = 0;
            for (int i = 0; i < soundNames.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(soundNames[i]))
                {
                    validSoundNames[validIndex++] = soundNames[i];
                }
            }

            string soundName = validSoundNames[RND_RANGE(0, validSoundNames.Length - 1)];
            PlaySound(soundName);
        }

        /// <summary>
        /// Plays a random Om Nom sound, resolving candidates for the selected skin first.
        /// </summary>
        /// <param name="soundNames">One or more base sound resource names to resolve and choose from.</param>
        public static void PlayRandomOmNomSound(params string[] soundNames)
        {
            if (soundNames == null || soundNames.Length == 0)
            {
                return;
            }

            string[] resolvedSounds = new string[soundNames.Length];
            for (int i = 0; i < soundNames.Length; i++)
            {
                resolvedSounds[i] = OmNomSoundResolver.ResolveSelectedSkinSoundResource(soundNames[i]);
            }

            PlayRandomSound(resolvedSounds);
        }

        /// <summary>
        /// Plays background music identified by its resource name.
        /// </summary>
        /// <param name="musicResourceName">The music resource name to play.</param>
        public static void PlayMusic(string musicResourceName)
        {
            if (Preferences.GetBooleanForKey("MUSIC_ON") && !string.IsNullOrWhiteSpace(musicResourceName))
            {
                SoundMgr.PlayMusic(musicResourceName);
            }
        }

        /// <summary>
        /// Plays a random music track from the supplied resource names, avoiding immediate repetition.
        /// </summary>
        /// <param name="musicNames">One or more music resource names to choose from.</param>
        public static void PlayRandomMusic(params string[] musicNames)
        {
            if (musicNames == null || musicNames.Length == 0)
            {
                return;
            }

            string name;
            do
            {
                name = musicNames[RND_RANGE(0, musicNames.Length - 1)];
            }
            while (name == prevMusic && musicNames.Length > 1);
            prevMusic = name;
            PlayMusic(name);
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
        /// Tracks the previously played music name to avoid immediate repetition in random playback.
        /// </summary>
        private static string prevMusic;
    }
}
