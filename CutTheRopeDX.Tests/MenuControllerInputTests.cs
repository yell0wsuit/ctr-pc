using System.Reflection;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public sealed class MenuControllerInputTests
    {
        [Fact]
        public void RepeatedLevelClickKeepsTheFirstPendingSelection()
        {
            _ = HeadlessGame.Boot();
            MenuController controller = new((CTRRootController)Application.SharedRootController());

            try
            {
                controller.PreLevelSelect();
                controller.ShowView(MenuController.VIEW_LEVEL_SELECT);

                controller.OnButtonPressed(MenuButtonId.ForLevel(0));
                controller.OnButtonPressed(MenuButtonId.ForLevel(1));

                FieldInfo field = typeof(MenuController).GetField("level", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.Equal(0, Assert.IsType<int>(field?.GetValue(controller)));
            }
            finally
            {
                controller.Dispose();
            }
        }

        [Fact]
        public void PreparingLevelSelectionAllowsANewSelection()
        {
            _ = HeadlessGame.Boot();
            MenuController controller = new((CTRRootController)Application.SharedRootController());

            try
            {
                controller.PreLevelSelect();
                controller.ShowView(MenuController.VIEW_LEVEL_SELECT);
                controller.OnButtonPressed(MenuButtonId.ForLevel(0));

                controller.PreLevelSelect();
                controller.ShowView(MenuController.VIEW_LEVEL_SELECT);
                controller.OnButtonPressed(MenuButtonId.ForLevel(1));

                FieldInfo field = typeof(MenuController).GetField("level", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.Equal(1, Assert.IsType<int>(field?.GetValue(controller)));
            }
            finally
            {
                controller.Dispose();
            }
        }
    }
}
