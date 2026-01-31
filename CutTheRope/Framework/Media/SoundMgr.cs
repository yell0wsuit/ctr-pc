using System;
using System.Collections.Generic;

using CutTheRope.GameMain;
using CutTheRope.Helpers;

using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

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
        /// Removes a cached sound effect from memory.
        /// </summary>
        /// <param name="resId">The resource ID of the sound to free.</param>
        public void FreeSound(int resId)
        {
            _ = LoadedSounds.Remove(resId);
        }

        /// <summary>
        /// Gets or loads a sound effect by its resource ID.
        /// </summary>
        /// <param name="resId">The resource ID of the sound effect.</param>
        /// <returns>The loaded sound effect, or <c>null</c> if not found.</returns>
        public SoundEffect GetSound(int resId)
        {
            if (!TryResolveResource(resId, out string resourceName, out int localizedResId))
            {
                return null;
            }

            if (localizedResId is >= 145 and <= 148)
            {
                return null;
            }
            if (LoadedSounds.TryGetValue(localizedResId, out SoundEffect value))
            {
                return value;
            }
            SoundEffect soundEffect;
            try
            {
                string soundPath = ContentPaths.GetSoundEffectPath(CTRResourceMgr.XNA_ResName(resourceName));
                value = _contentManager.Load<SoundEffect>(soundPath);
                LoadedSounds.Add(localizedResId, value);
                soundEffect = value;
            }
            catch (Exception)
            {
                soundEffect = value;
            }
            return soundEffect;
        }

        /// <summary>
        /// Gets a sound by its resource name (auto-assigns ID if needed).
        /// </summary>
        public SoundEffect GetSound(string soundResourceName)
        {
            int soundResID = ResourceNameTranslator.ToResourceId(soundResourceName);
            return GetSound(soundResID);
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
        /// Plays a one-shot sound effect by its resource ID.
        /// </summary>
        /// <param name="sid">The resource ID of the sound effect to play.</param>
        public void PlaySound(int sid)
        {
            ClearStopped();
            activeSounds.Add(Play(sid, false));
        }

        /// <summary>
        /// Plays a sound by its resource name (auto-assigns ID if needed).
        /// </summary>
        public void PlaySound(string soundResourceName)
        {
            int soundResID = ResourceNameTranslator.ToResourceId(soundResourceName);
            PlaySound(soundResID);
        }

        /// <summary>
        /// Plays a looping sound effect by its resource ID.
        /// </summary>
        /// <param name="sid">The resource ID of the sound effect to loop.</param>
        /// <returns>The sound effect instance for controlling playback, or <c>null</c> on failure.</returns>
        public SoundEffectInstance PlaySoundLooped(int sid)
        {
            ClearStopped();
            SoundEffectInstance soundEffectInstance = Play(sid, true);
            activeLoopedSounds.Add(soundEffectInstance);
            return soundEffectInstance;
        }

        /// <summary>
        /// Plays background music by its resource ID. Stops any currently playing music first.
        /// </summary>
        /// <param name="resId">The resource ID of the music track to play.</param>
        public static void PlayMusic(int resId)
        {
            if (!TryResolveResource(resId, out string resourceName, out _))
            {
                return;
            }

            StopMusic();
            string musicPath = ContentPaths.GetMusicPath(CTRResourceMgr.XNA_ResName(resourceName));
            Song song = _contentManager.Load<Song>(musicPath);
            MediaPlayer.IsRepeating = true;
            try
            {
                MediaPlayer.Play(song);
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
                MediaPlayer.Stop();
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
                if (MediaPlayer.State == MediaState.Playing)
                {
                    MediaPlayer.Pause();
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
                if (MediaPlayer.State == MediaState.Paused)
                {
                    MediaPlayer.Resume();
                }
            }
            catch (Exception)
            {
            }
        }

        private SoundEffectInstance Play(int sid, bool l)
        {
            SoundEffectInstance soundEffectInstance = null;
            SoundEffectInstance soundEffectInstance2;
            try
            {
                soundEffectInstance = GetSound(sid).CreateInstance();
                soundEffectInstance.IsLooped = l;
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

        /// <summary>
        /// Resolves a resource ID to its localized name and ID.
        /// </summary>
        /// <param name="resId">The original resource ID.</param>
        /// <param name="localizedName">The resolved localized resource name.</param>
        /// <param name="localizedResId">The resolved localized resource ID.</param>
        /// <returns><c>true</c> if the resource was resolved successfully; otherwise, <c>false</c>.</returns>
        private static bool TryResolveResource(int resId, out string localizedName, out int localizedResId)
        {
            localizedName = ResourceNameTranslator.TranslateLegacyId(resId);
            if (string.IsNullOrEmpty(localizedName))
            {
                localizedResId = -1;
                return false;
            }

            localizedName = CTRResourceMgr.HandleLocalizedResource(localizedName);
            localizedResId = ResourceNameTranslator.ToResourceId(localizedName);
            return localizedResId >= 0;
        }

        private readonly Dictionary<int, SoundEffect> LoadedSounds;

        private List<SoundEffectInstance> activeSounds;

        private readonly List<SoundEffectInstance> activeLoopedSounds;
    }
}
