using System.Xml.Linq;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Helpers;

using static CutTheRopeDX.Helpers.ParsingHelpers;

namespace CutTheRopeDX.GameMain
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
                int i = CTRMover.PathPointCapacity(pathString);
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
