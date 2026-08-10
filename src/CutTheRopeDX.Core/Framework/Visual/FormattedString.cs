namespace CutTheRopeDX.Framework.Visual
{
    /// <summary>
    /// A string paired with a pre-measured pixel width for text layout.
    /// </summary>
    internal sealed class FormattedString : FrameworkTypes
    {
        /// <summary>
        /// Initializes the formatted string with text and its measured width.
        /// </summary>
        /// <param name="str">Text content.</param>
        /// <param name="w">Pre-measured pixel width of the text.</param>
        /// <returns>The initialized formatted string instance.</returns>
        public FormattedString InitWithStringAndWidth(string str, float w)
        {
            string_ = str;
            width = w;
            return this;
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                string_ = null;
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// The text content.
        /// </summary>
        public string string_;

        /// <summary>
        /// Pre-measured pixel width of the text.
        /// </summary>
        public float width;
    }
}
