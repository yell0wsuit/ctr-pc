using System;
using System.Runtime.InteropServices.JavaScript;

using CutTheRopeDX.Commons;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Media;
using CutTheRopeDX.Framework.Platform;

namespace CutTheRopeDX.Browser
{
    /// <summary>Drives Core from <c>requestAnimationFrame</c> at a fixed timestep.</summary>
    internal static partial class GameLoop
    {
        private const double StepSeconds = 1.0 / 60.0;
        private const int MaxCatchUpSteps = 5;

        private static double _lastTimestampMs;
        private static double _accumulator;
        private static bool _active = true;

        internal static SkiaSurface Surface { get; set; }

        internal static BrowserHostApp Host { get; set; }

        /// <summary>Advances and draws one animation frame.</summary>
        /// <param name="timestampMs">The animation-frame timestamp in milliseconds.</param>
        [JSExport]
        internal static void Tick(double timestampMs)
        {
            double elapsed = _lastTimestampMs == 0
                ? StepSeconds
                : (timestampMs - _lastTimestampMs) / 1000.0;
            _lastTimestampMs = timestampMs;

            ResizeIfNeeded();

            _accumulator += Math.Min(elapsed, StepSeconds * MaxCatchUpSteps);

            int steps = 0;
            while (_accumulator >= StepSeconds && steps < MaxCatchUpSteps)
            {
                SoundMgr.Update(TimeSpan.FromSeconds(StepSeconds));
                // Every key read happens inside a step, and the step ends by clearing the
                // presses it consumed. Core asks "was this pressed since I last looked" once per
                // Update, which is once per step, so a frame that runs two catch-up steps would
                // otherwise hand the same arrow press to both and skip two packs at once. The
                // desktop host reaches the same place from the other side: its back key is read
                // in Update, and its key edges latch per read rather than per frame.
                HandleBackKey();
                CtrRenderer.Update();
                Preferences.Update();
                Host?.EndStep();
                _accumulator -= StepSeconds;
                steps++;
            }

            _ = Application.SharedRootController();

            PlatformServices.Render.BeginFrame();
            CtrRenderer.OnDrawFrame();
            Present();
        }

        /// <summary>
        /// Pauses or resumes the game as the page gains and loses focus, mirroring the desktop
        /// host's activate and deactivate handlers.
        /// </summary>
        /// <param name="active">Whether the page is both visible and focused.</param>
        /// <remarks>
        /// A blurred but still visible window keeps receiving animation frames, so the browser
        /// alone never stops the simulation — only the pause seam does.
        /// </remarks>
        [JSExport]
        internal static void SetActive(bool active)
        {
            if (active == _active)
            {
                return;
            }

            _active = active;
            if (active)
            {
                // The blurred interval is time the player did not see. Restarting the clock
                // drops it rather than letting the loop burn catch-up steps on it.
                _lastTimestampMs = 0;
                CtrRenderer.Java_com_zeptolab_ctr_CtrRenderer_nativeResume();
            }
            else
            {
                CtrRenderer.Java_com_zeptolab_ctr_CtrRenderer_nativePause();
                // Resigning active requests a save; the loop is about to stop advancing, so
                // nothing else would write it.
                Preferences.Update();
            }
        }

        /// <summary>
        /// Writes any pending preference save immediately. The page can go away between two
        /// animation frames, so the loop's per-step flush needs a lifecycle-driven partner.
        /// </summary>
        [JSExport]
        internal static void Flush()
        {
            Preferences.Update();
        }

        /// <summary>
        /// Runs the back action, which the desktop host drives from its own update loop.
        /// </summary>
        private static void HandleBackKey()
        {
            if (Host?.IsKeyPressed(KeyCode.Escape) == true)
            {
                Application.SharedMovieMgr().Stop();
                _ = CtrRenderer.Java_com_zeptolab_ctr_CtrRenderer_nativeBackPressed();
            }
        }

        private static void ResizeIfNeeded()
        {
            int[] canvas = GLContextInterop.CanvasSize("game");
            if (canvas[0] == Surface.Width && canvas[1] == Surface.Height)
            {
                return;
            }

            Surface.Resize(canvas[0], canvas[1]);
            ScreenPresentation.Instance.SetSurfaceSize(canvas[0], canvas[1]);
            CtrRenderer.OnSurfaceChanged(canvas[0], canvas[1]);
        }

        private static void Present()
        {
            PlatformServices.Render.EndFrame();
            PlatformServices.Render.CopyFromRenderTargetToScreen();
            Surface.Flush();
        }
    }
}
