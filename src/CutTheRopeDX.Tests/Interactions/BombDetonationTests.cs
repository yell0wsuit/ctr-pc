using System.Collections.Generic;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Physics;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests.Interactions
{
    /// <summary>
    /// Time Travel bomb: how it loads, what ropes it binds, what sets it off, and what the blast
    /// does to everything around it.
    /// </summary>
    public sealed class BombDetonationTests
    {
        [Fact]
        public void BombLoadsAsACandyLikeBodyThatIsNotCandy()
        {
            GameScene scene = Scenario.New().Candy(60, 100).Bomb(160, 200, "second").Build();

            CandyContext bomb = Assert.Single(scene.Bombs());

            Assert.Equal("second", bomb.bombNumber);
            Assert.Null(bomb.candyNumber);
            Assert.False(bomb.Capabilities.CanBeEaten);
            Assert.False(bomb.Capabilities.CanLoseLevelWhenOffScreen);
            Assert.False(bomb.Capabilities.CanRotateWithRopes);
            Assert.Equal(Scenario.WorldY(200), bomb.WholeBody.Point.pos.Y, 3);
        }

        [Fact]
        public void BombedGrabBindsItsRopeToTheBombNotTheCandy()
        {
            GameScene scene = Scenario.New()
                .Candy(60, 300)
                .Bomb(160, 200, "first")
                .Grab(160, 120, length: 100, candyNumber: "first", bombed: true)
                .Build();

            CandyContext bomb = Assert.Single(scene.Bombs());
            Bungee rope = Assert.Single(scene.Grabs()).RopeOf();

            Assert.NotNull(rope);
            Assert.Same(bomb.WholeBody.Point, rope.parts[^1]);
        }

        [Fact]
        public void UnbombedGrabStillBindsToTheCandy()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 200, "first")
                .Bomb(60, 200, "first")
                .Grab(160, 120, length: 100, candyNumber: "first")
                .Build();

            Bungee rope = Assert.Single(scene.Grabs()).RopeOf();

            Assert.NotNull(rope);
            Assert.Same(scene.Candies()[0].WholeBody.Point, rope.parts[^1]);
        }

        [Fact]
        public void CutStrokeAcrossTheBombDetonatesIt()
        {
            GameScene scene = Scenario.New().Candy(40, 400).Bomb(160, 200).Build();
            CandyContext bomb = Assert.Single(scene.Bombs());
            Vector at = bomb.WholeBody.Point.pos;

            _ = scene.CutWithRazorOrLine1Line2Immediate(
                null,
                Vect(at.X - 60f, at.Y),
                Vect(at.X + 60f, at.Y),
                false);

            Assert.True(bomb.bomb.Exploded);
        }

        [Fact]
        public void CutStrokeThatMissesTheBombLeavesItAlone()
        {
            GameScene scene = Scenario.New().Candy(40, 400).Bomb(160, 200).Build();
            CandyContext bomb = Assert.Single(scene.Bombs());
            Vector at = bomb.WholeBody.Point.pos;

            _ = scene.CutWithRazorOrLine1Line2Immediate(
                null,
                Vect(at.X - 60f, at.Y + 200f),
                Vect(at.X + 60f, at.Y + 200f),
                false);

            Assert.False(bomb.bomb.Exploded);
        }

        [Fact]
        public void CandyTouchingTheBombDetonatesIt()
        {
            GameScene scene = Scenario.New().Candy(40, 400).Bomb(160, 200).Build();
            CandyContext bomb = Assert.Single(scene.Bombs());

            PlaceAt(scene.Candy().WholeBody.Point, bomb.WholeBody.Point.pos);
            HeadlessGame.StepFrames(scene, 75);

            Assert.True(bomb.bomb.Exploded);
        }

        [Fact]
        public void CandyOutsideTheContactDistanceLeavesTheBombAlone()
        {
            GameScene scene = Scenario.New().Candy(40, 400).Bomb(160, 200).Build();
            CandyContext bomb = Assert.Single(scene.Bombs());
            Vector bombPos = bomb.WholeBody.Point.pos;

            PlaceAt(
                scene.Candy().WholeBody.Point,
                Vect(bombPos.X + BombDefinition.ContactTriggerDistance + 20f, bombPos.Y));
            HeadlessGame.StepFrames(scene, 1);

            Assert.False(bomb.bomb.Exploded);
        }

        [Fact]
        public void TwoBombsThatMeetBothDetonate()
        {
            GameScene scene = Scenario.New()
                .Candy(40, 440)
                .Bomb(160, 200, "first")
                .Bomb(40, 60, "second")
                .Build();
            List<CandyContext> bombs = scene.Bombs();
            Assert.Equal(2, bombs.Count);

            // Far from the candy, so the pair sets each other off rather than the candy doing it.
            PlaceAt(scene.Candy().WholeBody.Point, Vect(2000f, 200f));
            Vector meetingPoint = bombs[0].WholeBody.Point.pos;
            PlaceAt(
                bombs[1].WholeBody.Point,
                Vect(meetingPoint.X + (BombDefinition.BombPairTriggerDistance / 2f), meetingPoint.Y));
            HeadlessGame.StepFrames(scene, 1);

            Assert.True(bombs[0].bomb.Exploded);
            Assert.True(bombs[1].bomb.Exploded);
        }

        [Fact]
        public void BlastPushesCandyAwayFromTheBomb()
        {
            GameScene scene = Scenario.New().Candy(40, 400).Bomb(160, 200).Build();
            CandyContext bomb = Assert.Single(scene.Bombs());
            ConstraintedPoint candyPoint = scene.Candy().WholeBody.Point;
            Vector bombPos = bomb.WholeBody.Point.pos;

            // Well inside the blast radius but well outside the contact distance, so only the blast
            // moves it, and to one side so the push is unambiguously horizontal.
            float offset = BombDefinition.BlastRadius / 2f;
            PlaceAt(candyPoint, Vect(bombPos.X + offset, bombPos.Y));
            float startX = candyPoint.pos.X;

            scene.BoomBoomBomb(bomb, 0.016f);

            Assert.True(bomb.bomb.Exploded);
            Assert.True(
                candyPoint.pos.X > startX,
                $"blast should push the candy outward; x went {startX} -> {candyPoint.pos.X}");
        }

        [Fact]
        public void BlastFallsOffWithDistance()
        {
            float near = PushDistanceAtOffset(BombDefinition.BlastRadius * 0.25f);
            float far = PushDistanceAtOffset(BombDefinition.BlastRadius * 0.75f);

            Assert.True(near > far, $"near push {near} should exceed far push {far}");
        }

        [Fact]
        public void BlastReachesTheSameSliceOfTheLevelTheOriginalDoes()
        {
            // The original's 400-unit radius spans 200 cells of the 320x480 authoring grid, since it
            // doubles authored coordinates and DX triples them. Pin that reach in authored units so
            // a change to the conversion cannot quietly shrink the blast.
            const float authoredReach = 200f;

            float inside = PushDistanceAtOffset((authoredReach - 10f) * Scenario.Scale);
            float outside = PushDistanceAtOffset((authoredReach + 10f) * Scenario.Scale);

            Assert.True(inside > 0f, $"blast should still reach {authoredReach - 10f} authored units");
            Assert.Equal(0f, outside, 4);
        }

        [Fact]
        public void BlastDoesNotReachBeyondTheRadius()
        {
            float outside = PushDistanceAtOffset(BombDefinition.BlastRadius + 10f);

            Assert.Equal(0f, outside, 4);
        }

        [Fact]
        public void DetonationDropsTheBombsRopes()
        {
            GameScene scene = Scenario.New()
                .Candy(40, 440)
                .Bomb(160, 200, "first")
                .Grab(160, 120, length: 100, candyNumber: "first", bombed: true)
                .Build();
            CandyContext bomb = Assert.Single(scene.Bombs());
            Assert.Equal(1, scene.AttachedRopeCount(bomb));

            scene.BoomBoomBomb(bomb, 0.016f);

            Assert.Equal(0, scene.AttachedRopeCount(bomb));
        }

        [Fact]
        public void DetonatedBombDoesNotDetonateAgain()
        {
            GameScene scene = Scenario.New().Candy(40, 440).Bomb(160, 200).Build();
            CandyContext bomb = Assert.Single(scene.Bombs());
            ConstraintedPoint candyPoint = scene.Candy().WholeBody.Point;
            Vector bombPos = bomb.WholeBody.Point.pos;
            PlaceAt(candyPoint, Vect(bombPos.X + (BombDefinition.BlastRadius / 2f), bombPos.Y));

            scene.BoomBoomBomb(bomb, 0.016f);
            float afterFirstBlast = candyPoint.pos.X;
            PlaceAt(candyPoint, Vect(afterFirstBlast, bombPos.Y));

            scene.BoomBoomBomb(bomb, 0.016f);

            Assert.Equal(afterFirstBlast, candyPoint.pos.X, 4);
        }

        [Fact]
        public void DetonatedBombIsRetiredAfterItsDebrisDelay()
        {
            GameScene scene = Scenario.New().Candy(40, 440).Bomb(160, 200).Build();
            CandyContext bomb = Assert.Single(scene.Bombs());

            scene.BoomBoomBomb(bomb, 0.016f);
            Assert.False(bomb.HasNoWholeBodyInPlay);

            // Past the debris delay, which is what removes the wreck.
            HeadlessGame.StepFrames(scene, 20);

            Assert.True(bomb.HasNoWholeBodyInPlay);
        }

        [Fact]
        public void BombsHighPriorityGrabPrefersTheBombOverCandyInRange()
        {
            // Candy and bomb both inside the radius but far enough apart not to set the bomb off.
            GameScene scene = Scenario.New()
                .Candy(120, 200)
                .Bomb(200, 200, "first")
                .Grab(160, 200, radius: 80f, bombsHighPriority: true)
                .Build();

            CandyContext bomb = Assert.Single(scene.Bombs());
            HeadlessGame.StepFrames(scene, 2);
            Bungee rope = Assert.Single(scene.Grabs()).RopeOf();

            Assert.NotNull(rope);
            Assert.Same(bomb.WholeBody.Point, rope.parts[^1]);
        }

        [Fact]
        public void PlainRadiusGrabStillPrefersCandy()
        {
            GameScene scene = Scenario.New()
                .Candy(120, 200)
                .Bomb(200, 200, "first")
                .Grab(160, 200, radius: 80f)
                .Build();

            HeadlessGame.StepFrames(scene, 2);
            Bungee rope = Assert.Single(scene.Grabs()).RopeOf();

            Assert.NotNull(rope);
            Assert.Same(scene.Candies()[0].WholeBody.Point, rope.parts[^1]);
        }

        [Fact]
        public void RocketCanBindToABombAndFlyIt()
        {
            GameScene scene = Scenario.New()
                .Candy(40, 440)
                .Bomb(160, 200)
                .Rocket(160, 260, impulse: 0f)
                .Build();
            CandyContext bomb = Assert.Single(scene.Bombs());

            Rocket rocket = Act.BindRocket(scene, bomb);

            Assert.Same(rocket, bomb.Lifecycle.Attachments.Rocket);
            Assert.NotEqual(Rocket.STATE_ROCKET_EXAUST, rocket.state);
        }

        [Fact]
        public void DetonationCutsOutTheBombsRocket()
        {
            GameScene scene = Scenario.New()
                .Candy(40, 440)
                .Bomb(160, 200)
                .Rocket(160, 260, impulse: 0f)
                .Build();
            CandyContext bomb = Assert.Single(scene.Bombs());
            Rocket rocket = Act.BindRocket(scene, bomb);

            scene.BoomBoomBomb(bomb, 0.016f);

            Assert.Equal(Rocket.STATE_ROCKET_EXAUST, rocket.state);
            Assert.Null(bomb.Lifecycle.Attachments.Rocket);
        }

        /// <summary>
        /// Detonates a bomb with a candy parked <paramref name="offset"/> to its right and reports
        /// how far the blast moved that candy.
        /// </summary>
        private static float PushDistanceAtOffset(float offset)
        {
            GameScene scene = Scenario.New().Candy(40, 440).Bomb(160, 200).Build();
            CandyContext bomb = Assert.Single(scene.Bombs());
            ConstraintedPoint candyPoint = scene.Candy().WholeBody.Point;
            Vector bombPos = bomb.WholeBody.Point.pos;

            PlaceAt(candyPoint, Vect(bombPos.X + offset, bombPos.Y));
            float startX = candyPoint.pos.X;

            scene.BoomBoomBomb(bomb, 0.016f);

            return candyPoint.pos.X - startX;
        }

        /// <summary>Teleports a body, clearing its Verlet history so it starts from rest.</summary>
        private static void PlaceAt(ConstraintedPoint point, Vector position)
        {
            point.pos = position;
            point.prevPos = position;
            point.v = default;
            point.a = default;
            point.posDelta = default;
        }

        private static Vector Vect(float x, float y)
        {
            return new Vector(x, y);
        }
    }
}
