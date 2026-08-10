using CutTheRopeDX.Framework.Platform;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public sealed class AssetPlatformTests
    {
        [Fact]
        public void DefaultIsDeviceIndependentPlaceholder()
        {
            // Current cannot be asserted here: HeadlessGame.Boot swaps it process-wide and the
            // engine's one-shot statics make that irreversible, so the default is pinned instead.
            // CtrBootstrap.Initialize always overwrites Current with a concrete platform before any
            // asset load, so Default only needs to be a safe placeholder, not the desktop platform.
            _ = Assert.IsType<HeadlessAssetPlatform>(AssetPlatform.Default);
        }
    }
}
