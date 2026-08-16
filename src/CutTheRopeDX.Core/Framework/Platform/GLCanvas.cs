using System.Collections.Generic;
using System.Globalization;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.Framework.Platform
{
    /// <summary>
    /// Shared rendering canvas that manages viewport sizing, projection setup,
    /// touch forwarding, and a lightweight FPS overlay.
    /// </summary>
    internal sealed class GLCanvas : FrameworkTypes
    {
        /// <summary>
        /// Gets the current scaled view bounds in desktop window coordinates.
        /// </summary>
        public CTRRectangle Bounds
        {
            get
            {
                _bounds.w = ScreenPresentation.Instance.SurfaceWidth;
                _bounds.h = ScreenPresentation.Instance.SurfaceHeight;
                _bounds.x = 0;
                _bounds.y = 0;
                return _bounds;
            }
        }

        /// <summary>
        /// Initializes the canvas with the default master resolution and reset state.
        /// </summary>
        /// <returns>The initialized canvas instance.</returns>
        public GLCanvas InitWithFrame()
        {
            origWidth = 2560f;
            origHeight = 1440f;
            aspect = 1440f / 2560f;
            touchesCount = 0;
            return this;
        }

        /// <summary>
        /// Enables FPS text rendering using the supplied <paramref name="font"/>.
        /// </summary>
        /// <param name="font">Font used to draw the FPS overlay.</param>
        public void InitFPSMeterWithFont(Font font)
        {
            fpsFont = font;
            fpsText = new Text().InitWithFont(fpsFont);
        }

        /// <summary>
        /// Draws the current frames-per-second value in the top-left corner.
        /// </summary>
        /// <param name="fps">FPS value to display.</param>
        public void DrawFPS(float fps)
        {
            if (fpsText != null && fpsFont != null)
            {
                string @string = fps.ToString("F1", CultureInfo.InvariantCulture);
                fpsText.SetString(@string);
                Renderer.SetColor(Color.White);
                Renderer.Enable(Renderer.GL_TEXTURE_2D);
                Renderer.Enable(Renderer.GL_BLEND);
                Renderer.SetBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONEMINUSSRCALPHA);
                fpsText.x = 5f;
                fpsText.y = 5f;
                fpsText.Draw();
                Renderer.Disable(Renderer.GL_BLEND);
                Renderer.Disable(Renderer.GL_TEXTURE_2D);
            }
        }

        /// <summary>
        /// Performs one-time OpenGL preparation work.
        /// Retained as a no-op compatibility hook.
        /// </summary>
        public static void PrepareOpenGL()
        {
        }

        /// <summary>
        /// Sets the default projection used for rendering in real screen coordinates.
        /// </summary>
        public void SetDefaultRealProjection()
        {
            SetDefaultProjection();
        }

        /// <summary>
        /// Configures the renderer viewport and orthographic projection from the published
        /// viewport. The projection describes the logical region the game draws into and the
        /// viewport describes the surface pixels it lands on; both come from the same snapshot so
        /// they cannot disagree.
        /// </summary>
        public void SetDefaultProjection()
        {
            ViewportLayoutSnapshot snapshot = ScreenPresentation.Instance.Snapshot;
            CTRRectangle render = snapshot.RenderViewport;
            CTRRectangle visible = snapshot.VisibleBounds;

            origWidth = visible.w;
            origHeight = visible.h;

            isFullscreen = PlatformServices.Window?.IsFullScreen ?? false;
            Renderer.SetViewport(xOffset, yOffset, backingWidth, backingHeight);
            Renderer.SetMatrixMode(15);
            Renderer.LoadIdentity();
            Renderer.SetOrthographic(0f, origWidth, origHeight, 0f, -1f, 1f);
            Renderer.SetMatrixMode(14);
            Renderer.LoadIdentity();
        }

        /// <summary>
        /// Compatibility hook for rectangle drawing setup.
        /// </summary>
        public static void DrawRect()
        {
        }

        /// <summary>
        /// Makes the canvas active for rendering by applying the default projection.
        /// </summary>
        public void Show()
        {
            SetDefaultProjection();
        }

        /// <summary>
        /// Hides the canvas.
        /// Retained as a no-op compatibility hook.
        /// </summary>
        public static void Hide()
        {
        }

        /// <summary>
        /// Reapplies projection state after a surface change.
        /// </summary>
        public void Reshape()
        {
            SetDefaultProjection();
        }

        /// <summary>
        /// Swaps the back buffer.
        /// Retained as a no-op because MonoGame handles presentation.
        /// </summary>
        public static void SwapBuffers()
        {
        }

        /// <summary>
        /// Forwards touch-begin events to the active touch delegate.
        /// </summary>
        /// <param name="touches">Touches that began this frame.</param>
        public void TouchesBeganwithEvent(IList<TouchLocation> touches)
        {
            _ = (touchDelegate?.TouchesBeganwithEvent(touches));
        }

        /// <summary>
        /// Forwards touch-move events to the active touch delegate.
        /// </summary>
        /// <param name="touches">Touches that moved this frame.</param>
        public void TouchesMovedwithEvent(IList<TouchLocation> touches)
        {
            _ = (touchDelegate?.TouchesMovedwithEvent(touches));
        }

        /// <summary>
        /// Forwards touch-end events to the active touch delegate.
        /// </summary>
        /// <param name="touches">Touches that ended this frame.</param>
        public void TouchesEndedwithEvent(IList<TouchLocation> touches)
        {
            _ = (touchDelegate?.TouchesEndedwithEvent(touches));
        }

        /// <summary>
        /// Forwards touch-cancel events to the active touch delegate.
        /// </summary>
        /// <param name="touches">Touches cancelled by the platform.</param>
        public void TouchesCancelledwithEvent(IList<TouchLocation> touches)
        {
            _ = (touchDelegate?.TouchesCancelledwithEvent(touches));
        }

        /// <summary>
        /// Returns whether the active touch delegate handled a back-button press.
        /// </summary>
        /// <returns><see langword="true" /> if the press was handled; otherwise <see langword="false" />.</returns>
        public bool BackButtonPressed()
        {
            return touchDelegate != null && touchDelegate.BackButtonPressed();
        }

        /// <summary>
        /// Returns whether the active touch delegate handled a menu-button press.
        /// </summary>
        /// <returns><see langword="true" /> if the press was handled; otherwise <see langword="false" />.</returns>
        public bool MenuButtonPressed()
        {
            return touchDelegate != null && touchDelegate.MenuButtonPressed();
        }

        /// <summary>
        /// Converts raw platform <paramref name="touches"/> into the canvas touch format.
        /// Currently returns the input list unchanged.
        /// </summary>
        /// <param name="touches">Touch list to convert.</param>
        /// <returns>Converted touch list.</returns>
        public static List<TouchLocation> ConvertTouches(List<TouchLocation> touches)
        {
            return touches;
        }

        /// <summary>
        /// Returns whether the canvas can become the first responder for input.
        /// </summary>
        /// <returns>Always <see langword="true" />.</returns>
        public static bool AcceptsFirstResponder()
        {
            return true;
        }

        /// <summary>
        /// Requests first-responder status for input handling.
        /// </summary>
        /// <returns>Always <see langword="true" />.</returns>
        public static bool BecomeFirstResponder()
        {
            return true;
        }

        /// <summary>
        /// Prepares renderer state for a frame before scene drawing begins.
        /// </summary>
        public void BeforeRender()
        {
            SetDefaultProjection();
            Renderer.Disable(Renderer.GL_BLEND);
        }

        /// <summary>
        /// Restores renderer state after a frame has been drawn.
        /// Retained as a no-op compatibility hook.
        /// </summary>
        public static void AfterRender()
        {
        }

        /// <summary>
        /// Default logical canvas width used by the game.
        /// </summary>
        public const float MASTER_WIDTH = 2560f;

        /// <summary>
        /// Default logical canvas height used by the game.
        /// </summary>
        public const float MASTER_HEIGHT = 1440f;

        /// <summary>
        /// Logical width of the region the projection describes.
        /// </summary>
        private float origWidth;

        /// <summary>
        /// Logical height of the region the projection describes.
        /// </summary>
        private float origHeight;

        /// <summary>
        /// Logical width of the region the projection describes.
        /// </summary>
        internal float ProjectionWidth => ScreenPresentation.Instance.Snapshot.VisibleBounds.w;

        /// <summary>
        /// Logical height of the region the projection describes.
        /// </summary>
        internal float ProjectionHeight => ScreenPresentation.Instance.Snapshot.VisibleBounds.h;

        /// <summary>
        /// Active input delegate that receives touch and button events.
        /// </summary>
        public ITouchDelegate touchDelegate;

        /// <summary>
        /// Font used by the FPS overlay.
        /// </summary>
        private Font fpsFont;

        /// <summary>
        /// Cached text element used to render the FPS overlay.
        /// </summary>
        private Text fpsText;

        /// <summary>
        /// Cached rectangle reused when returning <see cref="Bounds"/>.
        /// </summary>
        private CTRRectangle _bounds;

        /// <summary>
        /// Whether the current view is fullscreen.
        /// </summary>
        public bool isFullscreen;

        /// <summary>
        /// Current backing-surface aspect ratio.
        /// </summary>
        public float aspect;

        /// <summary>
        /// Number of touches currently tracked by the canvas.
        /// </summary>
        public int touchesCount;

        /// <summary>
        /// Horizontal surface-pixel origin of the render viewport.
        /// </summary>
        public int xOffset => (int)ScreenPresentation.Instance.Snapshot.RenderViewport.x;

        /// <summary>
        /// Vertical surface-pixel origin of the render viewport.
        /// </summary>
        public int yOffset => (int)ScreenPresentation.Instance.Snapshot.RenderViewport.y;

        /// <summary>
        /// Width of the render viewport in surface pixels.
        /// </summary>
        public int backingWidth => (int)ScreenPresentation.Instance.Snapshot.RenderViewport.w;

        /// <summary>
        /// Height of the render viewport in surface pixels.
        /// </summary>
        public int backingHeight => (int)ScreenPresentation.Instance.Snapshot.RenderViewport.h;
    }
}
