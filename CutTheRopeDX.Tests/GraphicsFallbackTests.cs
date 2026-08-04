using System.Collections.Generic;

using CutTheRopeDX.Desktop.Graphics;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class GraphicsFallbackTests
    {
        /// <summary>Records every side effect in order so sequencing can be asserted.</summary>
        private sealed class Recorder
        {
            public List<string> Calls { get; } = [];

            public string StoredMode { get; set; } = "";

            public VulkanProbeResult ProbeResult { get; set; } = VulkanProbeResult.Hardware;

            public void Run()
            {
                GraphicsFallback.Run(
                    () => StoredMode,
                    mode => { StoredMode = mode; Calls.Add($"write:{mode}"); },
                    () => { Calls.Add("probe"); return ProbeResult; },
                    () => Calls.Add("notice"),
                    () => Calls.Add("software"));
            }
        }

        [Fact]
        public void RunStoredHardwareDoesNothingAtAll()
        {
            Recorder recorder = new() { StoredMode = GraphicsMode.Hardware };

            recorder.Run();

            Assert.Empty(recorder.Calls);
        }

        [Fact]
        public void RunStoredSoftwareAppliesSoftwareWithoutProbingOrWarning()
        {
            Recorder recorder = new() { StoredMode = GraphicsMode.Software };

            recorder.Run();

            Assert.Equal(["software"], recorder.Calls);
        }

        [Fact]
        public void RunNoStoredModeWritesMarkerBeforeProbing()
        {
            // This ordering is the entire point of the marker: if the probe kills the process,
            // "probing" must already be on disk.
            Recorder recorder = new() { StoredMode = "", ProbeResult = VulkanProbeResult.Hardware };

            recorder.Run();

            Assert.Equal([$"write:{GraphicsMode.Probing}", "probe", $"write:{GraphicsMode.Hardware}"], recorder.Calls);
        }

        [Fact]
        public void RunProbeFindsNoDevicePersistsWarnsThenApplies()
        {
            Recorder recorder = new() { StoredMode = "", ProbeResult = VulkanProbeResult.NoDevice };

            recorder.Run();

            Assert.Equal(
                [$"write:{GraphicsMode.Probing}", "probe", $"write:{GraphicsMode.Software}", "notice", "software"],
                recorder.Calls);
        }

        [Fact]
        public void RunStoredProbingRecoversToSoftwareWithoutProbingAgain()
        {
            Recorder recorder = new() { StoredMode = GraphicsMode.Probing };

            recorder.Run();

            Assert.Equal([$"write:{GraphicsMode.Software}", "notice", "software"], recorder.Calls);
            Assert.DoesNotContain("probe", recorder.Calls);
        }

        [Fact]
        public void RunSecondLaunchAfterSoftwareWasStoredIsSilent()
        {
            // First launch probes, warns, and stores "software".
            Recorder recorder = new() { StoredMode = "", ProbeResult = VulkanProbeResult.NoLoader };
            recorder.Run();
            Assert.Contains("notice", recorder.Calls);

            // Second launch reuses the stored answer and must not warn again.
            recorder.Calls.Clear();
            recorder.Run();

            Assert.Equal(["software"], recorder.Calls);
        }
    }
}
