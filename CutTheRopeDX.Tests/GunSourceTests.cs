using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class GunSourceTests
    {
        [Fact]
        public void UnfiredGunCanFireAndCanAttach()
        {
            GunSource gun = new();

            Assert.True(gun.CanAttach);
            Assert.True(gun.CanFire(candyInLantern: false));
            Assert.False(gun.HasFired);
        }

        [Fact]
        public void GunWithCandyInLanternCannotFire()
        {
            GunSource gun = new();

            Assert.False(gun.CanFire(candyInLantern: true));
        }

        [Fact]
        public void FiredGunIsSpent()
        {
            GunSource gun = new();
            gun.Fire(new Vector(0f, 0f), new Vector(0f, 100f), candyRotation: 0f);

            Assert.True(gun.HasFired);
            Assert.False(gun.CanFire(candyInLantern: false));
            Assert.False(gun.CanAttach);
        }

        [Fact]
        public void FireCapturesTheBaselineRotationsUsedByCupTracking()
        {
            GunSource gun = new();
            gun.Fire(new Vector(0f, 0f), new Vector(0f, 100f), candyRotation: 30f);

            // Cup rotation is initial + (live candy rotation - candy rotation at fire time).
            // At the moment of firing the delta is zero, so the cup sits at its initial rotation.
            gun.TrackFiredCup(new Vector(0f, 100f), candyRotation: 30f);

            Assert.Equal(gun.InitialRotation, gun.CupRotation);
        }

        [Fact]
        public void TrackFiredCupFollowsTheCandyRotationDelta()
        {
            GunSource gun = new();
            gun.Fire(new Vector(0f, 0f), new Vector(0f, 100f), candyRotation: 30f);

            gun.TrackFiredCup(new Vector(0f, 100f), candyRotation: 50f);

            Assert.Equal(gun.InitialRotation + 20f, gun.CupRotation);
        }
    }
}
