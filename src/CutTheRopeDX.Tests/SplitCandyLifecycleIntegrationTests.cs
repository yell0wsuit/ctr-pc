using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Physics;
using CutTheRopeDX.GameMain;
using CutTheRopeDX.Tests.Interactions;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// A split level is one logical candy that owns two physical halves, not a scene-wide pair of
    /// singleton flags. These load the shipped split level 5_10 and drive the real update loop, so
    /// the lifecycle they assert on is the one the loader and merge code actually built.
    /// </summary>
    public sealed class SplitCandyLifecycleIntegrationTests
    {
        /// <summary>World Y far past the kill line, for the tests that push a half out of play.</summary>
        private const float BelowTheWorld = 4000f;

        [Fact]
        public void SplitLevelLoadsOneLogicalCandyThatIsSplitAndSeparate()
        {
            (GameScene _, CandyContext primary) = SplitLevel();

            Assert.Equal(CandyPresence.Split, primary.Lifecycle.Presence);
            Assert.NotNull(primary.Lifecycle.Split);
            Assert.Equal(SplitPhase.Separate, primary.Lifecycle.Split.Phase);
        }

        [Fact]
        public void SplitLevelGivesEachHalfItsOwnBody()
        {
            (GameScene _, CandyContext primary) = SplitLevel();
            SplitCandyState split = primary.Lifecycle.Split;

            Assert.NotSame(split.Left.Body, split.Right.Body);
            Assert.NotSame(split.Left.Body.Point, split.Right.Body.Point);
            Assert.Equal(CandyBodyRole.LeftHalf, split.Left.Body.Role);
            Assert.Equal(CandyBodyRole.RightHalf, split.Right.Body.Role);
            Assert.Equal([split.Left.Body, split.Right.Body], primary.Lifecycle.ActiveBodies);
        }

        [Fact]
        public void SplitLevelDoesNotOfferTheDormantWholeBodyWhileSplit()
        {
            (GameScene _, CandyContext primary) = SplitLevel();

            Assert.DoesNotContain(primary.WholeBody, primary.Lifecycle.ActiveBodies);
        }

        [Fact]
        public void TouchingIntactHalvesMergeBackIntoOnePresentWholeCandy()
        {
            (GameScene scene, CandyContext primary) = SplitLevel();

            PushHalvesTogether(scene, primary);

            Assert.True(
                Interaction.StepUntil(scene, () => primary.Lifecycle.Presence == CandyPresence.Present),
                "the two halves never merged");
            Assert.Null(primary.Lifecycle.Split);
            Assert.Equal([primary.WholeBody], primary.Lifecycle.ActiveBodies);
        }

        [Fact]
        public void TouchingIntactHalvesEnterMergingBeforeTheyMerge()
        {
            (GameScene scene, CandyContext primary) = SplitLevel();

            PushHalvesTogether(scene, primary);

            Assert.True(
                Interaction.StepUntil(
                    scene,
                    () => primary.Lifecycle.Presence == CandyPresence.Present
                        || primary.Lifecycle.Split.Phase == SplitPhase.Merging),
                "the halves never began merging");
        }

        [Fact]
        public void AHalfLostOffScreenRecordsTheReasonOnThatHalfOnly()
        {
            (GameScene scene, CandyContext primary) = SplitLevel();
            SplitCandyState split = primary.Lifecycle.Split;

            DropHalfOffScreen(scene, split.Left);

            Assert.Equal(CandyRemovalReason.OffScreen, split.Left.RemovalReason);
            Assert.Null(split.Right.RemovalReason);
            Assert.True(primary.Lifecycle.HasFailedSplitHalf);
            Assert.False(primary.Lifecycle.WasEaten);
        }

        [Fact]
        public void AHalfLostOffScreenLeavesOnlyTheSurvivingBodyActive()
        {
            (GameScene scene, CandyContext primary) = SplitLevel();
            SplitCandyState split = primary.Lifecycle.Split;

            DropHalfOffScreen(scene, split.Left);

            Assert.Equal([split.Right.Body], primary.Lifecycle.ActiveBodies);
        }

        [Fact]
        public void AHalfLostOffScreenReleasesOnlyItsOwnBubbleAndRopes()
        {
            (GameScene scene, CandyContext primary) = SplitLevel();
            SplitCandyState split = primary.Lifecycle.Split;
            Assert.True(
                Interaction.StepUntil(
                    scene,
                    () => split.Left.Body.Bubble != null || split.Right.Body.Bubble != null),
                "no half ever picked up one of the level's bubbles");
            CandyHalf removed = split.Left.Body.Bubble != null ? split.Left : split.Right;
            CandyHalf surviving = removed == split.Left ? split.Right : split.Left;
            int survivingRopes = scene.AttachedRopeCount(surviving.Body);
            Assert.True(scene.AttachedRopeCount(removed.Body) > 0);
            Assert.True(survivingRopes > 0);

            DropHalfOffScreen(scene, removed);

            Assert.Null(removed.Body.Bubble);
            Assert.Equal(0, scene.AttachedRopeCount(removed.Body));
            Assert.Null(surviving.RemovalReason);
            Assert.Equal(survivingRopes, scene.AttachedRopeCount(surviving.Body));
        }

        [Fact]
        public void ALostHalfBlocksTheMerge()
        {
            (GameScene scene, CandyContext primary) = SplitLevel();
            SplitCandyState split = primary.Lifecycle.Split;
            DropHalfOffScreen(scene, split.Left);

            Assert.False(split.TryBeginMerge(100f));
            Assert.False(primary.Lifecycle.TryCompleteMerge());
            Assert.Equal(CandyPresence.Split, primary.Lifecycle.Presence);
        }

        [Fact]
        public void BothHalvesLostLoseTheLevelOnceAndNeverWin()
        {
            (GameScene scene, CandyContext primary) = SplitLevel();
            SplitCandyState split = primary.Lifecycle.Split;

            DropHalfOffScreen(scene, split.Left);
            DropHalfOffScreen(scene, split.Right);
            HeadlessGame.StepFrames(scene, 60);

            Assert.Empty(primary.Lifecycle.ActiveBodies);
            Assert.Equal(1, scene.Outcomes().LostCount);
            Assert.Equal(0, scene.Outcomes().WonCount);
            Assert.False(CandyDecisions.AllEaten([primary.ToOutcomeView()]));
            Assert.True(CandyDecisions.AnyFailedRemoval([primary.ToOutcomeView()]));
        }

        [Fact]
        public void SplitLevelOffersBothHalfBodiesToSceneSystems()
        {
            (GameScene scene, CandyContext primary) = SplitLevel();
            SplitCandyState split = primary.Lifecycle.Split;

            Assert.Equal([split.Left.Body, split.Right.Body], scene.ActiveBodies());
        }

        [Fact]
        public void SplitLevelResolvesEachHalfPointBackToItsOwnBodyAndLogicalCandy()
        {
            (GameScene scene, CandyContext primary) = SplitLevel();
            SplitCandyState split = primary.Lifecycle.Split;

            CandyBody left = scene.BodyForPoint(split.Left.Body.Point);
            CandyBody right = scene.BodyForPoint(split.Right.Body.Point);

            Assert.Same(split.Left.Body, left);
            Assert.Same(split.Right.Body, right);
            Assert.Same(primary, left.Owner);
            Assert.Same(primary, right.Owner);
        }

        [Fact]
        public void SplitLevelNeverOffersTheDormantWholeBodyToSceneSystems()
        {
            (GameScene scene, CandyContext primary) = SplitLevel();

            Assert.DoesNotContain(primary.WholeBody, scene.ActiveBodies());
            Assert.Null(scene.BodyForPoint(primary.WholeBody.Point));
        }

        [Fact]
        public void ALostHalfLeavesOnlyTheSurvivorInTheSceneBodyEnumerator()
        {
            (GameScene scene, CandyContext primary) = SplitLevel();
            SplitCandyState split = primary.Lifecycle.Split;

            DropHalfOffScreen(scene, split.Left);

            Assert.Equal([split.Right.Body], scene.ActiveBodies());
            Assert.Null(scene.BodyForPoint(split.Left.Body.Point));
        }

        /// <summary>
        /// 5_10 floats a bubble right on top of its right half. The capture has to land on that half's
        /// own body, not on a scene-level split-bubble slot, or a half's bubble is invisible to every
        /// system that reads bodies.
        /// </summary>
        [Fact]
        public void ABubbleOverAHalfIsStoredOnThatHalfsOwnBody()
        {
            (GameScene scene, CandyContext primary) = SplitLevel();
            SplitCandyState split = primary.Lifecycle.Split;

            Assert.True(
                Interaction.StepUntil(
                    scene,
                    () => split.Left.Body.Bubble != null || split.Right.Body.Bubble != null),
                "no half ever picked up one of the level's bubbles");
            Assert.Null(primary.WholeBody.Bubble);
        }

        [Fact]
        public void SplitLevelKeepsEachHalfsVisualOnItsOwnBody()
        {
            (GameScene _, CandyContext primary) = SplitLevel();
            SplitCandyState split = primary.Lifecycle.Split;

            Assert.NotNull(split.Left.Body.Visual);
            Assert.NotNull(split.Left.Body.BubbleAnimation);
            Assert.NotNull(split.Left.Body.GhostBubbleAnimation);
            Assert.NotSame(split.Left.Body.Visual, split.Right.Body.Visual);
            Assert.NotSame(split.Left.Body.BubbleAnimation, split.Right.Body.BubbleAnimation);
            Assert.NotSame(split.Left.Body.GhostBubbleAnimation, split.Right.Body.GhostBubbleAnimation);
        }

        /// <summary>
        /// Holds the two halves on top of each other until the scene notices they touch. They hang on
        /// their own ropes, so a single placement springs apart before the intersection test runs.
        /// </summary>
        private static void PushHalvesTogether(GameScene scene, CandyContext primary)
        {
            SplitCandyState split = primary.Lifecycle.Split;
            Vector meetingPoint = split.Left.Body.Point.pos;
            _ = Interaction.StepUntil(
                scene,
                () =>
                {
                    if (primary.Lifecycle.Presence != CandyPresence.Split)
                    {
                        return;
                    }

                    PinPointAt(split.Right.Body.Point, meetingPoint);
                    PinPointAt(split.Left.Body.Point, meetingPoint);
                },
                () => primary.Lifecycle.Presence == CandyPresence.Present
                    || split.Phase == SplitPhase.Merging);
        }

        /// <summary>Pushes one half past the kill line and waits for the scene to retire it.</summary>
        private static void DropHalfOffScreen(GameScene scene, CandyHalf half)
        {
            half.Body.Point.disableGravity = false;
            PinPointAt(half.Body.Point, new Vector(half.Body.Point.pos.X, BelowTheWorld));

            Assert.True(
                Interaction.StepUntil(scene, () => half.RemovalReason != null),
                "the half that left the screen was never removed");
        }

        private static void PinPointAt(ConstraintedPoint point, Vector position)
        {
            point.pos = position;
            point.prevPos = position;
            point.v = new Vector(0f, 0f);
            point.posDelta = new Vector(0f, 0f);
        }

        private static (GameScene Scene, CandyContext Primary) SplitLevel()
        {
            _ = HeadlessGame.Boot();
            GameScene scene = HeadlessGame.LoadLevel(pack: 4, level: 9);
            scene.gameSceneDelegate = new RecordingSceneDelegate();
            return (scene, scene.Candies()[0]);
        }
    }
}
