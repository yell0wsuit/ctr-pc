using CutTheRope.ios;

namespace CutTheRope.iframework.visual
{
    internal abstract class FontGeneric : NSObject
    {
        public virtual float stringWidth(NSString str)
        {
            float num = 0f;
            int num2 = str.length();
            char[] characters = str.getCharacters();
            float num3 = 0f;
            for (int i = 0; i < num2; i++)
            {
                num3 = getCharOffset(characters, i, num2);
                num += getCharWidth(characters[i]) + num3;
            }
            return num - num3;
        }

        public abstract void setCharOffsetLineOffsetSpaceWidth(float co, float lo, float sw);

        public abstract float fontHeight();

        public abstract bool canDraw(char c);

        public abstract float getCharWidth(char c);

        public abstract int getCharmapIndex(char c);

        public abstract int getCharQuad(char c);

        public abstract float getCharOffset(char[] s, int c, int len);

        public virtual float getLineOffset()
        {
            return lineOffset;
        }

        public virtual void notifyTextCreated(Text st)
        {
        }

        public virtual void notifyTextChanged(Text st)
        {
        }

        public virtual void notifyTextDeleted(Text st)
        {
        }

        public abstract int totalCharmaps();

        public abstract Image getCharmap(int i);

        protected float charOffset;

        protected float lineOffset;

        protected float spaceWidth;
    }
}
