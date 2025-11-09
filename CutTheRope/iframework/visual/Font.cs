using CutTheRope.ios;
using System;

namespace CutTheRope.iframework.visual
{
    internal class Font : FontGeneric
    {
        public virtual Font initWithVariableSizeCharscharMapFileKerning(NSString strParam, CTRTexture2D charmapfile, object k)
        {
            if (base.init() != null)
            {
                this._isWvga = charmapfile.isWvga();
                this.charmap = new Image().initWithTexture(charmapfile);
                this.quadsCount = charmapfile.quadsCount;
                this.height = charmapfile.quadRects[0].h;
                this.chars = strParam.copy();
                this.sortedChars = this.chars.getCharacters();
                Array.Sort<char>(this.sortedChars);
                this.charOffset = 0f;
                this.lineOffset = 0f;
            }
            return this;
        }

        public override void dealloc()
        {
            this.chars = null;
            this.sortedChars = null;
            this.charmap = null;
            base.dealloc();
        }

        public override void setCharOffsetLineOffsetSpaceWidth(float co, float lo, float sw)
        {
            this.charOffset = co;
            this.lineOffset = lo;
            this.spaceWidth = sw;
            if (this._isWvga)
            {
                this.charOffset = (float)(int)((double)this.charOffset / 1.5);
                this.lineOffset = (float)(int)((double)this.lineOffset / 1.5);
                this.spaceWidth = (float)(int)((double)this.spaceWidth / 1.5);
            }
        }

        public override float fontHeight()
        {
            return this.height;
        }

        public override bool canDraw(char c)
        {
            return c == ' ' || Array.BinarySearch<char>(this.sortedChars, c) >= 0;
        }

        public override float getCharWidth(char c)
        {
            if (c == ' ')
            {
                return this.spaceWidth;
            }
            if (c == '*')
            {
                return 0f;
            }
            return this.charmap.texture.quadRects[this.getCharQuad(c)].w;
        }

        public override int getCharmapIndex(char c)
        {
            return 0;
        }

        public override int getCharQuad(char c)
        {
            int num = this.chars.IndexOf(c);
            if (num >= 0)
            {
                return num;
            }
            return -1;
        }

        public override float getCharOffset(char[] s, int c, int len)
        {
            if (c == len - 1)
            {
                return 0f;
            }
            return this.charOffset;
        }

        public override int totalCharmaps()
        {
            return 1;
        }

        public override Image getCharmap(int i)
        {
            return this.charmap;
        }

        private NSString chars;

        private char[] sortedChars;

        private bool _isWvga;

        private int quadsCount;

        private float height;

        private Image charmap;
    }
}
