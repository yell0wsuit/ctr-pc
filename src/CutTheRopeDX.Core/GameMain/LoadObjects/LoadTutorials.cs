using System;
using System.Xml.Linq;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.GameMain.Tutorials;

using static CutTheRopeDX.Helpers.ParsingHelpers;

namespace CutTheRopeDX.GameMain
{
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Loads a tutorial text element from XML node data
        /// </summary>
        /// <param name="xmlNode">The XML node describing the tutorial text element.</param>
        /// <param name="scale">The level scale factor applied to object coordinates and width.</param>
        /// <param name="offsetX">The base X offset applied to loaded objects.</param>
        /// <param name="offsetY">The base Y offset applied to loaded objects.</param>
        /// <param name="mapOffsetX">The additional map X offset applied during loading.</param>
        /// <param name="mapOffsetY">The additional map Y offset applied during loading.</param>
        private void LoadTutorialText(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            if (!ShouldSkipTutorialElement(xmlNode))
            {
                CTRRootController cTRRootController = (CTRRootController)Application.SharedRootController();
                TutorialText tutorialText = (TutorialText)new TutorialText().InitWithFont(Application.GetFont(Resources.Fnt.SmallFont));
                tutorialText.color = RGBAColor.MakeRGBA(1, 1, 1, 0.9f);
                tutorialText.x = (ParseCoordinateIntOrZero(xmlNode.Attribute("x")?.Value) * scale) + offsetX + mapOffsetX;
                tutorialText.y = (ParseCoordinateIntOrZero(xmlNode.Attribute("y")?.Value) * scale) + offsetY + mapOffsetY;
                tutorialText.special = ParseIntOrZero(xmlNode.Attribute("special")?.Value);
                tutorialText.SetAlignment(2);
                string textKey = xmlNode.Attribute("text")?.Value ?? string.Empty;
                string newString = Helpers.LocalizationManager.GetString(textKey);
                tutorialText.SetStringandWidth(newString, (int)(ParseIntOrZero(xmlNode.Attribute("width")?.Value) * scale));
                tutorialText.color = RGBAColor.transparentRGBA;
                float hold = cTRRootController.GetPack() == 0 && cTRRootController.GetLevel() == 0 ? 10f : 5f;
                _ = TutorialPromptLoader.BuildEnvelope(tutorialText, 1f, hold, 0.5f);
                if (tutorialText.special == 0)
                {
                    tutorialText.PlayTimeline(0);
                }
                tutorials.Add(tutorialText);
            }
        }

        /// <summary>
        /// Loads a tutorial image element from XML node data
        /// </summary>
        /// <param name="xmlNode">The XML node describing the tutorial image element.</param>
        /// <param name="scale">The level scale factor applied to object coordinates.</param>
        /// <param name="offsetX">The base X offset applied to loaded objects.</param>
        /// <param name="offsetY">The base Y offset applied to loaded objects.</param>
        /// <param name="mapOffsetX">The additional map X offset applied during loading.</param>
        /// <param name="mapOffsetY">The additional map Y offset applied during loading.</param>
        private void LoadTutorialImage(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            if (!ShouldSkipTutorialElement(xmlNode))
            {
                CTRRootController cTRRootController = (CTRRootController)Application.SharedRootController();
                int q = ParseIntOrZero(new string(xmlNode.Name.LocalName.AsSpan()[8..])) - 1;
                GameObjectSpecial gameObjectSpecial = GameObjectSpecial.GameObjectSpecial_createWithResIDQuad(Resources.Img.TutorialSigns, q);
                gameObjectSpecial.color = RGBAColor.transparentRGBA;
                gameObjectSpecial.x = (ParseCoordinateIntOrZero(xmlNode.Attribute("x")?.Value) * scale) + offsetX + mapOffsetX;
                gameObjectSpecial.y = (ParseCoordinateIntOrZero(xmlNode.Attribute("y")?.Value) * scale) + offsetY + mapOffsetY;
                gameObjectSpecial.rotation = ParseIntOrZero(xmlNode.Attribute("angle")?.Value);
                gameObjectSpecial.special = ParseIntOrZero(xmlNode.Attribute("special")?.Value);
                gameObjectSpecial.ParseMover(xmlNode);
                float hold = cTRRootController.GetPack() == 0 && cTRRootController.GetLevel() == 0 ? 10f : 5.2f;
                _ = TutorialPromptLoader.BuildEnvelope(gameObjectSpecial, 1f, hold, 0.5f);
                if (gameObjectSpecial.special == 0)
                {
                    gameObjectSpecial.PlayTimeline(0);
                }
                if (gameObjectSpecial.special is 2)
                {
                    _ = TutorialPromptLoader.BuildSwipe(gameObjectSpecial);
                    gameObjectSpecial.PlayTimeline(1);
                }
                tutorialImages.Add(gameObjectSpecial);
            }
        }
    }
}
