using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.GameMain;
using CutTheRopeDX.GameMain.Tutorials;
using CutTheRopeDX.Tests.Interactions;

using Xunit;

namespace CutTheRopeDX.Tests.Tutorials
{
    public sealed class TutorialSceneIntegrationTests
    {
        private const BindingFlags Instance = BindingFlags.Instance | BindingFlags.NonPublic;

        [Fact]
        public void SceneHasExactlyOneTutorialOwnerAndNoLegacyFieldsOrNestedVisuals()
        {
            FieldInfo[] fields = typeof(GameScene).GetFields(Instance);
            FieldInfo director = Assert.Single(fields, field => field.FieldType == typeof(TutorialDirector));

            Assert.Equal("tutorialDirector", director.Name);
            Assert.DoesNotContain(fields, field => field.Name is "special" or "tutorials" or "tutorialImages");
            Assert.DoesNotContain(
                typeof(GameScene).GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic),
                type => type.Name is "TutorialText" or "GameObjectSpecial" or "TutorialSign");
        }

        [Fact]
        public void SceneLoadsTutorialsAsOneOrderedBatchBeforeStartFires()
        {
            GameScene scene = Scenario.New()
                .Candy(100, 100)
                .OmNom(160, 300)
                .TutorialText(80, 80, attributes: [new XAttribute("group", "intro")])
                .TutorialImage(4, 120, 80, new XAttribute("group", "intro"))
                .Build();

            IReadOnlyList<TutorialPrompt> prompts = scene.TutorialPrompts();
            Assert.Equal(2, prompts.Count);
            Assert.Equal(TutorialPromptState.Playing, prompts[0].State);
            Assert.Equal(TutorialPromptState.Cancelled, prompts[1].State);
        }

        [Fact]
        public void SampledCandyStatesReadTheirAuthoritativeLifecycleOwners()
        {
            AssertSampledCandyState("bubbled", (scene, candy) => candy.WholeBody.Bubble = new GameObject());
            AssertSampledCandyState("inLantern", (_, candy) => candy.Lifecycle.Attachments.CaptureInLantern());
            AssertSampledCandyState(
                "carriedByAnt",
                (_, candy) => candy.Lifecycle.Attachments.BeginAntCarry(
                    new AntsPathSegment(new Vector(0f, 0f), new Vector(10f, 0f), 1f, 1f),
                    candy.WholeBody.Point.pos,
                    0.3f,
                    0.01f));
            AssertSampledCandyState(
                "carriedBySnail",
                (scene, candy) => scene.Snails()[0].AttachToPoint(candy.WholeBody.Point),
                scenario => scenario.Snail(20, 40));
        }

        [Fact]
        public void SampledSceneStatesReadTimeAndGravityOwners()
        {
            GameScene timeScene = Scenario.New()
                .Candy(100, 100)
                .OmNom(160, 300)
                .PauseSwitcher(60, 440)
                .TutorialText(80, 80, attributes: [new XAttribute("showOn", "timeFrozen")])
                .Build();
            Vector button = timeScene.ScreenPositionOf(timeScene.PauseSwitchers()[0]);
            _ = timeScene.TouchDownXYIndex(button.X, button.Y, 0);
            _ = timeScene.TouchUpXYIndex(button.X, button.Y, 0);
            timeScene.TutorialDirector().Update(0f);

            Assert.True(timeScene.IsTimeFrozen());
            Assert.Equal(TutorialPromptState.Playing, Assert.Single(timeScene.TutorialPrompts()).State);

            GameScene gravityScene = Scenario.New()
                .Candy(100, 100)
                .OmNom(160, 300)
                .TutorialText(80, 80, attributes: [new XAttribute("showOn", "gravityInverted")])
                .Build();
            gravityScene.OnButtonPressed(GameSceneButtonId.GravityToggle);
            gravityScene.TutorialDirector().Update(0f);

            Assert.True(gravityScene.gravityState.IsInverted);
            Assert.Equal(TutorialPromptState.Playing, Assert.Single(gravityScene.TutorialPrompts()).State);
        }

        [Fact]
        public void CandyMovedRequiresTheSelectedBodyInsideItsAuthoredRegion()
        {
            GameScene scene = Scenario.New()
                .Candy(100, 100)
                .OmNom(160, 300)
                .TutorialText(
                    80,
                    80,
                    attributes:
                    [
                        new XAttribute("showOn", "candyMoved"),
                        new XAttribute("inArea", "90,90,20,20"),
                    ])
                .Build();

            scene.TutorialDirector().Update(0f);

            Assert.Equal(TutorialPromptState.Playing, Assert.Single(scene.TutorialPrompts()).State);
        }

        [Fact]
        public void SplitSampledStatesSelectTheAuthoredLeftOrRightBody()
        {
            GameScene scene = Scenario.New()
                .SplitCandy(100, 100, 200, 100)
                .OmNom(160, 300)
                .TutorialText(
                    80,
                    80,
                    attributes:
                    [
                        new XAttribute("showOn", "bubbled"),
                        new XAttribute("subject", "left"),
                    ])
                .TutorialText(
                    80,
                    100,
                    attributes:
                    [
                        new XAttribute("showOn", "bubbled"),
                        new XAttribute("subject", "right"),
                    ])
                .Build();
            scene.Candy().Lifecycle.Split.Right.Body.Bubble = new GameObject();

            scene.TutorialDirector().Update(0f);

            IReadOnlyList<TutorialPrompt> prompts = scene.TutorialPrompts();
            Assert.Equal(TutorialPromptState.Armed, prompts[0].State);
            Assert.Equal(TutorialPromptState.Playing, prompts[1].State);
        }

        private static void AssertSampledCandyState(
            string showOn,
            System.Action<GameScene, CandyContext> establish,
            System.Func<Scenario, Scenario> configure = null)
        {
            Scenario scenario = Scenario.New()
                .Candy(100, 100)
                .OmNom(160, 300)
                .TutorialText(80, 80, attributes: [new XAttribute("showOn", showOn)]);
            GameScene scene = (configure?.Invoke(scenario) ?? scenario).Build();

            establish(scene, scene.Candy());
            scene.TutorialDirector().Update(0f);

            Assert.Equal(TutorialPromptState.Playing, Assert.Single(scene.TutorialPrompts()).State);
        }
    }
}
