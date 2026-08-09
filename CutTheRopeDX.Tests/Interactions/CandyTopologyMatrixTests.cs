using System;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests.Interactions
{
    /// <summary>
    /// The same behaviour asked of each candy topology the engine supports: one whole candy, one
    /// split candy owning two halves, and several independent whole candies.
    /// </summary>
    /// <remarks>
    /// Every bug this file guards was a case of one topology borrowing another's state - a rocket
    /// taking its reach from whichever candy was primary, a merge measured off a frame-fresh position
    /// the reference engine never had, a carrier offered a body it can never hold. Scenarios are built
    /// in code so a cell never depends on a shipped level's incidental layout.
    /// </remarks>
    public sealed class CandyTopologyMatrixTests
    {
        /// <summary>Rate the reference engine closes a merge at, in world units per second.</summary>
        private const float MergeSpeed = 200f;

        private const float FrameDelta = 1f / 60f;

        /// <summary>World gap the halves are held at while touching: inside their hitboxes, non-zero.</summary>
        private const float MeetingGap = 8f;

        // ---------- one whole candy ----------

        [Fact]
        public void SingleCandyOffersExactlyOneBodyToTheScene()
        {
            GameScene scene = Scenario.New().Candy(160, 200).OmNom(30, 440).Build();

            _ = Assert.Single(scene.Candies());
            Assert.Equal([scene.Candy().WholeBody], scene.ActiveBodies());
        }

        /// <summary>
        /// With one candy the reach is unambiguous, so this is the control for the multi-candy case
        /// below: the same assertion has to hold whichever topology the level uses.
        /// </summary>
        [Fact]
        public void SingleCandyRocketLeashesTheCandyAtItsOwnReach()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 140)
                .Rocket(160, 210, angle: 90f, impulse: 20f)
                .OmNom(30, 440)
                .Build();
            CandyContext candy = scene.Candy();
            Rocket rocket = scene.Rockets()[0];

            Assert.True(
                Interaction.StepUntil(scene, () => candy.Lifecycle.Attachments.HasActiveRocket),
                "the rocket never bound the candy");

            AssertReachIsToItsOwnCandy(rocket, candy);
        }

        // ---------- one split candy, two halves ----------

        /// <summary>
        /// The merge closes at the reference engine's fixed 200 units/second - the rate its
        /// <c>moveVariableToTarget(&amp;partsDist, 0.0, 200.0, delta)</c> call sets. Pinning the frame
        /// count against the gap catches both a changed rate and a merge advanced more than once a frame.
        /// </summary>
        [Fact]
        public void SplitCandyMergeClosesAtTheReferenceRate()
        {
            (GameScene scene, SplitCandyState split) = SplitScene();
            BringHalvesTogether(scene, split, MeetingGap);

            float gap = split.MergeDistance;
            Assert.True(gap > 0f, "the merge began with nothing left to close");
            int expectedFrames = (int)MathF.Ceiling(gap / (MergeSpeed * FrameDelta));

            int frames = 0;
            CandyContext primary = scene.Candies()[0];
            while (frames < expectedFrames + 8 && primary.Lifecycle.Presence != CandyPresence.Present)
            {
                HeadlessGame.StepFrames(scene, 1);
                frames++;
            }

            Assert.Equal(CandyPresence.Present, primary.Lifecycle.Presence);
            // One frame of slack: the merge completes on the step that drives the remainder to zero.
            Assert.InRange(frames, expectedFrames, expectedFrames + 1);
        }

        /// <summary>
        /// Closing is a rate, not a fixed animation: halves that meet further apart take
        /// proportionally longer, because the same 200 units/second has more ground to cover.
        /// </summary>
        [Theory]
        [InlineData(8f)]
        [InlineData(24f)]
        [InlineData(48f)]
        public void SplitCandyMergeTakesTheGapDividedByTheRate(float gap)
        {
            int expectedFrames = (int)MathF.Ceiling(gap / (MergeSpeed * FrameDelta));

            Assert.InRange(MergeFramesForGap(gap), expectedFrames, expectedFrames + 1);
        }

        /// <summary>
        /// The same merge rate on the shipped level that teaches it. 5_1 hangs each half from an
        /// outer rope plus a shared central hook; cutting the two outer ropes is the level's own
        /// solution and swings the halves together, so this measures the merge a player actually
        /// sees rather than one a test pinned into place.
        /// </summary>
        [Fact]
        public void SplitCandyMergesAtTheReferenceRateOnShippedLevel51()
        {
            HeadlessGame ctr = HeadlessGame.Boot();
            GameScene scene = HeadlessGame.LoadLevel(pack: 4, level: 0);
            scene.gameSceneDelegate = new RecordingSceneDelegate();
            CandyContext primary = scene.Candies()[0];
            SplitCandyState split = primary.Lifecycle.Split;
            Assert.Equal(SplitPhase.Separate, split.Phase);

            Act.CutRope(scene, scene.GrabNearestTo(Level5_1World(98, 59)));
            Act.CutRope(scene, scene.GrabNearestTo(Level5_1World(227, 58)));

            Assert.True(
                Interaction.StepUntil(scene, () => split.Phase == SplitPhase.Merging),
                "the cut halves never swung together");

            float gap = split.MergeDistance;
            Assert.True(gap > 0f, "the merge began with nothing left to close");
            int expectedFrames = (int)MathF.Ceiling(gap / (MergeSpeed * FrameDelta));

            int frames = 0;
            while (frames < expectedFrames + 8 && primary.Lifecycle.Presence != CandyPresence.Present)
            {
                HeadlessGame.StepFrames(scene, 1);
                frames++;
            }

            Assert.Equal(CandyPresence.Present, primary.Lifecycle.Presence);
            Assert.InRange(frames, expectedFrames, expectedFrames + 1);
        }

        /// <summary>5_1 is a 320-wide map, so its authored coordinates land at x*3 + 800.</summary>
        private static Vector Level5_1World(int x, int y)
        {
            return new Vector((x * 3f) + 800f, y * 3f);
        }

        [Fact]
        public void SplitCandyNeverOffersAHalfToTheRocket()
        {
            (GameScene scene, SplitCandyState split) = SplitScene(withRocket: true);

            Assert.Empty(scene.ActiveBodies(CandyInteraction.Rocket));
            HeadlessGame.StepFrames(scene, 120);
            Assert.False(scene.Candies()[0].Lifecycle.Attachments.HasActiveRocket);
            Assert.Equal([split.Left.Body, split.Right.Body], scene.ActiveBodies());
        }

        [Fact]
        public void SplitCandyLosingOneHalfLosesTheLevelOnce()
        {
            (GameScene scene, SplitCandyState split) = SplitScene();

            Interaction.PlaceBodyAt(split.Left.Body, new Vector(split.Left.Body.Point.pos.X, 4000f));
            Assert.True(
                Interaction.StepUntil(scene, () => scene.Outcomes().LostCount > 0),
                "the half that left the screen never lost the level");
            HeadlessGame.StepFrames(scene, 30);

            Assert.Equal(1, scene.Outcomes().LostCount);
            Assert.Equal(0, scene.Outcomes().WonCount);
            Assert.Equal([split.Right.Body], scene.Candies()[0].Lifecycle.ActiveBodies);
        }

        // ---------- several independent whole candies ----------

        /// <summary>
        /// Two candies, each with its own rocket and Om Nom on opposite sides of the map - the shape
        /// a real two-rocket level uses.
        /// </summary>
        [Fact]
        public void TwoCandiesEachRocketBindsItsOwnCandy()
        {
            (GameScene scene, CandyContext left, CandyContext right) = TwoRocketScene();

            Assert.True(
                Interaction.StepUntil(scene, () => left.Lifecycle.Attachments.HasActiveRocket && right.Lifecycle.Attachments.HasActiveRocket),
                "both rockets never bound");

            Assert.Same(scene.Rockets()[0], right.Lifecycle.Attachments.Rocket);
            Assert.Same(scene.Rockets()[1], left.Lifecycle.Attachments.Rocket);
        }

        /// <summary>
        /// The regression this file exists for. A binding rocket resolves to no candy yet, so the
        /// reach used to be measured from the scene's primary candy and then applied as the leash to
        /// whichever candy the rocket actually caught - a leash sized off a candy across the map.
        /// </summary>
        [Fact]
        public void TwoCandiesEachRocketLeashesItsOwnCandyNotThePrimary()
        {
            (GameScene scene, CandyContext left, CandyContext right) = TwoRocketScene();
            Assert.True(
                Interaction.StepUntil(scene, () => left.Lifecycle.Attachments.HasActiveRocket && right.Lifecycle.Attachments.HasActiveRocket),
                "both rockets never bound");

            AssertReachIsToItsOwnCandy(right.Lifecycle.Attachments.Rocket, right);
            AssertReachIsToItsOwnCandy(left.Lifecycle.Attachments.Rocket, left);
        }

        [Fact]
        public void TwoCandiesARocketHoldsNoLeashOnTheCandyItDidNotBind()
        {
            (GameScene scene, CandyContext left, CandyContext right) = TwoRocketScene();
            Assert.True(
                Interaction.StepUntil(scene, () => left.Lifecycle.Attachments.HasActiveRocket && right.Lifecycle.Attachments.HasActiveRocket),
                "both rockets never bound");

            Assert.Null(right.Lifecycle.Attachments.Rocket.BindReach(left));
            Assert.Null(left.Lifecycle.Attachments.Rocket.BindReach(right));
        }

        [Fact]
        public void TwoCandiesEatingOneDoesNotWinWhileTheOtherRemains()
        {
            (GameScene scene, CandyContext first, CandyContext second) = TwoCandyScene();

            Act.Eat(scene, first);
            HeadlessGame.StepFrames(scene, 30);

            Assert.Equal(CandyRemovalReason.Eaten, first.Lifecycle.RemovalReason);
            Assert.Equal(CandyPresence.Present, second.Lifecycle.Presence);
            Assert.Equal(0, scene.Outcomes().WonCount);
            Assert.Equal(0, scene.Outcomes().LostCount);
        }

        [Fact]
        public void TwoCandiesEatingBothWinsOnce()
        {
            (GameScene scene, CandyContext first, CandyContext second) = TwoCandyScene();

            Act.Eat(scene, first);
            Act.Eat(scene, second, targetIndex: 1);
            Assert.True(
                Interaction.StepUntil(scene, () => scene.Outcomes().WonCount > 0),
                "the level was never won after both candies were eaten");

            Assert.Equal(1, scene.Outcomes().WonCount);
            Assert.Equal(0, scene.Outcomes().LostCount);
        }

        [Fact]
        public void TwoCandiesLosingOneLosesEvenWhileTheOtherIsFine()
        {
            (GameScene scene, CandyContext first, CandyContext second) = TwoCandyScene();

            Act.LoseOffScreen(scene, second);

            Assert.Equal(CandyRemovalReason.OffScreen, second.Lifecycle.RemovalReason);
            Assert.Equal(CandyPresence.Present, first.Lifecycle.Presence);
            Assert.Equal(1, scene.Outcomes().LostCount);
            Assert.Equal(0, scene.Outcomes().WonCount);
        }

        // ---------- helpers ----------

        private static void AssertReachIsToItsOwnCandy(Rocket rocket, CandyContext candy)
        {
            float? reach = rocket.BindReach(candy);
            _ = Assert.NotNull(reach);

            // The leash was sized when the rocket caught the candy, so it matches the separation the
            // pair still has before the reel-in starts closing it.
            float separation = Distance(rocket.point.pos, candy.WholeBody.Point.pos);
            Assert.InRange(reach.Value, 0f, separation + 1f);
        }

        private static float Distance(Vector a, Vector b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            return MathF.Sqrt((dx * dx) + (dy * dy));
        }

        private static int MergeFramesForGap(float meetingGap)
        {
            (GameScene scene, SplitCandyState split) = SplitScene();
            BringHalvesTogether(scene, split, meetingGap);

            CandyContext primary = scene.Candies()[0];
            int frames = 0;
            while (frames < 600 && primary.Lifecycle.Presence != CandyPresence.Present)
            {
                HeadlessGame.StepFrames(scene, 1);
                frames++;
            }

            Assert.Equal(CandyPresence.Present, primary.Lifecycle.Presence);
            return frames;
        }

        /// <summary>
        /// Holds the halves overlapping until the scene starts the merge. They are held a sliver
        /// apart, because the merge closes the gap it measures and one pinned to zero has nothing to
        /// close; the touch test itself reads where they were drawn last frame, so this takes two.
        /// </summary>
        private static void BringHalvesTogether(GameScene scene, SplitCandyState split, float meetingGap)
        {
            Vector meeting = split.Left.Body.Point.pos;
            Assert.True(
                Interaction.StepUntil(
                    scene,
                    () =>
                    {
                        if (split.Phase != SplitPhase.Separate)
                        {
                            return;
                        }

                        Interaction.PlaceBodyAt(split.Left.Body, meeting);
                        Interaction.PlaceBodyAt(split.Right.Body, new Vector(meeting.X + meetingGap, meeting.Y));
                    },
                    () => split.Phase == SplitPhase.Merging),
                "the halves never began merging");
        }

        private static (GameScene Scene, SplitCandyState Split) SplitScene(bool withRocket = false)
        {
            Scenario scenario = Scenario.New().SplitCandy(120, 200, 160, 200);
            if (withRocket)
            {
                scenario = scenario.Rocket(120, 210, angle: 90f, impulse: 20f);
            }

            GameScene scene = scenario.OmNom(30, 440).Build();
            SplitCandyState split = scene.Candies()[0].Lifecycle.Split;
            Interaction.Hover(split.Left.Body);
            Interaction.Hover(split.Right.Body);
            return (scene, split);
        }

        /// <summary>
        /// Two candies with a rocket under each, far enough apart that neither rocket can reach the
        /// other's candy - so a reach measured off the wrong candy is plainly wrong.
        /// </summary>
        private static (GameScene Scene, CandyContext Left, CandyContext Right) TwoRocketScene()
        {
            GameScene scene = Scenario.New()
                .Candy(245, 152, number: "0")
                .Candy(73, 142, number: "1")
                .Rocket(237, 221, angle: 100f, impulse: 20f)
                .Rocket(67, 213, angle: 92f, impulse: 20f)
                .OmNom(274, 423)
                .OmNom(47, 418)
                .Build();
            return (scene, scene.Candies()[1], scene.Candies()[0]);
        }

        private static (GameScene Scene, CandyContext First, CandyContext Second) TwoCandyScene()
        {
            GameScene scene = Scenario.New()
                .Candy(60, 200, number: "1")
                .Candy(260, 200, number: "2")
                .OmNom(20, 460)
                .OmNom(300, 460)
                .Build();
            CandyContext first = scene.Candies()[0];
            CandyContext second = scene.Candies()[1];
            Interaction.Hover(first);
            Interaction.Hover(second);
            return (scene, first, second);
        }
    }
}
