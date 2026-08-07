using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>Verifies the one place grab axis exclusivity is decided.</summary>
    public class GrabAxisResolverTests
    {
        private static GrabAxes Resolve(
            bool gun = false,
            bool wheel = false,
            bool kickable = false,
            float radius = -1f,
            float moveLength = -1f,
            bool hasMover = false)
        {
            return GrabAxisResolver.Resolve(new GrabAxisRequest(
                Gun: gun, Wheel: wheel, Kickable: kickable,
                Radius: radius, MoveLength: moveLength, HasMover: hasMover,
                MoveVertical: false, MoveOffset: 0f, AnchorX: 0f, AnchorY: 0f));
        }

        [Fact]
        public void PlainGrabIsPreAttachedAndStatic()
        {
            GrabAxes axes = Resolve();

            _ = Assert.IsType<PreAttachedSource>(axes.Source);
            _ = Assert.IsType<StaticMotion>(axes.Motion);
            Assert.Null(axes.Mount);
            Assert.Null(axes.Wheel);
        }

        [Fact]
        public void GunBeatsRadius()
        {
            // No shipped map authors both. SetRadius used to early-return on the gun branch and
            // leave the circle vertices null, which DrawGrabCircle then dereferenced.
            GrabAxes axes = Resolve(gun: true, radius: 100f);

            _ = Assert.IsType<GunSource>(axes.Source);
        }

        [Fact]
        public void PathBeatsRail()
        {
            GrabAxes axes = Resolve(hasMover: true, moveLength: 150f);

            _ = Assert.IsType<PathMotion>(axes.Motion);
        }

        [Fact]
        public void GunAndWheelAreOnDifferentAxesAndCoexist()
        {
            // The loader's `gun = gun && !wheel` was resolving a conflict that never existed.
            GrabAxes axes = Resolve(gun: true, wheel: true);

            _ = Assert.IsType<GunSource>(axes.Source);
            Assert.NotNull(axes.Wheel);
        }

        [Fact]
        public void RailAndCupCoexist()
        {
            // SetMoveLengthVerticalOffset used to clear kickable whenever moveLength >= 0.
            GrabAxes axes = Resolve(kickable: true, moveLength: 150f);

            _ = Assert.IsType<RailMotion>(axes.Motion);
            Assert.NotNull(axes.Mount);
        }

        [Fact]
        public void CupSurvivesAMissingMoveLengthAttribute()
        {
            // The trap: a map that omits moveLength used to silently lose its suction cup.
            GrabAxes axes = Resolve(kickable: true, moveLength: 0f);

            Assert.NotNull(axes.Mount);
        }

        [Fact]
        public void AuthoredKickedStateReachesTheMount()
        {
            GrabAxes axes = GrabAxisResolver.Resolve(
                new GrabAxisRequest(
                    Gun: false, Wheel: false, Kickable: true,
                    Radius: -1f, MoveLength: -1f, HasMover: false,
                    MoveVertical: false, MoveOffset: 0f, AnchorX: 0f, AnchorY: 0f),
                startsKicked: true);

            Assert.False(axes.Mount.IsMounted);
        }
    }
}
