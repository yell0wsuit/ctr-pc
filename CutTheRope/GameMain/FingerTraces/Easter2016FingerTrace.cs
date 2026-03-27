using System;
using System.Collections.Generic;

using CutTheRope.Desktop;
using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.GameMain.FingerTraces
{
    internal sealed class Easter2016FingerTrace : FingerTrace
    {
        private const float SegmentLife = 0.15f;
        private const float ParticleBurstDuration = 0.1f;
        private const float ParticleEmissionRate = 50f;
        private const float RibbonBaseWidth = 12f;
        private const float MinimumRibbonHalfWidth = 1f;
        private const int MaximumDirectionHistory = 10;
        private const int GlowQuadIndex = 2;
        private const float GlowTranslateY = 50f;

        private readonly Easter2016TraceParticles particles = new();
        private readonly List<Vector> directionHistory = [];
        private Image glowImage;

        private VertexPositionColor[] ribbonVerticesCache;
        private float particleTimer;
        private float averageRotation;

        public Easter2016FingerTrace()
        {
        }

        public Easter2016FingerTrace(int _)
            : this()
        {
        }

        protected override bool HasLiveParticles => particles.HasLiveParticles;

        public override void AddSegment(float startX, float startY, float endX, float endY)
        {
            Vector start = new(startX, startY);
            Vector end = new(endX, endY);
            Vector delta = VectSub(end, start);

            particleTimer = ParticleBurstDuration;
            StoreSegment(start, end, SegmentLife);
            directionHistory.Add(delta);
            particles.SetPosition(end);
            EnsureGlowImage();
            glowImage.x = startX;
            glowImage.y = startY;
        }

        public override void Draw()
        {
            List<FingerTraceSpritePose> particleSprites = [];
            particles.AppendSprites(particleSprites);
            if (particleSprites.Count > 0)
            {
                Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
                foreach (FingerTraceSpritePose sprite in particleSprites)
                {
                    DrawSpritePose(sprite);
                }
            }

            if (Segments.Count > 0 && glowImage != null)
            {
                Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
                glowImage.Draw();
            }

            DrawRibbon();

            Renderer.SetColor(Color.White);
            Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
        }

        protected override void UpdateCore(float delta)
        {
            particleTimer -= delta;
            particles.SetEmissionRate(particleTimer > 0f ? ParticleEmissionRate : 0f);
            RefreshHeadState();
            particles.Update(delta);
        }

        protected override void ResetCore()
        {
            directionHistory.Clear();
            particles.Reset();
            particleTimer = 0f;
            averageRotation = 0f;
        }

        protected override void BuildSnapshot(List<Vector> sampledPoints, List<FingerTraceSpritePose> sprites)
        {
            AppendRibbonSampledPoints(sampledPoints);
            particles.AppendSprites(sprites);
        }

        private void DrawRibbon()
        {
            if (!TryBuildRibbonGeometry(out List<Vector> sampledPoints))
            {
                return;
            }

            EnsureRibbonCache(sampledPoints.Count * 2);
            for (int i = 0; i < sampledPoints.Count; i++)
            {
                Vector point = sampledPoints[i];
                Vector direction = GetPointDirection(sampledPoints, i);
                float directionLength = MAX(0.0001f, VectLength(direction));
                Vector normal = new(-(direction.Y / directionLength), direction.X / directionLength);
                float t = sampledPoints.Count == 1 ? 1f : i / (float)(sampledPoints.Count - 1);
                float halfWidth = i == sampledPoints.Count - 1
                    ? MinimumRibbonHalfWidth
                    : MinimumRibbonHalfWidth + (RibbonBaseWidth * t);
                Vector left = VectSub(point, VectMult(normal, halfWidth));
                Vector right = VectAdd(point, VectMult(normal, halfWidth));
                Color color = GetRibbonColor(t).ToXNA();

                int vertexIndex = i * 2;
                ribbonVerticesCache[vertexIndex] = new VertexPositionColor(new Vector3(left.X, left.Y, 0f), color);
                ribbonVerticesCache[vertexIndex + 1] = new VertexPositionColor(new Vector3(right.X, right.Y, 0f), color);
            }

            Renderer.Disable(Renderer.GL_TEXTURE_2D);
            Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
            Renderer.DrawTriangleStrip(ribbonVerticesCache, sampledPoints.Count * 2);
        }

        private void AppendRibbonSampledPoints(List<Vector> sampledPoints)
        {
            if (!TryBuildRibbonGeometry(out List<Vector> centerLine))
            {
                return;
            }

            sampledPoints.AddRange(centerLine);
        }

        private bool TryBuildRibbonGeometry(out List<Vector> sampledPoints)
        {
            sampledPoints = [];
            List<Vector> controlPoints = GetControlPoints();
            if (controlPoints.Count < 2)
            {
                return false;
            }

            int sampleCount = MAX(2, (controlPoints.Count * 2) - 1);
            Vector[] controlPointArray = [.. controlPoints];
            for (int i = 0; i < sampleCount; i++)
            {
                float t = sampleCount == 1 ? 1f : i / (float)(sampleCount - 1);
                sampledPoints.Add(DrawHelper.CalcPathBezier(controlPointArray, controlPointArray.Length, t));
            }

            return sampledPoints.Count >= 2;
        }

        private List<Vector> GetControlPoints()
        {
            List<Vector> controlPoints = [];
            if (Segments.Count == 0)
            {
                return controlPoints;
            }

            for (int i = 0; i < Segments.Count; i++)
            {
                controlPoints.Add(Segments[i].Start);
            }

            controlPoints.Add(Segments[^1].End);
            return controlPoints;
        }

        private Vector GetAverageDirection()
        {
            if (directionHistory.Count == 0)
            {
                return vectZero;
            }

            Vector total = vectZero;
            for (int i = 0; i < directionHistory.Count; i++)
            {
                total = VectAdd(total, directionHistory[i]);
            }

            return VectDiv(total, directionHistory.Count);
        }

        private void RefreshHeadState()
        {
            while (directionHistory.Count > MaximumDirectionHistory)
            {
                directionHistory.RemoveAt(0);
            }

            Vector averageDirection = GetAverageDirection();
            averageRotation = RADIANS_TO_DEGREES(MathF.Atan2(averageDirection.Y, averageDirection.X));
            particles.SetRotation(averageRotation + DEG_180);

            if (glowImage != null)
            {
                glowImage.rotation = averageRotation + DEG_90;
                float glowAlpha = MIN(Segments.Count / 5f, VectLength(averageDirection) / 10f);
                glowImage.color = RGBAColor.MakeRGBA(1f, 1f, 1f, glowAlpha);
            }
        }

        private static Vector GetPointDirection(List<Vector> sampledPoints, int index)
        {
            return sampledPoints.Count == 1
                ? vectZero
                : index == 0
                ? VectSub(sampledPoints[1], sampledPoints[0])
                : index == sampledPoints.Count - 1
                ? VectSub(sampledPoints[^1], sampledPoints[^2])
                : VectSub(sampledPoints[index + 1], sampledPoints[index - 1]);
        }

        /// <summary>
        /// Golden yellow (1.0, 0.87451, 0.05490) fading to white by t=1/3.
        /// </summary>
        private static RGBAColor GetRibbonColor(float t)
        {
            if (t < 0.5f)
            {
                float blend = MathHelper.Clamp(t * 3f, 0f, 1f);
                return RGBAColor.MakeRGBA(
                    1f,
                    MathHelper.Lerp(0.87450981f, 1f, blend),
                    MathHelper.Lerp(0.054901961f, 1f, blend),
                    1f);
            }

            return RGBAColor.MakeRGBA(1f, 1f, 1f, 1f);
        }

        private void EnsureGlowImage()
        {
            if (glowImage != null)
            {
                return;
            }

            glowImage = Image.Image_createWithResIDQuad(Resources.Img.FingerTraceGlow, GlowQuadIndex);
            glowImage.anchor = CENTER;
            glowImage.translateY = GlowTranslateY;
        }

        private void EnsureRibbonCache(int vertexCount)
        {
            if (ribbonVerticesCache == null || ribbonVerticesCache.Length < vertexCount)
            {
                ribbonVerticesCache = new VertexPositionColor[vertexCount];
            }
        }
    }
}
