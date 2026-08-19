using System;

namespace CutTheRopeDX.Framework.Platform
{
    /// <summary>
    /// How much larger than its authored size design-space content is drawn at the current
    /// viewport. Pure and derived on read from the published snapshot, so every consumer - a
    /// controller fitting a menu, a scene placing its HUD, an element sizing itself - reads one
    /// value rather than each keeping a copy that has to be pushed to it and kept in step.
    /// </summary>
    /// <remarks>
    /// This is the tuning surface for the scale curve. Chrome that must stay physically reachable
    /// regardless of the curve is sized by <c>HudMetrics</c> instead; the two are deliberately
    /// separate, because one answers "how far from the design shape is this viewport" and the
    /// other "how small is this element allowed to get in the user's hand".
    /// </remarks>
    internal static class ContentFit
    {
        /// <summary>
        /// Uniform scale from design-box coordinates to logical space at the current viewport.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Exactly one at the design aspect ratio, and larger the further the viewport departs
        /// from it in either direction. Logical space normalizes the viewport's shorter side, so
        /// content held at one scale is sized for that side alone: on a shape the design was not
        /// drawn for, the long side gains room the composition never uses and everything reads as
        /// small for the screen it is on. Growing with the departure spends that room.
        /// </para>
        /// <para>
        /// The composition scales whole, so nothing crowds anything else at any of these; the
        /// authored spacing is preserved and simply drawn larger.
        /// </para>
        /// </remarks>
        public static float Scale => ScaleForAspect(ScreenPresentation.Instance.Snapshot.Aspect);

        /// <summary>
        /// The arithmetic behind <see cref="Scale"/>, with the aspect ratio passed in rather than
        /// read from the published snapshot, so the curve can be exercised at any shape.
        /// </summary>
        /// <param name="aspect">Width-to-height ratio of the region the game draws into.</param>
        /// <returns>The uniform scale to draw design-space content at.</returns>
        public static float ScaleForAspect(float aspect)
        {
            if (aspect >= DesignAspect)
            {
                return LayoutMath.Remap(
                    MathF.Min(aspect, ViewportLayout.MaxAspect),
                    DesignAspect,
                    ViewportLayout.MaxAspect,
                    1f,
                    WidestContentScale);
            }

            // Eased rather than linear: a square or 4:3 window is only mildly off the design
            // shape and should barely grow, while true phone-portrait aspects near MinAspect
            // are what the extra room is actually for. Cubing the linear departure keeps the
            // curve flat near the design aspect and steep only near the floor.
            float clampedAspect = MathF.Max(aspect, ViewportLayout.MinAspect);
            float departure = (DesignAspect - clampedAspect) / (DesignAspect - ViewportLayout.MinAspect);
            float eased = departure * departure * departure;
            return 1f + (eased * (NarrowestContentScale - 1f));
        }

        /// <summary>
        /// Aspect ratio of the fixed design size, where the content scale is exactly one.
        /// </summary>
        public const float DesignAspect =
            ViewportLayout.DesignWidth / ViewportLayout.DesignHeight;

        /// <summary>Content scale at the widest supported aspect ratio.</summary>
        private const float WidestContentScale = 1.15f;

        /// <summary>Content scale at the narrowest supported aspect ratio.</summary>
        private const float NarrowestContentScale = 1.55f;
    }
}
