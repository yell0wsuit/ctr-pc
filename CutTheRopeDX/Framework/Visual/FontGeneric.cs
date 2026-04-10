namespace CutTheRopeDX.Framework.Visual
{
    /// <summary>
    /// Abstract base class for bitmap fonts, providing character measurement and charmap access.
    /// </summary>
    internal abstract class FontGeneric : FrameworkTypes
    {
        /// <summary>
        /// Computes the total pixel width of the given string.
        /// </summary>
        /// <param name="str">String to measure.</param>
        /// <returns>The total width of <paramref name="str"/> in pixels.</returns>
        public virtual float StringWidth(string str)
        {
            float totalWidth = 0f;
            int length = str.Length;
            char[] characters = str.ToCharArray();
            float spacing = 0f;
            for (int i = 0; i < length; i++)
            {
                spacing = GetCharOffset(characters, i, length);
                totalWidth += GetCharWidth(characters[i]) + spacing;
            }
            return totalWidth - spacing;
        }

        /// <summary>
        /// Sets the character spacing, line spacing, and space character width.
        /// </summary>
        /// <param name="co">Character offset (spacing between characters).</param>
        /// <param name="lo">Line offset (spacing between lines).</param>
        /// <param name="sw">Width of the space character.</param>
        public abstract void SetCharOffsetLineOffsetSpaceWidth(float co, float lo, float sw);

        /// <summary>
        /// Returns the font height in pixels.
        /// </summary>
        /// <returns>The line height of the font in pixels.</returns>
        public abstract float FontHeight();

        /// <summary>
        /// Returns <see langword="true"/> if the font can draw the specified character.
        /// </summary>
        /// <param name="c">Character to check.</param>
        /// <returns><see langword="true"/> if the font has a glyph for <paramref name="c"/>.</returns>
        public abstract bool CanDraw(char c);

        /// <summary>
        /// Returns the pixel width of the specified character.
        /// </summary>
        /// <param name="c">Character to measure.</param>
        /// <returns>The width of <paramref name="c"/> in pixels.</returns>
        public abstract float GetCharWidth(char c);

        /// <summary>
        /// Returns the charmap index for the specified character.
        /// </summary>
        /// <param name="c">Character to look up.</param>
        /// <returns>The index of the charmap containing <paramref name="c"/>.</returns>
        public abstract int GetCharmapIndex(char c);

        /// <summary>
        /// Returns the quad index for the specified character.
        /// </summary>
        /// <param name="c">Character to look up.</param>
        /// <returns>The quad index for <paramref name="c"/> within its charmap.</returns>
        public abstract int GetCharQuad(char c);

        /// <summary>
        /// Returns the character offset (spacing) for the character at position <paramref name="c"/> in the string.
        /// </summary>
        /// <param name="s">Character array of the string.</param>
        /// <param name="c">Index of the current character.</param>
        /// <param name="len">Total length of the string.</param>
        /// <returns>The spacing in pixels to apply after the character at index <paramref name="c"/>.</returns>
        public abstract float GetCharOffset(char[] s, int c, int len);

        /// <summary>
        /// Returns the line offset (spacing between lines).
        /// </summary>
        /// <returns>The vertical spacing between lines in pixels.</returns>
        public virtual float GetLineOffset()
        {
            return lineOffset;
        }

        /// <summary>
        /// Returns the top spacing above the first line.
        /// </summary>
        /// <returns>The extra spacing above the first line in pixels.</returns>
        public virtual float GetTopSpacing()
        {
            return topSpacing;
        }

        /// <summary>
        /// Called when a <see cref="Text"/> element is created with this font.
        /// </summary>
        /// <param name="st">Text element that was created.</param>
        public virtual void NotifyTextCreated(Text st)
        {
        }

        /// <summary>
        /// Called when a <see cref="Text"/> element using this font changes its content.
        /// </summary>
        /// <param name="st">Text element that changed.</param>
        public virtual void NotifyTextChanged(Text st)
        {
        }

        /// <summary>
        /// Called when a <see cref="Text"/> element using this font is deleted.
        /// </summary>
        /// <param name="st">Text element that was deleted.</param>
        public virtual void NotifyTextDeleted(Text st)
        {
        }

        /// <summary>
        /// Returns the total number of charmaps in this font.
        /// </summary>
        /// <returns>The number of charmap textures used by this font.</returns>
        public abstract int TotalCharmaps();

        /// <summary>
        /// Returns the charmap <see cref="Image"/> at the specified index.
        /// </summary>
        /// <param name="i">Charmap index.</param>
        /// <returns>The charmap <see cref="Image"/> at index <paramref name="i"/>.</returns>
        public abstract Image GetCharmap(int i);

        /// <summary>
        /// Spacing between characters in pixels.
        /// </summary>
        protected float charOffset;

        /// <summary>
        /// Spacing between lines in pixels.
        /// </summary>
        protected float lineOffset;

        /// <summary>
        /// Width of the space character in pixels.
        /// </summary>
        protected float spaceWidth;

        /// <summary>
        /// Extra spacing above the first line in pixels.
        /// </summary>
        protected float topSpacing;
    }
}
