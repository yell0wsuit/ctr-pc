using CutTheRopeDX.Launcher;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class BackendNoticeTests
    {
        [Fact]
        public void TheFirstFallbackToOpenGlIsAnnounced()
        {
            // Nothing recorded yet: this machine has not been told why it is not on Vulkan.
            Assert.True(BackendNotice.ShouldWarn(GraphicsBackend.OpenGl, lastSeen: null, wasForced: false));
        }

        [Fact]
        public void RepeatedFallbacksAreNotAnnouncedAgain()
        {
            // Warning every launch would train the player to dismiss the dialog unread.
            Assert.False(BackendNotice.ShouldWarn(GraphicsBackend.OpenGl, GraphicsBackend.OpenGl, wasForced: false));
        }

        [Fact]
        public void FallingBackAfterVulkanPreviouslyWorkedIsAnnounced()
        {
            // Something changed on the machine, which is exactly the case worth reporting: a driver that
            // used to work no longer does, and that is usually fixable.
            Assert.True(BackendNotice.ShouldWarn(GraphicsBackend.OpenGl, GraphicsBackend.Vulkan, wasForced: false));
        }

        [Fact]
        public void RunningOnVulkanIsNeverAnnounced()
        {
            Assert.False(BackendNotice.ShouldWarn(GraphicsBackend.Vulkan, null, wasForced: false));
            Assert.False(BackendNotice.ShouldWarn(GraphicsBackend.Vulkan, GraphicsBackend.OpenGl, wasForced: false));
            Assert.False(BackendNotice.ShouldWarn(GraphicsBackend.Vulkan, GraphicsBackend.Vulkan, wasForced: false));
        }

        [Fact]
        public void AnExplicitlyChosenBackendIsNeverAnnounced()
        {
            // Someone who passed --gl or set the environment variable already knows what they asked for.
            Assert.False(BackendNotice.ShouldWarn(GraphicsBackend.OpenGl, null, wasForced: true));
            Assert.False(BackendNotice.ShouldWarn(GraphicsBackend.OpenGl, GraphicsBackend.Vulkan, wasForced: true));
        }

        [Fact]
        public void TheLaunchRecoveringFromAFatalProbeIsWarned()
        {
            // That launch left the probing marker behind, which does not parse as a backend, so it arrives
            // here as no last-seen value. The player has not been told anything yet and should be.
            Assert.True(BackendNotice.ShouldWarn(
                GraphicsBackend.OpenGl,
                LauncherState.LastBackend(LauncherState.ProbingMarker),
                wasForced: false));
        }
    }
}
