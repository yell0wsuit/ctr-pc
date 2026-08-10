using System;
using System.Collections.Generic;
using System.Diagnostics;

using CutTheRopeDX.Desktop;
using CutTheRopeDX.Framework.Platform;

using FontStashSharp;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRopeDX.Framework.Visual
{
    /// <summary>
    /// FontStashSharp-based font implementation that replaces sprite/texture-based fonts.
    /// </summary>
    internal sealed class FontStashFont : FontGeneric
    {
        /// <summary>
        /// The underlying FontStashSharp dynamic font.
        /// </summary>
        private DynamicSpriteFont font;

        /// <summary>
        /// Font size in pixels.
        /// </summary>
        private float fontSize;

        /// <summary>
        /// Text rendering color.
        /// </summary>
        private Color textColor;

        /// <summary>
        /// Stroke and shadow effect settings.
        /// </summary>
        private FontEffectSettings effectSettings;

        /// <summary>
        /// Cache for rendered character images.
        /// </summary>
        private readonly Dictionary<char, Image> charImageCache = [];

        /// <summary>
        /// Initializes the font with the specified dynamic font, <paramref name="size"/>, <paramref name="color"/>, and <paramref name="effects"/>.
        /// </summary>
        /// <param name="dynamicFont">FontStashSharp dynamic font instance.</param>
        /// <param name="size">Font size in pixels.</param>
        /// <param name="color">Text rendering color.</param>
        /// <param name="effects">Stroke and shadow effect settings.</param>
        /// <param name="lineSpacing">Extra spacing between lines.</param>
        /// <param name="topSpacing">Extra spacing above the first line.</param>
        /// <returns>The initialized <see cref="FontStashFont"/> instance.</returns>
        public FontStashFont InitWithFont(DynamicSpriteFont dynamicFont, float size, Color color, FontEffectSettings effects, float lineSpacing = 0f, float topSpacing = 0f)
        {
            font = dynamicFont ?? throw new ArgumentNullException(nameof(dynamicFont));
            fontSize = size;
            textColor = color;
            effectSettings = effects;

            // Set default values
            charOffset = 0f;
            lineOffset = lineSpacing;
            spaceWidth = MeasureCharWidth(' ');
            this.topSpacing = topSpacing;

            return this;
        }

        /// <summary>
        /// Sets the text rendering <paramref name="color"/>.
        /// </summary>
        /// <param name="color">New text color.</param>
        public void SetColor(Color color)
        {
            textColor = color;
        }

        /// <summary>
        /// Returns the current text rendering color.
        /// </summary>
        /// <returns>The color currently used for text rendering.</returns>
        public Color GetColor()
        {
            return textColor;
        }

        /// <summary>
        /// Returns the underlying FontStashSharp dynamic font, or <see langword="null"/> if disposed.
        /// </summary>
        /// <returns>The internal dynamic font instance, or <see langword="null"/>.</returns>
        public DynamicSpriteFont GetInternalFont()
        {
            return font;
        }

        /// <summary>
        /// Returns the current effect settings.
        /// </summary>
        /// <returns>The active stroke and shadow effect settings.</returns>
        public FontEffectSettings GetEffectSettings()
        {
            return effectSettings;
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Clear cached images
                foreach (Image cachedImage in charImageCache.Values)
                {
                    cachedImage?.Dispose();
                }
                charImageCache.Clear();

                font = null;
            }
            base.Dispose(disposing);
        }

        /// <inheritdoc />
        public override void SetCharOffsetLineOffsetSpaceWidth(float co, float lo, float sw)
        {
            charOffset = co;
            lineOffset = lo;
            spaceWidth = sw;
        }

        /// <inheritdoc />
        public override float FontHeight()
        {
            return font?.LineHeight ?? fontSize;
        }

        /// <inheritdoc />
        public override bool CanDraw(char c)
        {
            // FontStashSharp can draw most characters
            return font != null && !char.IsControl(c);
        }

        /// <inheritdoc />
        public override float GetCharWidth(char c)
        {
            return c == ' ' ? spaceWidth : c == '*' ? 0f : MeasureCharWidth(c);
        }

        /// <summary>
        /// Measures the pixel width of a single character using FontStashSharp.
        /// </summary>
        /// <param name="c">Character to measure.</param>
        /// <returns>The measured width in pixels.</returns>
        private float MeasureCharWidth(char c)
        {
            if (font == null)
            {
                return 0f;
            }

            string charStr = c.ToString();
            Vector2 size = font.MeasureString(charStr);
            return size.X;
        }

        /// <inheritdoc />
        public override int GetCharmapIndex(char c)
        {
            // FontStashSharp uses a single texture atlas, so always return 0
            return 0;
        }

        /// <inheritdoc />
        public override int GetCharQuad(char c)
        {
            // For FontStashSharp, we don't use quad-based rendering
            // Return the character code as an identifier
            return CanDraw(c) ? c : -1;
        }

        /// <inheritdoc />
        public override float GetCharOffset(char[] s, int c, int len)
        {
            return c == len - 1 ? 0f : charOffset;
        }

        /// <inheritdoc />
        public override int TotalCharmaps()
        {
            // FontStashSharp uses a single texture atlas
            return 1;
        }

        /// <inheritdoc />
        public override Image GetCharmap(int i)
        {
            // Return a placeholder image for compatibility
            // The actual rendering is done differently with FontStashSharp
            return null;
        }

        /// <inheritdoc />
        public override bool DrawsOwnText => true;

        /// <summary>
        /// Rasterizer state with scissor test enabled for text clipping.
        /// </summary>
        private static readonly RasterizerState ScissorRasterizerState = new()
        {
            CullMode = CullMode.None,
            ScissorTestEnable = true
        };

        /// <summary>
        /// Cached render targets for compositing text layers (shadow, stroke, fill) at full
        /// opacity before applying the fade alpha uniformly. Consecutive text draws alternate
        /// targets so a target is not rewritten immediately after being sampled.
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

        /// <summary>
        /// Converts a Core color to the XNA color the sprite/font APIs consume. Core owns the
        /// color type now; this Desktop-bound file converts at its own boundary.
        /// </summary>
        /// <param name="c">The Core color.</param>
        /// <returns>The equivalent XNA color.</returns>
        private static Color ToXnaColor(Core.Color c)
        {
            return new Color(c.R, c.G, c.B, c.A);
        }

        /// <summary>
        /// Renders text using FontStashSharp with stroke, shadow, and color modulation.
        /// When fading, all layers are first composited at full opacity onto a render target,
        /// then drawn to screen with the fade alpha so shadow/stroke/fill fade in sync.
        /// </summary>
        /// <param name="call">Layout, formatted lines, color modulation, and ping-pong clip state for the text element.</param>

        public override void DrawText(in TextDrawCall call)
        {
            Color parentColor = ToXnaColor(call.InheritedColor.ToColor());

            SpriteBatch spriteBatch = Global.SpriteBatch;
            if (spriteBatch == null)
            {
                Debug.WriteLine("FontStash: SpriteBatch is null");
                return;
            }

            DynamicSpriteFont internalFont = GetInternalFont();
            if (internalFont == null)
            {
                Debug.WriteLine("FontStash: Internal font is null");
                return;
            }

            if (call.Lines == null || call.Lines.Count == 0)
            {
                Debug.WriteLine("FontStash: No formatted strings for text");
                return;
            }

            FontEffectSettings effects = GetEffectSettings();
            Color textColor = GetColor();

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
            float inheritedAlpha = MathHelper.Clamp(call.ElementColor.AlphaChannel * (parentColor.A / 255f), 0f, 1f);

            bool hasEffects = effects?.HasStroke == true || effects?.HasShadow == true;
            bool needsComposite = hasEffects && inheritedAlpha < 1f;

            // Build colors: when compositing via render target, draw layers at full opacity;
            // the fade alpha is applied once when blitting the RT to screen.
            float layerAlpha = needsComposite ? 1f : inheritedAlpha;

            Color finalColor = MakeColor(textColor, inheritedRed, inheritedGreen, inheritedBlue, layerAlpha);

            float yPos = call.DrawY + GetTopSpacing();
            int lineHeight = (int)(internalFont.LineHeight + GetLineOffset());

            GraphicsDevice graphicsDevice = Global.GraphicsDevice;
            // Queued sprite quads must render before text draws above them or changes render targets.
            Renderer.FlushQuads();
            Viewport viewport = graphicsDevice.Viewport;

            float viewportScaleX = viewport.Width / SCREEN_WIDTH;
            float viewportScaleY = viewport.Height / SCREEN_HEIGHT;

            Matrix transformMatrix =
                Renderer.GetModelViewMatrix() *
                Matrix.CreateScale(viewportScaleX, viewportScaleY, 1f);

            // When fading multi-layer text, composite all layers at full opacity onto a
            // render target, then blit with the fade alpha so every layer fades uniformly.
            RenderTargetBinding[] previousTargets = null;
            RenderTarget2D textCompositeTarget = null;
            if (needsComposite)
            {
                int rtW = viewport.Width;
                int rtH = viewport.Height;
                s_textCompositeTargetIndex = GetNextCompositeTargetIndex(s_textCompositeTargetIndex);
                textCompositeTarget = s_textCompositeTargets[s_textCompositeTargetIndex];
                if (textCompositeTarget == null || textCompositeTarget.IsDisposed ||
                    textCompositeTarget.Width != rtW || textCompositeTarget.Height != rtH)
                {
                    textCompositeTarget?.Dispose();
                    textCompositeTarget = new RenderTarget2D(graphicsDevice, rtW, rtH, false,
                        SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
                    s_textCompositeTargets[s_textCompositeTargetIndex] = textCompositeTarget;
                }

                previousTargets = graphicsDevice.GetRenderTargets();
                graphicsDevice.SetRenderTarget(textCompositeTarget);
                graphicsDevice.Clear(Color.Transparent);
            }

            // Ping-pong clipping: set a scissor rect so overflowing text is clipped
            bool isPingPonging = call.IsPingPonging;
            Rectangle previousScissor = graphicsDevice.ScissorRectangle;

            spriteBatch.Begin(
                SpriteSortMode.Immediate,
                BlendState.AlphaBlend,
                SamplerState.LinearClamp,
                null,
                ScissorRasterizerState,
                null,
                transformMatrix
            );

            if (isPingPonging)
            {
                float clipW = call.PingPongClipWidth;
                float clipH = call.PingPongClipHeight;
                // Clip to the parent element's bounds (e.g., button background image)
                float clipX = call.PingPongClipLeft;
                float clipY = call.DrawY;
                // Transform clip rect corners through the full transform matrix (model-view + viewport scale)
                Vector2 topLeft = Vector2.Transform(new Vector2(clipX, clipY), transformMatrix);
                Vector2 bottomRight = Vector2.Transform(new Vector2(clipX + clipW, clipY + clipH), transformMatrix);
                int sx = (int)topLeft.X;
                int sy = (int)topLeft.Y;
                int sw = (int)(bottomRight.X - topLeft.X);
                int sh = (int)(bottomRight.Y - topLeft.Y);
                graphicsDevice.ScissorRectangle = new Rectangle(sx, sy, sw, sh);
            }

            // Render each formatted line
            foreach (FormattedString formattedString in call.Lines)
            {
                if (call.MaxHeight != -1f && yPos >= call.DrawY + call.MaxHeight)
                {
                    break;
                }

                float xPos = call.DrawX;

                if (call.Align == 2) // Center
                {
                    xPos += (call.WrapWidth - formattedString.width) / 2f;
                }
                else if (call.Align == 3) // Right
                {
                    xPos += call.WrapWidth - formattedString.width;
                }

                // When ping-ponging, left-align the text at the clip area's left edge and scroll
                if (isPingPonging)
                {
                    float clipLeft = call.PingPongClipLeft;
                    xPos = clipLeft - call.PingPongOffset;
                }

                Vector2 position = new(xPos, yPos);

                // Draw shadow if enabled
                if (effects?.HasShadow == true)
                {
                    Vector2 shadowBasePos = position + new Vector2(effects.ShadowOffsetX, effects.ShadowOffsetY);
                    int shadowStrokeAmount = effects.HasStroke ? effects.StrokeAmount : 1;
                    Color shadowColor = MakeColor(
                        ToXnaColor(effects.ShadowColor), inheritedRed, inheritedGreen, inheritedBlue, layerAlpha);

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
                        ToXnaColor(effects.StrokeColor), inheritedRed, inheritedGreen, inheritedBlue, layerAlpha);
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

                yPos += lineHeight;
            }

            spriteBatch.End();
            BlendParams.InvalidateDeviceCache();

            if (isPingPonging)
            {
                graphicsDevice.ScissorRectangle = previousScissor;
            }

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
                    SpriteSortMode.Immediate,
                    BlendState.AlphaBlend,
                    SamplerState.LinearClamp,
                    null,
                    null,
                    null,
                    null
                );
                spriteBatch.Draw(textCompositeTarget, Vector2.Zero, blitColor);
                spriteBatch.End();
                BlendParams.InvalidateDeviceCache();

                // SpriteBatch leaves its texture in slot zero. Mark it unbound so the next
                // composite pass cannot retain a sampled binding for a writable target.
                if (ReferenceEquals(graphicsDevice.Textures[0], textCompositeTarget))
                {
                    graphicsDevice.Textures[0] = null;
                }
            }
        }
    }
}
