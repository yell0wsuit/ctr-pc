using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Pins every headlessly constructible menu view's element geometry at every surface size in
    /// the matrix. A later change that moves, resizes or rescales an element appears as a golden
    /// file diff.
    /// </summary>
    public sealed class MenuLayoutCharacterizationTests
    {
        [Theory]
        [MemberData(nameof(Cases))]
        public void MenuViewGeometryIsPinned(
            string surfaceName,
            int width,
            int height,
            int viewId,
            string viewName)
        {
            _ = HeadlessGame.Boot();

            LayoutSurfaces.WithSurface(width, height, () =>
            {
                MenuController controller = new(
                    (CTRRootController)Application.SharedRootController());
                try
                {
                    if (viewId == MenuController.VIEW_LEVEL_SELECT)
                    {
                        controller.PreLevelSelect();
                    }

                    View view = controller.GetView(viewId);
                    Assert.NotNull(view);

                    controller.ShowView(viewId);
                    controller.Update(0.016f);

                    string described = ElementGeometryWalker.Describe(view);
                    Assert.Contains('\n', described);
                    LayoutBaseline.Assert($"Menu.{viewName}.{surfaceName}", described);
                }
                finally
                {
                    controller.Dispose();
                }
            });
        }

        public static TheoryData<string, int, int, int, string> Cases()
        {
            TheoryData<string, int, int, int, string> data = [];
            foreach (LayoutSurface surface in LayoutSurfaces.All)
            {
                data.Add(surface.Name, surface.Width, surface.Height, MenuController.VIEW_MAIN_MENU, "MainMenu");
                data.Add(surface.Name, surface.Width, surface.Height, MenuController.VIEW_OPTIONS, "Options");
                data.Add(surface.Name, surface.Width, surface.Height, MenuController.VIEW_ABOUT, "About");
                data.Add(surface.Name, surface.Width, surface.Height, MenuController.VIEW_RESET, "Reset");
                data.Add(surface.Name, surface.Width, surface.Height, MenuController.VIEW_PACK_SELECT, "PackSelect");
                data.Add(surface.Name, surface.Width, surface.Height, MenuController.VIEW_LEVEL_SELECT, "LevelSelect");
                data.Add(surface.Name, surface.Width, surface.Height, MenuController.VIEW_MOVIE, "Movie");
                data.Add(surface.Name, surface.Width, surface.Height, MenuController.VIEW_CANDY_SELECT, "CandySelect");
                data.Add(surface.Name, surface.Width, surface.Height, MenuController.VIEW_LANGUAGE_SELECT, "LanguageSelect");
            }
            return data;
        }
    }
}
