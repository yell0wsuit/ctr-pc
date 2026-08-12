using System;
using System.IO;

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
        public void EachBackendHasItsOwnExecutableName()
        {
            // Both builds share one directory, so the names are the only thing telling them apart.
            Assert.NotEqual(
                BackendSelection.ExecutableFor(GraphicsBackend.Vulkan),
                BackendSelection.ExecutableFor(GraphicsBackend.OpenGl));
        }

        [Fact]
        public void CandidatesCoverBothExtensionlessAndWindowsNames()
        {
            // Only Windows ships the launcher and appends .exe, but the same dispatch is exercised on the
            // platform it is developed on, where the published builds carry no extension.
            string[] candidates = BackendSelection.CandidatePaths("/base", GraphicsBackend.Vulkan);

            Assert.Contains(candidates, p => p.EndsWith(BackendSelection.VulkanExecutable + ".exe", StringComparison.Ordinal));
            Assert.Contains(candidates, p => p.EndsWith(BackendSelection.VulkanExecutable, StringComparison.Ordinal));
        }

        [Fact]
        public void EveryCandidateSitsBesideTheLauncher()
        {
            // Both builds share the launcher's directory, so nothing here should be reaching into a
            // subdirectory or above itself for them.
            foreach (string candidate in BackendSelection.CandidatePaths("/base", GraphicsBackend.OpenGl))
            {
                Assert.Equal("/base", Path.GetDirectoryName(candidate));
            }
        }

        [Fact]
        public void TheFirstFallbackToOpenGlIsAnnounced()
        {
            // Nothing recorded yet: this machine has not been told why it is not on Vulkan.
            Assert.True(BackendSelection.ShouldWarn(GraphicsBackend.OpenGl, lastSeen: null, wasForced: false));
        }

        [Fact]
        public void RepeatedFallbacksAreNotAnnouncedAgain()
        {
            // Warning every launch would train the player to dismiss the dialog unread.
            Assert.False(BackendSelection.ShouldWarn(GraphicsBackend.OpenGl, GraphicsBackend.OpenGl, wasForced: false));
        }

        [Fact]
        public void FallingBackAfterVulkanPreviouslyWorkedIsAnnounced()
        {
            // Something changed on the machine, which is exactly the case worth reporting: a driver that
            // used to work no longer does, and that is usually fixable.
            Assert.True(BackendSelection.ShouldWarn(GraphicsBackend.OpenGl, GraphicsBackend.Vulkan, wasForced: false));
        }

        [Fact]
        public void RunningOnVulkanIsNeverAnnounced()
        {
            Assert.False(BackendSelection.ShouldWarn(GraphicsBackend.Vulkan, null, wasForced: false));
            Assert.False(BackendSelection.ShouldWarn(GraphicsBackend.Vulkan, GraphicsBackend.OpenGl, wasForced: false));
            Assert.False(BackendSelection.ShouldWarn(GraphicsBackend.Vulkan, GraphicsBackend.Vulkan, wasForced: false));
        }

        [Fact]
        public void AnExplicitlyChosenBackendIsNeverAnnounced()
        {
            // Someone who passed --gl or set the environment variable already knows what they asked for.
            Assert.False(BackendSelection.ShouldWarn(GraphicsBackend.OpenGl, null, wasForced: true));
            Assert.False(BackendSelection.ShouldWarn(GraphicsBackend.OpenGl, GraphicsBackend.Vulkan, wasForced: true));
        }

        [Fact]
        public void TheLaunchRecoveringFromAFatalProbeIsWarned()
        {
            // That launch left the probing marker behind, which does not parse as a backend, so it arrives
            // here as no last-seen value. The player has not been told anything yet and should be.
            Assert.True(BackendSelection.ShouldWarn(
                GraphicsBackend.OpenGl,
                LauncherState.LastBackend(LauncherState.ProbingMarker),
                wasForced: false));
        }
    }
}
