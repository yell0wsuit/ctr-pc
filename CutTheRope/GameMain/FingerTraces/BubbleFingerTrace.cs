using System;
using System.Collections.Generic;

using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain.FingerTraces
{
    /// <summary>
    /// CTR2-style bubble finger trace with floating bubble particles that decelerate radially.
    /// </summary>
    internal sealed class BubbleFingerTrace : FingerTrace
    {
        private const float SegmentLife = 0.15f;
        private const float ParticleBurstDuration = 0.1f;
        private const float ParticleEmissionRate = 80f;

        private readonly BubbleTraceParticles particles = new();
        private readonly List<Vector> directionHistory = [];
        private const int MaximumDirectionHistory = 10;

        private float particleTimer;
        private float averageRotation;

        public BubbleFingerTrace()
        {
        }

        /// <summary>
        /// Initializes a bubble trace for a touch slot.
        /// </summary>
        /// <param name="_">
        /// Unused touch-slot placeholder retained for parity with the existing per-touch construction API.
        /// </param>
        public BubbleFingerTrace(int _)
            : this()
        {
        }

        protected override bool HasLiveParticles => particles.HasLiveParticles;

        /// <summary>
        /// Adds a new bubble segment with the fixed CTR2 bubble lifetime.
        /// </summary>
        public override void AddSegment(float startX, float startY, float endX, float endY)
        {
            Vector start = new(startX, startY);
            Vector end = new(endX, endY);
            Vector delta = VectSub(end, start);

            particleTimer = ParticleBurstDuration;
            StoreSegment(start, end, SegmentLife);
            directionHistory.Add(delta);
            RefreshHeadState();
            particles.SetPosition(end);
        }

        /// <summary>
        /// Advances particle emission and the averaged head direction state.
        /// </summary>
        protected override void UpdateCore(float delta)
        {
            particleTimer -= delta;
            particles.SetEmissionRate(particleTimer > 0f ? ParticleEmissionRate : 0f);
            RefreshHeadState();
            particles.Update(delta);
        }

        /// <summary>
        /// Clears bubble-specific transient state.
        /// </summary>
        protected override void ResetCore()
        {
            directionHistory.Clear();
            particles.Reset();
            particleTimer = 0f;
            averageRotation = 0f;
        }

        /// <summary>
        /// Publishes the bubble particle sprite metadata for snapshot rendering.
        /// </summary>
        protected override void BuildSnapshot(List<Vector> sampledPoints, List<FingerTraceSpritePose> sprites)
        {
            AppendSampledPoints(sampledPoints);
            particles.AppendSprites(sprites);
        }

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
        }
    }
}
