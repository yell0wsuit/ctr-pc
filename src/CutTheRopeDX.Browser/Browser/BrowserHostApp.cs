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
        private readonly HashSet<KeyCode> _pressedThisFrame = [];

        /// <summary>Records a key transition from the DOM.</summary>
        /// <param name="key">The mapped key.</param>
        /// <param name="down">Whether the key went down.</param>
        public void SetKey(KeyCode key, bool down)
        {
            if (down)
            {
                if (_down.Add(key))
                {
                    _ = _pressedThisFrame.Add(key);
                }
            }
            else
            {
                _ = _down.Remove(key);
            }
        }

        /// <summary>Clears the per-frame key transitions.</summary>
        public void EndFrame()
        {
            _pressedThisFrame.Clear();
        }

        /// <inheritdoc />
        public bool IsKeyPressed(KeyCode key)
        {
            return _pressedThisFrame.Contains(key);
        }

        /// <inheritdoc />
        public bool CanExit => false;

        /// <inheritdoc />
        public void Exit()
        {
        }

        /// <inheritdoc />
        public void DrawMovie()
        {
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
