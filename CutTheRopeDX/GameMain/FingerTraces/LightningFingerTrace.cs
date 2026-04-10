using System;
using System.Collections.Generic;

using CutTheRopeDX.Desktop;
using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Visual;

using Microsoft.Xna.Framework.Graphics;

using XnaColor = Microsoft.Xna.Framework.Color;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;
using XnaVector3 = Microsoft.Xna.Framework.Vector3;

namespace CutTheRopeDX.GameMain.FingerTraces
{
    /// <summary>
    /// CTR2-style lightning finger trace that renders randomized head / body quads plus trailing sparks.
    /// </summary>
    internal sealed class LightningFingerTrace : FingerTrace
    {
        /// <summary>The first atlas quad used for body segments.</summary>
        private const int BodyQuadStart = 14;

        /// <summary>The number of body atlas quads available.</summary>
        private const int BodyQuadCount = 6;

        /// <summary>The first atlas quad used for the lightning head.</summary>
        private const int HeadQuadStart = 20;

        /// <summary>The number of head atlas quads available.</summary>
        private const int HeadQuadCount = 4;

        /// <summary>The minimum lifetime applied to a lightning segment.</summary>
        private const float MinSegmentLife = 0.01f;

        /// <summary>The maximum lifetime applied to a lightning segment.</summary>
        private const float MaxSegmentLife = 0.25f;

        /// <summary>The length-to-lifetime scale factor.</summary>
        private const float SegmentLifeMultiplier = 0.007f;

        /// <summary>The base lifetime added before clamping.</summary>
        private const float SegmentLifeBase = 0.05f;

        /// <summary>The duration of the spark burst after each appended segment.</summary>
        private const float ParticleBurstDuration = 0.1f;

        /// <summary>The spark emission rate used while the burst timer is active.</summary>
        private const float ParticleEmissionRate = 50f;

        /// <summary>The time between randomized head/body variant refreshes.</summary>
        private const float VariantRefreshSeconds = 0.06f;

        /// <summary>The nominal height used to scale lightning quads.</summary>
        private const float QuadSpacing = 112f;

        /// <summary>The stretch factor applied to lightning quads.</summary>
        private const float QuadStretch = 140f / 112f;

        /// <summary>The lateral offset used to jag the body quads.</summary>
        private const float QuadOffsetAmount = 15f;

        /// <summary>The atlas quad used for the glow sprite.</summary>
        private const int GlowQuadIndex = 0;

        /// <summary>The Y translation applied to the glow sprite pivot.</summary>
        private const float GlowTranslateY = 48f;

        /// <summary>The lightning spark emitter.</summary>
        private readonly FingerParticles particles = NamedTracePresets.CreateLightningParticles();

        /// <summary>Recent segment directions used to smooth the head rotation.</summary>
        private readonly List<Vector> directionHistory = [];

        /// <summary>The randomized body variant sequence.</summary>
        private readonly int[] bodyVariants = new int[10];

        /// <summary>The glow sprite drawn at the lightning head.</summary>
        private Image glowImage;

        /// <summary>The reusable textured vertex cache for batched lightning quads.</summary>
        private VertexPositionColorTexture[] verticesCache;

        /// <summary>The reusable index cache for batched lightning quads.</summary>
        private short[] indicesCache;

        /// <summary>The currently selected head quad index.</summary>
        private int currentHeadVariant = -1;

        /// <summary>The remaining time in the active spark burst.</summary>
        private float particleTimer;

        /// <summary>The remaining time before the quad variants are randomized again.</summary>
        private float variantTimer;

        /// <summary>The smoothed head rotation in degrees.</summary>
        private float averageRotation;

        /// <summary>The current head glow scale.</summary>
        private float headScale;

        /// <summary>
        /// Initializes a lightning trace with the RNG path.
        /// </summary>
        public LightningFingerTrace()
        {
            ResetVariants();
        }

        /// <inheritdoc />
        protected override bool HasLiveParticles => particles.HasLiveParticles;

        /// <inheritdoc />
        public override void AddSegment(float startX, float startY, float endX, float endY)
        {
            Vector start = new(startX, startY);
            Vector end = new(endX, endY);
            Vector delta = VectSub(end, start);
            float length = VectLength(delta);
            float life = FIT_TO_BOUNDARIES((length * SegmentLifeMultiplier) + SegmentLifeBase, MinSegmentLife, MaxSegmentLife);

            particleTimer = ParticleBurstDuration;
            StoreSegment(start, end, life);
            directionHistory.Add(delta);
            particles.SetPosition(end);
            EnsureGlowImage();
            glowImage.x = startX;
            glowImage.y = startY;
        }

