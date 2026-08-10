using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using CutTheRopeDX.Framework.Core;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Pins the memory layout of the Core-owned render structs to the XNA types they are
    /// reinterpreted as at the Desktop boundary. The backend casts spans of these structs
    /// straight into MonoGame vertex uploads, so any drift here corrupts vertex data
    /// silently rather than failing loudly. The expected sizes/offsets below are the ones
    /// MonoGame reports for Microsoft.Xna.Framework.Graphics.VertexPosition* and Color.
    /// </summary>
    public class CoreStructLayoutTests
    {
        [Fact]
        public void ColorIsFourBytesInRgbaOrder()
        {
            Assert.Equal(4, Unsafe.SizeOf<Color>());
            Assert.Equal(0, Marshal.OffsetOf<Color>(nameof(Color.R)).ToInt32());
            Assert.Equal(1, Marshal.OffsetOf<Color>(nameof(Color.G)).ToInt32());
            Assert.Equal(2, Marshal.OffsetOf<Color>(nameof(Color.B)).ToInt32());
            Assert.Equal(3, Marshal.OffsetOf<Color>(nameof(Color.A)).ToInt32());
        }

        [Fact]
        public void VertexPositionColorMatchesXnaLayout()
        {
            Assert.Equal(16, Unsafe.SizeOf<VertexPositionColor>());
            Assert.Equal(0, Marshal.OffsetOf<VertexPositionColor>(nameof(VertexPositionColor.Position)).ToInt32());
            Assert.Equal(12, Marshal.OffsetOf<VertexPositionColor>(nameof(VertexPositionColor.Color)).ToInt32());
        }

        [Fact]
        public void VertexPositionColorTextureMatchesXnaLayout()
        {
            Assert.Equal(24, Unsafe.SizeOf<VertexPositionColorTexture>());
            Assert.Equal(0, Marshal.OffsetOf<VertexPositionColorTexture>(nameof(VertexPositionColorTexture.Position)).ToInt32());
            Assert.Equal(12, Marshal.OffsetOf<VertexPositionColorTexture>(nameof(VertexPositionColorTexture.Color)).ToInt32());
            Assert.Equal(16, Marshal.OffsetOf<VertexPositionColorTexture>(nameof(VertexPositionColorTexture.TextureCoordinate)).ToInt32());
        }

        [Fact]
        public void VertexPositionNormalTextureMatchesXnaLayout()
        {
            Assert.Equal(32, Unsafe.SizeOf<VertexPositionNormalTexture>());
            Assert.Equal(0, Marshal.OffsetOf<VertexPositionNormalTexture>(nameof(VertexPositionNormalTexture.Position)).ToInt32());
            Assert.Equal(12, Marshal.OffsetOf<VertexPositionNormalTexture>(nameof(VertexPositionNormalTexture.Normal)).ToInt32());
            Assert.Equal(24, Marshal.OffsetOf<VertexPositionNormalTexture>(nameof(VertexPositionNormalTexture.TextureCoordinate)).ToInt32());
        }

        [Fact]
        public void TouchLocationStateValuesMatchXna()
        {
            Assert.Equal(0, (int)TouchLocationState.Invalid);
            Assert.Equal(1, (int)TouchLocationState.Moved);
            Assert.Equal(2, (int)TouchLocationState.Pressed);
            Assert.Equal(3, (int)TouchLocationState.Released);
        }
    }
}
