using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class GrabHookAttachTests
    {
        [Fact]
        public void ShouldAttachTrueForRadiusHookWithNoRopeAndCandyInRange()
        {
            Assert.True(GrabHookAttach.ShouldAttach(
                radiusEnabled: true, ropeAbsent: true, candyPresent: true, inRange: true));
        }

        [Fact]
        public void ShouldAttachFalseWhenNotARadiusHook()
        {
            Assert.False(GrabHookAttach.ShouldAttach(radiusEnabled: false, true, true, true));
        }

        [Fact]
        public void ShouldAttachFalseWhenRopeAlreadyExists()
        {
            // one-time use: a hook that already created a rope never attaches again.
            Assert.False(GrabHookAttach.ShouldAttach(true, ropeAbsent: false, true, true));
        }

        [Fact]
        public void ShouldAttachFalseWhenMissingOrOutOfRange()
        {
            Assert.False(GrabHookAttach.ShouldAttach(true, true, candyPresent: false, true));
            Assert.False(GrabHookAttach.ShouldAttach(true, true, true, inRange: false));
        }
    }
}
