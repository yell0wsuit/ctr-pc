using CutTheRope.iframework;
using CutTheRope.iframework.helpers;
using CutTheRope.ios;
using System;

namespace CutTheRope.game
{
    internal class CTRGameObject : GameObject
    {
        public override void parseMover(XMLNode xml)
        {
            rotation = 0f;
            NSString nSString = xml["angle"];
            if (nSString != null)
            {
                rotation = nSString.floatValue();
            }
            NSString nSString2 = xml["path"];
            if (nSString2 != null && nSString2.length() != 0)
            {
                int i = 100;
                if (nSString2.characterAtIndex(0) == 'R')
                {
                    i = (int)((int)RTD(nSString2.substringFromIndex(2).intValue()) * 3.3f) / 2 + 1;
                }
                float m_ = xml["moveSpeed"].floatValue() * 3.3f;
                float r_ = xml["rotateSpeed"].floatValue();
                CTRMover cTRMover = (CTRMover)new CTRMover().initWithPathCapacityMoveSpeedRotateSpeed(i, m_, r_);
                cTRMover.angle_ = rotation;
                cTRMover.angle_initial = cTRMover.angle_;
                cTRMover.setPathFromStringandStart(nSString2, vect(x, y));
                setMover(cTRMover);
                cTRMover.start();
            }
        }
    }
}
