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
                s_Blend.enable();
            }
            s_glServerSideFlags[cap] = true;
        }

        public static void glDisable(int cap)
        {
            if (cap == 4)
            {
                glScissor(0.0, 0.0, FrameworkTypes.SCREEN_WIDTH, FrameworkTypes.SCREEN_HEIGHT);
            }
            if (cap == 1)
            {
                s_Blend.disable();
            }
            s_glServerSideFlags[cap] = false;
        }

        public static void glEnableClientState(int cap)
        {
            s_glClientStateFlags[cap] = true;
        }

        public static void glDisableClientState(int cap)
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

        public static void glViewport(double x, double y, double width, double height)
        {
            glViewport((int)x, (int)y, (int)width, (int)height);
        }

        public static void glViewport(int x, int y, int width, int height)
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

        public static void glMatrixMode(int mode)
        {
            s_glMatrixMode = mode;
        }

        public static void glLoadIdentity()
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

        public static void glOrthof(double left, double right, double bottom, double top, double near, double far)
        {
            s_matrixProjection = Matrix.CreateOrthographicOffCenter((float)left, (float)right, (float)bottom, (float)top, (float)near, (float)far);
        }

        public static void glPopMatrix()
        {
            if (s_matrixModelViewStack.Count > 0)
            {
                int index = s_matrixModelViewStack.Count - 1;
                s_matrixModelView = s_matrixModelViewStack[index];
                s_matrixModelViewStack.RemoveAt(index);
            }
        }

        public static void glPushMatrix()
        {
            s_matrixModelViewStack.Add(s_matrixModelView);
        }

        public static void glScalef(double x, double y, double z)
        {
            glScalef((float)x, (float)y, (float)z);
        }

        public static void glScalef(float x, float y, float z)
        {
            s_matrixModelView = Matrix.CreateScale(x, y, z) * s_matrixModelView;
        }

        public static void glRotatef(double angle, double x, double y, double z)
        {
            glRotatef((float)angle, (float)x, (float)y, (float)z);
        }

        public static void glRotatef(float angle, float x, float y, float z)
        {
            s_matrixModelView = Matrix.CreateRotationZ(MathHelper.ToRadians(angle)) * s_matrixModelView;
        }

        public static void glTranslatef(double x, double y, double z)
        {
            glTranslatef((float)x, (float)y, (float)z);
        }

        public static void glTranslatef(float x, float y, float z)
        {
            s_matrixModelView = Matrix.CreateTranslation(x, y, 0f) * s_matrixModelView;
        }

        public static void glBindTexture(CTRTexture2D t)
        {
            s_Texture = t;
        }

        public static void glClearColor(Color c)
        {
            s_glClearColor = c;
        }

        public static void glClearColorf(double red, double green, double blue, double alpha)
        {
            s_glClearColor = new Color((float)red, (float)green, (float)blue, (float)alpha);
        }

        public static void glClear(int mask_NotUsedParam)
        {
            BlendParams.applyDefault();
            Global.GraphicsDevice.Clear(s_glClearColor);
        }

        public static void glColor4f(Color c)
        {
            s_Color = c;
        }

        public static void glBlendFunc(BlendingFactor sfactor, BlendingFactor dfactor)
        {
            s_Blend = new BlendParams(sfactor, dfactor);
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
            s_GLColorPointer = pointer;
        }

        public static void glVertexPointer(int size, int type, int stride, object pointer)
        {
            s_GLVertexPointer = new GLVertexPointer(size, type, stride, pointer);
        }

        public static void glTexCoordPointer(int size, int type, int stride, object pointer)
        {
            s_GLTexCoordPointer = new GLTexCoordPointer(size, type, stride, pointer);
        }

        public static void glDrawArrays(int mode, int first, int count)
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

        public static void glColorPointer_setAdditive(int size)
        {
            s_GLColorPointer = new RGBAColor[size];
            s_GLColorPointer_additive_position = 0;
        }

        public static void glColorPointer_add(int size, int type, int stride, RGBAColor[] pointer)
        {
            pointer.CopyTo(s_GLColorPointer, s_GLColorPointer_additive_position);
            s_GLColorPointer_additive_position += pointer.Length;
        }

        public static void glVertexPointer_setAdditive(int size, int type, int stride, int length)
        {
            s_GLVertexPointer = new GLVertexPointer(size, type, stride, new float[length]);
            s_GLVertexPointer_additive_position = 0;
        }

        public static void glVertexPointer_add(int size, int type, int stride, float[] pointer)
        {
            pointer.CopyTo(s_GLVertexPointer.pointer_, s_GLVertexPointer_additive_position);
            s_GLVertexPointer_additive_position += pointer.Length;
        }

        private static VertexPositionColor[] ConstructColorVertices()
        {
            VertexPositionColor[] array = new VertexPositionColor[s_GLVertexPointer.Count];
            int num = 0;
            Vector3 position = default(Vector3);
            for (int i = 0; i < array.Length; i++)
            {
                position.X = s_GLVertexPointer.pointer_[num++];
                position.Y = s_GLVertexPointer.pointer_[num++];
                if (s_GLVertexPointer.size_ == 2)
                {
                    position.Z = 0f;
                }
                else
                {
                    position.Z = s_GLVertexPointer.pointer_[num++];
                }
                array[i] = new VertexPositionColor(position, s_GLColorPointer[i].toXNA());
            }
            return array;
        }

        private static VertexPositionColor[] ConstructCurrentColorVertices()
        {
            VertexPositionColor[] array = new VertexPositionColor[s_GLVertexPointer.Count];
            int num = 0;
            for (int i = 0; i < array.Length; i++)
            {
                Vector3 position = default(Vector3);
                position.X = s_GLVertexPointer.pointer_[num++];
                position.Y = s_GLVertexPointer.pointer_[num++];
                if (s_GLVertexPointer.size_ == 2)
                {
                    position.Z = 0f;
                }
                else
                {
                    position.Z = s_GLVertexPointer.pointer_[num++];
                }
                array[i] = new VertexPositionColor(position, s_Color);
            }
            s_GLVertexPointer = null;
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
            VertexPositionNormalTexture[] array = new VertexPositionNormalTexture[s_GLVertexPointer.Count];
            int num = 0;
            int num2 = 0;
            for (int i = 0; i < array.Length; i++)
            {
                Vector3 position = default(Vector3);
                position.X = s_GLVertexPointer.pointer_[num++];
                position.Y = s_GLVertexPointer.pointer_[num++];
                if (s_GLVertexPointer.size_ == 2)
                {
                    position.Z = 0f;
                }
                else
                {
                    position.Z = s_GLVertexPointer.pointer_[num++];
                }
                Vector2 textureCoordinate = default(Vector2);
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
                Vector3 position = default(Vector3);
                position.X = s_GLVertexPointer.pointer_[num++];
                position.Y = s_GLVertexPointer.pointer_[num++];
                if (s_GLVertexPointer.size_ == 2)
                {
                    position.Z = 0f;
                }
                else
                {
                    position.Z = s_GLVertexPointer.pointer_[num++];
                }
                Vector2 textureCoordinate = default(Vector2);
                textureCoordinate.X = s_GLTexCoordPointer.pointer_[num2++];
                textureCoordinate.Y = s_GLTexCoordPointer.pointer_[num2++];
                int num3 = 2;
                while (num3 < s_GLTexCoordPointer.size_)
                {
                    num3++;
                    num2++;
                }
                Color color = s_GLColorPointer[i].toXNA();
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
            s_effectTexture = new BasicEffect(Global.GraphicsDevice);
            s_effectTexture.VertexColorEnabled = false;
            s_effectTexture.TextureEnabled = true;
            s_effectTexture.View = Matrix.Identity;
            s_effectTextureColor = new BasicEffect(Global.GraphicsDevice);
            s_effectTextureColor.VertexColorEnabled = true;
            s_effectTextureColor.TextureEnabled = true;
            s_effectTextureColor.View = Matrix.Identity;
            s_effectColor = new BasicEffect(Global.GraphicsDevice);
            s_effectColor.VertexColorEnabled = true;
            s_effectColor.TextureEnabled = false;
            s_effectColor.Alpha = 1f;
            s_effectColor.Texture = null;
            s_effectColor.View = Matrix.Identity;
        }

        private static BasicEffect getEffect(bool useTexture, bool useColor)
        {
            BasicEffect basicEffect = (!useTexture) ? s_effectColor : (useColor ? s_effectTextureColor : s_effectTexture);
            if (useTexture)
            {
                basicEffect.Alpha = s_Color.A / 255f;
                if (basicEffect.Alpha == 0f)
                {
                    return basicEffect;
                }
                if (s_Texture_OptimizeLastUsed != s_Texture)
                {
                    basicEffect.Texture = s_Texture.xnaTexture_;
                    s_Texture_OptimizeLastUsed = s_Texture;
                }
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
            s_Blend.apply();
            return basicEffect;
        }

        private static void InitRasterizerState()
        {
            s_rasterizerStateSolidColor = new RasterizerState();
            s_rasterizerStateSolidColor.FillMode = FillMode.Solid;
            s_rasterizerStateSolidColor.CullMode = CullMode.None;
            s_rasterizerStateSolidColor.ScissorTestEnable = true;
            s_rasterizerStateTexture = new RasterizerState();
            s_rasterizerStateTexture.CullMode = CullMode.None;
            s_rasterizerStateTexture.ScissorTestEnable = true;
        }

        private static void DrawTriangleStrip(int first, int count)
        {
            bool value = false;
            s_glServerSideFlags.TryGetValue(0, out value);
            if (value)
            {
                s_glClientStateFlags.TryGetValue(0, out value);
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
            BasicEffect effect = getEffect(false, true);
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
            BasicEffect effect = getEffect(false, true);
            if (effect.Alpha == 0f)
            {
                s_LastVertices_PositionColor = null;
                return;
            }
            bool value = false;
            s_glClientStateFlags.TryGetValue(13, out value);
            VertexPositionColor[] array = s_LastVertices_PositionColor = value ? ConstructColorVertices() : ConstructCurrentColorVertices();
            foreach (EffectPass effectPass in effect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                Global.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, array, 0, array.Length - 2);
            }
        }

        private static void DrawTriangleStripTextured(int first, int count)
        {
            BasicEffect effect = getEffect(true, false);
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

        public static void Optimized_DrawTriangleList(VertexPositionNormalTexture[] vertices, short[] indices)
        {
            BasicEffect effect = getEffect(true, false);
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
            bool value = false;
            s_glClientStateFlags.TryGetValue(13, out value);
            if (value)
            {
                DrawTriangleListColored(first, count, indices);
                return;
            }
            BasicEffect effect = getEffect(true, false);
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
            BasicEffect effect = getEffect(true, true);
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

        public static void glDrawElements(int mode, int count, short[] indices)
        {
            if (mode == 7)
            {
                DrawTriangleList(0, count, indices);
            }
        }

        public static void glScissor(double x, double y, double width, double height)
        {
            glScissor((int)x, (int)y, (int)width, (int)height);
        }

        public static void glScissor(int x, int y, int width, int height)
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

        public static void glLineWidth(double width)
        {
            s_LineWidth = width;
        }

        public static void setScissorRectangle(double x, double y, double w, double h)
        {
            setScissorRectangle((float)x, (float)y, (float)w, (float)h);
        }

        public static void setScissorRectangle(float x, float y, float w, float h)
        {
            glScissor((double)x, (double)y, (double)w, (double)h);
        }

        private static Dictionary<int, bool> s_glServerSideFlags = new();

        private static Dictionary<int, bool> s_glClientStateFlags = new();

        private static RenderTarget2D s_RenderTarget;

        private static Viewport s_Viewport = default(Viewport);

        private static int s_glMatrixMode;

        private static List<Matrix> s_matrixModelViewStack = new();

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

        private static VertexPositionColor[] s_LastVertices_PositionColor = null;

        private static VertexPositionNormalTexture[] s_LastVertices_PositionNormalTexture = null;

        private static Rectangle ScreenRect = new(0, 0, Global.GraphicsDevice.Viewport.Width, Global.GraphicsDevice.Viewport.Height);

        private static double s_LineWidth;

        private class GLVertexPointer
        {
            // (get) Token: 0x06000653 RID: 1619 RVA: 0x00033AD0 File Offset: 0x00031CD0
            public int Count
            {
                get
                {
                    if (pointer_ == null || size_ == 0)
                    {
                        return 0;
                    }
                    return pointer_.Length / size_;
                }
            }

            public GLVertexPointer(int size, int type, int stride, object pointer)
            {
                pointer_ = (pointer != null) ? ((float[])pointer) : null;
                size_ = size;
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
                    if (pointer_ == null || size_ == 0)
                    {
                        return 0;
                    }
                    return pointer_.Length / size_;
                }
            }

            public GLTexCoordPointer(int size, int type, int stride, object pointer)
            {
                pointer_ = (pointer != null) ? ((float[])pointer) : null;
                size_ = size;
            }

            public int size_;

            public float[] pointer_;
        }
    }
}
