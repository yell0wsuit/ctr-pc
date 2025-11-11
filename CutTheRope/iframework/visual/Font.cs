using CutTheRope.ios;
using System;

namespace CutTheRope.iframework.visual
{
    internal sealed class Font : FontGeneric
    {
        public Font InitWithVariableSizeCharscharMapFileKerning(NSString strParam, CTRTexture2D charmapfile, object k)
        {
            if (base.Init() != null)
            {
                _isWvga = charmapfile.IsWvga();
                charmap = new Image().InitWithTexture(charmapfile);
                quadsCount = charmapfile.quadsCount;
                height = charmapfile.quadRects[0].h;
                chars = strParam.Copy();
                sortedChars = chars.GetCharacters();
                Array.Sort(sortedChars);
                charOffset = 0f;
                lineOffset = 0f;
            }
            return this;
        }

        public override void Dealloc()
        {
            chars = null;
            sortedChars = null;
            charmap = null;
            base.Dealloc();
        }

        public override void SetCharOffsetLineOffsetSpaceWidth(float co, float lo, float sw)
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

        public override float FontHeight()
        {
            return height;
        }

        public override bool CanDraw(char c)
        {
            return c == ' ' || Array.BinarySearch(sortedChars, c) >= 0;
        }

        public override float GetCharWidth(char c)
        {
            return c == ' ' ? spaceWidth : c == '*' ? 0f : charmap.texture.quadRects[GetCharQuad(c)].w;
        }

        public override int GetCharmapIndex(char c)
        {
            return 0;
        }

        public override int GetCharQuad(char c)
        {
            int num = chars.IndexOf(c);
            return num >= 0 ? num : -1;
        }

        public override float GetCharOffset(char[] s, int c, int len)
        {
            return c == len - 1 ? 0f : charOffset;
        }

        public override int TotalCharmaps()
        {
            return 1;
        }

        public override Image GetCharmap(int i)
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