        /// <inheritdoc />
        public override void Draw()
        {
            if (Segments.Count >= 2 && currentHeadVariant != -1 && glowImage != null)
            {
                Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
                glowImage.Draw();
            }

            DrawLightningBody();

            List<FingerTraceSpritePose> particleSprites = [];
            particles.AppendSprites(particleSprites);
            if (particleSprites.Count == 0)
            {
                return;
            }

            FingerTraceBlendMode? currentBlendMode = null;
            foreach (FingerTraceSpritePose sprite in particleSprites)
            {
                if (currentBlendMode != sprite.BlendMode)
                {
                    currentBlendMode = sprite.BlendMode;
                    Renderer.SetBlendFunc(
                        sprite.BlendMode == FingerTraceBlendMode.Additive
                            ? BlendingFactor.GLSRCALPHA
                            : BlendingFactor.GLONE,
                        sprite.BlendMode == FingerTraceBlendMode.Additive
                            ? BlendingFactor.GLONE
                            : BlendingFactor.GLONEMINUSSRCALPHA);
                }

                DrawSpritePose(sprite);
            }

            Renderer.SetColor(XnaColor.White);
            Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
        }

        /// <inheritdoc />
        protected override void UpdateCore(float delta)
        {
            particleTimer -= delta;
            particles.SetEmissionRate(particleTimer > 0f ? ParticleEmissionRate : 0f);

            while (directionHistory.Count > 10)
            {
                directionHistory.RemoveAt(0);
            }

            Vector averageDirection = GetAverageDirection();
            averageRotation = RADIANS_TO_DEGREES(MathF.Atan2(averageDirection.Y, averageDirection.X));
            particles.SetRotation(averageRotation + DEG_180);
            headScale = MIN(Segments.Count / 5f, VectLength(averageDirection) / 10f);

            if (glowImage != null)
            {
                glowImage.rotation = averageRotation + DEG_90;
                glowImage.color = RGBAColor.MakeRGBA(1f, 1f, 1f, headScale);
            }

            particles.Update(delta);

            variantTimer -= delta;
            if (variantTimer <= 0f || currentHeadVariant == -1)
            {
                currentHeadVariant = NextDifferentVariant(HeadQuadStart, HeadQuadCount, currentHeadVariant);
                FillBodyVariants();
                variantTimer = VariantRefreshSeconds;
            }
        }

        /// <inheritdoc />
        protected override void ResetCore()
        {
            directionHistory.Clear();
            particles.Reset();
            currentHeadVariant = -1;
            particleTimer = 0f;
            variantTimer = 0f;
            averageRotation = 0f;
            headScale = 0f;
            ResetVariants();
        }

        /// <inheritdoc />
        protected override void BuildSnapshot(List<Vector> sampledPoints, List<FingerTraceSpritePose> sprites)
        {
            AppendSampledPoints(sampledPoints);

            List<LightningQuad> quads = BuildLightningQuads();
            for (int i = 0; i < quads.Count; i++)
            {
                LightningQuad quad = quads[i];
                float rotation = RADIANS_TO_DEGREES(MathF.Atan2(quad.End.Y - quad.Start.Y, quad.End.X - quad.Start.X)) + DEG_90;
                float scale = MAX(0.01f, VectDistance(quad.Start, quad.End) / QuadSpacing);
                sprites.Add(new FingerTraceSpritePose(
                    i == 0 ? FingerTraceSpriteKind.Head : FingerTraceSpriteKind.Body,
                    Resources.Img.FingerTraces,
                    quad.QuadIndex,
                    quad.Center,
                    rotation,
                    i == 0 ? headScale : scale,
                    1f,
                    FingerTraceBlendMode.Alpha));
            }

            particles.AppendSprites(sprites);
        }

