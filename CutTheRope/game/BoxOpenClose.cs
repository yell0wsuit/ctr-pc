using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.visual;
using CutTheRope.ios;
using System;
using System.Runtime.CompilerServices;

namespace CutTheRope.game
{
    internal class BoxOpenClose : BaseElement, TimelineDelegate
    {
        public override void update(float delta)
        {
            base.update(delta);
            if (boxAnim != 2)
            {
                return;
            }
            bool flag = Mover.moveVariableToTarget(ref raDelay, 0.0, 1.0, (double)delta);
            switch (raState)
            {
                case -1:
                    {
                        cscore = 0;
                        ctime = time;
                        cstarBonus = starBonus;
                        ((Text)result.getChildWithName("scoreValue")).setString(cscore.ToString());
                        Text text27 = (Text)result.getChildWithName("dataTitle");
                        Image.setElementPositionWithQuadOffset(text27, 67, 5);
                        text27.setString(Application.getString(655378));
                        ((Text)result.getChildWithName("dataValue")).setString(cstarBonus.ToString());
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
                        Text text28 = (Text)result.getChildWithName("dataTitle");
                        text28.setEnabled(true);
                        Text text21 = (Text)result.getChildWithName("dataValue");
                        text21.setEnabled(true);
                        Text text22 = (Text)result.getChildWithName("scoreValue");
                        text28.color.a = text21.color.a = text22.color.a = 1f - raDelay / 0.2f;
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
                        ((Text)result.getChildWithName("dataValue")).setString(cstarBonus.ToString());
                        Text text29 = (Text)result.getChildWithName("scoreValue");
                        text29.setEnabled(true);
                        text29.setString(cscore.ToString());
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
                        BaseElement baseElement = (Text)result.getChildWithName("dataTitle");
                        Text text23 = (Text)result.getChildWithName("dataValue");
                        baseElement.color.a = text23.color.a = raDelay / 0.2f;
                        if (flag)
                        {
                            raState = 4;
                            raDelay = 0.2f;
                            int num = (int)Math.Floor((double)(round(time) / 60f));
                            int num2 = (int)(round(time) - num * 60f);
                            ((Text)result.getChildWithName("dataTitle")).setString(Application.getString(655377));
                            ((Text)result.getChildWithName("dataValue")).setString(NSS(num.ToString() + ":" + num2.ToString("D2")));
                            return;
                        }
                        break;
                    }
                case 4:
                    {
                        BaseElement baseElement2 = (Text)result.getChildWithName("dataTitle");
                        Text text24 = (Text)result.getChildWithName("dataValue");
                        baseElement2.color.a = text24.color.a = 1f - raDelay / 0.2f;
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
                        cscore = (int)(starBonus + (1f - raDelay) * timeBonus);
                        int num3 = (int)Math.Floor((double)round(ctime) / 60.0);
                        int num4 = (int)((double)round(ctime) - num3 * 60.0);
                        ((Text)result.getChildWithName("dataValue")).setString(NSS(num3.ToString() + ":" + num4.ToString("D2")));
                        ((Text)result.getChildWithName("scoreValue")).setString(cscore.ToString());
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
                        BaseElement baseElement3 = (Text)result.getChildWithName("dataTitle");
                        Text text25 = (Text)result.getChildWithName("dataValue");
                        baseElement3.color.a = text25.color.a = raDelay / 0.2f;
                        if (flag)
                        {
                            raState = 7;
                            raDelay = 0.2f;
                            Text text30 = (Text)result.getChildWithName("dataTitle");
                            Image.setElementPositionWithQuadOffset(text30, 67, 7);
                            text30.setString(Application.getString(655379));
                            ((Text)result.getChildWithName("dataValue")).setString("");
                            return;
                        }
                        break;
                    }
                case 7:
                    {
                        BaseElement baseElement4 = (Text)result.getChildWithName("dataTitle");
                        Text text26 = (Text)result.getChildWithName("dataValue");
                        baseElement4.color.a = text26.color.a = 1f - raDelay / 0.2f;
                        if (flag)
                        {
                            raState = 8;
                            if (shouldShowImprovedResult)
                            {
                                stamp.setEnabled(true);
                                stamp.playTimeline(0);
                            }
                        }
                        break;
                    }
                default:
                    return;
            }
        }

