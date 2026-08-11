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
        private readonly SKFont _font;
        private readonly SKPaint _fill;
        private readonly FontConfiguration _config;

        public SkiaFont(SKTypeface typeface, FontConfiguration config)
        {
            _config = config;

            using SKFont probe = new(typeface, 100f);
            SKFontMetrics metrics = probe.Metrics;
            float heightPer100 = metrics.Descent - metrics.Ascent;
            float emSize = heightPer100 > 0f
                ? config.Size * 100f / heightPer100
                : config.Size;

            _font = new SKFont(typeface, emSize);
            _fill = new SKPaint { IsAntialias = true };

            lineOffset = config.LineSpacing;
            topSpacing = config.TopSpacing;
            charOffset = 0f;
            spaceWidth = _font.MeasureText(" ");
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

        /// <inheritdoc />
        public override float FontHeight()
        {
            if (!IsAlive)
            {
                return _config.Size;
            }
            SKFontMetrics metrics = _font.Metrics;
            return metrics.Descent - metrics.Ascent;
        }

        /// <inheritdoc />
        public override bool CanDraw(char c)
        {
            return IsAlive && (c == ' ' || _font.ContainsGlyph(c));
        }

        /// <inheritdoc />
        public override float GetCharWidth(char c)
        {
            return !IsAlive ? 0f : c == ' ' ? spaceWidth : _font.MeasureText(c.ToString());
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
            SkiaTextRenderer.Draw(call, _font, _fill, _config, this);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                IsAlive = false;
                _font.Dispose();
                _fill.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>Draws a Core text call onto the active Skia render target.</summary>
    internal static class SkiaTextRenderer
    {
        public static void Draw(
            in TextDrawCall call,
            SKFont font,
            SKPaint fill,
            FontConfiguration config,
            FontGeneric metrics)
        {
            if (call.Lines is null || call.Lines.Count == 0
                || PlatformServices.Render is not SkiaRenderBackend renderer)
            {
                return;
            }

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
                using SKPaint layer = new()
                {
                    Color = new SKColor(255, 255, 255, ToByte(inheritedAlpha * 255f)),
                };
                _ = canvas.SaveLayer(layer);
            }

            float y = call.DrawY + metrics.GetTopSpacing();
            float baselineOffset = -font.Metrics.Ascent;
            int lineHeight = (int)(metrics.FontHeight() + metrics.GetLineOffset());
            SKColor textColor = Modulate(config.Color, inherited, layerAlpha);

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
                if (effects?.HasShadow == true)
                {
                    fill.Color = Modulate(effects.ShadowColor, inherited, layerAlpha);
                    canvas.DrawText(
                        line.string_,
                        x + effects.ShadowOffsetX,
                        baseline + effects.ShadowOffsetY,
                        SKTextAlign.Left,
                        font,
                        fill);
                }

                if (effects?.HasStroke == true)
                {
                    using SKPaint stroke = new()
                    {
                        IsAntialias = true,
                        Style = SKPaintStyle.Stroke,
                        StrokeWidth = effects.StrokeAmount * 2f,
                        StrokeJoin = SKStrokeJoin.Round,
                        Color = Modulate(effects.StrokeColor, inherited, layerAlpha),
                    };
                    canvas.DrawText(
                        line.string_, x, baseline, SKTextAlign.Left, font, stroke);
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
