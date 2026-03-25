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
        public const float BungeeRollBackOverflowPadding = 1f;
        public const float BungeeConstraintSlack = 3f;
        public const float BungeeRelaxThresholdSoft = 0.3f;
        public const float BungeeRelaxThresholdMedium = 1f;
        public const float BungeeRelaxThresholdHard = 4f;
        public const float BungeeStretchRedThreshold = 7f;
        public const int BungeeDrawSamplePoints = 3;

        // Bubble impulse
        public const float BubbleImpulseY = -18f;
        public const float BubbleImpulseDamping = 20f;
        public const float BubbleCaptureRadius = 30f;

        // Candy
        public const float CandyBreakGravityY = 500f;
        public const float CandyGrabPadding = 45f;

        // Magic hat / sock
        public const float SockTeleportSpeedMultiplier = 1f;
        public const float SockSpeedKoeff = 0.9f;
        public const float GrabRopeRollMaxLength = 500f;
        public const float GrabWheelRotateDeltaMax = 2f;

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

        // Spider
        public const float SpiderTraversalSpeed = 135f; // 45 * 3

        // Windows Phone's bungee renderer used a fixed buffer of 200 floats.
        public const float MaxRopeLength = 600f; // 20 segments * 30 rest length
        public static readonly int DrawPtsBufferSize = 200;
    }
}
