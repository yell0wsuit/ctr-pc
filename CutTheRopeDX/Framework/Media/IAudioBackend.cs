using System;

namespace CutTheRopeDX.Framework.Media
{
    /// <summary>Playback state of a sound instance or the music channel.</summary>
    internal enum AudioPlaybackState { Playing, Paused, Stopped }

    /// <summary>A loaded sound effect that can spawn playable instances.</summary>
    internal interface ISoundEffect : IDisposable
    {
        ISoundInstance CreateInstance();
    }

    /// <summary>A playable instance of a loaded sound effect.</summary>
    internal interface ISoundInstance : IDisposable
    {
        void Play();
        void Stop();
        void Pause();
        void Resume();
        bool IsLooped { get; set; }
        float Volume { get; set; }
        AudioPlaybackState State { get; }
    }

    /// <summary>A loaded music track.</summary>
    internal interface IMusicTrack
    {
        TimeSpan Duration { get; }
    }

    /// <summary>
    /// The device half of audio. <see cref="SoundMgr"/> keeps all caching and
    /// lifecycle logic; this interface is only what requires a platform audio API.
    /// </summary>
    internal interface IAudioBackend
    {
        ISoundEffect LoadSound(string contentPath);
        IMusicTrack LoadMusic(string contentPath);
        void PlayMusic(IMusicTrack track, bool repeating);
        void StopMusic();
        void PauseMusic();
        void ResumeMusic();
        AudioPlaybackState MusicState { get; }
        /// <summary>
        /// Installs the song-completion callback (MonoGame decoder-finished workaround).
        /// Returns false when the platform doesn't need or support it.
        /// </summary>
        bool TryInstallSongCompletionCallback(IMusicTrack track, EventHandler<EventArgs> onDecoderFinished);
    }
}
