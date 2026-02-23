namespace CutTheRope.Framework
{
    /// <summary>
    /// Constants for physics simulation for the PC version.
    /// </summary>
    internal static class PhysicsConstants
    {
        // Core simulation constants
        public const float TimeScale = 1f;
        public const float GravityEarthY = 784f; // 9.8 * 80 (PIXEL_TO_SI_METERS_K)
        public const float RopePhysicsSpeedMultiplier = 1.4f;
        public const float BungeeRestLength = 105f;

        // Bubble impulse
        public const float BubbleImpulseY = -40f;
        public const float BubbleImpulseDamping = 14f;

        // Candy
        public const float CandyBreakGravityY = 500f;

        // Magic hat / sock
        public const float SockTeleportSpeedMultiplier = 1.4f;
        public const float SockSpeedKoeff = 0.9f;

        // Water tuning
        public const float WaterSurfaceDetectionHeight = 2f;
        public const float WaterSplashParticleYOffset = 3f;
        public const float WaterCandyCollisionRadius = 15f;
        public const float WaterDamping = 20f;
        public const float WaterVerticalImpulseBase = -75f;
        public const float WaterRocketImpulseDivisor = 45f;
        public const float WaterRocketDampingMultiplier = 15f;
        public const float WaterRopeAnchorImpulse = -20f;

        // Bouncer tuning
        public const float BouncerCollisionRadius = 40f;
        public const float BouncerHeight = 5f;
        public const float BouncerImpulseVelocityScale = 40f;
        public const float BouncerMinImpulse = 840f;

        // Rocket tuning
        public const float RocketPointWeight = 2.5f;
        public const float RocketActiveVelocityDamping = 40f;

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
    }
}
