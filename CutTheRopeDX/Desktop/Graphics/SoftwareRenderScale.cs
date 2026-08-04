using System;

namespace CutTheRopeDX.Desktop.Graphics
{
    /// <summary>
    /// Picks how far below the on-screen size the scene is rendered when the bundled software renderer is
    /// in use, from how long recent frames took.
    /// </summary>
    /// <remarks>
    /// The scene is drawn into a render target that is then stretched over the on-screen rectangle, so
    /// dividing that target's size cuts scene fill-rate quadratically without moving anything in game
    /// coordinates. The blit that follows is not affected: it always covers the whole back buffer. Neither
    /// is the per-frame CPU work. Those two are the floor this cannot get under, which is why the overlay
    /// reports the divisor alongside the frame time rather than only the frame rate.
    /// <para>
    /// The ladder is whole divisors rather than percentages because
    /// <c>Renderer.ScreenBlitSamplerFor</c> point samples the blit only when the upscale repeats whole
    /// pixels. A fractional scale would drop it to bilinear, which costs four texel fetches per back-buffer
    /// pixel over a surface far larger than the saving, and shimmers on the ropes while they move.
    /// </para>
    /// <para>
    /// Timing is fed in rather than measured here so the policy can be tested without a graphics device.
    /// </para>
    /// </remarks>
    internal sealed class SoftwareRenderScale
    {
        /// <summary>Divisor used when the scene renders at its full on-screen size.</summary>
        public const int MinDivisor = 1;

        /// <summary>
        /// Scene height the base divisor aims at or below, in lines.
        /// </summary>
        /// <remarks>
        /// Roughly where the software renderer stops being fill-rate bound on a weak dual-core machine.
        /// It is a target rather than a size, because the divisor that reaches it has to be a whole number
        /// and few displays divide evenly into it: a 768-line display gets 384 and a 1440-line one gets 480.
        /// Missing low is deliberate, since the displays that miss are the ones a fractional blit would cost
        /// the most on.
        /// </remarks>
        public const int TargetRenderLines = 540;

        /// <summary>
        /// How much coarser than the base divisor the policy may go when frames still overrun.
        /// </summary>
        public const int MaxAdaptiveSteps = 2;

        /// <summary>Frames gathered before the divisor is reconsidered.</summary>
        public const int WindowFrames = 30;

        /// <summary>
        /// Frame-time median above which the divisor grows. Under the 16.6ms budget a 60Hz frame has, with
        /// room left for the present the measurement excludes.
        /// </summary>
        public const double StepDownAboveMs = 13.0;

        /// <summary>
        /// Frame-time median below which stepping back up is considered. Well under half the budget,
        /// because undoing a divisor multiplies scene fill by more than two and the frame has to survive it.
        /// </summary>
        public const double StepUpBelowMs = 7.0;

        /// <summary>Consecutive quiet windows required before the first step up.</summary>
        public const int BaseGoodWindowsForStepUp = 10;

        /// <summary>
        /// Shared instance the render path reads. The policy is per-process because the render target it
        /// governs is.
        /// </summary>
        public static SoftwareRenderScale Shared { get; } = new();

        /// <summary>Gets the divisor currently applied to the on-screen size.</summary>
        public int Divisor { get; private set; } = MinDivisor;

        /// <summary>
        /// Gets the divisor the display size alone calls for, which is also the finest the policy may
        /// return to.
        /// </summary>
        public int BaseDivisor { get; private set; } = MinDivisor;

        /// <summary>Coarsest divisor currently allowed.</summary>
        public int MaxDivisor => BaseDivisor + MaxAdaptiveSteps;

