using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    internal sealed class MechanicalHandButton : Button
    {
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

        public MechanicalHandSegment segment;
    }
}
