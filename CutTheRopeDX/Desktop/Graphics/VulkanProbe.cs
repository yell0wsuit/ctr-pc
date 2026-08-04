namespace CutTheRopeDX.Desktop.Graphics
{
    /// <summary>
    /// Outcome of checking the machine for a usable hardware Vulkan driver.
    /// </summary>
    internal enum VulkanProbeResult
    {
        /// <summary>A Vulkan device with a graphics queue and the required surface extensions was found.</summary>
        Hardware,

        /// <summary>The Vulkan loader is present but exposes no usable device.</summary>
        NoDevice,

        /// <summary>The Vulkan loader itself is missing or unusable.</summary>
        NoLoader,
    }
}
