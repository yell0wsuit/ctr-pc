using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// One straight-line segment of an ant path, including the interaction state
    /// used when the ant conveyor carries the candy.
    /// </summary>
    internal sealed class AntsPathSegment : FrameworkTypes
    {
        /// <summary>World-space start point of this segment.</summary>
        public Vector startPoint;

        /// <summary>World-space end point of this segment.</summary>
        public Vector endPoint;

        /// <summary>Velocity vector (direction × speed magnitude) in world units per second.</summary>
        public Vector speed;

        /// <summary>Angle of the segment direction in degrees [0, 360), used to orient ant sprites.</summary>
        public float angleDeg;

        /// <summary>When <see langword="false"/> the segment will not attempt to attach to the candy.</summary>
        public bool canInteract;

        /// <summary>Next segment in the path, or the first segment when the path is looped.</summary>
        public AntsPathSegment nextSegment;

        /// <summary>Previous segment in the path, or the last segment when the path is looped.</summary>
        public AntsPathSegment prevSegment;

        /// <summary>The <see cref="AntsPath"/> that owns this segment.</summary>
        public AntsPath container;

        /// <summary>World-space length of this segment in units.</summary>
        public float Length { get; private set; }

        /// <summary>
        /// Creates a segment from <paramref name="start"/> to <paramref name="end"/> with
        /// the given speed magnitude and device scale.
        /// </summary>
        /// <param name="start">World-space start point of the segment.</param>
        /// <param name="end">World-space end point of the segment.</param>
        /// <param name="speedMagnitude">Speed magnitude in world units per second.</param>
        /// <param name="deviceScale">Device pixel-density multiplier.</param>
        public AntsPathSegment(Vector start, Vector end, float speedMagnitude, float deviceScale)
        {
            SetupWithStartPointEndPointSpeed(start, end, speedMagnitude, deviceScale);
            canInteract = true;
        }

        /// <summary>
        /// (Re)initialises all derived fields from the given endpoints, speed magnitude and device scale.
        /// Called by the constructor and by the path builder.
        /// </summary>
        /// <param name="start">World-space start point of the segment.</param>
        /// <param name="end">World-space end point of the segment.</param>
        /// <param name="speedMagnitude">Speed magnitude in world units per second.</param>
        /// <param name="deviceScale">Device pixel-density multiplier.</param>
        public void SetupWithStartPointEndPointSpeed(Vector start, Vector end, float speedMagnitude, float deviceScale)
        {
            startPoint = start;
            endPoint = end;

            Vector delta = VectSub(end, start);
            Length = VectLength(delta);

            Vector direction = Length > 0f
                ? VectDiv(delta, Length)
                : vectZero;

            speed = VectMult(direction, speedMagnitude);

            float rawAngle = RADIANS_TO_DEGREES(VectAngleNormalized(direction));
            angleDeg = AngleTo0_360(rawAngle);

            internalHalfHeight = AntConveyorLogic.GetSegmentHalfHeight(deviceScale);
        }

        /// <summary>
        /// Returns the closest point on segment AB to point P, clamped to the segment endpoints.
        /// </summary>
        /// <param name="a">Segment start point.</param>
        /// <param name="b">Segment end point.</param>
        /// <param name="p">The point to project onto the segment.</param>
        /// <returns>The closest point on segment AB to <paramref name="p"/>.</returns>
        public static Vector GetPointOnSegmentFromPointtoPointnearestToPoint(Vector a, Vector b, Vector p)
        {
            Vector ab = VectSub(b, a);
            float den = VectDot(ab, ab);
            if (den <= 0f)
            {
                return a;
            }

            float t = VectDot(VectSub(p, a), ab) / den;
            t = FIT_TO_BOUNDARIES(t, 0f, 1f);

            return new Vector(a.X + (ab.X * t), a.Y + (ab.Y * t));
        }

        /// <summary>
        /// Returns <see langword="true"/> if <paramref name="point"/> lies inside the segment's internal interaction
        /// rectangle (equivalent to <see cref="ContainsPoint(Vector, bool)"/> with <c>external = false</c>).
        /// </summary>
        /// <param name="point">The world-space point to test.</param>
        /// <returns><see langword="true"/> if <paramref name="point"/> is inside the internal rectangle; otherwise <see langword="false"/>.</returns>
        public bool ContainsPoint(Vector point)
        {
            return ContainsPoint(point, false);
        }

        /// <summary>
        /// Returns <see langword="true"/> if <paramref name="point"/> lies inside the segment's bounding rectangle.
        /// </summary>
        /// <param name="point">The world-space point to test.</param>
        /// <param name="external">
        /// When <see langword="false"/>, tests the internal rectangle: longitudinal bounds [0, Length],
        /// perpendicular half-height = <see cref="AntConveyorLogic.GetSegmentHalfHeight"/>.
        /// Matches the iOS internal bounding box used for candy attach and detach checks.<br/>
        /// When <see langword="true"/>, tests the external rectangle: longitudinal bounds
        /// [−halfHeight, Length + halfHeight] (extends past each endpoint by halfHeight),
        /// same perpendicular half-height. Matches the iOS <c>externalBB</c> game object
        /// used for the wait-before-attach check.
        /// </param>
        /// <returns><see langword="true"/> if <paramref name="point"/> is inside the bounding rectangle; otherwise <see langword="false"/>.</returns>
        public bool ContainsPoint(Vector point, bool external)
        {
            if (Length <= 0f)
            {
                return false;
            }

            Vector dir = VectDiv(VectSub(endPoint, startPoint), Length);
            Vector ap = VectSub(point, startPoint);
            float proj = VectDot(ap, dir);
            float perpDist = VectLength(VectSub(ap, VectMult(dir, proj)));
            float halfHeight = internalHalfHeight;

            if (perpDist > halfHeight)
            {
                return false;
            }

            float minProj = external ? -internalHalfHeight : 0f;
            float maxProj = external ? Length + internalHalfHeight : Length;
            return proj >= minProj && proj <= maxProj;
        }

        /// <summary>Half-height of the segment's interaction rectangle, perpendicular to the path direction.</summary>
        private float internalHalfHeight;
    }
}
