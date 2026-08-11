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
                CtrRenderer.Update();
                _accumulator -= StepSeconds;
                steps++;
            }

            _ = Application.SharedRootController();

            PlatformServices.Render.BeginFrame();
            CtrRenderer.OnDrawFrame();
            Present();
            Host?.EndFrame();
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
