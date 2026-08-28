using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.GameMain;

using Xunit;

using static CutTheRopeDX.Framework.Helpers.CTRMathHelper;

namespace CutTheRopeDX.Tests.Interactions
{
    /// <summary>
    /// Pins the rocket behaviours that Cut the Rope: Time Travel does differently from the
    /// Experiments rocket the rest of the port descends from. Every one of them is gated on a
    /// map's <c>useTimeTravelRocketPhysics</c> flag, so each case checks both sides of the gate.
    /// </summary>
    public sealed class TimeTravelRocketPhysicsTests
    {
        [Fact]
        public void TheReelSpeedIsNotModelBranched()
        {
            // Time Travel's literal is 400, but it is a world distance per second and its world is
            // 960 tall to this one's 1440. 600 is the same reel in screen fractions, so branching
            // the constant on the flag would reel a Time Travel level in 1.5x too slowly.
            _ = BuildScene(timeTravel: true);
            Assert.Equal(600f, ActivePhysicsConstants.RocketReelSpeed);

            _ = BuildScene(timeTravel: false);
            Assert.Equal(600f, ActivePhysicsConstants.RocketReelSpeed);
        }

        [Fact]
        public void TimeTravelHoldsTheExhaustToAHigherMinimumSpeed()
        {
            _ = BuildScene(timeTravel: true);
            Assert.Equal(2f, ActivePhysicsConstants.RocketExhaustSpeedFloor);

            _ = BuildScene(timeTravel: false);
            Assert.Equal(1f, ActivePhysicsConstants.RocketExhaustSpeedFloor);
        }

        [Fact]
        public void TimeTravelRelaxesThePairOnlyWhileReelingIn()
        {
            _ = BuildScene(timeTravel: true);
            Assert.False(ActivePhysicsConstants.RocketRelaxDuringFlight);

            _ = BuildScene(timeTravel: false);
            Assert.True(ActivePhysicsConstants.RocketRelaxDuringFlight);
        }

        [Fact]
        public void TimeTravelSteersOffAHeldCandysRope()
        {
            // Time Travel does not care who is holding the candy. It steers off the candy
            // connector as well, which is not model-branched at all.
            _ = BuildScene(timeTravel: true);
            Assert.False(ActivePhysicsConstants.RocketRopeAlignRequiresFreeCandy);

            _ = BuildScene(timeTravel: false);
            Assert.True(ActivePhysicsConstants.RocketRopeAlignRequiresFreeCandy);
        }

        [Fact]
        public void TimeTravelPopsTheBubbleWhenTheRocketTakesTheCandy()
        {
            _ = BuildScene(timeTravel: true);
            Assert.True(ActivePhysicsConstants.RocketBindPopsCandyBubble);

            _ = BuildScene(timeTravel: false);
            Assert.False(ActivePhysicsConstants.RocketBindPopsCandyBubble);
        }

        [Fact]
        public void BothModelsBindThroughTheReelInWhenSomethingHoldsTheCandy()
        {
            // Time Travel always binds to the reel-in and always reels: its handler has no branch
            // on who is holding the candy, on the bind or on the state change to flight.
            _ = BuildScene(timeTravel: true);
            Assert.False(ActivePhysicsConstants.RocketBindsDirectlyToFlightWhenHeld);

            _ = BuildScene(timeTravel: false);
            Assert.True(ActivePhysicsConstants.RocketBindsDirectlyToFlightWhenHeld);
        }

        [Fact]
        public void NeitherModelRotatesInsideTheDeadZone()
        {
            // DELIBERATE DEVIATION, not an oversight: Time Travel really has no dead zone - its
            // handleRotate turns the rocket from the first move event. The port keeps the
            // Experiments dead zone in both models so a mouse click, which almost always carries a
            // pixel or two of movement, still reaches the tap-to-turn path instead of being read
            // as a drag. See TappingARotatableRocketTurnsIt45Degrees.
            Assert.Equal(0f, NudgeRotatableRocket(timeTravel: true));
            Assert.Equal(0f, NudgeRotatableRocket(timeTravel: false));
        }

        [Fact]
        public void TappingARotatableRocketTurnsIt45Degrees()
        {
            // A real mouse click carries a pixel or two of drift, so the gesture reports a move
            // before it lifts. Inside the dead zone that must still count as a tap: rotateHandled
            // stays false and the release plays the relative +45 timeline, rather than snapping to
            // wherever the drift left the rocket. Matches Time Travel's own touch-up, which stops
            // the running timeline and then playTimeline(0).
            GameScene scene = BuildScene(timeTravel: true, isRotatable: true);
            Rocket rocket = scene.Rockets()[0];
            float startRotation = rocket.rotation;

            // Grip off-centre so the bearing is well defined, then drift 4 world units - inside the
            // 10-unit zone. Ungated, that drift turns the rocket ~5.7 degrees, which snaps straight
            // back to where it started; the tap must instead advance it a full 45.
            Vector grip = scene.ScreenPositionOf(Vect(rocket.x + 40f, rocket.y));
            Vector drifted = scene.ScreenPositionOf(Vect(rocket.x + 40f, rocket.y + 4f));
            _ = scene.TouchDownXYIndex(grip.X, grip.Y, 0);
            _ = scene.TouchMoveXYIndex(drifted.X, drifted.Y, 0);
            Assert.False(rocket.rotateHandled, "drift inside the dead zone was read as a drag");
            _ = scene.TouchUpXYIndex(drifted.X, drifted.Y, 0);
            HeadlessGame.StepFrames(scene, 15);

            Assert.Equal(startRotation + DEG_45, rocket.rotation, 1);
        }

