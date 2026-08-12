using System;
using System.Numerics;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;

using SkiaSharp;

namespace CutTheRopeDX.Browser
{
    /// <summary>A self-drawing font rendered directly by Skia.</summary>
    internal sealed class SkiaFont : FontGeneric
    {
        // Effects are a property of the font, so the objects expressing them belong to the font
        // too. Only their colors vary per draw, and a paint's color is a setter while an image
        // filter is immutable - so the paint is mutated in place and the filter is rebuilt only
        // when the shadow color actually changes, which for a given font it usually does not.
        private SKPaint _effectPaint;
        private SKPaint _layerPaint;
        private SKImageFilter _dropShadow;
        private SKColor _dropShadowColor;

        public SkiaFont(SKTypeface typeface, FontConfiguration config)
        {
            Config = config;

            using SKFont probe = new(typeface, 100f);
            SKFontMetrics metrics = probe.Metrics;
            float heightPer100 = metrics.Descent - metrics.Ascent;
            float emSize = heightPer100 > 0f
                ? config.Size * 100f / heightPer100
                : config.Size;

            Font = new SKFont(typeface, emSize);
            Fill = new SKPaint { IsAntialias = true };

            lineOffset = config.LineSpacing;
            topSpacing = config.TopSpacing;
            charOffset = 0f;
            spaceWidth = Font.MeasureText(" ");
        }

        /// <inheritdoc />
        public override bool DrawsOwnText => true;

        /// <summary>
        /// Whether the Skia handles behind this font are still open. Resource packs list fonts
        /// alongside images, so freeing a pack disposes the font while the font cache still holds
        /// it; the cache tests this before handing the instance out again.
        /// </summary>
        /// <remarks>
        /// Views outlive the fonts they were built with. Changing language frees the localization
        /// pack and clears the font cache, then leaves already-built views on screen until they
        /// are rebuilt, so their text elements go on measuring and drawing through a font whose
        /// Skia handles are gone. Every member that touches those handles checks this first and
        /// degrades instead, matching how the desktop backend treats a disposed font.
        /// </remarks>
        internal bool IsAlive { get; private set; } = true;

        /// <summary>The glyph source, sized to the configuration.</summary>
        internal SKFont Font { get; }

        /// <summary>The paint the fill pass draws with.</summary>
        internal SKPaint Fill { get; }

        /// <summary>The configuration this font was built from.</summary>
        internal FontConfiguration Config { get; }

        /// <inheritdoc />
        public override float FontHeight()
        {
            if (!IsAlive)
            {
                return Config.Size;
            }
            SKFontMetrics metrics = Font.Metrics;
            return metrics.Descent - metrics.Ascent;
        }

        /// <inheritdoc />
        public override bool CanDraw(char c)
        {
            return IsAlive && (c == ' ' || Font.ContainsGlyph(c));
        }

        /// <inheritdoc />
        public override float GetCharWidth(char c)
        {
            return !IsAlive ? 0f : c == ' ' ? spaceWidth : Font.MeasureText(c.ToString());
        }

        /// <inheritdoc />
        public override float GetCharOffset(char[] s, int c, int len)
        {
            return charOffset;
        }

        /// <inheritdoc />
        public override int GetCharmapIndex(char c)
        {
            return 0;
        }

        /// <inheritdoc />
        public override int GetCharQuad(char c)
        {
            return CanDraw(c) ? c : -1;
        }

        /// <inheritdoc />
        public override int TotalCharmaps()
        {
            return 0;
        }

        /// <inheritdoc />
        public override Image GetCharmap(int i)
        {
            return null;
        }

        /// <inheritdoc />
        public override void SetCharOffsetLineOffsetSpaceWidth(float co, float lo, float sw)
        {
            charOffset = co;
            lineOffset = lo;
            spaceWidth = sw;
        }

        /// <inheritdoc />
        public override void DrawText(in TextDrawCall call)
        {
            if (!IsAlive)
            {
                return;
            }
            SkiaTextRenderer.Draw(call, this);
        }

        /// <summary>
        /// Returns the paint for the stroke and shadow pass, or <see langword="null"/> when the
        /// font carries neither effect.
        /// </summary>
        /// <param name="color">The color the pass draws with, already modulated.</param>
        /// <param name="shadowColor">The shadow color, already modulated.</param>
        internal SKPaint EffectPaint(SKColor color, SKColor shadowColor)
        {
            FontEffectSettings effects = Config.Effects;
            bool hasStroke = effects?.HasStroke == true;
            if (!hasStroke && effects?.HasShadow != true)
            {
                return null;
            }

            // A centered stroke reaches half its width past the outline, so the width is the
            // dilation desktop shows doubled. Without a stroke there is nothing to hang the
            // shadow on, and the pass falls back to a shadow-only filter over the bare glyphs.
            _effectPaint ??= new SKPaint
            {
                IsAntialias = true,
                Style = hasStroke ? SKPaintStyle.StrokeAndFill : SKPaintStyle.Fill,
                StrokeJoin = SKStrokeJoin.Round,
                StrokeWidth = hasStroke ? effects.StrokeAmount * 3f : 0f,
            };
            _effectPaint.Color = color;

            if (effects.HasShadow && (_dropShadow is null || _dropShadowColor != shadowColor))
            {
                // Skia strokes and shadows glyphs itself, so neither effect needs the offset
                // redraws the desktop font performs -- FontStashSharp has no such primitives, and
                // dilating the glyphs by hand is only its way around that. A drop shadow is cast
                // from whatever the paint draws, so hanging it on the outline pass shadows the
                // outlined glyph, which is the shape desktop's kernel arrives at the long way.
                // Sigma stays at zero because the original shadow is hard-edged.
                SKImageFilter replaced = _dropShadow;
                _dropShadow = hasStroke
                    ? SKImageFilter.CreateDropShadow(
                        effects.ShadowOffsetX, effects.ShadowOffsetY, 0f, 0f, shadowColor)
                    : SKImageFilter.CreateDropShadowOnly(
                        effects.ShadowOffsetX, effects.ShadowOffsetY, 0f, 0f, shadowColor);
                _dropShadowColor = shadowColor;
                // The paint takes its own reference before the previous filter is released.
                _effectPaint.ImageFilter = _dropShadow;
                replaced?.Dispose();
            }

            return _effectPaint;
        }

