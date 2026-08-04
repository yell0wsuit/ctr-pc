using System;

namespace CutTheRopeDX.Desktop.Graphics
{
    /// <summary>
    /// Picks the height the scene is rendered at when the bundled software renderer is in use, from how
    /// long recent frames took.
    /// </summary>
    /// <remarks>
    /// The scene is drawn into a render target that is then stretched over the on-screen rectangle, so
    /// lowering that target's height cuts scene fill-rate quadratically without moving anything in game
    /// coordinates. The blit that follows is not affected: it always covers the whole back buffer. Neither
    /// is the per-frame CPU work. Those two are the floor this cannot get under, which is why the overlay
    /// reports the step and the render size alongside the frame time rather than only the frame rate.
    /// <para>
    /// The ladder is absolute line counts rather than divisors of the display. Whole divisors would keep the
    /// blit on the point-sampled path in <c>Renderer.ScreenBlitSamplerFor</c>, but on a 768-line display the
    /// only rungs they offer are 768 and 384, and 384 is visibly coarse. Naming the heights instead lands
    /// every display on the same picture, and costs point sampling only where the ratio does not come out
    /// whole: 1080 and 2160-line displays still divide evenly into the first rung and keep it.
    /// </para>
    /// <para>
    /// Timing is fed in rather than measured here so the policy can be tested without a graphics device.
    /// </para>
    /// </remarks>
    internal sealed class SoftwareRenderScale
    {
        /// <summary>
        /// Scene heights the policy may render at, finest first, in lines.
        /// </summary>
        /// <remarks>
        /// The first rung is where the software renderer stops being fill-rate bound on a weak dual-core
        /// machine while still looking like the game. The two below it are held back for machines that
        /// cannot manage even that; each drops fill-rate by roughly a third of what the rung above costs.
        /// </remarks>
        public static readonly int[] RenderLineLadder = [540, 432, 360];

        /// <summary>Frames gathered before the step is reconsidered.</summary>
        public const int WindowFrames = 30;

        /// <summary>
        /// Frame-time median above which the scene is rendered smaller. Under the 16.6ms budget a 60Hz frame
        /// has, with room left for the present the measurement excludes.
        /// </summary>
        public const double StepDownAboveMs = 13.0;

        /// <summary>
        /// Frame-time median below which returning to a taller scene is considered. Well under half the
        /// budget, because the rung above costs meaningfully more fill and the frame has to survive it.
        /// </summary>
        public const double StepUpBelowMs = 7.0;

        /// <summary>Consecutive quiet windows required before the first step up.</summary>
        public const int BaseGoodWindowsForStepUp = 10;

        /// <summary>
        /// Shared instance the render path reads. The policy is per-process because the render target it
        /// governs is.
        /// </summary>
        public static SoftwareRenderScale Shared { get; } = new();

        /// <summary>Coarsest rung on the ladder.</summary>
        public static int MaxStep => RenderLineLadder.Length - 1;

        /// <summary>Gets the rung currently in use, zero being the tallest scene.</summary>
        public int Step { get; private set; }

        /// <summary>Gets the scene height currently rendered at, in lines.</summary>
        public int TargetLines => RenderLineLadder[Step];

        /// <summary>Gets the median frame time of the last completed window, in milliseconds.</summary>
        public double LastMedianMs { get; private set; }

        /// <summary>
        /// Fits an on-screen size to <paramref name="targetLines"/>, keeping its aspect ratio.
        /// </summary>
        /// <param name="width">Width the finished frame is shown at.</param>
        /// <param name="height">Height the finished frame is shown at.</param>
        /// <param name="targetLines">Scene height to render at.</param>
        /// <returns>The size to render the scene at, never degenerate.</returns>
        /// <remarks>
        /// A display already at or under the target is rendered whole. Both dimensions move together,
        /// because the result is stretched back over the full on-screen rectangle and a changed aspect ratio
        /// would show as a distorted picture.
        /// </remarks>
        public static (int Width, int Height) Apply(int width, int height, int targetLines)
        {
            if (height <= targetLines || height <= 0 || targetLines <= 0)
            {
                return (width, height);
            }
            int scaledWidth = ((width * targetLines) + (height / 2)) / height;
            return (Math.Max(1, scaledWidth), targetLines);
        }

