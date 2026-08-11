using System;
using System.Numerics;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Render backend installed as <see cref="PlatformServices.Render"/> during headless test
    /// runs. Before the Core/Desktop split, every one of these call sites read
    /// <c>Global.GraphicsDevice</c> directly and would NullReferenceException if reached headless
    /// - a loud, unmissable signal that a test's logic path had wandered into rendering code. The
    /// <see cref="Renderer"/> facade's null-conditional guards turned that crash into a silent
    /// no-op instead, so a test that now draws headless would pass without anyone knowing. This
    /// double restores the loud failure.
    /// </summary>
    /// <remarks>
    /// <see cref="IsAvailable"/> deliberately does not throw. It is the documented mechanism (see
    /// <see cref="Renderer.IsAvailable"/>) by which headless-safe production code branches away
    /// from rendering entirely - <c>RootController</c>, <c>CTRRootController</c>, and
    /// <c>LoadingView</c> all query it unconditionally, every frame, by design. Reporting
    /// <see langword="false"/> here matches the semantics headless runs have always had (no
    /// device present); every other member represents an actual attempt to drive a graphics
    /// device that no headless code path should ever reach, so those throw.
    /// </remarks>
    internal sealed class ThrowingRenderBackend : IRenderBackend
    {
        private const string Message =
            "Headless test reached the render backend - a test's logic path draws when it shouldn't be able to.";

        public bool IsAvailable => false;

        public void Enable(int cap)
        {
            throw new NotSupportedException(Message);
        }

        public void Disable(int cap)
        {
            throw new NotSupportedException(Message);
        }

        public void SetViewport(int x, int y, int width, int height)
        {
            throw new NotSupportedException(Message);
        }

        public ITextureHandle DetachRenderTarget()
        {
            throw new NotSupportedException(Message);
        }

        public void ResetRenderTarget()
        {
            throw new NotSupportedException(Message);
        }

        public void CopyFromRenderTargetToScreen()
        {
            throw new NotSupportedException(Message);
        }

        public void SetMatrixMode(int mode)
        {
            throw new NotSupportedException(Message);
        }

        public void LoadIdentity()
        {
            throw new NotSupportedException(Message);
        }

        public void SetOrthographic(float left, float right, float bottom, float top, float near, float far)
        {
            throw new NotSupportedException(Message);
        }

        public void PushMatrix()
        {
            throw new NotSupportedException(Message);
        }

        public void PopMatrix()
        {
            throw new NotSupportedException(Message);
        }

        public void Scale(float x, float y, float z)
        {
            throw new NotSupportedException(Message);
        }

        public void Rotate(float angle, float x, float y, float z)
        {
            throw new NotSupportedException(Message);
        }

        public void Skew(float skewXDegrees, float skewYDegrees)
        {
            throw new NotSupportedException(Message);
        }

        public void Translate(float x, float y, float z)
        {
            throw new NotSupportedException(Message);
        }

        public Matrix4x4 GetModelViewMatrix()
        {
            throw new NotSupportedException(Message);
        }

        public void SetColor(Color c)
        {
            throw new NotSupportedException(Message);
        }

        public Color GetCurrentColor()
        {
            throw new NotSupportedException(Message);
        }

        public void SetClearColor(Color c)
        {
            throw new NotSupportedException(Message);
        }

        public void Clear(int mask)
        {
            throw new NotSupportedException(Message);
        }

        public void SetBlendFunc(BlendingFactor sfactor, BlendingFactor dfactor)
        {
            throw new NotSupportedException(Message);
        }

        public void BindTexture(CTRTexture2D t)
        {
            throw new NotSupportedException(Message);
        }

        public void SetScissor(float x, float y, float width, float height)
        {
            throw new NotSupportedException(Message);
        }

        public void DrawTriangleStrip(VertexPositionColor[] vertices, int vertexCount)
        {
            throw new NotSupportedException(Message);
        }

        public void DrawTriangleStrip(VertexPositionNormalTexture[] vertices, int vertexCount)
        {
            throw new NotSupportedException(Message);
        }

        public void DrawTriangleStrip(VertexPositionColorTexture[] vertices, int vertexCount)
        {
            throw new NotSupportedException(Message);
        }

        public void DrawTriangleList(VertexPositionNormalTexture[] vertices, short[] indices, int indexCount)
        {
            throw new NotSupportedException(Message);
        }

        public void DrawTriangleList(VertexPositionColorTexture[] vertices, short[] indices, int indexCount)
        {
            throw new NotSupportedException(Message);
        }

        public void DrawLineStrip(VertexPositionColor[] vertices, int vertexCount)
        {
            throw new NotSupportedException(Message);
        }

        public VertexPositionColor[] GetLastVertices_PositionColor()
        {
            throw new NotSupportedException(Message);
        }

        public VertexPositionNormalTexture[] GetLastVertices_PositionNormalTexture()
        {
            throw new NotSupportedException(Message);
        }

        public void BeginFrame()
        {
            throw new NotSupportedException(Message);
        }

        public void EndFrame()
        {
            throw new NotSupportedException(Message);
        }

        public void FlushQuads()
        {
            throw new NotSupportedException(Message);
        }
    }
}