        /// <summary>
        /// Smallest whole divisor that brings an on-screen height to <see cref="TargetRenderLines"/> or
        /// below.
        /// </summary>
        /// <param name="onScreenHeight">Height the finished frame is shown at, in pixels.</param>
        /// <returns>The divisor, never below <see cref="MinDivisor"/>.</returns>
        /// <remarks>
        /// A whole divisor is what keeps the blit to the back buffer on the point-sampled path in
        /// <c>Renderer.ScreenBlitSamplerFor</c>, which matters more than hitting the target exactly: the
        /// blit covers the whole back buffer and never gets cheaper, so paying four texel fetches per pixel
        /// there can cost more than the scene saved by rendering at a size nearer the target.
        /// </remarks>
        public static int BaseDivisorFor(int onScreenHeight)
        {
            if (onScreenHeight <= TargetRenderLines)
            {
                return MinDivisor;
            }
            // Round up, so the result is at or under the target rather than straddling it.
            return (onScreenHeight + TargetRenderLines - 1) / TargetRenderLines;
        }

        /// <summary>
        /// Sets the divisor the current display size calls for, carrying any adaptive steps already taken.
        /// </summary>
        /// <param name="baseDivisor">Divisor from <see cref="BaseDivisorFor"/>.</param>
        /// <remarks>
        /// Called whenever the view is resized. The steps the policy had taken describe the machine rather
        /// than the window, so they are kept across the change; the measurements behind them are not, since
        /// they were taken at a size that no longer applies.
        /// </remarks>
        public void SetBaseDivisor(int baseDivisor)
        {
            baseDivisor = Math.Max(MinDivisor, baseDivisor);
            if (baseDivisor == BaseDivisor)
            {
                return;
            }
            int steps = Math.Clamp(Divisor - BaseDivisor, 0, MaxAdaptiveSteps);
            BaseDivisor = baseDivisor;
            Divisor = baseDivisor + steps;
            Reset();
            _settling = true;
        }

        /// <summary>Gets the median frame time of the last completed window, in milliseconds.</summary>
        public double LastMedianMs { get; private set; }

        /// <summary>
        /// Divides an on-screen size by <paramref name="divisor"/>, never yielding a degenerate size.
        /// </summary>
        /// <param name="width">Width the finished frame is shown at.</param>
        /// <param name="height">Height the finished frame is shown at.</param>
        /// <param name="divisor">Divisor to apply.</param>
        /// <returns>The size to render the scene at.</returns>
        public static (int Width, int Height) Apply(int width, int height, int divisor)
        {
            if (divisor <= MinDivisor)
            {
                return (width, height);
            }
            return (Math.Max(1, width / divisor), Math.Max(1, height / divisor));
        }

        /// <summary>
        /// Records how long one frame's work took and reconsiders the divisor once a window is full.
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

            // The window straddling a divisor change contains frames drawn at both sizes, so it measures
            // neither. Spend it settling.
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

            if (median < StepUpBelowMs && Divisor > BaseDivisor)
            {
                if (++_goodWindows >= RequiredGoodWindows)
                {
                    StepUp();
                }
                return;
            }

            // Anything between the two thresholds is the frame rate the divisor was chosen for. Treat the
            // quiet streak as broken so a step up needs a fresh run of genuinely cheap windows.
            _goodWindows = 0;
        }

        /// <summary>
        /// Drops the partial window and the quiet streak, keeping the current divisor.
        /// </summary>
        /// <remarks>
        /// For stretches whose frame times describe something other than the scene, such as a movie or a
        /// level load. Feeding those to the policy would move the divisor for the frames either side of
        /// them, which are the ones that have to look right.
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
        /// sitting near the step-up threshold makes the divisor oscillate, and a resolution that visibly
        /// pops back and forth is worse to look at than one that settled a step too coarse.
        /// </remarks>
        private int RequiredGoodWindows => BaseGoodWindowsForStepUp << Math.Min(_revertedStepUps, MaxGoodWindowShifts);

        private void StepDown()
        {
            _goodWindows = 0;
            if (Divisor >= MaxDivisor)
            {
                return;
            }
            // A step down arriving on the heels of a step up means that step up was a mistake.
            if (_windowsSinceStepUp <= RevertWindowLimit)
            {
                _revertedStepUps++;
            }
            Divisor++;
            _settling = true;
        }

        private void StepUp()
        {
            Divisor--;
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
