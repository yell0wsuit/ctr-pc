using System;
using System.Collections.Generic;
using CutTheRope.iframework;
using CutTheRope.iframework.visual;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.windows
{
	// Token: 0x02000010 RID: 16
	internal class OpenGL
	{
		// Token: 0x06000067 RID: 103 RVA: 0x00003B65 File Offset: 0x00001D65
		public static void glGenTextures(int n, object textures)
		{
		}

		// Token: 0x06000068 RID: 104 RVA: 0x00003B67 File Offset: 0x00001D67
		public static void glBindTexture(int target, uint texture)
		{
		}

		// Token: 0x06000069 RID: 105 RVA: 0x00003B69 File Offset: 0x00001D69
		public static void glEnable(int cap)
		{
			if (cap == 1)
			{
				OpenGL.s_Blend.enable();
			}
			OpenGL.s_glServerSideFlags[cap] = true;
		}

		// Token: 0x0600006A RID: 106 RVA: 0x00003B88 File Offset: 0x00001D88
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

		// Token: 0x0600006B RID: 107 RVA: 0x00003BD6 File Offset: 0x00001DD6
		public static void glEnableClientState(int cap)
		{
			OpenGL.s_glClientStateFlags[cap] = true;
		}

		// Token: 0x0600006C RID: 108 RVA: 0x00003BE4 File Offset: 0x00001DE4
		public static void glDisableClientState(int cap)
		{
			OpenGL.s_glClientStateFlags[cap] = false;
		}

		// Token: 0x0600006D RID: 109 RVA: 0x00003BF2 File Offset: 0x00001DF2
		public static RenderTarget2D DetachRenderTarget()
		{
			RenderTarget2D renderTarget2D = OpenGL.s_RenderTarget;
			OpenGL.s_RenderTarget = null;
			return renderTarget2D;
		}

		// Token: 0x0600006E RID: 110 RVA: 0x00003C00 File Offset: 0x00001E00
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

		// Token: 0x0600006F RID: 111 RVA: 0x00003C70 File Offset: 0x00001E70
		public static void glViewport(double x, double y, double width, double height)
		{
			OpenGL.glViewport((int)x, (int)y, (int)width, (int)height);
		}

		// Token: 0x06000070 RID: 112 RVA: 0x00003C80 File Offset: 0x00001E80
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

		// Token: 0x06000071 RID: 113 RVA: 0x00003D5A File Offset: 0x00001F5A
		public static void glMatrixMode(int mode)
		{
			OpenGL.s_glMatrixMode = mode;
		}

		// Token: 0x06000072 RID: 114 RVA: 0x00003D64 File Offset: 0x00001F64
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

		// Token: 0x06000073 RID: 115 RVA: 0x00003DB7 File Offset: 0x00001FB7
		public static void glOrthof(double left, double right, double bottom, double top, double near, double far)
		{
			OpenGL.s_matrixProjection = Matrix.CreateOrthographicOffCenter((float)left, (float)right, (float)bottom, (float)top, (float)near, (float)far);
		}

		// Token: 0x06000074 RID: 116 RVA: 0x00003DD4 File Offset: 0x00001FD4
		public static void glPopMatrix()
		{
			if (OpenGL.s_matrixModelViewStack.Count > 0)
			{
				int index = OpenGL.s_matrixModelViewStack.Count - 1;
				OpenGL.s_matrixModelView = OpenGL.s_matrixModelViewStack[index];
				OpenGL.s_matrixModelViewStack.RemoveAt(index);
			}
		}

		// Token: 0x06000075 RID: 117 RVA: 0x00003E16 File Offset: 0x00002016
		public static void glPushMatrix()
		{
			OpenGL.s_matrixModelViewStack.Add(OpenGL.s_matrixModelView);
		}

		// Token: 0x06000076 RID: 118 RVA: 0x00003E27 File Offset: 0x00002027
		public static void glScalef(double x, double y, double z)
		{
			OpenGL.glScalef((float)x, (float)y, (float)z);
		}

		// Token: 0x06000077 RID: 119 RVA: 0x00003E34 File Offset: 0x00002034
		public static void glScalef(float x, float y, float z)
		{
			OpenGL.s_matrixModelView = Matrix.CreateScale(x, y, z) * OpenGL.s_matrixModelView;
		}

		// Token: 0x06000078 RID: 120 RVA: 0x00003E4D File Offset: 0x0000204D
		public static void glRotatef(double angle, double x, double y, double z)
		{
			OpenGL.glRotatef((float)angle, (float)x, (float)y, (float)z);
		}

		// Token: 0x06000079 RID: 121 RVA: 0x00003E5C File Offset: 0x0000205C
		public static void glRotatef(float angle, float x, float y, float z)
		{
			OpenGL.s_matrixModelView = Matrix.CreateRotationZ(MathHelper.ToRadians(angle)) * OpenGL.s_matrixModelView;
		}

		// Token: 0x0600007A RID: 122 RVA: 0x00003E78 File Offset: 0x00002078
		public static void glTranslatef(double x, double y, double z)
		{
			OpenGL.glTranslatef((float)x, (float)y, (float)z);
		}

		// Token: 0x0600007B RID: 123 RVA: 0x00003E85 File Offset: 0x00002085
		public static void glTranslatef(float x, float y, float z)
		{
			OpenGL.s_matrixModelView = Matrix.CreateTranslation(x, y, 0f) * OpenGL.s_matrixModelView;
		}

		// Token: 0x0600007C RID: 124 RVA: 0x00003EA2 File Offset: 0x000020A2
		public static void glBindTexture(CutTheRope.iframework.visual.Texture2D t)
		{
			OpenGL.s_Texture = t;
		}

		// Token: 0x0600007D RID: 125 RVA: 0x00003EAA File Offset: 0x000020AA
		public static void glClearColor(Color c)
		{
			OpenGL.s_glClearColor = c;
		}

		// Token: 0x0600007E RID: 126 RVA: 0x00003EB2 File Offset: 0x000020B2
		public static void glClearColorf(double red, double green, double blue, double alpha)
		{
			OpenGL.s_glClearColor = new Color((float)red, (float)green, (float)blue, (float)alpha);
		}

		// Token: 0x0600007F RID: 127 RVA: 0x00003EC6 File Offset: 0x000020C6
		public static void glClear(int mask_NotUsedParam)
		{
			BlendParams.applyDefault();
			Global.GraphicsDevice.Clear(OpenGL.s_glClearColor);
		}

		// Token: 0x06000080 RID: 128 RVA: 0x00003EDC File Offset: 0x000020DC
		public static void glColor4f(Color c)
		{
			OpenGL.s_Color = c;
		}

		// Token: 0x06000081 RID: 129 RVA: 0x00003EE4 File Offset: 0x000020E4
		public static void glBlendFunc(BlendingFactor sfactor, BlendingFactor dfactor)
		{
			OpenGL.s_Blend = new BlendParams(sfactor, dfactor);
		}

		// Token: 0x06000082 RID: 130 RVA: 0x00003EF2 File Offset: 0x000020F2
		public static void drawSegment(float x1, float y1, float x2, float y2, RGBAColor color)
		{
		}

		// Token: 0x06000083 RID: 131 RVA: 0x00003EF4 File Offset: 0x000020F4
		public static void glGenBuffers(int n, ref uint buffer)
		{
		}

		// Token: 0x06000084 RID: 132 RVA: 0x00003EF6 File Offset: 0x000020F6
		public static void glGenBuffers(int n, ref uint[] buffers)
		{
		}

		// Token: 0x06000085 RID: 133 RVA: 0x00003EF8 File Offset: 0x000020F8
		public static void glDeleteBuffers(int n, ref uint[] buffers)
		{
		}

		// Token: 0x06000086 RID: 134 RVA: 0x00003EFA File Offset: 0x000020FA
		public static void glDeleteBuffers(int n, ref uint buffers)
		{
		}

		// Token: 0x06000087 RID: 135 RVA: 0x00003EFC File Offset: 0x000020FC
		public static void glBindBuffer(int target, uint buffer)
		{
		}

		// Token: 0x06000088 RID: 136 RVA: 0x00003EFE File Offset: 0x000020FE
		public static void glBufferData(int target, RGBAColor[] data, int usage)
		{
		}

		// Token: 0x06000089 RID: 137 RVA: 0x00003F00 File Offset: 0x00002100
		public static void glBufferData(int target, PointSprite[] data, int usage)
		{
		}

		// Token: 0x0600008A RID: 138 RVA: 0x00003F02 File Offset: 0x00002102
		public static void glColorPointer(int size, int type, int stride, RGBAColor[] pointer)
		{
			OpenGL.s_GLColorPointer = pointer;
		}

		// Token: 0x0600008B RID: 139 RVA: 0x00003F0A File Offset: 0x0000210A
		public static void glVertexPointer(int size, int type, int stride, object pointer)
		{
			OpenGL.s_GLVertexPointer = new OpenGL.GLVertexPointer(size, type, stride, pointer);
		}

		// Token: 0x0600008C RID: 140 RVA: 0x00003F1A File Offset: 0x0000211A
		public static void glTexCoordPointer(int size, int type, int stride, object pointer)
		{
			OpenGL.s_GLTexCoordPointer = new OpenGL.GLTexCoordPointer(size, type, stride, pointer);
		}

		// Token: 0x0600008D RID: 141 RVA: 0x00003F2A File Offset: 0x0000212A
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

		// Token: 0x0600008E RID: 142 RVA: 0x00003F47 File Offset: 0x00002147
		public static void glColorPointer_setAdditive(int size)
		{
			OpenGL.s_GLColorPointer = new RGBAColor[size];
			OpenGL.s_GLColorPointer_additive_position = 0;
		}

		// Token: 0x0600008F RID: 143 RVA: 0x00003F5A File Offset: 0x0000215A
		public static void glColorPointer_add(int size, int type, int stride, RGBAColor[] pointer)
		{
			pointer.CopyTo(OpenGL.s_GLColorPointer, OpenGL.s_GLColorPointer_additive_position);
			OpenGL.s_GLColorPointer_additive_position += pointer.Length;
		}

		// Token: 0x06000090 RID: 144 RVA: 0x00003F7A File Offset: 0x0000217A
		public static void glVertexPointer_setAdditive(int size, int type, int stride, int length)
		{
			OpenGL.s_GLVertexPointer = new OpenGL.GLVertexPointer(size, type, stride, new float[length]);
			OpenGL.s_GLVertexPointer_additive_position = 0;
		}

		// Token: 0x06000091 RID: 145 RVA: 0x00003F95 File Offset: 0x00002195
		public static void glVertexPointer_add(int size, int type, int stride, float[] pointer)
		{
			pointer.CopyTo(OpenGL.s_GLVertexPointer.pointer_, OpenGL.s_GLVertexPointer_additive_position);
			OpenGL.s_GLVertexPointer_additive_position += pointer.Length;
		}

		// Token: 0x06000092 RID: 146 RVA: 0x00003FBC File Offset: 0x000021BC
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

		// Token: 0x06000093 RID: 147 RVA: 0x00004078 File Offset: 0x00002278
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

		// Token: 0x06000094 RID: 148 RVA: 0x0000412C File Offset: 0x0000232C
		private static short[] InitializeTriangleStripIndices(int count)
		{
			short[] array = new short[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = (short)i;
			}
			return array;
		}

		// Token: 0x06000095 RID: 149 RVA: 0x00004154 File Offset: 0x00002354
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

		// Token: 0x06000096 RID: 150 RVA: 0x00004268 File Offset: 0x00002468
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

		// Token: 0x06000097 RID: 151 RVA: 0x00004380 File Offset: 0x00002580
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

		// Token: 0x06000098 RID: 152 RVA: 0x00004460 File Offset: 0x00002660
		private static BasicEffect getEffect(bool useTexture, bool useColor)
		{
			BasicEffect basicEffect = ((!useTexture) ? OpenGL.s_effectColor : (useColor ? OpenGL.s_effectTextureColor : OpenGL.s_effectTexture));
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

		// Token: 0x06000099 RID: 153 RVA: 0x00004540 File Offset: 0x00002740
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

		// Token: 0x0600009A RID: 154 RVA: 0x00004598 File Offset: 0x00002798
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

		// Token: 0x0600009B RID: 155 RVA: 0x000045D8 File Offset: 0x000027D8
		public static VertexPositionColor[] GetLastVertices_PositionColor()
		{
			return OpenGL.s_LastVertices_PositionColor;
		}

		// Token: 0x0600009C RID: 156 RVA: 0x000045E0 File Offset: 0x000027E0
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

		// Token: 0x0600009D RID: 157 RVA: 0x00004660 File Offset: 0x00002860
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
			VertexPositionColor[] array = (OpenGL.s_LastVertices_PositionColor = (value ? OpenGL.ConstructColorVertices() : OpenGL.ConstructCurrentColorVertices()));
			foreach (EffectPass effectPass in effect.CurrentTechnique.Passes)
			{
				effectPass.Apply();
				Global.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleStrip, array, 0, array.Length - 2);
			}
		}

		// Token: 0x0600009E RID: 158 RVA: 0x0000470C File Offset: 0x0000290C
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

		// Token: 0x0600009F RID: 159 RVA: 0x00004790 File Offset: 0x00002990
		public static VertexPositionNormalTexture[] GetLastVertices_PositionNormalTexture()
		{
			return OpenGL.s_LastVertices_PositionNormalTexture;
		}

		// Token: 0x060000A0 RID: 160 RVA: 0x00004798 File Offset: 0x00002998
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

		// Token: 0x060000A1 RID: 161 RVA: 0x0000481C File Offset: 0x00002A1C
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
			VertexPositionNormalTexture[] array = (OpenGL.s_LastVertices_PositionNormalTexture = OpenGL.ConstructTexturedVertices());
			foreach (EffectPass effectPass in effect.CurrentTechnique.Passes)
			{
				effectPass.Apply();
				Global.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList, array, 0, array.Length, indices, 0, indices.Length / 3);
			}
		}

		// Token: 0x060000A2 RID: 162 RVA: 0x000048D0 File Offset: 0x00002AD0
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

		// Token: 0x060000A3 RID: 163 RVA: 0x00004960 File Offset: 0x00002B60
		public static void glDrawElements(int mode, int count, short[] indices)
		{
			if (mode == 7)
			{
				OpenGL.DrawTriangleList(0, count, indices);
			}
		}

		// Token: 0x060000A4 RID: 164 RVA: 0x0000496E File Offset: 0x00002B6E
		public static void glScissor(double x, double y, double width, double height)
		{
			OpenGL.glScissor((int)x, (int)y, (int)width, (int)height);
		}

		// Token: 0x060000A5 RID: 165 RVA: 0x00004980 File Offset: 0x00002B80
		public static void glScissor(int x, int y, int width, int height)
		{
			try
			{
				Microsoft.Xna.Framework.Rectangle bounds = Global.XnaGame.GraphicsDevice.Viewport.Bounds;
				float num = FrameworkTypes.SCREEN_WIDTH / (float)bounds.Width;
				float num2 = FrameworkTypes.SCREEN_HEIGHT / (float)bounds.Height;
				Microsoft.Xna.Framework.Rectangle value = new Microsoft.Xna.Framework.Rectangle((int)((float)x / num), (int)((float)y / num2), (int)((float)width / num), (int)((float)height / num2));
				Global.GraphicsDevice.ScissorRectangle = Microsoft.Xna.Framework.Rectangle.Intersect(value, bounds);
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x060000A6 RID: 166 RVA: 0x00004A04 File Offset: 0x00002C04
		public static void glLineWidth(double width)
		{
			OpenGL.s_LineWidth = width;
		}

		// Token: 0x060000A7 RID: 167 RVA: 0x00004A0C File Offset: 0x00002C0C
		public static void setScissorRectangle(double x, double y, double w, double h)
		{
			OpenGL.setScissorRectangle((float)x, (float)y, (float)w, (float)h);
		}

		// Token: 0x060000A8 RID: 168 RVA: 0x00004A1B File Offset: 0x00002C1B
		public static void setScissorRectangle(float x, float y, float w, float h)
		{
			OpenGL.glScissor((double)x, (double)y, (double)w, (double)h);
		}

		// Token: 0x0400005E RID: 94
		private static Dictionary<int, bool> s_glServerSideFlags = new Dictionary<int, bool>();

		// Token: 0x0400005F RID: 95
		private static Dictionary<int, bool> s_glClientStateFlags = new Dictionary<int, bool>();

		// Token: 0x04000060 RID: 96
		private static RenderTarget2D s_RenderTarget;

		// Token: 0x04000061 RID: 97
		private static Viewport s_Viewport = default(Viewport);

		// Token: 0x04000062 RID: 98
		private static int s_glMatrixMode;

		// Token: 0x04000063 RID: 99
		private static List<Matrix> s_matrixModelViewStack = new List<Matrix>();

		// Token: 0x04000064 RID: 100
		private static Matrix s_matrixModelView = Matrix.Identity;

		// Token: 0x04000065 RID: 101
		private static Matrix s_matrixProjection = Matrix.Identity;

		// Token: 0x04000066 RID: 102
		private static CutTheRope.iframework.visual.Texture2D s_Texture;

		// Token: 0x04000067 RID: 103
		private static CutTheRope.iframework.visual.Texture2D s_Texture_OptimizeLastUsed;

		// Token: 0x04000068 RID: 104
		private static Color s_glClearColor = Color.White;

		// Token: 0x04000069 RID: 105
		private static Color s_Color = Color.White;

		// Token: 0x0400006A RID: 106
		private static BlendParams s_Blend = new BlendParams();

		// Token: 0x0400006B RID: 107
		private static RGBAColor[] s_GLColorPointer;

		// Token: 0x0400006C RID: 108
		private static OpenGL.GLVertexPointer s_GLVertexPointer;

		// Token: 0x0400006D RID: 109
		private static OpenGL.GLTexCoordPointer s_GLTexCoordPointer;

		// Token: 0x0400006E RID: 110
		private static int s_GLColorPointer_additive_position;

		// Token: 0x0400006F RID: 111
		private static int s_GLVertexPointer_additive_position;

		// Token: 0x04000070 RID: 112
		private static Vector3 normal = new Vector3(0f, 0f, 1f);

		// Token: 0x04000071 RID: 113
		private static BasicEffect s_effectTexture;

		// Token: 0x04000072 RID: 114
		private static BasicEffect s_effectColor;

		// Token: 0x04000073 RID: 115
		private static BasicEffect s_effectTextureColor;

		// Token: 0x04000074 RID: 116
		private static RasterizerState s_rasterizerStateSolidColor;

		// Token: 0x04000075 RID: 117
		private static RasterizerState s_rasterizerStateTexture;

		// Token: 0x04000076 RID: 118
		private static VertexPositionColor[] s_LastVertices_PositionColor = null;

		// Token: 0x04000077 RID: 119
		private static VertexPositionNormalTexture[] s_LastVertices_PositionNormalTexture = null;

		// Token: 0x04000078 RID: 120
		private static Microsoft.Xna.Framework.Rectangle ScreenRect = new Microsoft.Xna.Framework.Rectangle(0, 0, Global.GraphicsDevice.Viewport.Width, Global.GraphicsDevice.Viewport.Height);

		// Token: 0x04000079 RID: 121
		private static double s_LineWidth;

		// Token: 0x020000A4 RID: 164
		private class GLVertexPointer
		{
			// Token: 0x17000026 RID: 38
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

			// Token: 0x06000654 RID: 1620 RVA: 0x00033AF3 File Offset: 0x00031CF3
			public GLVertexPointer(int size, int type, int stride, object pointer)
			{
				this.pointer_ = ((pointer != null) ? ((float[])pointer) : null);
				this.size_ = size;
			}

			// Token: 0x04000884 RID: 2180
			public int size_;

			// Token: 0x04000885 RID: 2181
			public float[] pointer_;
		}

		// Token: 0x020000A5 RID: 165
		private class GLTexCoordPointer
		{
			// Token: 0x17000027 RID: 39
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

			// Token: 0x06000656 RID: 1622 RVA: 0x00033B39 File Offset: 0x00031D39
			public GLTexCoordPointer(int size, int type, int stride, object pointer)
			{
				this.pointer_ = ((pointer != null) ? ((float[])pointer) : null);
				this.size_ = size;
			}

			// Token: 0x04000886 RID: 2182
			public int size_;

			// Token: 0x04000887 RID: 2183
			public float[] pointer_;
		}
	}
}
