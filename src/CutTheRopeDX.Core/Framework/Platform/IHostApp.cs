using CutTheRopeDX.Framework.Core;

namespace CutTheRopeDX.Framework.Platform
{
    /// <summary>Host-application operations Core may request.</summary>
    internal interface IHostApp
    {
        void Exit();

        /// <summary>Returns whether <paramref name="key"/> transitioned from up to down this frame.</summary>
        /// <param name="key">The key to check.</param>
        bool IsKeyPressed(KeyCode key);

        /// <summary>Renders the current video frame to the screen in place of the game scene.</summary>
        void DrawMovie();

        /// <summary>Opens <paramref name="url"/> in whatever the host uses to show web pages.</summary>
        /// <param name="url">The absolute URL to open.</param>
        void OpenUrl(string url);
    }
}
