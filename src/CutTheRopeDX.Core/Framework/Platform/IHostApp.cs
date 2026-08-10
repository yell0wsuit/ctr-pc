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
    }
}
