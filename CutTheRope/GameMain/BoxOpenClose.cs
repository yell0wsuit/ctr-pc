using System;
using System.Globalization;

using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Visual;
using CutTheRope.Helpers;

namespace CutTheRope.GameMain
{
    internal sealed class BoxOpenClose : BaseElement, ITimelineDelegate
    {
        public override void Update(float delta)
        {
            base.Update(delta);
            if (boxAnim != 2)
            {
                return;
            }
            bool flag = Mover.MoveVariableToTarget(ref raDelay, 0, 1, delta);
            switch (raState)
            {
                case -1:
                    {
                        cscore = 0;
                        ctime = time;
                        cstarBonus = starBonus;
                        ((Text)result.GetChildWithName("scoreValue")).SetString(cscore.ToString(CultureInfo.InvariantCulture));
                        Text text27 = (Text)result.GetChildWithName("dataTitle");
                        Image.SetElementPositionWithQuadOffset(text27, Resources.Img.MenuResult, 5);
                        text27.SetString(Application.GetString("STAR_BONUS"));
                        ((Text)result.GetChildWithName("dataValue")).SetString(cstarBonus.ToString(CultureInfo.InvariantCulture));
                        raState = 1;
                        raDelay = 1f;
                        return;
                    }
                case 0:
                    if (flag)
                    {
                        raState = 1;
                        raDelay = 0.2f;
                        return;
                    }
                    break;
                case 1:
                    {
                        Text text28 = (Text)result.GetChildWithName("dataTitle");
                        text28.SetEnabled(true);
                        Text text21 = (Text)result.GetChildWithName("dataValue");
                        text21.SetEnabled(true);
                        Text text22 = (Text)result.GetChildWithName("scoreValue");
                        text22.SetEnabled(true);
                        text28.color.AlphaChannel = text21.color.AlphaChannel = text22.color.AlphaChannel = 1f - (raDelay / 0.2f);
                        if (flag)
                        {
                            raState = 2;
                            raDelay = 1f;
                            return;
                        }
                        break;
                    }
                case 2:
                    {
                        cstarBonus = (int)(starBonus * raDelay);
                        cscore = (int)((1f - raDelay) * starBonus);
                        ((Text)result.GetChildWithName("dataValue")).SetString(cstarBonus.ToString(CultureInfo.InvariantCulture));
                        Text text29 = (Text)result.GetChildWithName("scoreValue");
                        text29.SetEnabled(true);
                        text29.SetString(cscore.ToString(CultureInfo.InvariantCulture));
                        if (flag)
                        {
                            raState = 3;
                            raDelay = 0.2f;
                            return;
                        }
                        break;
                    }
                case 3:
                    {
                        BaseElement baseElement = (Text)result.GetChildWithName("dataTitle");
                        Text text23 = (Text)result.GetChildWithName("dataValue");
                        baseElement.color.AlphaChannel = text23.color.AlphaChannel = raDelay / 0.2f;
                        if (flag)
                        {
                            raState = 4;
                            raDelay = 0.2f;
                            int minutes = (int)MathF.Floor(Round(time) / 60f);
                            int seconds = (int)(Round(time) - (minutes * 60f));
                            ((Text)result.GetChildWithName("dataTitle")).SetString(Application.GetString("TIME"));
                            ((Text)result.GetChildWithName("dataValue")).SetString(minutes.ToString(CultureInfo.InvariantCulture) + ":" + seconds.ToString("D2", CultureInfo.InvariantCulture));
                            return;
                        }
                        break;
                    }
                case 4:
                    {
                        BaseElement baseElement2 = (Text)result.GetChildWithName("dataTitle");
                        Text text24 = (Text)result.GetChildWithName("dataValue");
                        baseElement2.color.AlphaChannel = text24.color.AlphaChannel = 1f - (raDelay / 0.2f);
                        if (flag)
                        {
                            raState = 5;
                            raDelay = 1f;
                            return;
                        }
                        break;
                    }
                case 5:
                    {
                        ctime = time * raDelay;
                        cscore = (int)(starBonus + ((1f - raDelay) * timeBonus));
                        int minutes = (int)MathF.Floor(Round(ctime) / 60);
                        int seconds = (int)(Round(ctime) - (minutes * 60));
                        ((Text)result.GetChildWithName("dataValue")).SetString(minutes.ToString(CultureInfo.InvariantCulture) + ":" + seconds.ToString("D2", CultureInfo.InvariantCulture));
                        ((Text)result.GetChildWithName("scoreValue")).SetString(cscore.ToString(CultureInfo.InvariantCulture));
                        if (flag)
                        {
                            raState = 6;
                            raDelay = 0.2f;
                            return;
                        }
                        break;
                    }
                case 6:
                    {
                        BaseElement baseElement3 = (Text)result.GetChildWithName("dataTitle");
                        Text text25 = (Text)result.GetChildWithName("dataValue");
                        baseElement3.color.AlphaChannel = text25.color.AlphaChannel = raDelay / 0.2f;
                        if (flag)
                        {
                            raState = 7;
                            raDelay = 0.2f;
                            Text text30 = (Text)result.GetChildWithName("dataTitle");
                            Image.SetElementPositionWithQuadOffset(text30, Resources.Img.MenuResult, 7);
                            text30.SetString(Application.GetString("FINAL_SCORE"));
                            ((Text)result.GetChildWithName("dataValue")).SetString("");
                            return;
                        }
                        break;
                    }
                case 7:
                    {
                        BaseElement baseElement4 = (Text)result.GetChildWithName("dataTitle");
                        Text text26 = (Text)result.GetChildWithName("dataValue");
                        baseElement4.color.AlphaChannel = text26.color.AlphaChannel = 1f - (raDelay / 0.2f);
                        if (flag)
                        {
                            raState = 8;
                            if (shouldShowImprovedResult)
                            {
                                stamp.SetEnabled(true);
                                stamp.PlayTimeline(0);
                            }
                        }
                        break;
                    }
                default:
                    return;
            }
        }

