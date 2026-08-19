using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers the options view being recreated on its own. A language change rebuilds it a frame
    /// after the layout pass that would have placed it, so it has to place itself.
    /// </summary>
    public sealed class OptionsRecreateTests
    {
        [Fact]
        public void ARecreatedOptionsViewIsPlacedWhereAShownOneIs()
        {
            _ = HeadlessGame.Boot();

            LayoutSurfaces.WithSurface(720, 1280, () =>
            {
                MenuController controller = new(
                    (CTRRootController)Application.SharedRootController());
                try
                {
                    controller.ShowView(MenuController.VIEW_OPTIONS);
                    BaseElement shown = OptionsContent(controller);
                    (float x, float y, float scale) = (shown.x, shown.y, shown.scaleX);
                    Assert.True(scale > 1f, "the fixture viewport should scale the options content");

                    controller.RecreateOptions();

                    BaseElement rebuilt = OptionsContent(controller);
                    Assert.Equal(x, rebuilt.x, 0.01);
                    Assert.Equal(y, rebuilt.y, 0.01);
                    Assert.Equal(scale, rebuilt.scaleX, 0.0001);
                }
                finally
                {
                    controller.Dispose();
                }
            });
        }

        /// <summary>The options view's design-space content.</summary>
        /// <param name="controller">Controller owning the view.</param>
        /// <returns>The fitted group holding the options content.</returns>
        private static BaseElement OptionsContent(MenuController controller)
        {
            BaseElement found = FindFittedGroup(controller.GetView(MenuController.VIEW_OPTIONS));
            Assert.NotNull(found);
            return found;
        }

        /// <summary>Returns the first fitted group in an element tree.</summary>
        /// <param name="element">Element to search from.</param>
        /// <returns>The group, or <see langword="null"/> when the tree holds none.</returns>
        private static BaseElement FindFittedGroup(BaseElement element)
        {
            if (element is FittedGroup group)
            {
                return group;
            }

            foreach (BaseElement child in element.GetChilds().Values)
            {
                BaseElement found = child == null ? null : FindFittedGroup(child);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}
