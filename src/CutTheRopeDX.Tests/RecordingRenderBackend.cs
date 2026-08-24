using System.Numerics;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.Tests
{
    /// <summary>Minimal no-op renderer that records the most recently selected blend function.</summary>
    internal sealed class RecordingRenderBackend : IRenderBackend
    {
        private Color color = Color.White;

        public bool IsAvailable => true;

        public BlendingFactor LastBlendSource { get; private set; }

        public BlendingFactor LastBlendDestination { get; private set; }

        public void Enable(int cap) { }

        public void Disable(int cap) { }

        public void SetViewport(int x, int y, int width, int height) { }

        public ITextureHandle DetachRenderTarget()
        {
            return null;
        }

        public void ResetRenderTarget() { }

        public void CopyFromRenderTargetToScreen() { }

        public void SetMatrixMode(int mode) { }

        public void LoadIdentity() { }

        public void SetOrthographic(float left, float right, float bottom, float top, float near, float far) { }

        public void PushMatrix() { }

        public void PopMatrix() { }

        public void Scale(float x, float y, float z) { }

        public void Rotate(float angle, float x, float y, float z) { }

        public void Skew(float skewXDegrees, float skewYDegrees) { }

        public void Translate(float x, float y, float z) { }

        public Matrix4x4 GetModelViewMatrix()
        {
            return Matrix4x4.Identity;
        }

        public void SetColor(Color c)
        {
            color = c;
        }

        public Color GetCurrentColor()
        {
            return color;
        }

        public void SetClearColor(Color c) { }

        public void Clear(int mask) { }

        public void SetBlendFunc(BlendingFactor sfactor, BlendingFactor dfactor)
        {
            LastBlendSource = sfactor;
            LastBlendDestination = dfactor;
        }

        public void BindTexture(CTRTexture2D t) { }

        public void SetScissor(float x, float y, float width, float height) { }

        public void DrawTriangleStrip(VertexPositionColor[] vertices, int vertexCount) { }

        public void DrawTriangleStrip(VertexPositionNormalTexture[] vertices, int vertexCount) { }

        public void DrawTriangleStrip(VertexPositionColorTexture[] vertices, int vertexCount) { }

        public void DrawTriangleList(VertexPositionNormalTexture[] vertices, short[] indices, int indexCount) { }

        public void DrawTriangleList(VertexPositionColorTexture[] vertices, short[] indices, int indexCount) { }

        public void DrawLineStrip(VertexPositionColor[] vertices, int vertexCount) { }

        public VertexPositionColor[] GetLastVertices_PositionColor()
        {
            return null;
        }

        public VertexPositionNormalTexture[] GetLastVertices_PositionNormalTexture()
        {
            return null;
        }

        public void BeginFrame() { }

        public void EndFrame() { }

        public void FlushQuads() { }
    }
}
