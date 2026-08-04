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

        [Fact]
        public void EachBackendHasItsOwnFlatExecutableName()
        {
            // The ahead-of-time layout puts both builds in one directory, so the names have to differ.
            Assert.NotEqual(
                BackendSelection.ExecutableFor(GraphicsBackend.Vulkan),
                BackendSelection.ExecutableFor(GraphicsBackend.OpenGl));
        }

        [Fact]
        public void TheFlatLayoutIsPreferredOverTheDirectoryLayout()
        {
            // A release ships one or the other, but a development tree can hold both. Matching the flat
            // names first means the ahead-of-time build wins, which is the one a release actually contains.
            string[] candidates = BackendSelection.CandidatePaths("/base", GraphicsBackend.OpenGl);

            int firstFlat = System.Array.FindIndex(candidates, p => p.Contains(BackendSelection.OpenGlExecutable, System.StringComparison.Ordinal));
            int firstDirectory = System.Array.FindIndex(candidates, p => p.Contains(BackendSelection.OpenGlDirectory + System.IO.Path.DirectorySeparatorChar, System.StringComparison.Ordinal));

            Assert.True(firstFlat >= 0, "no flat candidate offered");
            Assert.True(firstDirectory >= 0, "no directory candidate offered");
            Assert.True(firstFlat < firstDirectory, "the directory layout was preferred over the flat one");
        }

        [Fact]
        public void CandidatesCoverBothExtensionlessAndWindowsNames()
        {
            // One launcher build serves every platform it is shipped on, and only Windows appends .exe.
            string[] candidates = BackendSelection.CandidatePaths("/base", GraphicsBackend.Vulkan);

            Assert.Contains(candidates, p => p.EndsWith(BackendSelection.VulkanExecutable + ".exe", System.StringComparison.Ordinal));
            Assert.Contains(candidates, p => p.EndsWith(BackendSelection.VulkanExecutable, System.StringComparison.Ordinal));
        }

        [Fact]
        public void TheManagedAssemblyIsOfferedLastForNonPublishedBuilds()
        {
            // A framework-dependent build leaves a .dll with no native host beside it; running it needs
            // the dotnet muxer, so it is only worth trying once the real executables are ruled out.
            string[] candidates = BackendSelection.CandidatePaths("/base", GraphicsBackend.Vulkan);

            Assert.EndsWith(".dll", candidates[^1], System.StringComparison.Ordinal);
        }
    }
}
