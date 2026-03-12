using System;
using System.Collections.Generic;

using CutTheRope.GameMain;
using CutTheRope.Helpers;

using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

using XnaMediaPlayer = Microsoft.Xna.Framework.Media.MediaPlayer;
using XnaMediaState = Microsoft.Xna.Framework.Media.MediaState;
using XnaSong = Microsoft.Xna.Framework.Media.Song;

namespace CutTheRope.Framework.Media
{
    /// <summary>
    /// Manages sound effects and music playback using MonoGame's audio framework.
    /// Handles loading, caching, and playing of sound effects and background music.
    /// </summary>
    internal sealed class SoundMgr : FrameworkTypes
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SoundMgr"/> class.
        /// </summary>
        public SoundMgr()
        {
            LoadedSounds = [];
            activeSounds = [];
            activeLoopedSounds = [];
        }

        /// <summary>
        /// Sets the content manager used for loading audio assets.
        /// </summary>
        /// <param name="contentManager">The MonoGame content manager.</param>
        public static void SetContentManager(ContentManager contentManager)
        {
            _contentManager = contentManager;
        }

        /// <summary>
        /// Removes a cached sound effect from memory by resource name.
        /// </summary>
        public void FreeSound(string soundResourceName)
        {
            string localizedName = CTRResourceMgr.HandleLocalizedResource(soundResourceName);
            if (!string.IsNullOrEmpty(localizedName) && LoadedSounds.Remove(localizedName, out SoundEffect sound))
            {
                sound.Dispose();
            }
        }

        /// <summary>
        /// Gets or loads a sound effect by its resource name.
        /// </summary>
        public SoundEffect GetSound(string soundResourceName)
        {
            if (string.IsNullOrEmpty(soundResourceName))
            {
                return null;
            }

            string localizedName = CTRResourceMgr.HandleLocalizedResource(soundResourceName);
            if (string.IsNullOrEmpty(localizedName))
            {
                return null;
            }

            // Music resources are not sound effects
            if (Resources.IsMusic(localizedName))
            {
                return null;
            }

            if (LoadedSounds.TryGetValue(localizedName, out SoundEffect value))
            {
                return value;
            }

            SoundEffect soundEffect;
            try
            {
                string soundPath = ContentPaths.GetSoundEffectPath(CTRResourceMgr.XNA_ResName(localizedName));
                value = _contentManager.Load<SoundEffect>(soundPath);
                LoadedSounds.Add(localizedName, value);
                soundEffect = value;
            }
            catch (Exception)
            {
                soundEffect = value;
            }
            return soundEffect;
        }

        /// <summary>
        /// Removes stopped sound instances from the active sounds list.
        /// </summary>
        private void ClearStopped()
        {
            List<SoundEffectInstance> list = [];
            foreach (SoundEffectInstance activeSound in activeSounds)
            {
                if (activeSound != null && activeSound.State != SoundState.Stopped)
                {
                    list.Add(activeSound);
                }
            }
            activeSounds.Clear();
            activeSounds = list;
        }

        /// <summary>
        /// Plays a one-shot sound effect by its resource name.
        /// </summary>
        public void PlaySound(string soundResourceName)
        {
            ClearStopped();
            activeSounds.Add(Play(soundResourceName, false));
        }

        /// <summary>
        /// Plays a looping sound effect by its resource name.
        /// </summary>
        /// <returns>The sound effect instance for controlling playback, or <c>null</c> on failure.</returns>
        public SoundEffectInstance PlaySoundLooped(string soundResourceName)
        {
            ClearStopped();
            SoundEffectInstance soundEffectInstance = Play(soundResourceName, true);
            activeLoopedSounds.Add(soundEffectInstance);
            return soundEffectInstance;
        }

