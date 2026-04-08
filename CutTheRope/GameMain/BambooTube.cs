using CutTheRope.Framework.Core;
using CutTheRope.Framework.Physics;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Rotatable bamboo tube teleporter. The candy enters one hole and exits the other.
    /// The tube can be rotated by dragging its ring zone or tapped in the centre to spin CCW.
    /// </summary>
    internal sealed class BambooTube : CTRGameObject
    {
        /// <summary>
        /// Initialises the tube at <paramref name="position"/> with an initial
        /// <paramref name="angle"/> (degrees) and an optional <paramref name="scale"/> factor
        /// that drives the bounding-box size and capture radius.
        /// </summary>
        /// <param name="position">World-space centre position of the tube.</param>
        /// <param name="angle">Initial rotation in degrees.</param>
        /// <param name="scale">Interaction scale factor that drives bounding-box size and capture radius.</param>
        /// <returns>This instance, for fluent initialisation.</returns>
        public BambooTube InitWithPositionAngle(Vector position, float angle, float scale = 1f)
        {
            _ = InitWithTexture(Application.GetTexture(Resources.Img.ObjBambooTube));
            DoRestoreCutTransparency();
            SetDrawQuad(BambooCoreQuad);
            anchor = 18;
            parentAnchor = 18;
            x = position.X;
            y = position.Y;
            bambooVisualRotation = angle;
            rotation = bambooVisualRotation;
            interactionScale = MAX(0.1f, scale);
            bb = MakeRectangle(0f, 0f, BambooBaseBbSize * interactionScale, BambooBaseBbSize * interactionScale);
            rotatedBB = true;
            scaleX = 0.9f;
            scaleY = 0.9f;
            SetupBambooShellSprites();
            BambooTouchIndex = -1;
            bambooHoleOut = vectZero;
            UpdateBambooRotation();
            return this;
        }

        /// <inheritdoc />
        public override void Update(float delta)
        {
            base.Update(delta);
            if (!isRotatingBamboo)
            {
                return;
            }

            float deltaAngle = bambooTargetRotation - bambooVisualRotation;
            float distanceToTarget = ABS(deltaAngle);
            if (distanceToTarget <= BambooRotationSnapThreshold)
            {
                bambooVisualRotation = bambooTargetRotation;
                isRotatingBamboo = false;
            }
            else
            {
                bambooVisualRotation += delta * (deltaAngle * DEG_360 / distanceToTarget);
            }

            UpdateBambooRotation();
        }

        /// <summary>
        /// Handles a touch-down on the tube.
        /// Tapping the inner zone (r &lt; bb.w/3) triggers a CCW snap.
        /// Touching the ring zone (bb.w/3 &lt; r &lt; bb.w/1.5) begins a free rotation drag.
        /// </summary>
        /// <param name="touchPoint">World-space touch position.</param>
        /// <param name="touchIndex">Platform touch index identifying this finger.</param>
        /// <returns><see langword="true"/> if the caller should track subsequent move/end events for this touch index.</returns>
        public bool HandleBambooTouchWithIndex(Vector touchPoint, int touchIndex)
        {
            if (isRotatingBamboo)
            {
                return false;
            }

            float distance = VectDistance(touchPoint, Vect(x, y));
            float touchRadius = bb.w;
            if (distance < touchRadius / 3f)
            {
                RotateBambooCounterClockwise();
                return false;
            }

            if (distance > touchRadius / 3f && distance < touchRadius / 1.5f)
            {
                bambooStartRotation = bambooVisualRotation;
                bambooLastTouch = touchPoint;
                BambooTouchIndex = touchIndex;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Updates the tube rotation in response to a touch-move event.
        /// Advances <c>bambooStartRotation</c> by 90° steps so the snap on release
        /// always targets the nearest 90° boundary from the gesture origin.
        /// </summary>
        /// <param name="touchPoint">Current world-space touch position.</param>
        public void HandleBambooRotate(Vector touchPoint)
        {
            float angleDelta = GetRotateAngleForStartEndCenter(bambooLastTouch, touchPoint, Vect(x, y));
            angleDelta = AngleTo180(angleDelta);
            bambooVisualRotation += angleDelta;

            float fromStart = AngleTo180(bambooVisualRotation - bambooStartRotation);
            if (ABS(fromStart) > 60f)
            {
                if (fromStart > 30f)
                {
                    bambooStartRotation += DEG_90;
                }
                else if (fromStart < -30f)
                {
                    bambooStartRotation -= DEG_90;
                }
            }

            bambooLastTouch = touchPoint;
            UpdateBambooRotation();
        }

        /// <summary>
        /// Snaps the tube to the nearest 90° boundary when a drag is cancelled or lifted.
        /// Initiates the animated rotation and clears the touch owner.
        /// </summary>
        public void HandleBambooCancel()
        {
            float fromStart = AngleTo180(bambooVisualRotation - bambooStartRotation);
            bambooTargetRotation = fromStart > 30f
                ? bambooStartRotation + DEG_90
                : fromStart < -30f
                    ? bambooStartRotation - DEG_90
                    : bambooStartRotation;

            isRotatingBamboo = true;
            BambooTouchIndex = -1;
        }

        /// <summary>
        /// Tests whether <paramref name="candyPoint"/> has entered either hole.
        /// If so, records the exit hole in <see cref="HoleOut"/> and returns <see langword="true"/>.
        /// </summary>
        /// <param name="candyPoint">The candy physics point to test, or <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the candy entered a hole; otherwise <see langword="false"/>.</returns>
        public bool TryCatchCandy(ConstraintedPoint candyPoint)
        {
            if (candyPoint == null)
            {
                return false;
            }

            float captureRadius = BambooCaptureRadius * interactionScale;
            if (VectDistance(candyPoint.pos, bambooHole1) < captureRadius && IsCandyMovingInside(candyPoint, bambooHole1))
            {
                bambooHoleOut = bambooHole2;
                return true;
            }

            if (VectDistance(candyPoint.pos, bambooHole2) < captureRadius && IsCandyMovingInside(candyPoint, bambooHole2))
            {
                bambooHoleOut = bambooHole1;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Teleports <paramref name="candyPoint"/> to the exit hole and applies a Verlet
        /// impulse so the physics integrator launches it outward at <see cref="BambooThrowSpeed"/>.
        /// </summary>
        /// <param name="candyPoint">The candy physics point to teleport, or <see langword="null"/>.</param>
        public void ThrowCandy(ConstraintedPoint candyPoint)
        {
            if (candyPoint == null)
            {
                return;
            }

            Vector throwDirection = VectSub(bambooHoleOut, Vect(x, y));
            throwDirection = VectLengthsq(throwDirection) <= FLOAT_PRECISION
                ? Vect(0f, -1f)
                : VectNormalize(throwDirection);

            candyPoint.v = vectZero;
            candyPoint.a = vectZero;
            candyPoint.ResetForces();
            candyPoint.pos = bambooHoleOut;

            // Verlet impulse: set prevPos behind holeOut so the integrator derives the launch velocity.
            float throwStep = BambooThrowSpeed * interactionScale;
            Vector launchStep = VectMult(throwDirection, throwStep);
            candyPoint.prevPos = VectSub(candyPoint.pos, launchStep);
        }

        /// <summary>
        /// Spawns a <see cref="LeafParticles"/> burst at the exit hole.
        /// </summary>
        /// <param name="pool">The animations pool that will own the spawned particle system.</param>
        public void ThrowParticlesOut(AnimationsPool pool)
        {
            if (pool == null)
            {
                return;
            }

            Image grid = Image_createWithResID(Resources.Img.ObjBambooTube);
            grid.DoRestoreCutTransparency();
            float angle = RADIANS_TO_DEGREES(VectAngleNormalized(VectSub(bambooHoleOut, Vect(x, y))));
            if (new LeafParticles().Init(5, angle, grid, 0f) is LeafParticles bambooParticles)
            {
                bambooParticles.particlesDelegate = new Particles.ParticlesFinished(pool.ParticlesFinished);
                bambooParticles.x = bambooHoleOut.X;
                bambooParticles.y = bambooHoleOut.Y;
                bambooParticles.StartSystem(5);
                _ = pool.AddChild(bambooParticles);
            }
        }

        /// <summary>
        /// The touch index that currently owns this tube's drag interaction, or -1 if idle.
        /// </summary>
        public int BambooTouchIndex { get; private set; } = -1;

        /// <summary>
        /// World-space position of the exit hole, valid after a successful <see cref="TryCatchCandy"/>.
        /// </summary>
        public Vector HoleOut => bambooHoleOut;

        /// <summary>
        /// Begins an animated 90° CCW snap if no snap is already running.
        /// </summary>
        private void RotateBambooCounterClockwise()
        {
            if (isRotatingBamboo)
            {
                return;
            }

            bambooTargetRotation = bambooVisualRotation + DEG_90;
            isRotatingBamboo = true;
        }

        /// <summary>
        /// Returns the signed angle (degrees) needed to rotate from <paramref name="start"/>
        /// to <paramref name="end"/> about <paramref name="center"/>.
        /// </summary>
        /// <param name="start">Starting touch position.</param>
        /// <param name="end">Ending touch position.</param>
        /// <param name="center">Centre of rotation.</param>
        /// <returns>Signed rotation angle in degrees.</returns>
        private static float GetRotateAngleForStartEndCenter(Vector start, Vector end, Vector center)
        {
            Vector startOffset = VectSub(start, center);
            Vector endOffset = VectSub(end, center);
            float angleDelta = VectAngleNormalized(endOffset) - VectAngleNormalized(startOffset);
            return RADIANS_TO_DEGREES(angleDelta);
        }

        /// <summary>
        /// Normalises <paramref name="angle"/> into (-180, 180].
        /// </summary>
        /// <param name="angle">Angle in degrees to normalise.</param>
        /// <returns>The normalised angle in the range (-180, 180].</returns>
        private static float AngleTo180(float angle)
        {
            float normalized = AngleTo0_360(angle);
            if (normalized > DEG_180)
            {
                normalized -= DEG_360;
            }

            return normalized;
        }

        /// <summary>
        /// Returns <see langword="true"/> when <paramref name="candyPoint"/> is moving toward
        /// <paramref name="holePosition"/> (i.e. into the tube rather than away from it).
        /// Uses <c>posDelta</c> as a fallback when <c>prevPos</c> is not yet initialised.
        /// </summary>
        /// <param name="candyPoint">The candy physics point to test.</param>
        /// <param name="holePosition">World-space position of the hole being tested.</param>
        /// <returns><see langword="true"/> if the candy is moving toward the hole; otherwise <see langword="false"/>.</returns>
        private bool IsCandyMovingInside(ConstraintedPoint candyPoint, Vector holePosition)
        {
            Vector movement = candyPoint.prevPos.X == UNDEFINED_COORDINATE
                ? candyPoint.posDelta
                : VectSub(candyPoint.pos, candyPoint.prevPos);
            Vector centerToHole = VectSub(holePosition, Vect(x, y));
            return VectDot(movement, centerToHole) <= 0f;
        }

        /// <summary>
        /// Recomputes the world positions of both holes from the current visual rotation,
        /// then propagates the rotation to the shell sprites and the rotated bounding box.
        /// The core quad (quad 0, on the parent object itself) is intentionally left unrotated.
        /// </summary>
        private void UpdateBambooRotation()
        {
            bambooHole1 = Vect(x + (bb.w * 0.5f), y);
            bambooHole2 = Vect(x, y + (bb.w * 0.5f));
            float rotationRadians = DEGREES_TO_RADIANS(bambooVisualRotation - DEG_90);
            bambooHole1 = VectRotateAround(bambooHole1, rotationRadians, x, y);
            bambooHole2 = VectRotateAround(bambooHole2, rotationRadians, x, y);
            bambooBackSprite.rotation = bambooVisualRotation;
            bambooFrontSprite.rotation = bambooVisualRotation;
            RotateWithBB(bambooVisualRotation);
            rotation = 0f;
        }

        /// <summary>
        /// Creates and attaches the back-shell and front-shell child sprites.
        /// Both use centre anchoring (anchor 9) so they stay aligned with the tube centre.
        /// </summary>
        private void SetupBambooShellSprites()
        {
            bambooBackSprite = Image_createWithResIDQuad(Resources.Img.ObjBambooTube, BambooBackQuad);
            bambooBackSprite.DoRestoreCutTransparency();
            bambooBackSprite.anchor = 9;
            bambooBackSprite.parentAnchor = 9;
            _ = AddChild(bambooBackSprite);

            bambooFrontSprite = Image_createWithResIDQuad(Resources.Img.ObjBambooTube, BambooFrontQuad);
            bambooFrontSprite.DoRestoreCutTransparency();
            bambooFrontSprite.anchor = 9;
            bambooFrontSprite.parentAnchor = 9;
            _ = AddChild(bambooFrontSprite);
        }

        /// <summary>Bounding-box half-size in unscaled points.</summary>
        private const float BambooBaseBbSize = 75f;

        /// <summary>Capture radius in unscaled points. Scaled by <see cref="interactionScale"/> at runtime.</summary>
        private const float BambooCaptureRadius = 17.5f;

        /// <summary>Launch speed in points per physics step (Verlet impulse magnitude).</summary>
        private const float BambooThrowSpeed = 7.5f;

        /// <summary>Snap threshold: rotation within this many degrees of the target snaps immediately.</summary>
        private const float BambooRotationSnapThreshold = 5f;

        /// <summary>Texture quad index for the core (centre) sprite.</summary>
        private const int BambooCoreQuad = 0;

        /// <summary>Texture quad index for the back shell sprite.</summary>
        private const int BambooBackQuad = 1;

        /// <summary>Texture quad index for the front shell sprite.</summary>
        private const int BambooFrontQuad = 2;

        /// <summary>Scale factor applied to bounding box and capture radius.</summary>
        private float interactionScale = 1f;

        /// <summary><see langword="true"/> while the tube is animating toward <see cref="bambooTargetRotation"/>.</summary>
        private bool isRotatingBamboo;

        /// <summary>Rotation at the start of the current drag gesture, in degrees.</summary>
        private float bambooStartRotation;

        /// <summary>Target rotation the snap animation is converging toward, in degrees.</summary>
        private float bambooTargetRotation;

        /// <summary>Current visual rotation of the tube, in degrees.</summary>
        private float bambooVisualRotation;

        /// <summary>Last recorded touch position during a drag gesture.</summary>
        private Vector bambooLastTouch;

        /// <summary>World-space position of hole 1 (recomputed on rotation).</summary>
        private Vector bambooHole1;

        /// <summary>World-space position of hole 2 (recomputed on rotation).</summary>
        private Vector bambooHole2;

        /// <summary>World-space position of the exit hole after a successful <see cref="TryCatchCandy"/>.</summary>
        private Vector bambooHoleOut;

        /// <summary>Back shell child sprite.</summary>
        private Image bambooBackSprite;

        /// <summary>Front shell child sprite.</summary>
        private Image bambooFrontSprite;
    }
}
