using System.Collections.Generic;

using CutTheRope.desktop;
using CutTheRope.Helpers;
using CutTheRope.iframework.core;

namespace CutTheRope.iframework.visual
{
    internal class Text : BaseElement
    {
        public static Text CreateWithFontandString(int i, string str)
        {
            Text text = new Text().InitWithFont(Application.GetFont(i));
            text.SetString(str);
            return text;
        }

        public virtual Text InitWithFont(FontGeneric i)
        {
            font = i;
            formattedStrings = [];
            width = -1;
            height = -1;
            align = 1;
            multiDrawers = [];
            wrapLongWords = false;
            maxHeight = -1f;
            font.NotifyTextCreated(this);
            return this;
        }

        public virtual void SetString(string newString)
        {
            SetStringandWidth(newString, -1f);
        }

        public virtual void SetStringandWidth(string newString, double w)
        {
            SetStringandWidth(newString, (float)w);
        }

        public virtual void SetStringandWidth(string newString, float w)
        {
            string_ = newString;
            string_ ??= new string("");
            font.NotifyTextChanged(this);
            if (w == -1f)
            {
                float num = 0.1f;
                wrapWidth = font.StringWidth(string_) + num;
            }
            else
            {
                wrapWidth = w;
            }
            if (string_ != null)
            {
                FormatText();
                UpdateDrawerValues();
                return;
            }
            stringLength = 0;
        }

        public virtual void UpdateDrawerValues()
        {
            multiDrawers.Clear();
            int num = font.TotalCharmaps();
            int num2 = string_.Length();
            char[] characters = string_.GetCharacters();
            int[] array = new int[num];
            for (int i = 0; i < num2; i++)
            {
                if (characters[i] is not ' ' and not '*' and not '\n')
                {
                    array[font.GetCharmapIndex(characters[i])]++;
                }
            }
            for (int j = 0; j < num; j++)
            {
                int num3 = array[j];
                if (num3 > 0)
                {
                    ImageMultiDrawer item = new ImageMultiDrawer().InitWithImageandCapacity(font.GetCharmap(j), num3);
                    multiDrawers.Add(item);
                }
            }
            float num4 = 0f;
            int num5 = (int)font.FontHeight();
            int num6 = 0;
            char[] characters2 = "..".GetCharacters();
            int num7 = (int)font.GetCharOffset(characters2, 0, 2);
            int num8 = (int)(maxHeight == -1f ? formattedStrings.Count : MIN(formattedStrings.Count, maxHeight / (num5 + font.GetLineOffset())));
            bool flag = num8 != formattedStrings.Count;
            int[] array2 = new int[num];
            for (int k = 0; k < num8; k++)
            {
                FormattedString formattedString = formattedStrings[k];
                int num9 = formattedString.string_.Length();
                char[] characters3 = formattedString.string_.GetCharacters();
                float num10 = align == 1 ? 0f : align != 2 ? wrapWidth - formattedString.width : (wrapWidth - formattedString.width) / 2f;
                for (int l = 0; l < num9; l++)
                {
                    if (characters3[l] != '*')
                    {
                        if (characters3[l] == ' ')
                        {
                            num10 += font.GetCharWidth(' ') + font.GetCharOffset(characters3, l, num9);
                        }
                        else
                        {
                            int charmapIndex = font.GetCharmapIndex(characters3[l]);
                            int charQuad = font.GetCharQuad(characters3[l]);
                            ImageMultiDrawer imageMultiDrawer3 = multiDrawers[charmapIndex];
                            int num12 = charQuad;
                            float num13 = num10;
                            float num14 = num4;
                            int[] array3 = array2;
                            int num15 = charmapIndex;
                            int num16 = array3[num15];
                            array3[num15] = num16 + 1;
                            imageMultiDrawer3.MapTextureQuadAtXYatIndex(num12, num13, num14, num16);
                            num6++;
                            num10 += font.GetCharWidth(characters3[l]) + font.GetCharOffset(characters3, l, num9);
                        }
                        if (flag && k == num8 - 1)
                        {
                            int charmapIndex2 = font.GetCharmapIndex('.');
                            int charQuad2 = font.GetCharQuad('.');
                            ImageMultiDrawer imageMultiDrawer2 = multiDrawers[charmapIndex2];
                            int num11 = (int)font.GetCharWidth('.');
                            if (l == num9 - 1 || (l == num9 - 2 && num10 + (3 * (num11 + num7)) + font.GetCharWidth(' ') > wrapWidth))
                            {
                                imageMultiDrawer2.MapTextureQuadAtXYatIndex(charQuad2, num10, num4, num6++);
                                num10 += num11 + num7;
                                imageMultiDrawer2.MapTextureQuadAtXYatIndex(charQuad2, num10, num4, num6++);
                                num10 += num11 + num7;
                                imageMultiDrawer2.MapTextureQuadAtXYatIndex(charQuad2, num10, num4, num6++);
                                break;
                            }
                        }
                    }
                }
                num4 += num5 + font.GetLineOffset();
            }
            stringLength = num6;
            if (formattedStrings.Count <= 1)
            {
                height = (int)font.FontHeight();
                width = (int)wrapWidth;
            }
            else
            {
                height = (int)(((font.FontHeight() + font.GetLineOffset()) * formattedStrings.Count) - font.GetLineOffset());
                width = (int)wrapWidth;
            }
            if (maxHeight != -1f)
            {
                height = (int)MIN(height, maxHeight);
            }
        }

