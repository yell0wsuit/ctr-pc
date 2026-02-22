using System.Xml.Linq;

using CutTheRope.Framework.Helpers;
using System.Globalization;

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
                    i = ((int)((int)RTD(string.IsNullOrEmpty(pathString[2..]) ? 0 : int.Parse(pathString[2..])) * 3.3f) / 2) + 1;
                }
                float m_ = (string.IsNullOrEmpty(xml.Attribute("moveSpeed")?.Value ?? string.Empty) ? 0f : float.Parse(xml.Attribute("moveSpeed")?.Value ?? string.Empty, CultureInfo.InvariantCulture)) * 3.3f;
                float r_ = string.IsNullOrEmpty(xml.Attribute("rotateSpeed")?.Value ?? string.Empty) ? 0f : float.Parse(xml.Attribute("rotateSpeed")?.Value ?? string.Empty, CultureInfo.InvariantCulture);
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
