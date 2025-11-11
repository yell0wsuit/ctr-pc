using CutTheRope.iframework.helpers;
using CutTheRope.ios;

namespace CutTheRope.game
{
    internal class CTRGameObject : GameObject
    {
        public override void ParseMover(XMLNode xml)
        {
            rotation = 0f;
            NSString nSString = xml["angle"];
            if (nSString != null)
            {
                rotation = nSString.FloatValue();
            }
            NSString nSString2 = xml["path"];
            if (nSString2 != null && nSString2.Length() != 0)
            {
                int i = 100;
                if (nSString2.CharacterAtIndex(0) == 'R')
                {
                    i = ((int)((int)RTD(nSString2.SubstringFromIndex(2).IntValue()) * 3.3f) / 2) + 1;
                }
                float m_ = xml["moveSpeed"].FloatValue() * 3.3f;
                float r_ = xml["rotateSpeed"].FloatValue();
                CTRMover cTRMover = (CTRMover)new CTRMover().InitWithPathCapacityMoveSpeedRotateSpeed(i, m_, r_);
                cTRMover.angle_ = rotation;
                cTRMover.angle_initial = cTRMover.angle_;
                cTRMover.SetPathFromStringandStart(nSString2, Vect(x, y));
                SetMover(cTRMover);
                cTRMover.Start();
            }
        }
    }
}
