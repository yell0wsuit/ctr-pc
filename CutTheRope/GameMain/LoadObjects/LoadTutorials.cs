using System;
using System.Xml.Linq;

using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;

using static CutTheRope.Helpers.ParsingHelpers;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Handles loading tutorial objects from XML level data
    /// Includes tutorial text and tutorial visual elements
    /// </summary>
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Loads a tutorial text element from XML node data
        /// </summary>
        private void LoadTutorialText(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            if (!ShouldSkipTutorialElement(xmlNode))
            {
                CTRRootController cTRRootController = (CTRRootController)Application.SharedRootController();
                TutorialText tutorialText = (TutorialText)new TutorialText().InitWithFont(Application.GetFont(Resources.Fnt.SmallFont));
                tutorialText.color = RGBAColor.MakeRGBA(1, 1, 1, 0.9f);
                tutorialText.x = (ParseIntOrZero(xmlNode.Attribute("x")?.Value) * scale) + offsetX + mapOffsetX;
                tutorialText.y = (ParseIntOrZero(xmlNode.Attribute("y")?.Value) * scale) + offsetY + mapOffsetY;
                tutorialText.special = ParseIntOrZero(xmlNode.Attribute("special")?.Value);
                tutorialText.SetAlignment(2);
                string textKey = xmlNode.Attribute("text")?.Value ?? string.Empty;
                string newString = Helpers.LocalizationManager.HasString(textKey)
                    ? Helpers.LocalizationManager.GetString(textKey)
                    : textKey;
                tutorialText.SetStringandWidth(newString, (int)(ParseIntOrZero(xmlNode.Attribute("width")?.Value) * scale));
                tutorialText.color = RGBAColor.transparentRGBA;
                Timeline timeline3 = new Timeline().InitWithMaxKeyFramesOnTrack(4);
                timeline3.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0f));
                timeline3.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 1));
                if (cTRRootController.GetPack() == 0 && cTRRootController.GetLevel() == 0)
                {
                    timeline3.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 10));
                }
                else
                {
                    timeline3.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 5));
                }
                timeline3.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
                tutorialText.AddTimelinewithID(timeline3, 0);
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
        private void LoadTutorialImage(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            if (!ShouldSkipTutorialElement(xmlNode))
            {
                CTRRootController cTRRootController = (CTRRootController)Application.SharedRootController();
                int q = ParseIntOrZero(new string(xmlNode.Name.LocalName.AsSpan()[8..])) - 1;
                GameObjectSpecial gameObjectSpecial = GameObjectSpecial.GameObjectSpecial_createWithResIDQuad(Resources.Img.TutorialSigns, q);
                gameObjectSpecial.color = RGBAColor.transparentRGBA;
                gameObjectSpecial.x = (ParseIntOrZero(xmlNode.Attribute("x")?.Value) * scale) + offsetX + mapOffsetX;
                gameObjectSpecial.y = (ParseIntOrZero(xmlNode.Attribute("y")?.Value) * scale) + offsetY + mapOffsetY;
                gameObjectSpecial.rotation = ParseIntOrZero(xmlNode.Attribute("angle")?.Value);
                gameObjectSpecial.special = ParseIntOrZero(xmlNode.Attribute("special")?.Value);
                gameObjectSpecial.ParseMover(xmlNode);
                Timeline timeline4 = new Timeline().InitWithMaxKeyFramesOnTrack(4);
                timeline4.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0f));
                timeline4.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 1));
                if (cTRRootController.GetPack() == 0 && cTRRootController.GetLevel() == 0)
                {
                    timeline4.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 10));
                }
                else
                {
                    timeline4.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 5.2f));
                }
                timeline4.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
                gameObjectSpecial.AddTimelinewithID(timeline4, 0);
                if (gameObjectSpecial.special == 0)
                {
                    gameObjectSpecial.PlayTimeline(0);
                }
                if (gameObjectSpecial.special is 2)
                {
                    Timeline timeline5 = new Timeline().InitWithMaxKeyFramesOnTrack(12);
                    for (int j = 0; j < 2; j++)
                    {
                        timeline5.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0f));
                        timeline5.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
                        timeline5.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 1));
                        timeline5.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 1.1f));
                        timeline5.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
                        timeline5.AddKeyFrame(KeyFrame.MakePos((int)gameObjectSpecial.x, (int)gameObjectSpecial.y, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0));
                        timeline5.AddKeyFrame(KeyFrame.MakePos((int)gameObjectSpecial.x, (int)gameObjectSpecial.y, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
                        timeline5.AddKeyFrame(KeyFrame.MakePos((int)gameObjectSpecial.x, (int)gameObjectSpecial.y, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 1));
                        timeline5.AddKeyFrame(KeyFrame.MakePos((int)(gameObjectSpecial.x + 230), (int)gameObjectSpecial.y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.5f));
                        timeline5.AddKeyFrame(KeyFrame.MakePos((int)(gameObjectSpecial.x + 440), (int)gameObjectSpecial.y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.5f));
                        timeline5.AddKeyFrame(KeyFrame.MakePos((int)(gameObjectSpecial.x + 440), (int)gameObjectSpecial.y, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.6f));
                    }
                    timeline5.SetTimelineLoopType(Timeline.LoopType.TIMELINE_NO_LOOP);
                    gameObjectSpecial.AddTimelinewithID(timeline5, 1);
                    gameObjectSpecial.PlayTimeline(1);
                    gameObjectSpecial.rotation = 10f;
                }
                tutorialImages.Add(gameObjectSpecial);
            }
        }
    }
}
