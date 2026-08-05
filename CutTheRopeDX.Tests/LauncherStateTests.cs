using System;
using System.IO;

using CutTheRopeDX.Launcher;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Redirects the record into a temporary directory for the lifetime of the class, so the tests neither
    /// write into the profile of whoever runs them nor read state left by a real install of the game.
    /// </summary>
    public sealed class LauncherStateTests : IDisposable
    {
        private readonly string _directory =
            Path.Combine(Path.GetTempPath(), "ctrdx-launcher-state-" + Guid.NewGuid().ToString("N"));

        public LauncherStateTests()
        {
            LauncherState.OverrideDirectory = _directory;
        }

        public void Dispose()
        {
            LauncherState.OverrideDirectory = null;
            try
            {
                if (Directory.Exists(_directory))
                {
                    Directory.Delete(_directory, recursive: true);
                }
            }
            catch (IOException)
            {
            }
        }

        [Fact]
        public void TheProbingMarkerMeansTheLastProbeWasFatal()
        {
            // The only way that marker survives to the next launch is a probe that never returned: a driver
            // faulting inside vkCreateInstance takes the process down, and no catch block sees it.
            Assert.True(LauncherState.ProbeWasFatal(LauncherState.ProbingMarker));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("Vulkan")]
        [InlineData("OpenGl")]
        [InlineData("something else")]
        public void AnythingElseMeansTheLastProbeCompleted(string recorded)
        {
            Assert.False(LauncherState.ProbeWasFatal(recorded));
        }

        [Fact]
        public void ARecordedBackendIsReadBack()
        {
            Assert.Equal(GraphicsBackend.Vulkan, LauncherState.LastBackend("Vulkan"));
            Assert.Equal(GraphicsBackend.OpenGl, LauncherState.LastBackend("OpenGl"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("probing")]
        [InlineData("garbage")]
        public void AnUnusableRecordReadsAsNoBackend(string recorded)
        {
            // The probing marker deliberately does not parse as a backend, so the launch recovering from a
            // fatal probe reports no last-seen value and the player still gets told why they are on OpenGL.
            Assert.Null(LauncherState.LastBackend(recorded));
        }

        [Fact]
        public void TheProbingMarkerIsNotABackendName()
        {
            // Guards the overlap the two readings depend on staying apart.
            Assert.Null(LauncherState.LastBackend(LauncherState.ProbingMarker));
        }

        [Fact]
        public void TheRecordSurvivesAWriteAndReadCycle()
        {
            LauncherState.WriteBackend(GraphicsBackend.OpenGl);
            Assert.Equal(GraphicsBackend.OpenGl, LauncherState.LastBackend(LauncherState.Read()));
            Assert.False(LauncherState.ProbeWasFatal(LauncherState.Read()));

            LauncherState.WriteProbing();
            Assert.True(LauncherState.ProbeWasFatal(LauncherState.Read()));
            Assert.Null(LauncherState.LastBackend(LauncherState.Read()));

            LauncherState.WriteBackend(GraphicsBackend.Vulkan);
            Assert.Equal(GraphicsBackend.Vulkan, LauncherState.LastBackend(LauncherState.Read()));
        }

        [Fact]
        public void ClearingLeavesNothingBehind()
        {
            // Matters most after a forced backend: a leftover probing marker would cost the next unforced
            // launch its probe, pinning the machine to OpenGL for no reason.
            LauncherState.WriteProbing();
            LauncherState.Clear();

            Assert.Null(LauncherState.Read());
            Assert.False(LauncherState.ProbeWasFatal(LauncherState.Read()));
        }

        [Fact]
        public void StoreOperationsNeverThrow()
        {
            // Every one of these runs before the game starts; none may be the reason it does not.
            LauncherState.WriteProbing();
            _ = LauncherState.Read();
            LauncherState.WriteBackend(GraphicsBackend.Vulkan);
            LauncherState.Clear();
            LauncherState.Clear();
        }
    }
}
