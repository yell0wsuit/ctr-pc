using System;
using System.Collections.Generic;

using CutTheRopeDX.GameMain;
using CutTheRopeDX.Helpers;

using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

using XnaMediaPlayer = Microsoft.Xna.Framework.Media.MediaPlayer;
using XnaMediaState = Microsoft.Xna.Framework.Media.MediaState;
using XnaSong = Microsoft.Xna.Framework.Media.Song;

namespace CutTheRopeDX.Framework.Media
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
            loadedSounds = [];
            activeSounds = [];
            activeLoopedSounds = [];
        }

        /// <summary>
        /// An active sound instance paired with the effect that produced it,
        /// so the instance can be stopped when the parent effect is freed.
        /// </summary>
        /// <param name="Owner">The <see cref="SoundEffect"/> that created <paramref name="Instance"/>.</param>
        /// <param name="Instance">The playing sound effect instance.</param>
        private readonly record struct ActiveSound(SoundEffect Owner, SoundEffectInstance Instance);

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
        /// Any active instances spawned from this effect are stopped and disposed first.
        /// </summary>
        /// <param name="soundResourceName">Logical sound resource name to remove from the cache.</param>
        public void FreeSound(string soundResourceName)
        {
            string localizedName = CTRResourceMgr.HandleLocalizedResource(soundResourceName);
            if (string.IsNullOrEmpty(localizedName) || !loadedSounds.Remove(localizedName, out SoundEffect sound))
            {
                return;
            }

            StopAndRemoveByOwner(activeSounds, sound);
            StopAndRemoveByOwner(activeLoopedSounds, sound);
            sound.Dispose();
        }

        /// <summary>
        /// Gets or loads a sound effect by its resource name.
        /// </summary>
        /// <param name="soundResourceName">Logical sound resource name to resolve and load.</param>
        /// <returns>The loaded sound effect, or <see langword="null" /> when the name is invalid, localized lookup fails, the resource is music, or loading fails.</returns>
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

            if (loadedSounds.TryGetValue(localizedName, out SoundEffect cached))
            {
                return cached;
            }

            try
            {
                string soundPath = ContentPaths.GetSoundEffectPath(CTRResourceMgr.XNA_ResName(localizedName));
                SoundEffect loaded = _contentManager.Load<SoundEffect>(soundPath);
                loadedSounds.Add(localizedName, loaded);
                return loaded;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Removes stopped or null instances from the given active sounds list.
        /// </summary>
        private static void ClearStopped(List<ActiveSound> list)
        {
            _ = list.RemoveAll(static entry => entry.Instance == null || entry.Instance.State == SoundState.Stopped);
        }

        /// <summary>
        /// Plays a one-shot sound effect by its resource name.
        /// </summary>
        /// <param name="soundResourceName">Logical sound resource name to play once.</param>
        public void PlaySound(string soundResourceName)
        {
            ClearStopped(activeSounds);
            _ = TryPlay(soundResourceName, loop: false, activeSounds);
        }

        /// <summary>
        /// Plays a looping sound effect by its resource name.
        /// </summary>
        /// <param name="soundResourceName">Logical sound resource name to play in a loop.</param>
        /// <returns>The sound effect instance for controlling playback, or <see langword="null" /> on failure.</returns>
        public SoundEffectInstance PlaySoundLooped(string soundResourceName)
        {
            ClearStopped(activeLoopedSounds);
            return TryPlay(soundResourceName, loop: true, activeLoopedSounds);
        }

        /// <summary>
        /// Plays background music by its resource name. Stops any currently playing music first.
        /// </summary>
        /// <param name="musicResourceName">Logical music resource name to load and play.</param>
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
        /// Stops a single active looped sound <paramref name="instance"/> and drops its tracking
        /// entry, leaving every other looped sound and all one-shot effects untouched. Used by
        /// callers that own an individual loop (e.g. a rocket's fly loop) so stopping one source
        /// does not silence unrelated audio.
        /// </summary>
        /// <param name="instance">The looped instance to stop; ignored when <see langword="null"/>.</param>
        public void StopLoopedSound(SoundEffectInstance instance)
        {
            if (instance == null)
            {
                return;
            }

            try
            {
                if (instance.State != SoundState.Stopped)
                {
                    instance.Stop();
                }
            }
            catch (Exception)
            {
            }

            _ = activeLoopedSounds.RemoveAll(entry => ReferenceEquals(entry.Instance, instance));
        }

        /// <summary>
        /// Stops all currently playing sound effects, including looped sounds.
        /// Resets the pause and SFX-suspension state so subsequent audio starts from a clean slate.
        /// </summary>
        public void StopAllSounds()
        {
            StopList(activeSounds);
            activeSounds.Clear();
            StopLoopedSounds();
            pauseDepth = 0;
            sfxSuspended = false;
        }

        /// <summary>
        /// Stops and disposes any active instances in <paramref name="list"/> that were
        /// produced by <paramref name="owner"/>, and removes them from the list.
        /// </summary>
        private static void StopAndRemoveByOwner(List<ActiveSound> list, SoundEffect owner)
        {
            _ = list.RemoveAll(entry =>
            {
                if (!ReferenceEquals(entry.Owner, owner))
                {
                    return false;
                }
                SoundEffectInstance instance = entry.Instance;
                if (instance != null)
                {
                    try
                    {
                        if (instance.State != SoundState.Stopped)
                        {
                            instance.Stop();
                        }
                        instance.Dispose();
                    }
                    catch (Exception)
                    {
                    }
                }
                return true;
            });
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
        /// Pauses all looped sound effects and background music. Calls stack: if multiple
        /// independent sources (e.g. gameplay pause and app deactivation) both pause, the same
        /// number of <see cref="Unpause"/> calls is required before playback actually resumes.
        /// </summary>
        public void Pause()
        {
            try
            {
                if (pauseDepth == 0)
                {
                    ChangeListState(activeLoopedSounds, SoundState.Playing, SoundState.Paused);
                    if (XnaMediaPlayer.State == XnaMediaState.Playing)
                    {
                        XnaMediaPlayer.Pause();
                    }
                }
                pauseDepth++;
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Decrements the pause stack and, when it reaches zero, resumes all paused looped
        /// sound effects and background music. Calls beyond the outermost pause are ignored.
        /// Loops stay paused if sound effects have been independently suspended via
        /// <see cref="SuspendSoundEffects"/>.
        /// </summary>
        public void Unpause()
        {
            try
            {
                if (pauseDepth == 0)
                {
                    return;
                }

                pauseDepth--;
                if (pauseDepth == 0)
                {
                    if (!sfxSuspended)
                    {
                        ChangeListState(activeLoopedSounds, SoundState.Paused, SoundState.Playing);
                    }
                    if (XnaMediaPlayer.State == XnaMediaState.Paused)
                    {
                        XnaMediaPlayer.Resume();
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Pauses all active looped sound effects in response to the user disabling sound effects.
        /// Unlike <see cref="Pause"/>, this is independent of the transient pause stack, so focus
        /// restores or gameplay unpauses will not implicitly reactivate suspended loops.
        /// </summary>
        public void SuspendSoundEffects()
        {
            sfxSuspended = true;
            try
            {
                ChangeListState(activeLoopedSounds, SoundState.Playing, SoundState.Paused);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Resumes looped sound effects previously suspended by <see cref="SuspendSoundEffects"/>.
        /// If the game or app is still transiently paused, resumption is deferred until the
        /// outermost <see cref="Unpause"/> runs.
        /// </summary>
        public void RestoreSoundEffects()
        {
            sfxSuspended = false;
            if (pauseDepth > 0)
            {
                return;
            }

            try
            {
                ChangeListState(activeLoopedSounds, SoundState.Paused, SoundState.Playing);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Creates and starts a sound effect instance for the specified resource, appending it
        /// to <paramref name="destination"/> along with its owning <see cref="SoundEffect"/>.
        /// </summary>
        /// <param name="resourceName">Logical sound resource name to resolve.</param>
        /// <param name="loop">Whether the created instance should loop.</param>
        /// <param name="destination">List that receives the active sound entry on success.</param>
        /// <returns>The playing sound effect instance, or <see langword="null" /> if playback could not be started.</returns>
        private SoundEffectInstance TryPlay(string resourceName, bool loop, List<ActiveSound> destination)
        {
            SoundEffect sound = GetSound(resourceName);
            if (sound == null)
            {
                return null;
            }

            SoundEffectInstance instance;
            try
            {
                instance = sound.CreateInstance();
                instance.IsLooped = loop;
                instance.Play();
            }
            catch (Exception)
            {
                return null;
            }

            destination.Add(new ActiveSound(sound, instance));
            return instance;
        }

        /// <summary>
        /// Stops all sound effect instances in the specified <paramref name="list"/>.
        /// </summary>
        /// <param name="list">The list of active sound entries to stop.</param>
        private static void StopList(List<ActiveSound> list)
        {
            foreach (ActiveSound entry in list)
            {
                entry.Instance?.Stop();
            }
        }

        /// <summary>
        /// Changes the playback state of all sound effect instances in the specified <paramref name="list"/>.
        /// </summary>
        /// <param name="list">The list of active sound entries to modify.</param>
        /// <param name="fromState">The current state to match.</param>
        /// <param name="toState">The target state to transition to.</param>
        private static void ChangeListState(List<ActiveSound> list, SoundState fromState, SoundState toState)
        {
            foreach (ActiveSound entry in list)
            {
                SoundEffectInstance instance = entry.Instance;
                if (instance == null || instance.State != fromState)
                {
                    continue;
                }

                switch (toState)
                {
                    case SoundState.Paused:
                        instance.Pause();
                        break;
                    case SoundState.Playing:
                        instance.Resume();
                        break;
                    case SoundState.Stopped:
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Content manager used to load sound effects and songs.
        /// </summary>
        private static ContentManager _contentManager;

        /// <summary>
        /// Cache of loaded sound effects keyed by localized resource name.
        /// </summary>
        private readonly Dictionary<string, SoundEffect> loadedSounds;

        /// <summary>
        /// Active one-shot sound instances that may still be playing, tracked with their owning effect.
        /// </summary>
        private readonly List<ActiveSound> activeSounds;

        /// <summary>
        /// Active looped sound instances managed by pause and stop operations, tracked with their owning effect.
        /// </summary>
        private readonly List<ActiveSound> activeLoopedSounds;

        /// <summary>
        /// Nesting depth of pause calls. Resume only happens when the outermost pause unwinds,
        /// so stacked sources (gameplay pause + app deactivation) don't resume prematurely.
        /// </summary>
        private int pauseDepth;

        /// <summary>
        /// Whether looped sound effects are suspended by the user's sound-effects toggle.
        /// Independent of <see cref="pauseDepth"/> so transient pauses don't reactivate loops.
        /// </summary>
        private bool sfxSuspended;
    }
}
