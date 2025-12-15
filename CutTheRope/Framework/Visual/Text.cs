using System;
using System.Collections.Generic;
using System.Diagnostics;

using CutTheRope.Desktop;
using CutTheRope.Framework.Core;
using CutTheRope.Helpers;

using FontStashSharp;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.Framework.Visual
{
    internal class Text : BaseElement
    {
        private static readonly RasterizerState ScissorRasterizerState = new()
        {
            CullMode = CullMode.None,
            ScissorTestEnable = true
        };

        public static Text CreateWithFontandString(string fontResourceName, string str)
        {
            Text text = new Text().InitWithFont(Application.GetFont(fontResourceName));
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

                // Only update drawer values for sprite fonts, not FontStashSharp fonts
                if (font is not FontStashFont)
                {
                    UpdateDrawerValues();
                }
                else
                {
                    // Keep width/height in sync for anchoring and layout when using FontStashSharp
                    if (formattedStrings.Count <= 1)
                    {
                        height = (int)(font.FontHeight() + font.GetTopSpacing());
                        width = (int)wrapWidth;
                    }
                    else
                    {
                        height = (int)(((font.FontHeight() + font.GetLineOffset()) * formattedStrings.Count) - font.GetLineOffset() + font.GetTopSpacing());
                        width = (int)wrapWidth;
                    }

                    if (maxHeight != -1f)
                    {
                        height = (int)MIN(height, maxHeight);
                    }
                }
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

                            // Skip rendering if character is not in the font
                            if (charQuad >= 0)
                            {
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
                            }

                            num10 += font.GetCharWidth(characters3[l]) + font.GetCharOffset(characters3, l, num9);
                        }
                        if (flag && k == num8 - 1)
                        {
                            int charmapIndex2 = font.GetCharmapIndex('.');
                            int charQuad2 = font.GetCharQuad('.');

                            // Only render ellipsis if '.' character is available
                            if (charQuad2 >= 0)
                            {
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
                }
                num4 += num5 + font.GetLineOffset();
            }
            stringLength = num6;
            if (formattedStrings.Count <= 1)
            {
                height = (int)(font.FontHeight() + font.GetTopSpacing());
                width = (int)wrapWidth;
            }
            else
            {
                height = (int)(((font.FontHeight() + font.GetLineOffset()) * formattedStrings.Count) - font.GetLineOffset() + font.GetTopSpacing());
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
            // Capture inherited color before we apply this element's own modulation in PreDraw
            Color inheritedColor = OpenGL.GetCurrentColor();

            PreDraw();

            // Check if this is a FontStashSharp font
            if (font is FontStashFont fontStashFont && !string.IsNullOrEmpty(string_))
            {
                DrawFontStashText(fontStashFont, inheritedColor);
            }
            else if (stringLength > 0)
            {
                // Legacy sprite font rendering
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

        private void DrawFontStashText(FontStashFont fontStashFont, Color parentColor)
        {
            SpriteBatch spriteBatch = OpenGL.GetSpriteBatch();
            if (spriteBatch == null)
            {
                Debug.WriteLine("FontStash: SpriteBatch is null");
                return;
            }

            DynamicSpriteFont internalFont = fontStashFont.GetInternalFont();
            if (internalFont == null)
            {
                Debug.WriteLine("FontStash: Internal font is null");
                return;
            }

            if (formattedStrings == null || formattedStrings.Count == 0)
            {
                Debug.WriteLine($"FontStash: No formatted strings for text: {string_}");
                return;
            }

            //Debug.WriteLine($"FontStash: Drawing text '{string_}' at ({drawX}, {drawY}) with {formattedStrings.Count} lines");

            FontEffectSettings effects = fontStashFont.GetEffectSettings();
            Color textColor = fontStashFont.GetColor();
            static float CalculatePerPassAlpha(float targetAlpha, int sampleCount)
            {
                if (sampleCount <= 1)
                {
                    return MathHelper.Clamp(targetAlpha, 0f, 1f);
                }

                targetAlpha = MathHelper.Clamp(targetAlpha, 0f, 1f);
                if (targetAlpha <= 0f)
                {
                    return 0f;
                }
                if (targetAlpha >= 1f)
                {
                    return 1f;
                }

                // Normalize per-sample alpha so stacking multiple draws keeps overall opacity consistent
                float perSample = 1f - MathF.Pow(1f - targetAlpha, 1f / sampleCount);
                return MathHelper.Clamp(perSample, 0f, 1f);
            }

            // Apply element and inherited color modulation (RGBAColor uses 0-1 floats; textColor uses 0-255 bytes)
            static byte ScaleByte(byte channel, float factor)
            {
                float scaled = channel * factor; // factor already 0-1, so no /255
                if (scaled < 0f)
                {
                    scaled = 0f;
                }
                if (scaled > 255f)
                {
                    scaled = 255f;
                }
                return (byte)scaled;
            }

            static Color MakePremultipliedColor(Color baseColor, float redScale, float greenScale, float blueScale, float alphaScale)
            {
                byte finalAlpha = (byte)MathHelper.Clamp(baseColor.A / 255f * alphaScale * 255f, 0f, 255f);

                // Use FromNonPremultiplied so SpriteBatch receives premultiplied channels that honor the alpha timeline
                return Color.FromNonPremultiplied(
                    ScaleByte(baseColor.R, redScale),
                    ScaleByte(baseColor.G, greenScale),
                    ScaleByte(baseColor.B, blueScale),
                    finalAlpha
                );
            }

            // BaseElement color only modulates alpha (GL path uses ToWhiteAlphaXNA),
            // so keep RGB intact and apply timeline alpha once
            float inheritedRed = MathHelper.Clamp(parentColor.R / 255f, 0f, 1f);
            float inheritedGreen = MathHelper.Clamp(parentColor.G / 255f, 0f, 1f);
            float inheritedBlue = MathHelper.Clamp(parentColor.B / 255f, 0f, 1f);
            float inheritedAlpha = MathHelper.Clamp(color.a * (parentColor.A / 255f), 0f, 1f);

            // Premultiply channels for correct blending
            float effectiveAlpha = MathHelper.Clamp(textColor.A / 255f * inheritedAlpha, 0f, 1f);
            Color finalColor = MakePremultipliedColor(
                textColor,
                MathHelper.Clamp(inheritedRed, 0f, 1f),
                MathHelper.Clamp(inheritedGreen, 0f, 1f),
                MathHelper.Clamp(inheritedBlue, 0f, 1f),
                effectiveAlpha
            );

            float yPos = drawY + font.GetTopSpacing();
            int lineHeight = (int)(internalFont.LineHeight + font.GetLineOffset());

            // Calculate scale from virtual coordinates to physical viewport
            GraphicsDevice graphicsDevice = Global.GraphicsDevice;
            Viewport viewport = graphicsDevice.Viewport;

            float viewportScaleX = viewport.Width / SCREEN_WIDTH;
            float viewportScaleY = viewport.Height / SCREEN_HEIGHT;

            // Respect the current OpenGL emulation transform (including parent timelines/animations)
            Matrix transformMatrix =
                OpenGL.GetModelViewMatrix() *
                Matrix.CreateScale(viewportScaleX, viewportScaleY, 1f);

            // Begin SpriteBatch for text rendering with proper scaling
            spriteBatch.Begin(
                SpriteSortMode.Immediate,
                BlendState.AlphaBlend,
                SamplerState.LinearClamp,
                null,
                ScissorRasterizerState,
                null,
                transformMatrix
            );

            // Render each formatted line
            foreach (FormattedString formattedString in formattedStrings)
            {
                if (maxHeight != -1f && yPos >= drawY + maxHeight)
                {
                    break;
                }

                float xPos = drawX;

                // Calculate alignment offset
                if (align == 2) // Center
                {
                    xPos += (wrapWidth - formattedString.width) / 2f;
                }
                else if (align == 3) // Right
                {
                    xPos += wrapWidth - formattedString.width;
                }

                Vector2 position = new(xPos, yPos);

                // Draw shadow if enabled (with stroke for better backdrop effect)
                if (effects?.HasShadow == true)
                {
                    Vector2 shadowBasePos = position + new Vector2(effects.ShadowOffsetX, effects.ShadowOffsetY);
                    int shadowStrokeAmount = effects.HasStroke ? effects.StrokeAmount : 1;
                    int shadowSamples = ((shadowStrokeAmount * 2) + 1) * ((shadowStrokeAmount * 2) + 1);
                    float shadowTargetAlpha = effects.ShadowColor.A / 255f * inheritedAlpha;
                    float shadowAlpha = CalculatePerPassAlpha(shadowTargetAlpha, shadowSamples);
                    Color shadowColor = MakePremultipliedColor(
                        effects.ShadowColor,
                        MathHelper.Clamp(inheritedRed, 0f, 1f),
                        MathHelper.Clamp(inheritedGreen, 0f, 1f),
                        MathHelper.Clamp(inheritedBlue, 0f, 1f),
                        shadowAlpha
                    );

                    // Render shadow with stroke outline for better backdrop effect
                    for (int x = -shadowStrokeAmount; x <= shadowStrokeAmount; x++)
                    {
                        for (int y = -shadowStrokeAmount; y <= shadowStrokeAmount; y++)
                        {
                            Vector2 shadowPos = shadowBasePos + new Vector2(x, y);
                            _ = internalFont.DrawText(
                                spriteBatch,
                                formattedString.string_,
                                shadowPos,
                                shadowColor
                            );
                        }
                    }
                }

                // Draw stroke if enabled
                if (effects?.HasStroke == true)
                {
                    int strokeSamples = (((effects.StrokeAmount * 2) + 1) * ((effects.StrokeAmount * 2) + 1)) - 1;
                    strokeSamples = Math.Max(strokeSamples, 1);
                    float strokeTargetAlpha = effects.StrokeColor.A / 255f * inheritedAlpha;
                    float strokeAlpha = CalculatePerPassAlpha(strokeTargetAlpha, strokeSamples);
                    Color strokeColor = MakePremultipliedColor(
                        effects.StrokeColor,
                        MathHelper.Clamp(inheritedRed, 0f, 1f),
                        MathHelper.Clamp(inheritedGreen, 0f, 1f),
                        MathHelper.Clamp(inheritedBlue, 0f, 1f),
                        strokeAlpha
                    );
                    int strokeAmount = effects.StrokeAmount;

                    for (int x = -strokeAmount; x <= strokeAmount; x++)
                    {
                        for (int y = -strokeAmount; y <= strokeAmount; y++)
                        {
                            if (x != 0 || y != 0)
                            {
                                Vector2 strokePos = position + new Vector2(x, y);
                                // Use FontStashSharp's DrawText extension method
                                _ = internalFont.DrawText(
                                    spriteBatch,
                                    formattedString.string_,
                                    strokePos,
                                    strokeColor
                                );
                            }
                        }
                    }
                }

                // Draw main text using FontStashSharp's DrawText extension method
                _ = internalFont.DrawText(
                    spriteBatch,
                    formattedString.string_,
                    position,
                    finalColor
                );

                yPos += lineHeight;
            }

            // End SpriteBatch
            spriteBatch.End();
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
