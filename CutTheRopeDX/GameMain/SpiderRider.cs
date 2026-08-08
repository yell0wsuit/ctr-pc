using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Visual;

using static CutTheRopeDX.Framework.Helpers.CTRMathHelper;

namespace CutTheRopeDX.GameMain
{
    /// <summary>Stage of a spider's journey down its hook's rope.</summary>
    internal enum SpiderRiderState
    {
        /// <summary>Waiting for a rope to exist.</summary>
        Dormant,

        /// <summary>A rope exists and the spider will start on the next update.</summary>
        Arming,

        /// <summary>Descending the rope toward the candy.</summary>
        Walking,

        /// <summary>Reached the candy and took it.</summary>
        Won,

        /// <summary>The rope was cut out from under it.</summary>
        Busted,
    }

    /// <summary>
    /// A spider riding a hook's rope. Replaces the three separate booleans plus a sentinel position
    /// that used to encode this, with the transitions owned here instead of half here and half in
    /// <c>GameScene.Update</c>.
    /// </summary>
    internal sealed class SpiderRider
    {
        /// <summary>Gets the spider's current stage.</summary>
        public SpiderRiderState State { get; private set; } = SpiderRiderState.Dormant;

        /// <summary>Gets whether the spider is descending the rope.</summary>
        public bool IsWalking => State == SpiderRiderState.Walking;

        /// <summary>
        /// Gets whether cutting this spider's rope should knock it off the hook. An armed spider has
        /// already committed to starting on the next update, so it must fall just like one that is
        /// already walking.
        /// </summary>
        public bool ShouldBustOnRopeCut => State is SpiderRiderState.Arming or SpiderRiderState.Walking;

        /// <summary>
        /// Gets whether the spider is still on its hook. A won or busted spider has left: it is no
        /// longer drawn or updated, which is what clearing the old <c>hasSpider</c> flag did.
        /// </summary>
        public bool IsAttached => State is SpiderRiderState.Dormant
            or SpiderRiderState.Arming
            or SpiderRiderState.Walking;

        /// <summary>
        /// Gets whether the spider has walked to the end of its rope and is waiting for the scene to
        /// confirm the rope ends on a candy it may take.
        /// </summary>
        public bool HasReachedCandy { get; private set; }

        /// <summary>Gets the distance travelled along the rope.</summary>
        public float Position { get; private set; }

        /// <summary>Gets or sets the spider animation.</summary>
        public Animation Animation { get; set; }

        /// <summary>
        /// Arms the spider when its hook takes a rope, or disarms it when the scene reports that the
        /// rope does not end on a candy a spider may take. Has no effect once the spider is walking
        /// or has left the hook.
        /// </summary>
        /// <param name="ropeAttachedToCandy">
        /// <see langword="true"/> when the hook's rope ends on a spider-grabbable candy.
        /// </param>
        public void Arm(bool ropeAttachedToCandy)
        {
            if (State is SpiderRiderState.Walking or SpiderRiderState.Won or SpiderRiderState.Busted)
            {
                return;
            }

            State = ropeAttachedToCandy ? SpiderRiderState.Arming : SpiderRiderState.Dormant;
        }

        /// <summary>Starts the descent.</summary>
        public void Activate()
        {
            if (State != SpiderRiderState.Arming)
            {
                return;
            }

            State = SpiderRiderState.Walking;
            CTRSoundMgr.PlaySound(Resources.Snd.SpiderActivate);
            Animation?.PlayTimeline(0);
        }

        /// <summary>Marks the spider as having reached the candy.</summary>
        public void Win()
        {
            State = SpiderRiderState.Won;
        }

        /// <summary>Stops the spider permanently after its rope was cut.</summary>
        public void Bust()
        {
            State = SpiderRiderState.Busted;
        }

        /// <summary>Advances the spider along its hook's rope.</summary>
        /// <param name="grab">The hook this spider rides.</param>
        /// <param name="delta">Elapsed time in seconds.</param>
        public void Update(Grab grab, float delta)
        {
            Activate();
            if (!IsWalking || Animation == null)
            {
                return;
            }

            if (Animation.GetCurrentTimelineIndex() != 0)
            {
                Position += delta * ActivePhysicsConstants.SpiderTraversalSpeed;
            }

            Bungee rope = grab.Rope;
            if (rope == null)
            {
                return;
            }

            float traversedLength = 0f;
            int i = 0;
            while (i < rope.drawPtsCount)
            {
                Vector segmentStart = Vect(rope.drawPts[i], rope.drawPts[i + 1]);
                Vector segmentEnd = Vect(rope.drawPts[i + 2], rope.drawPts[i + 3]);
                float segmentLength = MAX(
                    2f * Bungee.BUNGEE_REST_LEN / 3f,
                    VectDistance(segmentStart, segmentEnd));

                if (Position >= traversedLength
                    && (Position < traversedLength + segmentLength || i > rope.drawPtsCount - 3))
                {
                    float segmentProgress = Position - traversedLength;
                    Vector along = VectSub(segmentEnd, segmentStart);
                    Vector step = VectMult(along, segmentProgress / segmentLength);
                    Animation.x = segmentStart.X + step.X;
                    Animation.y = segmentStart.Y + step.Y;

                    if (i > rope.drawPtsCount - 3)
                    {
                        HasReachedCandy = true;
                    }

                    if (Animation.GetCurrentTimelineIndex() != 0)
                    {
                        Animation.rotation = RADIANS_TO_DEGREES(VectAngleNormalized(along)) + DEG_270;
                    }

                    return;
                }

                traversedLength += segmentLength;
                i += 2;
            }
        }
    }
}
