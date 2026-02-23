namespace CutTheRope.Framework
{
    /// <summary>
    /// Constants for mobile physics (unscaled).
    /// </summary>
    internal static class MobilePhysicsConstants
    {
        // Core simulation constants
        public const float TimeScale = 1f;
        public const float GravityEarthY = 784f;
        public const float RopePhysicsSpeedMultiplier = 1f;
        public const float BungeeRestLength = 30f;

        // Bubble impulse
        public const float BubbleImpulseY = -18f;
        public const float BubbleImpulseDamping = 20f;

        // Candy
        public const float CandyBreakGravityY = 500f;

        // Magic hat / sock
        public const float SockTeleportSpeedMultiplier = 1f;
        public const float SockSpeedKoeff = 0.9f;

        // Water tuning
        public const float WaterSurfaceDetectionHeight = 2f;
        public const float WaterSplashParticleYOffset = 3f;
        public const float WaterCandyCollisionRadius = 15f;
        public const float WaterDamping = 20f;
        public const float WaterVerticalImpulseBase = -25f;
        public const float WaterRocketImpulseDivisor = 45f;
        public const float WaterRocketDampingMultiplier = 15f;
        public const float WaterRopeAnchorImpulse = -20f;

        // Bouncer tuning
        public const float BouncerCollisionRadius = 20f;
        public const float BouncerHeight = 5f;
        public const float BouncerImpulseVelocityScale = 40f;
        public const float BouncerMinImpulse = 300f;

        // Rocket tuning
        public const float RocketPointWeight = 0.5f;
        public const float RocketActiveVelocityDamping = 20f;

        // Steam tube force tuning
        public const float SteamTubeDamping = 5f;
        public const float SteamTubeNonAlignedDampingMultiplier = 15f;
        public const float SteamTubeWidthScale = 10f;
        public const float SteamTubeVerticalOffsetScale = 1f;
        public const float SteamTubeCollisionRadiusScale = 17.5f;
        public const float SteamTubeGravityCompensation = -32f;
        public const float SteamTubeSideGravityDivisor = 4f;
        public const float SteamTubeOppositeGravityDivisor = 2f;

        // Conveyor-belt velocity scaling
        public const float ConveyorVelocityScale = 0.4f;

        // Windows Phone's bungee renderer used a fixed buffer of 200 floats.
        public const float MaxRopeLength = 600f; // 20 segments * 30 rest length
        public static readonly int DrawPtsBufferSize = 200;
    }
}
