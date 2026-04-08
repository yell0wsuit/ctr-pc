using System.Collections.Generic;

using CutTheRope.Framework.Core;
using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Physics;
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
            clapTimer = 0f;
            canPlayClap = false;

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
        /// <returns><see langword="true" /> when at least one segment is animating.</returns>
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
            _ = animationPool;
            TheClaw().clawIdle.PlayTimeline(0);
        }

        /// <summary>
        /// Plays the claw clap bounce animation used when idle hands clap near each other.
        /// </summary>
        public void AnimateClap()
        {
            TheClaw().clawIdle.PlayTimeline(1);
        }

        /// <summary>
        /// Plays catch bounce animation on the claw and optional candy visuals.
        /// </summary>
        /// <param name="candyParts">Candy parts to animate alongside the claw.</param>
        /// <param name="animationPool">Animation pool responsible for timeline lifecycle.</param>
        public void AnimateCatchWithCandyPartsandAnimationsPool(List<BaseElement> candyParts, AnimationsPool animationPool)
        {
            const float amplitude = 0.1f;

            TheClaw().clawActive.PlayTimeline(1);
            TheClaw().clawActiveFingers.PlayTimeline(1);

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

        /// <inheritdoc />
        public override void Update(float delta)
        {
            base.Update(delta);
            cPoint.pos = ClawPosition();
            _ = Mover.MoveVariableToTarget(ref clapTimer, 0f, 1f, delta);
        }

        /// <inheritdoc />
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

        /// <summary>
        /// Creates a short scale bounce timeline used by mechanical hand catch and clap animations.
        /// </summary>
        /// <param name="startScale">Base scale to return to after the bounce.</param>
        /// <param name="amplitude">Bounce amplitude as a multiplier of <paramref name="startScale"/>.</param>
        /// <returns>The configured bounce timeline.</returns>
        internal static Timeline CatchBounceTimelineWithInitialScaleandAmplitude(float startScale, float amplitude)
        {
            Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            float bounceScale = startScale + (amplitude * startScale);
            timeline.AddKeyFrame(KeyFrame.MakeScale(bounceScale, bounceScale, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.05f));
            timeline.AddKeyFrame(KeyFrame.MakeScale(startScale, startScale, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.1f));
            return timeline;
        }

        /// <summary>Claw collision radius before world scaling.</summary>
        public const int MH_CLAW_RADIUS = 17;

        /// <summary>Joint collision radius before world scaling.</summary>
        public const int MH_JOINT_RADIUS = 12;

        /// <summary>World scaling factor used by mechanical hand distances.</summary>
        public const float MH_WORLD_SCALE = 3f;

        /// <summary>Touch radius for releasing candy from the claw.</summary>
        public const float MH_CLAW_TOUCH_RADIUS = MH_CLAW_RADIUS * MH_WORLD_SCALE;

        /// <summary>Touch radius for rotating segment buttons.</summary>
        public const float MH_BUTTON_TOUCH_RADIUS = 30f * MH_WORLD_SCALE;

        /// <summary>Maximum distance at which an idle hand can grab candy.</summary>
        public const float MH_GRAB_DISTANCE = 25.2f * MH_WORLD_SCALE;

        /// <summary>Distance at which a releasing hand returns to idle state.</summary>
        public const float MH_RELEASE_DISTANCE = 34f * MH_WORLD_SCALE;

        /// <summary>Maximum distance at which two idle hands can clap.</summary>
        public const float MH_CLAP_DISTANCE = 40.8f * MH_WORLD_SCALE;

        /// <summary>Cooldown before a hand can play another clap effect.</summary>
        public const float MH_CLAP_COOLDOWN = 0.3f;

        /// <summary>Idle hand state value.</summary>
        public const int STATE_HAND_IDLE = 0;

        /// <summary>State value used while the hand is holding the candy.</summary>
        public const int STATE_HAND_CANDY = 1;

        /// <summary>State value used while the hand is releasing the candy.</summary>
        public const int STATE_HAND_RELEASE = 2;

        /// <summary>Current mechanical hand state.</summary>
        public int state;

        /// <summary>Whether attached candy visuals should rotate with segment movement.</summary>
        public bool doRotateCandy;

        /// <summary>Whether this hand is eligible to play a clap effect.</summary>
        public bool canPlayClap;

        /// <summary>Remaining clap cooldown time in seconds.</summary>
        public float clapTimer;

        /// <summary>Whether the release sound has already played for the current release.</summary>
        public bool releaseSoundPlayed;

        /// <summary>Offset from the terminal joint to the candy anchor in claw local space.</summary>
        private Vector clawOffset;

        /// <summary>Lightweight constrained point used to attach candy to the claw.</summary>
        public ConstraintedPoint cPoint;

        /// <summary>Ordered mechanical hand segment chain.</summary>
        public List<MechanicalHandSegment> segments;

        /// <summary>Segment currently being rotated by input.</summary>
        public MechanicalHandSegment rotatingSegment;
    }
}
