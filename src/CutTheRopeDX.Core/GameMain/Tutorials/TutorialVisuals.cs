using System.Xml.Linq;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.Helpers;

namespace CutTheRopeDX.GameMain.Tutorials
{
    /// <summary>Localized text visual for an XML-authored tutorial prompt.</summary>
    internal sealed class TutorialText : Text
    {
        /// <summary>Creates and lays out a localized tutorial text visual.</summary>
        /// <param name="node">Validated tutorial text XML.</param>
        /// <param name="x">World-space X position.</param>
        /// <param name="y">World-space Y position.</param>
        /// <param name="width">Scaled text width.</param>
        /// <returns>The initialized tutorial text.</returns>
        internal static TutorialText Create(XElement node, float x, float y, float width)
        {
            TutorialText text = (TutorialText)new TutorialText().InitWithFont(
                Application.GetFont(Resources.Fnt.SmallFont));
            text.x = x;
            text.y = y;
            text.SetAlignment(2);
            string textKey = node.Attribute("text")?.Value ?? string.Empty;
            text.SetStringandWidth(LocalizationManager.GetString(textKey), width);
            return text;
        }

        /// <summary>
        /// Gives this prompt the same authored travel a sign gets. <see cref="Text"/> descends from
        /// <see cref="BaseElement"/> rather than the game-object branch that owns movers, so the
        /// mover is held here and stepped from <see cref="Update"/>.
        /// </summary>
        /// <param name="node">Element carrying <c>path</c>, <c>moveSpeed</c> and <c>rotateSpeed</c>.</param>
        internal void ParseMover(XElement node)
        {
            mover = CTRMover.FromXml(node, Vect(x, y), rotation);
        }

        /// <inheritdoc />
        public override void Update(float delta)
        {
            base.Update(delta);
            if (mover is null)
            {
                return;
            }

            mover.Update(delta);
            x = mover.pos.X;
            y = mover.pos.Y;
            rotation = mover.angle_;
        }

        private CTRMover mover;
    }

    /// <summary>Image visual for an XML-authored tutorial prompt.</summary>
    internal sealed class TutorialSign : CTRGameObject
    {
        /// <summary>Creates a tutorial sign from the shared sign atlas.</summary>
        /// <param name="quad">Zero-based tutorial-sign quad.</param>
        /// <param name="x">World-space X position.</param>
        /// <param name="y">World-space Y position.</param>
        /// <returns>The initialized tutorial sign.</returns>
        internal static TutorialSign Create(int quad, float x, float y)
        {
            TutorialSign sign = new();
            _ = sign.InitWithTexture(Application.GetTexture(Resources.Img.TutorialSigns));
            sign.SetDrawQuad(quad);
            sign.x = x;
            sign.y = y;
            return sign;
        }
    }

    /// <summary>Creates the game's concrete tutorial visuals for the strict prompt loader.</summary>
    internal sealed class TutorialVisualFactory : ITutorialVisualFactory
    {
        /// <inheritdoc />
        public BaseElement CreateText(XElement node, float x, float y, float width)
        {
            return TutorialText.Create(node, x, y, width);
        }

        /// <inheritdoc />
        public BaseElement CreateSign(XElement node, int quad, float x, float y)
        {
            return TutorialSign.Create(quad, x, y);
        }
    }
}
