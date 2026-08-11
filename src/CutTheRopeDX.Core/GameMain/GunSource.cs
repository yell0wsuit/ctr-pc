using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Visual;

using static CutTheRopeDX.Framework.Helpers.CTRMathHelper;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// A hook that fires a suction cup at the candy when tapped, creating its rope on the way. One
    /// shot per level. Owns the gun's four images and the cup's per-frame tracking, which used to be
    /// computed inside <c>GameScene.Draw</c>.
    /// </summary>
    internal sealed class GunSource : RopeSource
    {
        /// <summary>Gets whether the gun has already fired.</summary>
        public bool HasFired { get; private set; }

        /// <summary>Gets the cup rotation captured when the gun fired.</summary>
        public float InitialRotation { get; private set; }

        /// <summary>Gets the candy rotation captured when the gun fired.</summary>
        public float CandyInitialRotation { get; private set; }

        /// <summary>Gets the cup's current rotation, updated by <see cref="TrackFiredCup"/>.</summary>
        public float CupRotation { get; private set; }

        /// <summary>Gets or sets the gun's back image layer.</summary>
        public Image Back { get; set; }

        /// <summary>Gets or sets the gun's aim arrow image.</summary>
        public Image Arrow { get; set; }

        /// <summary>Gets or sets the gun's front image layer.</summary>
        public Image Front { get; set; }

        /// <summary>Gets or sets the animated cup the gun fires.</summary>
        public Animation Cup { get; set; }

        /// <inheritdoc />
        public override bool CanAttach => !HasFired;

        /// <summary>Determines whether the gun can fire at the candy right now.</summary>
        /// <param name="candyInLantern">
        /// <see langword="true"/> when the target candy is captured in a lantern.
        /// </param>
        /// <returns><see langword="true"/> when an unfired gun has an available candy.</returns>
        public bool CanFire(bool candyInLantern)
        {
            return !HasFired && !candyInLantern;
        }

        /// <summary>Fires the gun, capturing the baselines the cup tracks against.</summary>
        /// <param name="hookPosition">The gun's world position.</param>
        /// <param name="candyPosition">The candy's world position.</param>
        /// <param name="candyRotation">The candy's rotation at the moment of firing.</param>
        public void Fire(Vector hookPosition, Vector candyPosition, float candyRotation)
        {
            HasFired = true;
            Vector gunToCandy = VectSub(hookPosition, candyPosition);
            InitialRotation = RADIANS_TO_DEGREES(VectAngleNormalized(gunToCandy))
                + DEG_90;
            CandyInitialRotation = candyRotation;
            CupRotation = InitialRotation;
        }

        /// <summary>Aims the unfired arrow at the candy.</summary>
        /// <param name="hookPosition">The gun's world position.</param>
        /// <param name="candyPosition">The candy's world position.</param>
        public void TrackAim(Vector hookPosition, Vector candyPosition)
        {
            if (HasFired || Arrow == null)
            {
                return;
            }

            Vector gunToCandy = VectSub(hookPosition, candyPosition);
            Arrow.rotation = RADIANS_TO_DEGREES(VectAngleNormalized(gunToCandy));
        }

        /// <summary>Moves the fired cup with the candy it is stuck to.</summary>
        /// <param name="candyPosition">The candy's world position.</param>
        /// <param name="candyRotation">The candy's current rotation.</param>
        public void TrackFiredCup(Vector candyPosition, float candyRotation)
        {
            if (!HasFired)
            {
                return;
            }

            CupRotation = InitialRotation + candyRotation - CandyInitialRotation;

            if (Cup == null
                || Cup.GetCurrentTimelineIndex() == Grab.GUN_CUP_DROP_AND_HIDE)
            {
                return;
            }

            Cup.x = candyPosition.X;
            Cup.y = candyPosition.Y;
            Cup.rotation = CupRotation;
        }

        /// <summary>Switches the gun body between its enabled and disabled appearance.</summary>
        /// <param name="disabled"><see langword="true"/> to show the disabled body.</param>
        public void SetDisabled(bool disabled)
        {
            Front?.SetDrawQuad(HasFired || disabled ? Grab.GunDisabledFrontQuad : Grab.GunFrontQuad);
        }

        /// <inheritdoc />
        public override void OnRopeCut(RopeCutReason reason)
        {
            if (Cup == null)
            {
                return;
            }

            if (reason == RopeCutReason.Severed)
            {
                Cup.PlayTimeline(Grab.GUN_CUP_HIDE);
            }
            else if (RGBAColor.RGBAEqual(RGBAColor.solidOpaqueRGBA, Cup.color))
            {
                Cup.PlayTimeline(Grab.GUN_CUP_DROP_AND_HIDE);
            }
        }

        /// <inheritdoc />
        public override void Update(float delta)
        {
            if (HasFired)
            {
                Cup?.Update(delta);
            }
        }
    }
}
