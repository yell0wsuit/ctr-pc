using System;

namespace CutTheRopeDX.Desktop.Graphics
{
    /// <summary>
    /// Decides which rendering backend to use. Pure logic with no I/O, so it is fully unit-testable.
    /// </summary>
    /// <remarks>
    /// The decision is split in two because the caller must persist the <see cref="GraphicsMode.Probing"/>
    /// marker to disk between the two halves. See <see cref="GraphicsFallback"/> for the sequencing.
    /// </remarks>
    internal static class GraphicsBackendSelector
    {
        /// <summary>
        /// Decides what to do based on the previously stored mode, if any.
        /// </summary>
        /// <param name="storedMode">Value read from <see cref="GraphicsMode.PreferenceKey"/>; may be empty or <see langword="null"/>.</param>
        /// <returns>The decision. <see cref="GraphicsDecision.NeedsProbe"/> is set when no usable answer was stored.</returns>
        public static GraphicsDecision DecideFromStored(string storedMode)
        {
            if (string.Equals(storedMode, GraphicsMode.Hardware, StringComparison.Ordinal))
            {
                return new GraphicsDecision(false, false, false, null);
            }

            if (string.Equals(storedMode, GraphicsMode.Software, StringComparison.Ordinal))
            {
                return new GraphicsDecision(false, true, false, null);
            }

            // The marker survived from a previous launch, so that launch died inside the probe.
            // Assume the driver is at fault and go straight to software without probing again.
            return string.Equals(storedMode, GraphicsMode.Probing, StringComparison.Ordinal)
                ? new GraphicsDecision(false, true, true, GraphicsMode.Software)
                : new GraphicsDecision(true, false, false, null);
        }

        /// <summary>
        /// Decides what to do based on a completed probe.
        /// </summary>
        /// <param name="result">Outcome reported by <see cref="VulkanProbe"/>.</param>
        /// <returns>The final decision, always carrying a value to persist.</returns>
        public static GraphicsDecision DecideFromProbe(VulkanProbeResult result)
        {
            return result == VulkanProbeResult.Hardware
                ? new GraphicsDecision(false, false, false, GraphicsMode.Hardware)
                : new GraphicsDecision(false, true, true, GraphicsMode.Software);
        }
    }
}
