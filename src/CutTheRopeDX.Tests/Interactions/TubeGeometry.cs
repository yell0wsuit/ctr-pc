using CutTheRopeDX.Framework.Core;

namespace CutTheRopeDX.Tests.Interactions
{
    /// <summary>
    /// Maps a <see cref="TubeMouth"/> onto the authored tube angle and the offset from the catching
    /// hole to the tube's centre. A tube's two holes sit half a body width either side of its
    /// centre, rotated with the tube, and it only catches a candy moving from the hole toward the
    /// centre - so a scenario places the body on the far side of the candy's travel.
    /// </summary>
    internal static class TubeGeometry
    {
        /// <summary>Level angle that points the catching hole the requested way.</summary>
        /// <param name="mouth">Requested mouth direction.</param>
        /// <returns>The tube's <c>angle</c> attribute value.</returns>
        public static float AngleFor(TubeMouth mouth)
        {
            return mouth switch
            {
                TubeMouth.CatchesFalling => 270f,
                TubeMouth.CatchesRising => 90f,
                TubeMouth.CatchesRightward => 180f,
                TubeMouth.CatchesLeftward => 0f,
                _ => 0f,
            };
        }

        /// <summary>Offset from the catching hole to the tube's centre, in units of half a body width.</summary>
        /// <param name="mouth">Requested mouth direction.</param>
        /// <returns>A unit vector pointing from the hole to the centre.</returns>
        public static Vector CentreDirection(TubeMouth mouth)
        {
            return mouth switch
            {
                TubeMouth.CatchesFalling => new Vector(0f, 1f),
                TubeMouth.CatchesRising => new Vector(0f, -1f),
                TubeMouth.CatchesRightward => new Vector(1f, 0f),
                TubeMouth.CatchesLeftward => new Vector(-1f, 0f),
                _ => new Vector(-1f, 0f),
            };
        }
    }
}
