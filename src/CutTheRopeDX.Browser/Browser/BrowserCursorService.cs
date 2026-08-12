using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;

using CutTheRopeDX.Framework.Platform;

namespace CutTheRopeDX.Browser
{
    /// <summary>Cursor control, backed by the CSS cursor on the canvas.</summary>
    /// <remarks>
    /// The desktop host swaps a native cursor rather than drawing one into the scene, so this
    /// does the same with the two cursor bitmaps the game already ships.
    /// </remarks>
    internal sealed partial class BrowserCursorService : ICursorService
    {
        /// <summary>Imports cursor.js. Must be awaited once before any other call.</summary>
        public static Task ImportAsync()
        {
            return JSHost.ImportAsync("cursor", "../cursor.js");
        }

        /// <inheritdoc />
        public void Enable(bool enabled)
        {
            SetEnabled(enabled);
        }

        /// <inheritdoc />
        public void ReleaseButtons()
        {
            SetPressed(false);
        }

        /// <summary>Switches to the pressed bitmap while a pointer is held.</summary>
        /// <param name="pressed">Whether a pointer button is down.</param>
        public static void SetHeld(bool pressed)
        {
            SetPressed(pressed);
        }

        [JSImport("setEnabled", "cursor")]
        private static partial void SetEnabled(bool enabled);

        [JSImport("setPressed", "cursor")]
        private static partial void SetPressed(bool pressed);
    }
}
