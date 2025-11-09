using CutTheRope.iframework;
using CutTheRope.iframework.visual;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace CutTheRope.windows
{
    internal class OpenGL
    {
        public static void glGenTextures(int n, object textures)
        {
        }

        public static void glBindTexture(int target, uint texture)
        {
        }

        public static void glEnable(int cap)
        {
            if (cap == 1)
            {
                OpenGL.s_Blend.enable();
            }
            OpenGL.s_glServerSideFlags[cap] = true;
        }

        public static void glDisable(int cap)
        {
            if (cap == 4)
            {
                OpenGL.glScissor(0.0, 0.0, (double)FrameworkTypes.SCREEN_WIDTH, (double)FrameworkTypes.SCREEN_HEIGHT);
            }
            if (cap == 1)
            {
                OpenGL.s_Blend.disable();
            }
            OpenGL.s_glServerSideFlags[cap] = false;
        }

        public static void glEnableClientState(int cap)
        {
            OpenGL.s_glClientStateFlags[cap] = true;
        }

        public static void glDisableClientState(int cap)
        {
            OpenGL.s_glClientStateFlags[cap] = false;
        }

        public static RenderTarget2D DetachRenderTarget()
        {
            RenderTarget2D renderTarget2D = OpenGL.s_RenderTarget;
            OpenGL.s_RenderTarget = null;
            return renderTarget2D;
        }

        public static void CopyFromRenderTargetToScreen()
        {
            if (Global.ScreenSizeManager.IsFullScreen && OpenGL.s_RenderTarget != null)
            {
                Global.GraphicsDevice.Clear(Color.Black);
                Global.SpriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, null);
                Global.SpriteBatch.Draw(OpenGL.s_RenderTarget, Global.ScreenSizeManager.ScaledViewRect, Color.White);
                Global.SpriteBatch.End();
            }
        }

        public static void glViewport(double x, double y, double width, double height)
        {
            OpenGL.glViewport((int)x, (int)y, (int)width, (int)height);
        }

        public static void glViewport(int x, int y, int width, int height)
        {
            OpenGL.s_Viewport.X = x;
            OpenGL.s_Viewport.Y = y;
            OpenGL.s_Viewport.Width = width;
            OpenGL.s_Viewport.Height = height;
            if (Global.ScreenSizeManager.IsFullScreen)
            {
                if (OpenGL.s_RenderTarget == null || OpenGL.s_RenderTarget.Bounds.Width != OpenGL.s_Viewport.Bounds.Width || OpenGL.s_RenderTarget.Bounds.Height != OpenGL.s_Viewport.Bounds.Height)
                {
                    OpenGL.s_RenderTarget = new RenderTarget2D(Global.GraphicsDevice, OpenGL.s_Viewport.Width, OpenGL.s_Viewport.Height, false, SurfaceFormat.Color, DepthFormat.None);
                }
                Global.GraphicsDevice.SetRenderTarget(OpenGL.s_RenderTarget);
                Global.GraphicsDevice.Clear(Color.Black);
                return;
            }
            OpenGL.s_RenderTarget = null;
        }

        public static void glMatrixMode(int mode)
        {
            OpenGL.s_glMatrixMode = mode;
        }

        public static void glLoadIdentity()
        {
            if (OpenGL.s_glMatrixMode == 14)
            {
                OpenGL.s_matrixModelView = Matrix.Identity;
                return;
            }
            if (OpenGL.s_glMatrixMode == 15)
            {
                OpenGL.s_matrixProjection = Matrix.Identity;
                return;
            }
            if (OpenGL.s_glMatrixMode == 16)
            {
                throw new NotImplementedException();
            }
            if (OpenGL.s_glMatrixMode != 17)
            {
                return;
            }
            throw new NotImplementedException();
        }

        public static void glOrthof(double left, double right, double bottom, double top, double near, double far)
        {
            OpenGL.s_matrixProjection = Matrix.CreateOrthographicOffCenter((float)left, (float)right, (float)bottom, (float)top, (float)near, (float)far);
        }

        public static void glPopMatrix()
        {
            if (OpenGL.s_matrixModelViewStack.Count > 0)
            {
                int index = OpenGL.s_matrixModelViewStack.Count - 1;
                OpenGL.s_matrixModelView = OpenGL.s_matrixModelViewStack[index];
                OpenGL.s_matrixModelViewStack.RemoveAt(index);
            }
        }

        public static void glPushMatrix()
        {
            OpenGL.s_matrixModelViewStack.Add(OpenGL.s_matrixModelView);
        }

        public static void glScalef(double x, double y, double z)
        {
            OpenGL.glScalef((float)x, (float)y, (float)z);
        }

        public static void glScalef(float x, float y, float z)
        {
            OpenGL.s_matrixModelView = Matrix.CreateScale(x, y, z) * OpenGL.s_matrixModelView;
        }

        public static void glRotatef(double angle, double x, double y, double z)
        {
            OpenGL.glRotatef((float)angle, (float)x, (float)y, (float)z);
        }

        public static void glRotatef(float angle, float x, float y, float z)
        {
            OpenGL.s_matrixModelView = Matrix.CreateRotationZ(MathHelper.ToRadians(angle)) * OpenGL.s_matrixModelView;
        }

        public static void glTranslatef(double x, double y, double z)
        {
            OpenGL.glTranslatef((float)x, (float)y, (float)z);
        }

        public static void glTranslatef(float x, float y, float z)
        {
            OpenGL.s_matrixModelView = Matrix.CreateTranslation(x, y, 0f) * OpenGL.s_matrixModelView;
        }

        public static void glBindTexture(CutTheRope.iframework.visual.CTRTexture2D t)
        {
            OpenGL.s_Texture = t;
        }

        public static void glClearColor(Color c)
        {
            OpenGL.s_glClearColor = c;
        }

        public static void glClearColorf(double red, double green, double blue, double alpha)
        {
            OpenGL.s_glClearColor = new Color((float)red, (float)green, (float)blue, (float)alpha);
        }

        public static void glClear(int mask_NotUsedParam)
        {
            BlendParams.applyDefault();
            Global.GraphicsDevice.Clear(OpenGL.s_glClearColor);
        }

        public static void glColor4f(Color c)
        {
            OpenGL.s_Color = c;
        }

        public static void glBlendFunc(BlendingFactor sfactor, BlendingFactor dfactor)
        {
            OpenGL.s_Blend = new BlendParams(sfactor, dfactor);
        }

        public static void drawSegment(float x1, float y1, float x2, float y2, RGBAColor color)
        {
        }

        public static void glGenBuffers(int n, ref uint buffer)
        {
        }

        public static void glGenBuffers(int n, ref uint[] buffers)
        {
        }

        public static void glDeleteBuffers(int n, ref uint[] buffers)
        {
        }

        public static void glDeleteBuffers(int n, ref uint buffers)
        {
        }

        public static void glBindBuffer(int target, uint buffer)
        {
        }

        public static void glBufferData(int target, RGBAColor[] data, int usage)
        {
        }

        public static void glBufferData(int target, PointSprite[] data, int usage)
        {
        }

        public static void glColorPointer(int size, int type, int stride, RGBAColor[] pointer)
        {
            OpenGL.s_GLColorPointer = pointer;
        }

        public static void glVertexPointer(int size, int type, int stride, object pointer)
        {
            OpenGL.s_GLVertexPointer = new OpenGL.GLVertexPointer(size, type, stride, pointer);
        }

        public static void glTexCoordPointer(int size, int type, int stride, object pointer)
        {
            OpenGL.s_GLTexCoordPointer = new OpenGL.GLTexCoordPointer(size, type, stride, pointer);
        }

        public static void glDrawArrays(int mode, int first, int count)
        {
            if (mode == 8)
            {
                OpenGL.DrawTriangleStrip(first, count);
                return;
            }
            if (mode - 9 > 1)
            {
                throw new NotImplementedException();
            }
        }

        public static void glColorPointer_setAdditive(int size)
        {
            OpenGL.s_GLColorPointer = new RGBAColor[size];
            OpenGL.s_GLColorPointer_additive_position = 0;
        }

        public static void glColorPointer_add(int size, int type, int stride, RGBAColor[] pointer)
        {
            pointer.CopyTo(OpenGL.s_GLColorPointer, OpenGL.s_GLColorPointer_additive_position);
            OpenGL.s_GLColorPointer_additive_position += pointer.Length;
        }

        public static void glVertexPointer_setAdditive(int size, int type, int stride, int length)
        {
            OpenGL.s_GLVertexPointer = new OpenGL.GLVertexPointer(size, type, stride, new float[length]);
            OpenGL.s_GLVertexPointer_additive_position = 0;
        }

        public static void glVertexPointer_add(int size, int type, int stride, float[] pointer)
        {
            pointer.CopyTo(OpenGL.s_GLVertexPointer.pointer_, OpenGL.s_GLVertexPointer_additive_position);
            OpenGL.s_GLVertexPointer_additive_position += pointer.Length;
        }

        private static VertexPositionColor[] ConstructColorVertices()
        {
            VertexPositionColor[] array = new VertexPositionColor[OpenGL.s_GLVertexPointer.Count];
            int num = 0;
            Vector3 position = default(Vector3);
            for (int i = 0; i < array.Length; i++)
            {
                position.X = OpenGL.s_GLVertexPointer.pointer_[num++];
                position.Y = OpenGL.s_GLVertexPointer.pointer_[num++];
                if (OpenGL.s_GLVertexPointer.size_ == 2)
                {
                    position.Z = 0f;
                }
                else
                {
                    position.Z = OpenGL.s_GLVertexPointer.pointer_[num++];
                }
                array[i] = new VertexPositionColor(position, OpenGL.s_GLColorPointer[i].toXNA());
            }
            return array;
        }

        private static VertexPositionColor[] ConstructCurrentColorVertices()
        {
            VertexPositionColor[] array = new VertexPositionColor[OpenGL.s_GLVertexPointer.Count];
            int num = 0;
            for (int i = 0; i < array.Length; i++)
            {
                Vector3 position = default(Vector3);
                position.X = OpenGL.s_GLVertexPointer.pointer_[num++];
                position.Y = OpenGL.s_GLVertexPointer.pointer_[num++];
                if (OpenGL.s_GLVertexPointer.size_ == 2)
                {
                    position.Z = 0f;
                }
                else
                {
                    position.Z = OpenGL.s_GLVertexPointer.pointer_[num++];
                }
                array[i] = new VertexPositionColor(position, OpenGL.s_Color);
            }
            OpenGL.s_GLVertexPointer = null;
            return array;
        }

        private static short[] InitializeTriangleStripIndices(int count)
        {
            short[] array = new short[count];
            for (int i = 0; i < count; i++)
            {
                array[i] = (short)i;
            }
            return array;
        }

        private static VertexPositionNormalTexture[] ConstructTexturedVertices()
        {
            VertexPositionNormalTexture[] array = new VertexPositionNormalTexture[OpenGL.s_GLVertexPointer.Count];
            int num = 0;
            int num2 = 0;
            for (int i = 0; i < array.Length; i++)
            {
                Vector3 position = default(Vector3);
                position.X = OpenGL.s_GLVertexPointer.pointer_[num++];
                position.Y = OpenGL.s_GLVertexPointer.pointer_[num++];
                if (OpenGL.s_GLVertexPointer.size_ == 2)
                {
                    position.Z = 0f;
                }
                else
                {
                    position.Z = OpenGL.s_GLVertexPointer.pointer_[num++];
                }
                Vector2 textureCoordinate = default(Vector2);
                textureCoordinate.X = OpenGL.s_GLTexCoordPointer.pointer_[num2++];
                textureCoordinate.Y = OpenGL.s_GLTexCoordPointer.pointer_[num2++];
                int num3 = 2;
                while (num3 < OpenGL.s_GLTexCoordPointer.size_)
                {
                    num3++;
                    num2++;
                }
                array[i] = new VertexPositionNormalTexture(position, OpenGL.normal, textureCoordinate);
            }
            OpenGL.s_GLTexCoordPointer = null;
            OpenGL.s_GLVertexPointer = null;
            return array;
        }

        private static VertexPositionColorTexture[] ConstructTexturedColoredVertices(int vertexCount)
        {
            VertexPositionColorTexture[] array = new VertexPositionColorTexture[vertexCount];
            int num = 0;
            int num2 = 0;
            for (int i = 0; i < array.Length; i++)
            {
                Vector3 position = default(Vector3);
                position.X = OpenGL.s_GLVertexPointer.pointer_[num++];
                position.Y = OpenGL.s_GLVertexPointer.pointer_[num++];
                if (OpenGL.s_GLVertexPointer.size_ == 2)
                {
                    position.Z = 0f;
                }
                else
                {
                    position.Z = OpenGL.s_GLVertexPointer.pointer_[num++];
                }
                Vector2 textureCoordinate = default(Vector2);
                textureCoordinate.X = OpenGL.s_GLTexCoordPointer.pointer_[num2++];
                textureCoordinate.Y = OpenGL.s_GLTexCoordPointer.pointer_[num2++];
                int num3 = 2;
                while (num3 < OpenGL.s_GLTexCoordPointer.size_)
                {
                    num3++;
                    num2++;
                }
                Color color = OpenGL.s_GLColorPointer[i].toXNA();
                array[i] = new VertexPositionColorTexture(position, color, textureCoordinate);
            }
            OpenGL.s_GLTexCoordPointer = null;
            OpenGL.s_GLVertexPointer = null;
            return array;
        }

        public static void Init()
        {
            OpenGL.InitRasterizerState();
            OpenGL.s_glServerSideFlags[0] = true;
            OpenGL.s_glClientStateFlags[0] = true;
            OpenGL.s_effectTexture = new BasicEffect(Global.GraphicsDevice);
            OpenGL.s_effectTexture.VertexColorEnabled = false;
            OpenGL.s_effectTexture.TextureEnabled = true;
            OpenGL.s_effectTexture.View = Matrix.Identity;
            OpenGL.s_effectTextureColor = new BasicEffect(Global.GraphicsDevice);
            OpenGL.s_effectTextureColor.VertexColorEnabled = true;
            OpenGL.s_effectTextureColor.TextureEnabled = true;
            OpenGL.s_effectTextureColor.View = Matrix.Identity;
            OpenGL.s_effectColor = new BasicEffect(Global.GraphicsDevice);
            OpenGL.s_effectColor.VertexColorEnabled = true;
            OpenGL.s_effectColor.TextureEnabled = false;
            OpenGL.s_effectColor.Alpha = 1f;
            OpenGL.s_effectColor.Texture = null;
            OpenGL.s_effectColor.View = Matrix.Identity;
        }

        private static BasicEffect getEffect(bool useTexture, bool useColor)
        {
            BasicEffect basicEffect = (!useTexture) ? OpenGL.s_effectColor : (useColor ? OpenGL.s_effectTextureColor : OpenGL.s_effectTexture);
            if (useTexture)
            {
                basicEffect.Alpha = (float)OpenGL.s_Color.A / 255f;
                if (basicEffect.Alpha == 0f)
                {
                    return basicEffect;
                }
                if (OpenGL.s_Texture_OptimizeLastUsed != OpenGL.s_Texture)
                {
                    basicEffect.Texture = OpenGL.s_Texture.xnaTexture_;
                    OpenGL.s_Texture_OptimizeLastUsed = OpenGL.s_Texture;
                }
                basicEffect.DiffuseColor = OpenGL.s_Color.ToVector3();
                Global.GraphicsDevice.RasterizerState = OpenGL.s_rasterizerStateTexture;
                Global.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
            }
            else
            {
                Global.GraphicsDevice.RasterizerState = OpenGL.s_rasterizerStateSolidColor;
            }
            basicEffect.World = OpenGL.s_matrixModelView;
            basicEffect.Projection = OpenGL.s_matrixProjection;
            OpenGL.s_Blend.apply();
            return basicEffect;
        }

        private static void InitRasterizerState()
        {
            OpenGL.s_rasterizerStateSolidColor = new RasterizerState();
            OpenGL.s_rasterizerStateSolidColor.FillMode = FillMode.Solid;
            OpenGL.s_rasterizerStateSolidColor.CullMode = CullMode.None;
            OpenGL.s_rasterizerStateSolidColor.ScissorTestEnable = true;
            OpenGL.s_rasterizerStateTexture = new RasterizerState();
            OpenGL.s_rasterizerStateTexture.CullMode = CullMode.None;
            OpenGL.s_rasterizerStateTexture.ScissorTestEnable = true;
        }

        private static void DrawTriangleStrip(int first, int count)
        {
            bool value = false;
            OpenGL.s_glServerSideFlags.TryGetValue(0, out value);
            if (value)
            {
                OpenGL.s_glClientStateFlags.TryGetValue(0, out value);
            }
            if (value)
            {
                OpenGL.DrawTriangleStripTextured(first, count);
                return;
            }
            OpenGL.DrawTriangleStripColored(first, count);
        }

        public static VertexPositionColor[] GetLastVertices_PositionColor()
        {
            return OpenGL.s_LastVertices_PositionColor;
        }

        public static void Optimized_DrawTriangleStripColored(VertexPositionColor[] vertices)
        {
            BasicEffect effect = OpenGL.getEffect(false, true);
            if (effect.Alpha == 0f)
            {
                return;
            }
            foreach (EffectPass effectPass in effect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                Global.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleStrip, vertices, 0, vertices.Length - 2);
            }
        }

        private static void DrawTriangleStripColored(int first, int count)
        {
            BasicEffect effect = OpenGL.getEffect(false, true);
            if (effect.Alpha == 0f)
            {
                OpenGL.s_LastVertices_PositionColor = null;
                return;
            }
            bool value = false;
            OpenGL.s_glClientStateFlags.TryGetValue(13, out value);
            VertexPositionColor[] array = OpenGL.s_LastVertices_PositionColor = value ? OpenGL.ConstructColorVertices() : OpenGL.ConstructCurrentColorVertices();
            foreach (EffectPass effectPass in effect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                Global.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleStrip, array, 0, array.Length - 2);
            }
        }

        private static void DrawTriangleStripTextured(int first, int count)
        {
            BasicEffect effect = OpenGL.getEffect(true, false);
            if (effect.Alpha == 0f)
            {
                return;
            }
            VertexPositionNormalTexture[] array = OpenGL.ConstructTexturedVertices();
            foreach (EffectPass effectPass in effect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                Global.GraphicsDevice.DrawUserPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleStrip, array, 0, array.Length - 2);
            }
        }

        public static VertexPositionNormalTexture[] GetLastVertices_PositionNormalTexture()
        {
            return OpenGL.s_LastVertices_PositionNormalTexture;
        }

        public static void Optimized_DrawTriangleList(VertexPositionNormalTexture[] vertices, short[] indices)
        {
            BasicEffect effect = OpenGL.getEffect(true, false);
            if (effect.Alpha == 0f)
            {
                return;
            }
            foreach (EffectPass effectPass in effect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                Global.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, indices.Length / 3);
            }
        }

        private static void DrawTriangleList(int first, int count, short[] indices)
        {
            bool value = false;
            OpenGL.s_glClientStateFlags.TryGetValue(13, out value);
            if (value)
            {
                OpenGL.DrawTriangleListColored(first, count, indices);
                return;
            }
            BasicEffect effect = OpenGL.getEffect(true, false);
            if (effect.Alpha == 0f)
            {
                OpenGL.s_LastVertices_PositionNormalTexture = null;
                return;
            }
            VertexPositionNormalTexture[] array = OpenGL.s_LastVertices_PositionNormalTexture = OpenGL.ConstructTexturedVertices();
            foreach (EffectPass effectPass in effect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                Global.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList, array, 0, array.Length, indices, 0, indices.Length / 3);
            }
        }

        private static void DrawTriangleListColored(int first, int count, short[] indices)
        {
            if (count == 0)
            {
                return;
            }
            BasicEffect effect = OpenGL.getEffect(true, true);
            if (effect.Alpha == 0f)
            {
                return;
            }
            int num = count / 3 * 2;
            VertexPositionColorTexture[] vertexData = OpenGL.ConstructTexturedColoredVertices(num);
            foreach (EffectPass effectPass in effect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                Global.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColorTexture>(PrimitiveType.TriangleList, vertexData, 0, num, indices, 0, count / 3);
            }
        }

        public static void glDrawElements(int mode, int count, short[] indices)
        {
            if (mode == 7)
            {
                OpenGL.DrawTriangleList(0, count, indices);
            }
        }

        public static void glScissor(double x, double y, double width, double height)
        {
            OpenGL.glScissor((int)x, (int)y, (int)width, (int)height);
        }

        public static void glScissor(int x, int y, int width, int height)
        {
            try
            {
                Microsoft.Xna.Framework.Rectangle bounds = Global.XnaGame.GraphicsDevice.Viewport.Bounds;
                float num = FrameworkTypes.SCREEN_WIDTH / (float)bounds.Width;
                float num2 = FrameworkTypes.SCREEN_HEIGHT / (float)bounds.Height;
                Microsoft.Xna.Framework.Rectangle value = new((int)((float)x / num), (int)((float)y / num2), (int)((float)width / num), (int)((float)height / num2));
                Global.GraphicsDevice.ScissorRectangle = Microsoft.Xna.Framework.Rectangle.Intersect(value, bounds);
            }
            catch (Exception)
            {
            }
        }

        public static void glLineWidth(double width)
        {
            OpenGL.s_LineWidth = width;
        }

        public static void setScissorRectangle(double x, double y, double w, double h)
        {
            OpenGL.setScissorRectangle((float)x, (float)y, (float)w, (float)h);
        }

        public static void setScissorRectangle(float x, float y, float w, float h)
        {
            OpenGL.glScissor((double)x, (double)y, (double)w, (double)h);
        }

        private static Dictionary<int, bool> s_glServerSideFlags = new();

        private static Dictionary<int, bool> s_glClientStateFlags = new();

        private static RenderTarget2D s_RenderTarget;

        private static Viewport s_Viewport = default(Viewport);

        private static int s_glMatrixMode;

        private static List<Matrix> s_matrixModelViewStack = new();

        private static Matrix s_matrixModelView = Matrix.Identity;

        private static Matrix s_matrixProjection = Matrix.Identity;

        private static CutTheRope.iframework.visual.CTRTexture2D s_Texture;

        private static CutTheRope.iframework.visual.CTRTexture2D s_Texture_OptimizeLastUsed;

        private static Color s_glClearColor = Color.White;

        private static Color s_Color = Color.White;

        private static BlendParams s_Blend = new();

        private static RGBAColor[] s_GLColorPointer;

        private static OpenGL.GLVertexPointer s_GLVertexPointer;

        private static OpenGL.GLTexCoordPointer s_GLTexCoordPointer;

        private static int s_GLColorPointer_additive_position;

        private static int s_GLVertexPointer_additive_position;

        private static Vector3 normal = new(0f, 0f, 1f);

        private static BasicEffect s_effectTexture;

        private static BasicEffect s_effectColor;

        private static BasicEffect s_effectTextureColor;

        private static RasterizerState s_rasterizerStateSolidColor;

        private static RasterizerState s_rasterizerStateTexture;

        private static VertexPositionColor[] s_LastVertices_PositionColor = null;

        private static VertexPositionNormalTexture[] s_LastVertices_PositionNormalTexture = null;

        private static Microsoft.Xna.Framework.Rectangle ScreenRect = new(0, 0, Global.GraphicsDevice.Viewport.Width, Global.GraphicsDevice.Viewport.Height);

        private static double s_LineWidth;

        private class GLVertexPointer
        {
            // (get) Token: 0x06000653 RID: 1619 RVA: 0x00033AD0 File Offset: 0x00031CD0
            public int Count
            {
                get
                {
                    if (this.pointer_ == null || this.size_ == 0)
                    {
                        return 0;
                    }
                    return this.pointer_.Length / this.size_;
                }
            }

            public GLVertexPointer(int size, int type, int stride, object pointer)
            {
                this.pointer_ = (pointer != null) ? ((float[])pointer) : null;
                this.size_ = size;
            }

            public int size_;

            public float[] pointer_;
        }

        private class GLTexCoordPointer
        {
            // (get) Token: 0x06000655 RID: 1621 RVA: 0x00033B16 File Offset: 0x00031D16
            public int Count
            {
                get
                {
                    if (this.pointer_ == null || this.size_ == 0)
                    {
                        return 0;
                    }
                    return this.pointer_.Length / this.size_;
                }
            }

            public GLTexCoordPointer(int size, int type, int stride, object pointer)
            {
                this.pointer_ = (pointer != null) ? ((float[])pointer) : null;
                this.size_ = size;
            }

            public int size_;

            public float[] pointer_;
        }
    }
}
