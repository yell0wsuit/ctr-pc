using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;

namespace CutTheRopeDX.Browser
{
    /// <summary>Thin managed wrapper over the audio.js WebAudio module.</summary>
    internal static partial class AudioInterop
    {
        /// <summary>Imports audio.js. Must be awaited once before any other call.</summary>
        public static Task ImportAsync()
        {
            return JSHost.ImportAsync("audio", "../audio.js");
        }

        /// <summary>Resumes the AudioContext. Must follow a user gesture.</summary>
        [JSImport("resume", "audio")]
        public static partial Task Resume();

        /// <summary>Fetches and decodes an audio file, returning 1 on success.</summary>
        [JSImport("decode", "audio")]
        public static partial Task<int> Decode(string key, string url);

        /// <summary>Starts a voice, returning its handle or 0 when no decode was requested.</summary>
        [JSImport("play", "audio")]
        public static partial int Play(string key, bool loop, float volume);

        /// <summary>Stops a voice.</summary>
        [JSImport("stop", "audio")]
        public static partial void Stop(int handle);

        /// <summary>Sets a voice's gain.</summary>
        [JSImport("setVolume", "audio")]
        public static partial void SetVolume(int handle, float volume);

        /// <summary>Pauses a voice at its current playback position.</summary>
        [JSImport("pauseVoice", "audio")]
        public static partial void PauseVoice(int handle);

        /// <summary>Resumes a voice from its retained playback position.</summary>
        [JSImport("resumeVoice", "audio")]
        public static partial void ResumeVoice(int handle);

        /// <summary>Whether a voice is queued or playing.</summary>
        [JSImport("isPlaying", "audio")]
        public static partial bool IsPlaying(int handle);

        /// <summary>Duration of a decoded buffer in seconds, or 0 when absent.</summary>
        [JSImport("durationOf", "audio")]
        public static partial double DurationOf(string key);
    }
}
