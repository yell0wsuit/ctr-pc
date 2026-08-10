using System.Numerics;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.Framework.Platform
{
    /// <summary>
    /// Core-facing static facade over the GL-style rendering API. Every member forwards to the
    /// active <see cref="PlatformServices.Render"/> backend; when none is installed (headless runs
    /// have no graphics device), draw calls no-op and queries fall back to sensible defaults,
    /// mirroring the old <c>Global.GraphicsDevice == null</c> behavior of the Desktop-only renderer
    /// this facade replaces.
    /// </summary>
    internal static class Renderer
    {
        #region OpenGL State Constants
        /// <summary>
        /// Enables/disables 2D texture mapping. When enabled, textures are applied to primitives.
        /// OpenGL equivalent: GL_TEXTURE_2D (0x0DE1)
        /// </summary>
        public const int GL_TEXTURE_2D = 0;

        /// <summary>
        /// Enables/disables alpha blending. When enabled, fragments are blended with the framebuffer
        /// using the blend function set by <see cref="SetBlendFunc"/>.
        /// OpenGL equivalent: GL_BLEND (0x0BE2)
        /// </summary>
        public const int GL_BLEND = 1;

        /// <summary>
        /// Enables/disables scissor test. When enabled, fragments outside the scissor rectangle
        /// set by <see cref="SetScissor"/> are discarded.
        /// OpenGL equivalent: GL_SCISSOR_TEST (0x0C11)
        /// </summary>
        public const int GL_SCISSOR_TEST = 4;
        #endregion

        /// <summary>
        /// Gets a value indicating whether a render backend is installed. Headless runs have none,
        /// so the few render calls that sit outside the draw loop must skip themselves.
        /// </summary>
        public static bool IsAvailable => PlatformServices.Render?.IsAvailable ?? false;

        #region Enable/Disable State

        /// <summary>
        /// Enables an OpenGL capability.
        /// </summary>
        /// <param name="cap">Capability constant: 1 = GL_BLEND</param>
        public static void Enable(int cap)
        {
            if (PlatformServices.Render is not { } r)
            {
                return;
            }
            r.Enable(cap);
        }

        /// <summary>
        /// Disables an OpenGL capability.
        /// </summary>
        /// <param name="cap">Capability constant: 1 = GL_BLEND, 4 = GL_SCISSOR_TEST</param>
        public static void Disable(int cap)
        {
            if (PlatformServices.Render is not { } r)
            {
                return;
            }
            r.Disable(cap);
        }

        #endregion

        #region Viewport and Render Target

        /// <summary>
        /// Sets the viewport dimensions and manages render target.
        /// </summary>
        public static void SetViewport(int x, int y, int width, int height)
        {
            if (PlatformServices.Render is not { } r)
            {
                return;
            }
            r.SetViewport(x, y, width, height);
        }

        /// <summary>
        /// Detaches and returns the current render target, setting the internal reference to <see langword="null"/>.
        /// </summary>
        /// <returns>The detached render target wrapped as a texture handle, or <see langword="null"/> when no render target or backend is active.</returns>
        public static ITextureHandle DetachRenderTarget()
        {
            return PlatformServices.Render is { } r ? r.DetachRenderTarget() : null;
        }

        /// <summary>
        /// Clears the active render target on the graphics device so subsequent draws target the back buffer.
        /// </summary>
        public static void ResetRenderTarget()
        {
            if (PlatformServices.Render is not { } r)
            {
                return;
            }
            r.ResetRenderTarget();
        }

        /// <summary>
        /// Copies the render target contents to the screen.
        /// </summary>
        public static void CopyFromRenderTargetToScreen()
        {
            if (PlatformServices.Render is not { } r)
            {
                return;
            }
            r.CopyFromRenderTargetToScreen();
        }

        #endregion

        #region Matrix Operations

        /// <summary>
        /// Sets the current matrix <paramref name="mode"/> for subsequent matrix operations.
        /// </summary>
        /// <param name="mode">Matrix mode: 14 = GL_MODELVIEW, 15 = GL_PROJECTION</param>
        public static void SetMatrixMode(int mode)
        {
            if (PlatformServices.Render is not { } r)
            {
                return;
            }
            r.SetMatrixMode(mode);
        }

        /// <summary>
        /// Resets the current matrix to identity based on the active matrix mode.
        /// </summary>
        public static void LoadIdentity()
        {
            if (PlatformServices.Render is not { } r)
            {
                return;
            }
            r.LoadIdentity();
        }

        /// <summary>
        /// Sets up an orthographic projection matrix.
        /// </summary>
        public static void SetOrthographic(float left, float right, float bottom, float top, float near, float far)
        {
            if (PlatformServices.Render is not { } r)
            {
                return;
            }
            r.SetOrthographic(left, right, bottom, top, near, far);
        }

        /// <summary>
        /// Pushes the current model-view matrix onto the stack.
        /// </summary>
        public static void PushMatrix()
        {
            if (PlatformServices.Render is not { } r)
            {
                return;
            }
            r.PushMatrix();
        }

        /// <summary>
        /// Pops and restores the model-view matrix from the stack.
        /// </summary>
        public static void PopMatrix()
        {
            if (PlatformServices.Render is not { } r)
            {
                return;
            }
            r.PopMatrix();
        }

        /// <summary>
        /// Applies a scale transformation to the current model-view matrix.
        /// </summary>
        public static void Scale(float x, float y, float z)
        {
            if (PlatformServices.Render is not { } r)
            {
                return;
            }
            r.Scale(x, y, z);
        }

        /// <summary>
        /// Applies a rotation transformation around the Z axis (2D rotation).
        /// </summary>
        public static void Rotate(float angle, float x, float y, float z)
        {
            if (PlatformServices.Render is not { } r)
            {
                return;
            }
            r.Rotate(angle, x, y, z);
        }

        /// <summary>
        /// Applies a skew transformation that matches the legacy OpenGL matrix used by iOS.
        /// </summary>
        public static void Skew(float skewXDegrees, float skewYDegrees)
        {
            if (PlatformServices.Render is not { } r)
            {
                return;
            }
            r.Skew(skewXDegrees, skewYDegrees);
        }

        /// <summary>
        /// Applies a translation transformation to the current model-view matrix.
        /// </summary>
        public static void Translate(float x, float y, float z)
        {
            if (PlatformServices.Render is not { } r)
            {
                return;
            }
            r.Translate(x, y, z);
        }

        /// <summary>
        /// Returns the current model-view matrix.
        /// </summary>
        /// <returns>The current model-view matrix, or the identity matrix when no backend is installed.</returns>
        public static Matrix4x4 GetModelViewMatrix()
        {
            return PlatformServices.Render is { } r ? r.GetModelViewMatrix() : Matrix4x4.Identity;
        }

        #endregion

        #region Color and Blending

        /// <summary>
        /// Sets the current drawing color.
        /// </summary>
        public static void SetColor(Color c)
        {
            if (PlatformServices.Render is not { } r)
            {
                return;
            }
            r.SetColor(c);
        }

        /// <summary>
        /// Returns the current drawing color.
        /// </summary>
        /// <returns>The current draw color, or <see cref="Color.White"/> when no backend is installed.</returns>
        public static Color GetCurrentColor()
        {
            return PlatformServices.Render is { } r ? r.GetCurrentColor() : Color.White;
        }

        /// <summary>
        /// Sets the clear color for <see cref="Clear(int)"/> operations.
        /// </summary>
        public static void SetClearColor(Color c)
        {
            if (PlatformServices.Render is not { } r)
            {
                return;
            }
            r.SetClearColor(c);
        }

        /// <summary>
        /// Clears the screen with the current clear color.
        /// </summary>
        /// <param name="mask">OpenGL clear mask (ignored, always clears color buffer).</param>
        public static void Clear(int mask)
        {
            if (PlatformServices.Render is not { } r)
            {
                return;
            }
            r.Clear(mask);
        }

        /// <summary>
        /// Sets the blending function for alpha blending operations.
        /// </summary>
        public static void SetBlendFunc(BlendingFactor sfactor, BlendingFactor dfactor)
        {
            if (PlatformServices.Render is not { } r)
            {
                return;
            }
            r.SetBlendFunc(sfactor, dfactor);
        }

        #endregion

        #region Texture Binding

        /// <summary>
        /// Binds a texture for subsequent rendering operations.
        /// </summary>
        public static void BindTexture(CTRTexture2D t)
        {
            if (PlatformServices.Render is not { } r)
            {
                return;
            }
            r.BindTexture(t);
        }

        #endregion

        #region Scissor (Clipping)

        /// <summary>
        /// Sets the scissor rectangle for clipping, scaled to match the current viewport.
        /// </summary>
        public static void SetScissor(float x, float y, float width, float height)
        {
            if (PlatformServices.Render is not { } r)
            {
                return;
            }
            r.SetScissor(x, y, width, height);
        }

        #endregion

        #region Drawing Methods

        /// <summary>
        /// Draws a triangle strip using colored <paramref name="vertices"/> (no texture).
        /// </summary>
        public static void DrawTriangleStrip(VertexPositionColor[] vertices)
        {
            DrawTriangleStrip(vertices, vertices.Length);
        }

        /// <summary>
        /// Draws a triangle strip using colored <paramref name="vertices"/> with an explicit vertex count.
        /// </summary>
        public static void DrawTriangleStrip(VertexPositionColor[] vertices, int vertexCount)
        {
            if (PlatformServices.Render is not { } r)
            {
                return;
            }
            r.DrawTriangleStrip(vertices, vertexCount);
        }

        /// <summary>
        /// Draws a triangle strip using textured <paramref name="vertices"/>.
        /// </summary>
        public static void DrawTriangleStrip(VertexPositionNormalTexture[] vertices)
        {
            DrawTriangleStrip(vertices, vertices.Length);
        }

        /// <summary>
        /// Draws a triangle strip using textured <paramref name="vertices"/> with an explicit vertex count.
        /// </summary>
        public static void DrawTriangleStrip(VertexPositionNormalTexture[] vertices, int vertexCount)
        {
            if (PlatformServices.Render is not { } r)
            {
                return;
            }
            r.DrawTriangleStrip(vertices, vertexCount);
        }

        /// <summary>
        /// Draws a triangle strip using textured and colored <paramref name="vertices"/>.
        /// </summary>
        public static void DrawTriangleStrip(VertexPositionColorTexture[] vertices)
        {
            DrawTriangleStrip(vertices, vertices.Length);
        }

        /// <summary>
        /// Draws a triangle strip using textured and colored <paramref name="vertices"/> with an explicit vertex count.
        /// </summary>
        public static void DrawTriangleStrip(VertexPositionColorTexture[] vertices, int vertexCount)
        {
            if (PlatformServices.Render is not { } r)
            {
                return;
            }
            r.DrawTriangleStrip(vertices, vertexCount);
        }

        /// <summary>
        /// Draws an indexed triangle list using textured <paramref name="vertices"/>.
        /// </summary>
        public static void DrawTriangleList(VertexPositionNormalTexture[] vertices, short[] indices)
        {
            DrawTriangleList(vertices, indices, indices.Length);
        }

        /// <summary>
        /// Draws an indexed triangle list using textured <paramref name="vertices"/> with explicit index count.
        /// </summary>
        public static void DrawTriangleList(VertexPositionNormalTexture[] vertices, short[] indices, int indexCount)
        {
            if (PlatformServices.Render is not { } r)
            {
                return;
            }
            r.DrawTriangleList(vertices, indices, indexCount);
        }

        /// <summary>
        /// Draws an indexed triangle list using textured and colored <paramref name="vertices"/>.
        /// </summary>
        public static void DrawTriangleList(VertexPositionColorTexture[] vertices, short[] indices, int indexCount)
        {
            if (PlatformServices.Render is not { } r)
            {
                return;
            }
            r.DrawTriangleList(vertices, indices, indexCount);
        }

        /// <summary>
        /// Draws a line strip using colored <paramref name="vertices"/>.
        /// </summary>
        public static void DrawLineStrip(VertexPositionColor[] vertices)
        {
            DrawLineStrip(vertices, vertices.Length);
        }

        /// <summary>
        /// Draws a line strip using colored <paramref name="vertices"/> with an explicit vertex count.
        /// </summary>
        public static void DrawLineStrip(VertexPositionColor[] vertices, int vertexCount)
        {
            if (PlatformServices.Render is not { } r)
            {
                return;
            }
            r.DrawLineStrip(vertices, vertexCount);
        }

        /// <summary>
        /// Draws a line segment (stub - not implemented).
        /// Used for debug visualization.
        /// </summary>
        /// <param name="_">Segment start X.</param>
        /// <param name="__">Segment start Y.</param>
        /// <param name="___">Segment end X.</param>
        /// <param name="____">Segment end Y.</param>
        /// <param name="_____">Segment color.</param>
        public static void DrawSegment(float _, float __, float ___, float ____, RGBAColor _____)
        {
            // Stub: Debug visualization not implemented
        }

        #endregion

        #region Vertex Buffer Helpers

        /// <summary>
        /// Fills a vertex array with textured quad data from <paramref name="positions"/> and <paramref name="texCoordinates"/>.
        /// Pure vertex-array construction; no device access, so it lives on the facade rather than the backend.
        /// </summary>
        /// <param name="positions">Array of 3D quad positions.</param>
        /// <param name="texCoordinates">Array of 2D texture coordinates.</param>
        /// <param name="vertices">Output vertex array (must be pre-allocated with quadCount * 4 elements).</param>
        /// <param name="quadCount">Number of quads to process.</param>
        public static void FillTexturedVertices(Quad3D[] positions, Quad2D[] texCoordinates, VertexPositionNormalTexture[] vertices, int quadCount)
        {
            int vertexIndex = 0;
            for (int i = 0; i < quadCount; i++)
            {
                Quad3D position = positions[i];
                Vector3 pos0 = new(position.BlX, position.BlY, position.BlZ);
                Vector3 pos1 = new(position.BrX, position.BrY, position.BrZ);
                Vector3 pos2 = new(position.TlX, position.TlY, position.TlZ);
                Vector3 pos3 = new(position.TrX, position.TrY, position.TrZ);
                Quad2D tex = texCoordinates[i];
                Vector2 tex0 = new(tex.tlX, tex.tlY);
                Vector2 tex1 = new(tex.trX, tex.trY);
                Vector2 tex2 = new(tex.blX, tex.blY);
                Vector2 tex3 = new(tex.brX, tex.brY);
                for (int vertex = 0; vertex < 4; vertex++)
                {
                    Vector3 positionValue = vertex switch
                    {
                        0 => pos0,
                        1 => pos1,
                        2 => pos2,
                        _ => pos3
                    };
                    Vector2 texCoord = vertex switch
                    {
                        0 => tex0,
                        1 => tex1,
                        2 => tex2,
                        _ => tex3
                    };
                    vertices[vertexIndex++] = new VertexPositionNormalTexture(positionValue, s_normal, texCoord);
                }
            }
        }

        /// <summary>
        /// Fills a vertex array with textured and colored quad data.
        /// Pure vertex-array construction; no device access, so it lives on the facade rather than the backend.
        /// </summary>
        /// <param name="positions">Array of 3D quad positions.</param>
        /// <param name="texCoordinates">Array of 2D texture coordinates.</param>
        /// <param name="colors">Array of vertex colors (4 per quad).</param>
        /// <param name="vertices">Output vertex array (must be pre-allocated with quadCount * 4 elements).</param>
        /// <param name="quadCount">Number of quads to process.</param>
        public static void FillTexturedColoredVertices(Quad3D[] positions, Quad2D[] texCoordinates, RGBAColor[] colors, VertexPositionColorTexture[] vertices, int quadCount)
        {
            int vertexIndex = 0;
            for (int i = 0; i < quadCount; i++)
            {
                Quad3D position = positions[i];
                Vector3 pos0 = new(position.BlX, position.BlY, position.BlZ);
                Vector3 pos1 = new(position.BrX, position.BrY, position.BrZ);
                Vector3 pos2 = new(position.TlX, position.TlY, position.TlZ);
                Vector3 pos3 = new(position.TrX, position.TrY, position.TrZ);
                Quad2D tex = texCoordinates[i];
                Vector2 tex0 = new(tex.tlX, tex.tlY);
                Vector2 tex1 = new(tex.trX, tex.trY);
                Vector2 tex2 = new(tex.blX, tex.blY);
                Vector2 tex3 = new(tex.brX, tex.brY);
                int colorIndex = i * 4;
                for (int vertex = 0; vertex < 4; vertex++)
                {
                    Vector3 positionValue = vertex switch
                    {
                        0 => pos0,
                        1 => pos1,
                        2 => pos2,
                        _ => pos3
                    };
                    Vector2 texCoord = vertex switch
                    {
                        0 => tex0,
                        1 => tex1,
                        2 => tex2,
                        _ => tex3
                    };
                    Color color = colors[colorIndex + vertex].ToColor();
                    vertices[vertexIndex++] = new VertexPositionColorTexture(positionValue, color, texCoord);
                }
            }
        }

        /// <summary>
        /// Returns the last drawn colored vertices (for debugging/inspection).
        /// </summary>
        /// <returns>The last colored vertex array submitted by a matching draw call, or <see langword="null"/> when no backend is installed.</returns>
        public static VertexPositionColor[] GetLastVertices_PositionColor()
        {
            return PlatformServices.Render is { } r ? r.GetLastVertices_PositionColor() : null;
        }

        /// <summary>
        /// Returns the last drawn textured vertices (for debugging/inspection).
        /// </summary>
        /// <returns>The last textured vertex array submitted by a matching draw call, or <see langword="null"/> when no backend is installed.</returns>
        public static VertexPositionNormalTexture[] GetLastVertices_PositionNormalTexture()
        {
            return PlatformServices.Render is { } r ? r.GetLastVertices_PositionNormalTexture() : null;
        }

        #endregion

        #region Quad Batching

        /// <summary>
        /// Establishes a frame boundary and discards stray queued quads from a faulted previous frame.
        /// </summary>
        public static void BeginFrame()
        {
            if (PlatformServices.Render is not { } r)
            {
                return;
            }
            r.BeginFrame();
        }

        /// <summary>
        /// Flushes remaining queued quads at the end of the frame.
        /// </summary>
        public static void EndFrame()
        {
            if (PlatformServices.Render is not { } r)
            {
                return;
            }
            r.EndFrame();
        }

        /// <summary>
        /// Draws all queued quads as one indexed draw call, reapplying captured state unconditionally.
        /// </summary>
        public static void FlushQuads()
        {
            if (PlatformServices.Render is not { } r)
            {
                return;
            }
            r.FlushQuads();
        }

        #endregion

        /// <summary>
        /// Shared surface normal used for textured quad vertices built by <see cref="FillTexturedVertices"/>.
        /// </summary>
        private static readonly Vector3 s_normal = new(0f, 0f, 1f);
    }
}