        public BoxOpenClose InitWithButtonDelegate(IButtonDelegation b)
        {
            result = new BaseElement();
            _ = AddChildwithID(result, 1);
            anchor = parentAnchor = 18;
            result.anchor = result.parentAnchor = 18;
            result.SetEnabled(false);
            Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
            result.AddTimelinewithID(timeline, 0);
            timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
            result.AddTimelinewithID(timeline, 1);
            Image image = Image.Image_createWithResIDQuad(Resources.Img.MenuResult, 14);
            image.anchor = 18;
            image.SetName("star1");
            Image.SetElementPositionWithQuadOffset(image, Resources.Img.MenuResult, 0);
            _ = result.AddChild(image);
            Image image2 = Image.Image_createWithResIDQuad(Resources.Img.MenuResult, 14);
            image2.anchor = 18;
            image2.SetName("star2");
            Image.SetElementPositionWithQuadOffset(image2, Resources.Img.MenuResult, 1);
            _ = result.AddChild(image2);
            Image image3 = Image.Image_createWithResIDQuad(Resources.Img.MenuResult, 14);
            image3.anchor = 18;
            image3.SetName("star3");
            Image.SetElementPositionWithQuadOffset(image3, Resources.Img.MenuResult, 2);
            _ = result.AddChild(image3);
            Text text = new Text().InitWithFont(Application.GetFont(Resources.Fnt.BigFont));
            text.SetString(Application.GetString("LEVEL_CLEARED1"));
            Image.SetElementPositionWithQuadOffset(text, Resources.Img.MenuResult, 3);
            text.anchor = 18;
            text.SetName("passText");
            _ = result.AddChild(text);
            Image image4 = Image.Image_createWithResIDQuad(Resources.Img.MenuResult, 15);
            image4.anchor = 18;
            Image.SetElementPositionWithQuadOffset(image4, Resources.Img.MenuResult, 4);
            _ = result.AddChild(image4);
            stamp = Image.Image_createWithResIDQuad(Resources.Img.MenuResultEn, 0);
            Timeline timeline2 = new Timeline().InitWithMaxKeyFramesOnTrack(7);
            timeline2.AddKeyFrame(KeyFrame.MakeScale(3, 3, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
            timeline2.AddKeyFrame(KeyFrame.MakeScale(1, 1, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.5f));
            timeline2.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
            timeline2.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.5f));
            _ = stamp.AddTimeline(timeline2);
            stamp.anchor = 18;
            stamp.SetEnabled(false);
            Image.SetElementPositionWithQuadOffset(stamp, Resources.Img.MenuResult, 12);
            _ = result.AddChild(stamp);
            Button button = MenuController.CreateShortButtonWithTextIDDelegate(Application.GetString("REPLAY"), 8, b);
            button.anchor = 18;
            Image.SetElementPositionWithQuadOffset(button, Resources.Img.MenuResult, 11);
            _ = result.AddChild(button);
            Button button2 = MenuController.CreateShortButtonWithTextIDDelegate(Application.GetString("NEXT"), 9, b);
            button2.anchor = 18;
            Image.SetElementPositionWithQuadOffset(button2, Resources.Img.MenuResult, 10);
            _ = result.AddChild(button2);
            Button button3 = MenuController.CreateShortButtonWithTextIDDelegate(Application.GetString("MENU"), 5, b);
            button3.anchor = 18;
            Image.SetElementPositionWithQuadOffset(button3, Resources.Img.MenuResult, 9);
            _ = result.AddChild(button3);
            Text text2 = new Text().InitWithFont(Application.GetFont(Resources.Fnt.SmallFont));
            text2.SetName("dataTitle");
            text2.anchor = 18;
            Image.SetElementPositionWithQuadOffset(text2, Resources.Img.MenuResult, 5);
            _ = result.AddChild(text2);
            Text text3 = new Text().InitWithFont(Application.GetFont(Resources.Fnt.SmallFont));
            text3.SetName("dataValue");
            text3.anchor = 18;
            Image.SetElementPositionWithQuadOffset(text3, Resources.Img.MenuResult, 6);
            _ = result.AddChild(text3);
            Text text4 = new Text().InitWithFont(Application.GetFont(Resources.Fnt.FontNumbersBig));
            text4.SetName("scoreValue");
            text4.anchor = 18;
            Image.SetElementPositionWithQuadOffset(text4, Resources.Img.MenuResult, 8);
            _ = result.AddChild(text4);
            confettiAnims = new BaseElement();
            _ = result.AddChild(confettiAnims);
            openCloseAnims = null;
            boxAnim = -1;
            delegateboxClosed = null;
            return this;
        }

