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
        public const float BungeeRollBackOverflowPadding = 1f;
        public const float BungeeConstraintSlack = 3f;
        public const float BungeeRelaxThresholdSoft = 0.3f;
        public const float BungeeRelaxThresholdMedium = 1f;
        public const float BungeeRelaxThresholdHard = 4f;
        public const float BungeeStretchRedThreshold = 7f;
        public const int BungeeDrawSamplePoints = 4;

        // Bubble impulse
        public const float BubbleImpulseY = -40f;
        public const float BubbleImpulseDamping = 14f;
        public const float BubbleCaptureRadius = 85f;

        // Candy
        public const float CandyBreakGravityY = 500f;
        public const float CandyGrabPadding = 42f;

        // Magic hat / sock
        public const float SockTeleportSpeedMultiplier = 1.4f;
        public const float SockSpeedKoeff = 0.9f;
        public const float GrabRopeRollMaxLength = 1650f;
        public const float GrabWheelRotateDeltaMax = 4.5f;

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

        // Spider
        public const float SpiderTraversalSpeed = 117f;

        // Rope drawing buffer — sized to fit any rope in any level.
        // Formula: (MaxRopeLength / BungeeRestLength + 3) * 4 samples/segment * 2 floats/sample
        public const float MaxRopeLength = 2000f;
        public static readonly int DrawPtsBufferSize =
            (int)(((MaxRopeLength / BungeeRestLength) + 3) * 4 * 2); // = 176
    }
}
