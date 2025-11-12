using CutTheRope.desktop;
using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.sfe;
using CutTheRope.iframework.visual;
using CutTheRope.ios;

namespace CutTheRope.game
{
    /// <summary>
    /// Handles loading tutorial objects from XML level data
    /// Includes tutorial text and tutorial visual elements
    /// </summary>
    internal sealed partial class GameScene
    {
        private void LoadTutorialText(XMLNode item, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            if (item.Name != "tutorialText") return;
            if (GameScene.ShouldSkipTutorialElement(item)) return;

            TutorialText tutorialText = (TutorialText)new TutorialText().InitWithFont(Application.GetFont(4));
            tutorialText.color = RGBAColor.MakeRGBA(1.0, 1.0, 1.0, 0.9);
            tutorialText.x = (item["x"].IntValue() * scale) + offsetX + mapOffsetX;
            tutorialText.y = (item["y"].IntValue() * scale) + offsetY + mapOffsetY;
            tutorialText.special = item["special"].IntValue();
            tutorialText.SetAlignment(2);
            NSString newString = item["text"];
            tutorialText.SetStringandWidth(newString, item["width"].IntValue() * (int)scale);
            tutorialText.color = RGBAColor.transparentRGBA;

            float num6 = tutorialText.special == 3 ? 12f : 0f;
            Timeline timeline3 = new Timeline().InitWithMaxKeyFramesOnTrack(4);
            timeline3.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, num6));
            timeline3.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 1.0));

            CTRRootController cTRRootController = (CTRRootController)Application.SharedRootController();
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

        private void LoadTutorialImage(XMLNode item, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            if (!item.Name.StartsWith("tutorial", System.StringComparison.Ordinal)) return;
            if (GameScene.ShouldSkipTutorialElement(item)) return;

            int q = new NSString(item.Name[8..]).IntValue() - 1;
            GameObjectSpecial gameObjectSpecial = GameObjectSpecial.GameObjectSpecial_createWithResIDQuad(84, q);
            gameObjectSpecial.color = RGBAColor.transparentRGBA;
            gameObjectSpecial.x = (item["x"].IntValue() * scale) + offsetX + mapOffsetX;
            gameObjectSpecial.y = (item["y"].IntValue() * scale) + offsetY + mapOffsetY;
            gameObjectSpecial.rotation = item["angle"].IntValue();
            gameObjectSpecial.special = item["special"].IntValue();
            gameObjectSpecial.ParseMover(item);

            float num7 = gameObjectSpecial.special is 3 or 4 ? 12f : 0f;
            Timeline timeline4 = new Timeline().InitWithMaxKeyFramesOnTrack(4);
            timeline4.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, num7));
            timeline4.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 1.0));

            CTRRootController cTRRootController = (CTRRootController)Application.SharedRootController();
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
