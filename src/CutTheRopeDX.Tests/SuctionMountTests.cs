using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>Verifies the suction cup's kick cycle, stain supply and platform handoff.</summary>
    public class SuctionMountTests
    {
        [Fact]
        public void NewMountIsStuckToTheWallWithAFullStainSupply()
        {
            SuctionMount mount = new(startsKicked: false);

            Assert.True(mount.IsMounted);
            Assert.Equal(Grab.MAX_STAINS, mount.StainCount);
            Assert.Equal(-1f, mount.StickTimer);
        }

        [Fact]
        public void MountAuthoredAsKickedStartsDetached()
        {
            // Six Experiments maps author kicked="true".
            SuctionMount mount = new(startsKicked: true);

            Assert.False(mount.IsMounted);
        }

        [Fact]
        public void StickTimerRunsOnlyWhileSticking()
        {
            SuctionMount mount = new(startsKicked: true);

            Assert.False(mount.TickSticking(0.016f));

            mount.BeginSticking();
            Assert.False(mount.TickSticking(0.02f));
            Assert.True(mount.TickSticking(0.04f));
        }

        [Fact]
        public void CancelStickingStopsTheTimer()
        {
            SuctionMount mount = new(startsKicked: true);
            mount.BeginSticking();

            mount.CancelSticking();

            Assert.Equal(-1f, mount.StickTimer);
            Assert.False(mount.TickSticking(1f));
        }

        [Fact]
        public void TakeStainDecrementsUntilExhausted()
        {
            SuctionMount mount = new(startsKicked: false);

            for (int i = Grab.MAX_STAINS; i > 0; i--)
            {
                Assert.True(mount.TakeStain(out float alpha));
                Assert.Equal(i / 10f, alpha);
            }

            Assert.False(mount.TakeStain(out _));
        }

        [Fact]
        public void MountedCupFollowsAPlatformDetachedCupDoesNot()
        {
            SuctionMount mounted = new(startsKicked: false);
            SuctionMount detached = new(startsKicked: true);

            Assert.True(mounted.FollowsPlatform);
            Assert.False(detached.FollowsPlatform);
        }
    }
}
