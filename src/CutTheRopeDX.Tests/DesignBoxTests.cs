using System.Reflection;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Visual;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers the elastic design box and the group that carries design-space content, including
    /// the one property every scene's authored constants depend on: a child authored at x is drawn
    /// at the fitted box's origin plus x times the fit scale.
    /// </summary>
    public sealed class DesignBoxTests
    {
        [Fact]
        public void TheDesignBoxIsTheDesignSizeAtTheDesignAspect()
        {
            LayoutSurfaces.WithSurface(2560, 1440, () =>
            {
                CTRRectangle box = ReadDesignBox();

                Assert.Equal(2560f, box.w, 0.01);
                Assert.Equal(1440f, box.h, 0.01);
            });

            // The same ratio at a different size must give the same box.
            LayoutSurfaces.WithSurface(1280, 720, () =>
            {
                CTRRectangle box = ReadDesignBox();

                Assert.Equal(2560f, box.w, 0.01);
                Assert.Equal(1440f, box.h, 0.01);
            });
        }

        [Fact]
        public void ANarrowerViewportGetsATallerBoxThatFillsIt()
        {
            LayoutSurfaces.WithSurface(720, 1280, () =>
            {
                CTRRectangle box = ReadDesignBox();

                // Width-normalized below the design aspect: 2560 / (1440/2560).
                Assert.Equal(2560f, box.w, 0.01);
                Assert.Equal(4551.11f, box.h, 0.01);

                // Which means no slack at all: the fitted box is the whole viewport.
                Assert.Equal(1440f, ReadFittedBox().w, 0.01);
                Assert.Equal(2560f, ReadFittedBox().h, 0.01);
            });
        }

        [Fact]
        public void AWiderViewportGetsAShorterBoxAndZoomsIn()
        {
            float designScale = 0f;
            LayoutSurfaces.WithSurface(2560, 1440, () => designScale = ReadFittedScale());

            LayoutSurfaces.WithSurface(3840, 1080, () =>
            {
                CTRRectangle box = ReadDesignBox();

                Assert.Equal(2560f, box.w, 0.01);
                Assert.True(box.h < 1440f, $"expected a shorter box, got {box.h}");

                // A shorter box raises the fit scale, which is what zooms the composition in
                // rather than only spreading it out.
                Assert.True(
                    ReadFittedScale() > designScale,
                    $"expected to zoom in past {designScale}, got {ReadFittedScale()}");
            });
        }

        [Theory]
        [MemberData(nameof(Surfaces))]
        public void AChildOfTheFittedGroupLandsWhereTheFitPutsIt(string name, int width, int height)
        {
            LayoutSurfaces.WithSurface(width, height, () =>
            {
                BaseElement group = new();
                InvokePlaceFittedGroup(group);

                // 9 = anchored to the parent's left and top edges.
                BaseElement child = new() { parentAnchor = 9, x = 912f, y = 998f };
                _ = group.AddChild(child);
                BaseElement.CalculateTopLeft(group);
                BaseElement.CalculateTopLeft(child);

                // What the renderer does: scale about the group's own centre, which PreDraw
                // computes with an integer shift.
                float centreX = group.drawX + (group.width >> 1);
                float centreY = group.drawY + (group.height >> 1);
                float drawnX = centreX + ((child.drawX - centreX) * group.scaleX);
                float drawnY = centreY + ((child.drawY - centreY) * group.scaleY);

                CTRRectangle fitted = ReadFittedBox();
                float scale = ReadFittedScale();

                Assert.Equal(fitted.x + (912f * scale), drawnX, 0.01);
                Assert.Equal(fitted.y + (998f * scale), drawnY, 0.01);
                Assert.False(string.IsNullOrEmpty(name));
            });
        }

        [Fact]
        public void TheFittedGroupIsTheIdentityAtTheDesignAspect()
        {
            LayoutSurfaces.WithSurface(2560, 1440, () =>
            {
                BaseElement group = new();
                InvokePlaceFittedGroup(group);

                Assert.Equal(0f, group.x, 0.01);
                Assert.Equal(0f, group.y, 0.01);
                Assert.Equal(1f, group.scaleX, 0.001);
                Assert.Equal(2560, group.width);
                Assert.Equal(1440, group.height);
            });
        }

        private static ProbeController Probe()
        {
            _ = HeadlessGame.Boot();
            return new ProbeController();
        }

        private static CTRRectangle ReadDesignBox()
        {
            return (CTRRectangle)typeof(ViewController)
                .GetProperty("DesignBox", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(Probe());
        }

        private static CTRRectangle ReadFittedBox()
        {
            return (CTRRectangle)typeof(ViewController)
                .GetProperty("FittedBox", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(Probe());
        }

        private static float ReadFittedScale()
        {
            return (float)typeof(ViewController)
                .GetProperty("FittedScale", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(Probe());
        }

        private static void InvokePlaceFittedGroup(BaseElement group)
        {
            _ = typeof(ViewController)
                .GetMethod("PlaceFittedGroup", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(Probe(), [group]);
        }

        public static TheoryData<string, int, int> Surfaces()
        {
            return LayoutSurfaces.Theory();
        }

        /// <summary>Concrete controller, so the base class's layout members can be exercised.</summary>
        private sealed class ProbeController : ViewController
        {
        }
    }
}
