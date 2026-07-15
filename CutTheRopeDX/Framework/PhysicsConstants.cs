namespace CutTheRopeDX.Framework
{
    /// <summary>
    /// Constants for physics simulation for the PC version.
    /// </summary>
    internal static class PhysicsConstants
    {
        /// <inheritdoc cref="ActivePhysicsConstants.TimeScale" />
        public const float TimeScale = 1f;

        /// <inheritdoc cref="ActivePhysicsConstants.GravityEarthY" />
        public const float GravityEarthY = 784f; // 9.8 * 80 (PIXEL_TO_SI_METERS_K)

        /// <inheritdoc cref="ActivePhysicsConstants.RopePhysicsSpeedMultiplier" />
        public const float RopePhysicsSpeedMultiplier = 1.4f;

        /// <inheritdoc cref="ActivePhysicsConstants.BungeeRestLength" />
        public const float BungeeRestLength = 105f;

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
        public const int BungeeDrawSamplePoints = 4;

        /// <inheritdoc cref="ActivePhysicsConstants.BubbleImpulseY" />
        public const float BubbleImpulseY = -40f;

        /// <inheritdoc cref="ActivePhysicsConstants.BubbleImpulseDamping" />
        public const float BubbleImpulseDamping = 14f;

        /// <inheritdoc cref="ActivePhysicsConstants.BubbleCaptureRadius" />
        public const float BubbleCaptureRadius = 85f;

        /// <inheritdoc cref="ActivePhysicsConstants.CandyBreakGravityY" />
        public const float CandyBreakGravityY = 500f;

        /// <inheritdoc cref="ActivePhysicsConstants.CandyGrabPadding" />
        public const float CandyGrabPadding = 42f;

        /// <inheritdoc cref="ActivePhysicsConstants.SockTeleportSpeedMultiplier" />
        public const float SockTeleportSpeedMultiplier = 1.4f;

        /// <inheritdoc cref="ActivePhysicsConstants.SockSpeedKoeff" />
        public const float SockSpeedKoeff = 0.9f;

        /// <inheritdoc cref="ActivePhysicsConstants.SockCatchHalfSize" />
        public const float SockCatchHalfSize = 20f;

        /// <inheritdoc cref="ActivePhysicsConstants.SockExitOffsetY" />
        public const float SockExitOffsetY = -16f;

        /// <inheritdoc cref="ActivePhysicsConstants.GrabRopeRollMaxLength" />
        public const float GrabRopeRollMaxLength = 1650f;

        /// <inheritdoc cref="ActivePhysicsConstants.GrabWheelRotateDeltaMax" />
        public const float GrabWheelRotateDeltaMax = 4.5f;

        /// <inheritdoc cref="ActivePhysicsConstants.GrabWheelRotateDeltaMin" />
        public const float GrabWheelRotateDeltaMin = 1f;

        /// <inheritdoc cref="ActivePhysicsConstants.RocketReelSpeed" />
        public const float RocketReelSpeed = 200f;

        /// <inheritdoc cref="ActivePhysicsConstants.CandyPartsMergeSpeed" />
        public const float CandyPartsMergeSpeed = 200f;

        /// <inheritdoc cref="ActivePhysicsConstants.WaterSurfaceDetectionHeight" />
        public const float WaterSurfaceDetectionHeight = 2f;

        /// <inheritdoc cref="ActivePhysicsConstants.WaterSplashParticleYOffset" />
        public const float WaterSplashParticleYOffset = 3f;

        /// <inheritdoc cref="ActivePhysicsConstants.WaterCandyCollisionRadius" />
        public const float WaterCandyCollisionRadius = 15f;

        /// <inheritdoc cref="ActivePhysicsConstants.WaterDamping" />
        public const float WaterDamping = 20f;

        /// <inheritdoc cref="ActivePhysicsConstants.WaterVerticalImpulseBase" />
        public const float WaterVerticalImpulseBase = -75f;

        /// <inheritdoc cref="ActivePhysicsConstants.WaterRocketImpulseDivisor" />
        public const float WaterRocketImpulseDivisor = 45f;

        /// <inheritdoc cref="ActivePhysicsConstants.WaterRocketDampingMultiplier" />
        public const float WaterRocketDampingMultiplier = 15f;

        /// <inheritdoc cref="ActivePhysicsConstants.WaterRopeAnchorImpulse" />
        public const float WaterRopeAnchorImpulse = -20f;

        /// <inheritdoc cref="ActivePhysicsConstants.BouncerCollisionRadius" />
        public const float BouncerCollisionRadius = 40f;

        /// <inheritdoc cref="ActivePhysicsConstants.BouncerHeight" />
        public const float BouncerHeight = 5f;

        /// <inheritdoc cref="ActivePhysicsConstants.BouncerImpulseVelocityScale" />
        public const float BouncerImpulseVelocityScale = 40f;

        /// <inheritdoc cref="ActivePhysicsConstants.BouncerMinImpulse" />
        public const float BouncerMinImpulse = 840f;

        /// <inheritdoc cref="ActivePhysicsConstants.RocketPointWeight" />
        public const float RocketPointWeight = 2.5f;

        /// <inheritdoc cref="ActivePhysicsConstants.RocketActiveVelocityDamping" />
        public const float RocketActiveVelocityDamping = 40f;

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

        /// <inheritdoc cref="ActivePhysicsConstants.SteamTubeFalloffExponent" />
        public const float SteamTubeFalloffExponent = -2f;

        /// <inheritdoc cref="ActivePhysicsConstants.SteamTubeVelocityDeadzone" />
        public const float SteamTubeVelocityDeadzone = 1f;

        /// <inheritdoc cref="ActivePhysicsConstants.LanternCaptureRadius" />
        public const float LanternCaptureRadius = 82f;

        /// <inheritdoc cref="ActivePhysicsConstants.PumpFlowLength" />
        public const float PumpFlowLength = 624f;

        /// <inheritdoc cref="ActivePhysicsConstants.SpiderTraversalSpeed" />
        public const float SpiderTraversalSpeed = 117f;

        /// <inheritdoc cref="ActivePhysicsConstants.MouthOpenDistance" />
        public const float MouthOpenDistance = 200f;

        /// <inheritdoc cref="ActivePhysicsConstants.SpikesCollisionBandHalfHeight" />
        public const float SpikesCollisionBandHalfHeight = 5f;

        /// <inheritdoc cref="ActivePhysicsConstants.ElectroSpikesWidthReduction" />
        public const float ElectroSpikesWidthReduction = 400f;

        /// <summary>
        /// Original XML quad widths for static spikes 1-4 (obj_spikes_01..04). The JSON atlas is
        /// re-trimmed (+2 px per frame), so collision reads these instead of the live texture.
        /// </summary>
        public static readonly float[] SpikesQuadWidths = [212f, 333f, 453f, 566f];

        /// <inheritdoc cref="ActivePhysicsConstants.SpikesCollisionLineWidth" />
        public static readonly float[] RotatableSpikesQuadWidths = [202f, 319f, 444f, 559f];

        /// <summary>
        /// Original XML pre-cut width of obj_electrodes; the electro zap length is this minus
        /// <see cref="ElectroSpikesWidthReduction"/>.
        /// </summary>
        public const float ElectroSpikesObjectWidth = 833f;

        /// <summary>
        /// Original XML first-quad width of the small bouncer (obj_bouncer_01, quad 0).
        /// </summary>
        public const float BouncerSmallCollisionWidth = 194f;

        /// <inheritdoc cref="ActivePhysicsConstants.BouncerCollisionWidth" />
        /// <remarks>obj_bouncer_02 quad 0.</remarks>
        public const float BouncerLargeCollisionWidth = 302f;

        /// <summary>
        /// Rocket catch-slat box frozen from the JSON atlas: quad 10 is 358x179 centered at
        /// (288, 208.5) on the 619x418 obj_rocket sheet; the engine takes 0.6 x width and
        /// 0.05 x height of that quad.
        /// </summary>
        public const float RocketCatchBoxWidth = 214.8f; // 358 * 0.6

        /// <inheritdoc cref="ActivePhysicsConstants.RocketCatchBoxHeight" />
        public const float RocketCatchBoxHeight = 8.95f; // 179 * 0.05

        /// <inheritdoc cref="ActivePhysicsConstants.RocketCatchBoxCenterOffsetX" />
        public const float RocketCatchBoxCenterOffsetX = -21.5f; // 288 - 619/2

        /// <inheritdoc cref="ActivePhysicsConstants.RocketCatchBoxCenterOffsetY" />
        public const float RocketCatchBoxCenterOffsetY = -0.5f; // 208.5 - 418/2

        /// <summary>
        /// Maximum rope length used to size rope drawing buffers.
        /// </summary>
        public const float MaxRopeLength = 2000f;

        /// <inheritdoc cref="ActivePhysicsConstants.DrawPtsBufferSize" />
        public static readonly int DrawPtsBufferSize =
            (int)(((MaxRopeLength / BungeeRestLength) + 3) * 4 * 2); // = 176
    }
}
