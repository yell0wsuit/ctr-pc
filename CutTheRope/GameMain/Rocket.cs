using System;
using System.Xml.Linq;

using CutTheRope.Framework.Core;
using CutTheRope.Framework.Sfe;
using CutTheRope.Framework.Visual;
using CutTheRope.Helpers;

namespace CutTheRope.GameMain
{
    internal sealed class Rocket : CTRGameObject, ITimelineDelegate
    {
        private static Rocket Rocket_create(CTRTexture2D t)
        {
            return (Rocket)new Rocket().InitWithTexture(t);
        }

        public static Rocket Rocket_createWithResIDQuad(string resourceName, int q)
        {
            Rocket rocket = Rocket_create(Application.GetTexture(resourceName));
            rocket.SetDrawQuad(q);
            return rocket;
        }

        public override Image InitWithTexture(CTRTexture2D tx)
        {
            if (base.InitWithTexture(tx) != null)
            {
                isOperating = -1;

                Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
                AddTimelinewithID(timeline, 0);
                timeline.AddKeyFrame(KeyFrame.MakeRotation(0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0f));
                timeline.AddKeyFrame(KeyFrame.MakeRotation(45.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.1));
                timeline.delegateTimelineDelegate = this;
                Track track = timeline.GetTrack(Track.TrackType.TRACK_ROTATION);
                track.relative = true;

                timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
                AddTimelinewithID(timeline, 1);
                timeline.AddKeyFrame(KeyFrame.MakeRotation(0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0f));
                timeline.AddKeyFrame(KeyFrame.MakeRotation(-45.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.1));
                timeline.delegateTimelineDelegate = this;
                track = timeline.GetTrack(Track.TrackType.TRACK_ROTATION);
                track.relative = true;

                timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
                timeline.AddKeyFrame(KeyFrame.MakeScale(0.7, 0.7, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.AddKeyFrame(KeyFrame.MakeScale(0.0, 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.2));
                timeline.delegateTimelineDelegate = this;
                AddTimelinewithID(timeline, 2);

                point = new ConstraintedPoint
                {
                    disableGravity = true
                };
                point.SetWeight(0.5f);

                container = new BaseElement
                {
                    width = width,
                    height = height,
                    anchor = 18
                };

                sparks = Animation_createWithResID(Resources.Img.ObjRocket);
                sparks.parentAnchor = sparks.anchor = 18;
                sparks.SetEnabled(false);
                sparks.DoRestoreCutTransparency();
                _ = sparks.AddAnimationDelayLoopFirstLast(0.1f, Timeline.LoopType.TIMELINE_REPLAY, 1, 4);
                _ = container.AddChild(sparks);

                sparks.blendingMode = 2;
                blendingMode = 1;
                sparks.scaleX = sparks.scaleY = 0.7f;
            }
            return this;
        }

        public override void Update(float delta)
        {
            base.Update(delta);
            point.Update(delta);
            container.Update(delta);
            if (mover != null && !mover.IsPaused)
            {
                point.pos.X = x;
                point.pos.Y = y;
            }
            else
            {
                x = point.pos.X;
                y = point.pos.Y;
            }
            container.rotation = rotation;
            container.x = x;
            container.y = y;
            float num = VectLength(VectSub(point.prevPos, point.pos));
            num = MAX(num, 1f);
            float num2 = angle - (float)Math.PI;
            Vector vector = Vect(x, y);
            vector = VectAdd(vector, VectMult(VectForAngle(angle), 35f));
            if (particles != null)
            {
                particles.x = vector.X;
                particles.y = vector.Y;
                particles.angle = rotation;
                particles.initialAngle = num2;
                particles.speed = num * 50f;
            }
            if (cloudParticles != null)
            {
                cloudParticles.x = vector.X;
                cloudParticles.y = vector.Y;
                cloudParticles.angle = rotation;
                cloudParticles.initialAngle = num2;
                cloudParticles.speed = num * 40f;
            }
        }

        public override void ParseMover(XElement xml)
        {
            string path = xml.AttributeAsNSString("path");
            if (!string.IsNullOrEmpty(path))
            {
                int num = 100;
                if (path.CharacterAtIndex(0) == 'R')
                {
                    int num2 = path.SubstringFromIndex(2).IntValue();
                    num = MAX(11, (num2 / 2) + 1);
                }
                float moveSpeed = xml.AttributeAsNSString("moveSpeed").FloatValue();
                float rotateSpeed = xml.AttributeAsNSString("rotateSpeed").FloatValue();
                CTRMover ctrMover = new(num, moveSpeed, rotateSpeed)
                {
                    angle_ = rotation
                };
                ctrMover.angle_initial = ctrMover.angle_;
                ctrMover.SetPathFromStringandStart(path, Vect(x, y));
                SetMover(ctrMover);
                ctrMover.Start();
            }
        }

        public override void Draw()
        {
            container.Draw();
            base.Draw();
        }

        public void TimelinereachedKeyFramewithIndex(Timeline t, KeyFrame k, int i)
        {
        }

        public void TimelineFinished(Timeline t)
        {
            RotateWithBB(rotation);
            if (GetTimeline(2) == t && delegateRocketDelegate != null)
            {
                delegateRocketDelegate.Exhausted(this);
            }
        }

        public void UpdateRotation()
        {
            t1.X = x - (bb.w / 2f);
            t2.X = x + (bb.w / 2f);
            t1.Y = t2.Y = y;
            angle = DEGREES_TO_RADIANS(rotation);
            t1 = VectRotateAround(t1, angle, x, y);
            t2 = VectRotateAround(t2, angle, x, y);
        }

        private static float GetRotateAngleForStartEndCenter(Vector v1, Vector v2, Vector c)
        {
            Vector vector = VectSub(v1, c);
            Vector vector2 = VectSub(v2, c);
            float num = VectAngleNormalized(vector2) - VectAngleNormalized(vector);
            return RADIANS_TO_DEGREES(num);
        }

        public void HandleTouch(Vector v)
        {
            lastTouch = v;
            firstTouch = v;
        }

        public void HandleRotate(Vector v)
        {
            if (!rotateHandled && VectLength(VectSub(v, firstTouch)) <= 10f)
            {
                return;
            }
            float num = GetRotateAngleForStartEndCenter(lastTouch, v, Vect(x, y));
            num = AngleTo0_360(num);
            rotation += num;
            lastTouch = v;
            rotateHandled = true;
            RotateWithBB(rotation);
        }

        public void HandleRotateFinal(Vector v)
        {
            rotation = AngleTo0_360(rotation);
            float num = Round(rotation / 45f);
            float num2 = 45f * num;
            RemoveTimeline(1);
            Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            timeline.AddKeyFrame(KeyFrame.MakeRotation(rotation, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline.AddKeyFrame(KeyFrame.MakeRotation((double)num2, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.1));
            timeline.delegateTimelineDelegate = this;
            AddTimelinewithID(timeline, 1);
            PlayTimeline(1);
        }

        public void StartAnimation()
        {
            sparks.SetEnabled(true);
            sparks.PlayTimeline(0);
        }

        public void StopAnimation()
        {
            PlayTimeline(2);
            Timeline currentTimeline = sparks.GetCurrentTimeline();
            if (currentTimeline != null && currentTimeline.state == Timeline.TimelineState.TIMELINE_PLAYING)
            {
                sparks.StopCurrentTimeline();
            }
            sparks.SetEnabled(false);
            particles?.StopSystem();
            cloudParticles?.StopSystem();
            particles = null;
            cloudParticles = null;
            CTRSoundMgr.StopSounds();
        }

        public const int STATE_ROCKET_IDLE = 0;
        public const int STATE_ROCKET_DIST = 1;
        public const int STATE_ROCKET_FLY = 2;
        public const int STATE_ROCKET_EXAUST = 3;

        // private const int MIN_CICRLE_POINTS = 10;

        private Vector lastTouch;
        private Vector firstTouch;
        public ConstraintedPoint point;
        public float angle;
        private Vector t1;
        private Vector t2;
        public float time;
        public float impulse;
        public float impulseFactor;
        public float startCandyRotation;
        public float startRotation;
        public int isOperating;
        public bool isRotatable;
        public bool rotateHandled;
        public float anglePercent;
        public float additionalAngle;
        public bool perp;
        public bool perpSetted;
        public Bungee activeBungee;
        public Animation sparks;
        public BaseElement container;
        public AnimationsPool aniPool;
        public RocketSparks particles;
        public RocketClouds cloudParticles;
        public IRocketDelegate delegateRocketDelegate;
    }
}
