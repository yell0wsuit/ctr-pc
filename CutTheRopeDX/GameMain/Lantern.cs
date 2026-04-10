using System.Collections.Generic;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Physics;
using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Lantern object that can capture the candy, hold it in a shared lantern state, and release it on touch.
    /// </summary>
    internal sealed class Lantern : CTRGameObject
    {
        /// <summary>
        /// Initializes the lantern at a level position and creates its idle, active, fire, and candy visuals.
        /// </summary>
        /// <param name="position">World-space lantern position.</param>
        /// <returns>The initialized lantern, or <see langword="null"/> if its texture could not be loaded.</returns>
        public Lantern InitWithPosition(Vector position)
        {
            if (InitWithTexture(Application.GetTexture(Resources.Img.ObjLantern)) == null)
            {
                return null;
            }

            SharedCandyPoint = null;
            GetAllLanterns().Add(this);

            x = position.X;
            y = position.Y;
            lanternState = LanternStateInactive;

            delayedDispatcher ??= new DelayedDispatcher();

            fire = Image_createWithResIDQuad(Resources.Img.ObjLantern, FireQuad);
            fire.anchor = fire.parentAnchor = 18;
            fire.color = RGBAColor.transparentRGBA;
            fire.DoRestoreCutTransparency();
            _ = AddChild(fire);

            Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(3);
            timeline.AddKeyFrame(KeyFrame.MakeScale(1.4f, 1, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
            timeline.AddKeyFrame(KeyFrame.MakeScale(1.05f, 1.3f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.5f));
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.MakeRGBA(0.7f, 0.7f, 0.7f, 0.7f), KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
            timeline.SetTimelineLoopType(Timeline.LoopType.TIMELINE_PING_PONG);
            fire.AddTimelinewithID(timeline, (int)LanternActivation.FireBounce);

            idleForm = Image_createWithResIDQuad(Resources.Img.ObjLantern, LanternStartQuad);
            idleForm.anchor = idleForm.parentAnchor = 18;
            idleForm.DoRestoreCutTransparency();
            _ = AddChild(idleForm);

            timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.3f));
            idleForm.AddTimelinewithID(timeline, (int)LanternActivation.Activation);

            timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.3f));
            idleForm.AddTimelinewithID(timeline, (int)LanternActivation.Deactivation);

            activeForm = Image_createWithResIDQuad(Resources.Img.ObjLantern, LanternEndQuad);
            activeForm.anchor = activeForm.parentAnchor = 18;
            activeForm.color = RGBAColor.transparentRGBA;
            activeForm.y = 1f;
            activeForm.DoRestoreCutTransparency();
            _ = AddChild(activeForm);

            timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.3f));
            activeForm.AddTimelinewithID(timeline, (int)LanternActivation.Activation);

            timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.3f));
            activeForm.AddTimelinewithID(timeline, (int)LanternActivation.Deactivation);

            int candyVariant = Preferences.GetIntForKey("PREFS_SELECTED_CANDY");

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
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.2f));
            timeline.AddKeyFrame(KeyFrame.MakeScale(1, 1, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
            timeline.AddKeyFrame(KeyFrame.MakeScale(1, 0.8f, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.07f));
            timeline.AddKeyFrame(KeyFrame.MakeScale(0.85f, 1.05f, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.05f));
            timeline.AddKeyFrame(KeyFrame.MakeScale(1, 1, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.05f));
            timeline.AddKeyFrame(KeyFrame.MakePos(0, -4, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
            timeline.AddKeyFrame(KeyFrame.MakePos(0, 0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.1f));
            timeline.AddKeyFrame(KeyFrame.MakePos(0, -1, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.05f));
            innerCandy.AddTimelinewithID(timeline, InnerCandyAppearTimelineId);

            timeline = new Timeline().InitWithMaxKeyFramesOnTrack(3);
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.MakeRGBA(0.6f, 0.6f, 0.6f, 0.6f), KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.06f));
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.04f));
            timeline.AddKeyFrame(KeyFrame.MakeScale(1, 1, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
            timeline.AddKeyFrame(KeyFrame.MakeScale(1.15f, 0.8f, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.06f));
            timeline.AddKeyFrame(KeyFrame.MakeScale(1, 1, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.04f));
            timeline.AddKeyFrame(KeyFrame.MakePos(0, 0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
            timeline.AddKeyFrame(KeyFrame.MakePos(0, -4, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.06f));
            timeline.AddKeyFrame(KeyFrame.MakePos(0, 4, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.04f));
            innerCandy.AddTimelinewithID(timeline, InnerCandyHideTimelineId);

            timeline = new Timeline().InitWithMaxKeyFramesOnTrack(5);
            timeline.AddKeyFrame(KeyFrame.MakeScale(1, 1, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0));
            timeline.AddKeyFrame(KeyFrame.MakeScale(0.93f, 0.93f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.35f));
            timeline.AddKeyFrame(KeyFrame.MakeScale(0.87f, 0.87f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.35f));
            timeline.AddKeyFrame(KeyFrame.MakeScale(0.93f, 0.93f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.35f));
            timeline.AddKeyFrame(KeyFrame.MakeScale(1, 1, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.35f));
            timeline.SetTimelineLoopType(Timeline.LoopType.TIMELINE_REPLAY);
            innerCandy.AddTimelinewithID(timeline, InnerCandyIdleTimelineId);

            return this;
        }

        /// <summary>
        /// Dispatcher callback that captures a candy point.
        /// </summary>
        /// <param name="obj">Candy point passed through the dispatcher.</param>
        public void CaptureCandyFromDispatcher(FrameworkTypes obj)
        {
            CaptureCandy((ConstraintedPoint)obj);
        }

        /// <summary>
        /// Captures the candy into this lantern and activates all lantern visuals.
        /// </summary>
        /// <param name="candyPoint">Candy physics point to capture.</param>
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
                lantern.fire.color = RGBAColor.MakeRGBA(0.7f, 0.7f, 0.7f, 0.7f);
                lantern.delayedDispatcher.CancelAllDispatches();
                lantern.delayedDispatcher.CallObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(lantern.PlayFireBounceTimeline), null, 0.4f * RND_0_1);
                lantern.delayedDispatcher.CallObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(lantern.PlayInnerCandyIdleTimeline), null, 0.2f + (0.2f * RND_0_1));
            }
        }

        /// <summary>
        /// Gets the shared list of lanterns in the current level.
        /// </summary>
        /// <returns>The shared lantern list.</returns>
        public static List<Lantern> GetAllLanterns()
        {
            allLanterns ??= [];
            return allLanterns;
        }

        /// <summary>
        /// Clears the current level lantern registry and any shared captured candy point.
        /// </summary>
        public static void RemoveAllLanterns()
        {
            SharedCandyPoint = null;
            GetAllLanterns().Clear();
        }

        /// <inheritdoc />
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

        /// <summary>
        /// Handles a touch against the lantern and starts candy release when the lantern is active.
        /// </summary>
        /// <param name="tx">Touch X position in world space.</param>
        /// <param name="ty">Touch Y position in world space.</param>
        /// <returns><see langword="true"/> if the touch was handled by this lantern; otherwise, <see langword="false"/>.</returns>
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

        /// <summary>
        /// Dispatcher callback that releases the shared candy point from the lantern.
        /// </summary>
        /// <param name="obj">Unused dispatcher parameter.</param>
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

        /// <summary>
        /// Dispatcher callback that returns a lantern to the inactive state.
        /// </summary>
        /// <param name="obj">Lantern instance passed through the dispatcher.</param>
        private static void BecomeCandyAware(FrameworkTypes obj)
        {
            ((Lantern)obj).lanternState = LanternStateInactive;
        }

        /// <summary>
        /// Starts the delayed release animation sequence for the shared captured candy.
        /// </summary>
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

        /// <summary>
        /// Dispatcher callback that starts the fire bounce animation.
        /// </summary>
        /// <param name="_">Unused dispatcher parameter.</param>
        private void PlayFireBounceTimeline(FrameworkTypes _)
        {
            fire?.PlayTimeline((int)LanternActivation.FireBounce);
        }

        /// <summary>
        /// Dispatcher callback that starts the inner candy idle animation.
        /// </summary>
        /// <param name="_">Unused dispatcher parameter.</param>
        private void PlayInnerCandyIdleTimeline(FrameworkTypes _)
        {
            innerCandy?.PlayTimeline(InnerCandyIdleTimelineId);
        }

        /// <summary>
        /// Dispatcher callback that forwards a lantern to <see cref="BecomeCandyAware"/>.
        /// </summary>
        /// <param name="obj">Lantern instance passed through the dispatcher.</param>
        private void BecomingAwareDispatcher(FrameworkTypes obj)
        {
            BecomeCandyAware(obj);
        }

        /// <summary>Current lantern state.</summary>
        public int lanternState;

        /// <summary>Lantern position from the previous update.</summary>
        public Vector prevPos;

        /// <summary>Inactive lantern visual.</summary>
        private Image idleForm;

        /// <summary>Active lantern visual.</summary>
        private Image activeForm;

        /// <summary>Candy visual shown inside an active lantern.</summary>
        private Image innerCandy;

        /// <summary>Fire visual shown while a lantern is active.</summary>
        private Image fire;

        /// <summary>Dispatcher for delayed lantern animation and release callbacks.</summary>
        private DelayedDispatcher delayedDispatcher;

        /// <summary>Shared candy point currently captured by any lantern.</summary>
        private static ConstraintedPoint SharedCandyPoint { get; set; }

        /// <summary>Shared lantern registry for the current level.</summary>
        private static List<Lantern> allLanterns;

        /// <summary>Texture quad index for the fire visual.</summary>
        private const int FireQuad = 0;

        /// <summary>Texture quad index for the active lantern visual.</summary>
        private const int LanternEndQuad = 1;

        /// <summary>Texture quad index for the inactive lantern visual.</summary>
        private const int LanternStartQuad = 2;

        /// <summary>First texture quad index for lantern candy variants stored in the lantern texture.</summary>
        private const int InnerCandyStartQuad = 3;

        /// <summary>Texture quad index for the lantern candy frame in candy-specific textures.</summary>
        private const int LanternQuadInCandyTexture = 10; // frame_10_lantern.png in candy textures

        /// <summary>Timeline ID for showing the inner candy visual.</summary>
        private const int InnerCandyAppearTimelineId = 0;

        /// <summary>Timeline ID for hiding the inner candy visual.</summary>
        private const int InnerCandyHideTimelineId = 1;

        /// <summary>Timeline ID for the inner candy idle animation.</summary>
        private const int InnerCandyIdleTimelineId = 2;

        /// <summary>Delay before candy is revealed after a lantern touch.</summary>
        public const float LanternCandyRevealTime = 0.1f;

        /// <summary>Inactive lantern state value.</summary>
        public const int LanternStateInactive = 0;

        /// <summary>Active lantern state value.</summary>
        public const int LanternStateActive = 1;

        /// <summary>Touch radius used to release candy from an active lantern.</summary>
        private const float LanternTouchRadius = 85f;

        /// <summary>Delay before lanterns return to the inactive state after release begins.</summary>
        private const float LanternInactiveDelay = 0.4f;

        /// <summary>
        /// Lantern activation timeline identifiers.
        /// </summary>
        private enum LanternActivation
        {
            /// <summary>Activation timeline.</summary>
            Activation,

            /// <summary>Deactivation timeline.</summary>
            Deactivation,

            /// <summary>Fire bounce timeline.</summary>
            FireBounce
        }
    }
}
