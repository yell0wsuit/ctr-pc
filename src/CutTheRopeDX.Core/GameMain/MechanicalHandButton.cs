using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Rotate button attached to a mechanical hand segment.
    /// Uses joint-centric hit testing instead of the default button rectangle.
    /// </summary>
    internal sealed class MechanicalHandButton : Button
    {
        /// <inheritdoc />
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
