using System.Collections.Generic;
using System.Linq;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;
using CutTheRopeDX.GameMain.Tutorials;
using CutTheRopeDX.Tests.Interactions;

using Xunit;

namespace CutTheRopeDX.Tests.Tutorials
{
    /// <summary>
    /// The four shipped levels that author a triggered tutorial prompt, loaded from their real
    /// maps and driven through the real interaction. These assert the state a prompt reaches
    /// rather than anything about drawing: the fade a prompt plays is timeline state, and
    /// advancing it inside <c>Draw()</c> would wedge headless silently.
    /// </summary>
    public sealed class TutorialShippedLevelTests
    {
        private const int SwipeLevelPack = 0;
        private const int SwipeLevelIndex = 0;
        private const int BubbleRegionLevelPack = 0;
        private const int BubbleRegionLevelIndex = 4;
        private const int LanternLevelPack = 13;
        private const int LanternLevelIndex = 0;
        private const int MouseLevelPack = 14;
        private const int MouseLevelIndex = 0;

        /// <summary>1_5: the two untriggered prompts play at once; the bubbled pair waits.</summary>
        [Fact]
        public void BubbleRegionLevelStagesItsTriggeredPromptsIndependently()
        {
            GameScene scene = LoadBubbleRegionLevel();

            Assert.All(
                scene.TutorialPromptsFor(TutorialEvent.Start),
                prompt => Assert.Equal(TutorialPromptState.Playing, prompt.State));
            Assert.All(
                scene.TutorialPromptsFor(TutorialEvent.Bubbled),
                prompt => Assert.Equal(TutorialPromptState.Armed, prompt.State));
            Assert.Equal(2, scene.TutorialPromptsFor(TutorialEvent.Bubbled).Count);
        }

        /// <summary>1_5: both region prompts appear together once the bubbled candy is inside it.</summary>
        [Fact]
        public void BubbleRegionLevelFiresBothPromptsOnlyInsideTheAuthoredRegion()
        {
            GameScene scene = LoadBubbleRegionLevel();
            CandyContext candy = scene.Candy();
            Interaction.Hover(candy);
            _ = Act.CaptureInBubble(scene, candy);

            // Bubbled alone is not the condition: the candy is still where the bubble caught it.
            Assert.All(
                scene.TutorialPromptsFor(TutorialEvent.Bubbled),
                prompt => Assert.Equal(TutorialPromptState.Armed, prompt.State));

            Interaction.PlaceCandyAt(candy, RegionCenter(scene));
            HeadlessGame.StepFrames(scene, 1);

            Assert.All(
                scene.TutorialPromptsFor(TutorialEvent.Bubbled),
                prompt => Assert.Equal(TutorialPromptState.Playing, prompt.State));
        }

        /// <summary>1_1: the swipe runs its own preset while the rest hold for the authored ten seconds.</summary>
        [Fact]
        public void SwipeLevelStartsItsSwipeAtOnceAndKeepsTheTenSecondEnvelope()
        {
            GameScene scene = Load(SwipeLevelPack, SwipeLevelIndex);
            List<TutorialPrompt> prompts = [.. scene.TutorialPrompts()];
            TutorialPrompt swipe = Assert.Single(prompts, prompt => prompt.TimelineIndex == 1);

            Assert.All(prompts, prompt => Assert.Equal(TutorialPromptState.Playing, prompt.State));
            Assert.Equal(10f, swipe.Visual.rotation);
            Assert.Equal(
                Timeline.TimelineState.TIMELINE_PLAYING,
                swipe.Visual.GetTimeline(1).state);

            foreach (TutorialPrompt prompt in prompts.Where(prompt => prompt != swipe))
            {
                Assert.Equal(1f, prompt.FadeIn);
                Assert.Equal(10f, prompt.Hold);
                Assert.Equal(0.5f, prompt.FadeOut);
                Assert.Equal([0f, 1f, 10f, 0.5f], prompt.ColorKeyFrameTimes(0));
            }
        }

