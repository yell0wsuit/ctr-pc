namespace CutTheRopeDX.Framework.Platform
{
    /// <summary>
    /// The platform seam registry. The desktop host populates these at boot;
    /// headless (and later web) installs only what exists there. Optional
    /// services stay null and call sites use "?.".
    /// </summary>
    internal static class PlatformServices
    {
        public static IRichPresence RichPresence { get; set; }
        public static IUpdateService Updates { get; set; }
        public static ICursorService Cursor { get; set; }
        public static IHostApp Host { get; set; }
        public static IWindowService Window { get; set; }
        public static IRenderBackend Render { get; set; }
    }
}
