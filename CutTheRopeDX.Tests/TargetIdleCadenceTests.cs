using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class TargetIdleCadenceTests
    {
        [Fact]
        public void IdleLoopDrivesCadence()
        {
            Assert.True(TargetIdleCadence.DrivesIdleCadence(driverTimelineId: 0, idleLoopTimelineId: 0));
        }

        [Fact]
        public void ChewingDoesNotDriveCadence()
        {
            // Chewing (ID 7) loops and emits a keyframe at index 1, but must not advance
            // the idle cadence; otherwise a random idle variant interrupts the chew.
            Assert.False(TargetIdleCadence.DrivesIdleCadence(driverTimelineId: 7, idleLoopTimelineId: 0));
        }

        [Fact]
        public void NoDriverDoesNotDriveCadence()
        {
            Assert.False(TargetIdleCadence.DrivesIdleCadence(driverTimelineId: -1, idleLoopTimelineId: 0));
        }
    }
}
