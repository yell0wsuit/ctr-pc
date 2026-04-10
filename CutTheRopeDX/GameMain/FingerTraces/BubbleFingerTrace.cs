using System.Collections.Generic;

using CutTheRopeDX.Framework.Core;

namespace CutTheRopeDX.GameMain.FingerTraces
{
    /// <summary>
    /// CTR2-style bubble finger trace with floating bubble particles that decelerate radially.
    /// </summary>
    internal sealed class BubbleFingerTrace : ParticleFingerTrace
    {
        /// <summary>
        /// Initializes a bubble finger trace.
        /// </summary>
        public BubbleFingerTrace()
            : base(
                segmentLife: 0.15f,
                particleBurstDuration: 0.1f,
                particleEmissionRate: 80f,
                maximumDirectionHistory: 10,
                NamedTracePresets.CreateBubbleParticles())
        {
        }

        /// <inheritdoc />
        protected override void AppendSampledPoints(List<Vector> sampledPoints)
        {
            AppendSegmentEndpoints(sampledPoints);
        }
    }
}
