using System;

using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.Framework.Media
{
    internal interface IVideoPlayer : IDisposable
    {
        void Play(string moviePath, bool mute);

        Texture2D GetTexture();

        bool IsPlaying();

        bool IsTextureReady();

        void Stop();

        void Pause();

        void Resume();

        void Start();

        void Update();

        bool IsPaused { get; }

        event Action PlaybackFinished;
    }
}
