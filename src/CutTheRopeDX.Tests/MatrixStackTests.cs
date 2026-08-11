using System.Numerics;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Platform;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class MatrixStackTests
    {
        private const float Tolerance = 1e-4f;

        [Fact]
        public void StartsAtIdentity()
        {
            MatrixStack stack = new();
            Assert.Equal(Matrix4x4.Identity, stack.ModelView);
        }

        [Fact]
        public void TranslateMovesAPoint()
        {
            MatrixStack stack = new();
            stack.Translate(10f, 20f);
            Vector3 moved = Vector3.Transform(new Vector3(1f, 2f, 0f), stack.ModelView);
            Assert.Equal(11f, moved.X, Tolerance);
            Assert.Equal(22f, moved.Y, Tolerance);
        }

        [Fact]
        public void PushAndPopRestoreTheMatrix()
        {
            MatrixStack stack = new();
            stack.Translate(5f, 0f);
            stack.Push();
            stack.Translate(100f, 0f);
            stack.Pop();
            Vector3 moved = Vector3.Transform(Vector3.Zero, stack.ModelView);
            Assert.Equal(5f, moved.X, Tolerance);
        }

        [Fact]
        public void TransformsComposeInGlOrderSoTranslationAppliesLast()
        {
            MatrixStack stack = new();
            stack.Translate(10f, 0f);
            stack.Scale(2f, 2f);
            Vector3 moved = Vector3.Transform(new Vector3(1f, 0f, 0f), stack.ModelView);
            Assert.Equal(12f, moved.X, Tolerance);
        }

        [Fact]
        public void RotateNinetyDegreesMapsXOntoY()
        {
            MatrixStack stack = new();
            stack.RotateDegrees(90f);
            Vector3 moved = Vector3.Transform(new Vector3(1f, 0f, 0f), stack.ModelView);
            Assert.Equal(0f, moved.X, Tolerance);
            Assert.Equal(1f, moved.Y, Tolerance);
        }

        // Flash's skew, which the FlashXml animation data is authored against, rotates each axis
        // by its own angle and so leaves both axes unit length. A plain tangent shear keeps the
        // axis lengths but tilts the wrong one, which is what pulled animated parts off their
        // bodies. These pin the matrix the desktop backend already produces.
        [Fact]
        public void SkewXRotatesTheYAxis()
        {
            MatrixStack stack = new();
            stack.Skew(45f, 0f);
            Vector3 moved = Vector3.Transform(new Vector3(0f, 1f, 0f), stack.ModelView);
            Assert.Equal(-0.70710678f, moved.X, Tolerance);
            Assert.Equal(0.70710678f, moved.Y, Tolerance);
        }

        [Fact]
        public void SkewYRotatesTheXAxis()
        {
            MatrixStack stack = new();
            stack.Skew(0f, 45f);
            Vector3 moved = Vector3.Transform(new Vector3(1f, 0f, 0f), stack.ModelView);
            Assert.Equal(0.70710678f, moved.X, Tolerance);
            Assert.Equal(0.70710678f, moved.Y, Tolerance);
        }

        [Fact]
        public void SkewLeavesAxesUnitLength()
        {
            MatrixStack stack = new();
            stack.Skew(30f, -20f);
            Vector3 xAxis = Vector3.Transform(new Vector3(1f, 0f, 0f), stack.ModelView);
            Vector3 yAxis = Vector3.Transform(new Vector3(0f, 1f, 0f), stack.ModelView);
            Assert.Equal(1f, xAxis.Length(), Tolerance);
            Assert.Equal(1f, yAxis.Length(), Tolerance);
        }

        [Fact]
        public void ZeroSkewIsIdentity()
        {
            MatrixStack stack = new();
            stack.Skew(0f, 0f);
            Assert.Equal(Matrix4x4.Identity, stack.ModelView);
        }

        [Fact]
        public void OrthographicMapsCornersToClipSpace()
        {
            MatrixStack stack = new();
            stack.SetOrthographic(0f, 2560f, 1440f, 0f, -1f, 1f);
            Vector3 topLeft = Vector3.Transform(Vector3.Zero, stack.Projection);
            Vector3 bottomRight = Vector3.Transform(new Vector3(2560f, 1440f, 0f), stack.Projection);
            Assert.Equal(-1f, topLeft.X, Tolerance);
            Assert.Equal(1f, topLeft.Y, Tolerance);
            Assert.Equal(1f, bottomRight.X, Tolerance);
            Assert.Equal(-1f, bottomRight.Y, Tolerance);
        }

        [Fact]
        public void InvisibleTintIsDetected()
        {
            Assert.True(QuadBaking.IsInvisible(new Color(255, 255, 255, 0)));
            Assert.False(QuadBaking.IsInvisible(new Color(255, 255, 255, 1)));
        }

        [Fact]
        public void BakeTransformsPositionAndKeepsUv()
        {
            MatrixStack stack = new();
            stack.Translate(3f, 4f);
            VertexPositionNormalTexture source = new(
                new Vector3(1f, 1f, 0f), Vector3.UnitZ, new Vector2(0.25f, 0.75f));

            VertexPositionColorTexture baked = QuadBaking.Bake(
                source, stack.ModelView, new Color(10, 20, 30, 255));

            Assert.Equal(4f, baked.Position.X, Tolerance);
            Assert.Equal(5f, baked.Position.Y, Tolerance);
            Assert.Equal(new Vector2(0.25f, 0.75f), baked.TextureCoordinate);
        }
    }
}
