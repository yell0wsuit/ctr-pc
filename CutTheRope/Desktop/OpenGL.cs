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
            s_glServerSideFlags[cap] = true;
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
            s_glServerSideFlags[cap] = false;
        }

        public static void GlEnableClientState(int cap)
        {
            s_glClientStateFlags[cap] = true;
        }

        public static void GlDisableClientState(int cap)
        {
            s_glClientStateFlags[cap] = false;
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

        public static void GlGenBuffers(int n, ref uint buffer)
        {
        }

        public static void GlGenBuffers(int n, ref uint[] buffers)
        {
        }

        public static void GlDeleteBuffers(int n, ref uint[] buffers)
        {
        }

        public static void GlDeleteBuffers(int n, ref uint buffers)
        {
        }

        public static void GlBindBuffer(int target, uint buffer)
        {
        }

        public static void GlBufferData(int target, RGBAColor[] data, int usage)
        {
        }

        public static void GlBufferData(int target, PointSprite[] data, int usage)
        {
        }

        public static void GlColorPointer(int size, int type, int stride, RGBAColor[] pointer)
        {
            s_GLColorPointer = pointer;
        }

        public static void GlVertexPointer(int size, int type, int stride, object pointer)
        {
            s_GLVertexPointer = new GLVertexPointer(size, pointer);
        }

        public static void GlTexCoordPointer(int size, int type, int stride, object pointer)
        {
            s_GLTexCoordPointer = new GLTexCoordPointer(size, pointer);
        }

        public static void GlDrawArrays(int mode, int first, int count)
        {
            if (mode == 8)
            {
                DrawTriangleStrip(first, count);
                return;
            }
            if (mode - 9 > 1)
            {
                throw new NotImplementedException();
            }
        }

        public static void GlColorPointer_setAdditive(int size)
        {
            s_GLColorPointer = new RGBAColor[size];
            s_GLColorPointer_additive_position = 0;
        }

        public static void GlColorPointer_add(int size, int type, int stride, RGBAColor[] pointer)
        {
            pointer.CopyTo(s_GLColorPointer, s_GLColorPointer_additive_position);
            s_GLColorPointer_additive_position += pointer.Length;
        }

        public static void GlVertexPointer_setAdditive(int size, int type, int stride, int length)
        {
            s_GLVertexPointer = new GLVertexPointer(size, new float[length]);
            s_GLVertexPointer_additive_position = 0;
        }

        public static void GlVertexPointer_add(int size, int type, int stride, float[] pointer)
        {
            pointer.CopyTo(s_GLVertexPointer.pointer_, s_GLVertexPointer_additive_position);
            s_GLVertexPointer_additive_position += pointer.Length;
        }

        private static VertexPositionColor[] ConstructColorVertices()
        {
            VertexPositionColor[] array = new VertexPositionColor[s_GLVertexPointer.Count];
            int num = 0;
            Vector3 position = default;
            for (int i = 0; i < array.Length; i++)
            {
                position.X = s_GLVertexPointer.pointer_[num++];
                position.Y = s_GLVertexPointer.pointer_[num++];
                position.Z = s_GLVertexPointer.size_ == 2 ? 0f : s_GLVertexPointer.pointer_[num++];
                array[i] = new VertexPositionColor(position, s_GLColorPointer[i].ToXNA());
            }
            return array;
        }

        private static VertexPositionColor[] ConstructCurrentColorVertices()
        {
            VertexPositionColor[] array = new VertexPositionColor[s_GLVertexPointer.Count];
            int num = 0;
            for (int i = 0; i < array.Length; i++)
            {
                Vector3 position = default;
                position.X = s_GLVertexPointer.pointer_[num++];
                position.Y = s_GLVertexPointer.pointer_[num++];
                position.Z = s_GLVertexPointer.size_ == 2 ? 0f : s_GLVertexPointer.pointer_[num++];
                array[i] = new VertexPositionColor(position, s_Color);
            }
            s_GLVertexPointer = null;
            return array;
        }

        private static VertexPositionNormalTexture[] ConstructTexturedVertices()
        {
            VertexPositionNormalTexture[] array = new VertexPositionNormalTexture[s_GLVertexPointer.Count];
            int num = 0;
            int num2 = 0;
            for (int i = 0; i < array.Length; i++)
            {
                Vector3 position = default;
                position.X = s_GLVertexPointer.pointer_[num++];
                position.Y = s_GLVertexPointer.pointer_[num++];
                position.Z = s_GLVertexPointer.size_ == 2 ? 0f : s_GLVertexPointer.pointer_[num++];
                Vector2 textureCoordinate = default;
                textureCoordinate.X = s_GLTexCoordPointer.pointer_[num2++];
                textureCoordinate.Y = s_GLTexCoordPointer.pointer_[num2++];
                int num3 = 2;
                while (num3 < s_GLTexCoordPointer.size_)
                {
                    num3++;
                    num2++;
                }
                array[i] = new VertexPositionNormalTexture(position, normal, textureCoordinate);
            }
            s_GLTexCoordPointer = null;
            s_GLVertexPointer = null;
            return array;
        }

        private static VertexPositionColorTexture[] ConstructTexturedColoredVertices(int vertexCount)
        {
            VertexPositionColorTexture[] array = new VertexPositionColorTexture[vertexCount];
            int num = 0;
            int num2 = 0;
            for (int i = 0; i < array.Length; i++)
            {
                Vector3 position = default;
                position.X = s_GLVertexPointer.pointer_[num++];
                position.Y = s_GLVertexPointer.pointer_[num++];
                position.Z = s_GLVertexPointer.size_ == 2 ? 0f : s_GLVertexPointer.pointer_[num++];
                Vector2 textureCoordinate = default;
                textureCoordinate.X = s_GLTexCoordPointer.pointer_[num2++];
                textureCoordinate.Y = s_GLTexCoordPointer.pointer_[num2++];
                int num3 = 2;
                while (num3 < s_GLTexCoordPointer.size_)
                {
                    num3++;
                    num2++;
                }
                Color color = s_GLColorPointer[i].ToXNA();
                array[i] = new VertexPositionColorTexture(position, color, textureCoordinate);
            }
            s_GLTexCoordPointer = null;
            s_GLVertexPointer = null;
            return array;
        }

        public static void Init()
        {
            InitRasterizerState();
            s_glServerSideFlags[0] = true;
            s_glClientStateFlags[0] = true;
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

        private static void DrawTriangleStrip(int first, int count)
        {
            _ = s_glServerSideFlags.TryGetValue(0, out bool value);
            if (value)
            {
                _ = s_glClientStateFlags.TryGetValue(0, out value);
            }
            if (value)
            {
                DrawTriangleStripTextured(first, count);
                return;
            }
            DrawTriangleStripColored(first, count);
        }

        public static VertexPositionColor[] GetLastVertices_PositionColor()
        {
            return s_LastVertices_PositionColor;
        }

        public static void Optimized_DrawTriangleStripColored(VertexPositionColor[] vertices)
        {
            BasicEffect effect = GetEffect(false, true);
            if (effect.Alpha == 0f)
            {
                return;
            }
            foreach (EffectPass effectPass in effect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                Global.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices, 0, vertices.Length - 2);
            }
        }

        private static void DrawTriangleStripColored(int first, int count)
        {
            BasicEffect effect = GetEffect(false, true);
            if (effect.Alpha == 0f)
            {
                s_LastVertices_PositionColor = null;
                return;
            }
            _ = s_glClientStateFlags.TryGetValue(13, out bool value);
            VertexPositionColor[] array = s_LastVertices_PositionColor = value ? ConstructColorVertices() : ConstructCurrentColorVertices();
            foreach (EffectPass effectPass in effect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                Global.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, array, 0, array.Length - 2);
            }
        }

        private static void DrawTriangleStripTextured(int first, int count)
        {
            BasicEffect effect = GetEffect(true, false);
            if (effect.Alpha == 0f)
            {
                return;
            }
            VertexPositionNormalTexture[] array = ConstructTexturedVertices();
            foreach (EffectPass effectPass in effect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                Global.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, array, 0, array.Length - 2);
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

        public static void Optimized_DrawTriangleList(VertexPositionNormalTexture[] vertices, short[] indices)
        {
            BasicEffect effect = GetEffect(true, false);
            if (effect.Alpha == 0f)
            {
                return;
            }
            foreach (EffectPass effectPass in effect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                Global.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, indices.Length / 3);
            }
        }

        private static void DrawTriangleList(int first, int count, short[] indices)
        {
            _ = s_glClientStateFlags.TryGetValue(13, out bool value);
            if (value)
            {
                DrawTriangleListColored(first, count, indices);
                return;
            }
            BasicEffect effect = GetEffect(true, false);
            if (effect.Alpha == 0f)
            {
                s_LastVertices_PositionNormalTexture = null;
                return;
            }
            VertexPositionNormalTexture[] array = s_LastVertices_PositionNormalTexture = ConstructTexturedVertices();
            foreach (EffectPass effectPass in effect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                Global.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, array, 0, array.Length, indices, 0, indices.Length / 3);
            }
        }

        private static void DrawTriangleListColored(int first, int count, short[] indices)
        {
            if (count == 0)
            {
                return;
            }
            BasicEffect effect = GetEffect(true, true);
            if (effect.Alpha == 0f)
            {
                return;
            }
            int num = count / 3 * 2;
            VertexPositionColorTexture[] vertexData = ConstructTexturedColoredVertices(num);
            foreach (EffectPass effectPass in effect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                Global.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertexData, 0, num, indices, 0, count / 3);
            }
        }

        public static void GlDrawElements(int mode, int count, short[] indices)
        {
            if (mode == 7)
            {
                DrawTriangleList(0, count, indices);
            }
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

        private static readonly Dictionary<int, bool> s_glServerSideFlags = [];

        private static readonly Dictionary<int, bool> s_glClientStateFlags = [];

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

        private static RGBAColor[] s_GLColorPointer;

        private static GLVertexPointer s_GLVertexPointer;

        private static GLTexCoordPointer s_GLTexCoordPointer;

        private static int s_GLColorPointer_additive_position;

        private static int s_GLVertexPointer_additive_position;

        private static Vector3 normal = new(0f, 0f, 1f);

        private static BasicEffect s_effectTexture;

        private static BasicEffect s_effectColor;

        private static BasicEffect s_effectTextureColor;

        private static RasterizerState s_rasterizerStateSolidColor;

        private static RasterizerState s_rasterizerStateTexture;

        private static VertexPositionColor[] s_LastVertices_PositionColor;

        private static VertexPositionNormalTexture[] s_LastVertices_PositionNormalTexture;

        private static Rectangle ScreenRect = new(0, 0, Global.GraphicsDevice.Viewport.Width, Global.GraphicsDevice.Viewport.Height);

        private static double s_LineWidth;

        private sealed class GLVertexPointer(int size, object pointer)
        {
            // (get) Token: 0x06000653 RID: 1619 RVA: 0x00033AD0 File Offset: 0x00031CD0
            public int Count => pointer_ == null || size_ == 0 ? 0 : pointer_.Length / size_;

            public int size_ = size;

            public float[] pointer_ = pointer != null ? (float[])pointer : null;
        }

        private sealed class GLTexCoordPointer(int size, object pointer)
        {
            // (get) Token: 0x06000655 RID: 1621 RVA: 0x00033B16 File Offset: 0x00031D16
            public int Count => pointer_ == null || size_ == 0 ? 0 : pointer_.Length / size_;

            public int size_ = size;

            public float[] pointer_ = pointer != null ? (float[])pointer : null;
        }
    }
}