        /// <summary>
        /// Plays background music by its resource name. Stops any currently playing music first.
        /// </summary>
        public static void PlayMusic(string musicResourceName)
        {
            string localizedName = CTRResourceMgr.HandleLocalizedResource(musicResourceName);
            if (string.IsNullOrEmpty(localizedName))
            {
                return;
            }

            StopMusic();
            string musicPath = ContentPaths.GetMusicPath(CTRResourceMgr.XNA_ResName(localizedName));
            XnaSong song = _contentManager.Load<XnaSong>(musicPath);
            XnaMediaPlayer.IsRepeating = true;
            try
            {
                XnaMediaPlayer.Play(song);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Stops all currently playing looped sound effects.
        /// </summary>
        public void StopLoopedSounds()
        {
            StopList(activeLoopedSounds);
            activeLoopedSounds.Clear();
        }

        /// <summary>
        /// Stops all currently playing sound effects, including looped sounds.
        /// </summary>
        public void StopAllSounds()
        {
            StopLoopedSounds();
        }

        /// <summary>
        /// Stops the currently playing background music.
        /// </summary>
        public static void StopMusic()
        {
            try
            {
                XnaMediaPlayer.Stop();
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Suspends audio playback. No-op maintained for API compatibility.
        /// </summary>
        public static void Suspend()
        {
        }

        /// <summary>
        /// Resumes audio playback after suspension. No-op maintained for API compatibility.
        /// </summary>
        public static void Resume()
        {
        }

        /// <summary>
        /// Pauses all looped sound effects and background music.
        /// </summary>
        public void Pause()
        {
            try
            {
                ChangeListState(activeLoopedSounds, SoundState.Playing, SoundState.Paused);
                if (XnaMediaPlayer.State == XnaMediaState.Playing)
                {
                    XnaMediaPlayer.Pause();
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Resumes all paused looped sound effects and background music.
        /// </summary>
        public void Unpause()
        {
            try
            {
                ChangeListState(activeLoopedSounds, SoundState.Paused, SoundState.Playing);
                if (XnaMediaPlayer.State == XnaMediaState.Paused)
                {
                    XnaMediaPlayer.Resume();
                }
            }
            catch (Exception)
            {
            }
        }

        private SoundEffectInstance Play(string resourceName, bool loop)
        {
            SoundEffectInstance soundEffectInstance = null;
            SoundEffectInstance soundEffectInstance2;
            try
            {
                soundEffectInstance = GetSound(resourceName).CreateInstance();
                soundEffectInstance.IsLooped = loop;
                soundEffectInstance.Play();
                soundEffectInstance2 = soundEffectInstance;
            }
            catch (Exception)
            {
                soundEffectInstance2 = soundEffectInstance;
            }
            return soundEffectInstance2;
        }

        /// <summary>
        /// Stops all sound effect instances in the specified list.
        /// </summary>
        /// <param name="list">The list of sound effect instances to stop.</param>
        private static void StopList(List<SoundEffectInstance> list)
        {
            foreach (SoundEffectInstance item in list)
            {
                item?.Stop();
            }
        }

        /// <summary>
        /// Changes the playback state of all sound effect instances in the specified list.
        /// </summary>
        /// <param name="list">The list of sound effect instances to modify.</param>
        /// <param name="fromState">The current state to match.</param>
        /// <param name="toState">The target state to transition to.</param>
        private static void ChangeListState(List<SoundEffectInstance> list, SoundState fromState, SoundState toState)
        {
            foreach (SoundEffectInstance item in list)
            {
                if (item != null && item.State == fromState)
                {
                    if (toState != SoundState.Playing)
                    {
                        if (toState == SoundState.Paused)
                        {
                            item.Pause();
                        }
                    }
                    else
                    {
                        item.Resume();
                    }
                }
            }
        }

        private static ContentManager _contentManager;

        private readonly Dictionary<string, SoundEffect> LoadedSounds;

        private List<SoundEffectInstance> activeSounds;

        private readonly List<SoundEffectInstance> activeLoopedSounds;
    }
}
