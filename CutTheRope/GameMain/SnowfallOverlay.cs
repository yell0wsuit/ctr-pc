using System;
using System.Collections.Generic;

using CutTheRope.Desktop;
using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;

using Microsoft.Xna.Framework;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Snow overlay ported from the HTML5 implementation. It renders animated
    /// snowflakes across the entire screen and fades them in/out when toggled.
    /// </summary>
    internal sealed class SnowfallOverlay : BaseElement
    {
        private static float BaseCanvasArea => SCREEN_WIDTH * SCREEN_HEIGHT;
        private const int MaxSnowflakes = 80;
        private const int MinSnowflakes = 30;
        private const float EdgeBuffer = 40f;
        private const float FallSpeedMin = 30f;
        private const float FallSpeedMax = 70f;
        private const float DriftSpeedMax = 15f;
        private const float SwingAmplitudeMin = 8f;
        private const float SwingAmplitudeMax = 22f;
        private const float SwingSpeedMin = 0.5f;
        private const float SwingSpeedMax = 1.2f;
        private const float TwinkleSpeedMin = 0.4f;
        private const float TwinkleSpeedMax = 1f;
        private const float FadeDuration = 0.6f;

        private readonly List<Snowflake> snowflakes = [];
        private CTRTexture2D texture;
        private bool running;
        private bool fadingOut;
        private float fadeElapsed;
        private float globalAlpha;
        private bool textureUnavailable;

        private SnowfallOverlay()
        {
            width = (int)SCREEN_WIDTH;
            height = (int)SCREEN_HEIGHT;
            touchable = false;
            updateable = SpecialEvents.IsXmas;
            visible = SpecialEvents.IsXmas;
            globalAlpha = 0f;
        }

        public static SnowfallOverlay CreateIfEnabled()
        {
            return SpecialEvents.IsXmas ? new SnowfallOverlay() : null;
        }

        public override void Update(float delta)
        {
            base.Update(delta);

            if (!running)
            {
                return;
            }

            UpdateSnowflakes(delta);
            UpdateFade(delta);
        }

        public override void Draw()
        {
            if (!running || texture == null || snowflakes.Count == 0)
            {
                return;
            }

            PreDraw();

            OpenGL.GlEnable(0);
            OpenGL.GlEnable(1);
            OpenGL.GlBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);

            Vector[] offsets = texture.quadOffsets;
            CTRRectangle[] rects = texture.quadRects;
            Vector preCut = texture.preCutSize;

            for (int i = 0; i < snowflakes.Count; i++)
            {
                Snowflake flake = snowflakes[i];
                CTRRectangle rect = rects[flake.FrameIndex];
                Vector offset = offsets[flake.FrameIndex];

                float safePreCutWidth = IsFinite(preCut.x) && preCut.x > 0 && preCut.x < 10000 ? preCut.x : rect.w;
                float safePreCutHeight = IsFinite(preCut.y) && preCut.y > 0 && preCut.y < 10000 ? preCut.y : rect.h;
                float scaledPreWidth = safePreCutWidth * flake.Scale;
                float scaledPreHeight = safePreCutHeight * flake.Scale;
                float scaledOffsetX = offset.x * flake.Scale;
                float scaledOffsetY = offset.y * flake.Scale;

                float swingOffset = (float)Math.Sin(flake.SwingPhase) * flake.SwingAmplitude;
                float currentX = flake.BaseX + swingOffset;
                float drawX = currentX - (scaledPreWidth / 2f) + scaledOffsetX;
                float drawY = flake.Y - (scaledPreHeight / 2f) + scaledOffsetY;

                float alpha = flake.AlphaBase + ((float)Math.Sin(flake.TwinklePhase) * flake.AlphaRange);
                float finalAlpha = Math.Clamp(alpha, 0f, 1f) * Math.Clamp(globalAlpha, 0f, 1f);
                if (finalAlpha <= 0f)
                {
                    continue;
                }
                RGBAColor final = new(1f, 1f, 1f, finalAlpha);

                OpenGL.GlColor4f(final.ToXNA());
                OpenGL.GlPushMatrix();
                OpenGL.GlTranslatef(drawX, drawY, 0f);
                OpenGL.GlScalef(flake.Scale, flake.Scale, 1f);
                CTRTexture2D.DrawQuadAtPoint(texture, flake.FrameIndex, vectZero);
                OpenGL.GlPopMatrix();
            }

            OpenGL.GlColor4f(Color.White);
            OpenGL.GlDisable(1);
            OpenGL.GlDisable(0);

            PostDraw();
        }

        public void Start()
        {
            if (running || textureUnavailable)
            {
                return;
            }

            if (!EnsureTexture())
            {
                return;
            }

            globalAlpha = 0f;
            fadeElapsed = 0f;
            fadingOut = false;

            PrepareSnowflakes();
            running = true;
        }

        public void Stop(bool immediate = false)
        {
            if (immediate)
            {
                running = false;
                fadingOut = false;
                fadeElapsed = 0f;
                globalAlpha = 0f;
                return;
            }

            if (running)
            {
                fadingOut = true;
                fadeElapsed = 0f;
            }
        }

        private bool EnsureTexture()
        {
            if (texture != null)
            {
                return true;
            }

            try
            {
                texture = Application.GetTexture(Resources.Img.Snowflakes);
                return texture != null;
            }
            catch (Exception)
            {
                textureUnavailable = true;
                return false;
            }
        }

        private void PrepareSnowflakes()
        {
            snowflakes.Clear();
            int count = ComputeSnowflakeCount();
            for (int i = 0; i < count; i++)
            {
                Snowflake flake = CreateSnowflake(populateScreen: true);
                flake.Y = -RND_0_1 * height;
                snowflakes.Add(flake);
            }
        }

        private static int ComputeSnowflakeCount()
        {
            float scaleRatio = SCREEN_WIDTH * SCREEN_HEIGHT / BaseCanvasArea;
            int scaled = (int)Math.Round(scaleRatio * MaxSnowflakes);
            return Math.Clamp(scaled, MinSnowflakes, MaxSnowflakes);
        }

        private Snowflake CreateSnowflake(bool populateScreen)
        {
            int frameCount = texture?.quadsCount ?? 0;
            int frameIndex = frameCount > 0 ? random_.Next(0, frameCount) : 0;

            float scale = ((float)random_.NextDouble() * 0.5f) + 0.5f;
            float speedY = RandomRange(FallSpeedMin, FallSpeedMax);
            float speedX = RandomRange(-DriftSpeedMax, DriftSpeedMax);
            float swingAmplitude = RandomRange(SwingAmplitudeMin, SwingAmplitudeMax);
            float swingSpeed = RandomRange(SwingSpeedMin, SwingSpeedMax);
            float alphaBase = ((float)random_.NextDouble() * 0.3f) + 0.5f;
            float alphaRange = ((float)random_.NextDouble() * 0.25f) + 0.15f;

            float xStart = populateScreen
                ? ((float)random_.NextDouble() * (width + (EdgeBuffer * 2f))) - EdgeBuffer
                : (float)random_.NextDouble() * width;

            return new Snowflake
            {
                FrameIndex = frameIndex,
                Scale = scale,
                SpeedY = speedY,
                SpeedX = speedX,
                SwingAmplitude = swingAmplitude,
                SwingSpeed = swingSpeed,
                SwingPhase = (float)(random_.NextDouble() * Math.PI * 2),
                AlphaBase = alphaBase,
                AlphaRange = alphaRange,
                TwinklePhase = (float)(random_.NextDouble() * Math.PI * 2),
                TwinkleSpeed = RandomRange(TwinkleSpeedMin, TwinkleSpeedMax),
                BaseX = xStart,
                Y = populateScreen ? -(float)random_.NextDouble() * height : -EdgeBuffer
            };
        }

        private void ResetSnowflake(ref Snowflake flake)
        {
            Snowflake replacement = CreateSnowflake(populateScreen: false);
            flake = replacement;
        }

        private void UpdateSnowflakes(float delta)
        {
            float maxY = height + EdgeBuffer;
            float maxX = width + EdgeBuffer;
            float minX = -EdgeBuffer;

            for (int i = 0; i < snowflakes.Count; i++)
            {
                Snowflake flake = snowflakes[i];
                flake.Y += flake.SpeedY * delta;
                flake.BaseX += flake.SpeedX * delta;
                flake.SwingPhase += flake.SwingSpeed * delta;
                flake.TwinklePhase += flake.TwinkleSpeed * delta;

                float swingOffset = (float)Math.Sin(flake.SwingPhase) * flake.SwingAmplitude;
                float currentX = flake.BaseX + swingOffset;

                if (flake.Y > maxY || currentX < minX || currentX > maxX)
                {
                    ResetSnowflake(ref flake);
                }

                snowflakes[i] = flake;
            }
        }

        private void UpdateFade(float delta)
        {
            if (fadingOut)
            {
                fadeElapsed += delta;
                float progress = Math.Clamp(fadeElapsed / FadeDuration, 0f, 1f);
                globalAlpha = Math.Max(0f, 1f - progress);
                if (progress >= 1f)
                {
                    Stop(immediate: true);
                }
            }
            else if (globalAlpha < 1f)
            {
                fadeElapsed += delta;
                float progress = Math.Clamp(fadeElapsed / FadeDuration, 0f, 1f);
                globalAlpha = Math.Min(1f, progress);
            }
        }

        private static float RandomRange(float min, float max)
        {
            return min + ((float)random_.NextDouble() * (max - min));
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static readonly Random random_ = new();

        private struct Snowflake
        {
            public int FrameIndex;
            public float Scale;
            public float SpeedY;
            public float SpeedX;
            public float SwingAmplitude;
            public float SwingSpeed;
            public float SwingPhase;
            public float AlphaBase;
            public float AlphaRange;
            public float TwinklePhase;
            public float TwinkleSpeed;
            public float BaseX;
            public float Y;
        }
    }
}
