using System.Xml.Linq;

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
            CTRMover parsed = CTRMover.FromXml(xml, Vect(x, y), rotation);
            if (parsed != null)
            {
                SetMover(parsed);
            }
        }
    }
}