        public static BaseElement CreateConfettiParticleNear()
        {
            Confetti confetti = Confetti.Confetti_createWithResID(Resources.Img.ConfettiParticles);
            confetti.DoRestoreCutTransparency();
            int confettiVariant = RND_RANGE(0, 2);
            int firstFrame = 18;
            int lastFrame = 26;
            if (confettiVariant != 1)
            {
                if (confettiVariant == 2)
                {
                    firstFrame = 0;
                    lastFrame = 8;
                }
            }
            else
            {
                firstFrame = 9;
                lastFrame = 17;
            }
            float spawnX = RND_RANGE((int)RTPD(-100), (int)SCREEN_WIDTH);
            float spawnY = RND_RANGE((int)RTPD(-40), (int)RTPD(100));
            float fadeDuration = FLOAT_RND_RANGE(2, 5);
            int i = confetti.AddAnimationDelayLoopFirstLast(0.05f, Timeline.LoopType.TIMELINE_REPLAY, firstFrame, lastFrame);
            confetti.ani = confetti.GetTimeline(i);
            confetti.ani.PlayTimeline();
            confetti.ani.JumpToTrackKeyFrame((int)Track.TrackType.TRACK_ACTION, RND_RANGE(0, lastFrame - firstFrame - 1));
            Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, fadeDuration));
            timeline.AddKeyFrame(KeyFrame.MakePos((int)spawnX, (int)spawnY, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
            timeline.AddKeyFrame(KeyFrame.MakePos((int)spawnX, (int)(spawnY + FLOAT_RND_RANGE((int)RTPD(150), (int)RTPD(400))), KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, fadeDuration));
            timeline.AddKeyFrame(KeyFrame.MakeScale(0, 0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
            timeline.AddKeyFrame(KeyFrame.MakeScale(1, 1, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.3f));
            timeline.AddKeyFrame(KeyFrame.MakeRotation(RND_RANGE(-360, 360), KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
            timeline.AddKeyFrame(KeyFrame.MakeRotation(RND_RANGE(-360, 360), KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, fadeDuration));
            _ = confetti.AddTimeline(timeline);
            confetti.PlayTimeline(1);
            return confetti;
        }

        public void LevelFirstStart()
        {
            boxAnim = 0;
            RemoveOpenCloseAnims();
            ShowOpenAnim();
            if (result.IsEnabled())
            {
                result.PlayTimeline(1);
            }
        }

        public void LevelStart()
        {
            boxAnim = 1;
            RemoveOpenCloseAnims();
            ShowOpenAnim();
            if (result.IsEnabled())
            {
                result.PlayTimeline(1);
            }
        }

        public void LevelWon()
        {
            boxAnim = 2;
            raState = -1;
            RemoveOpenCloseAnims();
            ShowCloseAnim();
            ((Text)result.GetChildWithName("scoreValue")).SetEnabled(false);
            Text text = (Text)result.GetChildWithName("dataTitle");
            text.SetEnabled(false);
            Image.SetElementPositionWithQuadOffset(text, Resources.Img.MenuResult, 5);
            ((Text)result.GetChildWithName("dataValue")).SetEnabled(false);
            result.PlayTimeline(0);
            result.SetEnabled(true);
            stamp.SetEnabled(false);
        }

        public void LevelLost()
        {
            boxAnim = 3;
            RemoveOpenCloseAnims();
            ShowCloseAnim();
        }

        public void LevelQuit()
        {
            boxAnim = 4;
            result.SetEnabled(false);
            RemoveOpenCloseAnims();
            ShowCloseAnim();
        }

        public void ShowOpenAnim()
        {
            ShowOpenCloseAnim(true);
        }

        public void ShowCloseAnim()
        {
            ShowOpenCloseAnim(false);
        }

        public void ShowConfetti()
        {
            for (int i = 0; i < 70; i++)
            {
                _ = confettiAnims.AddChild(CreateConfettiParticleNear());
            }
        }

        public void ShowOpenCloseAnim(bool open)
        {
            CreateOpenCloseAnims();
            CTRRootController cTRRootController = (CTRRootController)Application.SharedRootController();
            string boxCover = PackConfig.GetBoxCoverOrDefault(cTRRootController.GetPack());
            Image image = Image.Image_createWithResIDQuad(Resources.Img.MenuResult, 16);
            image.rotationCenterX = (-image.width / 2f) + 1f;
            image.rotationCenterY = (-image.height / 2f) + 1f;
            image.scaleX = image.scaleY = 4f;
            Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            if (open)
            {
                timeline.AddKeyFrame(KeyFrame.MakePos(0, 0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
                timeline.AddKeyFrame(KeyFrame.MakePos(-image.width * 4, 0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
            }
            else
            {
                timeline.AddKeyFrame(KeyFrame.MakePos(-image.width * 4, 0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
                timeline.AddKeyFrame(KeyFrame.MakePos(0, 0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
            }
            image.AddTimelinewithID(timeline, 0);
            image.PlayTimeline(0);
            timeline.delegateTimelineDelegate = this;
            _ = openCloseAnims.AddChild(image);
            Vector quadSize = Image.GetQuadSize(boxCover, 0);
            float leftCoverX = (SCREEN_WIDTH / 2f) - quadSize.X;
            Image coverBackgroundLeft = Image.Image_createWithResIDQuad(boxCover, 0);
            Image coverBackgroundRight = Image.Image_createWithResIDQuad(boxCover, 0);
            coverBackgroundLeft.x = leftCoverX;
            coverBackgroundLeft.rotationCenterX = -coverBackgroundLeft.width / 2f;
            coverBackgroundRight.rotationCenterX = coverBackgroundLeft.rotationCenterX;
            coverBackgroundRight.rotation = 180f;
            coverBackgroundRight.x = SCREEN_WIDTH - ((SCREEN_WIDTH / 2f) - coverBackgroundLeft.width);
            coverBackgroundRight.y = -0.5f;
            timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            if (open)
            {
                timeline.AddKeyFrame(KeyFrame.MakeScale(1, 1, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
                timeline.AddKeyFrame(KeyFrame.MakeScale(0, 1.1f, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
                timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.MakeRGBA(0.85f, 0.85f, 0.85f, 1), KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
                timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.whiteRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
            }
            else
            {
                timeline.AddKeyFrame(KeyFrame.MakeScale(0, 1.1f, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
                timeline.AddKeyFrame(KeyFrame.MakeScale(1, 1, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
                timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.whiteRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
                timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.MakeRGBA(0.85f, 0.85f, 0.85f, 1), KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
            }
            coverBackgroundLeft.AddTimelinewithID(timeline, 0);
            coverBackgroundLeft.PlayTimeline(0);
            timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            if (open)
            {
                timeline.AddKeyFrame(KeyFrame.MakeScale(1, 1, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
                timeline.AddKeyFrame(KeyFrame.MakeScale(0, 1.1f, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
                timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.MakeRGBA(0.85f, 0.85f, 0.85f, 1), KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
                timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.MakeRGBA(0.4f, 0.4f, 0.4f, 1), KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
            }
            else
            {
                timeline.AddKeyFrame(KeyFrame.MakeScale(0, 1.1f, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
                timeline.AddKeyFrame(KeyFrame.MakeScale(1, 1, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
                timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.MakeRGBA(0.4f, 0.4f, 0.4f, 1), KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
                timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.MakeRGBA(0.85f, 0.85f, 0.85f, 1), KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
            }
            coverBackgroundRight.AddTimelinewithID(timeline, 0);
            coverBackgroundRight.PlayTimeline(0);
            Image image4 = Image.Image_createWithResIDQuad(Resources.Img.MenuLoading, 0);
            Image image5 = Image.Image_createWithResIDQuad(Resources.Img.MenuLoading, 1);
            float loadingY = 80f;
            float leftOpenOffset = 50f;
            float rightRestInset = 10f;
            float leftClosedX = -40f;
            float rightClosedX = 25f;
            timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            if (open)
            {
                timeline.AddKeyFrame(KeyFrame.MakePos((int)(coverBackgroundLeft.width - leftOpenOffset), (int)loadingY, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
                timeline.AddKeyFrame(KeyFrame.MakePos((int)leftClosedX, (int)loadingY, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
                timeline.AddKeyFrame(KeyFrame.MakeScale(1, 1, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
                timeline.AddKeyFrame(KeyFrame.MakeScale(0, 1.3f, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
            }
            else
            {
                timeline.AddKeyFrame(KeyFrame.MakePos((int)RTD(-15), (int)loadingY, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
                timeline.AddKeyFrame(KeyFrame.MakePos((int)(coverBackgroundLeft.width - leftOpenOffset), (int)loadingY, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
                timeline.AddKeyFrame(KeyFrame.MakeScale(0, 1.3f, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
                timeline.AddKeyFrame(KeyFrame.MakeScale(1, 1, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
            }
            image4.AddTimelinewithID(timeline, 0);
            image4.PlayTimeline(0);
            timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            if (open)
            {
                timeline.AddKeyFrame(KeyFrame.MakePos((int)(SCREEN_WIDTH - coverBackgroundLeft.width + rightRestInset), (int)loadingY, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
                timeline.AddKeyFrame(KeyFrame.MakePos((int)(SCREEN_WIDTH + rightClosedX), (int)loadingY, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
                timeline.AddKeyFrame(KeyFrame.MakeScale(1, 1, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
                timeline.AddKeyFrame(KeyFrame.MakeScale(0, 1.3f, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
            }
            else
            {
                timeline.AddKeyFrame(KeyFrame.MakePos((int)(SCREEN_WIDTH - RTD(9)), (int)loadingY, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
                timeline.AddKeyFrame(KeyFrame.MakePos((int)(SCREEN_WIDTH - coverBackgroundLeft.width + rightRestInset), (int)loadingY, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
                timeline.AddKeyFrame(KeyFrame.MakeScale(0, 1.3f, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
                timeline.AddKeyFrame(KeyFrame.MakeScale(1, 1, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
            }
            image5.AddTimelinewithID(timeline, 0);
            image5.PlayTimeline(0);
            Image coverSideLeft = Image.Image_createWithResIDQuad(boxCover, 1);
            Image coverSideRight = Image.Image_createWithResIDQuad(boxCover, 1);
            coverSideLeft.rotationCenterX = -coverSideLeft.width / 2f;
            coverSideRight.rotationCenterX = coverSideLeft.rotationCenterX;
            timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            if (open)
            {
                timeline.AddKeyFrame(KeyFrame.MakePos((int)(coverBackgroundLeft.x + coverBackgroundLeft.width - RTD(6)), 0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
                timeline.AddKeyFrame(KeyFrame.MakePos(-25, 0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
                timeline.AddKeyFrame(KeyFrame.MakeScale(0, 1.3f, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
                timeline.AddKeyFrame(KeyFrame.MakeScale(1, 1, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
            }
            else
            {
                timeline.AddKeyFrame(KeyFrame.MakePos((int)coverBackgroundLeft.x, 0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
                timeline.AddKeyFrame(KeyFrame.MakePos((int)(coverBackgroundLeft.width - 16f), 0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
                timeline.AddKeyFrame(KeyFrame.MakeScale(1, 1, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
                timeline.AddKeyFrame(KeyFrame.MakeScale(0, 1.3f, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
            }
            coverSideLeft.AddTimelinewithID(timeline, 0);
            coverSideLeft.PlayTimeline(0);
            _ = openCloseAnims.AddChild(coverSideLeft);
            timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            if (open)
            {
                timeline.AddKeyFrame(KeyFrame.MakePos((int)(SCREEN_WIDTH - coverBackgroundLeft.width + RTD(7)), 0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
                timeline.AddKeyFrame(KeyFrame.MakePos((int)SCREEN_WIDTH, 0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
                timeline.AddKeyFrame(KeyFrame.MakeScale(0, 1.3f, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
                timeline.AddKeyFrame(KeyFrame.MakeScale(1, 1, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
            }
            else
            {
                timeline.AddKeyFrame(KeyFrame.MakePos((int)(SCREEN_WIDTH - 40f), 0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
                timeline.AddKeyFrame(KeyFrame.MakePos((int)(SCREEN_WIDTH - coverBackgroundLeft.width + 20f), 0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
                timeline.AddKeyFrame(KeyFrame.MakeScale(1, 1, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
                timeline.AddKeyFrame(KeyFrame.MakeScale(0, 1.3f, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
            }
            coverSideRight.AddTimelinewithID(timeline, 0);
            coverSideRight.PlayTimeline(0);
            _ = openCloseAnims.AddChild(coverSideRight);
            _ = openCloseAnims.AddChild(coverBackgroundLeft);
            _ = openCloseAnims.AddChild(coverBackgroundRight);
            if (boxAnim == 0)
            {
                _ = openCloseAnims.AddChild(image4);
                _ = openCloseAnims.AddChild(image5);
            }
        }

        public void TimelinereachedKeyFramewithIndex(Timeline t, KeyFrame k, int i)
        {
        }

        public void TimelineFinished(Timeline t)
        {
            switch (boxAnim)
            {
                case 0:
                case 1:
                    {
                        DelayedDispatcher.DispatchFunc dispatchFunc = new(Selector_removeOpenCloseAnims);
                        TimerManager.RegisterDelayedObjectCall(dispatchFunc, this, 0.001f);
                        if (result.IsEnabled())
                        {
                            confettiAnims.RemoveAllChilds();
                            result.SetEnabled(false);
                            return;
                        }
                        break;
                    }
                case 2:
                    {
                        DelayedDispatcher.DispatchFunc dispatchFunc2 = new(Selector_postBoxClosed);
                        TimerManager.RegisterDelayedObjectCall(dispatchFunc2, this, 0.001f);
                        break;
                    }
                case 3:
                    break;
                case 4:
                    Application.SharedRootController().GetCurrentController().Deactivate();
                    return;
                default:
                    return;
            }
        }

        public void PostBoxClosed()
        {
            delegateboxClosed?.Invoke();
            if (shouldShowConfetti)
            {
                ShowConfetti();
            }
        }

        public void RemoveOpenCloseAnims()
        {
            if (GetChild(0) != null)
            {
                RemoveChild(openCloseAnims);
                openCloseAnims = null;
            }
            BaseElement baseElement = (Text)result.GetChildWithName("dataTitle");
            Text text2 = (Text)result.GetChildWithName("dataValue");
            Text text3 = (Text)result.GetChildWithName("scoreValue");
            baseElement.color.AlphaChannel = text2.color.AlphaChannel = text3.color.AlphaChannel = 1f;
        }

        public void CreateOpenCloseAnims()
        {
            openCloseAnims = new BaseElement();
            _ = AddChildwithID(openCloseAnims, 0);
        }

        private static void Selector_removeOpenCloseAnims(FrameworkTypes obj)
        {
            ((BoxOpenClose)obj).RemoveOpenCloseAnims();
        }

        private static void Selector_postBoxClosed(FrameworkTypes obj)
        {
            ((BoxOpenClose)obj).PostBoxClosed();
        }

        public const int BOX_ANIM_LEVEL_FIRST_START = 0;

        public const int BOX_ANIM_LEVEL_START = 1;

        public const int BOX_ANIM_LEVEL_WON = 2;

        public const int BOX_ANIM_LEVEL_LOST = 3;

        public const int BOX_ANIM_LEVEL_QUIT = 4;

        public const int RESULT_STATE_WAIT = 0;

        public const int RESULT_STATE_SHOW_STAR_BONUS = 1;

        public const int RESULT_STATE_COUNTDOWN_STAR_BONUS = 2;

        public const int RESULT_STATE_HIDE_STAR_BONUS = 3;

        public const int RESULT_STATE_SHOW_TIME_BONUS = 4;

        public const int RESULT_STATE_COUNTDOWN_TIME_BONUS = 5;

        public const int RESULT_STATE_HIDE_TIME_BONUS = 6;

        public const int RESULT_STATE_SHOW_FINAL_SCORE = 7;

        public const int RESULTS_SHOW_ANIM = 0;

        public const int RESULTS_HIDE_ANIM = 1;

        public BaseElement openCloseAnims;

        public BaseElement confettiAnims;

        public BaseElement result;

        public int boxAnim;

        public bool shouldShowConfetti;

        public bool shouldShowImprovedResult;

        public Image stamp;

        public int raState;

        public int timeBonus;

        public int starBonus;

        public int score;

        public float time;

        public float ctime;

        public int cstarBonus;

        public int cscore;

        public float raDelay;

        public boxClosed delegateboxClosed;

        // (Invoke) Token: 0x06000674 RID: 1652
        public delegate void boxClosed();

        private sealed class Confetti : Animation
        {
            public static Confetti Confetti_createWithResID(string resourceName)
            {
                return Confetti_create(Application.GetTexture(resourceName));
            }

            public static Confetti Confetti_create(CTRTexture2D t)
            {
                return (Confetti)new Confetti().InitWithTexture(t);
            }

            public override void Update(float delta)
            {
                base.Update(delta);
                Timeline.UpdateTimeline(ani, delta);
            }

            public Timeline ani;
        }
    }
}