        /// <summary>1_1: a prompt fades up over its authored fade-in rather than snapping on.</summary>
        [Fact]
        public void SwipeLevelPromptsFadeUpOverTheirAuthoredFadeIn()
        {
            GameScene scene = Load(SwipeLevelPack, SwipeLevelIndex);
            TutorialPrompt prompt = scene.TutorialPrompts().First(candidate => candidate.TimelineIndex == 0);

            Assert.Equal(0f, prompt.Alpha());
            HeadlessGame.StepFrames(scene, 30);

            Assert.InRange(prompt.Alpha(), 0.3f, 0.7f);
        }

        /// <summary>14_1: the second prompt waits for the lantern to take the candy.</summary>
        [Fact]
        public void LanternLevelFiresItsSecondPromptOnCapture()
        {
            GameScene scene = Load(LanternLevelPack, LanternLevelIndex);
            TutorialPrompt triggered = scene.TutorialPromptFor(TutorialEvent.LanternCatch);

            Assert.Equal(TutorialPromptState.Playing, scene.TutorialPromptFor(TutorialEvent.Start).State);
            Assert.Equal(TutorialPromptState.Armed, triggered.State);

            CandyContext candy = scene.Candy();
            Interaction.Hover(candy);
            Act.CaptureInLantern(scene, candy);

            Assert.Equal(TutorialPromptState.Playing, triggered.State);
        }

        /// <summary>15_1: both mouse prompts appear together when the mouse takes the candy.</summary>
        [Fact]
        public void MouseLevelFiresBothPromptsOnGrab()
        {
            GameScene scene = Load(MouseLevelPack, MouseLevelIndex);
            List<TutorialPrompt> triggered = scene.TutorialPromptsFor(TutorialEvent.MouseGrab);

            Assert.Equal(2, triggered.Count);
            Assert.All(triggered, prompt => Assert.Equal(TutorialPromptState.Armed, prompt.State));
            Assert.All(
                scene.TutorialPromptsFor(TutorialEvent.Start),
                prompt => Assert.Equal(TutorialPromptState.Playing, prompt.State));

            CandyContext candy = scene.Candy();
            Interaction.Hover(candy);
            _ = Act.CarryByMouse(scene, candy);

            Assert.All(triggered, prompt => Assert.Equal(TutorialPromptState.Playing, prompt.State));
        }

        /// <summary>A restart rebuilds every prompt from XML, so a fired trigger re-arms.</summary>
        [Fact]
        public void RestartRebuildsAndReArmsEveryPrompt()
        {
            GameScene scene = Load(LanternLevelPack, LanternLevelIndex);
            TutorialPrompt before = scene.TutorialPromptFor(TutorialEvent.LanternCatch);
            CandyContext candy = scene.Candy();
            Interaction.Hover(candy);
            Act.CaptureInLantern(scene, candy);
            Assert.Equal(TutorialPromptState.Playing, before.State);

            scene.Restart();

            TutorialPrompt after = scene.TutorialPromptFor(TutorialEvent.LanternCatch);
            Assert.NotSame(before, after);
            Assert.Equal(TutorialPromptState.Armed, after.State);
            Assert.Equal(TutorialPromptState.Playing, scene.TutorialPromptFor(TutorialEvent.Start).State);
        }

        private static GameScene LoadBubbleRegionLevel()
        {
            return Load(BubbleRegionLevelPack, BubbleRegionLevelIndex);
        }

        /// <summary>Boots the engine and loads one shipped level.</summary>
        /// <param name="pack">Zero-based pack index.</param>
        /// <param name="level">Zero-based level index.</param>
        /// <returns>The loaded scene.</returns>
        private static GameScene Load(int pack, int level)
        {
            _ = HeadlessGame.Boot();
            return HeadlessGame.LoadLevel(pack, level);
        }

        /// <summary>The middle of the authored region, in world coordinates.</summary>
        /// <param name="scene">Scene whose prompts carry the region.</param>
        /// <returns>The region's center.</returns>
        private static Vector RegionCenter(GameScene scene)
        {
            TutorialArea area = scene.TutorialPromptsFor(TutorialEvent.Bubbled)[0].Trigger.Area.Value;
            return new Vector(area.X + (area.Width / 2f), area.Y + (area.Height / 2f));
        }
    }
}
