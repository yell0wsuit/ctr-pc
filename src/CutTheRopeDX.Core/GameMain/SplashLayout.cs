using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Platform;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// How the startup splash divides a viewport: where the animation's stage lands, and the size,
    /// wrap width and place of the legal disclaimer under it.
    /// </summary>
    /// <remarks>
    /// Pure, and derived from the viewport and the stage's own size. The disclaimer belongs to the
    /// splash rather than to a menu's design box, so it grows with the stage it sits under rather
    /// than with the menus' content scale: on a phone the stage is drawn half again as large as it
    /// is on the design shape, and text held at its authored size under it reads as an afterthought.
    /// </remarks>
    /// <param name="Stage">Where the animation's stage lands, contained in the viewport.</param>
    /// <param name="DisclaimerScale">Uniform scale the disclaimer is drawn at.</param>
    /// <param name="DisclaimerWrapWidth">Width the disclaimer wraps within, before that scale.</param>
    /// <param name="DisclaimerBottom">Where the bottom of the disclaimer belongs.</param>
    internal readonly record struct SplashLayout(
        CTRRectangle Stage,
        float DisclaimerScale,
        float DisclaimerWrapWidth,
        float DisclaimerBottom)
    {
        /// <summary>
        /// Divides a viewport between the splash stage and the disclaimer under it.
        /// </summary>
        /// <param name="visible">The logical region the viewport exposes.</param>
        /// <param name="stageWidth">Width of the animation's own stage.</param>
        /// <param name="stageHeight">Height of the animation's own stage.</param>
        /// <returns>The layout for that viewport.</returns>
        public static SplashLayout For(CTRRectangle visible, float stageWidth, float stageHeight)
        {
            // Contained rather than covered, because a splash that overflows is a splash with its
            // logo cropped.
            CTRRectangle stage = LayoutMath.FitInside(stageWidth, stageHeight, visible);

            // How much taller the stage is drawn than on the design shape, which is one wherever
            // the fit is driven by the viewport's height - every landscape shape - and larger on
            // the phone shapes, where it is the width that decides.
            float boost = stage.h / ViewportLayout.DesignHeight;

            return new SplashLayout(
                stage,
                AuthoredScale * boost,
                // Divided by the same boost the scale is multiplied by, so the column the text is
                // drawn in keeps its share of the screen and only the letters in it grow.
                visible.w * WidthShare / boost,
                stage.y + stage.h - (BottomInset * boost));
        }

        /// <summary>Authored scale of the disclaimer, relative to its font size.</summary>
        private const float AuthoredScale = 0.65f;

        /// <summary>Authored share of the visible width the disclaimer wraps within.</summary>
        private const float WidthShare = 0.9f;

        /// <summary>Authored distance from the bottom of the stage to the disclaimer.</summary>
        private const float BottomInset = 35f;
    }
}
