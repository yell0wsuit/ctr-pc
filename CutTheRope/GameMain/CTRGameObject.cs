using System.Xml.Linq;

using CutTheRope.Framework.Helpers;
using CutTheRope.Helpers;
using System.Globalization;

namespace CutTheRope.GameMain
{
    internal class CTRGameObject : GameObject
    {
        public override void ParseMover(XElement xml)
        {
            rotation = 0f;
            string angleString = xml.AttributeAsNSString("angle");
            if (angleString.Length != 0)
            {
                rotation = string.IsNullOrEmpty(angleString) ? 0f : float.Parse(angleString, CultureInfo.InvariantCulture);
            }
            string pathString = xml.AttributeAsNSString("path");
            if (pathString != null && pathString.Length != 0)
            {
                int i = 100;
                if (pathString[0] == 'R')
                {
                    i = ((int)((int)RTD(string.IsNullOrEmpty(pathString[2..]) ? 0 : int.Parse(pathString[2..])) * 3.3f) / 2) + 1;
                }
                float m_ = (string.IsNullOrEmpty(xml.AttributeAsNSString("moveSpeed")) ? 0f : float.Parse(xml.AttributeAsNSString("moveSpeed"), CultureInfo.InvariantCulture)) * 3.3f;
                float r_ = string.IsNullOrEmpty(xml.AttributeAsNSString("rotateSpeed")) ? 0f : float.Parse(xml.AttributeAsNSString("rotateSpeed"), CultureInfo.InvariantCulture);
                CTRMover cTRMover = new(i, m_, r_)
                {
                    angle_ = rotation
                };
                cTRMover.angle_initial = cTRMover.angle_;
                cTRMover.SetPathFromStringandStart(pathString, Vect(x, y));
                SetMover(cTRMover);
                cTRMover.Start();
            }
        }
    }
}
