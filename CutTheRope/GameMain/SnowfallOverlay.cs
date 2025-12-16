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
        /// <summary>
        /// Base canvas area used for scaling snowflake count relative to screen size.
        /// </summary>
        private static float BaseCanvasArea => SCREEN_WIDTH * SCREEN_HEIGHT;

        /// <summary>
        /// Maximum number of snowflakes that can be rendered simultaneously.
        /// </summary>
        private const int MaxSnowflakes = 80;

        /// <summary>
        /// Minimum number of snowflakes that will be rendered.
        /// </summary>
        private const int MinSnowflakes = 30;

        /// <summary>
        /// Buffer zone around screen edges where snowflakes can spawn/despawn without visible pop-in.
        /// </summary>
        private const float EdgeBuffer = 40f;

        /// <summary>
        /// Minimum vertical fall speed in pixels per second.
        /// </summary>
        private const float FallSpeedMin = 30f;

        /// <summary>
        /// Maximum vertical fall speed in pixels per second.
        /// </summary>
        private const float FallSpeedMax = 70f;

        /// <summary>
        /// Maximum horizontal drift speed in pixels per second (applied in both directions).
        /// </summary>
        private const float DriftSpeedMax = 15f;

        /// <summary>
        /// Minimum amplitude for horizontal swinging motion in pixels.
        /// </summary>
        private const float SwingAmplitudeMin = 8f;

        /// <summary>
        /// Maximum amplitude for horizontal swinging motion in pixels.
        /// </summary>
        private const float SwingAmplitudeMax = 22f;

        /// <summary>
        /// Minimum frequency for swinging motion (radians per second).
        /// </summary>
        private const float SwingSpeedMin = 0.5f;

        /// <summary>
        /// Maximum frequency for swinging motion (radians per second).
        /// </summary>
        private const float SwingSpeedMax = 1.2f;

        /// <summary>
        /// Minimum frequency for opacity twinkling effect (radians per second).
        /// </summary>
        private const float TwinkleSpeedMin = 0.4f;

        /// <summary>
        /// Maximum frequency for opacity twinkling effect (radians per second).
        /// </summary>
        private const float TwinkleSpeedMax = 1f;

        /// <summary>
        /// Duration in seconds for fade-in and fade-out transitions.
        /// </summary>
        private const float FadeDuration = 0.6f;

        /// <summary>
        /// Collection of active snowflakes being rendered.
        /// </summary>
        private readonly List<Snowflake> snowflakes = [];

        /// <summary>
        /// Texture containing snowflake sprite frames.
        /// </summary>
        private CTRTexture2D texture;

        /// <summary>
        /// Whether the snowfall effect is currently active.
        /// </summary>
        private bool running;

        /// <summary>
        /// Whether the effect is currently fading out before stopping.
        /// </summary>
        private bool fadingOut;

        /// <summary>
        /// Time elapsed in the current fade transition.
        /// </summary>
        private float fadeElapsed;

        /// <summary>
        /// Global alpha multiplier for all snowflakes (0-1), controlled by fade transitions.
        /// </summary>
        private float globalAlpha;

        /// <summary>
        /// Flag indicating the snowflake texture failed to load and should not retry.
        /// </summary>
        private bool textureUnavailable;

        /// <summary>
        /// Private constructor to initialize the snowfall overlay.
        /// Only enabled if Christmas event is active.
        /// </summary>
        private SnowfallOverlay()
        {
            width = (int)SCREEN_WIDTH;
            height = (int)SCREEN_HEIGHT;
            touchable = false;
            updateable = SpecialEvents.IsXmas;
            visible = SpecialEvents.IsXmas;
            globalAlpha = 0f;
        }

        /// <summary>
        /// Factory method to create a snowfall overlay only when Christmas event is enabled.
        /// </summary>
        /// <returns>A new <see cref="SnowfallOverlay"/> instance if Christmas event is active; otherwise, null.</returns>
        public static SnowfallOverlay CreateIfEnabled()
        {
            return SpecialEvents.IsXmas ? new SnowfallOverlay() : null;
        }

        /// <summary>
        /// Updates snowflake positions, animations, and fade transitions each frame.
        /// </summary>
        /// <param name="delta">Time elapsed since last update in seconds.</param>
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

        /// <summary>
        /// Renders all active snowflakes with proper blending and transformations.
        /// Uses additive blending for a soft, glowing appearance.
        /// </summary>
        public override void Draw()
        {
            if (!running || texture == null || snowflakes.Count == 0)
            {
                return;
            }

            PreDraw();

            // Enable blending with additive mode for soft glow effect
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

                // Calculate scaled dimensions with safety checks for invalid texture data
                float safePreCutWidth = IsFinite(preCut.x) && preCut.x > 0 && preCut.x < 10000 ? preCut.x : rect.w;
                float safePreCutHeight = IsFinite(preCut.y) && preCut.y > 0 && preCut.y < 10000 ? preCut.y : rect.h;
                float scaledPreWidth = safePreCutWidth * flake.Scale;
                float scaledPreHeight = safePreCutHeight * flake.Scale;
                float scaledOffsetX = offset.x * flake.Scale;
                float scaledOffsetY = offset.y * flake.Scale;

                // Apply horizontal swinging motion
                float swingOffset = (float)Math.Sin(flake.SwingPhase) * flake.SwingAmplitude;
                float currentX = flake.BaseX + swingOffset;
                float drawX = currentX - (scaledPreWidth / 2f) + scaledOffsetX;
                float drawY = flake.Y - (scaledPreHeight / 2f) + scaledOffsetY;

                // Calculate twinkling alpha with global fade multiplier
                float alpha = flake.AlphaBase + ((float)Math.Sin(flake.TwinklePhase) * flake.AlphaRange);
                float finalAlpha = Math.Clamp(alpha, 0f, 1f) * Math.Clamp(globalAlpha, 0f, 1f);
                if (finalAlpha <= 0f)
                {
                    continue;
                }
                RGBAColor final = new(1f, 1f, 1f, finalAlpha);

                // Draw snowflake with transformation matrix
                OpenGL.GlColor4f(final.ToXNA());
                OpenGL.GlPushMatrix();
                OpenGL.GlTranslatef(drawX, drawY, 0f);
                OpenGL.GlScalef(flake.Scale, flake.Scale, 1f);
                CTRTexture2D.DrawQuadAtPoint(texture, flake.FrameIndex, vectZero);
                OpenGL.GlPopMatrix();
            }

            // Restore default GL state
            OpenGL.GlColor4f(Color.White);
            OpenGL.GlDisable(1);
            OpenGL.GlDisable(0);

            PostDraw();
        }

        /// <summary>
        /// Starts the snowfall effect with a fade-in transition.
        /// Loads texture and initializes snowflakes on first run.
        /// </summary>
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

        /// <summary>
        /// Stops the snowfall effect, either immediately or with a fade-out transition.
        /// </summary>
        /// <param name="immediate">If true, stops instantly without fade-out; otherwise fades out over <see cref="FadeDuration"/>.</param>
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

        /// <summary>
        /// Ensures the snowflake texture is loaded, loading it on first call.
        /// </summary>
        /// <returns>True if texture is available; false if loading failed.</returns>
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

        /// <summary>
        /// Initializes the snowflake collection with randomized positions spread across the screen.
        /// Called when starting the effect.
        /// </summary>
        private void PrepareSnowflakes()
        {
            snowflakes.Clear();
            int count = ComputeSnowflakeCount();
            for (int i = 0; i < count; i++)
            {
                Snowflake flake = CreateSnowflake(populateScreen: true);
                // Distribute vertically above screen for natural entry
                flake.Y = -RND_0_1 * height;
                snowflakes.Add(flake);
            }
        }

        /// <summary>
        /// Calculates the appropriate number of snowflakes based on screen size,
        /// scaling from the base canvas area while respecting min/max limits.
        /// </summary>
        /// <returns>Snowflake count scaled to screen size.</returns>
        private static int ComputeSnowflakeCount()
        {
            float scaleRatio = SCREEN_WIDTH * SCREEN_HEIGHT / BaseCanvasArea;
            int scaled = (int)Math.Round(scaleRatio * MaxSnowflakes);
            return Math.Clamp(scaled, MinSnowflakes, MaxSnowflakes);
        }

        /// <summary>
        /// Creates a new snowflake with randomized properties for natural variation.
        /// </summary>
        /// <param name="populateScreen">If true, spawns across entire screen width; if false, spawns at top center.</param>
        /// <returns>A new <see cref="Snowflake"/> with randomized animation parameters.</returns>
        private Snowflake CreateSnowflake(bool populateScreen)
        {
            int frameCount = texture?.quadsCount ?? 0;
            int frameIndex = frameCount > 0 ? random_.Next(0, frameCount) : 0;

            // Randomize visual properties
            float scale = ((float)random_.NextDouble() * 0.5f) + 0.5f; // 0.5 to 1.0
            float speedY = RandomRange(FallSpeedMin, FallSpeedMax);
            float speedX = RandomRange(-DriftSpeedMax, DriftSpeedMax);
            float swingAmplitude = RandomRange(SwingAmplitudeMin, SwingAmplitudeMax);
            float swingSpeed = RandomRange(SwingSpeedMin, SwingSpeedMax);
            float alphaBase = ((float)random_.NextDouble() * 0.3f) + 0.5f; // 0.5 to 0.8
            float alphaRange = ((float)random_.NextDouble() * 0.25f) + 0.15f; // 0.15 to 0.4

            // Position: spread across screen or spawn at top center
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

        /// <summary>
        /// Resets a snowflake that has moved off-screen by replacing it with a newly created one.
        /// </summary>
        /// <param name="flake">Reference to the snowflake to reset.</param>
        private void ResetSnowflake(ref Snowflake flake)
        {
            Snowflake replacement = CreateSnowflake(populateScreen: false);
            flake = replacement;
        }

        /// <summary>
        /// Updates all snowflake positions and animation phases.
        /// Resets snowflakes that have moved off-screen.
        /// </summary>
        /// <param name="delta">Time elapsed since last update in seconds.</param>
        private void UpdateSnowflakes(float delta)
        {
            float maxY = height + EdgeBuffer;
            float maxX = width + EdgeBuffer;
            float minX = -EdgeBuffer;

            for (int i = 0; i < snowflakes.Count; i++)
            {
                Snowflake flake = snowflakes[i];

                // Update position and animation phases
                flake.Y += flake.SpeedY * delta;
                flake.BaseX += flake.SpeedX * delta;
                flake.SwingPhase += flake.SwingSpeed * delta;
                flake.TwinklePhase += flake.TwinkleSpeed * delta;

                // Check if snowflake has moved off-screen
                float swingOffset = (float)Math.Sin(flake.SwingPhase) * flake.SwingAmplitude;
                float currentX = flake.BaseX + swingOffset;

                if (flake.Y > maxY || currentX < minX || currentX > maxX)
                {
                    ResetSnowflake(ref flake);
                }

                snowflakes[i] = flake;
            }
        }

        /// <summary>
        /// Updates the global alpha fade transition for smooth start/stop effects.
        /// </summary>
        /// <param name="delta">Time elapsed since last update in seconds.</param>
        private void UpdateFade(float delta)
        {
            if (fadingOut)
            {
                // Fade out transition
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
                // Fade in transition
                fadeElapsed += delta;
                float progress = Math.Clamp(fadeElapsed / FadeDuration, 0f, 1f);
                globalAlpha = Math.Min(1f, progress);
            }
        }

        /// <summary>
        /// Generates a random float value within the specified range.
        /// </summary>
        /// <param name="min">Minimum value (inclusive).</param>
        /// <param name="max">Maximum value (inclusive).</param>
        /// <returns>Random value between min and max.</returns>
        private static float RandomRange(float min, float max)
        {
            return min + ((float)random_.NextDouble() * (max - min));
        }

        /// <summary>
        /// Checks if a float value is finite (not NaN or infinity).
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <returns>True if the value is finite; false otherwise.</returns>
        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        /// <summary>
        /// Shared random number generator for all snowflake randomization.
        /// </summary>
        private static readonly Random random_ = new();

        /// <summary>
        /// Represents a single snowflake with its position, animation, and visual properties.
        /// </summary>
        private struct Snowflake
        {
            /// <summary>Texture frame index to render.</summary>
            public int FrameIndex;

            /// <summary>Visual scale multiplier (0.5 to 1.0).</summary>
            public float Scale;

            /// <summary>Vertical fall speed in pixels per second.</summary>
            public float SpeedY;

            /// <summary>Horizontal drift speed in pixels per second.</summary>
            public float SpeedX;

            /// <summary>Amplitude of horizontal swinging motion in pixels.</summary>
            public float SwingAmplitude;

            /// <summary>Frequency of swinging motion in radians per second.</summary>
            public float SwingSpeed;

            /// <summary>Current phase of swinging motion (0 to 2π).</summary>
            public float SwingPhase;

            /// <summary>Base opacity value before twinkling is applied.</summary>
            public float AlphaBase;

            /// <summary>Range of opacity variation for twinkling effect.</summary>
            public float AlphaRange;

            /// <summary>Current phase of twinkling animation (0 to 2π).</summary>
            public float TwinklePhase;

            /// <summary>Frequency of twinkling in radians per second.</summary>
            public float TwinkleSpeed;

            /// <summary>Base horizontal position (before swing offset is applied).</summary>
            public float BaseX;

            /// <summary>Current vertical position.</summary>
            public float Y;
        }
    }
}
