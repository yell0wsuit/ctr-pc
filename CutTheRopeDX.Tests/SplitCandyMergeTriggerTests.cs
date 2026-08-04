using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.GameMain;
using CutTheRopeDX.Tests.Interactions;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// The split halves begin merging off the positions their visuals were last drawn at, one frame
    /// behind their physics points.
    /// </summary>
    public sealed class SplitCandyMergeTriggerTests
    {
        [Fact]
        public void HalvesMovedOntoEachOtherDoNotBeginMergingOnThatSameFrame()
        {
            (GameScene scene, SplitCandyState split) = SeparatedHalves();

            Overlap(split);
            HeadlessGame.StepFrames(scene, 1);

            Assert.Equal(SplitPhase.Separate, split.Phase);
        }

        [Fact]
        public void HalvesMovedOntoEachOtherBeginMergingOnTheFollowingFrame()
        {
            (GameScene scene, SplitCandyState split) = SeparatedHalves();

            Overlap(split);
            HeadlessGame.StepFrames(scene, 1);
            Overlap(split);
            HeadlessGame.StepFrames(scene, 1);

            Assert.Equal(SplitPhase.Merging, split.Phase);
        }

        /// <summary>
        /// The gap the merge records is measured from the live points, not from the frame-old visuals,
        /// matching the reference engine's <c>vectDistance(starL.pos, starR.pos)</c>.
        /// </summary>
        [Fact]
        public void TheRecordedGapComesFromTheLivePointsNotTheStaleVisuals()
        {
            (GameScene scene, SplitCandyState split) = SeparatedHalves();

            Overlap(split);
            HeadlessGame.StepFrames(scene, 1);
            Overlap(split);
            HeadlessGame.StepFrames(scene, 1);

            Assert.Equal(SplitPhase.Merging, split.Phase);
            Assert.Equal(OverlapGap, split.MergeDistance, precision: 1);
        }

        /// <summary>World gap the halves are held at while overlapping: well inside their hitboxes.</summary>
        private const float OverlapGap = 2f;

        /// <summary>
        /// A split candy with no ropes, both halves held against gravity and parked far enough apart
        /// that their hitboxes cannot touch.
        /// </summary>
        private static (GameScene Scene, SplitCandyState Split) SeparatedHalves()
        {
            GameScene scene = Scenario.New()
                .SplitCandy(100, 200, 200, 200)
                .OmNom(20, 460)
                .Build();
            SplitCandyState split = scene.Candies()[0].Lifecycle.Split;

            Interaction.Hover(split.Left.Body);
            Interaction.Hover(split.Right.Body);
            PlaceApart(split);

            // Let the scene sync and top-left both visuals where they now stand, so the merge test
            // starts from a known "last drawn far apart" state.
            HeadlessGame.StepFrames(scene, 2);
            Assert.Equal(SplitPhase.Separate, split.Phase);
            return (scene, split);
        }

        private static void PlaceApart(SplitCandyState split)
        {
            Interaction.PlaceBodyAt(split.Left.Body, new Vector(1000f, 600f));
            Interaction.PlaceBodyAt(split.Right.Body, new Vector(1600f, 600f));
        }

        private static void Overlap(SplitCandyState split)
        {
            Interaction.PlaceBodyAt(split.Left.Body, new Vector(1300f, 600f));
            Interaction.PlaceBodyAt(split.Right.Body, new Vector(1300f + OverlapGap, 600f));
        }
    }
}
