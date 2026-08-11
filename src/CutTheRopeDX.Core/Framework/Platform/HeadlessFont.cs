using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;

namespace CutTheRopeDX.Framework.Platform
{
    /// <summary>
    /// Fixed-metric font for headless runs. Text is laid out but never drawn, so exact glyph
    /// metrics do not matter — only that layout code runs without a graphics device.
    /// </summary>
    internal sealed class HeadlessFont : FontGeneric
    {
        private const float CharWidth = 6f;
        private const float Height = 10f;

        private Image charmap;

        /// <inheritdoc />
        public override void SetCharOffsetLineOffsetSpaceWidth(float co, float lo, float sw)
        {
        }

        /// <inheritdoc />
        public override float FontHeight()
        {
            return Height;
        }

        /// <inheritdoc />
        public override bool CanDraw(char c)
        {
            return true;
        }

        /// <inheritdoc />
        public override float GetCharWidth(char c)
        {
            return CharWidth;
        }

        /// <inheritdoc />
        public override int GetCharmapIndex(char c)
        {
            return 0;
        }

        /// <inheritdoc />
        public override int GetCharQuad(char c)
        {
            return 0;
        }

        /// <inheritdoc />
        public override float GetCharOffset(char[] s, int c, int len)
        {
            return 0f;
        }

        /// <summary>
        /// Returns the number of charmaps. Must be at least 1:
        /// <see cref="Text.UpdateDrawerValues"/> sizes an array by this value and then
        /// indexes it, so zero throws.
        /// </summary>
        /// <returns>The number of charmaps.</returns>
        public override int TotalCharmaps()
        {
            return 1;
        }

        /// <inheritdoc />
        public override Image GetCharmap(int i)
        {
            // Any loaded atlas works; Text only needs a real Image to build its quad drawer.
            return charmap ??= Image.Image_createWithResIDQuad(Resources.Img.HudUi, 0);
        }
    }
}
