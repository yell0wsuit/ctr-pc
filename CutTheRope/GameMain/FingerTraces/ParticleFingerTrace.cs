using System;
using System.Collections.Generic;

using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain.FingerTraces
{
    /// <summary>
    /// Shared trace base for traces that only differ by particle configuration and sampled path logic.
    /// </summary>
    /// <remarks>
    /// Initializes a shared particle-trace base with the supplied segment, burst, and emitter settings.
    /// </remarks>
    /// <param name="segmentLife">The lifetime in seconds assigned to each appended segment.</param>
    /// <param name="particleBurstDuration">The duration in seconds that emission remains enabled after a segment is appended.</param>
    /// <param name="particleEmissionRate">The particle emission rate used while the burst timer is active.</param>
    /// <param name="maximumDirectionHistory">The maximum number of recent segment directions retained for head-state smoothing.</param>
    /// <param name="particles">The emitters owned by this trace.</param>
    internal abstract class ParticleFingerTrace(
        float segmentLife,
        float particleBurstDuration,
        float particleEmissionRate,
        int maximumDirectionHistory,
        params FingerParticles[] particles) : FingerTrace
    {
        /// <summary>
        /// Recent segment directions retained for smoothed head-state calculations.
        /// </summary>
        private readonly List<Vector> directionHistory = [];

        /// <summary>
        /// Particle emitters owned by this trace.
        /// </summary>
        private readonly FingerParticles[] particles = particles;

        /// <summary>
        /// Duration in seconds that particle emission remains active after a segment is appended.
        /// </summary>
        private readonly float particleBurstDuration = particleBurstDuration;

        /// <summary>
        /// Emission rate used while the active particle burst window is running.
        /// </summary>
        private readonly float particleEmissionRate = particleEmissionRate;

        /// <summary>
        /// Lifetime in seconds assigned to each appended segment.
        /// </summary>
        private readonly float segmentLife = segmentLife;

        /// <summary>
        /// Maximum number of recent segment directions retained for smoothing.
        /// </summary>
        private readonly int maximumDirectionHistory = maximumDirectionHistory;

        /// <summary>
        /// Remaining time in seconds before particle emission is disabled again.
        /// </summary>
        private float particleTimer;

        /// <summary>
        /// Gets the smoothed emitter or head rotation in degrees derived from recent segment directions.
        /// </summary>
        protected float AverageRotation { get; private set; }

        /// <inheritdoc />
        protected override bool HasLiveParticles
        {
            get
            {
                for (int i = 0; i < particles.Length; i++)
                {
                    if (particles[i].HasLiveParticles)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <inheritdoc />
        public override void AddSegment(float startX, float startY, float endX, float endY)
        {
            Vector start = new(startX, startY);
            Vector end = new(endX, endY);
            Vector delta = VectSub(end, start);

            particleTimer = particleBurstDuration;
            StoreSegment(start, end, segmentLife);
            directionHistory.Add(delta);

            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].SetPosition(end);
            }

            OnSegmentAdded(start, end, delta);
        }

        /// <inheritdoc />
        protected override void UpdateCore(float delta)
        {
            particleTimer -= delta;
            float emissionRate = particleTimer > 0f ? particleEmissionRate : 0f;
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].SetEmissionRate(emissionRate);
            }

            RefreshHeadState();

            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Update(delta);
            }
        }

        /// <inheritdoc />
        protected override void ResetCore()
        {
            directionHistory.Clear();
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Reset();
            }

            particleTimer = 0f;
            AverageRotation = 0f;
            OnReset();
        }

        /// <inheritdoc />
        protected override void BuildSnapshot(List<Vector> sampledPoints, List<FingerTraceSpritePose> sprites)
        {
            AppendSampledPoints(sampledPoints);
            AppendParticleSprites(sprites);
            AppendSprites(sprites);
        }

        /// <summary>
        /// Appends the raw segment endpoints as a polyline into <paramref name="sampledPoints"/>.
        /// </summary>
        /// <param name="sampledPoints">The destination list that receives the live segment endpoints.</param>
        protected void AppendSegmentEndpoints(List<Vector> sampledPoints)
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
        /// Appends sprites from all owned particle emitters into <paramref name="sprites"/>.
        /// </summary>
        /// <param name="sprites">The destination list that receives particle sprite poses.</param>
        protected void AppendParticleSprites(List<FingerTraceSpritePose> sprites)
        {
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].AppendSprites(sprites);
            }
        }

        /// <summary>
        /// Allows subclasses to react immediately after a new segment has been stored.
        /// </summary>
        /// <param name="start">The segment start position.</param>
        /// <param name="end">The segment end position.</param>
        /// <param name="delta">The segment direction vector from <paramref name="start"/> to <paramref name="end"/>.</param>
        protected virtual void OnSegmentAdded(Vector start, Vector end, Vector delta)
        {
        }

        /// <summary>
        /// Allows subclasses to react after the averaged head direction has been updated.
        /// </summary>
        /// <param name="averageDirection">The averaged direction vector derived from recent segment history.</param>
        protected virtual void OnHeadStateUpdated(Vector averageDirection)
        {
        }

        /// <summary>
        /// Allows subclasses to clear additional transient state when the trace resets.
        /// </summary>
        protected virtual void OnReset()
        {
        }

        /// <summary>
        /// Allows subclasses to append non-particle sprites to the current snapshot.
        /// </summary>
        /// <param name="sprites">The destination list that receives additional sprite poses.</param>
        protected virtual void AppendSprites(List<FingerTraceSpritePose> sprites)
        {
        }

        /// <summary>
        /// Appends the trace-specific sampled path into <paramref name="sampledPoints"/>.
        /// </summary>
        /// <param name="sampledPoints">The destination list that receives the sampled path points.</param>
        protected abstract void AppendSampledPoints(List<Vector> sampledPoints);

        /// <summary>
        /// Refreshes the smoothed head-state values derived from the recent direction history.
        /// </summary>
        private void RefreshHeadState()
        {
            while (directionHistory.Count > maximumDirectionHistory)
            {
                directionHistory.RemoveAt(0);
            }

            Vector averageDirection = GetAverageDirection();
            AverageRotation = RADIANS_TO_DEGREES(MathF.Atan2(averageDirection.Y, averageDirection.X));
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].SetRotation(AverageRotation + DEG_180);
            }

            OnHeadStateUpdated(averageDirection);
        }

        /// <summary>
        /// Returns the averaged direction vector derived from the retained segment history.
        /// </summary>
        /// <returns>The averaged direction vector, or the zero vector when no history is available.</returns>
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
    }
}
