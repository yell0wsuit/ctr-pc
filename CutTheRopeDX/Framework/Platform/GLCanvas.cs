using System.Collections.Generic;

using CutTheRopeDX.Desktop;
using CutTheRopeDX.Desktop.Graphics;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

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
        public Rectangle Bounds
        {
            get
            {
                _ = Global.XnaGame.GraphicsDevice.Viewport.Bounds;
                Rectangle currentSize = Global.ScreenSizeManager.CurrentSize;
                _bounds.Width = currentSize.Width;
                _bounds.Height = currentSize.Height;
                _bounds.X = currentSize.X;
                _bounds.Y = currentSize.Y;
                return _bounds;
            }
        }

        /// <summary>
        /// Initializes the canvas with the default master resolution and reset state.
        /// </summary>
        /// <returns>The initialized canvas instance.</returns>
        public GLCanvas InitWithFrame()
        {
            xOffset = 0;
            yOffset = 0;
            origWidth = backingWidth = 2560;
            origHeight = backingHeight = 1440;
            aspect = backingHeight / backingWidth;
            touchesCount = 0;
            return this;
        }

        /// <summary>
        /// Draws the frame diagnostics readout in the top-left corner, when it is switched on.
        /// </summary>
        /// <param name="fps">Frames per second measured by the runtime.</param>
        /// <remarks>
        /// Called every frame; costs nothing until <see cref="RenderDiagnostics"/> is enabled. The font is
        /// fetched on first use rather than at startup, because the resource manager has nothing loaded
        /// when the canvas is created and the readout has to survive being switched on at any point.
        /// </remarks>
        public void DrawFPS(float fps)
        {
            if (!RenderDiagnostics.Enabled)
            {
                return;
            }
            fpsFont ??= Application.GetFont(Resources.Fnt.SmallFont);
            if (fpsFont == null)
            {
                return;
            }
            fpsText ??= new Text().InitWithFont(fpsFont);
            SoftwareRenderScale scale = SoftwareRenderScale.Shared;
            fpsText.SetString(RenderDiagnostics.Format(
                (int)fps,
                scale.LastMedianMs,
                scale.Step,
                backingWidth,
                backingHeight,
                GraphicsFallback.IsSoftwareRendering));
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
        /// Configures the renderer viewport and orthographic projection for the current scaled view.
        /// </summary>
        public void SetDefaultProjection()
        {
            // Always calculate offsets for proper letterboxing in both windowed and fullscreen modes
            xOffset = Global.ScreenSizeManager.ScaledViewRect.X;
            xOffsetScaled = (int)(-xOffset / Global.ScreenSizeManager.WidthAspectRatio);
            isFullscreen = Global.ScreenSizeManager.IsFullScreen;
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
        /// Recomputes backing dimensions from the scaled view rectangle and reapplies projection state.
        /// </summary>
        /// <remarks>
        /// These dimensions size the render target the frame is drawn into, and nothing else: the
        /// projection below is built from the logical game size, and the finished target is stretched back
        /// over the full on-screen rectangle. Capping them therefore lowers the rendering resolution
        /// without shrinking the window or moving anything in game coordinates, which is what makes the
        /// software renderer affordable at desktop resolutions.
        /// </remarks>
        public void Reshape()
        {
            Rectangle scaledViewRect = Global.ScreenSizeManager.ScaledViewRect;
            // The policy names the scene height directly, which brings every display to the same picture and
            // lets it back off further on a machine that cannot afford even that. On the hardware path there
            // is no target and this is the size it always was.
            _appliedRenderLines = GraphicsFallback.IsSoftwareRendering ? SoftwareRenderScale.Shared.TargetLines : NoRenderLineTarget;
            (backingWidth, backingHeight) = _appliedRenderLines == NoRenderLineTarget
                ? (scaledViewRect.Width, scaledViewRect.Height)
                : SoftwareRenderScale.Apply(scaledViewRect.Width, scaledViewRect.Height, _appliedRenderLines);
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
            // Reshape is otherwise only reached from a window resize, and the adaptive divisor changes
            // without one. Recomputing the backing size here is what lets a divisor the policy picked mid-
            // level take effect: Renderer.SetViewport, which SetDefaultProjection calls either way, rebuilds
            // the render target as soon as the size it is handed differs from the target it holds.
            if (GraphicsFallback.IsSoftwareRendering && _appliedRenderLines != SoftwareRenderScale.Shared.TargetLines)
            {
                Reshape();
            }
            else
            {
                SetDefaultProjection();
            }
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
        /// Logical canvas width used when building the default orthographic projection.
        /// </summary>
        private int origWidth;

        /// <summary>
        /// Logical canvas height used when building the default orthographic projection.
        /// </summary>
        private int origHeight;

        /// <summary>
        /// Active input delegate that receives touch and button events.
        /// </summary>
        public ITouchDelegate touchDelegate;

        /// <summary>
        /// Font used by the diagnostics overlay, fetched on first use.
        /// </summary>
        private FontGeneric fpsFont;

        /// <summary>
        /// Cached text element used to render the FPS overlay.
        /// </summary>
        private Text fpsText;

        /// <summary>
        /// Cached rectangle reused when returning <see cref="Bounds"/>.
        /// </summary>
        private Rectangle _bounds;

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
        /// Horizontal viewport offset used for letterboxing.
        /// </summary>
        public int xOffset;

        /// <summary>
        /// Vertical viewport offset used for letterboxing.
        /// </summary>
        public int yOffset;

        /// <summary>
        /// Horizontal viewport offset converted into logical screen space.
        /// </summary>
        public int xOffsetScaled;

        // public int yOffsetScaled;

        /// <summary>
        /// Current backing-surface width after scaling and letterboxing.
        /// </summary>
        public int backingWidth;

        /// <summary>
        /// Current backing-surface height after scaling and letterboxing.
        /// </summary>
        public int backingHeight;

        /// <summary>
        /// Sentinel for the hardware path, where the scene is rendered at its full on-screen size.
        /// </summary>
        private const int NoRenderLineTarget = 0;

        /// <summary>
        /// Render-line target the current backing size was computed with, so a change to it can be noticed
        /// per frame.
        /// </summary>
        private int _appliedRenderLines = NoRenderLineTarget;
    }
}
