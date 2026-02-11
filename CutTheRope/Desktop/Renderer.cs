using System;
using System.Collections.Generic;

using CutTheRope.Framework;
using CutTheRope.Framework.Visual;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.Desktop
{
    /// <summary>
    /// Provides OpenGL ES 1.x emulation layer for MonoGame/XNA rendering.
    /// This class translates legacy OpenGL-style API calls to modern MonoGame primitives,
    /// using vertex buffers for efficient GPU rendering.
    /// </summary>
    internal sealed class Renderer
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

        /// <summary>
        /// Selects the modelview matrix stack for subsequent matrix operations.
        /// OpenGL equivalent: GL_MODELVIEW (0x1700)
        /// </summary>
        private const int MODE_MODELVIEW = 14;

        /// <summary>
        /// Selects the projection matrix stack for subsequent matrix operations.
        /// OpenGL equivalent: GL_PROJECTION (0x1701)
        /// </summary>
        private const int MODE_PROJECTION = 15;
        #endregion

        #region Initialization

        /// <summary>
        /// Initializes the OpenGL emulation layer. Must be called before any rendering operations.
        /// Sets up BasicEffect shaders and rasterizer states.
        /// </summary>
        public static void Init()
        {
            InitRasterizerState();
            s_effectTexture = new BasicEffect(Global.GraphicsDevice)
            {
                VertexColorEnabled = false,
                TextureEnabled = true,
                View = Matrix.Identity
            };
            s_effectTextureColor = new BasicEffect(Global.GraphicsDevice)
            {
                VertexColorEnabled = true,
                TextureEnabled = true,
                View = Matrix.Identity
            };
            s_effectColor = new BasicEffect(Global.GraphicsDevice)
            {
                VertexColorEnabled = true,
                TextureEnabled = false,
                Alpha = 1f,
                Texture = null,
                View = Matrix.Identity
            };
        }

        private static void InitRasterizerState()
        {
            s_rasterizerStateSolidColor = new RasterizerState
            {
                FillMode = FillMode.Solid,
                CullMode = CullMode.None,
                ScissorTestEnable = true
            };
            s_rasterizerStateTexture = new RasterizerState
            {
                CullMode = CullMode.None,
                ScissorTestEnable = true
            };
        }

        #endregion

        #region Enable/Disable State

        /// <summary>
        /// Enables an OpenGL capability.
        /// </summary>
        /// <param name="cap">Capability constant: 1 = GL_BLEND</param>
        public static void Enable(int cap)
        {
            if (cap == GL_BLEND)
            {
                s_Blend.Enable();
            }
        }

        /// <summary>
        /// Disables an OpenGL capability.
        /// </summary>
        /// <param name="cap">Capability constant: 1 = GL_BLEND, 4 = GL_SCISSOR_TEST</param>
        public static void Disable(int cap)
        {
            if (cap == GL_SCISSOR_TEST)
            {
                SetScissor(0f, 0f, FrameworkTypes.SCREEN_WIDTH, FrameworkTypes.SCREEN_HEIGHT);
            }
            if (cap == GL_BLEND)
            {
                s_Blend.Disable();
            }
        }

        #endregion

        #region Viewport and Render Target
        /// <summary>
        /// Sets the viewport dimensions and manages render target.
        /// Always creates a render target matching the viewport size for proper scaling.
        /// </summary>
        public static void SetViewport(int x, int y, int width, int height)
        {
            if (width <= 0 || height <= 0)
            {
                return;
            }

            s_Viewport.X = x;
            s_Viewport.Y = y;
            s_Viewport.Width = width;
            s_Viewport.Height = height;
            // Always use render target for proper scaling in both windowed and fullscreen modes
            if (s_RenderTarget == null || s_RenderTarget.Bounds.Width != s_Viewport.Bounds.Width || s_RenderTarget.Bounds.Height != s_Viewport.Bounds.Height)
            {
                s_RenderTarget?.Dispose();
                s_RenderTarget = new RenderTarget2D(Global.GraphicsDevice, s_Viewport.Width, s_Viewport.Height, false, SurfaceFormat.Color, DepthFormat.None);
            }
            Global.GraphicsDevice.SetRenderTarget(s_RenderTarget);
            Global.GraphicsDevice.Clear(Color.Black);
        }

        /// <summary>
        /// Detaches and returns the current render target, setting the internal reference to null.
        /// Used for screen capture operations.
        /// </summary>
        public static RenderTarget2D DetachRenderTarget()
        {
            RenderTarget2D renderTarget2D = s_RenderTarget;
            s_RenderTarget = null;
            return renderTarget2D;
        }

        /// <summary>
        /// Copies the render target contents to the screen.
        /// Applies scaling to fit the display in both windowed and fullscreen modes.
        /// </summary>
        public static void CopyFromRenderTargetToScreen()
        {
            if (s_RenderTarget != null)
            {
                Global.GraphicsDevice.Clear(Color.Black);
                Global.SpriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, null);
                Global.SpriteBatch.Draw(s_RenderTarget, Global.ScreenSizeManager.ScaledViewRect, Color.White);
                Global.SpriteBatch.End();
            }
        }

        #endregion

        #region Matrix Operations

        /// <summary>
        /// Sets the current matrix mode for subsequent matrix operations.
        /// </summary>
        /// <param name="mode">Matrix mode: 14 = GL_MODELVIEW, 15 = GL_PROJECTION</param>
        public static void SetMatrixMode(int mode)
        {
            s_glMatrixMode = mode;
        }

        /// <summary>
        /// Resets the current matrix to identity based on the active matrix mode.
        /// </summary>
        public static void LoadIdentity()
        {
            if (s_glMatrixMode == MODE_MODELVIEW)
            {
                s_matrixModelView = Matrix.Identity;
                return;
            }
            if (s_glMatrixMode == MODE_PROJECTION)
            {
                s_matrixProjection = Matrix.Identity;
                return;
            }
            if (s_glMatrixMode is 16 or 17)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Sets up an orthographic projection matrix.
        /// </summary>
        public static void SetOrthographic(float left, float right, float bottom, float top, float near, float far)
        {
            s_matrixProjection = Matrix.CreateOrthographicOffCenter(left, right, bottom, top, near, far);
        }

        /// <summary>
        /// Pushes the current model-view matrix onto the stack.
        /// </summary>
        public static void PushMatrix()
        {
            s_matrixModelViewStack.Add(s_matrixModelView);
        }

        /// <summary>
        /// Pops and restores the model-view matrix from the stack.
        /// </summary>
        public static void PopMatrix()
        {
            if (s_matrixModelViewStack.Count > 0)
            {
                int index = s_matrixModelViewStack.Count - 1;
                s_matrixModelView = s_matrixModelViewStack[index];
                s_matrixModelViewStack.RemoveAt(index);
            }
        }

        /// <summary>
        /// Applies a scale transformation to the current model-view matrix.
        /// </summary>
        public static void Scale(float x, float y, float z)
        {
            s_matrixModelView = Matrix.CreateScale(x, y, z) * s_matrixModelView;
        }

        /// <summary>
        /// Applies a rotation transformation around the Z axis (2D rotation).
        /// </summary>
        /// <param name="angle">Rotation angle in degrees.</param>
        public static void Rotate(float angle, float _, float _1, float _2)
        {
            s_matrixModelView = Matrix.CreateRotationZ(MathHelper.ToRadians(angle)) * s_matrixModelView;
        }

        /// <summary>
        /// Applies a translation transformation to the current model-view matrix.
        /// Z component is ignored for 2D rendering.
        /// </summary>
        public static void Translate(float x, float y, float _)
        {
            s_matrixModelView = Matrix.CreateTranslation(x, y, 0f) * s_matrixModelView;
        }

        /// <summary>
        /// Returns the current model-view matrix.
        /// </summary>
        public static Matrix GetModelViewMatrix()
        {
            return s_matrixModelView;
        }

        #endregion

        #region Color and Blending

        /// <summary>
        /// Sets the current drawing color.
        /// </summary>
        public static void SetColor(Color c)
        {
            s_Color = c;
        }

        /// <summary>
        /// Returns the current drawing color.
        /// </summary>
        public static Color GetCurrentColor()
        {
            return s_Color;
        }

        /// <summary>
        /// Sets the clear color for GlClear operations.
        /// </summary>
        public static void SetClearColor(Color c)
        {
            s_glClearColor = c;
        }

        /// <summary>
        /// Clears the screen with the current clear color.
        /// </summary>
        /// <param name="mask_NotUsedParam">OpenGL clear mask (ignored, always clears color buffer).</param>
        public static void Clear(int _)
        {
            BlendParams.ApplyDefault();
            Global.GraphicsDevice.Clear(s_glClearColor);
        }

        /// <summary>
        /// Sets the blending function for alpha blending operations.
        /// </summary>
        public static void SetBlendFunc(BlendingFactor sfactor, BlendingFactor dfactor)
        {
            s_Blend = new BlendParams(sfactor, dfactor);
        }

        #endregion

        #region Texture Binding

        /// <summary>
        /// Binds a texture for subsequent rendering operations.
        /// </summary>
        public static void BindTexture(CTRTexture2D t)
        {
            s_Texture = t;
        }

        #endregion

        #region Scissor (Clipping)

        /// <summary>
        /// Sets the scissor rectangle for clipping, scaled to match the current viewport.
        /// </summary>
        public static void SetScissor(float x, float y, float width, float height)
        {
            try
            {
                Rectangle bounds = Global.XnaGame.GraphicsDevice.Viewport.Bounds;
                float scaleX = FrameworkTypes.SCREEN_WIDTH / bounds.Width;
                float scaleY = FrameworkTypes.SCREEN_HEIGHT / bounds.Height;
                Rectangle scissorRect = new((int)(x / scaleX), (int)(y / scaleY), (int)(width / scaleX), (int)(height / scaleY));
                Global.GraphicsDevice.ScissorRectangle = Rectangle.Intersect(scissorRect, bounds);
            }
            catch (Exception)
            {
            }
        }

        #endregion

        #region Drawing Methods

        /// <summary>
        /// Draws a triangle strip using colored vertices (no texture).
        /// </summary>
        public static void DrawTriangleStrip(VertexPositionColor[] vertices)
        {
            DrawTriangleStrip(vertices, vertices.Length);
        }

        public static void DrawTriangleStrip(VertexPositionColor[] vertices, int vertexCount)
        {
            if (vertexCount < 3)
            {
                return;
            }
            BasicEffect effect = GetEffect(false, true);
            if (effect.Alpha == 0f)
            {
                return;
            }
            foreach (EffectPass effectPass in effect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                DrawPrimitives(PrimitiveType.TriangleStrip, vertices, vertexCount, vertexCount - 2);
            }
            s_LastVertices_PositionColor = vertices;
        }

        /// <summary>
        /// Draws a triangle strip using textured vertices.
        /// </summary>
        public static void DrawTriangleStrip(VertexPositionNormalTexture[] vertices)
        {
            DrawTriangleStrip(vertices, vertices.Length);
        }

        public static void DrawTriangleStrip(VertexPositionNormalTexture[] vertices, int vertexCount)
        {
            if (vertexCount < 3)
            {
                return;
            }
            BasicEffect effect = GetEffect(true, false);
            if (effect.Alpha == 0f)
            {
                return;
            }
            foreach (EffectPass effectPass in effect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                DrawPrimitives(PrimitiveType.TriangleStrip, vertices, vertexCount, vertexCount - 2);
            }
            s_LastVertices_PositionNormalTexture = vertices;
        }

        /// <summary>
        /// Draws a triangle strip using textured and colored vertices.
        /// </summary>
        public static void DrawTriangleStrip(VertexPositionColorTexture[] vertices)
        {
            DrawTriangleStrip(vertices, vertices.Length);
        }

        public static void DrawTriangleStrip(VertexPositionColorTexture[] vertices, int vertexCount)
        {
            if (vertexCount < 3)
            {
                return;
            }
            BasicEffect effect = GetEffect(true, true);
            if (effect.Alpha == 0f)
            {
                return;
            }
            foreach (EffectPass effectPass in effect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                DrawPrimitives(PrimitiveType.TriangleStrip, vertices, vertexCount, vertexCount - 2);
            }
        }

        /// <summary>
        /// Draws an indexed triangle list using textured vertices.
        /// </summary>
        public static void DrawTriangleList(VertexPositionNormalTexture[] vertices, short[] indices)
        {
            BasicEffect effect = GetEffect(true, false);
            if (effect.Alpha == 0f)
            {
                return;
            }
            foreach (EffectPass effectPass in effect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                DrawIndexedPrimitives(PrimitiveType.TriangleList, vertices, indices, indices.Length, indices.Length / 3);
            }
            s_LastVertices_PositionNormalTexture = vertices;
        }

        /// <summary>
        /// Draws an indexed triangle list using textured vertices with explicit index count.
        /// </summary>
        public static void DrawTriangleList(VertexPositionNormalTexture[] vertices, short[] indices, int indexCount)
        {
            BasicEffect effect = GetEffect(true, false);
            if (effect.Alpha == 0f)
            {
                return;
            }
            foreach (EffectPass effectPass in effect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                DrawIndexedPrimitives(PrimitiveType.TriangleList, vertices, indices, indexCount, indexCount / 3);
            }
            s_LastVertices_PositionNormalTexture = vertices;
        }

        /// <summary>
        /// Draws an indexed triangle list using textured and colored vertices.
        /// </summary>
        public static void DrawTriangleList(VertexPositionColorTexture[] vertices, short[] indices, int indexCount)
        {
            if (indexCount == 0)
            {
                return;
            }
            BasicEffect effect = GetEffect(true, true);
            if (effect.Alpha == 0f)
            {
                return;
            }
            foreach (EffectPass effectPass in effect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                DrawIndexedPrimitives(PrimitiveType.TriangleList, vertices, indices, indexCount, indexCount / 3);
            }
        }

        /// <summary>
        /// Draws a line strip using colored vertices.
        /// </summary>
        public static void DrawLineStrip(VertexPositionColor[] vertices)
        {
            DrawLineStrip(vertices, vertices.Length);
        }

        public static void DrawLineStrip(VertexPositionColor[] vertices, int vertexCount)
        {
            if (vertexCount < 2)
            {
                return;
            }
            BasicEffect effect = GetEffect(false, true);
            if (effect.Alpha == 0f)
            {
                return;
            }
            foreach (EffectPass effectPass in effect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                DrawPrimitives(PrimitiveType.LineStrip, vertices, vertexCount, vertexCount - 1);
            }
        }

        /// <summary>
        /// Draws a line segment (stub - not implemented).
        /// Used for debug visualization.
        /// </summary>
        public static void DrawSegment(float _, float __, float ___, float ____, RGBAColor _____)
        {
            // Stub: Debug visualization not implemented
        }

        #endregion

        #region Vertex Buffer Helpers

        /// <summary>
        /// Fills a vertex array with textured quad data from Quad3D positions and Quad2D texture coordinates.
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
                    Color color = colors[colorIndex + vertex].ToXNA();
                    vertices[vertexIndex++] = new VertexPositionColorTexture(positionValue, color, texCoord);
                }
            }
        }

        /// <summary>
        /// Returns the last drawn colored vertices (for debugging/inspection).
        /// </summary>
        public static VertexPositionColor[] GetLastVertices_PositionColor()
        {
            return s_LastVertices_PositionColor;
        }

        /// <summary>
        /// Returns the last drawn textured vertices (for debugging/inspection).
        /// </summary>
        public static VertexPositionNormalTexture[] GetLastVertices_PositionNormalTexture()
        {
            return s_LastVertices_PositionNormalTexture;
        }

        #endregion

        #region Utility

        /// <summary>
        /// Gets the SpriteBatch instance for text and sprite rendering.
        /// </summary>
        public static SpriteBatch GetSpriteBatch()
        {
            return Global.SpriteBatch;
        }

        #endregion

        #region Private Rendering Implementation

        private static BasicEffect GetEffect(bool useTexture, bool useColor)
        {
            BasicEffect basicEffect = !useTexture ? s_effectColor : useColor ? s_effectTextureColor : s_effectTexture;
            if (useTexture)
            {
                basicEffect.Alpha = s_Color.A / 255f;
                if (basicEffect.Alpha == 0f)
                {
                    return basicEffect;
                }
                basicEffect.Texture = s_Texture.xnaTexture_;
                basicEffect.DiffuseColor = s_Color.ToVector3();
                Global.GraphicsDevice.RasterizerState = s_rasterizerStateTexture;
                Global.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
            }
            else
            {
                Global.GraphicsDevice.RasterizerState = s_rasterizerStateSolidColor;
            }
            basicEffect.World = s_matrixModelView;
            basicEffect.Projection = s_matrixProjection;
            s_Blend.Apply();
            return basicEffect;
        }

        private static void DrawPrimitives<T>(PrimitiveType primitiveType, T[] vertices, int vertexCount, int primitiveCount) where T : struct, IVertexType
        {
            DynamicVertexBuffer vertexBuffer = GetVertexBuffer<T>(vertexCount);
            vertexBuffer.SetData(vertices, 0, vertexCount, SetDataOptions.Discard);
            Global.GraphicsDevice.SetVertexBuffer(vertexBuffer);
            Global.GraphicsDevice.DrawPrimitives(primitiveType, 0, primitiveCount);
            Global.GraphicsDevice.SetVertexBuffer(null);
        }

        private static void DrawIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertices, short[] indices, int indexCount, int primitiveCount) where T : struct, IVertexType
        {
            DynamicVertexBuffer vertexBuffer = GetVertexBuffer<T>(vertices.Length);
            vertexBuffer.SetData(vertices, 0, vertices.Length, SetDataOptions.Discard);
            IndexBuffer indexBuffer = GetIndexBuffer(indexCount, indices);
            Global.GraphicsDevice.SetVertexBuffer(vertexBuffer);
            Global.GraphicsDevice.Indices = indexBuffer;
            Global.GraphicsDevice.DrawIndexedPrimitives(primitiveType, 0, 0, primitiveCount);
            Global.GraphicsDevice.SetVertexBuffer(null);
            Global.GraphicsDevice.Indices = null;
        }

        private static DynamicVertexBuffer GetVertexBuffer<T>(int vertexCount) where T : struct, IVertexType
        {
            Type vertexType = typeof(T);
            if (!s_vertexBuffers.TryGetValue(vertexType, out DynamicVertexBuffer vertexBuffer) || vertexBuffer.VertexCount < vertexCount)
            {
                vertexBuffer?.Dispose();
                vertexBuffer = new DynamicVertexBuffer(Global.GraphicsDevice, default(T).VertexDeclaration, vertexCount, BufferUsage.WriteOnly);
                s_vertexBuffers[vertexType] = vertexBuffer;
            }
            return vertexBuffer;
        }

        private static IndexBuffer GetIndexBuffer(int indexCount, short[] indices)
        {
            if (s_indexBuffer == null || s_indexBuffer.IndexCount < indexCount)
            {
                s_indexBuffer?.Dispose();
                s_indexBuffer = new IndexBuffer(Global.GraphicsDevice, IndexElementSize.SixteenBits, indexCount, BufferUsage.WriteOnly);
                s_indexBuffer.SetData(indices, 0, indexCount);
            }
            return s_indexBuffer;
        }

        #endregion

        #region Static Fields

        // Render target for fullscreen mode
        private static RenderTarget2D s_RenderTarget;
        private static Viewport s_Viewport;

        // Matrix state
        private static int s_glMatrixMode;
        private static readonly List<Matrix> s_matrixModelViewStack = [];
        private static Matrix s_matrixModelView = Matrix.Identity;
        private static Matrix s_matrixProjection = Matrix.Identity;

        // Texture state
        private static CTRTexture2D s_Texture;

        // Color state
        private static Color s_glClearColor = Color.White;
        private static Color s_Color = Color.White;

        // Blend state
        private static BlendParams s_Blend = new();

        // Shader effects
        private static BasicEffect s_effectTexture;
        private static BasicEffect s_effectColor;
        private static BasicEffect s_effectTextureColor;

        // Rasterizer states
        private static RasterizerState s_rasterizerStateSolidColor;
        private static RasterizerState s_rasterizerStateTexture;

        // Vertex buffers (reused for performance, per vertex type)
        private static readonly Dictionary<Type, DynamicVertexBuffer> s_vertexBuffers = [];
        private static IndexBuffer s_indexBuffer;

        // Last drawn vertices (for debugging)
        private static VertexPositionColor[] s_LastVertices_PositionColor;
        private static VertexPositionNormalTexture[] s_LastVertices_PositionNormalTexture;

        // Constants
        private static readonly Vector3 s_normal = new(0f, 0f, 1f);

        #endregion
    }
}
