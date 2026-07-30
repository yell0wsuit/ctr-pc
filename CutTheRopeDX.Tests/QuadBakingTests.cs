using System;

using CutTheRopeDX.Desktop;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public sealed class QuadBakingTests
    {
        private const float Tolerance = 1e-4f;

        [Fact]
        public void PremultipliedTintMatchesBasicEffectDiffuseConvention()
        {
            Color baked = QuadBaking.BakePremultipliedTint(new Color(200, 100, 50, 128));
            Assert.Equal(Color.FromNonPremultiplied(200, 100, 50, 128), baked);
            Assert.Equal(128, baked.A);
            Assert.Equal((byte)(200 * 128 / 255), baked.R);
        }

        [Fact]
        public void OpaqueWhiteTintBakesToWhite()
        {
            Assert.Equal(Color.White, QuadBaking.BakePremultipliedTint(Color.White));
        }

        [Fact]
        public void ZeroAlphaIsInvisible()
        {
            Assert.True(QuadBaking.IsInvisible(new Color(255, 255, 255, 0)));
            Assert.False(QuadBaking.IsInvisible(new Color(255, 255, 255, 1)));
        }

        [Fact]
        public void TranslationBakesIntoPosition()
        {
            VertexPositionNormalTexture source = new(new Vector3(1f, 2f, 0f), Vector3.UnitZ, new Vector2(0.25f, 0.75f));
            Matrix modelView = Matrix.CreateTranslation(10f, 20f, 0f);
            VertexPositionColorTexture baked = QuadBaking.Bake(source, modelView, Color.White);
            Assert.Equal(11f, baked.Position.X, Tolerance);
            Assert.Equal(22f, baked.Position.Y, Tolerance);
        }

        [Fact]
        public void RotationBakesIntoPosition()
        {
            VertexPositionNormalTexture source = new(new Vector3(1f, 0f, 0f), Vector3.UnitZ, Vector2.Zero);
            Matrix modelView = Matrix.CreateRotationZ(MathHelper.PiOver2);
            VertexPositionColorTexture baked = QuadBaking.Bake(source, modelView, Color.White);
            Assert.Equal(0f, baked.Position.X, Tolerance);
            Assert.Equal(1f, baked.Position.Y, Tolerance);
        }

        [Fact]
        public void ScaleBakesIntoPosition()
        {
            VertexPositionNormalTexture source = new(new Vector3(2f, 3f, 0f), Vector3.UnitZ, Vector2.Zero);
            Matrix modelView = Matrix.CreateScale(2f, 0.5f, 1f);
            VertexPositionColorTexture baked = QuadBaking.Bake(source, modelView, Color.White);
            Assert.Equal(4f, baked.Position.X, Tolerance);
            Assert.Equal(1.5f, baked.Position.Y, Tolerance);
        }

        [Fact]
        public void SkewBakesIntoPosition()
        {
            float cos45 = MathF.Cos(MathHelper.PiOver4);
            Matrix skew = Matrix.Identity;
            skew.M21 = -cos45;
            skew.M22 = cos45;
            VertexPositionNormalTexture source = new(new Vector3(0f, 1f, 0f), Vector3.UnitZ, Vector2.Zero);
            VertexPositionColorTexture baked = QuadBaking.Bake(source, skew, Color.White);
            Assert.Equal(-cos45, baked.Position.X, Tolerance);
            Assert.Equal(cos45, baked.Position.Y, Tolerance);
        }

        [Fact]
        public void UvCoordinatesPassThroughUnchanged()
        {
            VertexPositionNormalTexture source = new(Vector3.Zero, Vector3.UnitZ, new Vector2(0.25f, 0.75f));
            VertexPositionColorTexture baked = QuadBaking.Bake(source, Matrix.CreateScale(3f), Color.White);
            Assert.Equal(new Vector2(0.25f, 0.75f), baked.TextureCoordinate);
        }
    }
}
