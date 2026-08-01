using System;

using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests.Interactions
{
    /// <summary>
    /// Grab-and-platform matrix: platforms never move a grab that has its own movement. A bee, a
    /// launcher and a player drag rail are excluded when the platform binds; a kicked suction cup
    /// is excluded per frame while it falls and resumes when it re-sticks. Everything else - gun,
    /// wheel, stuck cup, spider hook, auto-attach hook - rides along.
    /// </summary>
    public sealed class GrabPlatformMatrixTests
    {
        private const int PlatformX = 160;
        private const int PlatformY = 200;
        private const int RideFrames = 30;

        [Fact]
        public void Conveyor_DoesNotBindABee()
        {
            (GameScene scene, Grab bee) = OnConveyor(s => s.Grab(PlatformX, PlatformY, path: "60,0", moveSpeed: 30f));

            Assert.NotNull(bee.mover);
            Assert.False(scene.BeltHolds(bee));
        }

        [Fact]
        public void Disc_DoesNotCaptureABee()
        {
            (GameScene scene, Grab bee) = OnDisc(s => s.Grab(PlatformX, PlatformY, path: "60,0", moveSpeed: 30f));
            HeadlessGame.StepFrames(scene, 1);

            Assert.False(scene.DiscHolds(bee));
        }

        [Fact]
        public void Conveyor_DoesNotRebindAGrabThatBecomesALauncher()
        {
            // No level authors a launcher, so one is made the way the engine makes it. The belt's
            // bind pass is then re-run over it: a launcher moves itself, so it is refused for the
            // same reason a bee is - it is one predicate, not two rules.
            (GameScene scene, Grab launcher) = OnConveyor(s => s.Grab(PlatformX, PlatformY));
            Assert.True(scene.BeltHolds(launcher));

            scene.Conveyors().Remove(launcher);
            launcher.SetLauncher();
            scene.Conveyors().ProcessItems(new[] { launcher });

            Assert.False(scene.BeltHolds(launcher));
        }

        [Fact]
        public void Disc_ReleasesAGrabThatBecomesALauncher()
        {
            (GameScene scene, Grab launcher) = OnDisc(s => s.Grab(PlatformX, PlatformY));
            HeadlessGame.StepFrames(scene, 1);
            Assert.True(scene.DiscHolds(launcher));

            launcher.SetLauncher();
            HeadlessGame.StepFrames(scene, 1);

            Assert.False(scene.DiscHolds(launcher));
        }

        [Fact]
        public void Conveyor_DoesNotBindAMoveableRail()
        {
            (GameScene scene, Grab rail) = OnConveyor(s => s.Grab(PlatformX, PlatformY, moveLength: 60f));

            Assert.True(rail.moveLength > 0f);
            Assert.False(scene.BeltHolds(rail));
        }

        [Fact]
        public void Disc_DoesNotCaptureAMoveableRail()
        {
            (GameScene scene, Grab rail) = OnDisc(s => s.Grab(PlatformX, PlatformY, moveLength: 60f));
            HeadlessGame.StepFrames(scene, 1);

            Assert.False(scene.DiscHolds(rail));
        }

        [Fact]
        public void Conveyor_CarriesAGun()
        {
            (GameScene scene, Grab gun) = OnConveyor(s => s.Grab(PlatformX, PlatformY, gun: true));

            AssertCarriedByBelt(scene, gun);
        }

        [Fact]
        public void Disc_CapturesAGun()
        {
            (GameScene scene, Grab gun) = OnDisc(s => s.Grab(PlatformX, PlatformY, gun: true));
            HeadlessGame.StepFrames(scene, 1);

            Assert.True(scene.DiscHolds(gun));
        }

        [Fact]
        public void Conveyor_CarriesAWheel()
        {
            (GameScene scene, Grab wheel) = OnConveyor(s => s.Grab(PlatformX, PlatformY, wheel: true));

            AssertCarriedByBelt(scene, wheel);
        }

        [Fact]
        public void Disc_CapturesAWheel()
        {
            (GameScene scene, Grab wheel) = OnDisc(s => s.Grab(PlatformX, PlatformY, wheel: true));
            HeadlessGame.StepFrames(scene, 1);

            Assert.True(scene.DiscHolds(wheel));
        }

        [Fact]
        public void Conveyor_CarriesAStuckSuctionCup()
        {
            (GameScene scene, Grab cup) = OnConveyor(s => s.Grab(PlatformX, PlatformY, kickable: true));

            AssertCarriedByBelt(scene, cup);
        }

        [Fact]
        public void Disc_CapturesAStuckSuctionCup()
        {
            (GameScene scene, Grab cup) = OnDisc(s => s.Grab(PlatformX, PlatformY, kickable: true));
            HeadlessGame.StepFrames(scene, 1);

            Assert.True(scene.DiscHolds(cup));
        }

        [Fact]
        public void Conveyor_StopsDrivingAKickedSuctionCup()
        {
            // A kicked cup stays bound - the belt just stops advancing it, so it resumes on its own
            // once it re-sticks. Its own position keeps changing as it falls, so the belt's
            // coordinate is what has to hold still.
            (GameScene scene, Grab cup) = OnConveyor(s => s.Grab(PlatformX, PlatformY, kickable: true, kicked: true));
            Assert.True(scene.BeltHolds(cup));

            float startOffset = cup.PositionOnTransporter;
            HeadlessGame.StepFrames(scene, RideFrames);

            Assert.Equal(startOffset, cup.PositionOnTransporter);
        }

        [Fact]
        public void Conveyor_ResumesDrivingASuctionCupThatReSticks()
        {
            (GameScene scene, Grab cup) = OnConveyor(s => s.Grab(PlatformX, PlatformY, kickable: true, kicked: true));
            HeadlessGame.StepFrames(scene, RideFrames);

            cup.kicked = false;
            float restuckOffset = cup.PositionOnTransporter;
            HeadlessGame.StepFrames(scene, RideFrames);

            Assert.NotEqual(restuckOffset, cup.PositionOnTransporter);
        }

        [Fact]
        public void Disc_DoesNotCaptureAKickedSuctionCup()
        {
            (GameScene scene, Grab cup) = OnDisc(s => s.Grab(PlatformX, PlatformY, kickable: true, kicked: true));
            HeadlessGame.StepFrames(scene, 1);

            Assert.False(scene.DiscHolds(cup));
        }

        [Fact]
        public void Disc_CapturesASuctionCupThatReSticks()
        {
            (GameScene scene, Grab cup) = OnDisc(s => s.Grab(PlatformX, PlatformY, kickable: true, kicked: true));
            HeadlessGame.StepFrames(scene, 1);
            Assert.False(scene.DiscHolds(cup));

            cup.kicked = false;
            HeadlessGame.StepFrames(scene, 1);

            Assert.True(scene.DiscHolds(cup));
        }

        [Fact]
        public void Conveyor_CarriesASpiderHook()
        {
            (GameScene scene, Grab spider) = OnConveyor(s => s.Grab(PlatformX, PlatformY, spider: true));

            AssertCarriedByBelt(scene, spider);
        }

        [Fact]
        public void Disc_CapturesASpiderHook()
        {
            (GameScene scene, Grab spider) = OnDisc(s => s.Grab(PlatformX, PlatformY, spider: true));
            HeadlessGame.StepFrames(scene, 1);

            Assert.True(scene.DiscHolds(spider));
        }

        [Fact]
        public void Conveyor_CarriesAnAutoAttachHook()
        {
            (GameScene scene, Grab autoHook) = OnConveyor(s => s.Grab(PlatformX, PlatformY, radius: 40f));

            AssertCarriedByBelt(scene, autoHook);
        }

        [Fact]
        public void Disc_CapturesAnAutoAttachHook()
        {
            (GameScene scene, Grab autoHook) = OnDisc(s => s.Grab(PlatformX, PlatformY, radius: 40f));
            HeadlessGame.StepFrames(scene, 1);

            Assert.True(scene.DiscHolds(autoHook));
        }

        private static void AssertCarriedByBelt(GameScene scene, Grab grab)
        {
            Assert.True(scene.BeltHolds(grab), "the belt never bound the grab");

            float startOffset = grab.PositionOnTransporter;
            HeadlessGame.StepFrames(scene, RideFrames);

            Assert.NotEqual(startOffset, grab.PositionOnTransporter);
        }

        private static (GameScene Scene, Grab Grab) OnConveyor(Func<Scenario, Scenario> grab)
        {
            GameScene scene = grab(
                Scenario.New()
                    .Candy(160, 320)
                    .OmNom(20, 460)
                    .Conveyor(PlatformX, PlatformY))
                .Build();
            return (scene, scene.Grabs()[0]);
        }

        private static (GameScene Scene, Grab Grab) OnDisc(Func<Scenario, Scenario> grab)
        {
            GameScene scene = grab(
                Scenario.New()
                    .Candy(160, 320)
                    .OmNom(20, 460)
                    .Disc(PlatformX, PlatformY))
                .Build();
            return (scene, scene.Grabs()[0]);
        }
    }
}
