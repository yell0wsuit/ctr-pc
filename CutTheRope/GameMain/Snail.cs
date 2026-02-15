using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Sfe;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    internal sealed class Snail : GameObject, ITimelineDelegate
    {
        private const int SnailShellQuad = 8;
        private const int SnailSleepyEyesQuad = 2;
        private const int SnailWakeStartQuad = 3;
        private const int SnailWakeEndQuad = 5;
        private const int SnailEye1Quad = 6;
        private const int SnailEye2Quad = 7;
        private const int SnailSleepStartQuad = 5;
        private const int SnailSleepEndQuad = 3;
        private const string SnailActionDetach = "SNAIL_ACTION_DETACH";
        private const string PrefsGrabSnails = "PREFS_GRAB_SNAILS";
        private const string AchievementSnailTamer = "acSnailTamer";

        private static Snail Snail_create(CTRTexture2D texture)
        {
            return (Snail)new Snail().InitWithTexture(texture);
        }

        public static Snail Snail_createWithResIDQuad(string resourceName, int q)
        {
            Snail snail = Snail_create(Application.GetTexture(resourceName));
            snail.SetDrawQuad(q);
            return snail;
        }

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
            timeline.AddKeyFrame(KeyFrame.MakePos(x, y, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0.0));
            timeline.AddKeyFrame(KeyFrame.MakePos(x, y - 50.0, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.3));
            timeline.AddKeyFrame(KeyFrame.MakePos(x, y + SCREEN_HEIGHT, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 2.1));
            timeline.AddKeyFrame(KeyFrame.MakeRotation(0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline.AddKeyFrame(KeyFrame.MakeRotation(RND_RANGE(-120, 120), KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 2.4));
            timeline.AddKeyFrame(KeyFrame.MakeSingleAction(this, SnailActionDetach, 0, 0, 2.4));

            int timelineId = AddTimeline(timeline);
            Track rotationTrack = timeline.GetTrack(Track.TrackType.TRACK_ROTATION);
            rotationTrack.relative = true;
            PlayTimeline(timelineId);

            CTRSoundMgr.PlaySound(Resources.Snd.ExpSnailOut);
        }

        public void SetBBFromQuad(int quad)
        {
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
            pulseTimeline.AddKeyFrame(KeyFrame.MakeScale(0.9, 0.9, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 1.0));
            pulseTimeline.AddKeyFrame(KeyFrame.MakeScale(0.9, 0.9, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.3));
            pulseTimeline.AddKeyFrame(KeyFrame.MakeScale(1f, 1f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 1f));
            pulseTimeline.AddKeyFrame(KeyFrame.MakeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.3));
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

        public override void Draw()
        {
            backContainer?.Draw();
            base.Draw();
        }

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

        public void TimelinereachedKeyFramewithIndex(Timeline t, KeyFrame k, int i)
        {
        }

        public const int SNAIL_STATE_INACTIVE = 0;
        public const int SNAIL_STATE_ACTIVE = 1;
        public const int SNAIL_STATE_VANISH = 2;
        public const int SNAIL_STATE_VANISHED = 3;

        public float startRotation;

        private BaseElement backContainer;
        private ConstraintedPoint point;
        private Image sleepyEyes;
        private Image eye1;
        private Image eye2;
        private Animation wakeUp;
        private Animation sleep;
    }
}
