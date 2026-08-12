using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Platform;

namespace CutTheRopeDX.Browser
{
    /// <summary>Host-application operations, as a browser can serve them.</summary>
    internal sealed partial class BrowserHostApp : IHostApp
    {
        private readonly HashSet<KeyCode> _down = [];
        private readonly HashSet<KeyCode> _pressedThisStep = [];

        /// <summary>Records a key transition from the DOM.</summary>
        /// <remarks>
        /// Only the up-to-down edge counts as a press. Holding a key makes the browser repeat
        /// <c>keydown</c> at the system repeat rate, and the held set is what filters those back
        /// out, so a held arrow key scrolls the pack selector once rather than continuously.
        /// </remarks>
        /// <param name="key">The mapped key.</param>
        /// <param name="down">Whether the key went down.</param>
        public void SetKey(KeyCode key, bool down)
        {
            if (down)
            {
                if (_down.Add(key))
                {
                    _ = _pressedThisStep.Add(key);
                }
            }
            else
            {
                _ = _down.Remove(key);
            }
        }

        /// <summary>
        /// Clears the key presses the simulation step just consumed.
        /// </summary>
        /// <remarks>
        /// This belongs to the step rather than the animation frame. One frame can run several
        /// catch-up steps or none at all, and a press left standing across steps is acted on more
        /// than once, while one cleared without a step having run is never acted on at all.
        /// </remarks>
        public void EndStep()
        {
            _pressedThisStep.Clear();
        }

        /// <inheritdoc />
        public bool IsKeyPressed(KeyCode key)
        {
            return _pressedThisStep.Contains(key);
        }

        /// <inheritdoc />
        public bool CanExit => false;

        /// <inheritdoc />
        /// <remarks>
        /// This takes the main menu slot the quit button leaves empty in a browser, which cannot
        /// close its own tab.
        /// </remarks>
        public string LevelEditorUrl => "https://yell0wsuit.github.io/ctrdx-editor/";

        /// <inheritdoc />
        public void Exit()
        {
        }

        /// <inheritdoc />
        public void DrawMovie()
        {
            // The video element composites itself over the canvas, so there is no frame to
            // draw here - only the scene underneath to get rid of, which would otherwise
            // sit frozen in the letterbox around the video.
            Renderer.SetClearColor(Color.Black);
            Renderer.Clear(0);
        }

        /// <inheritdoc />
        public void OpenUrl(string url)
        {
            OpenWindow(url);
        }

        [JSImport("globalThis.open")]
        private static partial void OpenWindow(string url);
    }
}
