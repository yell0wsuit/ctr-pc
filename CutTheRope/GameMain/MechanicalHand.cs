using System.Collections.Generic;

using CutTheRope.Framework.Core;
using CutTheRope.Framework.Sfe;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Composite mechanical hand element made of articulated segments and a claw.
    /// Handles segment hierarchy, claw position tracking, and catch/release animations.
    /// </summary>
    internal sealed class MechanicalHand : BaseElement
    {
        /// <summary>
        /// Initializes a hand with a lightweight constrained point used for candy attachment.
        /// </summary>
        public MechanicalHand()
        {
            rotatingSegment = null;
            state = STATE_HAND_IDLE;
            cPoint = new ConstraintedPoint
            {
                disableGravity = true
            };
            cPoint.SetWeight(0.0001f);
            releaseSoundPlayed = false;

            Vector jointCenter = Image.GetQuadCenter(Resources.Img.ObjRoboHand, 2);
            Vector candyAnchor = Image.GetQuadCenter(Resources.Img.ObjRoboHand, 8);
            Vector offset = VectSub(candyAnchor, jointCenter);

            // Some atlases carry a broken marker frame offset for quad 8 (0,0),
            // which puts the candy anchor far away and prevents hand grabs.
            if (VectLength(offset) > 80f)
            {
                CTRTexture2D texture = Application.GetTexture(Resources.Img.ObjRoboHand);
                if (texture != null && texture.preCutSize.X > 0f && texture.preCutSize.Y > 0f)
                {
                    const float legacyAnchorX = 51f / 96f;
                    const float legacyAnchorY = 49f / 96f;
                    candyAnchor = Vect(texture.preCutSize.X * legacyAnchorX, texture.preCutSize.Y * legacyAnchorY);
                    offset = VectSub(candyAnchor, jointCenter);
                }
            }
            clawOffset = offset;
        }

        /// <summary>
        /// Appends a segment to the hand chain.
        /// </summary>
        /// <param name="segmentLength">Segment length in world units.</param>
        /// <param name="segmentAngle">Initial segment angle in degrees.</param>
        /// <param name="rotatable">Whether the segment can be rotated by player input.</param>
        public void AddSegmentWithLengthAngleRotatable(float segmentLength, float segmentAngle, bool rotatable)
        {
            Vector start = Vect(0f, 0f);
            segments ??= [];
            if (segments.Count > 0)
            {
                start = LastSegment().endPosition;
            }

            MechanicalHandSegment segment = new MechanicalHandSegment().InitWithPositionLengthAngleRotatable(Vect(start.X, start.Y), segmentLength, segmentAngle, rotatable);
            segment.anchor = 18;
            segment.parentAnchor = 18;
            segment.theHand = this;

            if (segments.Count > 0)
            {
                LastSegment().RemoveChildWithID(0);
                LastSegment().endsWithHand = false;
                _ = LastSegment().AddChild(segment);

                BaseElement parentElement = segment.parent;
                for (int i = 0; i <= segments.Count - 1 && parentElement != null; i++)
                {
                    segment.rotation -= parentElement.rotation;
                    parentElement = parentElement.parent;
                }
            }
            else
            {
                _ = AddChild(segment);
                segment.drawBase = true;
            }

            segments.Add(segment);
            CalculateTopLeft(segment);
            TheClaw().prevSegments = segments.Count - 1;
        }

        /// <summary>
        /// Gets the world position of a segment joint by index.
        /// </summary>
        /// <param name="index">Joint index where 0 is the hand base.</param>
        /// <returns>Joint world position.</returns>
        public Vector JointAtIndexPosition(int index)
        {
            if (index == 0)
            {
                return Vect(drawX, drawY);
            }

            Vector position = Vect(drawX, drawY);
            float angle = 0f;
            for (int i = 0; i < index; i++)
            {
                angle += SegmentAtIndex(i).rotation;
                position = VectAdd(position, VectRotate(SegmentAtIndex(i).endPosition, DEGREES_TO_RADIANS(angle)));
            }
            return position;
        }

        /// <summary>
        /// Computes the world position of the claw candy anchor.
        /// </summary>
        /// <returns>Claw anchor world position.</returns>
        public Vector ClawPosition()
        {
            BaseElement element = GetChild(0);
            Vector position = Vect(drawX, drawY);
            float angle = 0f;
            for (int i = 0; i <= segments.Count - 1; i++)
            {
                MechanicalHandSegment segment = (MechanicalHandSegment)element;
                angle += element.rotation;
                position = VectAdd(position, VectRotate(segment.endPosition, DEGREES_TO_RADIANS(angle)));
                element = element.GetChild(0);
            }
            return VectAdd(position, VectRotate(clawOffset, DEGREES_TO_RADIANS(angle)));
        }

        /// <summary>
        /// Indicates whether any segment is currently playing a rotation timeline.
        /// </summary>
        /// <returns><c>true</c> when at least one segment is animating.</returns>
        public bool IsRotating()
        {
            if (segments == null)
            {
                return false;
            }

            foreach (MechanicalHandSegment segment in segments)
            {
                if (segment != null && segment.GetCurrentTimeline() != null)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Plays the claw release bounce animation.
        /// </summary>
        /// <param name="animationPool">Animation pool responsible for timeline lifecycle.</param>
        public void AnimateReleaseWithAnimationsPool(AnimationsPool animationPool)
        {
            Timeline timeline = CatchBounceTimelineWithInitialScaleandAmplitude(TheClaw().clawIdle.scaleX, 0.25f);
            timeline.delegateTimelineDelegate = animationPool;
            int timelineId = TheClaw().clawIdle.AddTimeline(timeline);
            TheClaw().clawIdle.PlayTimeline(timelineId);
        }

        /// <summary>
        /// Plays catch bounce animation on the claw and optional candy visuals.
        /// </summary>
        /// <param name="candyParts">Candy parts to animate alongside the claw.</param>
        /// <param name="animationPool">Animation pool responsible for timeline lifecycle.</param>
        public void AnimateCatchWithCandyPartsandAnimationsPool(List<BaseElement> candyParts, AnimationsPool animationPool)
        {
            const float amplitude = 0.1f;

            Timeline bodyTimeline = CatchBounceTimelineWithInitialScaleandAmplitude(TheClaw().clawActive.scaleX, amplitude);
            Timeline fingersTimeline = CatchBounceTimelineWithInitialScaleandAmplitude(TheClaw().clawActiveFingers.scaleX, amplitude);
            bodyTimeline.delegateTimelineDelegate = animationPool;
            fingersTimeline.delegateTimelineDelegate = animationPool;

            int bodyTimelineId = TheClaw().clawActive.AddTimeline(bodyTimeline);
            int fingersTimelineId = TheClaw().clawActiveFingers.AddTimeline(fingersTimeline);
            TheClaw().clawActive.PlayTimeline(bodyTimelineId);
            TheClaw().clawActiveFingers.PlayTimeline(fingersTimelineId);

            if (candyParts == null)
            {
                return;
            }

            foreach (BaseElement candyPart in candyParts)
            {
                if (candyPart == null)
                {
                    continue;
                }

                Timeline candyTimeline = CatchBounceTimelineWithInitialScaleandAmplitude(0.71f, amplitude);
                candyTimeline.delegateTimelineDelegate = animationPool;
                int candyTimelineId = candyPart.AddTimeline(candyTimeline);
                candyPart.PlayTimeline(candyTimelineId);
            }
        }

        /// <summary>
        /// Gets a segment by index.
        /// </summary>
        /// <param name="index">Segment index.</param>
        /// <returns>The requested segment.</returns>
        public MechanicalHandSegment SegmentAtIndex(int index)
        {
            return segments[index];
        }

        /// <summary>
        /// Gets the terminal segment in the chain.
        /// </summary>
        /// <returns>The last segment.</returns>
        public MechanicalHandSegment LastSegment()
        {
            return segments[^1];
        }

        /// <summary>
        /// Gets the claw attached to the terminal segment.
        /// </summary>
        /// <returns>Current claw instance.</returns>
        public MechanicalHandClaw TheClaw()
        {
            return (MechanicalHandClaw)LastSegment().GetChild(0);
        }

        public override void Update(float delta)
        {
            base.Update(delta);
            cPoint.pos = ClawPosition();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                cPoint = null;
                segments = null;
                rotatingSegment = null;
            }
            base.Dispose(disposing);
        }

        private static Timeline CatchBounceTimelineWithInitialScaleandAmplitude(float startScale, float amplitude)
        {
            Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            float bounceScale = startScale + (amplitude * startScale);
            timeline.AddKeyFrame(KeyFrame.MakeScale(bounceScale, bounceScale, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.05));
            timeline.AddKeyFrame(KeyFrame.MakeScale(startScale, startScale, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.1));
            return timeline;
        }

        public const int MH_CLAW_RADIUS = 17;

        public const int MH_JOINT_RADIUS = 12;

        public const float MH_WORLD_SCALE = 3f;

        public const float MH_CLAW_TOUCH_RADIUS = MH_CLAW_RADIUS * MH_WORLD_SCALE;

        public const float MH_BUTTON_TOUCH_RADIUS = 30f * MH_WORLD_SCALE;

        public const float MH_GRAB_DISTANCE = 25.2f * MH_WORLD_SCALE;

        public const float MH_RELEASE_DISTANCE = 34f * MH_WORLD_SCALE;

        public const int STATE_HAND_IDLE = 0;

        public const int STATE_HAND_CANDY = 1;

        public const int STATE_HAND_RELEASE = 2;

        public int state;

        public bool doRotateCandy;

        public bool releaseSoundPlayed;

        private Vector clawOffset;

        public ConstraintedPoint cPoint;

        public List<MechanicalHandSegment> segments;

        public MechanicalHandSegment rotatingSegment;
    }
}
