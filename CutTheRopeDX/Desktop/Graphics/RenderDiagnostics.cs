using System.Globalization;

namespace CutTheRopeDX.Desktop.Graphics
{
    /// <summary>
    /// On-screen readout of what the frame is costing and what the renderer did about it.
    /// </summary>
    /// <remarks>
    /// The frame rate on its own does not distinguish a machine that is short of fill rate from one that is
    /// short of everything, and the adaptive divisor is invisible from the picture once it settles. Reporting
    /// the frame time and the divisor together is what makes a single run on an unfamiliar machine say
    /// something useful: a low divisor with a slow frame means the scene is not where the time goes, and the
    /// coarsest divisor with a slow frame means the blit and the CPU floor are already over budget.
    /// </remarks>
    internal static class RenderDiagnostics
    {
        /// <summary>Gets whether the readout is currently drawn.</summary>
        public static bool Enabled { get; private set; }

        /// <summary>Shows or hides the readout.</summary>
        public static void Toggle()
        {
            Enabled = !Enabled;
        }

        /// <summary>
        /// Builds the readout line.
        /// </summary>
        /// <param name="fps">Frames per second the runtime measured.</param>
        /// <param name="medianMs">Median per-frame work of the last completed window, in milliseconds.</param>
        /// <param name="step">Rung of the render-line ladder currently in use.</param>
        /// <param name="width">Current scene render width.</param>
        /// <param name="height">Current scene render height.</param>
        /// <param name="softwareRendering">Whether rendering goes through the bundled software library.</param>
        /// <returns>A single line short enough to sit in a corner without covering the game.</returns>
        /// <remarks>
        /// The step is reported next to the size it produced, because the size alone does not say whether
        /// the policy has backed off or is simply on a small display. Both are omitted on the hardware path,
        /// along with the frame time, where nothing measures the one or moves the other; showing zeroes
        /// there would look like a broken reading.
        /// </remarks>
        public static string Format(int fps, double medianMs, int step, int width, int height, bool softwareRendering)
        {
            string size = string.Create(CultureInfo.InvariantCulture, $"{width}x{height}");
            return !softwareRendering
                ? string.Create(CultureInfo.InvariantCulture, $"{fps}fps {size} hw")
                : string.Create(CultureInfo.InvariantCulture, $"{fps}fps {medianMs:F1}ms s{step} {size} sw");
        }
    }
}
