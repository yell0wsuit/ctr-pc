using System;
using System.Collections.Generic;

using CutTheRope.Framework;
using CutTheRope.Framework.Visual;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.Desktop
{
    internal sealed class OpenGL
    {
        public static void GlGenTextures(int n, object textures)
        {
        }

        public static void GlBindTexture(int target, uint texture)
        {
        }

        public static void GlEnable(int cap)
        {
            if (cap == 1)
            {
                s_Blend.Enable();
            }
        }

        public static void GlDisable(int cap)
        {
            if (cap == 4)
            {
                GlScissor(0.0, 0.0, FrameworkTypes.SCREEN_WIDTH, FrameworkTypes.SCREEN_HEIGHT);
            }
            if (cap == 1)
            {
                s_Blend.Disable();
            }
        }

        public static RenderTarget2D DetachRenderTarget()
        {
            RenderTarget2D renderTarget2D = s_RenderTarget;
            s_RenderTarget = null;
            return renderTarget2D;
        }

        public static void CopyFromRenderTargetToScreen()
        {
            if (Global.ScreenSizeManager.IsFullScreen && s_RenderTarget != null)
            {
                Global.GraphicsDevice.Clear(Color.Black);
                Global.SpriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, null);
                Global.SpriteBatch.Draw(s_RenderTarget, Global.ScreenSizeManager.ScaledViewRect, Color.White);
                Global.SpriteBatch.End();
            }
        }

        public static void GlViewport(double x, double y, double width, double height)
        {
            GlViewport((int)x, (int)y, (int)width, (int)height);
        }

        public static void GlViewport(int x, int y, int width, int height)
        {
            s_Viewport.X = x;
            s_Viewport.Y = y;
            s_Viewport.Width = width;
            s_Viewport.Height = height;
            if (Global.ScreenSizeManager.IsFullScreen)
            {
                if (s_RenderTarget == null || s_RenderTarget.Bounds.Width != s_Viewport.Bounds.Width || s_RenderTarget.Bounds.Height != s_Viewport.Bounds.Height)
                {
                    s_RenderTarget = new RenderTarget2D(Global.GraphicsDevice, s_Viewport.Width, s_Viewport.Height, false, SurfaceFormat.Color, DepthFormat.None);
                }
                Global.GraphicsDevice.SetRenderTarget(s_RenderTarget);
                Global.GraphicsDevice.Clear(Color.Black);
                return;
            }
            s_RenderTarget = null;
        }

        public static void GlMatrixMode(int mode)
        {
            s_glMatrixMode = mode;
        }

        public static void GlLoadIdentity()
        {
            if (s_glMatrixMode == 14)
            {
                s_matrixModelView = Matrix.Identity;
                return;
            }
            if (s_glMatrixMode == 15)
            {
                s_matrixProjection = Matrix.Identity;
                return;
            }
            if (s_glMatrixMode == 16)
            {
                throw new NotImplementedException();
            }
            if (s_glMatrixMode != 17)
            {
                return;
            }
            throw new NotImplementedException();
        }

        public static void GlOrthof(double left, double right, double bottom, double top, double near, double far)
        {
            s_matrixProjection = Matrix.CreateOrthographicOffCenter((float)left, (float)right, (float)bottom, (float)top, (float)near, (float)far);
        }

        public static void GlPopMatrix()
        {
            if (s_matrixModelViewStack.Count > 0)
            {
                int index = s_matrixModelViewStack.Count - 1;
                s_matrixModelView = s_matrixModelViewStack[index];
                s_matrixModelViewStack.RemoveAt(index);
            }
        }

        public static void GlPushMatrix()
        {
            s_matrixModelViewStack.Add(s_matrixModelView);
        }

        public static void GlScalef(double x, double y, double z)
        {
            GlScalef((float)x, (float)y, (float)z);
        }

        public static void GlScalef(float x, float y, float z)
        {
            s_matrixModelView = Matrix.CreateScale(x, y, z) * s_matrixModelView;
        }

        public static void GlRotatef(double angle, double x, double y, double z)
        {
            GlRotatef((float)angle, (float)x, (float)y, (float)z);
        }

        public static void GlRotatef(float angle, float x, float y, float z)
        {
            s_matrixModelView = Matrix.CreateRotationZ(MathHelper.ToRadians(angle)) * s_matrixModelView;
        }

        public static void GlTranslatef(double x, double y, double z)
        {
            GlTranslatef((float)x, (float)y, (float)z);
        }

        public static void GlTranslatef(float x, float y, float z)
        {
            s_matrixModelView = Matrix.CreateTranslation(x, y, 0f) * s_matrixModelView;
        }

        public static void GlBindTexture(CTRTexture2D t)
        {
            s_Texture = t;
        }

        public static void GlClearColor(Color c)
        {
            s_glClearColor = c;
        }

        public static void GlClearColorf(double red, double green, double blue, double alpha)
        {
            s_glClearColor = new Color((float)red, (float)green, (float)blue, (float)alpha);
        }

        public static void GlClear(int mask_NotUsedParam)
        {
            BlendParams.ApplyDefault();
            Global.GraphicsDevice.Clear(s_glClearColor);
        }

        public static void GlColor4f(Color c)
        {
            s_Color = c;
        }

        public static void GlBlendFunc(BlendingFactor sfactor, BlendingFactor dfactor)
        {
            s_Blend = new BlendParams(sfactor, dfactor);
        }

        public static void DrawSegment(float x1, float y1, float x2, float y2, RGBAColor color)
        {
        }

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
                s_Texture_OptimizeLastUsed = s_Texture;
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

        public static VertexPositionColor[] GetLastVertices_PositionColor()
        {
            return s_LastVertices_PositionColor;
        }

        public static void DrawTriangleStrip(VertexPositionColor[] vertices)
        {
            BasicEffect effect = GetEffect(false, true);
            if (effect.Alpha == 0f)
            {
                return;
            }
            foreach (EffectPass effectPass in effect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                DrawPrimitives(PrimitiveType.TriangleStrip, vertices, vertices.Length - 2);
            }
            s_LastVertices_PositionColor = vertices;
        }

        public static void DrawTriangleStrip(VertexPositionNormalTexture[] vertices)
        {
            BasicEffect effect = GetEffect(true, false);
            if (effect.Alpha == 0f)
            {
                return;
            }
            foreach (EffectPass effectPass in effect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                DrawPrimitives(PrimitiveType.TriangleStrip, vertices, vertices.Length - 2);
            }
            s_LastVertices_PositionNormalTexture = vertices;
        }

        public static void DrawTriangleStrip(VertexPositionColorTexture[] vertices)
        {
            BasicEffect effect = GetEffect(true, true);
            if (effect.Alpha == 0f)
            {
                return;
            }
            foreach (EffectPass effectPass in effect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                DrawPrimitives(PrimitiveType.TriangleStrip, vertices, vertices.Length - 2);
            }
        }

        public static VertexPositionNormalTexture[] GetLastVertices_PositionNormalTexture()
        {
            return s_LastVertices_PositionNormalTexture;
        }

        /// <summary>
        /// Returns the current model-view matrix that is being applied to drawable elements.
        /// </summary>
        public static Matrix GetModelViewMatrix()
        {
            return s_matrixModelView;
        }

        /// <summary>
        /// Returns the current OpenGL emulation color state.
        /// </summary>
        public static Color GetCurrentColor()
        {
            return s_Color;
        }

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

        public static void DrawLineStrip(VertexPositionColor[] vertices)
        {
            if (vertices.Length < 2)
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
                DrawPrimitives(PrimitiveType.LineStrip, vertices, vertices.Length - 1);
            }
        }

        public static void FillTexturedVertices(Quad3D[] positions, Quad2D[] texCoordinates, VertexPositionNormalTexture[] vertices, int quadCount)
        {
            int vertexIndex = 0;
            for (int i = 0; i < quadCount; i++)
            {
                float[] positionArray = positions[i].ToFloatArray();
                float[] texArray = texCoordinates[i].ToFloatArray();
                for (int vertex = 0; vertex < 4; vertex++)
                {
                    int positionOffset = vertex * 3;
                    int texOffset = vertex * 2;
                    Vector3 position = new(positionArray[positionOffset], positionArray[positionOffset + 1], positionArray[positionOffset + 2]);
                    Vector2 texCoord = new(texArray[texOffset], texArray[texOffset + 1]);
                    vertices[vertexIndex++] = new VertexPositionNormalTexture(position, normal, texCoord);
                }
            }
        }

        public static void FillTexturedColoredVertices(Quad3D[] positions, Quad2D[] texCoordinates, RGBAColor[] colors, VertexPositionColorTexture[] vertices, int quadCount)
        {
            int vertexIndex = 0;
            for (int i = 0; i < quadCount; i++)
            {
                float[] positionArray = positions[i].ToFloatArray();
                float[] texArray = texCoordinates[i].ToFloatArray();
                int colorIndex = i * 4;
                for (int vertex = 0; vertex < 4; vertex++)
                {
                    int positionOffset = vertex * 3;
                    int texOffset = vertex * 2;
                    Vector3 position = new(positionArray[positionOffset], positionArray[positionOffset + 1], positionArray[positionOffset + 2]);
                    Vector2 texCoord = new(texArray[texOffset], texArray[texOffset + 1]);
                    Color color = colors[colorIndex + vertex].ToXNA();
                    vertices[vertexIndex++] = new VertexPositionColorTexture(position, color, texCoord);
                }
            }
        }

        private static void DrawPrimitives<T>(PrimitiveType primitiveType, T[] vertices, int primitiveCount) where T : struct, IVertexType
        {
            DynamicVertexBuffer vertexBuffer = GetVertexBuffer<T>(vertices.Length);
            vertexBuffer.SetData(vertices, 0, vertices.Length, SetDataOptions.Discard);
            Global.GraphicsDevice.SetVertexBuffer(vertexBuffer);
            Global.GraphicsDevice.DrawPrimitives(primitiveType, 0, primitiveCount);
            Global.GraphicsDevice.SetVertexBuffer(null);
        }

        private static void DrawIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertices, short[] indices, int indexCount, int primitiveCount) where T : struct, IVertexType
        {
            DynamicVertexBuffer vertexBuffer = GetVertexBuffer<T>(vertices.Length);
            vertexBuffer.SetData(vertices, 0, vertices.Length, SetDataOptions.Discard);
            IndexBuffer indexBuffer = GetIndexBuffer(indexCount);
            indexBuffer.SetData(indices, 0, indexCount);
            Global.GraphicsDevice.SetVertexBuffer(vertexBuffer);
            Global.GraphicsDevice.Indices = indexBuffer;
            Global.GraphicsDevice.DrawIndexedPrimitives(primitiveType, 0, 0, primitiveCount);
            Global.GraphicsDevice.SetVertexBuffer(null);
            Global.GraphicsDevice.Indices = null;
        }

        private static DynamicVertexBuffer GetVertexBuffer<T>(int vertexCount) where T : struct, IVertexType
        {
            Type vertexType = typeof(T);
            if (s_vertexBuffer == null || s_vertexBufferType != vertexType || s_vertexBuffer.VertexCount < vertexCount)
            {
                s_vertexBuffer?.Dispose();
                s_vertexBufferType = vertexType;
                s_vertexBuffer = new DynamicVertexBuffer(Global.GraphicsDevice, default(T).VertexDeclaration, vertexCount, BufferUsage.WriteOnly);
            }
            return s_vertexBuffer;
        }

        private static IndexBuffer GetIndexBuffer(int indexCount)
        {
            if (s_indexBuffer == null || s_indexBuffer.IndexCount < indexCount)
            {
                s_indexBuffer?.Dispose();
                s_indexBuffer = new IndexBuffer(Global.GraphicsDevice, IndexElementSize.SixteenBits, indexCount, BufferUsage.WriteOnly);
            }
            return s_indexBuffer;
        }

        public static void GlScissor(double x, double y, double width, double height)
        {
            GlScissor((int)x, (int)y, (int)width, (int)height);
        }

        public static void GlScissor(int x, int y, int width, int height)
        {
            try
            {
                Rectangle bounds = Global.XnaGame.GraphicsDevice.Viewport.Bounds;
                float num = FrameworkTypes.SCREEN_WIDTH / bounds.Width;
                float num2 = FrameworkTypes.SCREEN_HEIGHT / bounds.Height;
                Rectangle value = new((int)(x / num), (int)(y / num2), (int)(width / num), (int)(height / num2));
                Global.GraphicsDevice.ScissorRectangle = Rectangle.Intersect(value, bounds);
            }
            catch (Exception)
            {
            }
        }

        public static void GlLineWidth(double width)
        {
            s_LineWidth = width;
        }

        /// <summary>
        /// Gets the SpriteBatch instance for rendering text and sprites.
        /// </summary>
        public static SpriteBatch GetSpriteBatch()
        {
            return Global.SpriteBatch;
        }

        public static void SetScissorRectangle(double x, double y, double w, double h)
        {
            SetScissorRectangle((float)x, (float)y, (float)w, (float)h);
        }

        public static void SetScissorRectangle(float x, float y, float w, float h)
        {
            GlScissor((double)x, (double)y, (double)w, (double)h);
        }

        private static RenderTarget2D s_RenderTarget;

        private static Viewport s_Viewport;

        private static int s_glMatrixMode;

        private static readonly List<Matrix> s_matrixModelViewStack = [];

        private static Matrix s_matrixModelView = Matrix.Identity;

        private static Matrix s_matrixProjection = Matrix.Identity;

        private static CTRTexture2D s_Texture;

        private static CTRTexture2D s_Texture_OptimizeLastUsed;

        private static Color s_glClearColor = Color.White;

        private static Color s_Color = Color.White;

        private static BlendParams s_Blend = new();

        private static Vector3 normal = new(0f, 0f, 1f);

        private static BasicEffect s_effectTexture;

        private static BasicEffect s_effectColor;

        private static BasicEffect s_effectTextureColor;

        private static RasterizerState s_rasterizerStateSolidColor;

        private static RasterizerState s_rasterizerStateTexture;

        private static VertexPositionColor[] s_LastVertices_PositionColor;

        private static VertexPositionNormalTexture[] s_LastVertices_PositionNormalTexture;

        private static DynamicVertexBuffer s_vertexBuffer;

        private static IndexBuffer s_indexBuffer;

        private static Type s_vertexBufferType;

        private static Rectangle ScreenRect = new(0, 0, Global.GraphicsDevice.Viewport.Width, Global.GraphicsDevice.Viewport.Height);

        private static double s_LineWidth;

    }
}
