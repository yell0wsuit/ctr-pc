using System;

using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.Framework.Media
{
    /// <summary>
    /// Defines the contract for video playback functionality.
    /// </summary>
    internal interface IVideoPlayer : IDisposable
    {
        /// <summary>
        /// Gets a value indicating whether playback is currently paused.
        /// </summary>
        bool IsPaused { get; }

        /// <summary>
        /// Occurs when video playback has finished or was skipped.
        /// </summary>
        event Action PlaybackFinished;

        /// <summary>
        /// Prepares a video for playback from the specified path.
        /// </summary>
        /// <param name="moviePath">The relative path to the video file without extension.</param>
        /// <param name="mute">If <c>true</c>, audio will be muted during playback.</param>
        void Play(string moviePath, bool mute);

        /// <summary>
        /// Gets the current video frame as a texture.
        /// </summary>
        /// <returns>
        /// A <see cref="Texture2D"/> containing the current video frame, or <c>null</c>
        /// if no video is playing or playback has finished.
        /// </returns>
        Texture2D GetTexture();

        /// <summary>
        /// Determines whether a video is currently loaded and potentially playing.
        /// </summary>
        /// <returns><c>true</c> if a video is active; otherwise, <c>false</c>.</returns>
        bool IsPlaying();

        /// <summary>
        /// Determines whether the video texture is ready for rendering.
        /// </summary>
        /// <returns><c>true</c> if the texture can be rendered; otherwise, <c>false</c>.</returns>
        bool IsTextureReady();

        /// <summary>
        /// Stops the current video playback.
        /// </summary>
        void Stop();

        /// <summary>
        /// Pauses the current video playback.
        /// </summary>
        void Pause();

        /// <summary>
        /// Resumes video playback after being paused.
        /// </summary>
        void Resume();

        /// <summary>
        /// Starts video playback after a video has been prepared with <see cref="Play"/>.
        /// </summary>
        void Start();

        /// <summary>
        /// Updates the video player state each frame.
        /// </summary>
        void Update();
    }
}