        [Fact]
        public void AFrozenRocketKeepsTravellingItsPath()
        {
            // Rocket::update reaches the mover advance whether or not the freeze flag is set, and
            // the position sync that follows it is unconditional too - so the point is dragged to
            // the mover even though it never integrates itself.
            GameScene scene = BuildScene(timeTravel: true, path: "160,0", moveSpeed: 60f, pauseSwitcher: true);
            Rocket rocket = scene.Rockets()[0];
            HeadlessGame.StepFrames(scene, 5);
            float runningX = rocket.x;

            Vector button = scene.ScreenPositionOf(scene.PauseSwitchers()[0]);
            _ = scene.TouchDownXYIndex(button.X, button.Y, 0);
            _ = scene.TouchUpXYIndex(button.X, button.Y, 0);
            Assert.True(scene.IsTimeFrozen());
            HeadlessGame.StepFrames(scene, 10);

            Assert.True(
                rocket.x > runningX,
                $"the frozen rocket stopped travelling its path: {runningX} -> {rocket.x}");
            Assert.Equal(rocket.x, rocket.point.pos.X, 3);
        }

        [Fact]
        public void AFrozenRocketsPointStopsIntegrating()
        {
            // Rocket::update guards point->update on the freeze flag in both arm64 builds, so a
            // frozen path-less rocket holds still no matter what velocity it was carrying. The
            // mover advance and the position sync that follows it are unguarded.
            GameScene scene = BuildScene(timeTravel: true, pauseSwitcher: true);
            Rocket rocket = scene.Rockets()[0];
            HeadlessGame.StepFrames(scene, 5);

            Vector button = scene.ScreenPositionOf(scene.PauseSwitchers()[0]);
            _ = scene.TouchDownXYIndex(button.X, button.Y, 0);
            _ = scene.TouchUpXYIndex(button.X, button.Y, 0);
            Assert.True(scene.IsTimeFrozen());

            // A path-less rocket has no mover to lead, so its point drives its position.
            Assert.Null(rocket.mover);
            rocket.point.prevPos = Vect(rocket.point.pos.X - 6f, rocket.point.pos.Y);
            float frozenX = rocket.point.pos.X;
            HeadlessGame.StepFrames(scene, 4);

            Assert.Equal(frozenX, rocket.point.pos.X, 3);
            Assert.Equal(rocket.point.pos.X, rocket.x, 3);
        }

        [Fact]
        public void ACircularPathRocketLoads()
        {
            // The circle points come from the world-scaled radius, so a mover sized from the raw
            // level radius overruns its path array while the level is still loading.
            foreach (bool timeTravel in new[] { true, false })
            {
                GameScene scene = BuildScene(timeTravel, path: "RC50", moveSpeed: 50f);
                Rocket rocket = scene.Rockets()[0];
                Assert.NotNull(rocket.mover);
                HeadlessGame.StepFrames(scene, 5);
            }
        }

        /// <summary>
        /// Drags a rotatable rocket 5 world units - inside the dead zone - and reports how far it
        /// turned.
        /// </summary>
        /// <param name="timeTravel">Whether the map opts into the Time Travel rocket.</param>
        /// <returns>The rotation the gesture produced, in degrees.</returns>
        private static float NudgeRotatableRocket(bool timeTravel)
        {
            GameScene scene = BuildScene(timeTravel, isRotatable: true);
            Rocket rocket = scene.Rockets()[0];
            float startRotation = rocket.rotation;
            Vector grip = Vect(rocket.x + 100f, rocket.y);
            rocket.HandleTouch(grip);
            rocket.HandleRotate(Vect(grip.X, grip.Y + 5f));
            return rocket.rotation - startRotation;
        }

        /// <summary>Builds a one-candy, one-rocket scene on either side of the physics gate.</summary>
        /// <param name="timeTravel">Whether the map opts into the Time Travel rocket.</param>
        /// <param name="isRotatable">Whether the rocket accepts rotation gestures.</param>
        /// <param name="path">Optional mover path string.</param>
        /// <param name="moveSpeed">Mover speed for <paramref name="path"/>.</param>
        /// <param name="pauseSwitcher">Whether the level carries a time-freeze button.</param>
        /// <returns>The built scene.</returns>
        private static GameScene BuildScene(
            bool timeTravel,
            bool isRotatable = false,
            string path = null,
            float moveSpeed = 0f,
            bool pauseSwitcher = false)
        {
            Scenario scenario = Scenario.New()
                .MapSize(320, 480)
                .Candy(160, 120)
                .Rope(160, 60, length: 60)
                .Rocket(160, 260, angle: 180f, impulse: 5f, isRotatable: isRotatable, path: path, moveSpeed: moveSpeed)
                .OmNom(60, 420);
            if (pauseSwitcher)
            {
                _ = scenario.PauseSwitcher(60, 60);
            }
            if (timeTravel)
            {
                _ = scenario.Design("useTimeTravelRocketPhysics", "true");
            }
            return scenario.Build();
        }
    }
}
