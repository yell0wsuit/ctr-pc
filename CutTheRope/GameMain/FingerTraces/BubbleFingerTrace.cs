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
        /// <summary>The lifetime of each stored segment in seconds.</summary>
        private const float SegmentLife = 0.15f;

        /// <summary>The duration of the particle burst after each appended segment.</summary>
        private const float ParticleBurstDuration = 0.1f;

        /// <summary>The emission rate used while the burst timer is active.</summary>
        private const float ParticleEmissionRate = 80f;

        /// <summary>The bubble particle emitter.</summary>
        private readonly BubbleTraceParticles particles = new();

        /// <summary>Recent segment directions used to smooth the emitter rotation.</summary>
        private readonly List<Vector> directionHistory = [];

        /// <summary>The number of direction samples kept for smoothing.</summary>
        private const int MaximumDirectionHistory = 10;

        /// <summary>The remaining time in the active particle burst.</summary>
        private float particleTimer;

        /// <summary>The smoothed head rotation in degrees.</summary>
        private float averageRotation;

        /// <summary>
        /// Initializes a bubble finger trace.
        /// </summary>
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

        /// <inheritdoc />
        protected override bool HasLiveParticles => particles.HasLiveParticles;

        /// <inheritdoc />
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

        /// <inheritdoc />
        protected override void UpdateCore(float delta)
        {
            particleTimer -= delta;
            particles.SetEmissionRate(particleTimer > 0f ? ParticleEmissionRate : 0f);
            RefreshHeadState();
            particles.Update(delta);
        }

        /// <inheritdoc />
        protected override void ResetCore()
        {
            directionHistory.Clear();
            particles.Reset();
            particleTimer = 0f;
            averageRotation = 0f;
        }

        /// <inheritdoc />
        protected override void BuildSnapshot(List<Vector> sampledPoints, List<FingerTraceSpritePose> sprites)
        {
            AppendSampledPoints(sampledPoints);
            particles.AppendSprites(sprites);
        }

        /// <summary>
        /// Appends the trace center line implied by the live segments.
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

        /// <summary>
        /// Refreshes the emitter rotation from the recent direction history.
        /// </summary>
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
