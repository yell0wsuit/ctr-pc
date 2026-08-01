namespace CutTheRopeDX.Framework.Platform
{
    /// <summary>
    /// Selects the active asset platform. Defaults to desktop so the shipping game is unchanged;
    /// the headless host swaps it during bootstrap.
    /// </summary>
    internal static class AssetPlatform
    {
        /// <summary>
        /// Gets the platform the shipping desktop game runs on, and the value
        /// <see cref="Current"/> starts at. Named separately so the default stays observable
        /// after a headless run has swapped <see cref="Current"/> for the rest of the process.
        /// </summary>
        public static IAssetPlatform Default { get; } = new DesktopAssetPlatform();

        /// <summary>Gets or sets the active platform. Set once during bootstrap, before any asset load.</summary>
        public static IAssetPlatform Current { get; set; } = Default;
    }
}
