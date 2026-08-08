using static CutTheRopeDX.Framework.Helpers.CTRMathHelper;

namespace CutTheRopeDX.GameMain
{
    /// <summary>Raw grab attributes as authored in a level file.</summary>
    /// <param name="Gun">The <c>gun</c> attribute.</param>
    /// <param name="Wheel">The <c>wheel</c> attribute.</param>
    /// <param name="Kickable">The <c>kickable</c> attribute.</param>
    /// <param name="Radius">The <c>radius</c> attribute, or -1 for a fixed hook.</param>
    /// <param name="MoveLength">The <c>moveLength</c> attribute, or a non-positive value for none.</param>
    /// <param name="HasMover">Whether the grab parsed a path mover.</param>
    /// <param name="MoveVertical">The <c>moveVertical</c> attribute.</param>
    /// <param name="MoveOffset">The <c>moveOffset</c> attribute.</param>
    /// <param name="AnchorX">The grab's authored X position.</param>
    /// <param name="AnchorY">The grab's authored Y position.</param>
    internal readonly record struct GrabAxisRequest(
        bool Gun,
        bool Wheel,
        bool Kickable,
        float Radius,
        float MoveLength,
        bool HasMover,
        bool MoveVertical,
        float MoveOffset,
        float AnchorX,
        float AnchorY);

    /// <summary>The axis objects a grab is built from.</summary>
    /// <param name="Source">The rope source.</param>
    /// <param name="Motion">The anchor motion.</param>
    /// <param name="Mount">The suction mount, or <see langword="null"/>.</param>
    /// <param name="Wheel">The wheel control, or <see langword="null"/>.</param>
    internal readonly record struct GrabAxes(
        RopeSource Source,
        AnchorMotion Motion,
        SuctionMount Mount,
        WheelControl Wheel);

    /// <summary>
    /// Turns authored grab attributes into axis objects. The only place exclusivity is decided, and
    /// it is decided per axis: the loader used to patch it at three unrelated sites, one of which
    /// silently disabled a suction cup whenever <c>moveLength</c> was absent.
    /// </summary>
    internal static class GrabAxisResolver
    {
        /// <summary>Resolves a grab's axes.</summary>
        /// <param name="request">The authored attributes.</param>
        /// <param name="startsKicked">Whether the cup is authored as already detached.</param>
        /// <returns>The resolved axis objects.</returns>
        public static GrabAxes Resolve(GrabAxisRequest request, bool startsKicked = false)
        {
            // Rope source. Gun beats radius: SetRadius used to early-return on the gun branch, so a
            // gun+radius grab never got a circle - and then crashed when one was drawn.
            RopeSource source = request.Gun ? new GunSource()
                : request.Radius != -1f
                    ? new AutoRadiusSource(request.Radius, Vect(request.AnchorX, request.AnchorY))
                    : new PreAttachedSource();

            // Anchor motion. A path beats a rail: an authored path and a drag rail are two ways to
            // move the same hook, and the path wins.
            AnchorMotion motion = request.HasMover ? new PathMotion()
                : request.MoveLength > 0f
                    ? new RailMotion(request.MoveLength, request.MoveVertical, request.MoveOffset, request.AnchorX, request.AnchorY)
                    : new StaticMotion();

            return new GrabAxes(
                source,
                motion,
                request.Kickable ? new SuctionMount(startsKicked) : null,
                request.Wheel ? new WheelControl() : null);
        }
    }
}
