using CutTheRopeDX.Framework.Platform;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public sealed class AssetPlatformTests
    {
        [Fact]
        public void Current_DefaultsToDesktop()
        {
            Assert.IsType<DesktopAssetPlatform>(AssetPlatform.Current);
        }
    }
}
