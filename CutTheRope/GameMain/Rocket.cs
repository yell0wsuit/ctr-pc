using System;
using System.Xml.Linq;

using CutTheRope.Framework.Core;
using CutTheRope.Framework.Sfe;
using CutTheRope.Framework.Visual;
using CutTheRope.Helpers;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Represents a rocket game object that can be rotated by touch input, fly along a path,
    /// and produce spark and cloud particle effects from its exhaust.
    /// </summary>
    internal sealed class Rocket : CTRGameObject, ITimelineDelegate
    {
        /// <summary>
        /// Creates a new <see cref="Rocket"/> instance initialized with the specified texture.
        /// </summary>
        /// <param name="t">The texture to apply to the rocket.</param>
        /// <returns>A new <see cref="Rocket"/> initialized with the given texture.</returns>
        private static Rocket Rocket_create(CTRTexture2D t)
        {
            return (Rocket)new Rocket().InitWithTexture(t);
        }

        /// <summary>
        /// Creates a new <see cref="Rocket"/> from a named texture resource and assigns it a draw quad.
        /// </summary>
        /// <param name="resourceName">The resource name used to look up the texture.</param>
        /// <param name="q">The draw quad index to assign to the rocket.</param>
        /// <returns>A new <see cref="Rocket"/> configured with the specified resource and quad.</returns>
        public static Rocket Rocket_createWithResIDQuad(string resourceName, int q)
        {
            Rocket rocket = Rocket_create(Application.GetTexture(resourceName));
            rocket.SetDrawQuad(q);
            return rocket;
        }

        /// <summary>
        /// Initializes the rocket with a texture, setting up rotation timelines, the scale-down
        /// (exhaust) timeline, the physics point, the container element, and the spark animation.
        /// </summary>
        /// <param name="tx">The texture to initialize the rocket with.</param>
        /// <returns>This <see cref="Rocket"/> instance after initialization.</returns>
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

                // Why 2.5 weight instead of 0.5?
                // In the decompiled Windows Phone version, the rocket's weight
                // is 0.5 (invWeight = 1 / 0.5 = 2.0). However, on PC the lower
                // weight makes the rocket too susceptible to constraint forces,
                // causing it to drift from its intended position. A weight of
                // 2.5 (invWeight = 0.4) keeps the rocket stable by reducing
                // how much it moves in response to forces from connected rope
                // points.
                point.SetWeight(2.5f);

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

        /// <summary>
        /// Updates the rocket each frame: synchronizes position with the physics point,
        /// updates the container, and repositions the exhaust particle emitters.
        /// </summary>
        /// <param name="delta">The elapsed time since the last frame, in seconds.</param>
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
            float exhaustOffset = GetExhaustOffset();
            Vector vector = Vect(x, y);
            vector = VectAdd(vector, VectMult(VectForAngle(angle), exhaustOffset));
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

        /// <summary>
        /// Parses a mover definition from XML, creating a <see cref="CTRMover"/> that follows the
        /// specified path with the given move and rotate speeds.
        /// </summary>
        /// <param name="xml">The XML element containing <c>path</c>, <c>moveSpeed</c>, and <c>rotateSpeed</c> attributes.</param>
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

        /// <summary>
        /// Draws the rocket by first rendering the container (which holds the spark animation),
        /// then rendering the rocket sprite itself.
        /// </summary>
        public override void Draw()
        {
            container.Draw();
            base.Draw();
        }

        /// <inheritdoc />
        public void TimelinereachedKeyFramewithIndex(Timeline t, KeyFrame k, int i)
        {
        }

        /// <inheritdoc />
        /// <remarks>
        /// When the scale-down timeline (ID 2) finishes, notifies the <see cref="delegateRocketDelegate"/>
        /// that the rocket has been exhausted.
        /// </remarks>
        public void TimelineFinished(Timeline t)
        {
            RotateWithBB(rotation);
            if (GetTimeline(2) == t && delegateRocketDelegate != null)
            {
                delegateRocketDelegate.Exhausted(this);
            }
        }

        /// <summary>
        /// Recalculates the bounding box corner vectors (<see cref="t1"/> and <see cref="t2"/>)
        /// based on the current rotation and position.
        /// </summary>
        public void UpdateRotation()
        {
            t1.X = x - (bb.w / 2f);
            t2.X = x + (bb.w / 2f);
            t1.Y = t2.Y = y;
            angle = DEGREES_TO_RADIANS(rotation);
            t1 = VectRotateAround(t1, angle, x, y);
            t2 = VectRotateAround(t2, angle, x, y);
        }

        /// <summary>
        /// Computes the rotation angle (in degrees) between two points relative to a center point.
        /// </summary>
        /// <param name="v1">The starting point.</param>
        /// <param name="v2">The ending point.</param>
        /// <param name="c">The center of rotation.</param>
        /// <returns>The signed rotation angle in degrees from <paramref name="v1"/> to <paramref name="v2"/>.</returns>
        private static float GetRotateAngleForStartEndCenter(Vector v1, Vector v2, Vector c)
        {
            Vector vector = VectSub(v1, c);
            Vector vector2 = VectSub(v2, c);
            float num = VectAngleNormalized(vector2) - VectAngleNormalized(vector);
            return RADIANS_TO_DEGREES(num);
        }

        /// <summary>
        /// Records the initial touch position for a rotation gesture.
        /// </summary>
        /// <param name="v">The touch position.</param>
        public void HandleTouch(Vector v)
        {
            lastTouch = v;
            firstTouch = v;
        }

        /// <summary>
        /// Processes a rotation gesture update. Ignores movement below a 10-unit dead zone
        /// from the initial touch, then applies incremental rotation based on the angle change
        /// around the rocket's center.
        /// </summary>
        /// <param name="v">The current touch position.</param>
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

        /// <summary>
        /// Finalizes a rotation gesture by snapping the rocket's rotation to the nearest 45-degree
        /// increment via an animated timeline.
        /// </summary>
        public void HandleRotateFinal()
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

        /// <summary>
        /// Enables the spark animation and begins playing it.
        /// </summary>
        public void StartAnimation()
        {
            sparks.SetEnabled(true);
            sparks.PlayTimeline(0);
        }

        /// <summary>
        /// Stops the rocket animation by playing the scale-down timeline, stopping the spark
        /// animation, stopping and releasing both particle systems, and stopping all sounds.
        /// </summary>
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

        /// <summary>The rocket is idle and not in use.</summary>
        public const int STATE_ROCKET_IDLE = 0;

        /// <summary>The rocket is in the distance/approach phase.</summary>
        public const int STATE_ROCKET_DIST = 1;

        /// <summary>The rocket is actively flying.</summary>
        public const int STATE_ROCKET_FLY = 2;

        /// <summary>The rocket has exhausted its fuel and is winding down.</summary>
        public const int STATE_ROCKET_EXAUST = 3;

        /// <summary>
        /// Calculates the offset from the rocket's center to the exhaust emission point,
        /// based on the rocket quad's half-length and current scale.
        /// </summary>
        /// <returns>The exhaust offset distance.</returns>
        private float GetExhaustOffset()
        {
            return GetRocketQuadHalfLength() * Math.Abs(scaleX);
        }

        /// <summary>
        /// Returns the cached half-length of the rocket body quad, computing it from the
        /// quad size on first access.
        /// </summary>
        /// <returns>Half the width of the rocket body quad.</returns>
        private static float GetRocketQuadHalfLength()
        {
            if (rocketQuadHalfLength < 0f)
            {
                Vector quadSize = GetQuadSize(Resources.Img.ObjRocket, RocketBodyQuad);
                rocketQuadHalfLength = quadSize.X * 0.5f;
            }
            return rocketQuadHalfLength;
        }

        // private const int MIN_CICRLE_POINTS = 10;

        /// <summary>The quad index for the rocket body sprite.</summary>
        private const int RocketBodyQuad = 10;

        /// <summary>Cached half-length of the rocket body quad; -1 indicates not yet computed.</summary>
        private static float rocketQuadHalfLength = -1f;

        /// <summary>The most recent touch position during a rotation gesture.</summary>
        private Vector lastTouch;

        /// <summary>The initial touch position when a rotation gesture started.</summary>
        private Vector firstTouch;

        /// <summary>The physics constraint point controlling the rocket's position.</summary>
        public ConstraintedPoint point;

        /// <summary>The rocket's current facing angle in radians.</summary>
        public float angle;

        /// <summary>Left edge vector of the rotated bounding box.</summary>
        private Vector t1;

        /// <summary>Right edge vector of the rotated bounding box.</summary>
        private Vector t2;

        /// <summary>Elapsed time tracker used during flight.</summary>
        public float time;

        /// <summary>The impulse force applied to the rocket when launched.</summary>
        public float impulse;

        /// <summary>Multiplier applied to the impulse force.</summary>
        public float impulseFactor;

        /// <summary>The candy's rotation at the time the rocket was activated.</summary>
        public float startCandyRotation;

        /// <summary>The rocket's rotation at the time it was activated.</summary>
        public float startRotation;

        /// <summary>Current operating state (-1 = uninitialized, see <c>STATE_ROCKET_*</c> constants).</summary>
        public int isOperating;

        /// <summary>Whether the rocket can be rotated by touch input.</summary>
        public bool isRotatable;

        /// <summary>Whether a rotation gesture has been recognized (past the dead zone).</summary>
        public bool rotateHandled;

        /// <summary>The percentage of the rotation angle used for interpolation.</summary>
        public float anglePercent;

        /// <summary>An additional angle offset applied on top of the base rotation.</summary>
        public float additionalAngle;

        /// <summary>Whether the perpendicular direction has been set.</summary>
        public bool perpSetted;

        /// <summary>The spark animation displayed at the rocket's exhaust.</summary>
        public Animation sparks;

        /// <summary>Container element that holds the spark animation and matches the rocket's transform.</summary>
        public BaseElement container;

        /// <summary>Particle system for the rocket's spark exhaust trail.</summary>
        public RocketSparks particles;

        /// <summary>Particle system for the rocket's cloud exhaust trail.</summary>
        public RocketClouds cloudParticles;

        /// <summary>Delegate that receives rocket lifecycle callbacks (e.g., exhaustion).</summary>
        public IRocketDelegate delegateRocketDelegate;
    }
}
