using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class HandGrabTests
    {
        [Fact]
        public void ShouldGrab_TrueForIdleHandNearFreeCandy()
        {
            Assert.True(HandGrab.ShouldGrab(
                handIdle: true, candyPresent: true, candyInLantern: false,
                candyInSock: false, inRange: true));
        }

        [Fact]
        public void ShouldGrab_FalseWhenHandBusy()
        {
            Assert.False(HandGrab.ShouldGrab(handIdle: false, true, false, false, true));
        }

        [Fact]
        public void ShouldGrab_FalseWhenCandyInLanternOrSock()
        {
            Assert.False(HandGrab.ShouldGrab(true, true, candyInLantern: true, false, true));
            Assert.False(HandGrab.ShouldGrab(true, true, false, candyInSock: true, true));
        }

        [Fact]
        public void ShouldGrab_FalseWhenMissingOrOutOfRange()
        {
            Assert.False(HandGrab.ShouldGrab(true, candyPresent: false, false, false, true));
            Assert.False(HandGrab.ShouldGrab(true, true, false, false, inRange: false));
        }

        [Fact]
        public void ShouldGrab_FalseForACandyInSockTransit()
        {
            // Sock transit keeps the candy "present" (noCandy stays false), so the gate needs
            // the explicit sock parameter.
            Assert.False(HandGrab.ShouldGrab(
                handIdle: true, candyPresent: true, candyInLantern: false, candyInSock: true, inRange: true));
        }

        [Fact]
        public void ShouldGrab_FalseForACandyInBambooTransit()
        {
            // Bamboo transit sets noCandy = true (GameScene.Systems.OperateBambooTube), so the
            // candyPresent parameter is the bamboo gate. If bamboo ever stops setting noCandy,
            // this pin documents that the hand gate must gain a bamboo parameter.
            Assert.False(HandGrab.ShouldGrab(
                handIdle: true, candyPresent: false, candyInLantern: false, candyInSock: false, inRange: true));
        }
    }
}
