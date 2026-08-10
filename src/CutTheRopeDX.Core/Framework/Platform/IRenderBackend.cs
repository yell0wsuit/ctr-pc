using System.Numerics;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.Framework.Platform
{
    /// <summary>
    /// Instance mirror of the GL-style rendering surface that <see cref="Renderer"/> exposes to
    /// Core code. The Desktop host installs a concrete implementation into
    /// <see cref="PlatformServices.Render"/>; headless runs leave it null so
    /// <see cref="Renderer.IsAvailable"/> reports <see langword="false"/>.
    /// </summary>
    internal interface IRenderBackend
    {
        /// <summary>
        /// Gets a value indicating whether a graphics device is present.
        /// </summary>
        bool IsAvailable { get; }

        /// <summary>
        /// Enables an OpenGL capability.
        /// </summary>
        /// <param name="cap">Capability constant.</param>
        void Enable(int cap);

        /// <summary>
        /// Disables an OpenGL capability.
        /// </summary>
        /// <param name="cap">Capability constant.</param>
        void Disable(int cap);

        /// <summary>
        /// Sets the viewport dimensions and manages render target.
        /// </summary>
        void SetViewport(int x, int y, int width, int height);

        /// <summary>
        /// Detaches and returns the current render target.
        /// </summary>
        ITextureHandle DetachRenderTarget();

        /// <summary>
        /// Clears the active render target on the graphics device so subsequent draws target the back buffer.
        /// </summary>
        void ResetRenderTarget();

        /// <summary>
        /// Copies the render target contents to the screen.
        /// </summary>
        void CopyFromRenderTargetToScreen();

        /// <summary>
        /// Sets the current matrix mode for subsequent matrix operations.
        /// </summary>
        void SetMatrixMode(int mode);

        /// <summary>
        /// Resets the current matrix to identity based on the active matrix mode.
        /// </summary>
        void LoadIdentity();

        /// <summary>
        /// Sets up an orthographic projection matrix.
        /// </summary>
        void SetOrthographic(float left, float right, float bottom, float top, float near, float far);

        /// <summary>
        /// Pushes the current model-view matrix onto the stack.
        /// </summary>
        void PushMatrix();

        /// <summary>
        /// Pops and restores the model-view matrix from the stack.
        /// </summary>
        void PopMatrix();

        /// <summary>
        /// Applies a scale transformation to the current model-view matrix.
        /// </summary>
        void Scale(float x, float y, float z);

        /// <summary>
        /// Applies a rotation transformation around the Z axis (2D rotation).
        /// </summary>
        void Rotate(float angle, float x, float y, float z);

        /// <summary>
        /// Applies a skew transformation that matches the legacy OpenGL matrix used by iOS.
        /// </summary>
        void Skew(float skewXDegrees, float skewYDegrees);

        /// <summary>
        /// Applies a translation transformation to the current model-view matrix.
        /// </summary>
        void Translate(float x, float y, float z);

        /// <summary>
        /// Returns the current model-view matrix.
        /// </summary>
        Matrix4x4 GetModelViewMatrix();

        /// <summary>
        /// Sets the current drawing color.
        /// </summary>
        void SetColor(Color c);

        /// <summary>
        /// Returns the current drawing color.
        /// </summary>
        Color GetCurrentColor();

        /// <summary>
        /// Sets the clear color for <see cref="Clear"/> operations.
        /// </summary>
        void SetClearColor(Color c);

        /// <summary>
        /// Clears the screen with the current clear color.
        /// </summary>
        void Clear(int mask);

        /// <summary>
        /// Sets the blending function for alpha blending operations.
        /// </summary>
        void SetBlendFunc(BlendingFactor sfactor, BlendingFactor dfactor);

        /// <summary>
        /// Binds a texture for subsequent rendering operations.
        /// </summary>
        void BindTexture(CTRTexture2D t);

        /// <summary>
        /// Sets the scissor rectangle for clipping, scaled to match the current viewport.
        /// </summary>
        void SetScissor(float x, float y, float width, float height);

        /// <summary>
        /// Draws a triangle strip using colored vertices with an explicit vertex count.
        /// </summary>
        void DrawTriangleStrip(VertexPositionColor[] vertices, int vertexCount);

        /// <summary>
        /// Draws a triangle strip using textured vertices with an explicit vertex count.
        /// </summary>
        void DrawTriangleStrip(VertexPositionNormalTexture[] vertices, int vertexCount);

        /// <summary>
        /// Draws a triangle strip using textured and colored vertices with an explicit vertex count.
        /// </summary>
        void DrawTriangleStrip(VertexPositionColorTexture[] vertices, int vertexCount);

        /// <summary>
        /// Draws an indexed triangle list using textured vertices with an explicit index count.
        /// </summary>
        void DrawTriangleList(VertexPositionNormalTexture[] vertices, short[] indices, int indexCount);

        /// <summary>
        /// Draws an indexed triangle list using textured and colored vertices with an explicit index count.
        /// </summary>
        void DrawTriangleList(VertexPositionColorTexture[] vertices, short[] indices, int indexCount);

        /// <summary>
        /// Draws a line strip using colored vertices with an explicit vertex count.
        /// </summary>
        void DrawLineStrip(VertexPositionColor[] vertices, int vertexCount);

        /// <summary>
        /// Returns the last drawn colored vertices (for debugging/inspection).
        /// </summary>
        VertexPositionColor[] GetLastVertices_PositionColor();

        /// <summary>
        /// Returns the last drawn textured vertices (for debugging/inspection).
        /// </summary>
        VertexPositionNormalTexture[] GetLastVertices_PositionNormalTexture();

        /// <summary>
        /// Establishes a frame boundary and discards stray queued quads from a faulted previous frame.
        /// </summary>
        void BeginFrame();

        /// <summary>
        /// Flushes remaining queued quads at the end of the frame.
        /// </summary>
        void EndFrame();

        /// <summary>
        /// Draws all queued quads as one indexed draw call, reapplying captured state unconditionally.
        /// </summary>
        void FlushQuads();
    }
}
