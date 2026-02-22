using System.Globalization;
using System.Xml.Linq;

using CutTheRope.Framework.Helpers;

namespace CutTheRope.GameMain
{
    internal class CTRGameObject : GameObject
    {
        public override void ParseMover(XElement xml)
        {
            rotation = 0f;
            string angleString = xml.Attribute("angle")?.Value ?? string.Empty;
            if (angleString.Length != 0)
            {
                rotation = string.IsNullOrEmpty(angleString) ? 0f : float.Parse(angleString, CultureInfo.InvariantCulture);
            }
            string pathString = xml.Attribute("path")?.Value ?? string.Empty;
            if (pathString != null && pathString.Length != 0)
            {
                int i = 100;
                if (pathString[0] == 'R')
                {
                    i = ((int)((int)RTD(ParseIntOrZero(pathString[2..])) * 3.3f) / 2) + 1;
                }
                float m_ = ParseFloatOrZero(xml.Attribute("moveSpeed")?.Value) * 3.3f;
                float r_ = ParseFloatOrZero(xml.Attribute("rotateSpeed")?.Value);
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
