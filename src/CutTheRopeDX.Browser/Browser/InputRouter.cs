using System.Numerics;
using System.Runtime.InteropServices.JavaScript;

using CutTheRopeDX.Commons;
using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Platform;

namespace CutTheRopeDX.Browser
{
    /// <summary>Routes DOM pointer and keyboard events into Core.</summary>
    internal static partial class InputRouter
    {
        /// <summary>Pointer went down.</summary>
        public const int PhaseDown = 0;

        /// <summary>Pointer moved.</summary>
        public const int PhaseMove = 1;

        /// <summary>Pointer went up.</summary>
        public const int PhaseUp = 2;

        internal static BrowserHostApp Host { get; set; }

        private static bool _pointerDown;

        /// <summary>Handles one pointer event in canvas backing pixels.</summary>
        /// <param name="x">Pointer X in canvas backing pixels.</param>
        /// <param name="y">Pointer Y in canvas backing pixels.</param>
        /// <param name="phase">One of the pointer phase constants.</param>
        [JSExport]
        internal static void OnPointer(double x, double y, int phase)
        {
            ViewportLayoutSnapshot snapshot = ScreenPresentation.Instance.Snapshot;
            CTRRectangle render = snapshot.RenderViewport;

            // Surface pixels relative to the drawn region. Touches are reported in this space
            // rather than in logical units: Core divides what it is handed by the viewport scale
            // on the way in, so a logical position would be scaled a second time and land further
            // from the corner the further out it was - the pointer would drift away from itself.
            float viewX = (float)x - render.x;
            float viewY = (float)y - render.y;

            float logicalX = viewX / snapshot.Scale;
            float logicalY = viewY / snapshot.Scale;

            _ = Application.SharedRootController().MouseMoved(logicalX, logicalY);

            // A hovering pointer must produce no touch at all. Desktop synthesises touches from
            // the mouse only while its button is held, and Core's touch entry point raises began,
            // moved and ended for whatever it is handed without looking at the state — so
            // forwarding hover would raise a touch-began on every mouse move. That skips the
            // startup splash the instant the pointer crosses the canvas, and presses buttons
            // under the cursor for the rest of the game.
            TouchLocationState? state = phase switch
            {
                PhaseDown => TouchLocationState.Pressed,
                PhaseUp => _pointerDown ? TouchLocationState.Released : null,
                _ => _pointerDown ? TouchLocationState.Moved : null,
            };

            bool wasDown = _pointerDown;
            _pointerDown = phase switch
            {
                PhaseDown => true,
                PhaseUp => false,
                _ => _pointerDown,
            };

            // Desktop picks its cursor bitmap from the live button state each frame; this host
            // has no such poll, so the transition drives it.
            if (_pointerDown != wasDown)
            {
                BrowserCursorService.SetHeld(_pointerDown);
            }

            if (state is null)
            {
                return;
            }

            CtrRenderer.Java_com_zeptolab_ctr_CtrRenderer_nativeTouchProcess(
                [new TouchLocation(0, state.Value, new Vector2(viewX, viewY))]);
        }

        /// <summary>Scrolls the active view, as the desktop host does from its update loop.</summary>
        /// <param name="delta">
        /// Wheel movement in the desktop's units, where one notch is 120 and positive scrolls up.
        /// </param>
        [JSExport]
        internal static void OnWheel(int delta)
        {
            _ = Application.SharedRootController().HandleMouseWheel(delta);
        }

        /// <summary>Handles one keyboard transition.</summary>
        /// <param name="code">The DOM <c>KeyboardEvent.code</c> value.</param>
        /// <param name="down">Whether the key went down.</param>
        [JSExport]
        internal static void OnKey(string code, bool down)
        {
            KeyCode? mapped = Map(code);
            if (mapped is not null)
            {
                Host?.SetKey(mapped.Value, down);
            }
        }

        private static KeyCode? Map(string code)
        {
            return code switch
            {
                // Q and R stand in for Escape and F5: a browser keeps both of those for itself,
                // leaving fullscreen on one and reloading the page on the other, and neither can
                // be reliably taken back from it.
                "KeyQ" => KeyCode.Escape,
                "KeyR" => KeyCode.F5,
                "Space" => KeyCode.Space,
                "Enter" => KeyCode.Enter,
                "ArrowLeft" => KeyCode.Left,
                "ArrowRight" => KeyCode.Right,
                _ => null,
            };
        }
    }
}
