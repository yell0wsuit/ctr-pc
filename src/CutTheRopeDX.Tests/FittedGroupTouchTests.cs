using System.Reflection;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Visual;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// A touch must land on whatever the player sees under it. Drawing scales design-space content
    /// up into logical space, so the touch has to come back down by the same amount at the same
    /// point, or a scene reacts to presses somewhere other than where its buttons are drawn.
    /// </summary>
    public sealed class FittedGroupTouchTests
    {
        [Theory]
        [MemberData(nameof(Surfaces))]
        public void ATouchWhereAChildIsDrawnReachesThatChild(string name, int width, int height)
        {
            LayoutSurfaces.WithSurface(width, height, () =>
            {
                FittedGroup group = new() { anchor = 9, parentAnchor = 9 };
                PlaceFittedGroup(group);

                TouchProbe child = new() { parentAnchor = 9, x = 912f, y = 998f, width = 737, height = 176 };
                _ = group.AddChild(child);
                BaseElement.CalculateTopLeft(group);
                BaseElement.CalculateTopLeft(child);

                // Where the renderer puts the child's center, in logical space.
                float scale = group.scaleX;
                float groupCentreX = group.drawX + (group.width >> 1);
                float groupCentreY = group.drawY + (group.height >> 1);
                float drawnX = groupCentreX + ((child.drawX + (child.width / 2f) - groupCentreX) * scale);
                float drawnY = groupCentreY + ((child.drawY + (child.height / 2f) - groupCentreY) * scale);

                Assert.True(group.OnTouchDownXY(drawnX, drawnY), $"{name}: touch missed the child");
                Assert.True(child.Hit, $"{name}: child never saw the touch");
            });
        }

        [Fact]
        public void ATouchOutsideAChildDoesNotReachIt()
        {
            LayoutSurfaces.WithSurface(720, 1280, () =>
            {
                FittedGroup group = new() { anchor = 9, parentAnchor = 9 };
                PlaceFittedGroup(group);

                TouchProbe child = new() { parentAnchor = 9, x = 912f, y = 998f, width = 737, height = 176 };
                _ = group.AddChild(child);
                BaseElement.CalculateTopLeft(group);
                BaseElement.CalculateTopLeft(child);

                Assert.False(group.OnTouchDownXY(5f, 5f));
                Assert.False(child.Hit);
            });
        }

        private static void PlaceFittedGroup(BaseElement group)
        {
            _ = HeadlessGame.Boot();
            _ = typeof(ViewController)
                .GetMethod(
                    "PlaceFittedGroup",
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    [typeof(BaseElement)],
                    null)
                .Invoke(new ProbeController(), [group]);
        }

        public static TheoryData<string, int, int> Surfaces()
        {
            return LayoutSurfaces.Theory();
        }

        private sealed class ProbeController : ViewController
        {
        }

        /// <summary>Records whether a touch fell inside its own rectangle.</summary>
        private sealed class TouchProbe : BaseElement
        {
            public bool Hit { get; private set; }

            public override bool OnTouchDownXY(float tx, float ty)
            {
                CalculateTopLeft(this);
                Hit = tx >= drawX && tx <= drawX + width && ty >= drawY && ty <= drawY + height;
                return Hit;
            }
        }
    }
}
