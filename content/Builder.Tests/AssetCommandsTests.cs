using System.Reflection;

using CutTheRopeDX.Content.Commands;

using Xunit;

namespace CutTheRopeDX.Content.Tests
{
    public sealed class AssetCommandsTests
    {
        [Fact]
        public void DesktopVkFetchUsesDedicatedReleaseArchive()
        {
            FieldInfo? assetsUrlField = typeof(AssetCommands).GetField(
                "AssetsUrl",
                BindingFlags.NonPublic | BindingFlags.Static);

            string assetsUrl = Assert.IsType<string>(
                assetsUrlField?.GetRawConstantValue());

            Assert.EndsWith("/ctrdx-assets-vk.zip", assetsUrl, StringComparison.Ordinal);
        }
    }
}