        /// <summary>
        /// Draws the current lightning head and body quads as a single indexed batch.
        /// </summary>
        private void DrawLightningBody()
        {
            List<LightningQuad> quads = BuildLightningQuads();
            if (quads.Count == 0)
            {
                return;
            }

            CTRTexture2D texture = Application.GetTexture(Resources.Img.FingerTraces);
            EnsureBuffers(quads.Count);

            for (int i = 0; i < quads.Count; i++)
            {
                LightningQuad quad = quads[i];
                Quad2D textureQuad = texture.quads[quad.QuadIndex];
                XnaColor color = RGBAColor.MakeRGBA(1f, 1f, (i + 0.5f) / quads.Count, 1f).ToXNA();
                int vertexIndex = i * 4;

                verticesCache[vertexIndex] = new VertexPositionColorTexture(
                    new XnaVector3(quad.BottomLeft.X, quad.BottomLeft.Y, 0f),
                    color,
                    new XnaVector2(textureQuad.blX, textureQuad.blY));
                verticesCache[vertexIndex + 1] = new VertexPositionColorTexture(
                    new XnaVector3(quad.BottomRight.X, quad.BottomRight.Y, 0f),
                    color,
                    new XnaVector2(textureQuad.brX, textureQuad.brY));
                verticesCache[vertexIndex + 2] = new VertexPositionColorTexture(
                    new XnaVector3(quad.TopLeft.X, quad.TopLeft.Y, 0f),
                    color,
                    new XnaVector2(textureQuad.tlX, textureQuad.tlY));
                verticesCache[vertexIndex + 3] = new VertexPositionColorTexture(
                    new XnaVector3(quad.TopRight.X, quad.TopRight.Y, 0f),
                    color,
                    new XnaVector2(textureQuad.trX, textureQuad.trY));
            }

            Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
            Renderer.Enable(Renderer.GL_TEXTURE_2D);
            Renderer.BindTexture(texture.Name());
            Renderer.DrawTriangleList(verticesCache, indicesCache, quads.Count * 6);
            Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
        }

        /// <summary>
        /// Builds the current lightning head/body quad layout from the live segments.
        /// </summary>
        /// <returns>The list of quads to draw for the current frame.</returns>
        private List<LightningQuad> BuildLightningQuads()
        {
            List<LightningQuad> quads = [];
            if (Segments.Count < 2 || currentHeadVariant == -1)
            {
                return quads;
            }

            float totalLength = 0f;
            for (int i = 0; i < Segments.Count; i++)
            {
                totalLength += VectDistance(Segments[i].Start, Segments[i].End);
            }

            int quadCount = (int)(totalLength / QuadSpacing) + 1;
            if (quadCount <= 0)
            {
                return quads;
            }

            float stepLength = totalLength / quadCount;
            int startIndex = 0;
            for (int quadIndex = 0; quadIndex < quadCount && startIndex < Segments.Count; quadIndex++)
            {
                int endIndexExclusive = startIndex;
                if (stepLength > 0f)
                {
                    float accumulated = 0f;
                    while (endIndexExclusive < Segments.Count)
                    {
                        accumulated += VectDistance(Segments[endIndexExclusive].Start, Segments[endIndexExclusive].End);
                        endIndexExclusive++;
                        if (accumulated >= stepLength || endIndexExclusive >= Segments.Count)
                        {
                            break;
                        }
                    }
                }

                int endIndex = endIndexExclusive - (endIndexExclusive == Segments.Count ? 1 : 0);
                if (endIndex <= startIndex)
                {
                    startIndex++;
                    continue;
                }

                Vector start = Segments[startIndex].Start;
                Vector end = Segments[endIndex].End;
                Vector center = VectMult(VectAdd(start, end), 0.5f);
                Vector halfSpan = VectMult(VectSub(end, start), 0.5f * QuadStretch);
                Vector quadStart = VectSub(center, halfSpan);
                Vector quadEnd = VectAdd(center, halfSpan);
                Vector direction = VectSub(quadEnd, quadStart);
                float directionLength = VectLength(direction);
                if (directionLength <= FLOAT_PRECISION)
                {
                    startIndex = endIndex + 1;
                    continue;
                }

                Vector normal = Vect(direction.Y / directionLength * -1f, direction.X / directionLength);
                Vector offset = VectMult(normal, (quadIndex + 1) * QuadOffsetAmount);
                quads.Add(new LightningQuad(
                    quadIndex == 0 ? currentHeadVariant : bodyVariants[quadIndex % bodyVariants.Length],
                    quadStart,
                    quadEnd,
                    VectAdd(quadStart, offset),
                    VectSub(quadStart, offset),
                    VectAdd(quadEnd, offset),
                    VectSub(quadEnd, offset),
                    center));
                startIndex = endIndex + 1;
            }

            return quads;
        }

        /// <summary>
        /// Appends representative sampled points for the current lightning path.
        /// </summary>
        /// <param name="sampledPoints">The destination sampled-point list.</param>
        private void AppendSampledPoints(List<Vector> sampledPoints)
        {
            if (Segments.Count == 0)
            {
                return;
            }

            for (int i = 0; i < Segments.Count; i++)
            {
                sampledPoints.Add(Segments[i].Start);
            }

            sampledPoints.Add(Segments[^1].End);
        }

