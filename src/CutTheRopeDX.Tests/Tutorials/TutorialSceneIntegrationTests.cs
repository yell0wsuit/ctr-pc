using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;

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
    }
}
