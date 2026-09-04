using System;
using System.Threading.Tasks;

using SkiaSharp;

namespace CutTheRopeDX.Browser
{
    /// <summary>Runs the opt-in, single-frame Skia/WebGL worker proof.</summary>
    internal static class WorkerRenderProbe
    {
        private const string InterfaceFailure = "Could not create the Skia GL interface.";
        private const string ContextFailure = "Could not create the Skia GL context.";
        private const string SurfaceFailure = "Could not create the Skia surface.";

        /// <summary>Runs the probe and emits exactly one terminal result marker.</summary>
        public static async Task RunAsync()
        {
            ProbeState state = new();
            string result;
            try
            {
                result = await RunCoreAsync(state);
            }
            catch (Exception exception)
            {
                result = ClassifyFailure(state.Milestone, exception);
                Console.WriteLine(
                    $"ctrdx-render-probe: failure milestone={state.Milestone} " +
                    $"exception={exception.GetType().Name} message={exception.Message}");
            }

            Console.WriteLine($"ctrdx-render-probe: result={result}");
        }

        private static async Task<string> RunCoreAsync(ProbeState state)
        {
            int ownerThreadId = Environment.CurrentManagedThreadId;
            Mark(state, "entry", $"owner={ownerThreadId}");

            // Worker execution is proven here rather than in the boot path, so a normal
            // launch does not carry a diagnostic it never reads.
            (int workerThreadId, int workerResult) = await Task.Run(
                () => (Environment.CurrentManagedThreadId, 42));
            bool differentThread = ownerThreadId != workerThreadId;
            Console.WriteLine(
                $"ctrdx-thread-smoke: owner={ownerThreadId} worker={workerThreadId} " +
                $"different={differentThread.ToString().ToLowerInvariant()} " +
                $"result={workerResult}");

            string executionContext = RenderProbeInterop.ExecutionContext();
            Mark(state, "browser-context", $"context={executionContext}");

            Mark(state, "context-create");
            int fbo = GLContextInterop.CreateContext("game");
            int[] size = GLContextInterop.CanvasSize("game");
            if (size.Length < 2 || size[0] <= 0 || size[1] <= 0)
            {
                return "CONTEXT_CREATE_FAILED";
            }

            int[] contextStatus = RenderProbeInterop.CurrentContextStatus();
            if (contextStatus.Length < 2 || contextStatus[0] == 0)
            {
                return "CONTEXT_CREATE_FAILED";
            }
            Mark(
                state,
                "webgl-context-created",
                $"fbo={fbo} size={size[0]}x{size[1]}");

            if (contextStatus[1] == 0)
            {
                return "CONTEXT_NOT_CURRENT";
            }
            Mark(
                state,
                "current-context-verified",
                $"current={contextStatus[0]} usable={contextStatus[1]}");

            Mark(state, "skia-interface-create");
            using SkiaSurface surface = new(fbo, size[0], size[1]);
            Mark(state, "skia-interface-created");
            Mark(state, "skia-context-created");
            Mark(state, "skia-surface-created");

            Mark(state, "clear");
            if (!RenderProbeInterop.ClearErrors())
            {
                return "CONTEXT_NOT_CURRENT";
            }
            surface.Canvas.Clear(new SKColor(17, 34, 51, 255));

            Mark(state, "flush");
            surface.Flush();
            Mark(state, "clear-flushed");

            Mark(state, "pixel-readback");
            int[] pixel = RenderProbeInterop.ReadCenterPixel("game");
            if (pixel.Length != 5)
            {
                return "PIXEL_READBACK_FAILED";
            }

            Mark(state, "gl-error", $"error={pixel[4]}");
            if (pixel[4] != 0)
            {
                return "PIXEL_READBACK_FAILED";
            }

            Mark(
                state,
                "pixel-read",
                $"rgba={pixel[0]},{pixel[1]},{pixel[2]},{pixel[3]}");
            Mark(state, "pixel-compare");
            if (!RenderProbeInterop.IsExpectedPixel(pixel))
            {
                return "PIXEL_MISMATCH";
            }

            Mark(state, "pixel-verified");
            return "GATE2_PASS";
        }

        private static string ClassifyFailure(string milestone, Exception exception)
        {
            return exception.Message switch
            {
                InterfaceFailure => "SKIA_INTERFACE_FAILED",
                ContextFailure => "SKIA_CONTEXT_FAILED",
                SurfaceFailure => "SKIA_SURFACE_FAILED",
                _ when milestone.StartsWith("skia", StringComparison.Ordinal) =>
                    "SKIA_SURFACE_FAILED",
                _ when milestone is "clear" or "flush" or "clear-flushed" =>
                    "SKIA_FLUSH_FAILED",
                _ when milestone.StartsWith("pixel", StringComparison.Ordinal) ||
                    milestone == "gl-error" => "PIXEL_READBACK_FAILED",
                _ when milestone.StartsWith("current", StringComparison.Ordinal) =>
                    "CONTEXT_NOT_CURRENT",
                _ => "CONTEXT_CREATE_FAILED",
            };
        }

        private static void Mark(ProbeState state, string name, string detail = null)
        {
            state.Milestone = name;
            Console.WriteLine(
                $"ctrdx-render-probe: milestone={name}" +
                (detail is null ? string.Empty : $" {detail}"));
        }
    }

    /// <summary>Carries the probe's active milestone across its awaits.</summary>
    internal sealed class ProbeState
    {
        /// <summary>The most recent milestone the probe reached.</summary>
        public string Milestone { get; set; } = "entry";
    }
}