        /// <summary>
        /// Returns the averaged head direction derived from recent segment deltas.
        /// </summary>
        /// <returns>The averaged direction vector.</returns>
        private Vector GetAverageDirection()
        {
            Vector sum = vectZero;
            for (int i = 0; i < directionHistory.Count; i++)
            {
                sum = VectAdd(sum, directionHistory[i]);
            }

            return directionHistory.Count == 0
                ? vectZero
                : VectDiv(sum, directionHistory.Count);
        }

        /// <summary>
        /// Refreshes the randomized body variant sequence.
        /// </summary>
        private void FillBodyVariants()
        {
            for (int i = 0; i < bodyVariants.Length; i++)
            {
                bodyVariants[i] = NextDifferentVariant(BodyQuadStart, BodyQuadCount, bodyVariants[i]);
            }
        }

        /// <summary>
        /// Resets the current head and body variant selection.
        /// </summary>
        private void ResetVariants()
        {
            currentHeadVariant = -1;
            for (int i = 0; i < bodyVariants.Length; i++)
            {
                bodyVariants[i] = -1;
            }
        }

        /// <summary>
        /// Chooses a quad index from the given range that differs from the previous value when possible.
        /// </summary>
        /// <param name="start">The first quad index in the range.</param>
        /// <param name="count">The number of indices in the range.</param>
        /// <param name="previous">The previously selected quad index.</param>
        /// <returns>A quad index within the requested range.</returns>
        private static int NextDifferentVariant(int start, int count, int previous)
        {
            int next;
            do
            {
                next = start + NextInt(count);
            }
            while (count > 1 && next == previous);

            return next;
        }

        /// <summary>
        /// Creates the glow sprite on first use.
        /// </summary>
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

        /// <summary>
        /// Ensures the reusable vertex and index buffers can hold the requested number of quads.
        /// </summary>
        /// <param name="quadCount">The required quad capacity.</param>
        private void EnsureBuffers(int quadCount)
        {
            int requiredVertices = quadCount * 4;
            if (verticesCache == null || verticesCache.Length < requiredVertices)
            {
                verticesCache = new VertexPositionColorTexture[requiredVertices];
            }

            int requiredIndices = quadCount * 6;
            if (indicesCache != null && indicesCache.Length >= requiredIndices)
            {
                return;
            }

            indicesCache = new short[requiredIndices];
            for (int i = 0; i < quadCount; i++)
            {
                int vertexIndex = i * 4;
                int indexIndex = i * 6;
                indicesCache[indexIndex] = (short)vertexIndex;
                indicesCache[indexIndex + 1] = (short)(vertexIndex + 1);
                indicesCache[indexIndex + 2] = (short)(vertexIndex + 2);
                indicesCache[indexIndex + 3] = (short)(vertexIndex + 3);
                indicesCache[indexIndex + 4] = (short)(vertexIndex + 2);
                indicesCache[indexIndex + 5] = (short)(vertexIndex + 1);
            }
        }

        /// <summary>
        /// Returns a random integer in the range <c>[0, upperExclusive)</c>.
        /// </summary>
        /// <param name="upperExclusive">The exclusive upper bound.</param>
        /// <returns>A random integer less than <paramref name="upperExclusive"/>.</returns>
        private static int NextInt(int upperExclusive)
        {
            return upperExclusive <= 1
                ? 0
                : (int)(Arc4random() % (uint)upperExclusive);
        }

        /// <summary>
        /// Describes one lightning quad ready for batched rendering.
        /// </summary>
        /// <param name="QuadIndex">Texture quad index used for this segment.</param>
        /// <param name="Start">Segment start point.</param>
        /// <param name="End">Segment end point.</param>
        /// <param name="BottomLeft">Bottom-left corner of the rendered quad.</param>
        /// <param name="BottomRight">Bottom-right corner of the rendered quad.</param>
        /// <param name="TopLeft">Top-left corner of the rendered quad.</param>
        /// <param name="TopRight">Top-right corner of the rendered quad.</param>
        /// <param name="Center">Quad center point.</param>
        private readonly record struct LightningQuad(
            int QuadIndex,
            Vector Start,
            Vector End,
            Vector BottomLeft,
            Vector BottomRight,
            Vector TopLeft,
            Vector TopRight,
            Vector Center);
    }
}
