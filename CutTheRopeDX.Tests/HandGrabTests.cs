using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class HandGrabTests
    {
        [Fact]
        public void ShouldGrabTrueForIdleHandNearFreeCandy()
        {
            Assert.True(HandGrab.ShouldGrab(
                handIdle: true, candyPresent: true, candyInLantern: false,
                candyInSock: false, inRange: true));
        }

        [Fact]
        public void ShouldGrabFalseWhenHandBusy()
        {
            Assert.False(HandGrab.ShouldGrab(handIdle: false, true, false, false, true));
        }

        [Fact]
        public void ShouldGrabFalseWhenCandyInLanternOrSock()
        {
            Assert.False(HandGrab.ShouldGrab(true, true, candyInLantern: true, false, true));
            Assert.False(HandGrab.ShouldGrab(true, true, false, candyInSock: true, true));
        }

        [Fact]
        public void ShouldGrabFalseWhenMissingOrOutOfRange()
        {
            Assert.False(HandGrab.ShouldGrab(true, candyPresent: false, false, false, true));
            Assert.False(HandGrab.ShouldGrab(true, true, false, false, inRange: false));
        }

        [Fact]
        public void ShouldGrabFalseForACandyInSockTransit()
        {
            // The explicit sock parameter alone refuses the grab, independently of how the caller
            // computes candyPresent.
            Assert.False(HandGrab.ShouldGrab(
                handIdle: true, candyPresent: true, candyInLantern: false, candyInSock: true, inRange: true));
        }

        [Fact]
        public void ShouldGrabFalseForACandyInBambooTransit()
        {
            // Bamboo transit hides the whole body, so the caller passes candyPresent: false. If
            // bamboo ever stops hiding it, this pin documents that the hand gate must gain a
            // bamboo parameter of its own.
            Assert.False(HandGrab.ShouldGrab(
                handIdle: true, candyPresent: false, candyInLantern: false, candyInSock: false, inRange: true));
        }
    }
}
