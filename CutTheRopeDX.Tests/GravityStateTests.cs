using System;
using System.Reflection;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Physics;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public sealed class GravityStateTests : IDisposable
    {
        private const BindingFlags InstanceFields = BindingFlags.Instance
            | BindingFlags.Public
            | BindingFlags.NonPublic;

        private readonly Vector originalGlobalGravity = MaterialPoint.globalGravity;
        private readonly bool originalGlobalDisableGravity = MaterialPoint.globalDisableGravity;

        /// <inheritdoc />
        public void Dispose()
        {
            MaterialPoint.globalGravity = originalGlobalGravity;
            MaterialPoint.globalDisableGravity = originalGlobalDisableGravity;
        }

        [Fact]
        public void GameSceneOwnsOneGravityStateInsteadOfParallelFields()
        {
            FieldInfo[] fields = typeof(GameScene).GetFields(InstanceFields);

            FieldInfo gravity = Assert.Single(
                fields,
                field => field.FieldType == typeof(GravityState));
            Assert.Equal("gravityState", gravity.Name);
            Assert.DoesNotContain(
                fields,
                field => field.Name is "gravityNormal"
                    or "gravityTouchDown"
                    or "globalGravityX"
                    or "globalGravityY"
                    or "gravityButton"
                    or "earthAnims");
        }

        [Fact]
        public void GravityStateDoesNotExposeMutablePresentationObjects()
        {
            PropertyInfo[] properties = typeof(GravityState).GetProperties(
                BindingFlags.Instance | BindingFlags.Public);

            Assert.DoesNotContain(properties, property => property.PropertyType == typeof(ToggleButton));
            Assert.DoesNotContain(properties, property => property.Name == "EarthAnimations");
        }

        [Fact]
        public void ToggleAtomicallyUpdatesDerivedVectorPhysicsAndPresentation()
        {
            GravityState gravity = new();
            ToggleButton button = CreateToggleButton();
            Image earth = CreateEarthAnimation();
            gravity.ConfigureBase(new Vector(12f, 34f));
            gravity.AttachButton(button);
            gravity.AddEarthAnimation(earth);

            gravity.Activate();

            Assert.False(gravity.IsInverted);
            AssertVector(12f, 34f, gravity.BaseVector);
            AssertVector(12f, 34f, gravity.CurrentVector);
            AssertVector(12f, 34f, MaterialPoint.globalGravity);
            Assert.False(MaterialPoint.globalDisableGravity);
            Assert.False(button.On());
            Assert.Equal(-1, earth.GetCurrentTimelineIndex());
            Assert.Equal(0f, earth.rotation);

            gravity.Toggle();

            Assert.True(gravity.IsInverted);
            AssertVector(12f, 34f, gravity.BaseVector);
            AssertVector(12f, -34f, gravity.CurrentVector);
            AssertVector(12f, -34f, MaterialPoint.globalGravity);
            Assert.True(button.On());
            Assert.Equal(1, earth.GetCurrentTimelineIndex());
        }

        [Fact]
        public void EveryAttachedButtonFollowsTheAuthoritativeOrientation()
        {
            GravityState gravity = new();
            ToggleButton first = CreateToggleButton();
            ToggleButton second = CreateToggleButton();
            gravity.ConfigureBase(new Vector(0f, 100f));
            gravity.AttachButton(first);
            gravity.AttachButton(second);
            gravity.Activate();

            gravity.Toggle();

            Assert.True(first.On());
            Assert.True(second.On());

            gravity.Toggle();

            Assert.False(first.On());
            Assert.False(second.On());
        }

        [Fact]
        public void ActivateRestoresNormalOrientationAcrossEveryRepresentation()
        {
            GravityState gravity = new();
            ToggleButton button = CreateToggleButton();
            Image earth = CreateEarthAnimation();
            gravity.ConfigureBase(new Vector(-7f, 25f));
            gravity.AttachButton(button);
            gravity.AddEarthAnimation(earth);
            gravity.Activate();
            gravity.Toggle();
            gravity.CaptureToggleTouch(3);

            gravity.Activate();

            Assert.False(gravity.IsInverted);
            AssertVector(-7f, 25f, gravity.CurrentVector);
            AssertVector(-7f, 25f, MaterialPoint.globalGravity);
            Assert.False(button.On());
            Assert.Equal(-1, earth.GetCurrentTimelineIndex());
            Assert.Equal(0f, earth.rotation);
            Assert.False(gravity.ReleaseToggleTouch(3));
        }

        [Fact]
        public void ToggleTouchCanOnlyBeReleasedByItsOwningPointer()
        {
            GravityState gravity = new();

            gravity.CaptureToggleTouch(2);

            Assert.False(gravity.ReleaseToggleTouch(1));
            Assert.True(gravity.ReleaseToggleTouch(2));
            Assert.False(gravity.ReleaseToggleTouch(2));
        }

        [Fact]
        public void ZeroBaseGravityDisablesGlobalGravityWithoutASecondAuthority()
        {
            GravityState gravity = new();
            gravity.ConfigureBase(default);

            gravity.Activate();

            Assert.Equal(default, gravity.CurrentVector);
            Assert.Equal(default, MaterialPoint.globalGravity);
            Assert.True(MaterialPoint.globalDisableGravity);
        }

        private static ToggleButton CreateToggleButton()
        {
            return new ToggleButton().InitWithUpElement1DownElement1UpElement2DownElement2andID(
                new BaseElement(),
                new BaseElement(),
                new BaseElement(),
                new BaseElement(),
                GameSceneButtonId.GravityToggle);
        }

        private static Image CreateEarthAnimation()
        {
            Image earth = new();
            earth.AddTimelinewithID(new Timeline().InitWithMaxKeyFramesOnTrack(0), 0);
            earth.AddTimelinewithID(new Timeline().InitWithMaxKeyFramesOnTrack(0), 1);
            return earth;
        }

        private static void AssertVector(float expectedX, float expectedY, Vector actual)
        {
            Assert.Equal(expectedX, actual.X);
            Assert.Equal(expectedY, actual.Y);
        }
    }
}
