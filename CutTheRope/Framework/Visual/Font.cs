using System;

namespace CutTheRope.Framework.Visual
{
    /// <summary>
    /// A variable-width bitmap font backed by a single charmap texture atlas.
    /// </summary>
    internal sealed class Font : FontGeneric
    {
        /// <summary>
        /// Initializes the font from a character string and charmap texture.
        /// </summary>
        /// <param name="strParam">String of characters in the order they appear in the atlas.</param>
        /// <param name="charmapfile">Texture atlas containing character quads.</param>
        /// <returns>The initialized font instance.</returns>
        public Font InitWithVariableSizeCharscharMapFileKerning(string strParam, CTRTexture2D charmapfile)
        {
            _isWvga = charmapfile.IsWvga();
            charmap = new Image().InitWithTexture(charmapfile);
            // quadsCount = charmapfile.quadsCount;
            height = charmapfile.quadRects[0].h;
            chars = strParam;
            sortedChars = chars.ToCharArray();
            Array.Sort(sortedChars);
            charOffset = 0f;
            lineOffset = 0f;
            return this;
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                chars = null;
                sortedChars = null;
                charmap?.Dispose();
                charmap = null;
            }
            base.Dispose(disposing);
        }

        /// <inheritdoc />
        public override void SetCharOffsetLineOffsetSpaceWidth(float co, float lo, float sw)
        {
            charOffset = co;
            lineOffset = lo;
            spaceWidth = sw;
            if (_isWvga)
            {
                charOffset /= 1.5f;
                lineOffset /= 1.5f;
                spaceWidth /= 1.5f;
            }
        }

        /// <inheritdoc />
        public override float FontHeight()
        {
            return height;
        }

        /// <inheritdoc />
        public override bool CanDraw(char c)
        {
            return c == ' ' || Array.BinarySearch(sortedChars, c) >= 0;
        }

        /// <inheritdoc />
        public override float GetCharWidth(char c)
        {
            if (c == ' ')
            {
                return spaceWidth;
            }

            if (c == '*')
            {
                return 0f;
            }

            int quadIndex = GetCharQuad(c);
            if (quadIndex < 0)
            {
                return 0f; // Character not found in font, return 0 width
            }

            return charmap.texture.quadRects[quadIndex].w;
        }

        /// <inheritdoc />
        public override int GetCharmapIndex(char c)
        {
            return 0;
        }

        /// <inheritdoc />
        public override int GetCharQuad(char c)
        {
            int charIndex = chars.IndexOf(c);
            return charIndex >= 0 ? charIndex : -1;
        }

        /// <inheritdoc />
        public override float GetCharOffset(char[] s, int c, int len)
        {
            return c == len - 1 ? 0f : charOffset;
        }

        /// <inheritdoc />
        public override int TotalCharmaps()
        {
            return 1;
        }

        /// <inheritdoc />
        public override Image GetCharmap(int i)
        {
            return charmap;
        }

        /// <summary>
        /// String of characters in the order they appear in the atlas.
        /// </summary>
        private string chars;

        /// <summary>
        /// Sorted copy of <see cref="chars"/> for binary search in <see cref="CanDraw"/>.
        /// </summary>
        private char[] sortedChars;

        /// <summary>
        /// Whether the charmap texture uses WVGA scaling.
        /// </summary>
        private bool _isWvga;

        // private int quadsCount;

        /// <summary>
        /// Font height in pixels.
        /// </summary>
        private float height;

        /// <summary>
        /// Charmap image containing all character quads.
        /// </summary>
        private Image charmap;
    }
}
