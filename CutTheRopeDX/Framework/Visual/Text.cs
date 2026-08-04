using System;
using System.Collections.Generic;
using System.Diagnostics;

using CutTheRopeDX.Desktop;
using CutTheRopeDX.Framework.Core;

using FontStashSharp;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRopeDX.Framework.Visual
{
    /// <summary>
    /// A <see cref="BaseElement"/> that renders formatted text using either sprite-based or FontStashSharp fonts.
    /// </summary>
    internal class Text : BaseElement
    {
        /// <summary>
        /// Rasterizer state with scissor test enabled for text clipping.
        /// </summary>
        private static readonly RasterizerState ScissorRasterizerState = new()
        {
            CullMode = CullMode.None,
            ScissorTestEnable = true
        };

        /// <summary>
        /// Creates a text element from a font resource name and string.
        /// </summary>
        /// <param name="fontResourceName">Font resource name.</param>
        /// <param name="str">Text to display.</param>
        /// <returns>A new text element initialized with the requested font and string.</returns>
        public static Text CreateWithFontandString(string fontResourceName, string str)
        {
            Text text = new Text().InitWithFont(Application.GetFont(fontResourceName));
            text.SetString(str);
            return text;
        }

        /// <summary>
        /// Initializes the text element with the specified font.
        /// </summary>
        /// <param name="i">Font to use for rendering.</param>
        /// <returns>The initialized text instance.</returns>
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

        /// <summary>
        /// Sets the display text, auto-wrapping to fit the font's measured width.
        /// </summary>
        /// <param name="newString">Text to display.</param>
        public virtual void SetString(string newString)
        {
            SetStringandWidth(newString, -1f);
        }

        /// <summary>
        /// Sets the display text with an explicit wrap width.
        /// </summary>
        /// <param name="newString">Text to display.</param>
        /// <param name="w">Wrap width in pixels, or -1 to auto-measure.</param>
        public virtual void SetStringandWidth(string newString, float w)
        {
            string_ = newString;
            string_ ??= new string("");
            font.NotifyTextChanged(this);
            if (w == -1f)
            {
                float widthPadding = 0.1f;
                wrapWidth = font.StringWidth(string_) + widthPadding;
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

        /// <summary>
        /// Rebuilds the multi-drawer quad data from the formatted strings (sprite font path).
        /// </summary>
        public virtual void UpdateDrawerValues()
        {
            multiDrawers.Clear();
            int totalCharmaps = font.TotalCharmaps();
            int textLength = string_.Length;
            char[] characters = string_.ToCharArray();
            int[] array = new int[totalCharmaps];
            for (int i = 0; i < textLength; i++)
            {
                if (characters[i] is not ' ' and not '*' and not '\n')
                {
                    array[font.GetCharmapIndex(characters[i])]++;
                }
            }
            for (int j = 0; j < totalCharmaps; j++)
            {
                int charCount = array[j];
                if (charCount > 0)
                {
                    ImageMultiDrawer item = new ImageMultiDrawer().InitWithImageandCapacity(font.GetCharmap(j), charCount);
                    multiDrawers.Add(item);
                }
            }
            float lineY = 0f;
            int fontHeight = (int)font.FontHeight();
            int renderedCharCount = 0;
            char[] characters2 = "..".ToCharArray();
            int dotSpacing = (int)font.GetCharOffset(characters2, 0, 2);
            int visibleLineCount = (int)(maxHeight == -1f ? formattedStrings.Count : MIN(formattedStrings.Count, maxHeight / (fontHeight + font.GetLineOffset())));
            bool isTruncated = visibleLineCount != formattedStrings.Count;
            int[] array2 = new int[totalCharmaps];
            for (int k = 0; k < visibleLineCount; k++)
            {
                FormattedString formattedString = formattedStrings[k];
                int lineLength = formattedString.string_.Length;
                char[] characters3 = formattedString.string_.ToCharArray();
                float lineX = align == 1 ? 0f : align != 2 ? wrapWidth - formattedString.width : (wrapWidth - formattedString.width) / 2f;
                for (int l = 0; l < lineLength; l++)
                {
                    if (characters3[l] != '*')
                    {
                        if (characters3[l] == ' ')
                        {
                            lineX += font.GetCharWidth(' ') + font.GetCharOffset(characters3, l, lineLength);
                        }
                        else
                        {
                            int charmapIndex = font.GetCharmapIndex(characters3[l]);
                            int charQuad = font.GetCharQuad(characters3[l]);

                            // Skip rendering if character is not in the font
                            if (charQuad >= 0)
                            {
                                ImageMultiDrawer imageMultiDrawer3 = multiDrawers[charmapIndex];
                                int quadIndex = charQuad;
                                float quadX = lineX;
                                float quadY = lineY;
                                int[] array3 = array2;
                                int mapIndex = charmapIndex;
                                int drawIndex = array3[mapIndex];
                                array3[mapIndex] = drawIndex + 1;
                                imageMultiDrawer3.MapTextureQuadAtXYatIndex(quadIndex, quadX, quadY, drawIndex);
                                renderedCharCount++;
                            }

                            lineX += font.GetCharWidth(characters3[l]) + font.GetCharOffset(characters3, l, lineLength);
                        }
                        if (isTruncated && k == visibleLineCount - 1)
                        {
                            int charmapIndex2 = font.GetCharmapIndex('.');
                            int charQuad2 = font.GetCharQuad('.');

                            // Only render ellipsis if '.' character is available
                            if (charQuad2 >= 0)
                            {
                                ImageMultiDrawer imageMultiDrawer2 = multiDrawers[charmapIndex2];
                                int dotWidth = (int)font.GetCharWidth('.');
                                if (l == lineLength - 1 || (l == lineLength - 2 && lineX + (3 * (dotWidth + dotSpacing)) + font.GetCharWidth(' ') > wrapWidth))
                                {
                                    imageMultiDrawer2.MapTextureQuadAtXYatIndex(charQuad2, lineX, lineY, renderedCharCount++);
                                    lineX += dotWidth + dotSpacing;
                                    imageMultiDrawer2.MapTextureQuadAtXYatIndex(charQuad2, lineX, lineY, renderedCharCount++);
                                    lineX += dotWidth + dotSpacing;
                                    imageMultiDrawer2.MapTextureQuadAtXYatIndex(charQuad2, lineX, lineY, renderedCharCount++);
                                    break;
                                }
                            }
                        }
                    }
                }
                lineY += fontHeight + font.GetLineOffset();
            }
            stringLength = renderedCharCount;
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

        /// <summary>
        /// Returns the current display text.
        /// </summary>
        /// <returns>The currently assigned display text.</returns>
        public virtual string GetString()
        {
            return string_;
        }

        /// <summary>
        /// Sets the text alignment (1 = left, 2 = center, 3 = right).
        /// </summary>
        /// <param name="a">Alignment value.</param>
        public virtual void SetAlignment(int a)
        {
            align = a;
        }

        /// <inheritdoc />
        public override void Draw()
        {
            // Capture inherited color before we apply this element's own modulation in PreDraw
            Color inheritedColor = Renderer.GetCurrentColor();

            PreDraw();

            // Check if this is a FontStashSharp font
            if (font is FontStashFont fontStashFont && !string.IsNullOrEmpty(string_))
            {
                DrawFontStashText(fontStashFont, inheritedColor);
            }
            else if (stringLength > 0)
            {
                // Legacy sprite font rendering
                Renderer.Translate(drawX, drawY, 0f);
                int i = 0;
                int count = multiDrawers.Count;
                while (i < count)
                {
                    ImageMultiDrawer imageMultiDrawer = multiDrawers[i];
                    if (imageMultiDrawer != null)
                    {
                        imageMultiDrawer.DrawAllQuads();
                        imageMultiDrawer.Optimize(Renderer.GetLastVertices_PositionNormalTexture());
                    }
                    i++;
                }
                Renderer.Translate(0f - drawX, 0f - drawY, 0f);
            }

            PostDraw();
        }

        /// <summary>
        /// Cached render targets for compositing text layers (shadow, stroke, fill) at full
        /// opacity before applying the fade alpha uniformly. Consecutive text draws alternate
        /// targets so a target is not rewritten immediately after being sampled. Each is sized to the text
        /// that needs it rather than to the viewport, and only ever grows; see
        /// <see cref="AcquireCompositeTarget"/>.
        /// </summary>
        private static readonly RenderTarget2D[] s_textCompositeTargets = new RenderTarget2D[2];

        /// <summary>
        /// Index of the render target used by the previous composite text draw.
        /// </summary>
        private static int s_textCompositeTargetIndex = -1;

        /// <summary>
        /// Selects the other composite render target for the next text draw.
        /// </summary>
        /// <param name="currentIndex">The target index used by the previous draw.</param>
        /// <returns>The target index to use for the next draw.</returns>
        internal static int GetNextCompositeTargetIndex(int currentIndex)
        {
            return (currentIndex + 1) % s_textCompositeTargets.Length;
        }

        /// <summary>One laid-out line: what to draw and where, in logical coordinates.</summary>
        /// <param name="Line">The formatted line.</param>
        /// <param name="Position">Top-left corner the line is drawn from.</param>
        private readonly record struct LineLayout(FormattedString Line, Vector2 Position);

        /// <summary>
        /// Lines placed by the last <see cref="LayoutLines"/> call. Reused rather than rebuilt, because
        /// text draws on one thread and this path runs for every string on screen.
        /// </summary>
        private static readonly List<LineLayout> s_lineLayout = [];

        /// <summary>
        /// Places each formatted line, applying alignment, the ping-pong scroll offset and the height
        /// limit, into <see cref="s_lineLayout"/>.
        /// </summary>
        /// <param name="startY">Y coordinate of the first line, in logical coordinates.</param>
        /// <param name="lineHeight">Distance between line origins, in logical coordinates.</param>
        /// <param name="isPingPonging">Whether the element is scrolling its text horizontally.</param>
        /// <returns>Number of lines placed.</returns>
        /// <remarks>
        /// Separate from drawing so the composite path can measure the element before it binds and clears a
        /// target for it. Both passes read the same placements, so they cannot drift apart.
        /// </remarks>
        private int LayoutLines(float startY, int lineHeight, bool isPingPonging)
        {
            s_lineLayout.Clear();
            float yPos = startY;
            foreach (FormattedString formattedString in formattedStrings)
            {
                if (maxHeight != -1f && yPos >= drawY + maxHeight)
                {
                    break;
                }

                float xPos = drawX;

                if (align == 2) // Center
                {
                    xPos += (wrapWidth - formattedString.width) / 2f;
                }
                else if (align == 3) // Right
                {
                    xPos += wrapWidth - formattedString.width;
                }

                // When ping-ponging, left-align the text at the clip area's left edge and scroll
                if (isPingPonging)
                {
                    float clipLeft = HasParent ? parent.drawX + pingPongPadding : drawX;
                    xPos = clipLeft - pingPongOffset;
                }

                s_lineLayout.Add(new LineLayout(formattedString, new Vector2(xPos, yPos)));
                yPos += lineHeight;
            }
            return s_lineLayout.Count;
        }

        /// <summary>
        /// Returns the union of the placed line boxes, in logical coordinates.
        /// </summary>
        /// <param name="lineHeight">Height of one line, in logical coordinates.</param>
        /// <param name="left">Leftmost edge of any line.</param>
        /// <param name="top">Topmost edge of any line.</param>
        /// <param name="right">Rightmost edge of any line.</param>
        /// <param name="bottom">Bottommost edge of any line.</param>
        private static void MeasureLayout(int lineHeight, out float left, out float top, out float right, out float bottom)
        {
            left = float.MaxValue;
            top = float.MaxValue;
            right = float.MinValue;
            bottom = float.MinValue;
            foreach (LineLayout laidOutLine in s_lineLayout)
            {
                left = MathF.Min(left, laidOutLine.Position.X);
                top = MathF.Min(top, laidOutLine.Position.Y);
                right = MathF.Max(right, laidOutLine.Position.X + laidOutLine.Line.width);
                bottom = MathF.Max(bottom, laidOutLine.Position.Y + lineHeight);
            }
        }

        /// <summary>
        /// Returns a composite target big enough to hold <paramref name="region"/> at its origin.
        /// </summary>
        /// <param name="graphicsDevice">Device the target is created on.</param>
        /// <param name="region">Region of the viewport the text occupies, in viewport pixels.</param>
        /// <param name="viewport">Current viewport, which bounds how large a target can be needed.</param>
        /// <returns>A target at least as large as <paramref name="region"/> in both dimensions.</returns>
        /// <remarks>
        /// Sized to the text rather than to the viewport, because the clear that follows costs the size of
        /// the target, and a screenful of it per faded element is most of what this path spends.
        /// </remarks>
        private static RenderTarget2D AcquireCompositeTarget(GraphicsDevice graphicsDevice, Rectangle region, Viewport viewport)
        {
            int width = Math.Min(CompositeTargetExtent(region.Width), viewport.Width);
            int height = Math.Min(CompositeTargetExtent(region.Height), viewport.Height);

            s_textCompositeTargetIndex = GetNextCompositeTargetIndex(s_textCompositeTargetIndex);
            RenderTarget2D target = s_textCompositeTargets[s_textCompositeTargetIndex];
            if (target != null && !target.IsDisposed)
            {
                // A target left over from a larger window is rebuilt rather than kept. Growth is the only
                // thing that ever replaces one, and a target already wider than the viewport can no longer
                // be outgrown, so it would hold its old size for the rest of the session.
                bool fitsViewport = target.Width <= viewport.Width && target.Height <= viewport.Height;
                if (fitsViewport && target.Width >= width && target.Height >= height)
                {
                    return target;
                }
                if (fitsViewport)
                {
                    // Only ever grow. Sizing a slot to each element exactly would rebuild it whenever a
                    // screenful of labels of different widths took turns using it, and building targets
                    // every frame costs more than the oversized clear that keeping one buys.
                    width = Math.Max(width, target.Width);
                    height = Math.Max(height, target.Height);
                }
                target.Dispose();
            }
            target = new RenderTarget2D(graphicsDevice, width, height, false,
                SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            s_textCompositeTargets[s_textCompositeTargetIndex] = target;
            return target;
        }

        /// <summary>
        /// Rounds a required extent up to the granularity composite targets are allocated at.
        /// </summary>
        /// <param name="required">Extent the text needs, in pixels.</param>
        /// <returns>The extent to allocate, never smaller than <paramref name="required"/>.</returns>
        internal static int CompositeTargetExtent(int required)
        {
            const int granularity = 128;
            int rounded = (Math.Max(required, 1) + granularity - 1) / granularity * granularity;
            return rounded;
        }

        /// <summary>
        /// Returns the region of the composite target the finished text actually occupies, in target
        /// pixels, so the blit back to the scene can be bounded by it.
        /// </summary>
        /// <param name="left">Left edge of the union of drawn line boxes, in logical coordinates.</param>
        /// <param name="top">Top edge of the union of drawn line boxes, in logical coordinates.</param>
        /// <param name="right">Right edge of the union of drawn line boxes, in logical coordinates.</param>
        /// <param name="bottom">Bottom edge of the union of drawn line boxes, in logical coordinates.</param>
        /// <param name="effects">Stroke and shadow settings, which widen the region past the line boxes.</param>
        /// <param name="lineHeight">Line height in logical coordinates, used to size the safety margin.</param>
        /// <param name="transformMatrix">Model-view and viewport scale the text was drawn through.</param>
        /// <param name="targetBounds">Bounds of the composite target, which the result is clipped to.</param>
        /// <returns>The region to blit, or <see cref="Rectangle.Empty"/> when nothing was drawn.</returns>
        /// <remarks>
        /// This region is what the whole composite path is sized against: the target is allocated to hold
        /// it, cleared over it, and blitted back through it. Sized to the viewport instead, as it once was,
        /// every faded text element costs a full-resolution clear and a full-resolution alpha blend to
        /// change the few hundred pixels the text covers. On the software renderer those passes are the
        /// difference between menus running and menus crawling.
        /// </remarks>
        internal static Rectangle CompositeBlitRectangle(
            float left,
            float top,
            float right,
            float bottom,
            FontEffectSettings effects,
            int lineHeight,
            Matrix transformMatrix,
            Rectangle targetBounds)
        {
            if (right <= left || bottom <= top)
            {
                return Rectangle.Empty;
            }

            // Stroke and shadow are drawn as offset copies, so ink reaches past the line boxes.
            float strokePad = effects?.HasStroke == true ? effects.StrokeAmount : 0f;
            float shadowPad = effects?.HasShadow != true ? 0f : effects.HasStroke ? effects.StrokeAmount : 1f;
            float shadowX = effects?.HasShadow == true ? effects.ShadowOffsetX : 0f;
            float shadowY = effects?.HasShadow == true ? effects.ShadowOffsetY : 0f;

            // Half a line of slack on top of that absorbs what the line box does not describe: descenders,
            // side bearings, the antialiased edge. Being a little generous costs a few pixels of blit and
            // guards against clipping text, which the old whole-target blit could not do by construction.
            float slack = (MathF.Max(lineHeight, 0) * 0.5f) + MathF.Max(strokePad, shadowPad);
            left += MathF.Min(shadowX, 0f) - slack;
            top += MathF.Min(shadowY, 0f) - slack;
            right += MathF.Max(shadowX, 0f) + slack;
            bottom += MathF.Max(shadowY, 0f) + slack;

            // The model-view matrix can rotate and skew, so map all four corners, not just two.
            Vector2 topLeft = Vector2.Transform(new Vector2(left, top), transformMatrix);
            Vector2 topRight = Vector2.Transform(new Vector2(right, top), transformMatrix);
            Vector2 bottomLeft = Vector2.Transform(new Vector2(left, bottom), transformMatrix);
            Vector2 bottomRight = Vector2.Transform(new Vector2(right, bottom), transformMatrix);

            int x0 = (int)MathF.Floor(MathF.Min(MathF.Min(topLeft.X, topRight.X), MathF.Min(bottomLeft.X, bottomRight.X)));
            int y0 = (int)MathF.Floor(MathF.Min(MathF.Min(topLeft.Y, topRight.Y), MathF.Min(bottomLeft.Y, bottomRight.Y)));
            int x1 = (int)MathF.Ceiling(MathF.Max(MathF.Max(topLeft.X, topRight.X), MathF.Max(bottomLeft.X, bottomRight.X)));
            int y1 = (int)MathF.Ceiling(MathF.Max(MathF.Max(topLeft.Y, topRight.Y), MathF.Max(bottomLeft.Y, bottomRight.Y)));

            return Rectangle.Intersect(new Rectangle(x0, y0, x1 - x0, y1 - y0), targetBounds);
        }

        /// <summary>
        /// Renders text using FontStashSharp with stroke, shadow, and color modulation.
        /// When fading, all layers are first composited at full opacity onto a render target,
        /// then drawn to screen with the fade alpha so shadow/stroke/fill fade in sync.
        /// </summary>
        /// <param name="fontStashFont">FontStash-backed font used for glyph rendering.</param>
        /// <param name="parentColor">Inherited parent color modulation.</param>
        private void DrawFontStashText(FontStashFont fontStashFont, Color parentColor)
        {
            SpriteBatch spriteBatch = Renderer.GetSpriteBatch();
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

            FontEffectSettings effects = fontStashFont.GetEffectSettings();
            Color textColor = fontStashFont.GetColor();

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

            static Color MakeColor(Color baseColor, float redScale, float greenScale, float blueScale, float alphaScale)
            {
                byte finalAlpha = (byte)MathHelper.Clamp(baseColor.A / 255f * alphaScale * 255f, 0f, 255f);

                return Color.FromNonPremultiplied(
                    ScaleByte(baseColor.R, redScale),
                    ScaleByte(baseColor.G, greenScale),
                    ScaleByte(baseColor.B, blueScale),
                    finalAlpha
                );
            }

            float inheritedRed = MathHelper.Clamp(parentColor.R / 255f, 0f, 1f);
            float inheritedGreen = MathHelper.Clamp(parentColor.G / 255f, 0f, 1f);
            float inheritedBlue = MathHelper.Clamp(parentColor.B / 255f, 0f, 1f);
            float inheritedAlpha = MathHelper.Clamp(color.AlphaChannel * (parentColor.A / 255f), 0f, 1f);

            bool hasEffects = effects?.HasStroke == true || effects?.HasShadow == true;
            bool needsComposite = hasEffects && inheritedAlpha < 1f;

            // Build colors: when compositing via render target, draw layers at full opacity;
            // the fade alpha is applied once when blitting the RT to screen.
            float layerAlpha = needsComposite ? 1f : inheritedAlpha;

            Color finalColor = MakeColor(textColor, inheritedRed, inheritedGreen, inheritedBlue, layerAlpha);

            int lineHeight = (int)(internalFont.LineHeight + font.GetLineOffset());

            GraphicsDevice graphicsDevice = Global.GraphicsDevice;
            // Queued sprite quads must render before text draws above them or changes render targets.
            Renderer.FlushQuads();
            Viewport viewport = graphicsDevice.Viewport;

            float viewportScaleX = viewport.Width / SCREEN_WIDTH;
            float viewportScaleY = viewport.Height / SCREEN_HEIGHT;

            Matrix transformMatrix =
                Renderer.GetModelViewMatrix() *
                Matrix.CreateScale(viewportScaleX, viewportScaleY, 1f);

            // Ping-pong clipping: overflowing text is clipped to a scissor rect
            float pingPongOverflow = pingPongEnabled ? GetPingPongOverflow() : 0f;
            bool isPingPonging = pingPongOverflow > 0f;

            // Place every line before anything is bound. The composite path below has to size and clear its
            // target before the first glyph is drawn, and the size it needs is a property of the layout.
            if (LayoutLines(drawY + font.GetTopSpacing(), lineHeight, isPingPonging) == 0)
            {
                return;
            }

            Rectangle previousScissor = graphicsDevice.ScissorRectangle;
            Rectangle pingPongScissor = Rectangle.Empty;
            if (isPingPonging)
            {
                float clipW = EffectivePingPongClipWidth;
                float clipH = maxHeight > 0f ? maxHeight : height;
                // Clip to the parent element's bounds (e.g., button background image)
                float clipX = HasParent ? parent.drawX + pingPongPadding : drawX;
                float clipY = drawY;
                // Transform clip rect corners through the full transform matrix (model-view + viewport scale)
                Vector2 topLeft = Vector2.Transform(new Vector2(clipX, clipY), transformMatrix);
                Vector2 bottomRight = Vector2.Transform(new Vector2(clipX + clipW, clipY + clipH), transformMatrix);
                int sx = (int)topLeft.X;
                int sy = (int)topLeft.Y;
                int sw = (int)(bottomRight.X - topLeft.X);
                int sh = (int)(bottomRight.Y - topLeft.Y);
                pingPongScissor = new Rectangle(sx, sy, sw, sh);
            }

            // When fading multi-layer text, composite all layers at full opacity onto a
            // render target, then blit with the fade alpha so every layer fades uniformly.
            RenderTargetBinding[] previousTargets = null;
            RenderTarget2D textCompositeTarget = null;
            Rectangle compositeRect = Rectangle.Empty;
            Matrix drawTransform = transformMatrix;
            if (needsComposite)
            {
                MeasureLayout(lineHeight, out float left, out float top, out float right, out float bottom);
                compositeRect = CompositeBlitRectangle(
                    left, top, right, bottom,
                    effects,
                    lineHeight,
                    transformMatrix,
                    new Rectangle(0, 0, viewport.Width, viewport.Height));
                if (isPingPonging)
                {
                    // Glyphs outside the clip are never drawn, so the target need not cover them.
                    compositeRect = Rectangle.Intersect(compositeRect, pingPongScissor);
                }
                if (compositeRect.IsEmpty)
                {
                    return;
                }

                textCompositeTarget = AcquireCompositeTarget(graphicsDevice, compositeRect, viewport);
                previousTargets = graphicsDevice.GetRenderTargets();
                graphicsDevice.SetRenderTarget(textCompositeTarget);
                graphicsDevice.Clear(Color.Transparent);
                // The target's origin is the rectangle's top-left corner in viewport space, so everything
                // drawn into it shifts by that much.
                drawTransform = transformMatrix * Matrix.CreateTranslation(-compositeRect.X, -compositeRect.Y, 0f);
            }

            // Deferred, not immediate: a stroked and shadowed line is drawn seventeen times over, and
            // immediate mode turns every glyph of every one of those passes into its own draw call.
            // Deferred keeps submission order, which is what the layering depends on, and collapses the
            // whole element into one call per atlas page.
            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.LinearClamp,
                null,
                ScissorRasterizerState,
                null,
                drawTransform
            );

            // The scissor is device state in the bound target's coordinates, so compositing has to restate
            // it rather than inherit it: the scene's rectangle means something else inside a target that
            // covers a corner of the viewport, and an unclipped element still has to say so explicitly.
            if (needsComposite)
            {
                graphicsDevice.ScissorRectangle = isPingPonging
                    ? new Rectangle(
                        pingPongScissor.X - compositeRect.X,
                        pingPongScissor.Y - compositeRect.Y,
                        pingPongScissor.Width,
                        pingPongScissor.Height)
                    : new Rectangle(0, 0, textCompositeTarget.Width, textCompositeTarget.Height);
            }
            else if (isPingPonging)
            {
                graphicsDevice.ScissorRectangle = pingPongScissor;
            }

            // Render each laid-out line
            foreach (LineLayout laidOutLine in s_lineLayout)
            {
                FormattedString formattedString = laidOutLine.Line;
                Vector2 position = laidOutLine.Position;

                // Draw shadow if enabled
                if (effects?.HasShadow == true)
                {
                    Vector2 shadowBasePos = position + new Vector2(effects.ShadowOffsetX, effects.ShadowOffsetY);
                    int shadowStrokeAmount = effects.HasStroke ? effects.StrokeAmount : 1;
                    Color shadowColor = MakeColor(
                        effects.ShadowColor, inheritedRed, inheritedGreen, inheritedBlue, layerAlpha);

                    for (int x = -shadowStrokeAmount; x <= shadowStrokeAmount; x++)
                    {
                        for (int y = -shadowStrokeAmount; y <= shadowStrokeAmount; y++)
                        {
                            _ = internalFont.DrawText(
                                spriteBatch,
                                formattedString.string_,
                                shadowBasePos + new Vector2(x, y),
                                shadowColor
                            );
                        }
                    }
                }

                // Draw stroke if enabled
                if (effects?.HasStroke == true)
                {
                    Color strokeColor = MakeColor(
                        effects.StrokeColor, inheritedRed, inheritedGreen, inheritedBlue, layerAlpha);
                    int strokeAmount = effects.StrokeAmount;

                    for (int x = -strokeAmount; x <= strokeAmount; x++)
                    {
                        for (int y = -strokeAmount; y <= strokeAmount; y++)
                        {
                            if (x != 0 || y != 0)
                            {
                                _ = internalFont.DrawText(
                                    spriteBatch,
                                    formattedString.string_,
                                    position + new Vector2(x, y),
                                    strokeColor
                                );
                            }
                        }
                    }
                }

                // Draw main text
                _ = internalFont.DrawText(
                    spriteBatch,
                    formattedString.string_,
                    position,
                    finalColor
                );
            }

            spriteBatch.End();
            BlendParams.InvalidateDeviceCache();

            // Blit the composite render target to screen with the uniform fade alpha
            if (needsComposite)
            {
                if (previousTargets != null && previousTargets.Length > 0)
                {
                    graphicsDevice.SetRenderTargets(previousTargets);
                }
                else
                {
                    graphicsDevice.SetRenderTarget(null);
                }

                byte fadeByte = (byte)MathHelper.Clamp(inheritedAlpha * 255f, 0f, 255f);
                Color blitColor = new(fadeByte, fadeByte, fadeByte, fadeByte); // premultiplied tint

                spriteBatch.Begin(
                    SpriteSortMode.Deferred,
                    BlendState.AlphaBlend,
                    SamplerState.LinearClamp,
                    null,
                    null,
                    null,
                    null
                );
                // The target holds the text at its own origin, and goes back to the rectangle it was
                // measured for. The target can be larger than that rectangle, since its size is rounded up
                // to keep it reusable, so the source region is taken from the corner rather than the whole
                // surface.
                spriteBatch.Draw(
                    textCompositeTarget,
                    compositeRect,
                    new Rectangle(0, 0, compositeRect.Width, compositeRect.Height),
                    blitColor);
                spriteBatch.End();
                BlendParams.InvalidateDeviceCache();

                // SpriteBatch leaves its texture in slot zero. Mark it unbound so the next
                // composite pass cannot retain a sampled binding for a writable target.
                if (ReferenceEquals(graphicsDevice.Textures[0], textCompositeTarget))
                {
                    graphicsDevice.Textures[0] = null;
                }
            }

            // Put the scene's scissor back, once the scene is what is bound again. Restoring it any earlier
            // would set a rectangle in the scene's coordinates while the composite target was still bound.
            if (needsComposite || isPingPonging)
            {
                graphicsDevice.ScissorRectangle = previousScissor;
            }
        }

        /// <inheritdoc />
        public override void Update(float delta)
        {
            base.Update(delta);

            if (!pingPongEnabled)
            {
                return;
            }

            float overflow = GetPingPongOverflow();
            if (overflow <= 0f)
            {
                pingPongOffset = 0f;
                return;
            }

            // Wait for the initial delay before starting the scroll
            if (!pingPongStarted)
            {
                pingPongPauseTimer += delta;
                if (pingPongPauseTimer >= pingPongPauseDuration)
                {
                    pingPongStarted = true;
                    pingPongPauseTimer = 0f;
                }
                return;
            }

            if (pingPongPauseTimer > 0f)
            {
                pingPongPauseTimer -= delta;
                return;
            }

            pingPongOffset += pingPongSpeed * delta * pingPongDirection;

            if (pingPongOffset >= overflow)
            {
                pingPongOffset = overflow;
                pingPongDirection = -1;
                pingPongPauseTimer = pingPongPauseDuration;
            }
            else if (pingPongOffset <= 0f)
            {
                pingPongOffset = 0f;
                pingPongDirection = 1;
                pingPongPauseTimer = pingPongPauseDuration;
            }
        }

        /// <summary>
        /// Word-wraps the current string into <see cref="FormattedString"/> lines based on <see cref="wrapWidth"/>.
        /// </summary>
        public virtual void FormatText()
        {
            short[] array = new short[512];
            char[] characters = string_.ToCharArray();
            int textLength = string_.Length;
            int rangesLength = 0;
            int wordStart = 0;
            float wordWidth = 0f;
            int lineStart = 0;
            int lineEnd = 0;
            float lineWidth = 0f;
            int cursor = 0;
            while (cursor < textLength)
            {
                char c = characters[cursor++];
                if (c is ' ' or '\n' or '*')
                {
                    lineWidth += wordWidth;
                    lineEnd = cursor - 1;
                    wordWidth = 0f;
                    wordStart = cursor;
                    if (c == ' ')
                    {
                        wordStart--;
                        wordWidth = font.GetCharWidth(' ') + font.GetCharOffset(characters, cursor - 1, textLength);
                    }
                }
                else
                {
                    wordWidth += font.GetCharWidth(c) + font.GetCharOffset(characters, cursor - 1, textLength);
                }
                bool exceedsWrap = lineWidth + wordWidth > wrapWidth;
                if (wrapLongWords && exceedsWrap && lineEnd == lineStart)
                {
                    lineWidth += wordWidth;
                    lineEnd = cursor;
                    wordWidth = 0f;
                    wordStart = cursor;
                }
                if ((lineWidth + wordWidth > wrapWidth && lineEnd != lineStart) || c == '\n')
                {
                    array[rangesLength++] = (short)lineStart;
                    array[rangesLength++] = (short)lineEnd;
                    while (wordStart < textLength && characters[wordStart] == ' ')
                    {
                        wordStart++;
                        wordWidth -= font.GetCharWidth(' ');
                    }
                    lineStart = wordStart;
                    lineEnd = lineStart;
                    lineWidth = 0f;
                }
            }
            if (wordWidth != 0f)
            {
                array[rangesLength++] = (short)lineStart;
                array[rangesLength++] = (short)cursor;
            }
            int lineCount = rangesLength >> 1;
            formattedStrings.Clear();
            for (int i = 0; i < lineCount; i++)
            {
                int rangeStart = array[i << 1];
                int rangeEnd = array[(i << 1) + 1];
                int length = rangeEnd - rangeStart;
                string str = string_.Substring(rangeStart, length);
                float w = font.StringWidth(str);
                FormattedString item = new FormattedString().InitWithStringAndWidth(str, w);
                formattedStrings.Add(item);
            }
        }

        /// <inheritdoc />
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

        /// <summary>
        /// Text alignment (1 = left, 2 = center, 3 = right).
        /// </summary>
        public int align;

        /// <summary>
        /// The raw display string.
        /// </summary>
        public string string_;

        /// <summary>
        /// Number of rendered characters (sprite font path).
        /// </summary>
        public int stringLength;

        /// <summary>
        /// Font used for measuring and rendering.
        /// </summary>
        public FontGeneric font;

        /// <summary>
        /// Width at which text wraps to the next line.
        /// </summary>
        public float wrapWidth;

        /// <summary>
        /// Word-wrapped lines of text.
        /// </summary>
        private List<FormattedString> formattedStrings;

        /// <summary>
        /// Multi-drawers for sprite font character quads.
        /// </summary>
        private List<ImageMultiDrawer> multiDrawers;

        /// <summary>
        /// Maximum rendered height in pixels, or -1 for unlimited.
        /// </summary>
        public float maxHeight;

        /// <summary>
        /// Whether to break long words that exceed the wrap width.
        /// </summary>
        public bool wrapLongWords;

        /// <summary>
        /// Enables the ping-pong scrolling effect for text that overflows <see cref="pingPongClipWidth"/>.
        /// </summary>
        public bool pingPongEnabled;

        /// <summary>
        /// The visible width for ping-pong clipping. Text wider than this scrolls back and forth.
        /// Defaults to -1 (uses parent element's width, or <see cref="wrapWidth"/> if no parent).
        /// </summary>
        public float pingPongClipWidth = -1f;

        /// <summary>
        /// Horizontal padding on each side within the clip area for the ping-pong effect.
        /// </summary>
        public float pingPongPadding = 60f;

        /// <summary>
        /// Scroll speed in virtual pixels per second for the ping-pong effect.
        /// </summary>
        public float pingPongSpeed = 80f;

        /// <summary>
        /// Pause duration in seconds at each end of the ping-pong scroll.
        /// </summary>
        public float pingPongPauseDuration = 2.5f;

        /// <summary>
        /// Current horizontal scroll offset for the ping-pong effect.
        /// </summary>
        private float pingPongOffset;

        /// <summary>
        /// Current scroll direction: 1 = scrolling left (showing more of the right side), -1 = scrolling back.
        /// </summary>
        private int pingPongDirection = 1;

        /// <summary>
        /// Remaining pause time at the current scroll end.
        /// </summary>
        private float pingPongPauseTimer;

        /// <summary>
        /// Whether the initial delay has elapsed.
        /// </summary>
        private bool pingPongStarted;

        /// <summary>
        /// Returns the effective clip width for ping-pong, falling back to the parent's width or <see cref="wrapWidth"/>.
        /// </summary>
        private float EffectivePingPongClipWidth =>
            pingPongClipWidth > 0f ? pingPongClipWidth :
            HasParent ? parent.width - (pingPongPadding * 2f) : wrapWidth;

        /// <summary>
        /// Returns the overflow amount for the widest formatted line, or 0 if no overflow.
        /// </summary>
        private float GetPingPongOverflow()
        {
            float clipW = EffectivePingPongClipWidth;
            float maxW = 0f;
            if (formattedStrings != null)
            {
                foreach (FormattedString fs in formattedStrings)
                {
                    if (fs.width > maxW)
                    {
                        maxW = fs.width;
                    }
                }
            }
            return maxW > clipW ? maxW - clipW : 0f;
        }
    }
}
