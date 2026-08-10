using System.Collections.Generic;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Physics;
using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.GameMain
{
    /// <summary>The mutually exclusive states a mechanical hand can be in.</summary>
    internal enum MechanicalHandState
    {
        /// <summary>Waiting, and free to grab a candy in reach.</summary>
        Idle,

        /// <summary>Holding a candy in the claw.</summary>
        HoldingCandy,

        /// <summary>Let go, and waiting for the candy to clear the claw before going idle.</summary>
        Releasing
    }

    /// <summary>Outcome of a settle attempt on a releasing hand.</summary>
    internal enum HandSettle
    {
        /// <summary>The hand is not releasing, or the candy has not cleared the claw yet.</summary>
        Stayed,

        /// <summary>The hand went idle; the drop sound was already played by the release site.</summary>
        Settled,

        /// <summary>The hand went idle and the caller now owes the drop sound.</summary>
        SettledOwingDropSound
    }

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
            State = MechanicalHandState.Idle;
            cPoint = new ConstraintedPoint
            {
                disableGravity = true
            };
            cPoint.SetWeight(0.0001f);
            releaseSoundPlayed = false;
            clapTimer = 0f;
            CanPlayClap = false;

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
        /// <param name="initialScale">Base scale for the caught object visuals.</param>
        /// <param name="animationPool">Animation pool responsible for timeline lifecycle.</param>
        public void AnimateCatchWithCandyPartsandAnimationsPool(List<BaseElement> candyParts, float initialScale, AnimationsPool animationPool)
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

                Timeline candyTimeline = CatchBounceTimelineWithInitialScaleandAmplitude(initialScale, amplitude);
                candyTimeline.delegateTimelineDelegate = animationPool;
                int candyTimelineId = candyPart.AddTimeline(candyTimeline);
                candyPart.PlayTimeline(candyTimelineId);
            }
        }

        /// <summary>
        /// Takes hold of a candy. Rotation state is cleared so a hold never inherits anything from
        /// the previous one; the update loop re-derives it from the candy actually held.
        /// </summary>
        public void GrabCandy()
        {
            State = MechanicalHandState.HoldingCandy;
            DoRotateCandy = false;
            releaseSoundPlayed = false;
            graceTimer = MH_BOUNCER_GRACE;
        }

        /// <summary>
        /// Lets go of the candy. The drop sound is still owed and plays when the hand settles.
        /// </summary>
        public void ReleaseCandy()
        {
            BeginRelease(dropSoundPlayed: false);
        }

        /// <summary>
        /// Lets go of the candy when the caller has already played the drop sound itself.
        /// </summary>
        public void ReleaseCandyAfterDropSound()
        {
            BeginRelease(dropSoundPlayed: true);
        }

        /// <summary>
        /// Settles a releasing hand back to idle once the candy has cleared the claw.
        /// </summary>
        /// <param name="clawDistance">Distance from this hand to the candy it let go of.</param>
        /// <returns>Whether the hand settled, and whether the caller now owes the drop sound.</returns>
        public HandSettle TrySettleToIdle(float clawDistance)
        {
            if (State != MechanicalHandState.Releasing || clawDistance <= MH_RELEASE_DISTANCE)
            {
                return HandSettle.Stayed;
            }

            State = MechanicalHandState.Idle;
            bool owed = !releaseSoundPlayed;
            releaseSoundPlayed = false;
            return owed ? HandSettle.SettledOwingDropSound : HandSettle.Settled;
        }

        /// <summary>
        /// Marks this hand as eligible to clap. Never reset once set, matching the original.
        /// </summary>
        public void ArmClap()
        {
            CanPlayClap = true;
        }

        /// <summary>
        /// Arms the clap cooldown on both hands when they are idle and within clap range, and
        /// reports whether the pair should emit the clap effect.
        /// </summary>
        /// <param name="other">The other hand in the pair.</param>
        /// <returns><see langword="true"/> when the caller should play the clap effect.</returns>
        public bool TryClapWith(MechanicalHand other)
        {
            if (other == null
                || State != MechanicalHandState.Idle
                || other.State != MechanicalHandState.Idle
                || VectDistance(cPoint.pos, other.cPoint.pos) >= MH_CLAP_DISTANCE)
            {
                return false;
            }

            bool clap = (ClapTimer <= 0f || other.ClapTimer <= 0f) && (CanPlayClap || other.CanPlayClap);
            ClapTimer = MH_CLAP_COOLDOWN;
            other.ClapTimer = MH_CLAP_COOLDOWN;
            return clap;
        }

        /// <summary>Starts rotating the held candy along with the hand's moving segment.</summary>
        public void BeginCandyRotation()
        {
            DoRotateCandy = true;
        }

        private void BeginRelease(bool dropSoundPlayed)
        {
            State = MechanicalHandState.Releasing;
            DoRotateCandy = false;
            releaseSoundPlayed = dropSoundPlayed;
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
            _ = Mover.MoveVariableToTarget(ref graceTimer, 0f, 1f, delta);
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

        /// <summary>
        /// Grace period after a grab during which a bouncer may not strip the candy away. Without it
        /// a hand can never pick a candy up off a bouncer: the collision would take it back the frame
        /// after the catch, and the pair would fight each other. Unscaled, matching the original.
        /// </summary>
        public const float MH_BOUNCER_GRACE = 0.1f;

        /// <summary>Current mechanical hand state.</summary>
        public MechanicalHandState State { get; private set; }

        /// <summary>Whether the candy held by this hand should rotate with segment movement.</summary>
        public bool DoRotateCandy { get; private set; }

        /// <summary>Whether this hand is eligible to play a clap effect.</summary>
        public bool CanPlayClap { get; private set; }

        /// <summary>Remaining clap cooldown time in seconds.</summary>
        public float ClapTimer { get => clapTimer; private set => clapTimer = value; }

        /// <summary>Whether a bouncer is allowed to take this hand's candy yet.</summary>
        public bool CanBeDetachedByBouncer => graceTimer <= 0f;

        /// <summary>Backing store for <see cref="ClapTimer"/>, needed because it is moved by ref.</summary>
        private float clapTimer;

        /// <summary>Remaining post-grab bouncer grace time in seconds.</summary>
        private float graceTimer;

        /// <summary>Whether the release sound has already played for the current release.</summary>
        private bool releaseSoundPlayed;

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
