using CutTheRopeDX.Desktop.Graphics;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class GraphicsBackendSelectorTests
    {
        [Fact]
        public void DecideFromStoredHardwareUsesHardwareAndStaysSilent()
        {
            GraphicsDecision decision = GraphicsBackendSelector.DecideFromStored(GraphicsMode.Hardware);

            Assert.False(decision.NeedsProbe);
            Assert.False(decision.UseSoftware);
            Assert.False(decision.ShowNotice);
            Assert.Null(decision.ModeToPersist);
        }

        [Fact]
        public void DecideFromStoredSoftwareUsesSoftwareWithoutRepeatingTheNotice()
        {
            GraphicsDecision decision = GraphicsBackendSelector.DecideFromStored(GraphicsMode.Software);

            Assert.False(decision.NeedsProbe);
            Assert.True(decision.UseSoftware);
            Assert.False(decision.ShowNotice);
            Assert.Null(decision.ModeToPersist);
        }

        [Fact]
        public void DecideFromStoredProbingRecoversToSoftwareAndWarns()
        {
            // A stored "probing" means the previous launch died inside the probe.
            GraphicsDecision decision = GraphicsBackendSelector.DecideFromStored(GraphicsMode.Probing);

            Assert.False(decision.NeedsProbe);
            Assert.True(decision.UseSoftware);
            Assert.True(decision.ShowNotice);
            Assert.Equal(GraphicsMode.Software, decision.ModeToPersist);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("HARDWARE")]
        [InlineData("banana")]
        public void DecideFromStoredAbsentOrUnrecognisedRequestsProbe(string stored)
        {
            GraphicsDecision decision = GraphicsBackendSelector.DecideFromStored(stored);

            Assert.True(decision.NeedsProbe);
            Assert.False(decision.ShowNotice);
            Assert.Null(decision.ModeToPersist);
        }

        [Fact]
        public void DecideFromProbeHardwarePersistsHardwareAndStaysSilent()
        {
            GraphicsDecision decision = GraphicsBackendSelector.DecideFromProbe(VulkanProbeResult.Hardware);

            Assert.False(decision.NeedsProbe);
            Assert.False(decision.UseSoftware);
            Assert.False(decision.ShowNotice);
            Assert.Equal(GraphicsMode.Hardware, decision.ModeToPersist);
        }

        [Fact]
        public void DecideFromProbeNoDevicePersistsSoftwareAndWarns()
        {
            AssertFallsBackToSoftware(VulkanProbeResult.NoDevice);
        }

        [Fact]
        public void DecideFromProbeNoLoaderPersistsSoftwareAndWarns()
        {
            AssertFallsBackToSoftware(VulkanProbeResult.NoLoader);
        }

        // Not a [Theory]: VulkanProbeResult is internal, so it cannot appear in the signature
        // of a public test method. A private helper sidesteps that without widening the enum.
        private static void AssertFallsBackToSoftware(VulkanProbeResult result)
        {
            GraphicsDecision decision = GraphicsBackendSelector.DecideFromProbe(result);

            Assert.False(decision.NeedsProbe);
            Assert.True(decision.UseSoftware);
            Assert.True(decision.ShowNotice);
            Assert.Equal(GraphicsMode.Software, decision.ModeToPersist);
        }
    }
}
