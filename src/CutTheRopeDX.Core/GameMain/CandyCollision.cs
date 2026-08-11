using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Physics;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Pure candy-collision eligibility checks.
    /// </summary>
    internal static class CandyCollision
    {
        /// <summary>HTML nudge: X-axis reverse-velocity scale (engine constant).</summary>
        public const float HtmlNudgeScaleX = 62.5f;

        /// <summary>HTML nudge: Y-axis reverse-velocity scale (engine constant).</summary>
        public const float HtmlNudgeScaleY = 75f;

        /// <summary>
        /// HTML candy↔candy trigger: fraction of the candy's bounding-box WIDTH. The HTML engine
        /// uses the width of the candy bounds (112 wide), not the
        /// radius — so 0.9× of it is roughly the surface-touch distance (≈ the radius sum), not a
        /// near-center overlap.
        /// </summary>
        public const float HtmlTriggerWidthFactor = 0.9f;

        /// <summary>
        /// Whether a candy takes part in candy-to-candy collision: it needs a whole body in play and
        /// must not be held in a lantern.
        /// </summary>
        /// <param name="hasNoWholeBodyInPlay">Whether the candy's lifecycle leaves no whole body in play.</param>
        /// <param name="inLantern">Whether a lantern currently holds the candy.</param>
        /// <returns><see langword="true"/> when the candy can collide.</returns>
        public static bool ShouldParticipate(bool hasNoWholeBodyInPlay, bool inLantern)
        {
            return !hasNoWholeBodyInPlay && !inLantern;
        }

        /// <summary>
        /// Whether a candy-like body takes part in candy-to-candy collision: its lifecycle must leave
        /// a whole body in play, no lantern may hold it, and its capabilities must allow body collision
        /// (an axe, for instance, is a physical hazard that does not push candies around).
        /// </summary>
        public static bool ShouldParticipate(CandyContext ctx)
        {
            return ctx != null
                && ShouldParticipate(ctx.HasNoWholeBodyInPlay, ctx.Lifecycle.Attachments.InLantern)
                && ctx.Capabilities.CanCollideWithCandyBodies;
        }

        public static float PairDistance(CandyContext a, CandyContext b)
        {
            return a.collisionDistanceOverride.HasValue || b.collisionDistanceOverride.HasValue
                ? System.MathF.Max(a.collisionDistanceOverride ?? 0f, b.collisionDistanceOverride ?? 0f)
                : a.CollisionRadius + b.CollisionRadius;
        }

        /// <summary>
        /// Selects the HTML candy↔candy nudge model. Candy-like non-candy bodies keep the
        /// mobile overlap solver even when the level uses desktop physics tuning.
        /// </summary>
        public static bool ShouldUseHtmlModel(CandyContext a, CandyContext b, bool useMobilePhysicsModel)
        {
            return !useMobilePhysicsModel
                && a.Capabilities == CandyCapabilities.Candy
                && b.Capabilities == CandyCapabilities.Candy;
        }

        /// <summary>
        /// HTML-build candy↔candy trigger: fire only when the centers are within
        /// <see cref="HtmlTriggerWidthFactor"/> × the candy's bounding-box width AND still closing in (current distance below the previous frame's).
        /// </summary>
        public static bool ShouldHtmlNudge(float distance, float previousDistance, float candyBodyWidth)
        {
            return distance <= HtmlTriggerWidthFactor * candyBodyWidth && distance < previousDistance;
        }

        /// <summary>
        /// HTML-build candy↔candy nudge impulse for point <paramref name="a"/>.
        /// Each point contributes its reverse last-frame displacement (prevPos − pos) scaled by
        /// <see cref="HtmlNudgeScaleX"/>/<see cref="HtmlNudgeScaleY"/>; the impulse is their
        /// difference. Point <paramref name="b"/>'s impulse is the negation of this one.
        /// </summary>
        public static Vector HtmlNudgeImpulse(ConstraintedPoint a, ConstraintedPoint b)
        {
            float aReverseX = (a.prevPos.X - a.pos.X) * HtmlNudgeScaleX;
            float aReverseY = (a.prevPos.Y - a.pos.Y) * HtmlNudgeScaleY;
            float bReverseX = (b.prevPos.X - b.pos.X) * HtmlNudgeScaleX;
            float bReverseY = (b.prevPos.Y - b.pos.Y) * HtmlNudgeScaleY;
            return new Vector(aReverseX - bReverseX, aReverseY - bReverseY);
        }
    }
}
