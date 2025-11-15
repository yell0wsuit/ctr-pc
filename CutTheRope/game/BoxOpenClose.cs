using System;
using System.Globalization;

using CutTheRope.Helpers;
using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.visual;

namespace CutTheRope.game
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
            bool flag = Mover.MoveVariableToTarget(ref raDelay, 0.0, 1.0, (double)delta);
            switch (raState)
            {
                case -1:
                    {
                        cscore = 0;
                        ctime = time;
                        cstarBonus = starBonus;
                        ((Text)result.GetChildWithName("scoreValue")).SetString(cscore.ToString(CultureInfo.InvariantCulture));
                        Text text27 = (Text)result.GetChildWithName("dataTitle");
                        Image.SetElementPositionWithQuadOffset(text27, 67, 5);
                        text27.SetString(Application.GetString(655378));
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
                        text28.color.a = text21.color.a = text22.color.a = 1f - (raDelay / 0.2f);
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
                        baseElement.color.a = text23.color.a = raDelay / 0.2f;
                        if (flag)
                        {
                            raState = 4;
                            raDelay = 0.2f;
                            int num = (int)Math.Floor((double)(Round(time) / 60f));
                            int num2 = (int)(Round(time) - (num * 60f));
                            ((Text)result.GetChildWithName("dataTitle")).SetString(Application.GetString(655377));
                            ((Text)result.GetChildWithName("dataValue")).SetString(num.ToString(CultureInfo.InvariantCulture) + ":" + num2.ToString("D2", CultureInfo.InvariantCulture));
                            return;
                        }
                        break;
                    }
                case 4:
                    {
                        BaseElement baseElement2 = (Text)result.GetChildWithName("dataTitle");
                        Text text24 = (Text)result.GetChildWithName("dataValue");
                        baseElement2.color.a = text24.color.a = 1f - (raDelay / 0.2f);
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
                        int num3 = (int)Math.Floor((double)Round(ctime) / 60.0);
                        int num4 = (int)((double)Round(ctime) - (num3 * 60.0));
                        ((Text)result.GetChildWithName("dataValue")).SetString(num3.ToString(CultureInfo.InvariantCulture) + ":" + num4.ToString("D2", CultureInfo.InvariantCulture));
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
                        baseElement3.color.a = text25.color.a = raDelay / 0.2f;
                        if (flag)
                        {
                            raState = 7;
                            raDelay = 0.2f;
                            Text text30 = (Text)result.GetChildWithName("dataTitle");
                            Image.SetElementPositionWithQuadOffset(text30, 67, 7);
                            text30.SetString(Application.GetString(655379));
                            ((Text)result.GetChildWithName("dataValue")).SetString("");
                            return;
                        }
                        break;
                    }
                case 7:
                    {
                        BaseElement baseElement4 = (Text)result.GetChildWithName("dataTitle");
                        Text text26 = (Text)result.GetChildWithName("dataValue");
                        baseElement4.color.a = text26.color.a = 1f - (raDelay / 0.2f);
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
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
            result.AddTimelinewithID(timeline, 0);
            timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
            result.AddTimelinewithID(timeline, 1);
            Image image = Image.Image_createWithResIDQuad(67, 14);
            image.anchor = 18;
            image.SetName("star1");
            Image.SetElementPositionWithQuadOffset(image, 67, 0);
            _ = result.AddChild(image);
            Image image2 = Image.Image_createWithResIDQuad(67, 14);
            image2.anchor = 18;
            image2.SetName("star2");
            Image.SetElementPositionWithQuadOffset(image2, 67, 1);
            _ = result.AddChild(image2);
            Image image3 = Image.Image_createWithResIDQuad(67, 14);
            image3.anchor = 18;
            image3.SetName("star3");
            Image.SetElementPositionWithQuadOffset(image3, 67, 2);
            _ = result.AddChild(image3);
            Text text = new Text().InitWithFont(Application.GetFont(3));
            text.SetString(Application.GetString(655372));
            Image.SetElementPositionWithQuadOffset(text, 67, 3);
            text.anchor = 18;
            text.SetName("passText");
            _ = result.AddChild(text);
            Image image4 = Image.Image_createWithResIDQuad(67, 15);
            image4.anchor = 18;
            Image.SetElementPositionWithQuadOffset(image4, 67, 4);
            _ = result.AddChild(image4);
            stamp = Image.Image_createWithResIDQuad(70, 0);
            Timeline timeline2 = new Timeline().InitWithMaxKeyFramesOnTrack(7);
            timeline2.AddKeyFrame(KeyFrame.MakeScale(3.0, 3.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline2.AddKeyFrame(KeyFrame.MakeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.5));
            timeline2.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline2.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.5f));
            _ = stamp.AddTimeline(timeline2);
            stamp.anchor = 18;
            stamp.SetEnabled(false);
            Image.SetElementPositionWithQuadOffset(stamp, 67, 12);
            _ = result.AddChild(stamp);
            Button button = MenuController.CreateShortButtonWithTextIDDelegate(Application.GetString(655384), 8, b);
            button.anchor = 18;
            Image.SetElementPositionWithQuadOffset(button, 67, 11);
            _ = result.AddChild(button);
            Button button2 = MenuController.CreateShortButtonWithTextIDDelegate(Application.GetString(655385), 9, b);
            button2.anchor = 18;
            Image.SetElementPositionWithQuadOffset(button2, 67, 10);
            _ = result.AddChild(button2);
            Button button3 = MenuController.CreateShortButtonWithTextIDDelegate(Application.GetString(655386), 5, b);
            button3.anchor = 18;
            Image.SetElementPositionWithQuadOffset(button3, 67, 9);
            _ = result.AddChild(button3);
            Text text2 = new Text().InitWithFont(Application.GetFont(4));
            text2.SetName("dataTitle");
            text2.anchor = 18;
            Image.SetElementPositionWithQuadOffset(text2, 67, 5);
            _ = result.AddChild(text2);
            Text text3 = new Text().InitWithFont(Application.GetFont(4));
            text3.SetName("dataValue");
            text3.anchor = 18;
            Image.SetElementPositionWithQuadOffset(text3, 67, 6);
            _ = result.AddChild(text3);
            Text text4 = new Text().InitWithFont(Application.GetFont(68));
            text4.SetName("scoreValue");
            text4.anchor = 18;
            Image.SetElementPositionWithQuadOffset(text4, 67, 8);
            _ = result.AddChild(text4);
            confettiAnims = new BaseElement();
            _ = result.AddChild(confettiAnims);
            openCloseAnims = null;
            boxAnim = -1;
            delegateboxClosed = null;
            return this;
        }

        public static BaseElement CreateConfettiParticleNear(Vector p)
        {
            Confetti confetti = Confetti.Confetti_createWithResID(65);
            confetti.DoRestoreCutTransparency();
            int num = RND_RANGE(0, 2);
            int num2 = 18;
            int num3 = 26;
            if (num != 1)
            {
                if (num == 2)
                {
                    num2 = 0;
                    num3 = 8;
                }
            }
            else
            {
                num2 = 9;
                num3 = 17;
            }
            float num4 = RND_RANGE((int)RTPD(-100.0), (int)SCREEN_WIDTH);
            float num5 = RND_RANGE((int)RTPD(-40.0), (int)RTPD(100.0));
            float num6 = FLOAT_RND_RANGE(2, 5);
            int i = confetti.AddAnimationDelayLoopFirstLast(0.05, Timeline.LoopType.TIMELINE_REPLAY, num2, num3);
            confetti.ani = confetti.GetTimeline(i);
            confetti.ani.PlayTimeline();
            confetti.ani.JumpToTrackKeyFrame(4, RND_RANGE(0, num3 - num2 - 1));
            Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, num6));
            timeline.AddKeyFrame(KeyFrame.MakePos((double)num4, (double)num5, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline.AddKeyFrame(KeyFrame.MakePos((double)num4, (double)(num5 + FLOAT_RND_RANGE((int)RTPD(150.0), (int)RTPD(400.0))), KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, (double)num6));
            timeline.AddKeyFrame(KeyFrame.MakeScale(0.0, 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline.AddKeyFrame(KeyFrame.MakeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.3));
            timeline.AddKeyFrame(KeyFrame.MakeRotation(RND_RANGE(-360, 360), KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline.AddKeyFrame(KeyFrame.MakeRotation(RND_RANGE(-360, 360), KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, num6));
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
            Image.SetElementPositionWithQuadOffset(text, 67, 5);
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
                _ = confettiAnims.AddChild(CreateConfettiParticleNear(vectZero));
            }
        }

        public void ShowOpenCloseAnim(bool open)
        {
            CreateOpenCloseAnims();
            CTRRootController cTRRootController = (CTRRootController)Application.SharedRootController();
            int num9 = 126 + cTRRootController.GetPack();
            Image image = Image.Image_createWithResIDQuad(67, 16);
            image.rotationCenterX = ((float)-(float)image.width / 2f) + 1f;
            image.rotationCenterY = ((float)-(float)image.height / 2f) + 1f;
            image.scaleX = image.scaleY = 4f;
            Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            if (open)
            {
                timeline.AddKeyFrame(KeyFrame.MakePos(0.0, 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.AddKeyFrame(KeyFrame.MakePos((double)(-(double)image.width * 4), 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
            }
            else
            {
                timeline.AddKeyFrame(KeyFrame.MakePos((double)(-(double)image.width * 4), 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.AddKeyFrame(KeyFrame.MakePos(0.0, 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
            }
            image.AddTimelinewithID(timeline, 0);
            image.PlayTimeline(0);
            timeline.delegateTimelineDelegate = this;
            _ = openCloseAnims.AddChild(image);
            Vector quadSize = Image.GetQuadSize(num9, 0);
            float num2 = (SCREEN_WIDTH / 2f) - quadSize.x;
            Image image2 = Image.Image_createWithResIDQuad(num9, 0);
            Image image3 = Image.Image_createWithResIDQuad(num9, 0);
            image2.x = num2;
            image2.rotationCenterX = (float)-(float)image2.width / 2f;
            image3.rotationCenterX = image2.rotationCenterX;
            image3.rotation = 180f;
            image3.x = SCREEN_WIDTH - ((SCREEN_WIDTH / 2f) - image2.width);
            image3.y = -0.5f;
            timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            if (open)
            {
                timeline.AddKeyFrame(KeyFrame.MakeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.AddKeyFrame(KeyFrame.MakeScale(0.0, 1.1, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
                timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.MakeRGBA(0.85, 0.85, 0.85, 1.0), KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.whiteRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
            }
            else
            {
                timeline.AddKeyFrame(KeyFrame.MakeScale(0.0, 1.1, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.AddKeyFrame(KeyFrame.MakeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
                timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.whiteRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.MakeRGBA(0.85, 0.85, 0.85, 1.0), KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
            }
            image2.AddTimelinewithID(timeline, 0);
            image2.PlayTimeline(0);
            timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            if (open)
            {
                timeline.AddKeyFrame(KeyFrame.MakeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.AddKeyFrame(KeyFrame.MakeScale(0.0, 1.1, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
                timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.MakeRGBA(0.85, 0.85, 0.85, 1.0), KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.MakeRGBA(0.4, 0.4, 0.4, 1.0), KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
            }
            else
            {
                timeline.AddKeyFrame(KeyFrame.MakeScale(0.0, 1.1, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.AddKeyFrame(KeyFrame.MakeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
                timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.MakeRGBA(0.4, 0.4, 0.4, 1.0), KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.MakeRGBA(0.85, 0.85, 0.85, 1.0), KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
            }
            image3.AddTimelinewithID(timeline, 0);
            image3.PlayTimeline(0);
            Image image4 = Image.Image_createWithResIDQuad(5, 0);
            Image image5 = Image.Image_createWithResIDQuad(5, 1);
            float num3 = 80f;
            float num4 = 50f;
            float num5 = 10f;
            float num6 = 10f;
            float num7 = -40f;
            float num8 = 25f;
            timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            if (open)
            {
                timeline.AddKeyFrame(KeyFrame.MakePos((double)(image2.width - num4), (double)num3, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.AddKeyFrame(KeyFrame.MakePos((double)num7, (double)num3, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
                timeline.AddKeyFrame(KeyFrame.MakeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.AddKeyFrame(KeyFrame.MakeScale(0.0, 1.3, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
            }
            else
            {
                timeline.AddKeyFrame(KeyFrame.MakePos((double)RTD(-15.0), (double)num3, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.AddKeyFrame(KeyFrame.MakePos((double)(image2.width - num4), (double)num3, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
                timeline.AddKeyFrame(KeyFrame.MakeScale(0.0, 1.3, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.AddKeyFrame(KeyFrame.MakeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
            }
            image4.AddTimelinewithID(timeline, 0);
            image4.PlayTimeline(0);
            timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            if (open)
            {
                timeline.AddKeyFrame(KeyFrame.MakePos((double)(SCREEN_WIDTH - image2.width + num5), (double)num3, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.AddKeyFrame(KeyFrame.MakePos((double)(SCREEN_WIDTH + num8), (double)num3, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
                timeline.AddKeyFrame(KeyFrame.MakeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.AddKeyFrame(KeyFrame.MakeScale(0.0, 1.3, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
            }
            else
            {
                timeline.AddKeyFrame(KeyFrame.MakePos((double)(SCREEN_WIDTH - RTD(9.0)), (double)num3, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.AddKeyFrame(KeyFrame.MakePos((double)(SCREEN_WIDTH - image2.width + num6), (double)num3, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
                timeline.AddKeyFrame(KeyFrame.MakeScale(0.0, 1.3, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.AddKeyFrame(KeyFrame.MakeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
            }
            image5.AddTimelinewithID(timeline, 0);
            image5.PlayTimeline(0);
            Image image6 = Image.Image_createWithResIDQuad(num9, 1);
            Image image7 = Image.Image_createWithResIDQuad(num9, 1);
            image6.rotationCenterX = (float)-(float)image6.width / 2f;
            image7.rotationCenterX = image6.rotationCenterX;
            timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            if (open)
            {
                timeline.AddKeyFrame(KeyFrame.MakePos((double)(image2.x + image2.width - RTD(6.0)), 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.AddKeyFrame(KeyFrame.MakePos(-25.0, 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
                timeline.AddKeyFrame(KeyFrame.MakeScale(0.0, 1.3, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.AddKeyFrame(KeyFrame.MakeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
            }
            else
            {
                timeline.AddKeyFrame(KeyFrame.MakePos((double)(image2.x + 0f), 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.AddKeyFrame(KeyFrame.MakePos((double)(image2.width - 16f), 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
                timeline.AddKeyFrame(KeyFrame.MakeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.AddKeyFrame(KeyFrame.MakeScale(0.0, 1.3, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
            }
            image6.AddTimelinewithID(timeline, 0);
            image6.PlayTimeline(0);
            _ = openCloseAnims.AddChild(image6);
            timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            if (open)
            {
                timeline.AddKeyFrame(KeyFrame.MakePos((double)(SCREEN_WIDTH - image2.width + RTD(7.0)), 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.AddKeyFrame(KeyFrame.MakePos(SCREEN_WIDTH, 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
                timeline.AddKeyFrame(KeyFrame.MakeScale(0.0, 1.3, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.AddKeyFrame(KeyFrame.MakeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
            }
            else
            {
                timeline.AddKeyFrame(KeyFrame.MakePos((double)(SCREEN_WIDTH - 40f), 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.AddKeyFrame(KeyFrame.MakePos((double)(SCREEN_WIDTH - image2.width + 20f), 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
                timeline.AddKeyFrame(KeyFrame.MakeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.AddKeyFrame(KeyFrame.MakeScale(0.0, 1.3, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
            }
            image7.AddTimelinewithID(timeline, 0);
            image7.PlayTimeline(0);
            _ = openCloseAnims.AddChild(image7);
            _ = openCloseAnims.AddChild(image2);
            _ = openCloseAnims.AddChild(image3);
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
                        TimerManager.RegisterDelayedObjectCall(dispatchFunc, this, 0.001);
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
                        TimerManager.RegisterDelayedObjectCall(dispatchFunc2, this, 0.001);
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
            baseElement.color.a = text2.color.a = text3.color.a = 1f;
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
            public static Confetti Confetti_createWithResID(int r)
            {
                return Confetti_create(Application.GetTexture(r));
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
