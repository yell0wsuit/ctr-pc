using System.Collections.Generic;

using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Sfe;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    internal sealed class Lantern : CTRGameObject
    {
        public Lantern InitWithPosition(Vector position)
        {
            if (InitWithTexture(Application.GetTexture(Resources.Img.ObjLantern)) == null)
            {
                return null;
            }

            SharedCandyPoint = null;
            GetAllLanterns().Add(this);

            x = position.x;
            y = position.y;
            lanternState = LanternStateInactive;

            delayedDispatcher ??= new DelayedDispatcher();

            fire = Image_createWithResIDQuad(Resources.Img.ObjLantern, FireQuad);
            fire.anchor = fire.parentAnchor = 18;
            fire.color = RGBAColor.transparentRGBA;
            fire.DoRestoreCutTransparency();
            _ = AddChild(fire);

            Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(3);
            timeline.AddKeyFrame(KeyFrame.MakeScale(1.4, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline.AddKeyFrame(KeyFrame.MakeScale(1.05, 1.3, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.5));
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.MakeRGBA(0.7, 0.7, 0.7, 0.7), KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
            timeline.SetTimelineLoopType(Timeline.LoopType.TIMELINE_PING_PONG);
            fire.AddTimelinewithID(timeline, (int)LanternActivation.FireBounce);

            idleForm = Image_createWithResIDQuad(Resources.Img.ObjLantern, LanternStartQuad);
            idleForm.anchor = idleForm.parentAnchor = 18;
            idleForm.DoRestoreCutTransparency();
            _ = AddChild(idleForm);

            timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.3));
            idleForm.AddTimelinewithID(timeline, (int)LanternActivation.Activation);

            timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.3));
            idleForm.AddTimelinewithID(timeline, (int)LanternActivation.Deactivation);

            activeForm = Image_createWithResIDQuad(Resources.Img.ObjLantern, LanternEndQuad);
            activeForm.anchor = activeForm.parentAnchor = 18;
            activeForm.color = RGBAColor.transparentRGBA;
            activeForm.y = 1f;
            activeForm.DoRestoreCutTransparency();
            _ = AddChild(activeForm);

            timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.3));
            activeForm.AddTimelinewithID(timeline, (int)LanternActivation.Activation);

            timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.3));
            activeForm.AddTimelinewithID(timeline, (int)LanternActivation.Deactivation);

            int candyVariant = Preferences.GetIntForKey(CTRPreferences.PREFS_SELECTED_CANDY);

            // First 3 candy variants are in obj_lantern texture (quads 3, 4, 5)
            // Variants 3+ use the _lantern quad (quad 10) from their respective candy textures
            if (candyVariant < 3)
            {
                innerCandy = Image_createWithResIDQuad(Resources.Img.ObjLantern, InnerCandyStartQuad + candyVariant);
            }
            else
            {
                string candyResource = CandySkinHelper.GetCandyResource(candyVariant);
                innerCandy = Image_createWithResIDQuad(candyResource, LanternQuadInCandyTexture);
            }

            innerCandy.anchor = innerCandy.parentAnchor = 18;
            innerCandy.color = RGBAColor.transparentRGBA;
            innerCandy.y = -4f;
            innerCandy.DoRestoreCutTransparency();
            _ = AddChild(innerCandy);

            timeline = new Timeline().InitWithMaxKeyFramesOnTrack(4);
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.2));
            timeline.AddKeyFrame(KeyFrame.MakeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline.AddKeyFrame(KeyFrame.MakeScale(1.0, 0.8, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.07));
            timeline.AddKeyFrame(KeyFrame.MakeScale(0.85, 1.05, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.05));
            timeline.AddKeyFrame(KeyFrame.MakeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.05));
            timeline.AddKeyFrame(KeyFrame.MakePos(0.0, -4.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline.AddKeyFrame(KeyFrame.MakePos(0.0, 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.1));
            timeline.AddKeyFrame(KeyFrame.MakePos(0.0, -1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.05));
            innerCandy.AddTimelinewithID(timeline, InnerCandyAppearTimelineId);

            timeline = new Timeline().InitWithMaxKeyFramesOnTrack(3);
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.MakeRGBA(0.6, 0.6, 0.6, 0.6), KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.06));
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.04));
            timeline.AddKeyFrame(KeyFrame.MakeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline.AddKeyFrame(KeyFrame.MakeScale(1.15, 0.8, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.06));
            timeline.AddKeyFrame(KeyFrame.MakeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.04));
            timeline.AddKeyFrame(KeyFrame.MakePos(0.0, 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline.AddKeyFrame(KeyFrame.MakePos(0.0, -4.0, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.06));
            timeline.AddKeyFrame(KeyFrame.MakePos(0.0, 4.0, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.04));
            innerCandy.AddTimelinewithID(timeline, InnerCandyHideTimelineId);

            timeline = new Timeline().InitWithMaxKeyFramesOnTrack(5);
            timeline.AddKeyFrame(KeyFrame.MakeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0.0));
            timeline.AddKeyFrame(KeyFrame.MakeScale(0.93, 0.93, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.35));
            timeline.AddKeyFrame(KeyFrame.MakeScale(0.87, 0.87, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.35));
            timeline.AddKeyFrame(KeyFrame.MakeScale(0.93, 0.93, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.35));
            timeline.AddKeyFrame(KeyFrame.MakeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.35));
            timeline.SetTimelineLoopType(Timeline.LoopType.TIMELINE_REPLAY);
            innerCandy.AddTimelinewithID(timeline, InnerCandyIdleTimelineId);

            return this;
        }

        public void CaptureCandyFromDispatcher(FrameworkTypes obj)
        {
            CaptureCandy((ConstraintedPoint)obj);
        }

        public void CaptureCandy(ConstraintedPoint candyPoint)
        {
            CTRSoundMgr.PlaySound(Resources.Snd.LanternTeleportIn);

            SharedCandyPoint = candyPoint;
            candyPoint.disableGravity = true;
            candyPoint.pos = candyPoint.prevPos = Vect(x, y);

            foreach (Lantern lantern in GetAllLanterns())
            {
                lantern.lanternState = LanternStateActive;
                lantern.idleForm.PlayTimeline((int)LanternActivation.Activation);
                lantern.activeForm.PlayTimeline((int)LanternActivation.Activation);
                lantern.innerCandy.PlayTimeline(InnerCandyAppearTimelineId);
                lantern.fire.scaleX = 1.4f;
                lantern.fire.scaleY = 1f;
                lantern.fire.color = RGBAColor.MakeRGBA(0.7, 0.7, 0.7, 0.7);
                lantern.delayedDispatcher.CancelAllDispatches();
                lantern.delayedDispatcher.CallObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(lantern.PlayFireBounceTimeline), null, 0.4 * RND_0_1);
                lantern.delayedDispatcher.CallObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(lantern.PlayInnerCandyIdleTimeline), null, 0.2 + (0.2 * RND_0_1));
            }
        }

        public static List<Lantern> GetAllLanterns()
        {
            allLanterns ??= [];
            return allLanterns;
        }

        public static void RemoveAllLanterns()
        {
            SharedCandyPoint = null;
            GetAllLanterns().Clear();
        }

        public override void Update(float delta)
        {
            prevPos = Vect(x, y);
            base.Update(delta);
            delayedDispatcher.Update(delta);
            if (SharedCandyPoint != null)
            {
                SharedCandyPoint.pos = SharedCandyPoint.prevPos = Vect(x, y);
                if (lanternState != LanternStateActive)
                {
                    lanternState = LanternStateActive;
                }
            }
        }

        public bool OnTouchDown(float tx, float ty)
        {
            float distance = VectDistance(Vect(tx, ty), Vect(x, y));
            if (lanternState == LanternStateActive && distance < LanternTouchRadius && SharedCandyPoint != null)
            {
                InitiateReleasingCandy();
                return true;
            }

            return false;
        }

        private void ReleaseCandy(FrameworkTypes obj)
        {
            if (SharedCandyPoint == null)
            {
                return;
            }

            SharedCandyPoint.disableGravity = false;
            SharedCandyPoint.pos = Vect(x, y);
            SharedCandyPoint.prevPos = prevPos;
            SharedCandyPoint = null;
        }

        private static void BecomeCandyAware(FrameworkTypes obj)
        {
            ((Lantern)obj).lanternState = LanternStateInactive;
        }

        private void InitiateReleasingCandy()
        {
            CTRSoundMgr.PlaySound(Resources.Snd.LanternTeleportOut);
            foreach (Lantern lantern in GetAllLanterns())
            {
                lantern.idleForm.PlayTimeline((int)LanternActivation.Deactivation);
                lantern.activeForm.PlayTimeline((int)LanternActivation.Deactivation);
                lantern.innerCandy.PlayTimeline(InnerCandyHideTimelineId);
                Timeline fireTimeline = lantern.fire.GetTimeline((int)LanternActivation.FireBounce);
                if (fireTimeline != null && fireTimeline.state == Timeline.TimelineState.TIMELINE_PLAYING)
                {
                    lantern.fire.StopCurrentTimeline();
                }
                lantern.fire.color = RGBAColor.transparentRGBA;
                lantern.delayedDispatcher.CancelAllDispatches();
                lantern.delayedDispatcher.CallObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(lantern.BecomingAwareDispatcher), lantern, LanternInactiveDelay + 0.1f);
            }
            delayedDispatcher.CallObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(ReleaseCandy), null, 0.01f);
        }

        private void PlayFireBounceTimeline(FrameworkTypes _)
        {
            fire?.PlayTimeline((int)LanternActivation.FireBounce);
        }

        private void PlayInnerCandyIdleTimeline(FrameworkTypes _)
        {
            innerCandy?.PlayTimeline(InnerCandyIdleTimelineId);
        }

        private void BecomingAwareDispatcher(FrameworkTypes obj)
        {
            BecomeCandyAware(obj);
        }

        public int lanternState;

        public Vector prevPos;

        private Image idleForm;

        private Image activeForm;

        private Image innerCandy;

        private Image fire;

        private DelayedDispatcher delayedDispatcher;

        private static ConstraintedPoint SharedCandyPoint { get; set; }

        private static List<Lantern> allLanterns;

        private const int FireQuad = 0;
        private const int LanternEndQuad = 1;
        private const int LanternStartQuad = 2;
        private const int InnerCandyStartQuad = 3;
        private const int LanternQuadInCandyTexture = 10; // frame_10_lantern.png in candy textures
        private const int InnerCandyAppearTimelineId = 0;
        private const int InnerCandyHideTimelineId = 1;
        private const int InnerCandyIdleTimelineId = 2;
        public const float LanternCandyRevealTime = 0.1f;
        public const int LanternStateInactive = 0;
        public const int LanternStateActive = 1;
        private const float LanternTouchRadius = 85f;
        private const float LanternInactiveDelay = 0.4f;

        private enum LanternActivation
        {
            Activation,
            Deactivation,
            FireBounce
        }
    }
}
