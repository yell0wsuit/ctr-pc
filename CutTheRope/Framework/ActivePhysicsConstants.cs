namespace CutTheRope.Framework
{
    /// <summary>
    /// Runtime-selected physics constants.
    /// Applies raw Windows Phone constants transformed into PC world units, with PC fallback.
    /// </summary>
    internal static class ActivePhysicsConstants
    {
        public static bool UseMobilePhysicsModel { get; set; }

        // PC world scale relative to raw Windows Phone coordinates.
        public const float Wp7ToWorldScale = 3f;
        // Mover speed used by original PC tuning.
        public const float DesktopMoverSpeedScale = 3.3f;

        private static float ToWorld(float value)
        {
            return value * Wp7ToWorldScale;
        }

        private static float SelectRaw(float pc, float mobile)
        {
            return UseMobilePhysicsModel ? mobile : pc;
        }

        private static float SelectScaled(float pc, float mobile)
        {
            return UseMobilePhysicsModel ? ToWorld(mobile) : pc;
        }

        private static int SelectRaw(int pc, int mobile)
        {
            return UseMobilePhysicsModel ? mobile : pc;
        }

        public static float TimeScale => SelectRaw(PhysicsConstants.TimeScale, MobilePhysicsConstants.TimeScale);
        public static float GravityEarthY => SelectScaled(PhysicsConstants.GravityEarthY, MobilePhysicsConstants.GravityEarthY);
        public static float RopePhysicsSpeedMultiplier => SelectRaw(PhysicsConstants.RopePhysicsSpeedMultiplier, MobilePhysicsConstants.RopePhysicsSpeedMultiplier);
        public static float BungeeRestLength => SelectScaled(PhysicsConstants.BungeeRestLength, MobilePhysicsConstants.BungeeRestLength);
        public static float BungeeRollBackOverflowPadding => SelectScaled(PhysicsConstants.BungeeRollBackOverflowPadding, MobilePhysicsConstants.BungeeRollBackOverflowPadding);
        public static float BungeeConstraintSlack => SelectScaled(PhysicsConstants.BungeeConstraintSlack, MobilePhysicsConstants.BungeeConstraintSlack);
        public static float BungeeRelaxThresholdSoft => SelectScaled(PhysicsConstants.BungeeRelaxThresholdSoft, MobilePhysicsConstants.BungeeRelaxThresholdSoft);
        public static float BungeeRelaxThresholdMedium => SelectScaled(PhysicsConstants.BungeeRelaxThresholdMedium, MobilePhysicsConstants.BungeeRelaxThresholdMedium);
        public static float BungeeRelaxThresholdHard => SelectScaled(PhysicsConstants.BungeeRelaxThresholdHard, MobilePhysicsConstants.BungeeRelaxThresholdHard);
        public static float BungeeStretchRedThreshold => SelectScaled(PhysicsConstants.BungeeStretchRedThreshold, MobilePhysicsConstants.BungeeStretchRedThreshold);

        public static float BubbleImpulseY => SelectScaled(PhysicsConstants.BubbleImpulseY, MobilePhysicsConstants.BubbleImpulseY);
        public static float BubbleImpulseDamping => SelectRaw(PhysicsConstants.BubbleImpulseDamping, MobilePhysicsConstants.BubbleImpulseDamping);
        public static float BubbleCaptureRadius => SelectScaled(PhysicsConstants.BubbleCaptureRadius, MobilePhysicsConstants.BubbleCaptureRadius);

        public static float CandyBreakGravityY => SelectScaled(PhysicsConstants.CandyBreakGravityY, MobilePhysicsConstants.CandyBreakGravityY);
        public static float CandyGrabPadding => SelectRaw(PhysicsConstants.CandyGrabPadding, MobilePhysicsConstants.CandyGrabPadding);

        public static float SockTeleportSpeedMultiplier => SelectRaw(PhysicsConstants.SockTeleportSpeedMultiplier, MobilePhysicsConstants.SockTeleportSpeedMultiplier);
        public static float SockSpeedKoeff => SelectRaw(PhysicsConstants.SockSpeedKoeff, MobilePhysicsConstants.SockSpeedKoeff);
        public static float GrabRopeRollMaxLength => SelectScaled(PhysicsConstants.GrabRopeRollMaxLength, MobilePhysicsConstants.GrabRopeRollMaxLength);
        public static float GrabWheelRotateDeltaMax => SelectRaw(PhysicsConstants.GrabWheelRotateDeltaMax, MobilePhysicsConstants.GrabWheelRotateDeltaMax);

        public static float WaterSurfaceDetectionHeight => SelectScaled(PhysicsConstants.WaterSurfaceDetectionHeight, MobilePhysicsConstants.WaterSurfaceDetectionHeight);
        public static float WaterSplashParticleYOffset => SelectScaled(PhysicsConstants.WaterSplashParticleYOffset, MobilePhysicsConstants.WaterSplashParticleYOffset);
        public static float WaterCandyCollisionRadius => SelectScaled(PhysicsConstants.WaterCandyCollisionRadius, MobilePhysicsConstants.WaterCandyCollisionRadius);
        public static float WaterDamping => SelectRaw(PhysicsConstants.WaterDamping, MobilePhysicsConstants.WaterDamping);
        public static float WaterVerticalImpulseBase => SelectScaled(PhysicsConstants.WaterVerticalImpulseBase, MobilePhysicsConstants.WaterVerticalImpulseBase);
        public static float WaterRocketImpulseDivisor => SelectRaw(PhysicsConstants.WaterRocketImpulseDivisor, MobilePhysicsConstants.WaterRocketImpulseDivisor);
        public static float WaterRocketDampingMultiplier => SelectRaw(PhysicsConstants.WaterRocketDampingMultiplier, MobilePhysicsConstants.WaterRocketDampingMultiplier);
        public static float WaterRopeAnchorImpulse => SelectScaled(PhysicsConstants.WaterRopeAnchorImpulse, MobilePhysicsConstants.WaterRopeAnchorImpulse);

        public static float BouncerCollisionRadius => SelectScaled(PhysicsConstants.BouncerCollisionRadius, MobilePhysicsConstants.BouncerCollisionRadius);
        public static float BouncerHeight => SelectScaled(PhysicsConstants.BouncerHeight, MobilePhysicsConstants.BouncerHeight);
        public static float BouncerImpulseVelocityScale => SelectRaw(PhysicsConstants.BouncerImpulseVelocityScale, MobilePhysicsConstants.BouncerImpulseVelocityScale);
        public static float BouncerMinImpulse => SelectScaled(PhysicsConstants.BouncerMinImpulse, MobilePhysicsConstants.BouncerMinImpulse);

        public static float RocketPointWeight => SelectRaw(PhysicsConstants.RocketPointWeight, MobilePhysicsConstants.RocketPointWeight);
        public static float RocketActiveVelocityDamping => SelectRaw(PhysicsConstants.RocketActiveVelocityDamping, MobilePhysicsConstants.RocketActiveVelocityDamping);
        public static float RocketImpulseScale => UseMobilePhysicsModel ? Wp7ToWorldScale : 1f;

        public static float MoverPathScale => Wp7ToWorldScale;
        public static float MoverSpeedScale => UseMobilePhysicsModel ? Wp7ToWorldScale : DesktopMoverSpeedScale;

        // These are multiplied by per-object tubeScale in gameplay code.
        public static float SteamTubeDamping => SelectRaw(PhysicsConstants.SteamTubeDamping, MobilePhysicsConstants.SteamTubeDamping);
        public static float SteamTubeNonAlignedDampingMultiplier => SelectRaw(PhysicsConstants.SteamTubeNonAlignedDampingMultiplier, MobilePhysicsConstants.SteamTubeNonAlignedDampingMultiplier);
        public static float SteamTubeWidthScale => SelectRaw(PhysicsConstants.SteamTubeWidthScale, MobilePhysicsConstants.SteamTubeWidthScale);
        public static float SteamTubeVerticalOffsetScale => SelectRaw(PhysicsConstants.SteamTubeVerticalOffsetScale, MobilePhysicsConstants.SteamTubeVerticalOffsetScale);
        public static float SteamTubeCollisionRadiusScale => SelectRaw(PhysicsConstants.SteamTubeCollisionRadiusScale, MobilePhysicsConstants.SteamTubeCollisionRadiusScale);
        public static float SteamTubeGravityCompensation => SelectRaw(PhysicsConstants.SteamTubeGravityCompensation, MobilePhysicsConstants.SteamTubeGravityCompensation);
        public static float SteamTubeSideGravityDivisor => SelectRaw(PhysicsConstants.SteamTubeSideGravityDivisor, MobilePhysicsConstants.SteamTubeSideGravityDivisor);
        public static float SteamTubeOppositeGravityDivisor => SelectRaw(PhysicsConstants.SteamTubeOppositeGravityDivisor, MobilePhysicsConstants.SteamTubeOppositeGravityDivisor);

        public static float SpiderTraversalSpeed => SelectRaw(PhysicsConstants.SpiderTraversalSpeed, MobilePhysicsConstants.SpiderTraversalSpeed);
        public static int BungeeDrawSamplePoints => SelectRaw(PhysicsConstants.BungeeDrawSamplePoints, MobilePhysicsConstants.BungeeDrawSamplePoints);

        public static float ConveyorVelocityScale => SelectRaw(PhysicsConstants.ConveyorVelocityScale, MobilePhysicsConstants.ConveyorVelocityScale);

        public static int DrawPtsBufferSize => UseMobilePhysicsModel
            ? MobilePhysicsConstants.DrawPtsBufferSize
            : PhysicsConstants.DrawPtsBufferSize;
    }
}