        /// <summary>Returns the paint that composites an effected layer at a partial alpha.</summary>
        /// <param name="alpha">The layer alpha, from 0 to 1.</param>
        internal SKPaint LayerPaint(float alpha)
        {
            _layerPaint ??= new SKPaint();
            _layerPaint.Color = new SKColor(
                255, 255, 255, (byte)Math.Clamp(alpha * 255f, 0f, 255f));
            return _layerPaint;
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                IsAlive = false;
                Font.Dispose();
                Fill.Dispose();
                _effectPaint?.Dispose();
                _layerPaint?.Dispose();
                _dropShadow?.Dispose();
                _effectPaint = null;
                _layerPaint = null;
                _dropShadow = null;
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>Draws a Core text call onto the active Skia render target.</summary>
    internal static class SkiaTextRenderer
    {
        public static void Draw(in TextDrawCall call, SkiaFont metrics)
        {
            if (call.Lines is null || call.Lines.Count == 0
                || PlatformServices.Render is not SkiaRenderBackend renderer)
            {
                return;
            }

            SKFont font = metrics.Font;
            SKPaint fill = metrics.Fill;
            FontConfiguration config = metrics.Config;

            renderer.FlushQuads();
            SKCanvas canvas = renderer.Target;
            _ = canvas.Save();

            Matrix4x4 modelView = renderer.GetModelViewMatrix();
            SKMatrix matrix = new(
                modelView.M11, modelView.M21, modelView.M41,
                modelView.M12, modelView.M22, modelView.M42,
                modelView.M14, modelView.M24, modelView.M44);
            canvas.Concat(matrix);

            if (call.IsPingPonging)
            {
                canvas.ClipRect(SKRect.Create(
                    call.PingPongClipLeft,
                    call.DrawY,
                    call.PingPongClipWidth,
                    call.PingPongClipHeight));
            }

            Color inherited = call.InheritedColor.ToColor();
            float inheritedAlpha = Math.Clamp(
                call.ElementColor.AlphaChannel * inherited.A / 255f, 0f, 1f);
            FontEffectSettings effects = config.Effects;
            bool hasEffects = effects?.HasStroke == true || effects?.HasShadow == true;
            bool needsLayer = hasEffects && inheritedAlpha < 1f;
            float layerAlpha = needsLayer ? 1f : inheritedAlpha;

            if (needsLayer)
            {
                _ = canvas.SaveLayer(metrics.LayerPaint(inheritedAlpha));
            }

            float y = call.DrawY + metrics.GetTopSpacing();
            float baselineOffset = -font.Metrics.Ascent;
            int lineHeight = (int)(metrics.FontHeight() + metrics.GetLineOffset());
            SKColor textColor = Modulate(config.Color, inherited, layerAlpha);

            // Stroking alone would leave the glyph interior translucent, so the effect pass fills
            // as well and the fill pass then draws over it.
            SKPaint effectPaint = !hasEffects
                ? null
                : metrics.EffectPaint(
                    effects.HasStroke
                        ? Modulate(effects.StrokeColor, inherited, layerAlpha)
                        : textColor,
                    effects.HasShadow
                        ? Modulate(effects.ShadowColor, inherited, layerAlpha)
                        : default);

            foreach (FormattedString line in call.Lines)
            {
                if (call.MaxHeight != -1f && y >= call.DrawY + call.MaxHeight)
                {
                    break;
                }

                float x = call.DrawX;
                if (call.Align == 2)
                {
                    x += (call.WrapWidth - line.width) / 2f;
                }
                else if (call.Align == 3)
                {
                    x += call.WrapWidth - line.width;
                }

                if (call.IsPingPonging)
                {
                    x = call.PingPongClipLeft - call.PingPongOffset;
                }

                float baseline = y + baselineOffset;
                if (effectPaint is not null)
                {
                    canvas.DrawText(
                        line.string_, x, baseline, SKTextAlign.Left, font, effectPaint);
                }

                fill.Color = textColor;
                canvas.DrawText(
                    line.string_, x, baseline, SKTextAlign.Left, font, fill);
                y += lineHeight;
            }

            if (needsLayer)
            {
                canvas.Restore();
            }
            canvas.Restore();
        }

        private static SKColor Modulate(Color color, Color inherited, float alpha)
        {
            return new SKColor(
                Scale(color.R, inherited.R / 255f),
                Scale(color.G, inherited.G / 255f),
                Scale(color.B, inherited.B / 255f),
                ToByte(color.A / 255f * alpha * 255f));
        }

        private static byte Scale(byte channel, float factor)
        {
            return ToByte(channel * factor);
        }

        private static byte ToByte(float value)
        {
            return (byte)Math.Clamp(value, 0f, 255f);
        }
    }
}
