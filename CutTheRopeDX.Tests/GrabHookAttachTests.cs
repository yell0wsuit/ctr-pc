using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class GrabHookAttachTests
    {
        [Fact]
        public void ShouldAttach_TrueForRadiusHookWithNoRopeAndCandyInRange()
        {
            Assert.True(GrabHookAttach.ShouldAttach(
                radiusEnabled: true, ropeAbsent: true, candyPresent: true, inRange: true));
        }

        [Fact]
        public void ShouldAttach_FalseWhenNotARadiusHook()
        {
            Assert.False(GrabHookAttach.ShouldAttach(radiusEnabled: false, true, true, true));
        }

        [Fact]
        public void ShouldAttach_FalseWhenRopeAlreadyExists()
        {
            // one-time use: a hook that already created a rope never attaches again.
            Assert.False(GrabHookAttach.ShouldAttach(true, ropeAbsent: false, true, true));
        }

        [Fact]
        public void ShouldAttach_FalseWhenMissingOrOutOfRange()
        {
            Assert.False(GrabHookAttach.ShouldAttach(true, true, candyPresent: false, true));
            Assert.False(GrabHookAttach.ShouldAttach(true, true, true, inRange: false));
        }
    }
}
