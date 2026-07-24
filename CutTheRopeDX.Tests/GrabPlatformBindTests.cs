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
        public void CanBind_TrueForAPlainHook()
        {
            Assert.True(GrabPlatformBind.CanBind(hasOwnMover: false, isMoveableRail: false));
        }

        [Fact]
        public void CanBind_FalseForAPathMoverGrab()
        {
            // Bee and launcher keep their own movement; the platform must not fight it.
            Assert.False(GrabPlatformBind.CanBind(hasOwnMover: true, isMoveableRail: false));
        }

        [Fact]
        public void CanBind_FalseForAMoveableRailGrab()
        {
            Assert.False(GrabPlatformBind.CanBind(hasOwnMover: false, isMoveableRail: true));
        }

        [Fact]
        public void FollowsPlatform_TrueForABoundStuckGrab()
        {
            Assert.True(GrabPlatformBind.FollowsPlatform(canBind: true, isKickedFree: false));
        }

        [Fact]
        public void FollowsPlatform_FalseWhileKickedFree()
        {
            // Kicked suction cup falls freely; the platform skips it until it re-sticks.
            Assert.False(GrabPlatformBind.FollowsPlatform(canBind: true, isKickedFree: true));
        }

        [Fact]
        public void FollowsPlatform_FalseWhenNotBindableAtAll()
        {
            Assert.False(GrabPlatformBind.FollowsPlatform(canBind: false, isKickedFree: false));
        }
    }
}
