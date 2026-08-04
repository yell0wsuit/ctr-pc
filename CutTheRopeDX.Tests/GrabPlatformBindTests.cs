using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Platforms (conveyor, DJ disc) never move a grab that has its own movement: bee/launcher
    /// path movers and player drag rails are excluded at bind time; a kicked suction cup is a
    /// free physics body, excluded dynamically per frame and resumed on re-stick.
    /// </summary>
    public class GrabPlatformBindTests
    {
        [Fact]
        public void CanBindTrueForAPlainHook()
        {
            Assert.True(GrabPlatformBind.CanBind(hasOwnMover: false, isMoveableRail: false));
        }

        [Fact]
        public void CanBindFalseForAPathMoverGrab()
        {
            // Bee and launcher keep their own movement; the platform must not fight it.
            Assert.False(GrabPlatformBind.CanBind(hasOwnMover: true, isMoveableRail: false));
        }

        [Fact]
        public void CanBindFalseForAMoveableRailGrab()
        {
            Assert.False(GrabPlatformBind.CanBind(hasOwnMover: false, isMoveableRail: true));
        }

        [Fact]
        public void FollowsPlatformTrueForABoundStuckGrab()
        {
            Assert.True(GrabPlatformBind.FollowsPlatform(canBind: true, isKickedFree: false));
        }

        [Fact]
        public void FollowsPlatformFalseWhileKickedFree()
        {
            // Kicked suction cup falls freely; the platform skips it until it re-sticks.
            Assert.False(GrabPlatformBind.FollowsPlatform(canBind: true, isKickedFree: true));
        }

        [Fact]
        public void FollowsPlatformFalseWhenNotBindableAtAll()
        {
            Assert.False(GrabPlatformBind.FollowsPlatform(canBind: false, isKickedFree: false));
        }
    }
}
