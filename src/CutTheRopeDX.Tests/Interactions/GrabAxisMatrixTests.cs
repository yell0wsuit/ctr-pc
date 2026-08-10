using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests.Interactions
{
    /// <summary>
    /// Pins the behavior of every grab axis combination the shipped content produces, so the
    /// axis refactor can be verified against observable outcomes rather than field values.
    /// </summary>
    public class GrabAxisMatrixTests
    {
        private const int GrabX = 250;
        private const int GrabY = 120;
        private const int CandyX = 250;
        private const int CandyY = 300;

        private static GameScene Load(Scenario scenario)
        {
            return scenario.Build();
        }

        /// <summary>605 grabs in the original packs: a plain hook with an authored rope.</summary>
        [Fact]
        public void FixedStartsWithAnAttachedRope()
        {
            GameScene scene = Load(new Scenario()
                .Candy(CandyX, CandyY)
                .Grab(GrabX, GrabY, length: 200, moveLength: -1f));

            Grab hook = scene.Grabs()[0];

            Assert.NotNull(hook.Rope);
            Assert.Equal(-1, hook.Rope.cut);
        }

        /// <summary>160 grabs: no rope until the candy enters the radius, then exactly one.</summary>
        [Fact]
        public void RadiusAttachesOnceWhenTheCandyComesInRange()
        {
            GameScene scene = Load(new Scenario()
                .Candy(CandyX, CandyY)
                .Grab(GrabX, GrabY, radius: 200f, moveLength: -1f));

            Grab hook = scene.Grabs()[0];
            Assert.Null(hook.Rope);

            Assert.True(Interaction.StepUntil(scene, () => hook.Rope != null));
            Bungee first = hook.Rope;

            HeadlessGame.StepFrames(scene, 30);
            Assert.Same(first, hook.Rope);
        }

        /// <summary>
        /// 48 grabs in the original packs plus 10 in Experiments. The single most important row:
        /// a radius hook that also slides on a rail. A one-kind model would have destroyed this.
        /// </summary>
        [Fact]
        public void RadiusPlusRailKeepsBothTheRailAndTheAutoAttach()
        {
            GameScene scene = Load(new Scenario()
                .Candy(CandyX, CandyY)
                .Grab(GrabX, GrabY, radius: 200f, moveLength: 150f));

            Grab hook = scene.Grabs()[0];
            float startX = hook.x;

            // The rail responds to a drag.
            int hookX = (int)hook.x;
            int hookY = (int)hook.y;

            Assert.True(scene.TouchDownXYIndex(hookX, hookY, 0));
            _ = scene.TouchMoveXYIndex(hookX + 100, hookY, 0);
            Assert.NotEqual(startX, hook.x);
            _ = scene.TouchUpXYIndex(hookX + 100, hookY, 0);

            // And the radius still attaches.
            Assert.True(Interaction.StepUntil(scene, () => hook.Rope != null));
        }

        /// <summary>28 grabs: a bee-path hook that also auto-attaches. The path wins over any rail.</summary>
        [Fact]
        public void BeePlusRadiusMovesAlongItsPathAndStillAttaches()
        {
            GameScene scene = Load(new Scenario()
                .Candy(CandyX, CandyY)
                .Grab(GrabX, GrabY, radius: 200f, path: "80,0", moveSpeed: 60f, moveLength: -1f));

            Grab hook = scene.Grabs()[0];
            float startX = hook.x;

            HeadlessGame.StepFrames(scene, 30);
            Assert.NotEqual(startX, hook.x);
            Assert.True(Interaction.StepUntil(scene, () => hook.Rope != null));
        }

        /// <summary>2 grabs: a wheel that rolls an auto-attached rope.</summary>
        [Fact]
        public void RadiusPlusWheelRollsTheAutoAttachedRope()
        {
            GameScene scene = Load(new Scenario()
                .Candy(CandyX, CandyY)
                .Grab(GrabX, GrabY, radius: 200f, wheel: true, moveLength: -1f));

            Grab hook = scene.Grabs()[0];
            Assert.True(Interaction.StepUntil(scene, () => hook.Rope != null));

            int hookX = (int)hook.x;
            int hookY = (int)hook.y;
            int lengthBefore = hook.Rope.GetLength();

            // Start on the right side of the wheel.
            Assert.True(scene.TouchDownXYIndex(hookX + 80, hookY, 0));

            // Rotate in the direction that rolls the rope back.
            _ = scene.TouchMoveXYIndex(hookX + 57, hookY - 57, 0);
            _ = scene.TouchMoveXYIndex(hookX, hookY - 80, 0);
            _ = scene.TouchMoveXYIndex(hookX - 57, hookY - 57, 0);
            _ = scene.TouchMoveXYIndex(hookX - 80, hookY, 0);

            _ = scene.TouchUpXYIndex(hookX - 80, hookY, 0);

            Assert.NotEqual(lengthBefore, hook.Rope.GetLength());
        }

        /// <summary>34 grabs: a radius hook carrying a spider that walks once a rope exists.</summary>
        [Fact]
        public void RadiusPlusSpiderStartsWalkingAfterTheRopeAttaches()
        {
            GameScene scene = Load(new Scenario()
                .Candy(CandyX, CandyY)
                .Grab(GrabX, GrabY, radius: 200f, spider: true, moveLength: -1f));

            Grab hook = scene.Grabs()[0];
            Assert.False(hook.Spider.IsWalking);

            Assert.True(Interaction.StepUntil(scene, () => hook.Rope != null));
            Assert.True(Interaction.StepUntil(scene, () => hook.Spider.IsWalking));
        }

        /// <summary>
        /// 47 grabs across 32 Experiments maps. Tapping a stuck cup detaches it: the rope anchor
        /// unpins and takes weight, and the hook starts falling.
        /// </summary>
        [Fact]
        public void SuctionCupDetachesOnTapAndFalls()
        {
            GameScene scene = Load(new Scenario()
                .Candy(CandyX, CandyY)
                .Grab(GrabX, GrabY, length: 200, kickable: true, moveLength: -1f));

            Grab cup = scene.Grabs()[0];
            Assert.True(cup.Mount.IsMounted);

            int cupX = (int)cup.x;
            int cupY = (int)cup.y;

            Assert.True(scene.TouchDownXYIndex(cupX, cupY, 0));
            _ = scene.TouchUpXYIndex(cupX, cupY, 0);

            Assert.False(cup.Mount.IsMounted);
            Assert.Equal(-1f, cup.Rope.bungeeAnchor.pin.X);
        }

        /// <summary>6 Experiments maps author kicked="true", so a cup can start detached.</summary>
        [Fact]
        public void SuctionCupCanStartDetached()
        {
            GameScene scene = Load(new Scenario()
                .Candy(CandyX, CandyY)
                .Grab(GrabX, GrabY, length: 200, kickable: true, kicked: true, moveLength: -1f));

            Grab cup = scene.Grabs()[0];

            Assert.False(cup.Mount.IsMounted);
            Assert.Equal(-1f, cup.Rope.bungeeAnchor.pin.X);
        }

        /// <summary>73 grabs across 38 Experiments maps: an unfired gun makes a rope on tap.</summary>
        [Fact]
        public void GunFiresOnceAndCreatesARope()
        {
            GameScene scene = Load(new Scenario()
                .Candy(CandyX, CandyY)
                .Grab(GrabX, GrabY, gun: true, moveLength: -1f));

            Grab gun = scene.Grabs()[0];
            Assert.Null(gun.Rope);

            int gunX = (int)gun.x;
            int gunY = (int)gun.y;

            Assert.True(scene.TouchDownXYIndex(gunX, gunY, 0));

            Assert.NotNull(gun.Rope);

            Bungee fired = gun.Rope;

            _ = scene.TouchDownXYIndex(gunX, gunY, 0);
            Assert.Same(fired, gun.Rope);
        }

        /// <summary>
        /// The trap the design removes: a map that omits moveLength entirely used to have its
        /// suction cup silently cleared by SetMoveLengthVerticalOffset. Rail-vs-cup is now an
        /// explicit resolver decision and the cup survives.
        /// </summary>
        [Fact]
        public void SuctionCupSurvivesAMissingMoveLengthAttribute()
        {
            GameScene scene = Load(new Scenario()
                .Candy(CandyX, CandyY)
                .Grab(GrabX, GrabY, length: 200, kickable: true));

            Grab cup = scene.Grabs()[0];

            Assert.NotNull(cup.Mount);
            Assert.Null(cup.Rail);
        }
    }
}
