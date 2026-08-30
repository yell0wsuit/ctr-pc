namespace CutTheRopeDX.Framework
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
        /// Gets or sets a value indicating whether the loaded map uses Cut the Rope: Time Travel's
        /// rocket, rather than the Experiments rocket the rest of the port descends from. Set once
        /// per level load, alongside <see cref="UseMobilePhysicsModel"/>.
        /// </summary>
        public static bool UseTimeTravelRocketModel { get; set; }

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
        /// Raw level-space radius used when a bamboo tube captures the candy center.
        /// The tube applies the level's interaction scale after selecting the physics model.
        /// </summary>
        public static float BambooCaptureRadius => SelectRaw(PhysicsConstants.BambooCaptureRadius, MobilePhysicsConstants.BambooCaptureRadius);

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
        /// Half-size of the candy collision box used when a sock catches the candy.
        /// </summary>
        public static float SockCatchHalfSize => SelectScaled(PhysicsConstants.SockCatchHalfSize, MobilePhysicsConstants.SockCatchHalfSize);

        /// <summary>
        /// Vertical offset applied to the candy when it exits a sock.
        /// </summary>
        public static float SockExitOffsetY => SelectScaled(PhysicsConstants.SockExitOffsetY, MobilePhysicsConstants.SockExitOffsetY);

        /// <summary>
        /// Maximum rope roll length used by grab mechanics.
        /// </summary>
        public static float GrabRopeRollMaxLength => SelectScaled(PhysicsConstants.GrabRopeRollMaxLength, MobilePhysicsConstants.GrabRopeRollMaxLength);

        /// <summary>
        /// Maximum rope length rolled per wheel rotate event.
        /// </summary>
        public static float GrabWheelRotateDeltaMax => SelectScaled(PhysicsConstants.GrabWheelRotateDeltaMax, MobilePhysicsConstants.GrabWheelRotateDeltaMax);

        /// <summary>
        /// Minimum rope length rolled per wheel rotate event.
        /// </summary>
        public static float GrabWheelRotateDeltaMin => SelectScaled(PhysicsConstants.GrabWheelRotateDeltaMin, MobilePhysicsConstants.GrabWheelRotateDeltaMin);

        /// <summary>
        /// Speed at which a rocket reels the candy in before flying.
        /// </summary>
        public static float RocketReelSpeed => UseTimeTravelRocketModel
            ? PhysicsConstants.TimeTravelRocketReelSpeed
            : SelectScaled(PhysicsConstants.RocketReelSpeed, MobilePhysicsConstants.RocketReelSpeed);

        /// <summary>
        /// Speed at which two candy halves converge while merging.
        /// </summary>
        public static float CandyPartsMergeSpeed => SelectScaled(PhysicsConstants.CandyPartsMergeSpeed, MobilePhysicsConstants.CandyPartsMergeSpeed);

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
        public static float RocketPointWeight => UseTimeTravelRocketModel
            ? PhysicsConstants.TimeTravelRocketPointWeight
            : SelectRaw(PhysicsConstants.RocketPointWeight, MobilePhysicsConstants.RocketPointWeight);

        /// <summary>
        /// Velocity damping applied while a rocket is active. Time Travel populates no force slot
        /// on any point, so it damps nothing - see <see cref="RocketDampsCandyVelocity"/>.
        /// </summary>
        public static float RocketActiveVelocityDamping => SelectRaw(
            PhysicsConstants.RocketActiveVelocityDamping,
            MobilePhysicsConstants.RocketActiveVelocityDamping);

        /// <summary>
        /// Impulse scale applied to rocket thrust. Time Travel and mobile Experiments author their
        /// impulse values in level coordinates; desktop Experiments values are already world-tuned.
        /// </summary>
        public static float RocketImpulseScale => UseTimeTravelRocketModel || UseMobilePhysicsModel
            ? Wp7ToWorldScale
            : 1f;

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
        /// Exponent (per world unit) of the steam force falloff beyond the column top.
        /// </summary>
        public static float SteamTubeFalloffExponent => SelectRaw(PhysicsConstants.SteamTubeFalloffExponent, MobilePhysicsConstants.SteamTubeFalloffExponent);

        /// <summary>
        /// Horizontal velocity below which the steam column cancels velocity outright.
        /// </summary>
        public static float SteamTubeVelocityDeadzone => SelectScaled(PhysicsConstants.SteamTubeVelocityDeadzone, MobilePhysicsConstants.SteamTubeVelocityDeadzone);

        /// <summary>
        /// Distance from a lantern at which it captures the candy.
        /// </summary>
        public static float LanternCaptureRadius => SelectScaled(PhysicsConstants.LanternCaptureRadius, MobilePhysicsConstants.LanternCaptureRadius);

        /// <summary>
        /// Length of the pump's air flow column.
        /// </summary>
        public static float PumpFlowLength => SelectScaled(PhysicsConstants.PumpFlowLength, MobilePhysicsConstants.PumpFlowLength);

        /// <summary>
        /// Spider's traversal speed.
        /// </summary>
        public static float SpiderTraversalSpeed => SelectRaw(PhysicsConstants.SpiderTraversalSpeed, MobilePhysicsConstants.SpiderTraversalSpeed);

        /// <summary>
        /// Distance from Om Nom at which the candy makes him open his mouth.
        /// </summary>
        public static float MouthOpenDistance => SelectScaled(PhysicsConstants.MouthOpenDistance, MobilePhysicsConstants.MouthOpenDistance);

        /// <summary>
        /// Half-height of the spikes collision band around the spike line.
        /// </summary>
        public static float SpikesCollisionBandHalfHeight => SelectScaled(PhysicsConstants.SpikesCollisionBandHalfHeight, MobilePhysicsConstants.SpikesCollisionBandHalfHeight);

        /// <summary>
        /// Amount subtracted from an electro spike's width to get the active zap segment length.
        /// </summary>
        public static float ElectroSpikesWidthReduction => SelectScaled(PhysicsConstants.ElectroSpikesWidthReduction, MobilePhysicsConstants.ElectroSpikesWidthReduction);

        /// <summary>
        /// Full collision line width for a non-electro spike, table-driven from the original
        /// XML quad. Never read from
        /// the live texture: the JSON atlas trim differs from both originals, so atlas-derived
        /// widths silently change collision whenever art is re-packed.
        /// </summary>
        /// <param name="rotatable">Whether the spike belongs to a rotate-toggle group.</param>
        /// <param name="widthIndex">Spike width/type index (1-4).</param>
        public static float SpikesCollisionLineWidth(bool rotatable, int widthIndex)
        {
            float[] table = UseMobilePhysicsModel
                ? rotatable ? MobilePhysicsConstants.RotatableSpikesQuadWidths : MobilePhysicsConstants.SpikesQuadWidths
                : rotatable ? PhysicsConstants.RotatableSpikesQuadWidths : PhysicsConstants.SpikesQuadWidths;
            int index = System.Math.Clamp(widthIndex - 1, 0, table.Length - 1);
            return UseMobilePhysicsModel ? ToWorld(table[index]) : table[index];
        }

        /// <summary>
        /// Effective electro spike object width the zap length is derived from
        /// (zap = this minus <see cref="ElectroSpikesWidthReduction"/>).
        /// </summary>
        public static float ElectroSpikesCollisionObjectWidth()
        {
            return SelectScaled(PhysicsConstants.ElectroSpikesObjectWidth, MobilePhysicsConstants.ElectroSpikesObjectWidth);
        }

        /// <summary>
        /// Full collision width of a bouncer, pinned from the original XML first quad (see
        /// <see cref="SpikesCollisionLineWidth"/>).
        /// </summary>
        /// <param name="large">Whether this is the large (type 2) bouncer.</param>
        public static float BouncerCollisionWidth(bool large)
        {
            return large
                ? SelectScaled(PhysicsConstants.BouncerLargeCollisionWidth, MobilePhysicsConstants.BouncerLargeCollisionWidth)
                : SelectScaled(PhysicsConstants.BouncerSmallCollisionWidth, MobilePhysicsConstants.BouncerSmallCollisionWidth);
        }

        /// <summary>
        /// Width of the rocket's catch-slat bounding box (0.65 x the rocket body quad width),
        /// pinned from the original XML quads rather than the live atlas.
        /// </summary>
        public static float RocketCatchBoxWidth => UseTimeTravelRocketModel
            ? PhysicsConstants.TimeTravelRocketCatchBoxWidth
            : SelectScaled(PhysicsConstants.RocketCatchBoxWidth, MobilePhysicsConstants.RocketCatchBoxWidth);

        /// <summary>
        /// Height of the rocket's catch-slat bounding box (0.05 x the rocket body quad height).
        /// </summary>
        public static float RocketCatchBoxHeight => UseTimeTravelRocketModel
            ? PhysicsConstants.TimeTravelRocketCatchBoxHeight
            : SelectScaled(PhysicsConstants.RocketCatchBoxHeight, MobilePhysicsConstants.RocketCatchBoxHeight);

        /// <summary>
        /// X offset of the catch-slat box center from the rocket object's center.
        /// </summary>
        public static float RocketCatchBoxCenterOffsetX => UseTimeTravelRocketModel
            ? PhysicsConstants.TimeTravelRocketCatchBoxCenterOffsetX
            : SelectScaled(PhysicsConstants.RocketCatchBoxCenterOffsetX, MobilePhysicsConstants.RocketCatchBoxCenterOffsetX);

        /// <summary>
        /// Y offset of the catch-slat box center from the rocket object's center.
        /// </summary>
        public static float RocketCatchBoxCenterOffsetY => UseTimeTravelRocketModel
            ? PhysicsConstants.TimeTravelRocketCatchBoxCenterOffsetY
            : SelectScaled(PhysicsConstants.RocketCatchBoxCenterOffsetY, MobilePhysicsConstants.RocketCatchBoxCenterOffsetY);

        /// <summary>
        /// Draw scale applied to the rocket body, matching the sheet each game's quad came from.
        /// </summary>
        public static float RocketBodyScale => UseTimeTravelRocketModel
            ? PhysicsConstants.TimeTravelRocketBodyScale
            : PhysicsConstants.RocketBodyScale;

        /// <summary>
        /// Floor applied to the per-frame rocket travel distance that drives exhaust particle speed.
        /// </summary>
        public static float RocketExhaustSpeedFloor => UseTimeTravelRocketModel
            ? PhysicsConstants.TimeTravelRocketExhaustSpeedFloor
            : PhysicsConstants.RocketExhaustSpeedFloor;

        /// <summary>
        /// Gets a value indicating whether the rocket and candy points are relaxed while the rocket
        /// is already flying. Time Travel relaxes them only during the reel-in
        /// (<c>STATE_ROCKET_DIST</c>) phase.
        /// </summary>
        public static bool RocketRelaxDuringFlight => !UseTimeTravelRocketModel;

        /// <summary>
        /// Gets a value indicating whether a held candy suppresses rope-perpendicular steering.
        /// Time Travel steers off any uncut, relaxed rope regardless of who holds the candy.
        /// </summary>
        public static bool RocketRopeAlignRequiresFreeCandy => !UseTimeTravelRocketModel;

        /// <summary>
        /// Gets a value indicating whether catching a candy in a bubble pops it. Time Travel's
        /// bind bursts the bubble before it takes the candy.
        /// </summary>
        public static bool RocketBindPopsCandyBubble => UseTimeTravelRocketModel;

        /// <summary>
        /// Gets a value indicating whether a rocket binds straight into flight when something is
        /// already holding the candy. Time Travel always reels in from wherever the rocket caught
        /// it, no matter who is holding it.
        /// </summary>
        public static bool RocketBindsDirectlyToFlightWhenHeld => !UseTimeTravelRocketModel;

        /// <summary>
        /// Gets a value indicating whether binding a rocket cancels the candy's velocity outright.
        /// Time Travel snaps <c>prevPos</c> onto <c>pos</c>; Experiments bleeds off a fraction.
        /// </summary>
        public static bool RocketBindClearsCandyVelocity => UseTimeTravelRocketModel;

        /// <summary>
        /// Gets a value indicating whether a rocket-bound candy has its velocity damped every frame.
        /// Time Travel populates no force slot on any point, so its thrust builds unopposed.
        /// </summary>
        public static bool RocketDampsCandyVelocity => !UseTimeTravelRocketModel;

        /// <summary>
        /// Gets a value indicating whether each candy point is relaxed immediately after it is
        /// integrated, and the candy connector's own endpoints once every candy has moved. Time
        /// Travel does both in its simulation step; the Experiments path leaves that to the
        /// bungee's own relaxation pass.
        /// </summary>
        public static bool RelaxCandyPointsAfterIntegration => UseTimeTravelRocketModel;

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