        public virtual NSObject initWithButtonDelegate(ButtonDelegate b)
        {
            if (init() != null)
            {
                result = (BaseElement)new BaseElement().init();
                addChildwithID(result, 1);
                anchor = parentAnchor = 18;
                result.anchor = result.parentAnchor = 18;
                result.setEnabled(false);
                Timeline timeline = new Timeline().initWithMaxKeyFramesOnTrack(2);
                timeline.addKeyFrame(KeyFrame.makeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.addKeyFrame(KeyFrame.makeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
                result.addTimelinewithID(timeline, 0);
                timeline = new Timeline().initWithMaxKeyFramesOnTrack(2);
                timeline.addKeyFrame(KeyFrame.makeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.addKeyFrame(KeyFrame.makeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
                result.addTimelinewithID(timeline, 1);
                Image image = Image.Image_createWithResIDQuad(67, 14);
                image.anchor = 18;
                image.setName("star1");
                Image.setElementPositionWithQuadOffset(image, 67, 0);
                result.addChild(image);
                Image image2 = Image.Image_createWithResIDQuad(67, 14);
                image2.anchor = 18;
                image2.setName("star2");
                Image.setElementPositionWithQuadOffset(image2, 67, 1);
                result.addChild(image2);
                Image image3 = Image.Image_createWithResIDQuad(67, 14);
                image3.anchor = 18;
                image3.setName("star3");
                Image.setElementPositionWithQuadOffset(image3, 67, 2);
                result.addChild(image3);
                Text text = new Text().initWithFont(Application.getFont(3));
                text.setString(Application.getString(655372));
                Image.setElementPositionWithQuadOffset(text, 67, 3);
                text.anchor = 18;
                text.setName("passText");
                result.addChild(text);
                Image image4 = Image.Image_createWithResIDQuad(67, 15);
                image4.anchor = 18;
                Image.setElementPositionWithQuadOffset(image4, 67, 4);
                result.addChild(image4);
                stamp = Image.Image_createWithResIDQuad(70, 0);
                Timeline timeline2 = new Timeline().initWithMaxKeyFramesOnTrack(7);
                timeline2.addKeyFrame(KeyFrame.makeScale(3.0, 3.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline2.addKeyFrame(KeyFrame.makeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.5));
                timeline2.addKeyFrame(KeyFrame.makeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline2.addKeyFrame(KeyFrame.makeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.5f));
                stamp.addTimeline(timeline2);
                stamp.anchor = 18;
                stamp.setEnabled(false);
                Image.setElementPositionWithQuadOffset(stamp, 67, 12);
                result.addChild(stamp);
                Button button = MenuController.createShortButtonWithTextIDDelegate(Application.getString(655384), 8, b);
                button.anchor = 18;
                Image.setElementPositionWithQuadOffset(button, 67, 11);
                result.addChild(button);
                Button button2 = MenuController.createShortButtonWithTextIDDelegate(Application.getString(655385), 9, b);
                button2.anchor = 18;
                Image.setElementPositionWithQuadOffset(button2, 67, 10);
                result.addChild(button2);
                Button button3 = MenuController.createShortButtonWithTextIDDelegate(Application.getString(655386), 5, b);
                button3.anchor = 18;
                Image.setElementPositionWithQuadOffset(button3, 67, 9);
                result.addChild(button3);
                Text text2 = new Text().initWithFont(Application.getFont(4));
                text2.setName("dataTitle");
                text2.anchor = 18;
                Image.setElementPositionWithQuadOffset(text2, 67, 5);
                result.addChild(text2);
                Text text3 = new Text().initWithFont(Application.getFont(4));
                text3.setName("dataValue");
                text3.anchor = 18;
                Image.setElementPositionWithQuadOffset(text3, 67, 6);
                result.addChild(text3);
                Text text4 = new Text().initWithFont(Application.getFont(68));
                text4.setName("scoreValue");
                text4.anchor = 18;
                Image.setElementPositionWithQuadOffset(text4, 67, 8);
                result.addChild(text4);
                confettiAnims = (BaseElement)new BaseElement().init();
                result.addChild(confettiAnims);
                openCloseAnims = null;
                boxAnim = -1;
                delegateboxClosed = null;
            }
            return this;
        }

        public virtual BaseElement createConfettiParticleNear(Vector p)
        {
            Confetti confetti = Confetti.Confetti_createWithResID(65);
            confetti.doRestoreCutTransparency();
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
            int i = confetti.addAnimationDelayLoopFirstLast(0.05, Timeline.LoopType.TIMELINE_REPLAY, num2, num3);
            confetti.ani = confetti.getTimeline(i);
            confetti.ani.playTimeline();
            confetti.ani.jumpToTrackKeyFrame(4, RND_RANGE(0, num3 - num2 - 1));
            Timeline timeline = new Timeline().initWithMaxKeyFramesOnTrack(2);
            timeline.addKeyFrame(KeyFrame.makeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline.addKeyFrame(KeyFrame.makeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, num6));
            timeline.addKeyFrame(KeyFrame.makePos((double)num4, (double)num5, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline.addKeyFrame(KeyFrame.makePos((double)num4, (double)(num5 + FLOAT_RND_RANGE((int)RTPD(150.0), (int)RTPD(400.0))), KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, (double)num6));
            timeline.addKeyFrame(KeyFrame.makeScale(0.0, 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline.addKeyFrame(KeyFrame.makeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.3));
            timeline.addKeyFrame(KeyFrame.makeRotation(RND_RANGE(-360, 360), KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline.addKeyFrame(KeyFrame.makeRotation(RND_RANGE(-360, 360), KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, num6));
            confetti.addTimeline(timeline);
            confetti.playTimeline(1);
            return confetti;
        }

        public virtual void levelFirstStart()
        {
            boxAnim = 0;
            removeOpenCloseAnims();
            showOpenAnim();
            if (result.isEnabled())
            {
                result.playTimeline(1);
            }
        }

        public virtual void levelStart()
        {
            boxAnim = 1;
            removeOpenCloseAnims();
            showOpenAnim();
            if (result.isEnabled())
            {
                result.playTimeline(1);
            }
        }

        public virtual void levelWon()
        {
            boxAnim = 2;
            raState = -1;
            removeOpenCloseAnims();
            showCloseAnim();
            ((Text)result.getChildWithName("scoreValue")).setEnabled(false);
            Text text = (Text)result.getChildWithName("dataTitle");
            text.setEnabled(false);
            Image.setElementPositionWithQuadOffset(text, 67, 5);
            ((Text)result.getChildWithName("dataValue")).setEnabled(false);
            result.playTimeline(0);
            result.setEnabled(true);
            stamp.setEnabled(false);
        }

        public virtual void levelLost()
        {
            boxAnim = 3;
            removeOpenCloseAnims();
            showCloseAnim();
        }

        public virtual void levelQuit()
        {
            boxAnim = 4;
            result.setEnabled(false);
            removeOpenCloseAnims();
            showCloseAnim();
        }

        public virtual void showOpenAnim()
        {
            showOpenCloseAnim(true);
        }

        public virtual void showCloseAnim()
        {
            showOpenCloseAnim(false);
        }

        public virtual void showConfetti()
        {
            for (int i = 0; i < 70; i++)
            {
                confettiAnims.addChild(createConfettiParticleNear(vectZero));
            }
        }

        public virtual void showOpenCloseAnim(bool open)
        {
            createOpenCloseAnims();
            CTRRootController cTRRootController = (CTRRootController)Application.sharedRootController();
            int num9 = 126 + cTRRootController.getPack();
            Image image = Image.Image_createWithResIDQuad(67, 16);
            image.rotationCenterX = (float)-(float)image.width / 2f + 1f;
            image.rotationCenterY = (float)-(float)image.height / 2f + 1f;
            image.scaleX = image.scaleY = 4f;
            Timeline timeline = new Timeline().initWithMaxKeyFramesOnTrack(2);
            if (open)
            {
                timeline.addKeyFrame(KeyFrame.makePos(0.0, 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.addKeyFrame(KeyFrame.makePos((double)(-(double)image.width * 4), 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
            }
            else
            {
                timeline.addKeyFrame(KeyFrame.makePos((double)(-(double)image.width * 4), 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.addKeyFrame(KeyFrame.makePos(0.0, 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
            }
            image.addTimelinewithID(timeline, 0);
            image.playTimeline(0);
            timeline.delegateTimelineDelegate = this;
            openCloseAnims.addChild(image);
            Vector quadSize = Image.getQuadSize(num9, 0);
            float num2 = SCREEN_WIDTH / 2f - quadSize.x;
            Image image2 = Image.Image_createWithResIDQuad(num9, 0);
            Image image3 = Image.Image_createWithResIDQuad(num9, 0);
            image2.x = num2;
            image2.rotationCenterX = (float)-(float)image2.width / 2f;
            image3.rotationCenterX = image2.rotationCenterX;
            image3.rotation = 180f;
            image3.x = SCREEN_WIDTH - (SCREEN_WIDTH / 2f - image2.width);
            image3.y = -0.5f;
            timeline = new Timeline().initWithMaxKeyFramesOnTrack(2);
            if (open)
            {
                timeline.addKeyFrame(KeyFrame.makeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.addKeyFrame(KeyFrame.makeScale(0.0, 1.1, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
                timeline.addKeyFrame(KeyFrame.makeColor(RGBAColor.MakeRGBA(0.85, 0.85, 0.85, 1.0), KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.addKeyFrame(KeyFrame.makeColor(RGBAColor.whiteRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
            }
            else
            {
                timeline.addKeyFrame(KeyFrame.makeScale(0.0, 1.1, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.addKeyFrame(KeyFrame.makeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
                timeline.addKeyFrame(KeyFrame.makeColor(RGBAColor.whiteRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.addKeyFrame(KeyFrame.makeColor(RGBAColor.MakeRGBA(0.85, 0.85, 0.85, 1.0), KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
            }
            image2.addTimelinewithID(timeline, 0);
            image2.playTimeline(0);
            timeline = new Timeline().initWithMaxKeyFramesOnTrack(2);
            if (open)
            {
                timeline.addKeyFrame(KeyFrame.makeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.addKeyFrame(KeyFrame.makeScale(0.0, 1.1, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
                timeline.addKeyFrame(KeyFrame.makeColor(RGBAColor.MakeRGBA(0.85, 0.85, 0.85, 1.0), KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.addKeyFrame(KeyFrame.makeColor(RGBAColor.MakeRGBA(0.4, 0.4, 0.4, 1.0), KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
            }
            else
            {
                timeline.addKeyFrame(KeyFrame.makeScale(0.0, 1.1, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.addKeyFrame(KeyFrame.makeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
                timeline.addKeyFrame(KeyFrame.makeColor(RGBAColor.MakeRGBA(0.4, 0.4, 0.4, 1.0), KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.addKeyFrame(KeyFrame.makeColor(RGBAColor.MakeRGBA(0.85, 0.85, 0.85, 1.0), KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
            }
            image3.addTimelinewithID(timeline, 0);
            image3.playTimeline(0);
            Image image4 = Image.Image_createWithResIDQuad(5, 0);
            Image image5 = Image.Image_createWithResIDQuad(5, 1);
            float num3 = 80f;
            float num4 = 50f;
            float num5 = 10f;
            float num6 = 10f;
            float num7 = -40f;
            float num8 = 25f;
            timeline = new Timeline().initWithMaxKeyFramesOnTrack(2);
            if (open)
            {
                timeline.addKeyFrame(KeyFrame.makePos((double)(image2.width - num4), (double)num3, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.addKeyFrame(KeyFrame.makePos((double)num7, (double)num3, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
                timeline.addKeyFrame(KeyFrame.makeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.addKeyFrame(KeyFrame.makeScale(0.0, 1.3, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
            }
            else
            {
                timeline.addKeyFrame(KeyFrame.makePos((double)RTD(-15.0), (double)num3, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.addKeyFrame(KeyFrame.makePos((double)(image2.width - num4), (double)num3, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
                timeline.addKeyFrame(KeyFrame.makeScale(0.0, 1.3, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.addKeyFrame(KeyFrame.makeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
            }
            image4.addTimelinewithID(timeline, 0);
            image4.playTimeline(0);
            timeline = new Timeline().initWithMaxKeyFramesOnTrack(2);
            if (open)
            {
                timeline.addKeyFrame(KeyFrame.makePos((double)(SCREEN_WIDTH - image2.width + num5), (double)num3, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.addKeyFrame(KeyFrame.makePos((double)(SCREEN_WIDTH + num8), (double)num3, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
                timeline.addKeyFrame(KeyFrame.makeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.addKeyFrame(KeyFrame.makeScale(0.0, 1.3, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
            }
            else
            {
                timeline.addKeyFrame(KeyFrame.makePos((double)(SCREEN_WIDTH - RTD(9.0)), (double)num3, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.addKeyFrame(KeyFrame.makePos((double)(SCREEN_WIDTH - image2.width + num6), (double)num3, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
                timeline.addKeyFrame(KeyFrame.makeScale(0.0, 1.3, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.addKeyFrame(KeyFrame.makeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
            }
            image5.addTimelinewithID(timeline, 0);
            image5.playTimeline(0);
            Image image6 = Image.Image_createWithResIDQuad(num9, 1);
            Image image7 = Image.Image_createWithResIDQuad(num9, 1);
            image6.rotationCenterX = (float)-(float)image6.width / 2f;
            image7.rotationCenterX = image6.rotationCenterX;
            timeline = new Timeline().initWithMaxKeyFramesOnTrack(2);
            if (open)
            {
                timeline.addKeyFrame(KeyFrame.makePos((double)(image2.x + image2.width - RTD(6.0)), 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.addKeyFrame(KeyFrame.makePos(-25.0, 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
                timeline.addKeyFrame(KeyFrame.makeScale(0.0, 1.3, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.addKeyFrame(KeyFrame.makeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
            }
            else
            {
                timeline.addKeyFrame(KeyFrame.makePos((double)(image2.x + 0f), 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.addKeyFrame(KeyFrame.makePos((double)(image2.width - 16f), 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
                timeline.addKeyFrame(KeyFrame.makeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.addKeyFrame(KeyFrame.makeScale(0.0, 1.3, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
            }
            image6.addTimelinewithID(timeline, 0);
            image6.playTimeline(0);
            openCloseAnims.addChild(image6);
            timeline = new Timeline().initWithMaxKeyFramesOnTrack(2);
            if (open)
            {
                timeline.addKeyFrame(KeyFrame.makePos((double)(SCREEN_WIDTH - image2.width + RTD(7.0)), 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.addKeyFrame(KeyFrame.makePos(SCREEN_WIDTH, 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
                timeline.addKeyFrame(KeyFrame.makeScale(0.0, 1.3, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.addKeyFrame(KeyFrame.makeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
            }
            else
            {
                timeline.addKeyFrame(KeyFrame.makePos((double)(SCREEN_WIDTH - 40f), 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.addKeyFrame(KeyFrame.makePos((double)(SCREEN_WIDTH - image2.width + 20f), 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
                timeline.addKeyFrame(KeyFrame.makeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.addKeyFrame(KeyFrame.makeScale(0.0, 1.3, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
            }
            image7.addTimelinewithID(timeline, 0);
            image7.playTimeline(0);
            openCloseAnims.addChild(image7);
            openCloseAnims.addChild(image2);
            openCloseAnims.addChild(image3);
            if (boxAnim == 0)
            {
                openCloseAnims.addChild(image4);
                openCloseAnims.addChild(image5);
            }
        }

        public virtual void timelinereachedKeyFramewithIndex(Timeline t, KeyFrame k, int i)
        {
        }

        public virtual void timelineFinished(Timeline t)
        {
            switch (boxAnim)
            {
                case 0:
                case 1:
                    {
                        DelayedDispatcher.DispatchFunc dispatchFunc = new DelayedDispatcher.DispatchFunc(selector_removeOpenCloseAnims);
                        NSTimer.registerDelayedObjectCall(dispatchFunc, this, 0.001);
                        if (result.isEnabled())
                        {
                            confettiAnims.removeAllChilds();
                            result.setEnabled(false);
                            return;
                        }
                        break;
                    }
                case 2:
                    {
                        DelayedDispatcher.DispatchFunc dispatchFunc2 = new DelayedDispatcher.DispatchFunc(selector_postBoxClosed);
                        NSTimer.registerDelayedObjectCall(dispatchFunc2, this, 0.001);
                        break;
                    }
                case 3:
                    break;
                case 4:
                    Application.sharedRootController().getCurrentController().deactivate();
                    return;
                default:
                    return;
            }
        }

        public virtual void postBoxClosed()
        {
            if (delegateboxClosed != null)
            {
                delegateboxClosed();
            }
            if (shouldShowConfetti)
            {
                showConfetti();
            }
        }

        public virtual void removeOpenCloseAnims()
        {
            if (getChild(0) != null)
            {
                removeChild(openCloseAnims);
                openCloseAnims = null;
            }
            BaseElement baseElement = (Text)result.getChildWithName("dataTitle");
            Text text2 = (Text)result.getChildWithName("dataValue");
            Text text3 = (Text)result.getChildWithName("scoreValue");
            baseElement.color.a = text2.color.a = text3.color.a = 1f;
        }

        public virtual void createOpenCloseAnims()
        {
            openCloseAnims = (BaseElement)new BaseElement().init();
            addChildwithID(openCloseAnims, 0);
        }

        private static void selector_removeOpenCloseAnims(NSObject obj)
        {
            ((BoxOpenClose)obj).removeOpenCloseAnims();
        }

        private static void selector_postBoxClosed(NSObject obj)
        {
            ((BoxOpenClose)obj).postBoxClosed();
        }

        private const float aminationTime = 0.5f;

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

        private class Confetti : Animation
        {
            public static Confetti Confetti_createWithResID(int r)
            {
                return Confetti_create(Application.getTexture(r));
            }

            public static Confetti Confetti_create(CTRTexture2D t)
            {
                return (Confetti)new Confetti().initWithTexture(t);
            }

            public override void update(float delta)
            {
                base.update(delta);
                Timeline.updateTimeline(ani, delta);
            }

            public Timeline ani;
        }
    }
}
