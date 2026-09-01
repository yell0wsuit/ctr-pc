using System.Xml.Linq;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.Helpers;

namespace CutTheRopeDX.GameMain.Tutorials
{
    internal sealed class TutorialText : Text
    {
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
    }

    internal sealed class TutorialSign : CTRGameObject
    {
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

    internal sealed class TutorialVisualFactory : ITutorialVisualFactory
    {
        public BaseElement CreateText(XElement node, float x, float y, float width)
        {
            return TutorialText.Create(node, x, y, width);
        }

        public BaseElement CreateSign(XElement node, int quad, float x, float y)
        {
            return TutorialSign.Create(quad, x, y);
        }
    }
}
