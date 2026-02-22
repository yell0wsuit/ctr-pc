namespace CutTheRope.Framework.Visual
{
    internal abstract class FontGeneric : FrameworkTypes
    {
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

        public abstract void SetCharOffsetLineOffsetSpaceWidth(float co, float lo, float sw);

        public abstract float FontHeight();

        public abstract bool CanDraw(char c);

        public abstract float GetCharWidth(char c);

        public abstract int GetCharmapIndex(char c);

        public abstract int GetCharQuad(char c);

        public abstract float GetCharOffset(char[] s, int c, int len);

        public virtual float GetLineOffset()
        {
            return lineOffset;
        }

        public virtual float GetTopSpacing()
        {
            return topSpacing;
        }

        public virtual void NotifyTextCreated(Text st)
        {
        }

        public virtual void NotifyTextChanged(Text st)
        {
        }

        public virtual void NotifyTextDeleted(Text st)
        {
        }

        public abstract int TotalCharmaps();

        public abstract Image GetCharmap(int i);

        protected float charOffset;

        protected float lineOffset;

        protected float spaceWidth;

        protected float topSpacing;
    }
}
