using System.Xml.Linq;

using CutTheRopeDX.Framework;
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
        /// <summary>
        /// Whether a quad's art is drawn in its own colors. Every quad below this one is black ink
        /// whose shape and shading live entirely in alpha, which is what lets a color replace it.
        /// </summary>
        /// <param name="quad">Zero-based tutorial-sign quad.</param>
        /// <returns><see langword="true"/> when the art carries colors of its own.</returns>
        internal static bool IsDrawnInColor(int quad)
        {
            return quad >= FirstColorQuad;
        }

        private const int FirstColorQuad = 9;

        /// <summary>Creates a tutorial sign from a sign texture.</summary>
        /// <param name="texture">Sign atlas, or a recolored copy of one of its frames.</param>
        /// <param name="quad">Zero-based quad within <paramref name="texture"/>.</param>
        /// <param name="x">World-space X position.</param>
        /// <param name="y">World-space Y position.</param>
        /// <returns>The initialized tutorial sign.</returns>
        internal static TutorialSign Create(CTRTexture2D texture, int quad, float x, float y)
        {
            TutorialSign sign = new();
            _ = sign.InitWithTexture(texture);
            sign.SetDrawQuad(quad);
            sign.x = x;
            sign.y = y;
            return sign;
        }
    }

    /// <summary>Creates the game's concrete tutorial visuals for the strict prompt loader.</summary>
    /// <param name="tints">Scene-owned cache of recolored sign frames.</param>
    internal sealed class TutorialVisualFactory(TutorialSignTints tints) : ITutorialVisualFactory
    {
        /// <inheritdoc />
        public BaseElement CreateText(XElement node, float x, float y, float width)
        {
            return TutorialText.Create(node, x, y, width);
        }

        /// <inheritdoc />
        public BaseElement CreateSign(XElement node, int quad, float x, float y, RGBAColor? color)
        {
            CTRTexture2D atlas = Application.GetTexture(Resources.Img.TutorialSigns);

            // A recolored frame stands alone, so it is drawn as its own first and only quad.
            return color is null
                ? TutorialSign.Create(atlas, quad, x, y)
                : TutorialSign.Create(tints.Tinted(atlas, quad, color.Value), 0, x, y);
        }
    }
}