        /// <summary>
        /// Records how long one frame's work took and reconsiders the step once a window is full.
        /// </summary>
        /// <param name="milliseconds">Wall-clock time the frame spent updating and drawing.</param>
        public void RecordFrame(double milliseconds)
        {
            // A frame that reports nothing usable says nothing about the load either.
            if (double.IsNaN(milliseconds) || milliseconds < 0d)
            {
                return;
            }

            _samples[_sampleCount++] = milliseconds;
            if (_sampleCount < WindowFrames)
            {
                return;
            }

            double median = Median(_samples, _sampleCount);
            _sampleCount = 0;
            LastMedianMs = median;

            // The window straddling a change contains frames drawn at both sizes, so it measures neither.
            // Spend it settling.
            if (_settling)
            {
                _settling = false;
                return;
            }

            if (_windowsSinceStepUp < int.MaxValue)
            {
                _windowsSinceStepUp++;
            }

            if (median > StepDownAboveMs)
            {
                StepDown();
                return;
            }

            if (median < StepUpBelowMs && Step > 0)
            {
                if (++_goodWindows >= RequiredGoodWindows)
                {
                    StepUp();
                }
                return;
            }

            // Anything between the two thresholds is the frame rate the step was chosen for. Treat the quiet
            // streak as broken so a step up needs a fresh run of genuinely cheap windows.
            _goodWindows = 0;
        }

        /// <summary>
        /// Drops the partial window and the quiet streak, keeping the current step.
        /// </summary>
        /// <remarks>
        /// For stretches whose frame times describe something other than the scene, such as a movie or a
        /// level load. Feeding those to the policy would move the step for the frames either side of them,
        /// which are the ones that have to look right.
        /// </remarks>
        public void Reset()
        {
            _sampleCount = 0;
            _goodWindows = 0;
        }

        /// <summary>
        /// Quiet windows currently required before stepping up.
        /// </summary>
        /// <remarks>
        /// Doubles for each step up that had to be undone shortly afterwards. Without that, a frame time
        /// sitting near the step-up threshold makes the render size oscillate, and a resolution that visibly
        /// pops back and forth is worse to look at than one that settled a rung too coarse.
        /// </remarks>
        private int RequiredGoodWindows => BaseGoodWindowsForStepUp << Math.Min(_revertedStepUps, MaxGoodWindowShifts);

        private void StepDown()
        {
            _goodWindows = 0;
            if (Step >= MaxStep)
            {
                return;
            }
            // A step down arriving on the heels of a step up means that step up was a mistake.
            if (_windowsSinceStepUp <= RevertWindowLimit)
            {
                _revertedStepUps++;
            }
            Step++;
            _settling = true;
        }

        private void StepUp()
        {
            Step--;
            _goodWindows = 0;
            _windowsSinceStepUp = 0;
            _settling = true;
        }

        /// <summary>
        /// Median of the first <paramref name="count"/> samples.
        /// </summary>
        /// <remarks>
        /// Median rather than mean so that one stalled frame, from a garbage collection or a texture
        /// upload, does not drop the resolution on its own.
        /// </remarks>
        private static double Median(double[] samples, int count)
        {
            double[] ordered = new double[count];
            Array.Copy(samples, ordered, count);
            Array.Sort(ordered);
            int middle = count / 2;
            return count % 2 != 0 ? ordered[middle] : (ordered[middle - 1] + ordered[middle]) / 2d;
        }

        /// <summary>How many doublings of the quiet-window requirement are allowed.</summary>
        private const int MaxGoodWindowShifts = 4;

        /// <summary>
        /// Windows within which a step down still counts as undoing the step up before it.
        /// </summary>
        private const int RevertWindowLimit = 3;

        private readonly double[] _samples = new double[WindowFrames];

        private int _sampleCount;

        private int _goodWindows;

        private int _revertedStepUps;

        private int _windowsSinceStepUp = int.MaxValue;

        private bool _settling;
    }
}
