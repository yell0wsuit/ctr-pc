using CutTheRopeDX.Framework.Core;

namespace CutTheRopeDX.Framework.Platform
{
    /// <summary>Host-application operations Core may request.</summary>
    internal interface IHostApp
    {
        /// <summary>
        /// Whether <see cref="Exit"/> can actually close the game. A browser tab cannot close
        /// itself, so the menu offers no way to quit there.
        /// </summary>
        bool CanExit { get; }

        /// <summary>
        /// The level editor to offer from the main menu, or <see langword="null"/> when this host
        /// has none. The editor is a web application, so only a host that can keep the player
        /// alongside a second page has anywhere to send them.
        /// </summary>
        string LevelEditorUrl { get; }

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
