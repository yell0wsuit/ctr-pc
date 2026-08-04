using System;

using CutTheRopeDX.Framework.Core;

namespace CutTheRopeDX.GameMain
{
    internal static class AxeSpin
    {
        public static float RotationStepForVelocity(Vector velocity)
        {
            float speed = MathF.Sqrt((velocity.X * velocity.X) + (velocity.Y * velocity.Y)) / 20f;
            return MathF.Min(speed, 40f);
        }
    }
}
