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
            if (this.boxAnim != 2)
            {
                return;
            }
            bool flag = Mover.moveVariableToTarget(ref this.raDelay, 0.0, 1.0, (double)delta);
            switch (this.raState)
            {
                case -1:
                    {
                        this.cscore = 0;
                        this.ctime = this.time;
                        this.cstarBonus = this.starBonus;
                        ((Text)this.result.getChildWithName("scoreValue")).setString(this.cscore.ToString());
                        Text text27 = (Text)this.result.getChildWithName("dataTitle");
                        Image.setElementPositionWithQuadOffset(text27, 67, 5);
                        text27.setString(Application.getString(655378));
                        ((Text)this.result.getChildWithName("dataValue")).setString(this.cstarBonus.ToString());
                        this.raState = 1;
                        this.raDelay = 1f;
                        return;
                    }
                case 0:
                    if (flag)
                    {
                        this.raState = 1;
                        this.raDelay = 0.2f;
                        return;
                    }
                    break;
                case 1:
                    {
                        Text text28 = (Text)this.result.getChildWithName("dataTitle");
                        text28.setEnabled(true);
                        Text text21 = (Text)this.result.getChildWithName("dataValue");
                        text21.setEnabled(true);
                        Text text22 = (Text)this.result.getChildWithName("scoreValue");
                        text28.color.a = text21.color.a = text22.color.a = 1f - this.raDelay / 0.2f;
                        if (flag)
                        {
                            this.raState = 2;
                            this.raDelay = 1f;
                            return;
                        }
                        break;
                    }
                case 2:
                    {
                        this.cstarBonus = (int)((float)this.starBonus * this.raDelay);
                        this.cscore = (int)((1f - this.raDelay) * (float)this.starBonus);
                        ((Text)this.result.getChildWithName("dataValue")).setString(this.cstarBonus.ToString());
                        Text text29 = (Text)this.result.getChildWithName("scoreValue");
                        text29.setEnabled(true);
                        text29.setString(this.cscore.ToString());
                        if (flag)
                        {
                            this.raState = 3;
                            this.raDelay = 0.2f;
                            return;
                        }
                        break;
                    }
                case 3:
                    {
                        BaseElement baseElement = (Text)this.result.getChildWithName("dataTitle");
                        Text text23 = (Text)this.result.getChildWithName("dataValue");
                        baseElement.color.a = text23.color.a = this.raDelay / 0.2f;
                        if (flag)
                        {
                            this.raState = 4;
                            this.raDelay = 0.2f;
                            int num = (int)Math.Floor((double)(CTRMathHelper.round((double)this.time) / 60f));
                            int num2 = (int)(CTRMathHelper.round((double)this.time) - (float)num * 60f);
                            ((Text)this.result.getChildWithName("dataTitle")).setString(Application.getString(655377));
                            ((Text)this.result.getChildWithName("dataValue")).setString(NSObject.NSS(num.ToString() + ":" + num2.ToString("D2")));
                            return;
                        }
                        break;
                    }
                case 4:
                    {
                        BaseElement baseElement2 = (Text)this.result.getChildWithName("dataTitle");
                        Text text24 = (Text)this.result.getChildWithName("dataValue");
                        baseElement2.color.a = text24.color.a = 1f - this.raDelay / 0.2f;
                        if (flag)
                        {
                            this.raState = 5;
                            this.raDelay = 1f;
                            return;
                        }
                        break;
                    }
                case 5:
                    {
                        this.ctime = this.time * this.raDelay;
                        this.cscore = (int)((float)this.starBonus + (1f - this.raDelay) * (float)this.timeBonus);
                        int num3 = (int)Math.Floor((double)CTRMathHelper.round((double)this.ctime) / 60.0);
                        int num4 = (int)((double)CTRMathHelper.round((double)this.ctime) - (double)num3 * 60.0);
                        ((Text)this.result.getChildWithName("dataValue")).setString(NSObject.NSS(num3.ToString() + ":" + num4.ToString("D2")));
                        ((Text)this.result.getChildWithName("scoreValue")).setString(this.cscore.ToString());
                        if (flag)
                        {
                            this.raState = 6;
                            this.raDelay = 0.2f;
                            return;
                        }
                        break;
                    }
                case 6:
                    {
                        BaseElement baseElement3 = (Text)this.result.getChildWithName("dataTitle");
                        Text text25 = (Text)this.result.getChildWithName("dataValue");
                        baseElement3.color.a = text25.color.a = this.raDelay / 0.2f;
                        if (flag)
                        {
                            this.raState = 7;
                            this.raDelay = 0.2f;
                            Text text30 = (Text)this.result.getChildWithName("dataTitle");
                            Image.setElementPositionWithQuadOffset(text30, 67, 7);
                            text30.setString(Application.getString(655379));
                            ((Text)this.result.getChildWithName("dataValue")).setString("");
                            return;
                        }
                        break;
                    }
                case 7:
                    {
                        BaseElement baseElement4 = (Text)this.result.getChildWithName("dataTitle");
                        Text text26 = (Text)this.result.getChildWithName("dataValue");
                        baseElement4.color.a = text26.color.a = 1f - this.raDelay / 0.2f;
                        if (flag)
                        {
                            this.raState = 8;
                            if (this.shouldShowImprovedResult)
                            {
                                this.stamp.setEnabled(true);
                                this.stamp.playTimeline(0);
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
            if (this.init() != null)
            {
                this.result = (BaseElement)new BaseElement().init();
                this.addChildwithID(this.result, 1);
                this.anchor = this.parentAnchor = 18;
                this.result.anchor = this.result.parentAnchor = 18;
                this.result.setEnabled(false);
                Timeline timeline = new Timeline().initWithMaxKeyFramesOnTrack(2);
                timeline.addKeyFrame(KeyFrame.makeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.addKeyFrame(KeyFrame.makeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
                this.result.addTimelinewithID(timeline, 0);
                timeline = new Timeline().initWithMaxKeyFramesOnTrack(2);
                timeline.addKeyFrame(KeyFrame.makeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.addKeyFrame(KeyFrame.makeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
                this.result.addTimelinewithID(timeline, 1);
                Image image = Image.Image_createWithResIDQuad(67, 14);
                image.anchor = 18;
                image.setName("star1");
                Image.setElementPositionWithQuadOffset(image, 67, 0);
                this.result.addChild(image);
                Image image2 = Image.Image_createWithResIDQuad(67, 14);
                image2.anchor = 18;
                image2.setName("star2");
                Image.setElementPositionWithQuadOffset(image2, 67, 1);
                this.result.addChild(image2);
                Image image3 = Image.Image_createWithResIDQuad(67, 14);
                image3.anchor = 18;
                image3.setName("star3");
                Image.setElementPositionWithQuadOffset(image3, 67, 2);
                this.result.addChild(image3);
                Text text = new Text().initWithFont(Application.getFont(3));
                text.setString(Application.getString(655372));
                Image.setElementPositionWithQuadOffset(text, 67, 3);
                text.anchor = 18;
                text.setName("passText");
                this.result.addChild(text);
                Image image4 = Image.Image_createWithResIDQuad(67, 15);
                image4.anchor = 18;
                Image.setElementPositionWithQuadOffset(image4, 67, 4);
                this.result.addChild(image4);
                this.stamp = Image.Image_createWithResIDQuad(70, 0);
                Timeline timeline2 = new Timeline().initWithMaxKeyFramesOnTrack(7);
                timeline2.addKeyFrame(KeyFrame.makeScale(3.0, 3.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline2.addKeyFrame(KeyFrame.makeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.5));
                timeline2.addKeyFrame(KeyFrame.makeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline2.addKeyFrame(KeyFrame.makeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.5f));
                this.stamp.addTimeline(timeline2);
                this.stamp.anchor = 18;
                this.stamp.setEnabled(false);
                Image.setElementPositionWithQuadOffset(this.stamp, 67, 12);
                this.result.addChild(this.stamp);
                Button button = MenuController.createShortButtonWithTextIDDelegate(Application.getString(655384), 8, b);
                button.anchor = 18;
                Image.setElementPositionWithQuadOffset(button, 67, 11);
                this.result.addChild(button);
                Button button2 = MenuController.createShortButtonWithTextIDDelegate(Application.getString(655385), 9, b);
                button2.anchor = 18;
                Image.setElementPositionWithQuadOffset(button2, 67, 10);
                this.result.addChild(button2);
                Button button3 = MenuController.createShortButtonWithTextIDDelegate(Application.getString(655386), 5, b);
                button3.anchor = 18;
                Image.setElementPositionWithQuadOffset(button3, 67, 9);
                this.result.addChild(button3);
                Text text2 = new Text().initWithFont(Application.getFont(4));
                text2.setName("dataTitle");
                text2.anchor = 18;
                Image.setElementPositionWithQuadOffset(text2, 67, 5);
                this.result.addChild(text2);
                Text text3 = new Text().initWithFont(Application.getFont(4));
                text3.setName("dataValue");
                text3.anchor = 18;
                Image.setElementPositionWithQuadOffset(text3, 67, 6);
                this.result.addChild(text3);
                Text text4 = new Text().initWithFont(Application.getFont(68));
                text4.setName("scoreValue");
                text4.anchor = 18;
                Image.setElementPositionWithQuadOffset(text4, 67, 8);
                this.result.addChild(text4);
                this.confettiAnims = (BaseElement)new BaseElement().init();
                this.result.addChild(this.confettiAnims);
                this.openCloseAnims = null;
                this.boxAnim = -1;
                this.delegateboxClosed = null;
            }
            return this;
        }

        public virtual BaseElement createConfettiParticleNear(Vector p)
        {
            BoxOpenClose.Confetti confetti = BoxOpenClose.Confetti.Confetti_createWithResID(65);
            confetti.doRestoreCutTransparency();
            int num = CTRMathHelper.RND_RANGE(0, 2);
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
            float num4 = (float)CTRMathHelper.RND_RANGE((int)FrameworkTypes.RTPD(-100.0), (int)FrameworkTypes.SCREEN_WIDTH);
            float num5 = (float)CTRMathHelper.RND_RANGE((int)FrameworkTypes.RTPD(-40.0), (int)FrameworkTypes.RTPD(100.0));
            float num6 = CTRMathHelper.FLOAT_RND_RANGE(2, 5);
            int i = confetti.addAnimationDelayLoopFirstLast(0.05, Timeline.LoopType.TIMELINE_REPLAY, num2, num3);
            confetti.ani = confetti.getTimeline(i);
            confetti.ani.playTimeline();
            confetti.ani.jumpToTrackKeyFrame(4, CTRMathHelper.RND_RANGE(0, num3 - num2 - 1));
            Timeline timeline = new Timeline().initWithMaxKeyFramesOnTrack(2);
            timeline.addKeyFrame(KeyFrame.makeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline.addKeyFrame(KeyFrame.makeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, num6));
            timeline.addKeyFrame(KeyFrame.makePos((double)num4, (double)num5, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline.addKeyFrame(KeyFrame.makePos((double)num4, (double)(num5 + CTRMathHelper.FLOAT_RND_RANGE((int)FrameworkTypes.RTPD(150.0), (int)FrameworkTypes.RTPD(400.0))), KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, (double)num6));
            timeline.addKeyFrame(KeyFrame.makeScale(0.0, 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline.addKeyFrame(KeyFrame.makeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.3));
            timeline.addKeyFrame(KeyFrame.makeRotation((double)CTRMathHelper.RND_RANGE(-360, 360), KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline.addKeyFrame(KeyFrame.makeRotation(CTRMathHelper.RND_RANGE(-360, 360), KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, num6));
            confetti.addTimeline(timeline);
            confetti.playTimeline(1);
            return confetti;
        }

        public virtual void levelFirstStart()
        {
            this.boxAnim = 0;
            this.removeOpenCloseAnims();
            this.showOpenAnim();
            if (this.result.isEnabled())
            {
                this.result.playTimeline(1);
            }
        }

        public virtual void levelStart()
        {
            this.boxAnim = 1;
            this.removeOpenCloseAnims();
            this.showOpenAnim();
            if (this.result.isEnabled())
            {
                this.result.playTimeline(1);
            }
        }

        public virtual void levelWon()
        {
            this.boxAnim = 2;
            this.raState = -1;
            this.removeOpenCloseAnims();
            this.showCloseAnim();
            ((Text)this.result.getChildWithName("scoreValue")).setEnabled(false);
            Text text = (Text)this.result.getChildWithName("dataTitle");
            text.setEnabled(false);
            Image.setElementPositionWithQuadOffset(text, 67, 5);
            ((Text)this.result.getChildWithName("dataValue")).setEnabled(false);
            this.result.playTimeline(0);
            this.result.setEnabled(true);
            this.stamp.setEnabled(false);
        }

        public virtual void levelLost()
        {
            this.boxAnim = 3;
            this.removeOpenCloseAnims();
            this.showCloseAnim();
        }

        public virtual void levelQuit()
        {
            this.boxAnim = 4;
            this.result.setEnabled(false);
            this.removeOpenCloseAnims();
            this.showCloseAnim();
        }

        public virtual void showOpenAnim()
        {
            this.showOpenCloseAnim(true);
        }

        public virtual void showCloseAnim()
        {
            this.showOpenCloseAnim(false);
        }

        public virtual void showConfetti()
        {
            for (int i = 0; i < 70; i++)
            {
                this.confettiAnims.addChild(this.createConfettiParticleNear(CTRMathHelper.vectZero));
            }
        }

        public virtual void showOpenCloseAnim(bool open)
        {
            this.createOpenCloseAnims();
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
            this.openCloseAnims.addChild(image);
            Vector quadSize = Image.getQuadSize(num9, 0);
            float num2 = FrameworkTypes.SCREEN_WIDTH / 2f - quadSize.x;
            Image image2 = Image.Image_createWithResIDQuad(num9, 0);
            Image image3 = Image.Image_createWithResIDQuad(num9, 0);
            image2.x = num2;
            image2.rotationCenterX = (float)-(float)image2.width / 2f;
            image3.rotationCenterX = image2.rotationCenterX;
            image3.rotation = 180f;
            image3.x = FrameworkTypes.SCREEN_WIDTH - (FrameworkTypes.SCREEN_WIDTH / 2f - (float)image2.width);
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
                timeline.addKeyFrame(KeyFrame.makePos((double)((float)image2.width - num4), (double)num3, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.addKeyFrame(KeyFrame.makePos((double)num7, (double)num3, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
                timeline.addKeyFrame(KeyFrame.makeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.addKeyFrame(KeyFrame.makeScale(0.0, 1.3, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
            }
            else
            {
                timeline.addKeyFrame(KeyFrame.makePos((double)FrameworkTypes.RTD(-15.0), (double)num3, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.addKeyFrame(KeyFrame.makePos((double)((float)image2.width - num4), (double)num3, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
                timeline.addKeyFrame(KeyFrame.makeScale(0.0, 1.3, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.addKeyFrame(KeyFrame.makeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
            }
            image4.addTimelinewithID(timeline, 0);
            image4.playTimeline(0);
            timeline = new Timeline().initWithMaxKeyFramesOnTrack(2);
            if (open)
            {
                timeline.addKeyFrame(KeyFrame.makePos((double)(FrameworkTypes.SCREEN_WIDTH - (float)image2.width + num5), (double)num3, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.addKeyFrame(KeyFrame.makePos((double)(FrameworkTypes.SCREEN_WIDTH + num8), (double)num3, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
                timeline.addKeyFrame(KeyFrame.makeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.addKeyFrame(KeyFrame.makeScale(0.0, 1.3, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
            }
            else
            {
                timeline.addKeyFrame(KeyFrame.makePos((double)(FrameworkTypes.SCREEN_WIDTH - FrameworkTypes.RTD(9.0)), (double)num3, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.addKeyFrame(KeyFrame.makePos((double)(FrameworkTypes.SCREEN_WIDTH - (float)image2.width + num6), (double)num3, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
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
                timeline.addKeyFrame(KeyFrame.makePos((double)(image2.x + (float)image2.width - FrameworkTypes.RTD(6.0)), 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.addKeyFrame(KeyFrame.makePos(-25.0, 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
                timeline.addKeyFrame(KeyFrame.makeScale(0.0, 1.3, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.addKeyFrame(KeyFrame.makeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
            }
            else
            {
                timeline.addKeyFrame(KeyFrame.makePos((double)(image2.x + 0f), 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.addKeyFrame(KeyFrame.makePos((double)((float)image2.width - 16f), 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
                timeline.addKeyFrame(KeyFrame.makeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.addKeyFrame(KeyFrame.makeScale(0.0, 1.3, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
            }
            image6.addTimelinewithID(timeline, 0);
            image6.playTimeline(0);
            this.openCloseAnims.addChild(image6);
            timeline = new Timeline().initWithMaxKeyFramesOnTrack(2);
            if (open)
            {
                timeline.addKeyFrame(KeyFrame.makePos((double)(FrameworkTypes.SCREEN_WIDTH - (float)image2.width + FrameworkTypes.RTD(7.0)), 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.addKeyFrame(KeyFrame.makePos((double)FrameworkTypes.SCREEN_WIDTH, 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
                timeline.addKeyFrame(KeyFrame.makeScale(0.0, 1.3, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.addKeyFrame(KeyFrame.makeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
            }
            else
            {
                timeline.addKeyFrame(KeyFrame.makePos((double)(FrameworkTypes.SCREEN_WIDTH - 40f), 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.addKeyFrame(KeyFrame.makePos((double)(FrameworkTypes.SCREEN_WIDTH - (float)image2.width + 20f), 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
                timeline.addKeyFrame(KeyFrame.makeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.addKeyFrame(KeyFrame.makeScale(0.0, 1.3, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
            }
            image7.addTimelinewithID(timeline, 0);
            image7.playTimeline(0);
            this.openCloseAnims.addChild(image7);
            this.openCloseAnims.addChild(image2);
            this.openCloseAnims.addChild(image3);
            if (this.boxAnim == 0)
            {
                this.openCloseAnims.addChild(image4);
                this.openCloseAnims.addChild(image5);
            }
        }

        public virtual void timelinereachedKeyFramewithIndex(Timeline t, KeyFrame k, int i)
        {
        }

        public virtual void timelineFinished(Timeline t)
        {
            switch (this.boxAnim)
            {
                case 0:
                case 1:
                    {
                        DelayedDispatcher.DispatchFunc dispatchFunc;
                        if ((dispatchFunc = BoxOpenClose.<> O.< 0 > __selector_removeOpenCloseAnims) == null)
                        {
                            dispatchFunc = BoxOpenClose.<> O.< 0 > __selector_removeOpenCloseAnims = new DelayedDispatcher.DispatchFunc(BoxOpenClose.selector_removeOpenCloseAnims);
                        }
                        NSTimer.registerDelayedObjectCall(dispatchFunc, this, 0.001);
                        if (this.result.isEnabled())
                        {
                            this.confettiAnims.removeAllChilds();
                            this.result.setEnabled(false);
                            return;
                        }
                        break;
                    }
                case 2:
                    {
                        DelayedDispatcher.DispatchFunc dispatchFunc2;
                        if ((dispatchFunc2 = BoxOpenClose.<> O.< 1 > __selector_postBoxClosed) == null)
                        {
                            dispatchFunc2 = BoxOpenClose.<> O.< 1 > __selector_postBoxClosed = new DelayedDispatcher.DispatchFunc(BoxOpenClose.selector_postBoxClosed);
                        }
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
            if (this.delegateboxClosed != null)
            {
                this.delegateboxClosed();
            }
            if (this.shouldShowConfetti)
            {
                this.showConfetti();
            }
        }

        public virtual void removeOpenCloseAnims()
        {
            if (this.getChild(0) != null)
            {
                this.removeChild(this.openCloseAnims);
                this.openCloseAnims = null;
            }
            BaseElement baseElement = (Text)this.result.getChildWithName("dataTitle");
            Text text2 = (Text)this.result.getChildWithName("dataValue");
            Text text3 = (Text)this.result.getChildWithName("scoreValue");
            baseElement.color.a = text2.color.a = text3.color.a = 1f;
        }

        public virtual void createOpenCloseAnims()
        {
            this.openCloseAnims = (BaseElement)new BaseElement().init();
            this.addChildwithID(this.openCloseAnims, 0);
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

        public BoxOpenClose.boxClosed delegateboxClosed;

        // (Invoke) Token: 0x06000674 RID: 1652
        public delegate void boxClosed();

        private class Confetti : Animation
        {
            public static BoxOpenClose.Confetti Confetti_createWithResID(int r)
            {
                return BoxOpenClose.Confetti.Confetti_create(Application.getTexture(r));
            }

            public static BoxOpenClose.Confetti Confetti_create(CTRTexture2D t)
            {
                return (BoxOpenClose.Confetti)new BoxOpenClose.Confetti().initWithTexture(t);
            }

            public override void update(float delta)
            {
                base.update(delta);
                Timeline.updateTimeline(this.ani, delta);
            }

            public Timeline ani;
        }

        [CompilerGenerated]
        private static class <>O
		{
						public static DelayedDispatcher.DispatchFunc<0> __selector_removeOpenCloseAnims;

        public static DelayedDispatcher.DispatchFunc<1> __selector_postBoxClosed;
    }
}
}
