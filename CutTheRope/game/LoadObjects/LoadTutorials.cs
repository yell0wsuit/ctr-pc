using System;
using System.Xml.Linq;

using CutTheRope.Helpers;
using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.visual;

namespace CutTheRope.game
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
                TutorialText tutorialText = (TutorialText)new TutorialText().InitWithFont(Application.GetFont(4));
                tutorialText.color = RGBAColor.MakeRGBA(1.0, 1.0, 1.0, 0.9);
                tutorialText.x = (xmlNode.AttributeAsNSString("x").IntValue() * scale) + offsetX + mapOffsetX;
                tutorialText.y = (xmlNode.AttributeAsNSString("y").IntValue() * scale) + offsetY + mapOffsetY;
                tutorialText.special = xmlNode.AttributeAsNSString("special").IntValue();
                tutorialText.SetAlignment(2);
                string newString = xmlNode.AttributeAsNSString("text");
                tutorialText.SetStringandWidth(newString, (int)(xmlNode.AttributeAsNSString("width").IntValue() * scale));
                tutorialText.color = RGBAColor.transparentRGBA;
                float num6 = tutorialText.special == 3 ? 12f : 0f;
                Timeline timeline3 = new Timeline().InitWithMaxKeyFramesOnTrack(4);
                timeline3.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, num6));
                timeline3.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 1.0));
                if (cTRRootController.GetPack() == 0 && cTRRootController.GetLevel() == 0)
                {
                    timeline3.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 10.0));
                }
                else
                {
                    timeline3.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 5.0));
                }
                timeline3.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
                tutorialText.AddTimelinewithID(timeline3, 0);
                if (tutorialText.special is 0 or 3)
                {
                    tutorialText.PlayTimeline(0);
                }
                _ = tutorials.AddObject(tutorialText);
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
                int q = new string(xmlNode.Name.LocalName.AsSpan()[8..]).IntValue() - 1;
                GameObjectSpecial gameObjectSpecial = GameObjectSpecial.GameObjectSpecial_createWithResIDQuad(84, q);
                gameObjectSpecial.color = RGBAColor.transparentRGBA;
                gameObjectSpecial.x = (xmlNode.AttributeAsNSString("x").IntValue() * scale) + offsetX + mapOffsetX;
                gameObjectSpecial.y = (xmlNode.AttributeAsNSString("y").IntValue() * scale) + offsetY + mapOffsetY;
                gameObjectSpecial.rotation = xmlNode.AttributeAsNSString("angle").IntValue();
                gameObjectSpecial.special = xmlNode.AttributeAsNSString("special").IntValue();
                gameObjectSpecial.ParseMover(xmlNode);
                float num7 = gameObjectSpecial.special is 3 or 4 ? 12f : 0f;
                Timeline timeline4 = new Timeline().InitWithMaxKeyFramesOnTrack(4);
                timeline4.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, num7));
                timeline4.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 1.0));
                if (cTRRootController.GetPack() == 0 && cTRRootController.GetLevel() == 0)
                {
                    timeline4.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 10.0));
                }
                else
                {
                    timeline4.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 5.2));
                }
                timeline4.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
                gameObjectSpecial.AddTimelinewithID(timeline4, 0);
                if (gameObjectSpecial.special is 0 or 3)
                {
                    gameObjectSpecial.PlayTimeline(0);
                }
                if (gameObjectSpecial.special is 2 or 4)
                {
                    Timeline timeline5 = new Timeline().InitWithMaxKeyFramesOnTrack(12);
                    for (int j = 0; j < 2; j++)
                    {
                        timeline5.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, j == 1 ? 0f : num7));
                        timeline5.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
                        timeline5.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 1.0));
                        timeline5.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 1.1));
                        timeline5.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
                        timeline5.AddKeyFrame(KeyFrame.MakePos(gameObjectSpecial.x, gameObjectSpecial.y, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, (double)(j == 1 ? 0f : num7)));
                        timeline5.AddKeyFrame(KeyFrame.MakePos(gameObjectSpecial.x, gameObjectSpecial.y, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
                        timeline5.AddKeyFrame(KeyFrame.MakePos(gameObjectSpecial.x, gameObjectSpecial.y, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 1.0));
                        timeline5.AddKeyFrame(KeyFrame.MakePos(gameObjectSpecial.x + 230.0, gameObjectSpecial.y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.5));
                        timeline5.AddKeyFrame(KeyFrame.MakePos(gameObjectSpecial.x + 440.0, gameObjectSpecial.y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.5));
                        timeline5.AddKeyFrame(KeyFrame.MakePos(gameObjectSpecial.x + 440.0, gameObjectSpecial.y, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.6));
                    }
                    timeline5.SetTimelineLoopType(Timeline.LoopType.TIMELINE_NO_LOOP);
                    gameObjectSpecial.AddTimelinewithID(timeline5, 1);
                    gameObjectSpecial.PlayTimeline(1);
                    gameObjectSpecial.rotation = 10f;
                }
                _ = tutorialImages.AddObject(gameObjectSpecial);
            }
        }
    }
}
