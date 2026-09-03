using System;

using CutTheRopeDX.Framework.Core;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// How far the axe's blade turns in one frame: the speed divided by a
    /// constant, capped at 40 degrees.
    /// </summary>
    internal static class AxeSpin
    {
        /// <summary>
        /// Speed that produces one degree of turn. The original divides by 20 in its own world,
        /// whose units are two thirds the size of DX's, so the same swing has to divide by 30 here
        /// to spin the blade at the same rate. The 40-degree cap is angular and carries over as-is.
        /// </summary>
        private const float SpeedPerDegree = 20f * AxeDefinition.TimeTravelToWorldScale;

        /// <summary>Maximum turn per frame, in degrees.</summary>
        private const float MaxStepDegrees = 40f;

        public static float RotationStepForVelocity(Vector velocity)
        {
            float speed = MathF.Sqrt((velocity.X * velocity.X) + (velocity.Y * velocity.Y)) / SpeedPerDegree;
            return MathF.Min(speed, MaxStepDegrees);
        }
    }
}
