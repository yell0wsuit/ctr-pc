using CutTheRopeDX.Launcher;
using CutTheRopeDX.Launcher.Graphics;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class BackendSelectionTests
    {
        [Fact]
        public void WindowsWithAHardwareDriverRunsTheVulkanBuild()
        {
            GraphicsBackend backend = BackendSelection.Decide(
                isWindows: true, VulkanProbeResult.Hardware, forced: null);

            Assert.Equal(GraphicsBackend.Vulkan, backend);
        }

        [Theory]
        [InlineData(VulkanProbeResult.NoDevice)]
        [InlineData(VulkanProbeResult.NoLoader)]
        public void WindowsWithoutAUsableDriverRunsTheOpenGlBuild(VulkanProbeResult probe)
        {
            // Both outcomes mean the same thing to the launcher: a machine whose Vulkan cannot draw. That
            // covers the missing loader on an inbox driver and a loader exposing nothing but software.
            GraphicsBackend backend = BackendSelection.Decide(isWindows: true, probe, forced: null);

            Assert.Equal(GraphicsBackend.OpenGl, backend);
        }

        [Theory]
        [InlineData(VulkanProbeResult.Hardware)]
        [InlineData(VulkanProbeResult.NoDevice)]
        [InlineData(VulkanProbeResult.NoLoader)]
        public void OtherPlatformsAlwaysRunTheVulkanBuild(VulkanProbeResult probe)
        {
            // Only Windows ships an OpenGL build, so redirecting to one elsewhere would point at a
            // directory that was never produced. Failing on the build that exists is the better outcome.
            GraphicsBackend backend = BackendSelection.Decide(isWindows: false, probe, forced: null);

            Assert.Equal(GraphicsBackend.Vulkan, backend);
        }

        [Fact]
        public void AForcedBackendBeatsTheProbe()
        {
            GraphicsBackend backend = BackendSelection.Decide(
                isWindows: true, VulkanProbeResult.Hardware, forced: GraphicsBackend.OpenGl);

            Assert.Equal(GraphicsBackend.OpenGl, backend);
        }

        [Fact]
        public void AForcedBackendAppliesOffWindowsToo()
        {
            GraphicsBackend backend = BackendSelection.Decide(
                isWindows: false, VulkanProbeResult.Hardware, forced: GraphicsBackend.OpenGl);

            Assert.Equal(GraphicsBackend.OpenGl, backend);
        }

        [Theory]
        [InlineData("gl")]
        [InlineData("GL")]
        [InlineData("--gl")]
        [InlineData("/gl")]
        [InlineData("opengl")]
        public void OpenGlIsNamedSeveralWays(string value)
        {
            Assert.Equal(GraphicsBackend.OpenGl, BackendSelection.ParseOverride(value));
        }

        [Theory]
        [InlineData("vk")]
        [InlineData("--vulkan")]
        [InlineData("VK")]
        public void VulkanIsNamedSeveralWays(string value)
        {
            Assert.Equal(GraphicsBackend.Vulkan, BackendSelection.ParseOverride(value));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        [InlineData("directx")]
        [InlineData("--headless")]
        public void AnythingElseNamesNoBackend(string value)
        {
            // A mistyped or stale setting has to fall through to detection rather than stop the game.
            Assert.Null(BackendSelection.ParseOverride(value));
        }

        [Fact]
        public void ArgumentsThatNameNoBackendLeaveTheChoiceToDetection()
        {
            Assert.Null(BackendSelection.ParseOverride(["--headless", "--level", "3"]));
        }

        [Fact]
        public void TheLastNamedBackendWins()
        {
            Assert.Equal(GraphicsBackend.Vulkan, BackendSelection.ParseOverride(["--gl", "--vk"]));
        }

        [Fact]
        public void EachBackendHasItsOwnDirectory()
        {
            Assert.Equal(BackendSelection.VulkanDirectory, BackendSelection.DirectoryFor(GraphicsBackend.Vulkan));
            Assert.Equal(BackendSelection.OpenGlDirectory, BackendSelection.DirectoryFor(GraphicsBackend.OpenGl));
            Assert.NotEqual(BackendSelection.VulkanDirectory, BackendSelection.OpenGlDirectory);
        }
    }
}
