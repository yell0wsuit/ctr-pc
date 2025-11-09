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
                _isWvga = charmapfile.isWvga();
                charmap = new Image().initWithTexture(charmapfile);
                quadsCount = charmapfile.quadsCount;
                height = charmapfile.quadRects[0].h;
                chars = strParam.copy();
                sortedChars = chars.getCharacters();
                Array.Sort(sortedChars);
                charOffset = 0f;
                lineOffset = 0f;
            }
            return this;
        }

        public override void dealloc()
        {
            chars = null;
            sortedChars = null;
            charmap = null;
            base.dealloc();
        }

        public override void setCharOffsetLineOffsetSpaceWidth(float co, float lo, float sw)
        {
            charOffset = co;
            lineOffset = lo;
            spaceWidth = sw;
            if (_isWvga)
            {
                charOffset = (int)(charOffset / 1.5);
                lineOffset = (int)(lineOffset / 1.5);
                spaceWidth = (int)(spaceWidth / 1.5);
            }
        }

        public override float fontHeight()
        {
            return height;
        }

        public override bool canDraw(char c)
        {
            return c == ' ' || Array.BinarySearch(sortedChars, c) >= 0;
        }

        public override float getCharWidth(char c)
        {
            if (c == ' ')
            {
                return spaceWidth;
            }
            if (c == '*')
            {
                return 0f;
            }
            return charmap.texture.quadRects[getCharQuad(c)].w;
        }

        public override int getCharmapIndex(char c)
        {
            return 0;
        }

        public override int getCharQuad(char c)
        {
            int num = chars.IndexOf(c);
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
            return charOffset;
        }

        public override int totalCharmaps()
        {
            return 1;
        }

        public override Image getCharmap(int i)
        {
            return charmap;
        }

        private NSString chars;

        private char[] sortedChars;

        private bool _isWvga;

        private int quadsCount;

        private float height;

        private Image charmap;
    }
}
