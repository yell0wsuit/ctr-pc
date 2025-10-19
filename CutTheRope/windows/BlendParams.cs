using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.windows
{
	// Token: 0x02000008 RID: 8
	internal class BlendParams
	{
		// Token: 0x0600003A RID: 58 RVA: 0x00002FFC File Offset: 0x000011FC
		public BlendParams()
		{
			this.defaultBlending = true;
		}

		// Token: 0x0600003B RID: 59 RVA: 0x00003012 File Offset: 0x00001212
		public BlendParams(BlendingFactor s, BlendingFactor d)
		{
			this.sfactor = s;
			this.dfactor = d;
			this.defaultBlending = false;
			this.enabled = true;
		}

		// Token: 0x0600003C RID: 60 RVA: 0x0000303D File Offset: 0x0000123D
		public void enable()
		{
			this.enabled = true;
		}

		// Token: 0x0600003D RID: 61 RVA: 0x00003046 File Offset: 0x00001246
		public void disable()
		{
			this.enabled = false;
		}

		// Token: 0x0600003E RID: 62 RVA: 0x0000304F File Offset: 0x0000124F
		public static void applyDefault()
		{
			if (BlendParams.states[0] == null)
			{
				BlendParams.states[0] = BlendState.Opaque;
			}
			Global.GraphicsDevice.BlendState = BlendParams.states[0];
			Global.GraphicsDevice.BlendFactor = Color.White;
		}

		// Token: 0x0600003F RID: 63 RVA: 0x00003088 File Offset: 0x00001288
		public void apply()
		{
			if (this.defaultBlending || !this.enabled)
			{
				if (this.lastBlend != BlendParams.BlendType.Default)
				{
					this.lastBlend = BlendParams.BlendType.Default;
					BlendParams.applyDefault();
					return;
				}
			}
			else if (this.sfactor == BlendingFactor.GL_SRC_ALPHA && this.dfactor == BlendingFactor.GL_ONE_MINUS_SRC_ALPHA)
			{
				if (this.lastBlend != BlendParams.BlendType.SourceAlpha_InverseSourceAlpha)
				{
					this.lastBlend = BlendParams.BlendType.SourceAlpha_InverseSourceAlpha;
					if (BlendParams.states[(int)this.lastBlend] == null)
					{
						BlendState blendState = new BlendState();
						blendState.AlphaSourceBlend = Blend.SourceAlpha;
						blendState.AlphaDestinationBlend = Blend.InverseSourceAlpha;
						blendState.ColorDestinationBlend = blendState.AlphaDestinationBlend;
						blendState.ColorSourceBlend = blendState.AlphaSourceBlend;
						BlendParams.states[(int)this.lastBlend] = blendState;
					}
					Global.GraphicsDevice.BlendState = BlendParams.states[(int)this.lastBlend];
					return;
				}
			}
			else if (this.sfactor == BlendingFactor.GL_ONE && this.dfactor == BlendingFactor.GL_ONE_MINUS_SRC_ALPHA)
			{
				if (this.lastBlend != BlendParams.BlendType.One_InverseSourceAlpha)
				{
					this.lastBlend = BlendParams.BlendType.One_InverseSourceAlpha;
					if (BlendParams.states[(int)this.lastBlend] == null)
					{
						BlendState blendState2 = new BlendState();
						blendState2.AlphaSourceBlend = Blend.One;
						blendState2.AlphaDestinationBlend = Blend.InverseSourceAlpha;
						blendState2.ColorDestinationBlend = blendState2.AlphaDestinationBlend;
						blendState2.ColorSourceBlend = blendState2.AlphaSourceBlend;
						BlendParams.states[(int)this.lastBlend] = blendState2;
					}
					Global.GraphicsDevice.BlendState = BlendParams.states[(int)this.lastBlend];
					return;
				}
			}
			else if (this.sfactor == BlendingFactor.GL_SRC_ALPHA && this.dfactor == BlendingFactor.GL_ONE && this.lastBlend != BlendParams.BlendType.SourceAlpha_One)
			{
				this.lastBlend = BlendParams.BlendType.SourceAlpha_One;
				if (BlendParams.states[(int)this.lastBlend] == null)
				{
					BlendState blendState3 = new BlendState();
					blendState3.AlphaSourceBlend = Blend.SourceAlpha;
					blendState3.AlphaDestinationBlend = Blend.One;
					blendState3.ColorDestinationBlend = blendState3.AlphaDestinationBlend;
					blendState3.ColorSourceBlend = blendState3.AlphaSourceBlend;
					BlendParams.states[(int)this.lastBlend] = blendState3;
				}
				Global.GraphicsDevice.BlendState = BlendParams.states[(int)this.lastBlend];
			}
		}

		// Token: 0x06000040 RID: 64 RVA: 0x00003254 File Offset: 0x00001454
		public override string ToString()
		{
			if (!this.defaultBlending)
			{
				return string.Concat(new object[] { "BlendParams(src=", this.sfactor, ", dst=", this.dfactor, ", enabled=", this.enabled, ")" });
			}
			return "BlendParams(default)";
		}

		// Token: 0x0400002D RID: 45
		private static BlendState[] states = new BlendState[4];

		// Token: 0x0400002E RID: 46
		private BlendParams.BlendType lastBlend = BlendParams.BlendType.Unknown;

		// Token: 0x0400002F RID: 47
		private bool enabled;

		// Token: 0x04000030 RID: 48
		private bool defaultBlending;

		// Token: 0x04000031 RID: 49
		private BlendingFactor sfactor;

		// Token: 0x04000032 RID: 50
		private BlendingFactor dfactor;

		// Token: 0x020000A3 RID: 163
		private enum BlendType
		{
			// Token: 0x0400087F RID: 2175
			Unknown = -1,
			// Token: 0x04000880 RID: 2176
			Default,
			// Token: 0x04000881 RID: 2177
			SourceAlpha_InverseSourceAlpha,
			// Token: 0x04000882 RID: 2178
			One_InverseSourceAlpha,
			// Token: 0x04000883 RID: 2179
			SourceAlpha_One
		}
	}
}
