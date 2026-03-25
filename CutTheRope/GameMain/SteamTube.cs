using System.Collections.Generic;

using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Steam tube object that emits animated steam puffs based on its valve state.
    /// </summary>
    internal sealed class SteamTube : BaseElement, ITimelineDelegate, ITransporterItem, ITransporterBindAware, ITransporterScaleAware
    {
        public SteamTube()
        {
            dd = new DelayedDispatcher();
            steamState = 0;
            phase = 0f;
        }

        /// <summary>
        /// Initializes steam tube with position, rotation angle, and height scale.
        /// </summary>
        /// <param name="heightScale">
        /// Scale factor for steam tube dimensions. Typically 3 for PC (vs 1 on WP).
        /// Scales: tube width (10f), valve position (27f), touch offset (28f), collision radius (17.5f),
        /// base heights (32.9f/94f/141f), and vertical offset (1f).
        /// Does NOT scale: sine wave modulation amplitude (always 1f).
        /// </param>
        public SteamTube InitWithPositionAngle(Vector position, float angle, float heightScale = 1f)
        {
            x = position.X;
            y = position.Y;
            this.heightScale = heightScale;
            rotation = angle;
            anchor = 18;
            steamBack = new BaseElement();
            steamBack.anchor = steamBack.parentAnchor = 18;
            steamFront = new BaseElement();
            steamFront.anchor = steamFront.parentAnchor = 18;
            tube = Image.Image_createWithResIDQuad(Resources.Img.ObjPipe, 0);
            tube.x = 0f;
            tube.y = 0f;
            tube.anchor = 10;
            tube.parentAnchor = 18;
            _ = AddChild(tube);
            width = tube.width;
            height = tube.height;
            valve = Image.Image_createWithResIDQuad(Resources.Img.ObjPipe, 1);
            valve.x = 0f;
            valve.y = 27f * heightScale;
            valve.anchor = 18;
            valve.parentAnchor = 18;
            _ = AddChild(valve);
            _ = AddChild(steamBack);
            _ = AddChild(steamFront);
            AdjustSteam();
            Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            timeline.AddKeyFrame(KeyFrame.MakeRotation(0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
            timeline.AddKeyFrame(KeyFrame.MakeRotation(180, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.55f));
            valve.AddTimelinewithID(timeline, 0);
            timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            timeline.AddKeyFrame(KeyFrame.MakeRotation(0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
            timeline.AddKeyFrame(KeyFrame.MakeRotation(-180, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.55f));
            valve.AddTimelinewithID(timeline, 1);
            return this;
        }

        public void DrawBack()
        {
            PreDraw();
            tube.Draw();
            valve.Draw();
            steamBack.Draw();
            RestoreTransformations(this);
        }

        public void DrawFront()
        {
            PreDraw();
            steamFront.Draw();
            RestoreTransformations(this);
        }

        /// <summary>
        /// Gets current steam height with sine wave modulation for pulsing effect.
        /// </summary>
        public float GetCurrentHeightModulated()
        {
            float currentHeight = GetCurrentHeight();
            return currentHeight + (heightScale * Sinf(6f * phase));
        }

        public float GetHeightScale()
        {
            return heightScale;
        }

        public override void Update(float delta)
        {
            base.Update(delta);
            dd.Update(delta);
            phase += delta;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                tube = null;
                valve = null;
                steamBack = null;
                steamFront = null;
                dd?.Dispose();
                dd = null;
            }
            base.Dispose(disposing);
        }

        public override bool OnTouchDownXY(float tx, float ty)
        {
            Vector vector = VectAdd(Vect(x, y), VectRotate(Vect(0f, 28f * heightScale), DEGREES_TO_RADIANS(rotation)));
            float touchZone = VectLength(VectSub(Vect(tx, ty), vector));
            if (touchZone < 40f)
            {
                int valveTimelineIndex = 0;
                switch (steamState)
                {
                    case 0:
                        steamState++;
                        valveTimelineIndex = 0;
                        CTRSoundMgr.PlaySound(Resources.Snd.SteamStart2);
                        break;
                    case 1:
                        steamState++;
                        valveTimelineIndex = 0;
                        CTRSoundMgr.PlaySound(Resources.Snd.SteamStart);
                        break;
                    case 2:
                        steamState = 0;
                        valveTimelineIndex = 1;
                        CTRSoundMgr.PlaySound(Resources.Snd.SteamEnd);
                        break;
                    default:
                        break;
                }
                AdjustSteam();
                if (valve.GetTimeline(0).state != Timeline.TimelineState.TIMELINE_PLAYING && valve.GetTimeline(1).state != Timeline.TimelineState.TIMELINE_PLAYING)
                {
                    valve.PlayTimeline(valveTimelineIndex);
                }
                return true;
            }
            return false;
        }

        public void TimelinereachedKeyFramewithIndex(Timeline t, KeyFrame k, int i)
        {
        }

        public void TimelineFinished(Timeline t)
        {
            BaseElement element = t.element;
            element.parent.RemoveChild(element);
        }

        public float PositionOnTransporter { get; set; }

        public Vector BindPoint
        {
            get
            {
                float angle = DEGREES_TO_RADIANS(rotation);
                Vector offset = VectRotate(Vect(0f, height * 0.45f * scaleY), angle);
                return VectAdd(Vect(x, y), offset);
            }
        }

        /// <summary>
        /// Sets the steam tube's position so its transporter bind point matches
        /// the given world point (inverse of <see cref="BindPoint"/>).
        /// </summary>
        public void SetBindPoint(Vector point)
        {
            float angle = DEGREES_TO_RADIANS(rotation);
            Vector offset = VectRotate(Vect(0f, height * 0.45f * scaleY), angle);
            Vector adjusted = VectSub(point, offset);
            x = adjusted.X;
            y = adjusted.Y;
        }

        public float CollisionRadius => 52.5f;

        public float MinScale => 0.7f;

        public float MaxScale => 1.0f;

        public float TransporterScale { get; set; } = 1.0f;

        public bool IsDrawnByTransporter { get; set; }

        public void WillBind()
        {
            IsDrawnByTransporter = true;
        }

        public void SetTransporterScale(float scale)
        {
            scaleX = scale;
            scaleY = scale;

            if (tube != null)
            {
                tube.scaleX = scale;
                tube.scaleY = scale;
            }

            if (valve != null)
            {
                valve.scaleX = scale;
                valve.scaleY = scale;
            }
        }

        /// <summary>
        /// Gets base steam height for current valve state (0=low, 1=medium, 2=high).
        /// PC vs Windows Phone: Returns base heights (32.9f/94f/141f) scaled by heightScale.
        /// Windows Phone equivalent returns unscaled values.
        /// </summary>
        private float GetCurrentHeight()
        {
            float baseHeight = steamState switch
            {
                0 => 32.9f,
                1 => 94f,
                2 => 141f,
                _ => 0f,
            };
            return baseHeight * heightScale;
        }

        private void AdjustSteam()
        {
            phase = 0f;
            if (steamBack != null)
            {
                Dictionary<int, BaseElement> childs = steamBack.GetChilds();
                foreach (KeyValuePair<int, BaseElement> keyValuePair in childs)
                {
                    BaseElement value = keyValuePair.Value;
                    value?.GetTimeline(0).SetTimelineLoopType(Timeline.LoopType.TIMELINE_NO_LOOP);
                }
            }
            if (steamFront != null)
            {
                Dictionary<int, BaseElement> childs2 = steamFront.GetChilds();
                foreach (KeyValuePair<int, BaseElement> keyValuePair2 in childs2)
                {
                    BaseElement value2 = keyValuePair2.Value;
                    value2?.GetTimeline(0).SetTimelineLoopType(Timeline.LoopType.TIMELINE_NO_LOOP);
                }
            }
            if (steamState != 3)
            {
                steamBack.anchor = steamBack.parentAnchor = 18;
                steamFront.anchor = steamFront.parentAnchor = 18;
                int puffCount = 7;
                if (steamState == 1)
                {
                    puffCount = 14;
                }
                if (steamState == 2)
                {
                    puffCount = 20;
                }
                for (int i = 0; i < puffCount; i++)
                {
                    int animationStartFrame = 0;
                    int animationEndFrame = 0;
                    switch (i % 3)
                    {
                        case 0:
                            animationStartFrame = 24;
                            animationEndFrame = 34;
                            break;
                        case 1:
                            animationStartFrame = 13;
                            animationEndFrame = 23;
                            break;
                        case 2:
                            animationStartFrame = 2;
                            animationEndFrame = 12;
                            break;
                        default:
                            break;
                    }
                    float puffDuration = 0.6f;
                    float frameDelay = puffDuration / (animationEndFrame - animationStartFrame + 1);
                    float puffHeight = -GetCurrentHeight();
                    puffHeight *= 1f + (0.1f * RND_MINUS1_1);
                    if (steamState == 1 && (i % 3 == 1 || i % 3 == 2))
                    {
                        puffHeight *= 0.95f;
                    }
                    if (steamState == 2 && (i % 3 == 1 || i % 3 == 2))
                    {
                        puffHeight *= 0.94f;
                    }
                    float horizontalOffset = 1f;
                    if (i % 3 == 0)
                    {
                        horizontalOffset = 0f;
                    }
                    else if (i % 3 == 1)
                    {
                        horizontalOffset *= steamState;
                    }
                    else if (i % 3 == 2)
                    {
                        horizontalOffset *= -steamState;
                    }
                    Animation animation = Animation.Animation_createWithResID(Resources.Img.ObjPipe);
                    animation.DoRestoreCutTransparency();
                    _ = animation.AddAnimationDelayLoopFirstLast(frameDelay, Timeline.LoopType.TIMELINE_REPLAY, animationStartFrame, animationEndFrame);
                    animation.anchor = animation.parentAnchor = 18;
                    Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
                    timeline.AddKeyFrame(KeyFrame.MakePos(0, 0, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0));
                    timeline.AddKeyFrame(KeyFrame.MakePos((int)horizontalOffset, (int)puffHeight, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, puffDuration));
                    timeline.AddKeyFrame(KeyFrame.MakeScale(1, 1, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0));
                    timeline.AddKeyFrame(KeyFrame.MakeScale(1.5f, 1.5f, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, puffDuration));
                    timeline.SetTimelineLoopType(Timeline.LoopType.TIMELINE_REPLAY);
                    timeline.delegateTimelineDelegate = this;
                    BaseElement baseElement = new();
                    baseElement.AddTimelinewithID(timeline, 0);
                    dd.CallObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(StartPuffFloatingAndAnimation), baseElement, puffDuration * i / puffCount);
                    _ = baseElement.AddChild(animation);
                    baseElement.anchor = baseElement.parentAnchor = 18;
                    baseElement.SetEnabled(false);
                    _ = i % 3 == 0 ? steamBack.AddChild(baseElement) : steamFront.AddChild(baseElement);
                }
            }
        }

        private void StartPuffFloatingAndAnimation(FrameworkTypes param)
        {
            BaseElement baseElement = (BaseElement)param;
            baseElement.SetEnabled(true);
            baseElement.PlayTimeline(0);
            BaseElement child = baseElement.GetChild(baseElement.ChildsCount() - 1);
            child.PlayTimeline(0);
        }

        private float heightScale = 1f;
        public int steamState;

        private DelayedDispatcher dd;

        private Image tube;

        private Image valve;

        private BaseElement steamBack;

        private BaseElement steamFront;

        private float phase;

    }
}
