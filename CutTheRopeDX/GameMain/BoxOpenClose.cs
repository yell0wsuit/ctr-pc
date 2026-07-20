using System;
using System.Globalization;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.Helpers;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Manages the level transition, result panel, score countdown, and confetti effects.
    /// </summary>
    internal sealed class BoxOpenClose : BaseElement, ITimelineDelegate
    {
        /// <inheritdoc />
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
                        Image.SetElementPositionWithQuadOffset(text27, Resources.Img.MenuResults, 5);
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
                            Image.SetElementPositionWithQuadOffset(text30, Resources.Img.MenuResults, 7);
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

        /// <summary>
        /// Initializes the transition box UI, result panel, buttons, and score labels.
        /// </summary>
        /// <param name="b">Button delegate that receives result-panel button events.</param>
        /// <returns>The initialized transition box instance.</returns>
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
            Image image = Image.Image_createWithResIDQuad(Resources.Img.MenuResults, 14);
            image.anchor = 18;
            image.SetName("star1");
            Image.SetElementPositionWithQuadOffset(image, Resources.Img.MenuResults, 0);
            _ = result.AddChild(image);
            Image image2 = Image.Image_createWithResIDQuad(Resources.Img.MenuResults, 14);
            image2.anchor = 18;
            image2.SetName("star2");
            Image.SetElementPositionWithQuadOffset(image2, Resources.Img.MenuResults, 1);
            _ = result.AddChild(image2);
            Image image3 = Image.Image_createWithResIDQuad(Resources.Img.MenuResults, 14);
            image3.anchor = 18;
            image3.SetName("star3");
            Image.SetElementPositionWithQuadOffset(image3, Resources.Img.MenuResults, 2);
            _ = result.AddChild(image3);
            Text text = new Text().InitWithFont(Application.GetFont(Resources.Fnt.BigFont));
            text.SetString(Application.GetString("LEVEL_CLEARED1"));
            Image.SetElementPositionWithQuadOffset(text, Resources.Img.MenuResults, 3);
            text.anchor = 18;
            text.SetName("passText");
            _ = result.AddChild(text);
            Image image4 = Image.Image_createWithResIDQuad(Resources.Img.MenuResults, 15);
            image4.anchor = 18;
            Image.SetElementPositionWithQuadOffset(image4, Resources.Img.MenuResults, 4);
            _ = result.AddChild(image4);
            stamp = Image.Image_createWithResIDQuad(Resources.Img.MenuResults, CTRResourceMgr.GetResultStampQuad());
            Timeline timeline2 = new Timeline().InitWithMaxKeyFramesOnTrack(7);
            timeline2.AddKeyFrame(KeyFrame.MakeScale(3, 3, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
            timeline2.AddKeyFrame(KeyFrame.MakeScale(1, 1, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.5f));
            timeline2.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
            timeline2.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.5f));
            _ = stamp.AddTimeline(timeline2);
            stamp.anchor = 18;
            stamp.SetEnabled(false);
            Image.SetElementPositionWithQuadOffset(stamp, Resources.Img.MenuResults, 12);
            _ = result.AddChild(stamp);
            Button button = MenuController.CreateShortButtonWithTextIDDelegate(Application.GetString("REPLAY"), 8, b);
            button.anchor = 18;
            // Custom levels hide the NEXT/MENU buttons, so replay takes the centered menu slot instead.
            Image.SetElementPositionWithQuadOffset(button, Resources.Img.MenuResults, CustomLevelSession.IsActive ? 9 : 11);
            _ = result.AddChild(button);
            if (!CustomLevelSession.IsActive)
            {
                Button button2 = MenuController.CreateShortButtonWithTextIDDelegate(Application.GetString("NEXT"), 9, b);
                button2.anchor = 18;
                Image.SetElementPositionWithQuadOffset(button2, Resources.Img.MenuResults, 10);
                _ = result.AddChild(button2);
                Button button3 = MenuController.CreateShortButtonWithTextIDDelegate(Application.GetString("MENU"), 5, b);
                button3.anchor = 18;
                Image.SetElementPositionWithQuadOffset(button3, Resources.Img.MenuResults, 9);
                _ = result.AddChild(button3);
            }
            Text text2 = new Text().InitWithFont(Application.GetFont(Resources.Fnt.SmallFont));
            text2.SetName("dataTitle");
            text2.anchor = 18;
            Image.SetElementPositionWithQuadOffset(text2, Resources.Img.MenuResults, 5);
            _ = result.AddChild(text2);
            Text text3 = new Text().InitWithFont(Application.GetFont(Resources.Fnt.SmallFont));
            text3.SetName("dataValue");
            text3.anchor = 18;
            Image.SetElementPositionWithQuadOffset(text3, Resources.Img.MenuResults, 6);
            _ = result.AddChild(text3);
            Text text4 = new Text().InitWithFont(Application.GetFont(Resources.Fnt.FontNumbersBig));
            text4.SetName("scoreValue");
            text4.anchor = 18;
            Image.SetElementPositionWithQuadOffset(text4, Resources.Img.MenuResults, 8);
            _ = result.AddChild(text4);
            confettiAnims = new BaseElement();
            _ = result.AddChild(confettiAnims);
            openCloseAnims = null;
            boxAnim = -1;
            delegateboxClosed = null;
            return this;
        }

        /// <summary>
        /// Creates one randomized confetti particle near the top of the screen.
        /// </summary>
        /// <returns>The configured confetti particle element.</returns>
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

        /// <summary>
        /// Starts the first-level opening transition and hides any visible result panel.
        /// </summary>
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

        /// <summary>
        /// Starts the normal level opening transition and hides any visible result panel.
        /// </summary>
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

        /// <summary>
        /// Starts the closing transition for a won level and prepares the result panel countdown.
        /// </summary>
        public void LevelWon()
        {
            boxAnim = 2;
            raState = -1;
            RemoveOpenCloseAnims();
            ShowCloseAnim();
            ((Text)result.GetChildWithName("scoreValue")).SetEnabled(false);
            Text text = (Text)result.GetChildWithName("dataTitle");
            text.SetEnabled(false);
            Image.SetElementPositionWithQuadOffset(text, Resources.Img.MenuResults, 5);
            ((Text)result.GetChildWithName("dataValue")).SetEnabled(false);
            result.PlayTimeline(0);
            result.SetEnabled(true);
            stamp.SetEnabled(false);
        }

        /// <summary>
        /// Starts the closing transition for a lost level.
        /// </summary>
        public void LevelLost()
        {
            boxAnim = 3;
            RemoveOpenCloseAnims();
            ShowCloseAnim();
        }

        /// <summary>
        /// Starts the closing transition for quitting a level.
        /// </summary>
        public void LevelQuit()
        {
            boxAnim = 4;
            result.SetEnabled(false);
            RemoveOpenCloseAnims();
            ShowCloseAnim();
        }

        /// <summary>
        /// Shows the box opening animation.
        /// </summary>
        public void ShowOpenAnim()
        {
            ShowOpenCloseAnim(true);
        }

        /// <summary>
        /// Shows the box closing animation.
        /// </summary>
        public void ShowCloseAnim()
        {
            ShowOpenCloseAnim(false);
        }

        /// <summary>
        /// Adds a burst of confetti particles to the result panel.
        /// </summary>
        public void ShowConfetti()
        {
            for (int i = 0; i < 70; i++)
            {
                _ = confettiAnims.AddChild(CreateConfettiParticleNear());
            }
        }

        /// <summary>
        /// Builds and plays the animated box cover used for opening and closing transitions.
        /// </summary>
        /// <param name="open"><see langword="true"/> to play the opening animation; <see langword="false"/> to play the closing animation.</param>
        public void ShowOpenCloseAnim(bool open)
        {
            CreateOpenCloseAnims();
            CTRRootController cTRRootController = (CTRRootController)Application.SharedRootController();
            string boxCover = PackConfig.GetBoxCoverOrDefault(cTRRootController.GetPack());
            Image image = Image.Image_createWithResIDQuad(Resources.Img.MenuResults, 16);
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
            Image image4 = Image.Image_createWithResIDQuad(Resources.Img.MenuLevelUi, 6);
            Image image5 = Image.Image_createWithResIDQuad(Resources.Img.MenuLevelUi, 7);
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

        /// <inheritdoc />
        public void TimelinereachedKeyFramewithIndex(Timeline t, KeyFrame k, int i)
        {
        }

        /// <inheritdoc />
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

        /// <summary>
        /// Invokes the box-closed callback and optionally starts the confetti burst.
        /// </summary>
        public void PostBoxClosed()
        {
            delegateboxClosed?.Invoke();
            if (shouldShowConfetti)
            {
                ShowConfetti();
            }
        }

        /// <summary>
        /// Removes active box transition animation elements and restores result label opacity.
        /// </summary>
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

        /// <summary>
        /// Creates the container used to hold box transition animation elements.
        /// </summary>
        public void CreateOpenCloseAnims()
        {
            openCloseAnims = new BaseElement();
            _ = AddChildwithID(openCloseAnims, 0);
        }

        /// <summary>
        /// Delayed-dispatcher callback that removes box open/close animation elements.
        /// </summary>
        /// <param name="obj">Box transition instance to update.</param>
        private static void Selector_removeOpenCloseAnims(FrameworkTypes obj)
        {
            ((BoxOpenClose)obj).RemoveOpenCloseAnims();
        }

        /// <summary>
        /// Delayed-dispatcher callback that finalizes the closed-box state.
        /// </summary>
        /// <param name="obj">Box transition instance to update.</param>
        private static void Selector_postBoxClosed(FrameworkTypes obj)
        {
            ((BoxOpenClose)obj).PostBoxClosed();
        }

        /// <summary>Box animation state for the first level start transition.</summary>
        public const int BOX_ANIM_LEVEL_FIRST_START = 0;

        /// <summary>Box animation state for a normal level start transition.</summary>
        public const int BOX_ANIM_LEVEL_START = 1;

        /// <summary>Box animation state for a won level transition.</summary>
        public const int BOX_ANIM_LEVEL_WON = 2;

        /// <summary>Box animation state for a lost level transition.</summary>
        public const int BOX_ANIM_LEVEL_LOST = 3;

        /// <summary>Box animation state for a quit-level transition.</summary>
        public const int BOX_ANIM_LEVEL_QUIT = 4;

        /// <summary>Result panel state before the countdown sequence begins.</summary>
        public const int RESULT_STATE_WAIT = 0;

        /// <summary>Result panel state that fades in the star bonus row.</summary>
        public const int RESULT_STATE_SHOW_STAR_BONUS = 1;

        /// <summary>Result panel state that counts star bonus points into the score.</summary>
        public const int RESULT_STATE_COUNTDOWN_STAR_BONUS = 2;

        /// <summary>Result panel state that fades out the star bonus row.</summary>
        public const int RESULT_STATE_HIDE_STAR_BONUS = 3;

        /// <summary>Result panel state that fades in the time bonus row.</summary>
        public const int RESULT_STATE_SHOW_TIME_BONUS = 4;

        /// <summary>Result panel state that counts time bonus points into the score.</summary>
        public const int RESULT_STATE_COUNTDOWN_TIME_BONUS = 5;

        /// <summary>Result panel state that fades out the time bonus row.</summary>
        public const int RESULT_STATE_HIDE_TIME_BONUS = 6;

        /// <summary>Result panel state that shows the final score label.</summary>
        public const int RESULT_STATE_SHOW_FINAL_SCORE = 7;

        /// <summary>Result panel timeline ID for showing the panel.</summary>
        public const int RESULTS_SHOW_ANIM = 0;

        /// <summary>Result panel timeline ID for hiding the panel.</summary>
        public const int RESULTS_HIDE_ANIM = 1;

        /// <summary>Container for active box cover transition elements.</summary>
        public BaseElement openCloseAnims;

        /// <summary>Container for active confetti particle elements.</summary>
        public BaseElement confettiAnims;

        /// <summary>Result panel root element.</summary>
        public BaseElement result;

        /// <summary>Current box animation state.</summary>
        public int boxAnim;

        /// <summary>Whether confetti should be shown after the box closes.</summary>
        public bool shouldShowConfetti;

        /// <summary>Whether the improved-result stamp should be shown.</summary>
        public bool shouldShowImprovedResult;

        /// <summary>Improved-result stamp image.</summary>
        public Image stamp;

        /// <summary>Current result panel countdown state.</summary>
        public int raState;

        /// <summary>Time bonus score used by the result countdown.</summary>
        public int timeBonus;

        /// <summary>Star bonus score used by the result countdown.</summary>
        public int starBonus;

        /// <summary>Base score before result countdown bonuses are applied.</summary>
        public int score;

        /// <summary>Level completion time used by the result countdown.</summary>
        public float time;

        /// <summary>Displayed countdown time value.</summary>
        public float ctime;

        /// <summary>Displayed countdown star bonus value.</summary>
        public int cstarBonus;

        /// <summary>Displayed countdown score value.</summary>
        public int cscore;

        /// <summary>Delay timer used by the result panel countdown state machine.</summary>
        public float raDelay;

        /// <summary>Callback invoked after the box closing transition completes.</summary>
        public boxClosed delegateboxClosed;

        /// <summary>
        /// Callback invoked after the box closing transition completes.
        /// </summary>
        public delegate void boxClosed();

        /// <summary>
        /// Confetti particle animation that advances its sprite animation timeline during updates.
        /// </summary>
        private sealed class Confetti : Animation
        {
            /// <summary>
            /// Creates a confetti particle from a texture resource name.
            /// </summary>
            /// <param name="resourceName">Texture resource name to load.</param>
            /// <returns>The initialized confetti particle.</returns>
            public static Confetti Confetti_createWithResID(string resourceName)
            {
                return Confetti_create(Application.GetTexture(resourceName));
            }

            /// <summary>
            /// Creates a confetti particle from a texture.
            /// </summary>
            /// <param name="t">Texture used by the confetti particle.</param>
            /// <returns>The initialized confetti particle.</returns>
            public static Confetti Confetti_create(CTRTexture2D t)
            {
                return (Confetti)new Confetti().InitWithTexture(t);
            }

            /// <inheritdoc />
            public override void Update(float delta)
            {
                base.Update(delta);
                Timeline.UpdateTimeline(ani, delta);
            }

            /// <summary>Sprite animation timeline used by this confetti particle.</summary>
            public Timeline ani;
        }
    }
}