        public virtual string GetString()
        {
            return string_;
        }

        public virtual void SetAlignment(int a)
        {
            align = a;
        }

        public override void Draw()
        {
            PreDraw();
            if (stringLength > 0)
            {
                OpenGL.GlTranslatef(drawX, drawY, 0f);
                int i = 0;
                int count = multiDrawers.Count;
                while (i < count)
                {
                    ImageMultiDrawer imageMultiDrawer = multiDrawers[i];
                    if (imageMultiDrawer != null)
                    {
                        imageMultiDrawer.DrawAllQuads();
                        imageMultiDrawer.Optimize(OpenGL.GetLastVertices_PositionNormalTexture());
                    }
                    i++;
                }
                OpenGL.GlTranslatef(0f - drawX, 0f - drawY, 0f);
            }
            PostDraw();
        }

        public virtual void FormatText()
        {
            short[] array = new short[512];
            char[] characters = string_.GetCharacters();
            int num = string_.Length();
            int num2 = 0;
            int num3 = 0;
            float num4 = 0f;
            int num5 = 0;
            int num6 = 0;
            float num7 = 0f;
            int num8 = 0;
            while (num8 < num)
            {
                char c = characters[num8++];
                if (c is ' ' or '\n' or '*')
                {
                    num7 += num4;
                    num6 = num8 - 1;
                    num4 = 0f;
                    num3 = num8;
                    if (c == ' ')
                    {
                        num3--;
                        num4 = font.GetCharWidth(' ') + font.GetCharOffset(characters, num8 - 1, num);
                    }
                }
                else
                {
                    num4 += font.GetCharWidth(c) + font.GetCharOffset(characters, num8 - 1, num);
                }
                bool flag = num7 + num4 > wrapWidth;
                if (wrapLongWords && flag && num6 == num5)
                {
                    num7 += num4;
                    num6 = num8;
                    num4 = 0f;
                    num3 = num8;
                }
                if ((num7 + num4 > wrapWidth && num6 != num5) || c == '\n')
                {
                    array[num2++] = (short)num5;
                    array[num2++] = (short)num6;
                    while (num3 < num && characters[num3] == ' ')
                    {
                        num3++;
                        num4 -= font.GetCharWidth(' ');
                    }
                    num5 = num3;
                    num6 = num5;
                    num7 = 0f;
                }
            }
            if (num4 != 0f)
            {
                array[num2++] = (short)num5;
                array[num2++] = (short)num8;
            }
            int num9 = num2 >> 1;
            formattedStrings.Clear();
            for (int i = 0; i < num9; i++)
            {
                int num10 = array[i << 1];
                int num11 = array[(i << 1) + 1];
                int length = num11 - num10;
                string str = string_.Substring(num10, length);
                float w = font.StringWidth(str);
                FormattedString item = new FormattedString().InitWithStringAndWidth(str, w);
                formattedStrings.Add(item);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                font?.NotifyTextDeleted(this);
                string_ = null;
                font = null;
                formattedStrings = null;
                multiDrawers?.Clear();
                multiDrawers = null;
            }
            base.Dispose(disposing);
        }

        public int align;

        public string string_;

        public int stringLength;

        public FontGeneric font;

        public float wrapWidth;

        private List<FormattedString> formattedStrings;

        private List<ImageMultiDrawer> multiDrawers;

        public float maxHeight;

        public bool wrapLongWords;
    }
}
