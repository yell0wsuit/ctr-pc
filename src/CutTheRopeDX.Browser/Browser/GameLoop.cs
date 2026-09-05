using System;
using System.Runtime.InteropServices;

using CutTheRopeDX.Commons;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Media;
using CutTheRopeDX.Framework.Platform;

namespace CutTheRopeDX.Browser
{
    /// <summary>Drives Core from <c>requestAnimationFrame</c> at a fixed timestep.</summary>
    internal static unsafe partial class GameLoop
    {
        private const double StepSeconds = 1.0 / 60.0;
        private const int MaxCatchUpSteps = 5;

        private static double _lastTimestampMs;
        private static double _accumulator;
        private static bool _active = true;
        private static bool _hidden;
        private static bool _contextLostReported;
        private static int _reportedDroppedEvents;

        internal static SkiaSurface Surface { get; set; }

        internal static BrowserHostApp Host { get; set; }

        /// <summary>Begins driving frames from this thread's own animation frame.</summary>
        internal static void Start()
        {
            HostShim.SetFrameCallback(&OnFrame);
            HostShim.RequestFrame();
        }

        /// <summary>
        /// Runs one frame and schedules the next. Re-arming from inside the callback
        /// rather than on a timer is what keeps the loop on the display's cadence, and
        /// the transferred canvas presents when this task ends.
        /// </summary>
        /// <remarks>
        /// Nothing may propagate out of here. This is called from native code, which has
        /// no handler to unwind into, so an escaping exception takes the runtime down
        /// with it and the loop stops for good rather than dropping one frame.
        /// </remarks>
        [UnmanagedCallersOnly]
        private static void OnFrame(double timestampMs)
        {
            bool requestNextFrame = true;
            try
            {
                if (HostShim.ContextLost() != 0)
                {
                    requestNextFrame = false;
                    if (!_contextLostReported)
                    {
                        _contextLostReported = true;
                        Console.WriteLine(
                            "ctrdx-context-lost: simulation paused; reload required");
                        CtrRenderer.Java_com_zeptolab_ctr_CtrRenderer_nativePause();
                        // Forced: no further frame is coming, so a save still backing off from
                        // an earlier failure would never get another chance.
                        Preferences.Update(force: true);
                    }

                    return;
                }

                Tick(timestampMs);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"ctrdx-frame-error: {exception}");
            }
            finally
            {
                if (requestNextFrame)
                {
                    try
                    {
                        HostShim.RequestFrame();
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine($"ctrdx-frame-request-error: {exception}");
                    }
                }
            }
        }

        /// <summary>Advances and draws one animation frame.</summary>
        /// <param name="timestampMs">The animation-frame timestamp in milliseconds.</param>
        private static void Tick(double timestampMs)
        {
            // Between frames, not during one: a level that arrives mid-step would be half-applied.
            PlaytestSession.Pump();

            // Between frames, never inside a step: a step that saw input applied
            // half-way through would act on a world its own physics had not produced.
            DrainHostEvents();

            double elapsed = _lastTimestampMs == 0
                ? StepSeconds
                : (timestampMs - _lastTimestampMs) / 1000.0;
            _lastTimestampMs = timestampMs;

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
        /// <param name="hidden">Whether the page is hidden, as opposed to merely unfocused.</param>
        /// <remarks>
        /// A blurred but still visible window keeps receiving animation frames, so the browser
        /// alone never stops the simulation — only the pause seam does. Hidden is tracked apart
        /// from active because the two call for different responses: a window pushed behind
        /// another is inactive but still composited, while a hidden page is not drawn at all.
        /// </remarks>
        internal static void SetActive(bool active, bool hidden)
        {
            bool wasHidden = _hidden;
            _hidden = hidden;

            if (active == _active)
            {
                PurgeOnHidden(wasHidden);
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
                // Resigning active requests a save. Every full fixed step also saves, so the
                // only remaining exposure is a change made during the final partial frame.
                // Forced, because the page may be suspended before another step runs.
                Preferences.Update(force: true);
            }

            PurgeOnHidden(wasHidden);
        }

        /// <summary>Hands back Skia's scratch cache as the page becomes hidden.</summary>
        /// <param name="wasHidden">Whether the page was already hidden before this transition.</param>
        /// <remarks>
        /// Only on the edge into hidden. Losing focus is not the same event: a window merely
        /// pushed behind another is still composited and still on screen, and dropping its
        /// scratch resources there buys no headroom while costing a hitch to regenerate them
        /// the moment it comes back.
        /// </remarks>
        private static void PurgeOnHidden(bool wasHidden)
        {
            if (_hidden && !wasHidden)
            {
                Surface?.PurgeGpuResources();
            }
        }

        /// <summary>
        /// Adopts a canvas shape the browser thread reported. Sizing the backing store
        /// belongs to this thread: the canvas was transferred, so assigning its width
        /// on the browser thread throws.
        /// </summary>
        internal static void ApplyResize(float cssWidth, float cssHeight, float ratio)
        {
            int width = Math.Max(1, (int)Math.Round(cssWidth * ratio));
            int height = Math.Max(1, (int)Math.Round(cssHeight * ratio));
            if (width == Surface.Width
                && height == Surface.Height
                && ratio == ScreenPresentation.Instance.Snapshot.DevicePixelRatio)
            {
                return;
            }

            _ = HostShim.ResizeCanvas(width, height);
            Surface.Resize(width, height);
            CtrRenderer.OnSurfaceChanged(width, height, ratio);
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

        private static void Present()
        {
            PlatformServices.Render.EndFrame();
            PlatformServices.Render.CopyFromRenderTargetToScreen();
            Surface.Flush();
        }

        private static void DrainHostEvents()
        {
            foreach (HostEvent value in HostEventQueue.Drain())
            {
                switch (value.Kind)
                {
                    case HostEventKind.Pointer:
                        InputRouter.HandlePointer(
                            BitConverter.Int32BitsToSingle(value.Word1),
                            BitConverter.Int32BitsToSingle(value.Word2),
                            BitConverter.Int32BitsToSingle(value.Word3),
                            BitConverter.Int32BitsToSingle(value.Word4),
                            value.Word0);
                        break;
                    case HostEventKind.Key:
                        InputRouter.HandleKey(value.Word1, value.Word0 != 0);
                        break;
                    case HostEventKind.Wheel:
                        InputRouter.HandleWheel(value.Word0);
                        break;
                    case HostEventKind.Active:
                        SetActive(value.Word0 != 0, value.Word1 != 0);
                        break;
                    case HostEventKind.Resize:
                        ApplyResize(
                            BitConverter.Int32BitsToSingle(value.Word0),
                            BitConverter.Int32BitsToSingle(value.Word1),
                            BitConverter.Int32BitsToSingle(value.Word2));
                        break;
                    case HostEventKind.None:
                        break;
                    default:
                        break;
                }
            }

            int dropped = HostEventQueue.DroppedCount();
            if (dropped != _reportedDroppedEvents)
            {
                _reportedDroppedEvents = dropped;
                Console.WriteLine($"ctrdx-host-events-dropped: total={dropped}");
            }
        }
    }
}
