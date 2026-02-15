using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Rotate button attached to a mechanical hand segment.
    /// Uses joint-centric hit testing instead of the default button rectangle.
    /// </summary>
    internal sealed class MechanicalHandButton : Button
    {
        /// <summary>
        /// Checks whether touch input is inside the segment rotate interaction radius.
        /// </summary>
        /// <param name="tx">Touch world X coordinate.</param>
        /// <param name="ty">Touch world Y coordinate.</param>
        /// <param name="td">Whether this is touch down context.</param>
        /// <returns><c>true</c> when touch should activate segment rotation.</returns>
        public override bool IsInTouchZoneXYforTouchDown(float tx, float ty, bool td)
        {
            if (segment?.theHand == null || segment.theHand.segments == null)
            {
                return false;
            }

            MechanicalHand hand = segment.theHand;
            int segmentIndex = hand.segments.IndexOf(segment);
            return segmentIndex >= 0 && VectDistance(Vect(tx, ty), hand.JointAtIndexPosition(segmentIndex)) < MechanicalHand.MH_BUTTON_TOUCH_RADIUS;
        }

        /// <summary>
        /// Segment that owns this rotate button.
        /// </summary>
        public MechanicalHandSegment segment;
    }
}
