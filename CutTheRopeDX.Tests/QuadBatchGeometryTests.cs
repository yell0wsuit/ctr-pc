using System.Collections.Generic;
using System.Linq;

using CutTheRopeDX.Desktop;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers staging geometry into the shared quad batch. The batch draws everything through one fixed
    /// index pattern, so anything staged into it has to decompose to that pattern exactly; getting it
    /// wrong silently renders the wrong triangles.
    /// </summary>
    public class QuadBatchGeometryTests
    {
        /// <summary>
        /// Builds vertices whose X coordinate is their own index, so a staged vertex can be traced back to
        /// the source vertex it came from.
        /// </summary>
        private static VertexPositionNormalTexture[] TraceableVertices(int count)
        {
            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[count];
            for (int i = 0; i < count; i++)
            {
                vertices[i] = new VertexPositionNormalTexture(new Vector3(i, 0f, 0f), Vector3.Up, Vector2.Zero);
            }
            return vertices;
        }

        private static QuadBatchKey AnyKey()
        {
            return new QuadBatchKey(new object(), BlendParams.BlendType.Default, Rectangle.Empty, Matrix.Identity);
        }

        /// <summary>Source vertex index a staged vertex came from, recovered from its X coordinate.</summary>
        private static int SourceIndex(QuadBatch batch, int stagedIndex)
        {
            return (int)batch.StagingArray[stagedIndex].Position.X;
        }

        /// <summary>The triangles the batch's index pattern draws over the staged vertices, as source indices.</summary>
        private static HashSet<string> StagedTriangles(QuadBatch batch)
        {
            HashSet<string> triangles = [];
            for (int quad = 0; quad < batch.QuadCount; quad++)
            {
                int stagedBase = quad * 4;
                // The pattern Build() emits: (0,1,2) and (2,1,3), rebased per quad.
                AddTriangle(triangles, SourceIndex(batch, stagedBase), SourceIndex(batch, stagedBase + 1), SourceIndex(batch, stagedBase + 2));
                AddTriangle(triangles, SourceIndex(batch, stagedBase + 2), SourceIndex(batch, stagedBase + 1), SourceIndex(batch, stagedBase + 3));
            }
            return triangles;
        }

        /// <summary>The triangles a hardware triangle strip of the given length draws.</summary>
        private static HashSet<string> StripTriangles(int vertexCount)
        {
            HashSet<string> triangles = [];
            for (int i = 0; i + 2 < vertexCount; i++)
            {
                AddTriangle(triangles, i, i + 1, i + 2);
            }
            return triangles;
        }

        /// <summary>Records a triangle by its vertex set, since cull mode is None and winding does not show.</summary>
        private static void AddTriangle(HashSet<string> triangles, int a, int b, int c)
        {
            int[] sorted = [a, b, c];
            System.Array.Sort(sorted);
            _ = triangles.Add(string.Join(",", sorted));
        }

        [Theory]
        [InlineData(4, 1)]
        [InlineData(6, 2)]
        [InlineData(8, 3)]
        [InlineData(10, 4)]
        public void EvenStripsDecomposeToQuads(int vertexCount, int expectedQuads)
        {
            Assert.Equal(expectedQuads, QuadBatch.StripQuadCount(vertexCount));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(3)]
        [InlineData(5)]
        [InlineData(9)]
        public void StripsThatAreNotWholeQuadsAreRefused(int vertexCount)
        {
            // An odd strip ends on a triangle with no partner, and no arrangement of quads covers it.
            Assert.Equal(0, QuadBatch.StripQuadCount(vertexCount));
        }

        [Theory]
        [InlineData(4)]
        [InlineData(6)]
        [InlineData(8)]
        [InlineData(10)]
        public void AStagedStripDrawsExactlyTheTrianglesTheStripWouldHave(int vertexCount)
        {
            QuadBatch batch = new();

            batch.AppendStrip(TraceableVertices(vertexCount), vertexCount, AnyKey(), Matrix.Identity, Color.White);

            Assert.Equal(StripTriangles(vertexCount), StagedTriangles(batch));
        }

        [Fact]
        public void StagedQuadsKeepTheirOwnVerticesContiguous()
        {
            QuadBatch batch = new();

            batch.AppendQuads(TraceableVertices(12), 3, AnyKey(), Matrix.Identity, Color.White);

            Assert.Equal(3, batch.QuadCount);
            for (int i = 0; i < 12; i++)
            {
                Assert.Equal(i, SourceIndex(batch, i));
            }
        }

        [Fact]
        public void AppendingLeavesEarlierQuadsInPlace()
        {
            QuadBatch batch = new();
            QuadBatchKey key = AnyKey();

            batch.AppendQuads(TraceableVertices(4), 1, key, Matrix.Identity, Color.White);
            batch.AppendStrip(TraceableVertices(6), 6, key, Matrix.Identity, Color.White);

            Assert.Equal(3, batch.QuadCount);
            // The first quad's four vertices are untouched by the strip staged after it.
            for (int i = 0; i < 4; i++)
            {
                Assert.Equal(i, SourceIndex(batch, i));
            }
            // The strip's first quad follows it, starting from the strip's own vertex zero.
            Assert.Equal(0, SourceIndex(batch, 4));
        }

        [Fact]
        public void RemainingCapacityTracksWhatHasBeenStaged()
        {
            QuadBatch batch = new();
            Assert.Equal(QuadBatch.Capacity, batch.RemainingCapacity);

            batch.AppendQuads(TraceableVertices(8), 2, AnyKey(), Matrix.Identity, Color.White);
            Assert.Equal(QuadBatch.Capacity - 2, batch.RemainingCapacity);

            batch.Clear();
            Assert.Equal(QuadBatch.Capacity, batch.RemainingCapacity);
        }

        [Fact]
        public void TheBuiltPatternIsAccepted()
        {
            Assert.True(QuadIndexPattern.Matches(QuadIndexPattern.Build(64), 64));
        }

        [Fact]
        public void TheMultiDrawerWindingIsAccepted()
        {
            // ImageMultiDrawer builds the second triangle the other way round. Cull mode is None, so it
            // covers the same pixels, and refusing it would leave every particle system unbatched.
            short[] indices = new short[12];
            for (int quad = 0; quad < 2; quad++)
            {
                int index = quad * 6;
                int vertex = quad * 4;
                indices[index] = (short)vertex;
                indices[index + 1] = (short)(vertex + 1);
                indices[index + 2] = (short)(vertex + 2);
                indices[index + 3] = (short)(vertex + 3);
                indices[index + 4] = (short)(vertex + 2);
                indices[index + 5] = (short)(vertex + 1);
            }

            Assert.True(QuadIndexPattern.Matches(indices, 2));
        }

        [Fact]
        public void TheOtherDiagonalIsRefused()
        {
            // (0,1,2) and (0,2,3) covers the same area but splits the quad the other way, which
            // interpolates differently across it. Not a substitution to make silently.
            short[] indices = [0, 1, 2, 0, 2, 3];

            Assert.False(QuadIndexPattern.Matches(indices, 1));
        }

        [Fact]
        public void GeometryThatIsNotQuadsIsRefused()
        {
            // A fan or a shared-vertex mesh reuses vertices across quads, so its indices do not step by
            // four, and staging it as quads would draw something else entirely.
            short[] indices = [0, 1, 2, 2, 1, 3, 2, 3, 4, 4, 3, 5];

            Assert.False(QuadIndexPattern.Matches(indices, 2));
        }

        [Fact]
        public void AShortIndexArrayIsRefused()
        {
            Assert.False(QuadIndexPattern.Matches(QuadIndexPattern.Build(2), 3));
            Assert.False(QuadIndexPattern.Matches(null, 1));
        }

        [Fact]
        public void EveryVertexOfAStagedStripCarriesTheBakedTint()
        {
            QuadBatch batch = new();
            Color tint = new(10, 20, 30, 40);

            batch.AppendStrip(TraceableVertices(6), 6, AnyKey(), Matrix.Identity, tint);

            Assert.All(
                Enumerable.Range(0, batch.QuadCount * 4),
                staged => Assert.Equal(tint, batch.StagingArray[staged].Color));
        }
    }
}
