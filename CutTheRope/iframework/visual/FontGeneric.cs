using CutTheRope.Helpers;

namespace CutTheRope.iframework.visual
{
    internal abstract class FontGeneric : FrameworkTypes
    {
        public virtual float StringWidth(string str)
        {
            float num = 0f;
            int num2 = str.Length();
            char[] characters = str.GetCharacters();
            float num3 = 0f;
            for (int i = 0; i < num2; i++)
            {
                num3 = GetCharOffset(characters, i, num2);
                num += GetCharWidth(characters[i]) + num3;
            }
            return num - num3;
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
    }
}
