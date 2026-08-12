using System;
using System.Collections.Generic;

using CutTheRopeDX.GameMain;
using CutTheRopeDX.Helpers;

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
        /// <param name="Owner">The <see cref="ISoundEffect"/> that created <paramref name="Instance"/>.</param>
        /// <param name="Instance">The playing sound effect instance.</param>
        private readonly record struct ActiveSound(ISoundEffect Owner, ISoundInstance Instance);

        /// <summary>
        /// Sets the audio backend used for loading and playing audio assets.
        /// </summary>
        /// <param name="backend">The platform audio backend, or <see langword="null"/> for silent runs.</param>
        public static void SetBackend(IAudioBackend backend)
        {
            _backend = backend;
        }

        /// <summary>
        /// Removes a cached sound effect from memory by resource name.
        /// Any active instances spawned from this effect are stopped and disposed first.
        /// </summary>
        /// <param name="soundResourceName">Logical sound resource name to remove from the cache.</param>
        public void FreeSound(string soundResourceName)
        {
            string localizedName = CTRResourceMgr.HandleLocalizedResource(soundResourceName);
            if (string.IsNullOrEmpty(localizedName) || !loadedSounds.Remove(localizedName, out ISoundEffect sound))
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
        public ISoundEffect GetSound(string soundResourceName)
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

            if (loadedSounds.TryGetValue(localizedName, out ISoundEffect cached))
            {
                return cached;
            }

            try
            {
                string soundPath = ContentPaths.GetSoundEffectPath(CTRResourceMgr.XNA_ResName(localizedName));
                ISoundEffect loaded = _backend.LoadSound(soundPath);
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
            _ = list.RemoveAll(static entry => entry.Instance == null || entry.Instance.State == AudioPlaybackState.Stopped);
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
        public ISoundInstance PlaySoundLooped(string soundResourceName)
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
            // Headless runs install no audio backend and are silent. GetSound already tolerates
            // this via its try/catch; the music load below sits outside one, so it is checked here.
            if (_backend == null)
            {
                return;
            }

            string localizedName = CTRResourceMgr.HandleLocalizedResource(musicResourceName);
            if (string.IsNullOrEmpty(localizedName))
            {
                return;
            }

            StopMusic();
            string musicPath = ContentPaths.GetMusicPath(CTRResourceMgr.XNA_ResName(localizedName));
            IMusicTrack track = _backend.LoadMusic(musicPath);
            activeSong = track;
            try
            {
                _backend.PlayMusic(track, true);
                usesSongCompletionWorkaround =
                    _backend.TryInstallSongCompletionCallback(track, OnSongDecoderFinished);
            }
            catch (Exception)
            {
                activeSong = null;
                usesSongCompletionWorkaround = false;
            }
        }

        /// <summary>
        /// Advances a pending music-loop restart after MonoGame's queued decoded tail
        /// has had time to reach the audio device.
        /// </summary>
        /// <param name="elapsed">Elapsed game time since the previous update.</param>
        public static void Update(TimeSpan elapsed)
        {
            if (_backend == null ||
                !usesSongCompletionWorkaround ||
                !songLoopScheduler.Advance(
                    elapsed,
                    _backend.MusicState == AudioPlaybackState.Playing))
            {
                return;
            }

            IMusicTrack track = activeSong;
            if (track == null)
            {
                return;
            }

            try
            {
                _backend.PlayMusic(track, true);
            }
            catch (Exception)
            {
                StopMusic();
            }
        }

        /// <summary>
        /// Records how much audio remains queued when MonoGame reports decoder EOF.
        /// </summary>
        private static void OnSongDecoderFinished(object sender, EventArgs args)
        {
            if (!usesSongCompletionWorkaround || !ReferenceEquals(sender, activeSong))
            {
                return;
            }

            songLoopScheduler.Schedule(activeSong.Duration);
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
        public void StopLoopedSound(ISoundInstance instance)
        {
            if (instance == null)
            {
                return;
            }

            try
            {
                if (instance.State != AudioPlaybackState.Stopped)
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
            musicPauseTickets.Clear();
            sfxSuspended = false;
        }

        /// <summary>
        /// Stops and disposes any active instances in <paramref name="list"/> that were
        /// produced by <paramref name="owner"/>, and removes them from the list.
        /// </summary>
        private static void StopAndRemoveByOwner(List<ActiveSound> list, ISoundEffect owner)
        {
            _ = list.RemoveAll(entry =>
            {
                if (!ReferenceEquals(entry.Owner, owner))
                {
                    return false;
                }
                ISoundInstance instance = entry.Instance;
                if (instance != null)
                {
                    try
                    {
                        if (instance.State != AudioPlaybackState.Stopped)
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
            usesSongCompletionWorkaround = false;
            activeSong = null;
            songLoopScheduler.Cancel();
            try
            {
                _backend?.StopMusic();
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Pauses looped sound effects and any background music currently playing. Calls stack:
        /// each pause records whether it suspended music so its matching <see cref="Unpause"/>
        /// can restore that music independently of an outer pause.
        /// </summary>
        public void Pause()
        {
            try
            {
                if (pauseDepth == 0)
                {
                    ChangeListState(activeLoopedSounds, AudioPlaybackState.Playing, AudioPlaybackState.Paused);
                }

                bool pausedMusic = _backend != null
                    && _backend.MusicState == AudioPlaybackState.Playing;
                if (pausedMusic)
                {
                    _backend.PauseMusic();
                }
                musicPauseTickets.Push(pausedMusic);
                pauseDepth++;
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Decrements the pause stack, resumes music if this specific pause suspended it, and
        /// resumes looped effects when the outermost pause ends. Calls beyond the outermost pause
        /// are ignored. Loops stay paused if sound effects have been independently suspended via
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
                bool resumeMusic = musicPauseTickets.Count > 0 && musicPauseTickets.Pop();
                if (pauseDepth == 0)
                {
                    if (!sfxSuspended)
                    {
                        ChangeListState(activeLoopedSounds, AudioPlaybackState.Paused, AudioPlaybackState.Playing);
                    }
                }
                if (resumeMusic
                    && _backend != null
                    && _backend.MusicState == AudioPlaybackState.Paused)
                {
                    _backend.ResumeMusic();
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
                ChangeListState(activeLoopedSounds, AudioPlaybackState.Playing, AudioPlaybackState.Paused);
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
                ChangeListState(activeLoopedSounds, AudioPlaybackState.Paused, AudioPlaybackState.Playing);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Creates and starts a sound effect instance for the specified resource, appending it
        /// to <paramref name="destination"/> along with its owning <see cref="ISoundEffect"/>.
        /// </summary>
        /// <param name="resourceName">Logical sound resource name to resolve.</param>
        /// <param name="loop">Whether the created instance should loop.</param>
        /// <param name="destination">List that receives the active sound entry on success.</param>
        /// <returns>The playing sound effect instance, or <see langword="null" /> if playback could not be started.</returns>
        private ISoundInstance TryPlay(string resourceName, bool loop, List<ActiveSound> destination)
        {
            ISoundEffect sound = GetSound(resourceName);
            if (sound == null)
            {
                return null;
            }

            ISoundInstance instance;
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
        private static void ChangeListState(List<ActiveSound> list, AudioPlaybackState fromState, AudioPlaybackState toState)
        {
            foreach (ActiveSound entry in list)
            {
                ISoundInstance instance = entry.Instance;
                if (instance == null || instance.State != fromState)
                {
                    continue;
                }

                switch (toState)
                {
                    case AudioPlaybackState.Paused:
                        instance.Pause();
                        break;
                    case AudioPlaybackState.Playing:
                        instance.Resume();
                        break;
                    case AudioPlaybackState.Stopped:
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Audio backend used to load sound effects and songs and control music playback.
        /// </summary>
        private static IAudioBackend _backend;

        /// <summary>
        /// Music track currently owned by the media player.
        /// </summary>
        private static IMusicTrack activeSong;

        /// <summary>
        /// Waits for the native voice's queued tail before restarting <see cref="activeSong"/>.
        /// </summary>
        private static readonly SongLoopScheduler songLoopScheduler = new();

        /// <summary>
        /// Whether MonoGame's premature completion callback was replaced successfully.
        /// </summary>
        private static bool usesSongCompletionWorkaround;

        /// <summary>
        /// Cache of loaded sound effects keyed by localized resource name.
        /// </summary>
        private readonly Dictionary<string, ISoundEffect> loadedSounds;

        /// <summary>
        /// Active one-shot sound instances that may still be playing, tracked with their owning effect.
        /// </summary>
        private readonly List<ActiveSound> activeSounds;

        /// <summary>
        /// Active looped sound instances managed by pause and stop operations, tracked with their owning effect.
        /// </summary>
        private readonly List<ActiveSound> activeLoopedSounds;

        /// <summary>
        /// Nesting depth of pause calls, used to keep looped sound effects suspended until the
        /// outermost pause unwinds.
        /// </summary>
        private int pauseDepth;

        /// <summary>
        /// One entry per transient pause, recording whether that pause suspended the music voice.
        /// </summary>
        private readonly Stack<bool> musicPauseTickets = [];

        /// <summary>
        /// Whether looped sound effects are suspended by the user's sound-effects toggle.
        /// Independent of <see cref="pauseDepth"/> so transient pauses don't reactivate loops.
        /// </summary>
        private bool sfxSuspended;
    }
}
