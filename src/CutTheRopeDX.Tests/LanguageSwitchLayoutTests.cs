using System;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers what the crossfade photographs when the language changes. The change rebuilds every
    /// menu, the picker on screen included, and showing the options view afterwards begins by
    /// drawing that picker to capture the screen it fades from.
    /// </summary>
    public sealed class LanguageSwitchLayoutTests
    {
        [Fact]
        public void ThePickerIsPlacedBeforeTheCrossfadePhotographsIt()
        {
            _ = HeadlessGame.Boot();

            RootController root = Application.SharedRootController();
            int restoreTransition = root.viewTransition;

            LayoutSurfaces.WithSurface(720, 1280, () =>
            {
                MenuController controller = new((CTRRootController)root);
                try
                {
                    controller.ShowView(MenuController.VIEW_LANGUAGE_SELECT);

                    // What the real game has on hand when it crossfades between two screens.
                    // Taking the picture is a full draw, which a headless run cannot do, so
                    // reaching the render backend is exactly the moment of capture and what the
                    // controller has done by then is what the picture shows.
                    root.viewTransition = 0;

                    _ = Assert.Throws<NotSupportedException>(
                        () => controller.OnButtonPressed(MenuButtonId.ForLanguage(0)));

                    BaseElement content = FindFittedGroup(
                        controller.GetView(MenuController.VIEW_LANGUAGE_SELECT));
                    Assert.NotNull(content);
                    Assert.True(
                        content.scaleX > 1f && content.x != 0f,
                        $"the picker was photographed unplaced, at {content.x} scaled {content.scaleX}");
                }
                finally
                {
                    root.viewTransition = restoreTransition;
                    controller.Dispose();
                }
            });
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
