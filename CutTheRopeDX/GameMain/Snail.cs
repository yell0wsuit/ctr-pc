using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Physics;
using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Snail object that can attach to the candy point, drag candy down, and vanish when detached.
    /// </summary>
    internal sealed class Snail : GameObject, ITimelineDelegate
    {
        /// <summary>Texture quad index for the snail shell.</summary>
        private const int SnailShellQuad = 8;

        /// <summary>Texture quad index for the sleepy eyes overlay.</summary>
        private const int SnailSleepyEyesQuad = 2;

        /// <summary>First texture quad index for the wake-up animation.</summary>
        private const int SnailWakeStartQuad = 3;

        /// <summary>Last texture quad index for the wake-up animation.</summary>
        private const int SnailWakeEndQuad = 5;

        /// <summary>Texture quad index for the first active eye overlay.</summary>
        private const int SnailEye1Quad = 6;

        /// <summary>Texture quad index for the second active eye overlay.</summary>
        private const int SnailEye2Quad = 7;

        /// <summary>First texture quad index for the sleep animation.</summary>
        private const int SnailSleepStartQuad = 5;

        /// <summary>Last texture quad index for the sleep animation.</summary>
        private const int SnailSleepEndQuad = 3;

        /// <summary>Action name fired when the detach vanish timeline finishes.</summary>
        private const string SnailActionDetach = "SNAIL_ACTION_DETACH";

        /// <summary>Preference key that tracks how many snails have been grabbed.</summary>
        private const string PrefsGrabSnails = "PREFS_GRAB_SNAILS";

        /// <summary>Achievement identifier awarded after enough snails are grabbed.</summary>
        private const string AchievementSnailTamer = "acSnailTamer";

        /// <summary>
        /// Creates a snail from a texture.
        /// </summary>
        /// <param name="texture">Texture used by the snail.</param>
        /// <returns>The initialized snail.</returns>
        private static Snail Snail_create(CTRTexture2D texture)
        {
            return (Snail)new Snail().InitWithTexture(texture);
        }

        /// <summary>
        /// Creates a snail from a texture resource and applies a draw quad.
        /// </summary>
        /// <param name="resourceName">Texture resource name.</param>
        /// <param name="q">Quad index to draw.</param>
        /// <returns>The initialized snail.</returns>
        public static Snail Snail_createWithResIDQuad(string resourceName, int q)
        {
            Snail snail = Snail_create(Application.GetTexture(resourceName));
            snail.SetDrawQuad(q);
            return snail;
        }

        /// <summary>
        /// Attaches the snail to a candy physics point and plays the wake-up state transition.
        /// </summary>
        /// <param name="p">Candy physics point to follow.</param>
        public void AttachToPoint(ConstraintedPoint p)
        {
            point = p;
            state = SNAIL_STATE_ACTIVE;

            sleepyEyes?.SetEnabled(false);
            wakeUp?.SetEnabled(true);
            wakeUp?.PlayTimeline(0);

            if (GetCurrentTimeline() != null)
            {
                StopCurrentTimeline();
            }

            int grabbedSnails = Preferences.GetIntForKey(PrefsGrabSnails) + 1;
            Preferences.SetIntForKey(grabbedSnails, PrefsGrabSnails, false);
            if (grabbedSnails >= 100)
            {
                CTRRootController.PostAchievementName(AchievementSnailTamer);
            }

            CTRSoundMgr.PlaySound(Resources.Snd.ExpSnailIn);
        }

        /// <summary>
        /// Detaches the snail from the candy point and starts its vanish animation.
        /// </summary>
        public void Detach()
        {
            point = null;
            state = SNAIL_STATE_VANISH;

            eye1?.SetEnabled(false);
            eye2?.SetEnabled(false);

            sleep?.SetEnabled(true);
            sleep?.PlayTimeline(0);

            wakeUp?.SetEnabled(false);

            Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(3);
            timeline.AddKeyFrame(KeyFrame.MakePos((int)x, (int)y, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0));
            timeline.AddKeyFrame(KeyFrame.MakePos((int)x, (int)(y - 50), KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.3f));
            timeline.AddKeyFrame(KeyFrame.MakePos((int)x, (int)(y + SCREEN_HEIGHT), KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 2.1f));
            timeline.AddKeyFrame(KeyFrame.MakeRotation(0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
            timeline.AddKeyFrame(KeyFrame.MakeRotation(RND_RANGE(-120, 120), KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 2.4f));
            timeline.AddKeyFrame(KeyFrame.MakeSingleAction(this, SnailActionDetach, 0, 0, 2.4f));

            int timelineId = AddTimeline(timeline);
            Track rotationTrack = timeline.GetTrack(Track.TrackType.TRACK_ROTATION);
            rotationTrack.relative = true;
            PlayTimeline(timelineId);

            CTRSoundMgr.PlaySound(Resources.Snd.ExpSnailOut);
        }

        /// <summary>
        /// Sets the snail bounding box from a texture quad.
        /// </summary>
        /// <param name="quad">Texture quad index used to compute the bounding box.</param>
        public void SetBBFromQuad(int quad)
        {
            if (quad == SnailShellQuad)
            {
                bb = GameScene.GetSnailBoundingBox();
                rbb = new Quad2D(bb.x, bb.y, bb.w, bb.h);
                return;
            }

            if (texture?.quadOffsets == null || texture.quadRects == null || quad < 0 || quad >= texture.quadRects.Length)
            {
                return;
            }

            bb = MakeRectangle(
                Round(texture.quadOffsets[quad].X),
                Round(texture.quadOffsets[quad].Y),
                texture.quadRects[quad].w,
                texture.quadRects[quad].h);
            rbb = new Quad2D(bb.x, bb.y, bb.w, bb.h);
        }

        /// <inheritdoc />
        public override Image InitWithTexture(CTRTexture2D t)
        {
            if (base.InitWithTexture(t) == null)
            {
                return this;
            }

            DoRestoreCutTransparency();
            SetBBFromQuad(SnailShellQuad);

            Timeline spawnTimeline = new Timeline().InitWithMaxKeyFramesOnTrack(3);
            float spawnScale = RND_RANGE(9, 10) / 10f;
            float spawnDelay = RND_RANGE(0, 6) / 10f;
            spawnTimeline.AddKeyFrame(KeyFrame.MakeScale(spawnScale, spawnScale, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0f));
            spawnTimeline.AddKeyFrame(KeyFrame.MakeScale(1f, 1f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, spawnDelay));
            spawnTimeline.AddKeyFrame(KeyFrame.MakeSingleAction(this, ACTION_PLAY_TIMELINE, 1, 1, spawnDelay));
            _ = AddTimeline(spawnTimeline);

            Timeline pulseTimeline = new Timeline().InitWithMaxKeyFramesOnTrack(5);
            pulseTimeline.AddKeyFrame(KeyFrame.MakeScale(1f, 1f, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0f));
            pulseTimeline.AddKeyFrame(KeyFrame.MakeScale(0.9f, 0.9f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 1));
            pulseTimeline.AddKeyFrame(KeyFrame.MakeScale(0.9f, 0.9f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.3f));
            pulseTimeline.AddKeyFrame(KeyFrame.MakeScale(1f, 1f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 1f));
            pulseTimeline.AddKeyFrame(KeyFrame.MakeScale(1, 1, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.3f));
            pulseTimeline.SetTimelineLoopType(Timeline.LoopType.TIMELINE_REPLAY);
            _ = AddTimeline(pulseTimeline);

            PlayTimeline(0);

            backContainer = new BaseElement
            {
                anchor = 18,
                width = width,
                height = height
            };

            sleepyEyes = Image_createWithResIDQuad(Resources.Img.ObjSnail, SnailSleepyEyesQuad);
            sleepyEyes.DoRestoreCutTransparency();
            sleepyEyes.parentAnchor = sleepyEyes.anchor = 9;
            _ = backContainer.AddChild(sleepyEyes);

            eye1 = Image_createWithResIDQuad(Resources.Img.ObjSnail, SnailEye1Quad);
            eye1.DoRestoreCutTransparency();
            eye1.parentAnchor = eye1.anchor = 9;
            eye1.SetEnabled(false);
            _ = backContainer.AddChild(eye1);

            eye2 = Image_createWithResIDQuad(Resources.Img.ObjSnail, SnailEye2Quad);
            eye2.DoRestoreCutTransparency();
            eye2.parentAnchor = eye2.anchor = 9;
            eye2.SetEnabled(false);
            _ = backContainer.AddChild(eye2);

            wakeUp = Animation_createWithResID(Resources.Img.ObjSnail);
            wakeUp.SetDrawQuad(SnailWakeStartQuad);
            wakeUp.parentAnchor = wakeUp.anchor = 9;
            wakeUp.SetEnabled(false);
            wakeUp.DoRestoreCutTransparency();
            _ = wakeUp.AddAnimationDelayLoopFirstLast(0.1f, Timeline.LoopType.TIMELINE_NO_LOOP, SnailWakeStartQuad, SnailWakeEndQuad);
            Timeline wakeUpTimeline = wakeUp.GetTimeline(0);
            wakeUpTimeline.delegateTimelineDelegate = this;
            _ = backContainer.AddChild(wakeUp);

            sleep = Animation_createWithResID(Resources.Img.ObjSnail);
            sleep.SetDrawQuad(SnailSleepStartQuad);
            sleep.parentAnchor = sleep.anchor = 9;
            sleep.SetEnabled(false);
            sleep.DoRestoreCutTransparency();
            _ = sleep.AddAnimationDelayLoopFirstLast(0.1f, Timeline.LoopType.TIMELINE_NO_LOOP, SnailSleepStartQuad, SnailSleepEndQuad);
            sleep.PlayTimeline(0);
            Timeline sleepTimeline = sleep.GetTimeline(0);
            sleepTimeline.delegateTimelineDelegate = this;
            _ = backContainer.AddChild(sleep);

            state = SNAIL_STATE_INACTIVE;
            return this;
        }

        /// <inheritdoc />
        public override void Draw()
        {
            backContainer?.Draw();
            base.Draw();
        }

        /// <inheritdoc />
        public override void Update(float delta)
        {
            base.Update(delta);
            backContainer?.Update(delta);

            if (point != null)
            {
                x = point.pos.X;
                y = point.pos.Y;
            }

            if (backContainer != null)
            {
                backContainer.x = x;
                backContainer.y = y;
                backContainer.color = color;
                backContainer.scaleX = scaleX;
                backContainer.scaleY = scaleY;
                backContainer.rotation = rotation;
            }
        }

        /// <inheritdoc />
        public override bool HandleAction(ActionData a)
        {
            if (base.HandleAction(a))
            {
                return true;
            }

            if (a.actionName != SnailActionDetach)
            {
                return false;
            }

            state = SNAIL_STATE_VANISHED;
            return true;
        }

        /// <inheritdoc />
        public void TimelineFinished(Timeline t)
        {
            if (t?.element == wakeUp)
            {
                eye1?.SetEnabled(true);
                eye2?.SetEnabled(true);
                wakeUp?.SetEnabled(false);
                return;
            }

            if (t?.element == sleep)
            {
                sleepyEyes?.SetEnabled(true);
                sleep?.SetEnabled(false);
            }
        }

        /// <inheritdoc />
        public void TimelinereachedKeyFramewithIndex(Timeline t, KeyFrame k, int i)
        {
        }

        /// <summary>Inactive snail state value.</summary>
        public const int SNAIL_STATE_INACTIVE = 0;

        /// <summary>State value used while the snail is attached to candy.</summary>
        public const int SNAIL_STATE_ACTIVE = 1;

        /// <summary>State value used while the snail is playing its vanish animation.</summary>
        public const int SNAIL_STATE_VANISH = 2;

        /// <summary>State value used after the vanish animation has completed.</summary>
        public const int SNAIL_STATE_VANISHED = 3;

        /// <summary>Initial snail rotation used when restoring state.</summary>
        public float startRotation;

        /// <summary>Container for eye and wake/sleep overlay visuals drawn behind the shell.</summary>
        private BaseElement backContainer;

        /// <summary>Candy physics point currently followed by the snail.</summary>
        private ConstraintedPoint point;

        /// <summary>Sleepy eyes overlay visual.</summary>
        private Image sleepyEyes;

        /// <summary>First active eye overlay visual.</summary>
        private Image eye1;

        /// <summary>Second active eye overlay visual.</summary>
        private Image eye2;

        /// <summary>Wake-up animation played when the snail attaches to candy.</summary>
        private Animation wakeUp;

        /// <summary>Sleep animation played when the snail detaches from candy.</summary>
        private Animation sleep;
    }
}
