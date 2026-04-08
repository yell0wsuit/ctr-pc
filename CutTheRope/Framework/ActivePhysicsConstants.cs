namespace CutTheRope.Framework
{
    /// <summary>
    /// Runtime-selected physics constants.
    /// Applies raw Windows Phone constants transformed into PC world units, with PC fallback.
    /// </summary>
    internal static class ActivePhysicsConstants
    {
        /// <summary>
        /// Gets or sets a value indicating whether mobile physics tuning should be used.
        /// </summary>
        public static bool UseMobilePhysicsModel { get; set; }

        /// <summary>
        /// Scale factor between Windows Phone coordinate units and desktop world units.
        /// </summary>
        public const float Wp7ToWorldScale = 3f;

        /// <summary>
        /// Mover speed scale used by the desktop physics tuning.
        /// </summary>
        public const float DesktopMoverSpeedScale = 3.3f;

        /// <summary>
        /// Converts a Windows Phone coordinate-space <paramref name="value"/> to desktop world units.
        /// </summary>
        /// <param name="value">Value in Windows Phone coordinate units.</param>
        /// <returns>The <paramref name="value"/> scaled into desktop world units.</returns>
        private static float ToWorld(float value)
        {
            return value * Wp7ToWorldScale;
        }

        /// <summary>
        /// Selects one of two raw floating-point tuning values.
        /// </summary>
        /// <param name="pc">Desktop tuning value.</param>
        /// <param name="mobile">Mobile tuning value.</param>
        /// <returns>The active raw tuning value.</returns>
        private static float SelectRaw(float pc, float mobile)
        {
            return UseMobilePhysicsModel ? mobile : pc;
        }

        /// <summary>
        /// Selects a floating-point tuning value, scaling <paramref name="mobile"/> values to desktop world units.
        /// </summary>
        /// <param name="pc">Desktop tuning value.</param>
        /// <param name="mobile">Mobile tuning value.</param>
        /// <returns>The active tuning value in desktop world units.</returns>
        private static float SelectScaled(float pc, float mobile)
        {
            return UseMobilePhysicsModel ? ToWorld(mobile) : pc;
        }

        /// <summary>
        /// Selects one of two raw integer tuning values.
        /// </summary>
        /// <param name="pc">Desktop tuning value.</param>
        /// <param name="mobile">Mobile tuning value.</param>
        /// <returns>The active raw tuning value.</returns>
        private static int SelectRaw(int pc, int mobile)
        {
            return UseMobilePhysicsModel ? mobile : pc;
        }

        /// <summary>
        /// Simulation timestep scale applied to physics updates.
        /// </summary>
        public static float TimeScale => SelectRaw(PhysicsConstants.TimeScale, MobilePhysicsConstants.TimeScale);

        /// <summary>
        /// Vertical gravity acceleration applied to physics bodies.
        /// </summary>
        public static float GravityEarthY => SelectScaled(PhysicsConstants.GravityEarthY, MobilePhysicsConstants.GravityEarthY);

        /// <summary>
        /// Speed multiplier applied to rope physics updates.
        /// </summary>
        public static float RopePhysicsSpeedMultiplier => SelectRaw(PhysicsConstants.RopePhysicsSpeedMultiplier, MobilePhysicsConstants.RopePhysicsSpeedMultiplier);

        /// <summary>
        /// Rest length used by bungee rope constraints.
        /// </summary>
        public static float BungeeRestLength => SelectScaled(PhysicsConstants.BungeeRestLength, MobilePhysicsConstants.BungeeRestLength);

        /// <summary>
        /// Extra rollback padding allowed when a bungee stretches past its limit.
        /// </summary>
        public static float BungeeRollBackOverflowPadding => SelectScaled(PhysicsConstants.BungeeRollBackOverflowPadding, MobilePhysicsConstants.BungeeRollBackOverflowPadding);

        /// <summary>
        /// Slack distance allowed in bungee constraints.
        /// </summary>
        public static float BungeeConstraintSlack => SelectScaled(PhysicsConstants.BungeeConstraintSlack, MobilePhysicsConstants.BungeeConstraintSlack);

        /// <summary>
        /// Soft relaxation threshold for bungee constraints.
        /// </summary>
        public static float BungeeRelaxThresholdSoft => SelectScaled(PhysicsConstants.BungeeRelaxThresholdSoft, MobilePhysicsConstants.BungeeRelaxThresholdSoft);

        /// <summary>
        /// Medium relaxation threshold for bungee constraints.
        /// </summary>
        public static float BungeeRelaxThresholdMedium => SelectScaled(PhysicsConstants.BungeeRelaxThresholdMedium, MobilePhysicsConstants.BungeeRelaxThresholdMedium);

        /// <summary>
        /// Hard relaxation threshold for bungee constraints.
        /// </summary>
        public static float BungeeRelaxThresholdHard => SelectScaled(PhysicsConstants.BungeeRelaxThresholdHard, MobilePhysicsConstants.BungeeRelaxThresholdHard);

        /// <summary>
        /// Stretch threshold at which bungee ropes render in the warning state.
        /// </summary>
        public static float BungeeStretchRedThreshold => SelectScaled(PhysicsConstants.BungeeStretchRedThreshold, MobilePhysicsConstants.BungeeStretchRedThreshold);

        /// <summary>
        /// Upward impulse applied by bubbles.
        /// </summary>
        public static float BubbleImpulseY => SelectScaled(PhysicsConstants.BubbleImpulseY, MobilePhysicsConstants.BubbleImpulseY);

        /// <summary>
        /// Damping applied while a bubble carries the candy.
        /// </summary>
        public static float BubbleImpulseDamping => SelectRaw(PhysicsConstants.BubbleImpulseDamping, MobilePhysicsConstants.BubbleImpulseDamping);

        /// <summary>
        /// Radius used when a bubble captures the candy.
        /// </summary>
        public static float BubbleCaptureRadius => SelectScaled(PhysicsConstants.BubbleCaptureRadius, MobilePhysicsConstants.BubbleCaptureRadius);

        /// <summary>
        /// Gravity acceleration applied to candy break particles.
        /// </summary>
        public static float CandyBreakGravityY => SelectScaled(PhysicsConstants.CandyBreakGravityY, MobilePhysicsConstants.CandyBreakGravityY);

        /// <summary>
        /// Padding used around candy grab interactions.
        /// </summary>
        public static float CandyGrabPadding => SelectRaw(PhysicsConstants.CandyGrabPadding, MobilePhysicsConstants.CandyGrabPadding);

        /// <summary>
        /// Speed multiplier used when teleporting through socks.
        /// </summary>
        public static float SockTeleportSpeedMultiplier => SelectRaw(PhysicsConstants.SockTeleportSpeedMultiplier, MobilePhysicsConstants.SockTeleportSpeedMultiplier);

        /// <summary>
        /// Speed coefficient applied to sock movement.
        /// </summary>
        public static float SockSpeedKoeff => SelectRaw(PhysicsConstants.SockSpeedKoeff, MobilePhysicsConstants.SockSpeedKoeff);

        /// <summary>
        /// Maximum rope roll length used by grab mechanics.
        /// </summary>
        public static float GrabRopeRollMaxLength => SelectScaled(PhysicsConstants.GrabRopeRollMaxLength, MobilePhysicsConstants.GrabRopeRollMaxLength);

        /// <summary>
        /// Maximum wheel rotation delta used by grab mechanics.
        /// </summary>
        public static float GrabWheelRotateDeltaMax => SelectRaw(PhysicsConstants.GrabWheelRotateDeltaMax, MobilePhysicsConstants.GrabWheelRotateDeltaMax);

        /// <summary>
        /// Height band used for detecting the water surface.
        /// </summary>
        public static float WaterSurfaceDetectionHeight => SelectScaled(PhysicsConstants.WaterSurfaceDetectionHeight, MobilePhysicsConstants.WaterSurfaceDetectionHeight);

        /// <summary>
        /// Vertical offset applied when spawning water splash particles.
        /// </summary>
        public static float WaterSplashParticleYOffset => SelectScaled(PhysicsConstants.WaterSplashParticleYOffset, MobilePhysicsConstants.WaterSplashParticleYOffset);

        /// <summary>
        /// Collision radius used when candy interacts with water.
        /// </summary>
        public static float WaterCandyCollisionRadius => SelectScaled(PhysicsConstants.WaterCandyCollisionRadius, MobilePhysicsConstants.WaterCandyCollisionRadius);

        /// <summary>
        /// Damping applied to bodies moving through water.
        /// </summary>
        public static float WaterDamping => SelectRaw(PhysicsConstants.WaterDamping, MobilePhysicsConstants.WaterDamping);

        /// <summary>
        /// Base vertical impulse applied by water interactions.
        /// </summary>
        public static float WaterVerticalImpulseBase => SelectScaled(PhysicsConstants.WaterVerticalImpulseBase, MobilePhysicsConstants.WaterVerticalImpulseBase);

        /// <summary>
        /// Divisor applied to rocket impulse while interacting with water.
        /// </summary>
        public static float WaterRocketImpulseDivisor => SelectRaw(PhysicsConstants.WaterRocketImpulseDivisor, MobilePhysicsConstants.WaterRocketImpulseDivisor);

        /// <summary>
        /// Damping multiplier applied to rockets while interacting with water.
        /// </summary>
        public static float WaterRocketDampingMultiplier => SelectRaw(PhysicsConstants.WaterRocketDampingMultiplier, MobilePhysicsConstants.WaterRocketDampingMultiplier);

        /// <summary>
        /// Impulse applied to rope anchors while interacting with water.
        /// </summary>
        public static float WaterRopeAnchorImpulse => SelectScaled(PhysicsConstants.WaterRopeAnchorImpulse, MobilePhysicsConstants.WaterRopeAnchorImpulse);

        /// <summary>
        /// Collision radius used by bouncers.
        /// </summary>
        public static float BouncerCollisionRadius => SelectScaled(PhysicsConstants.BouncerCollisionRadius, MobilePhysicsConstants.BouncerCollisionRadius);

        /// <summary>
        /// Height offset used by bouncers.
        /// </summary>
        public static float BouncerHeight => SelectScaled(PhysicsConstants.BouncerHeight, MobilePhysicsConstants.BouncerHeight);

        /// <summary>
        /// Velocity scale applied to bouncer impulses.
        /// </summary>
        public static float BouncerImpulseVelocityScale => SelectRaw(PhysicsConstants.BouncerImpulseVelocityScale, MobilePhysicsConstants.BouncerImpulseVelocityScale);

        /// <summary>
        /// Minimum impulse applied by bouncers.
        /// </summary>
        public static float BouncerMinImpulse => SelectScaled(PhysicsConstants.BouncerMinImpulse, MobilePhysicsConstants.BouncerMinImpulse);

        /// <summary>
        /// Physics weight assigned to rocket control points.
        /// </summary>
        public static float RocketPointWeight => SelectRaw(PhysicsConstants.RocketPointWeight, MobilePhysicsConstants.RocketPointWeight);

        /// <summary>
        /// Velocity damping applied while a rocket is active.
        /// </summary>
        public static float RocketActiveVelocityDamping => SelectRaw(PhysicsConstants.RocketActiveVelocityDamping, MobilePhysicsConstants.RocketActiveVelocityDamping);

        /// <summary>
        /// Impulse scale applied to rocket thrust.
        /// </summary>
        public static float RocketImpulseScale => UseMobilePhysicsModel ? Wp7ToWorldScale : 1f;

        /// <summary>
        /// Scale applied to mover path coordinates.
        /// </summary>
        public static float MoverPathScale => Wp7ToWorldScale;

        /// <summary>
        /// Speed scale applied to mover traversal.
        /// </summary>
        public static float MoverSpeedScale => UseMobilePhysicsModel ? Wp7ToWorldScale : DesktopMoverSpeedScale;

        /// <summary>
        /// Damping applied by steam tubes.
        /// </summary>
        public static float SteamTubeDamping => SelectRaw(PhysicsConstants.SteamTubeDamping, MobilePhysicsConstants.SteamTubeDamping);

        /// <summary>
        /// Additional damping multiplier applied when a body is not aligned with a steam tube.
        /// </summary>
        public static float SteamTubeNonAlignedDampingMultiplier => SelectRaw(PhysicsConstants.SteamTubeNonAlignedDampingMultiplier, MobilePhysicsConstants.SteamTubeNonAlignedDampingMultiplier);

        /// <summary>
        /// Width scale used by steam tube force volumes.
        /// </summary>
        public static float SteamTubeWidthScale => SelectRaw(PhysicsConstants.SteamTubeWidthScale, MobilePhysicsConstants.SteamTubeWidthScale);

        /// <summary>
        /// Vertical offset scale used by steam tube force volumes.
        /// </summary>
        public static float SteamTubeVerticalOffsetScale => SelectRaw(PhysicsConstants.SteamTubeVerticalOffsetScale, MobilePhysicsConstants.SteamTubeVerticalOffsetScale);

        /// <summary>
        /// Collision radius scale used by steam tube force volumes.
        /// </summary>
        public static float SteamTubeCollisionRadiusScale => SelectRaw(PhysicsConstants.SteamTubeCollisionRadiusScale, MobilePhysicsConstants.SteamTubeCollisionRadiusScale);

        /// <summary>
        /// Gravity compensation applied by steam tubes.
        /// </summary>
        public static float SteamTubeGravityCompensation => SelectRaw(PhysicsConstants.SteamTubeGravityCompensation, MobilePhysicsConstants.SteamTubeGravityCompensation);

        /// <summary>
        /// Gravity divisor used for side-aligned steam tube forces.
        /// </summary>
        public static float SteamTubeSideGravityDivisor => SelectRaw(PhysicsConstants.SteamTubeSideGravityDivisor, MobilePhysicsConstants.SteamTubeSideGravityDivisor);

        /// <summary>
        /// Gravity divisor used for opposite-direction steam tube forces.
        /// </summary>
        public static float SteamTubeOppositeGravityDivisor => SelectRaw(PhysicsConstants.SteamTubeOppositeGravityDivisor, MobilePhysicsConstants.SteamTubeOppositeGravityDivisor);

        /// <summary>
        /// Spider's traversal speed.
        /// </summary>
        public static float SpiderTraversalSpeed => SelectRaw(PhysicsConstants.SpiderTraversalSpeed, MobilePhysicsConstants.SpiderTraversalSpeed);

        /// <summary>
        /// Number of sample points drawn for each bungee segment.
        /// </summary>
        public static int BungeeDrawSamplePoints => SelectRaw(PhysicsConstants.BungeeDrawSamplePoints, MobilePhysicsConstants.BungeeDrawSamplePoints);

        /// <summary>
        /// Number of float entries allocated for rope drawing point buffers.
        /// </summary>
        public static int DrawPtsBufferSize => UseMobilePhysicsModel
            ? MobilePhysicsConstants.DrawPtsBufferSize
            : PhysicsConstants.DrawPtsBufferSize;
    }
}
