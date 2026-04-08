namespace CutTheRope.Framework
{
    /// <summary>
    /// Constants for mobile physics (unscaled).
    /// </summary>
    internal static class MobilePhysicsConstants
    {
        /// <inheritdoc cref="ActivePhysicsConstants.TimeScale" />
        public const float TimeScale = 1f;

        /// <inheritdoc cref="ActivePhysicsConstants.GravityEarthY" />
        public const float GravityEarthY = 784f;

        /// <inheritdoc cref="ActivePhysicsConstants.RopePhysicsSpeedMultiplier" />
        public const float RopePhysicsSpeedMultiplier = 1f;

        /// <inheritdoc cref="ActivePhysicsConstants.BungeeRestLength" />
        public const float BungeeRestLength = 30f;

        /// <inheritdoc cref="ActivePhysicsConstants.BungeeRollBackOverflowPadding" />
        public const float BungeeRollBackOverflowPadding = 1f;

        /// <inheritdoc cref="ActivePhysicsConstants.BungeeConstraintSlack" />
        public const float BungeeConstraintSlack = 3f;

        /// <inheritdoc cref="ActivePhysicsConstants.BungeeRelaxThresholdSoft" />
        public const float BungeeRelaxThresholdSoft = 0.3f;

        /// <inheritdoc cref="ActivePhysicsConstants.BungeeRelaxThresholdMedium" />
        public const float BungeeRelaxThresholdMedium = 1f;

        /// <inheritdoc cref="ActivePhysicsConstants.BungeeRelaxThresholdHard" />
        public const float BungeeRelaxThresholdHard = 4f;

        /// <inheritdoc cref="ActivePhysicsConstants.BungeeStretchRedThreshold" />
        public const float BungeeStretchRedThreshold = 7f;

        /// <inheritdoc cref="ActivePhysicsConstants.BungeeDrawSamplePoints" />
        public const int BungeeDrawSamplePoints = 3;

        /// <inheritdoc cref="ActivePhysicsConstants.BubbleImpulseY" />
        public const float BubbleImpulseY = -18f;

        /// <inheritdoc cref="ActivePhysicsConstants.BubbleImpulseDamping" />
        public const float BubbleImpulseDamping = 20f;

        /// <inheritdoc cref="ActivePhysicsConstants.BubbleCaptureRadius" />
        public const float BubbleCaptureRadius = 30f;

        /// <inheritdoc cref="ActivePhysicsConstants.CandyBreakGravityY" />
        public const float CandyBreakGravityY = 500f;

        /// <inheritdoc cref="ActivePhysicsConstants.CandyGrabPadding" />
        public const float CandyGrabPadding = 45f;

        /// <inheritdoc cref="ActivePhysicsConstants.SockTeleportSpeedMultiplier" />
        public const float SockTeleportSpeedMultiplier = 1f;

        /// <inheritdoc cref="ActivePhysicsConstants.SockSpeedKoeff" />
        public const float SockSpeedKoeff = 0.9f;

        /// <inheritdoc cref="ActivePhysicsConstants.GrabRopeRollMaxLength" />
        public const float GrabRopeRollMaxLength = 500f;

        /// <inheritdoc cref="ActivePhysicsConstants.GrabWheelRotateDeltaMax" />
        public const float GrabWheelRotateDeltaMax = 2f;

        /// <inheritdoc cref="ActivePhysicsConstants.WaterSurfaceDetectionHeight" />
        public const float WaterSurfaceDetectionHeight = 2f;

        /// <inheritdoc cref="ActivePhysicsConstants.WaterSplashParticleYOffset" />
        public const float WaterSplashParticleYOffset = 3f;

        /// <inheritdoc cref="ActivePhysicsConstants.WaterCandyCollisionRadius" />
        public const float WaterCandyCollisionRadius = 15f;

        /// <inheritdoc cref="ActivePhysicsConstants.WaterDamping" />
        public const float WaterDamping = 20f;

        /// <inheritdoc cref="ActivePhysicsConstants.WaterVerticalImpulseBase" />
        public const float WaterVerticalImpulseBase = -25f;

        /// <inheritdoc cref="ActivePhysicsConstants.WaterRocketImpulseDivisor" />
        public const float WaterRocketImpulseDivisor = 45f;

        /// <inheritdoc cref="ActivePhysicsConstants.WaterRocketDampingMultiplier" />
        public const float WaterRocketDampingMultiplier = 15f;

        /// <inheritdoc cref="ActivePhysicsConstants.WaterRopeAnchorImpulse" />
        public const float WaterRopeAnchorImpulse = -20f;

        /// <inheritdoc cref="ActivePhysicsConstants.BouncerCollisionRadius" />
        public const float BouncerCollisionRadius = 20f;

        /// <inheritdoc cref="ActivePhysicsConstants.BouncerHeight" />
        public const float BouncerHeight = 5f;

        /// <inheritdoc cref="ActivePhysicsConstants.BouncerImpulseVelocityScale" />
        public const float BouncerImpulseVelocityScale = 40f;

        /// <inheritdoc cref="ActivePhysicsConstants.BouncerMinImpulse" />
        public const float BouncerMinImpulse = 300f;

        /// <inheritdoc cref="ActivePhysicsConstants.RocketPointWeight" />
        public const float RocketPointWeight = 0.5f;

        /// <inheritdoc cref="ActivePhysicsConstants.RocketActiveVelocityDamping" />
        public const float RocketActiveVelocityDamping = 20f;

        /// <inheritdoc cref="ActivePhysicsConstants.SteamTubeDamping" />
        public const float SteamTubeDamping = 5f;

        /// <inheritdoc cref="ActivePhysicsConstants.SteamTubeNonAlignedDampingMultiplier" />
        public const float SteamTubeNonAlignedDampingMultiplier = 15f;

        /// <inheritdoc cref="ActivePhysicsConstants.SteamTubeWidthScale" />
        public const float SteamTubeWidthScale = 10f;

        /// <inheritdoc cref="ActivePhysicsConstants.SteamTubeVerticalOffsetScale" />
        public const float SteamTubeVerticalOffsetScale = 1f;

        /// <inheritdoc cref="ActivePhysicsConstants.SteamTubeCollisionRadiusScale" />
        public const float SteamTubeCollisionRadiusScale = 17.5f;

        /// <inheritdoc cref="ActivePhysicsConstants.SteamTubeGravityCompensation" />
        public const float SteamTubeGravityCompensation = -32f;

        /// <inheritdoc cref="ActivePhysicsConstants.SteamTubeSideGravityDivisor" />
        public const float SteamTubeSideGravityDivisor = 4f;

        /// <inheritdoc cref="ActivePhysicsConstants.SteamTubeOppositeGravityDivisor" />
        public const float SteamTubeOppositeGravityDivisor = 2f;

        /// <inheritdoc cref="ActivePhysicsConstants.SpiderTraversalSpeed" />
        public const float SpiderTraversalSpeed = 135f; // 45 * 3

        /// <summary>
        /// Maximum rope length used by the mobile bungee renderer.
        /// </summary>
        public const float MaxRopeLength = 600f; // 20 segments * 30 rest length

        /// <inheritdoc cref="ActivePhysicsConstants.DrawPtsBufferSize" />
        public static readonly int DrawPtsBufferSize = 200;
    }
}
