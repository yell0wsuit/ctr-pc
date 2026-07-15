namespace CutTheRopeDX.Framework
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

        /// <inheritdoc cref="ActivePhysicsConstants.SockCatchHalfSize" />
        public const float SockCatchHalfSize = 10f;

        /// <inheritdoc cref="ActivePhysicsConstants.SockExitOffsetY" />
        public const float SockExitOffsetY = -8f;

        /// <inheritdoc cref="ActivePhysicsConstants.GrabRopeRollMaxLength" />
        public const float GrabRopeRollMaxLength = 500f;

        /// <inheritdoc cref="ActivePhysicsConstants.GrabWheelRotateDeltaMax" />
        public const float GrabWheelRotateDeltaMax = 2f;

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

        /// <inheritdoc cref="ActivePhysicsConstants.SteamTubeFalloffExponent" />
        public const float SteamTubeFalloffExponent = -2f / 3f; // WP7 -2 per mobile unit

        /// <inheritdoc cref="ActivePhysicsConstants.SteamTubeVelocityDeadzone" />
        public const float SteamTubeVelocityDeadzone = 1f;

        /// <inheritdoc cref="ActivePhysicsConstants.LanternCaptureRadius" />
        public const float LanternCaptureRadius = 32f;

        /// <inheritdoc cref="ActivePhysicsConstants.PumpFlowLength" />
        public const float PumpFlowLength = 200f;

        /// <inheritdoc cref="ActivePhysicsConstants.SpiderTraversalSpeed" />
        public const float SpiderTraversalSpeed = 135f; // 45 * 3

        /// <inheritdoc cref="ActivePhysicsConstants.MouthOpenDistance" />
        public const float MouthOpenDistance = 100f;

        /// <inheritdoc cref="ActivePhysicsConstants.SpikesCollisionBandHalfHeight" />
        public const float SpikesCollisionBandHalfHeight = 5f;

        /// <inheritdoc cref="ActivePhysicsConstants.ElectroSpikesWidthReduction" />
        public const float ElectroSpikesWidthReduction = 130f;

        /// <summary>
        /// WP7 base-asset quad widths for static spikes 1-4 (obj_spikes_01..04). The desktop
        /// atlas is trimmed differently (214/335/455/568 at 3x), so mobile physics reads these.
        /// </summary>
        public static readonly float[] SpikesQuadWidths = [68f, 106f, 146f, 181f];

        /// <inheritdoc cref="ActivePhysicsConstants.SpikesCollisionLineWidth" />
        public static readonly float[] RotatableSpikesQuadWidths = [68f, 118f, 142f, 178f];

        /// <summary>
        /// WP7 pre-cut width of obj_electrodes; the electro zap length is this minus
        /// <see cref="ElectroSpikesWidthReduction"/>. The JSON electrodes sheet is 833 wide, not 267x3.
        /// </summary>
        public const float ElectroSpikesObjectWidth = 267f;

        /// <summary>
        /// WP7 first-quad width of the small bouncer (obj_bouncer_01, quad 0), subtract 20.
        /// </summary>
        public const float BouncerSmallCollisionWidth = 46f;

        /// <inheritdoc cref="ActivePhysicsConstants.BouncerCollisionWidth" />
        /// <remarks>obj_bouncer_02 quad 0, subtract 20</remarks>
        public const float BouncerLargeCollisionWidth = 91f;

        /// <summary>
        /// Rocket catch-slat box from the Experiments base assets: quad 10 is 116x58 centered
        /// at (91,67) on the 199x134 obj_rocket sheet; the engine takes 0.6 x width and
        /// 0.05 x height of that quad.
        /// </summary>
        public const float RocketCatchBoxWidth = 69.6f; // 116 * 0.6

        /// <inheritdoc cref="ActivePhysicsConstants.RocketCatchBoxHeight" />
        public const float RocketCatchBoxHeight = 2.9f; // 58 * 0.05

        /// <inheritdoc cref="ActivePhysicsConstants.RocketCatchBoxCenterOffsetX" />
        public const float RocketCatchBoxCenterOffsetX = -8.5f; // 91 - 199/2

        /// <inheritdoc cref="ActivePhysicsConstants.RocketCatchBoxCenterOffsetY" />
        public const float RocketCatchBoxCenterOffsetY = 0f; // 67 - 134/2

        /// <summary>
        /// Maximum rope length used by the mobile bungee renderer.
        /// </summary>
        public const float MaxRopeLength = 600f; // 20 segments * 30 rest length

        /// <inheritdoc cref="ActivePhysicsConstants.DrawPtsBufferSize" />
        public static readonly int DrawPtsBufferSize = 200;
    }
}
