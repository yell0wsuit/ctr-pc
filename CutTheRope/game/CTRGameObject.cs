using System.Xml.Linq;

using CutTheRope.Helpers;
using CutTheRope.iframework.helpers;

namespace CutTheRope.game
{
    internal class CTRGameObject : GameObject
    {
        public override void ParseMover(XElement xml)
        {
            rotation = 0f;
            string nSString = xml.AttributeAsNSString("angle");
            if (nSString.Length() != 0)
            {
                rotation = nSString.FloatValue();
            }
            string nSString2 = xml.AttributeAsNSString("path");
            if (nSString2 != null && nSString2.Length() != 0)
            {
                int i = 100;
                if (nSString2.CharacterAtIndex(0) == 'R')
                {
                    i = ((int)((int)RTD(nSString2.SubstringFromIndex(2).IntValue()) * 3.3f) / 2) + 1;
                }
                float m_ = xml.AttributeAsNSString("moveSpeed").FloatValue() * 3.3f;
                float r_ = xml.AttributeAsNSString("rotateSpeed").FloatValue();
                CTRMover cTRMover = new(i, m_, r_)
                {
                    angle_ = rotation
                };
                cTRMover.angle_initial = cTRMover.angle_;
                cTRMover.SetPathFromStringandStart(nSString2, Vect(x, y));
                SetMover(cTRMover);
                cTRMover.Start();
            }
        }
    }
}
