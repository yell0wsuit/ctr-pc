using System.Xml.Linq;

using CutTheRope.Framework;
using CutTheRope.Framework.Helpers;

using static CutTheRope.Helpers.ParsingHelpers;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Game-specific <see cref="GameObject"/> subclass that parses mover paths and rotation from level XML.
    /// </summary>
    internal class CTRGameObject : GameObject
    {
        /// <inheritdoc />
        public override void ParseMover(XElement xml)
        {
            rotation = 0f;
            string angleString = xml.Attribute("angle")?.Value ?? string.Empty;
            if (angleString.Length != 0)
            {
                rotation = ParseFloatOrZero(angleString);
            }
            string pathString = xml.Attribute("path")?.Value ?? string.Empty;
            if (pathString != null && pathString.Length != 0)
            {
                int i = 100;
                if (pathString[0] == 'R')
                {
                    i = ((int)(RTD(ParseIntOrZero(pathString[2..])) * ActivePhysicsConstants.MoverPathScale) / 2) + 1;
                }
                float m_ = ParseFloatOrZero(xml.Attribute("moveSpeed")?.Value) * ActivePhysicsConstants.MoverSpeedScale;
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
