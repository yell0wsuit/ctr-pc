namespace CutTheRopeDX.Desktop.Graphics
{
    /// <summary>
    /// Names of the rendering backends the game can select, and the preference key they are stored under.
    /// </summary>
    internal static class GraphicsMode
    {
        /// <summary>Global preference key holding the resolved rendering backend.</summary>
        public const string PreferenceKey = "PREFS_GRAPHICS_MODE";

        /// <summary>A probe was started and has not reported back; the launch that started it died.</summary>
        public const string Probing = "probing";

        /// <summary>The system Vulkan driver is usable.</summary>
        public const string Hardware = "hardware";

        /// <summary>No usable Vulkan driver; the bundled SwiftShader library is used instead.</summary>
        public const string Software = "software";
    }
}
