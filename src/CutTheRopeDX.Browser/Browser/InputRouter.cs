using System.Numerics;

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

        /// <summary>Handles one pointer event reported in CSS pixels.</summary>
        /// <param name="offsetX">Pointer X relative to the canvas box, in CSS pixels.</param>
        /// <param name="offsetY">Pointer Y relative to the canvas box, in CSS pixels.</param>
        /// <param name="rectWidth">The canvas box width the offsets were measured against.</param>
        /// <param name="rectHeight">The canvas box height the offsets were measured against.</param>
        /// <param name="phase">One of the pointer phase constants.</param>
        /// <remarks>
        /// The browser thread reports CSS pixels and the box that produced them, because
        /// it no longer owns the backing store and cannot know its size. Scaling to
        /// backing pixels therefore happens here.
        /// </remarks>
        internal static void HandlePointer(
            float offsetX, float offsetY, float rectWidth, float rectHeight, int phase)
        {
            if (rectWidth <= 0f || rectHeight <= 0f)
            {
                return;
            }

            float x = offsetX * (GameLoop.Surface.Width / rectWidth);
            float y = offsetY * (GameLoop.Surface.Height / rectHeight);
            OnPointer(x, y, phase);
        }

        private static void OnPointer(double x, double y, int phase)
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
        internal static void HandleWheel(int delta)
        {
            _ = Application.SharedRootController().HandleMouseWheel(delta);
        }

        /// <summary>Handles one keyboard transition.</summary>
        internal static void HandleKey(int keyId, bool down)
        {
            KeyCode? mapped = Map((HostKey)keyId);
            if (mapped is not null)
            {
                Host?.SetKey(mapped.Value, down);
            }
        }

        private static KeyCode? Map(HostKey key)
        {
            return key switch
            {
                HostKey.Escape => KeyCode.Escape,
                HostKey.F5 => KeyCode.F5,
                HostKey.Space => KeyCode.Space,
                HostKey.Enter => KeyCode.Enter,
                HostKey.Left => KeyCode.Left,
                HostKey.Right => KeyCode.Right,
                HostKey.None => null,
                _ => null,
            };
        }
    }
}
