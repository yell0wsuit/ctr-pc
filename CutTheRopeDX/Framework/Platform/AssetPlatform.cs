namespace CutTheRopeDX.Framework.Platform
{
    /// <summary>
    /// Selects the active asset platform. Defaults to desktop so the shipping game is unchanged;
    /// the headless host swaps it during bootstrap.
    /// </summary>
    internal static class AssetPlatform
    {
        /// <summary>Gets or sets the active platform. Set once during bootstrap, before any asset load.</summary>
        public static IAssetPlatform Current { get; set; } = new DesktopAssetPlatform();
    }
}
