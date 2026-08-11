namespace CutTheRopeDX.Framework.Platform
{
    /// <summary>
    /// Selects the active asset platform. <see cref="CtrBootstrap.Initialize"/> is the
    /// single boot path both hosts use and always overwrites <see cref="Current"/> with a concrete
    /// platform (desktop or headless) before any asset load, so <see cref="Default"/> only needs to be
    /// a safe, device-independent placeholder for the window before that call.
    /// </summary>
    internal static class AssetPlatform
    {
        /// <summary>
        /// Gets the device-independent placeholder value <see cref="Current"/> starts at. Named
        /// separately so the pre-bootstrap default stays observable after a headless run has swapped
        /// <see cref="Current"/> for the rest of the process.
        /// </summary>
        public static IAssetPlatform Default { get; } = new HeadlessAssetPlatform();

        /// <summary>Gets or sets the active platform. Set once during bootstrap, before any asset load.</summary>
        public static IAssetPlatform Current { get; set; } = Default;
    }
}
