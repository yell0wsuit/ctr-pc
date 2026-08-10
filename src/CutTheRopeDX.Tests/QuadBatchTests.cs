using CutTheRopeDX.Desktop;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public sealed class QuadBatchTests
    {
        private static readonly object TextureA = new();
        private static readonly object TextureB = new();

        private static QuadBatchKey MakeKey(object texture = null)
        {
            return new QuadBatchKey(
                texture ?? TextureA,
                BlendParams.BlendType.SourceAlpha_InverseSourceAlpha,
                new Rectangle(0, 0, 1024, 576),
                Matrix.CreateOrthographicOffCenter(0f, 1024f, 576f, 0f, -1f, 1f));
        }

        private static VertexPositionNormalTexture[] MakeQuad(float x)
        {
            return
            [
                new VertexPositionNormalTexture(new Vector3(x, 0f, 0f), Vector3.UnitZ, new Vector2(0f, 0f)),
                new VertexPositionNormalTexture(new Vector3(x + 1f, 0f, 0f), Vector3.UnitZ, new Vector2(1f, 0f)),
                new VertexPositionNormalTexture(new Vector3(x, 1f, 0f), Vector3.UnitZ, new Vector2(0f, 1f)),
                new VertexPositionNormalTexture(new Vector3(x + 1f, 1f, 0f), Vector3.UnitZ, new Vector2(1f, 1f)),
            ];
        }

        [Fact]
        public void EmptyBatchAcceptsAnyKey()
        {
            QuadBatch batch = new();
            Assert.True(batch.CanAccept(MakeKey()));
            Assert.True(batch.CanAccept(MakeKey(TextureB)));
        }

        [Fact]
        public void NonEmptyBatchAcceptsOnlyMatchingKey()
        {
            QuadBatch batch = new();
            batch.Append(MakeQuad(0f), MakeKey(), Matrix.Identity, Color.White);
            Assert.True(batch.CanAccept(MakeKey()));
            Assert.False(batch.CanAccept(MakeKey(TextureB)));
        }

        [Fact]
        public void DifferentBlendScissorOrProjectionRejects()
        {
            QuadBatch batch = new();
            QuadBatchKey key = MakeKey();
            batch.Append(MakeQuad(0f), key, Matrix.Identity, Color.White);

            QuadBatchKey otherBlend = new(TextureA, BlendParams.BlendType.SourceAlpha_One, key.Scissor, key.Projection);
            QuadBatchKey otherScissor = new(TextureA, key.Blend, new Rectangle(0, 0, 100, 100), key.Projection);
            QuadBatchKey otherProjection = new(TextureA, key.Blend, key.Scissor, Matrix.Identity);
            Assert.False(batch.CanAccept(otherBlend));
            Assert.False(batch.CanAccept(otherScissor));
            Assert.False(batch.CanAccept(otherProjection));
        }

        [Fact]
        public void AppendPreservesSubmissionOrderAndTransforms()
        {
            QuadBatch batch = new();
            batch.Append(MakeQuad(0f), MakeKey(), Matrix.CreateTranslation(100f, 0f, 0f), Color.White);
            batch.Append(MakeQuad(0f), MakeKey(), Matrix.CreateTranslation(200f, 0f, 0f), Color.White);
            Assert.Equal(2, batch.QuadCount);
            Assert.Equal(100f, batch.StagingArray[0].Position.X);
            Assert.Equal(200f, batch.StagingArray[4].Position.X);
            Assert.Equal(new Vector2(1f, 1f), batch.StagingArray[7].TextureCoordinate);
        }

        [Fact]
        public void AppendBakesPremultipliedTint()
        {
            QuadBatch batch = new();
            Color premultiplied = QuadBaking.BakePremultipliedTint(new Color(255, 255, 255, 128));
            batch.Append(MakeQuad(0f), MakeKey(), Matrix.Identity, premultiplied);
            Assert.Equal(premultiplied, batch.StagingArray[0].Color);
        }

        [Fact]
        public void BatchIsFullAtCapacity()
        {
            QuadBatch batch = new();
            VertexPositionNormalTexture[] quad = MakeQuad(0f);
            QuadBatchKey key = MakeKey();
            for (int i = 0; i < QuadBatch.Capacity; i++)
            {
                batch.Append(quad, key, Matrix.Identity, Color.White);
            }
            Assert.True(batch.IsFull);
        }

        [Fact]
        public void ClearResetsCountAndAcceptsNewKey()
        {
            QuadBatch batch = new();
            batch.Append(MakeQuad(0f), MakeKey(), Matrix.Identity, Color.White);
            batch.Clear();
            Assert.True(batch.IsEmpty);
            Assert.True(batch.CanAccept(MakeKey(TextureB)));
        }
    }
}
