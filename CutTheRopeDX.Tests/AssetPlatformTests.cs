using CutTheRopeDX.Framework.Platform;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public sealed class AssetPlatformTests
    {
        [Fact]
        public void Default_IsDesktop()
        {
            // Current cannot be asserted here: HeadlessGame.Boot swaps it process-wide and the
            // engine's one-shot statics make that irreversible, so the default is pinned instead.
            _ = Assert.IsType<DesktopAssetPlatform>(AssetPlatform.Default);
        }
    }
}
