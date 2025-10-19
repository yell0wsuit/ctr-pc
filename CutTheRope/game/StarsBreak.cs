using System;
using CutTheRope.iframework;
using CutTheRope.iframework.visual;
using CutTheRope.windows;

namespace CutTheRope.game
{
	// Token: 0x02000097 RID: 151
	internal class StarsBreak : RotateableMultiParticles
	{
		// Token: 0x06000604 RID: 1540 RVA: 0x000324E4 File Offset: 0x000306E4
		public override Particles initWithTotalParticlesandImageGrid(int p, Image grid)
		{
			if (base.initWithTotalParticlesandImageGrid(p, grid) == null)
			{
				return null;
			}
			this.duration = 2f;
			this.gravity.x = 0f;
			this.gravity.y = 200f;
			this.angle = -90f;
			this.angleVar = 50f;
			this.speed = 150f;
			this.speedVar = 70f;
			this.radialAccel = 0f;
			this.radialAccelVar = 1f;
			this.tangentialAccel = 0f;
			this.tangentialAccelVar = 1f;
			this.x = FrameworkTypes.SCREEN_WIDTH / 2f;
			this.y = FrameworkTypes.SCREEN_HEIGHT / 2f;
			this.posVar.x = FrameworkTypes.SCREEN_WIDTH / 2f;
			this.posVar.y = FrameworkTypes.SCREEN_HEIGHT / 2f;
			this.life = 4f;
			this.lifeVar = 0f;
			this.size = 1f;
			this.sizeVar = 0f;
			this.emissionRate = 100f;
			this.startColor.r = 1f;
			this.startColor.g = 1f;
			this.startColor.b = 1f;
			this.startColor.a = 1f;
			this.startColorVar.r = 0f;
			this.startColorVar.g = 0f;
			this.startColorVar.b = 0f;
			this.startColorVar.a = 0f;
			this.endColor.r = 1f;
			this.endColor.g = 1f;
			this.endColor.b = 1f;
			this.endColor.a = 1f;
			this.endColorVar.r = 0f;
			this.endColorVar.g = 0f;
			this.endColorVar.b = 0f;
			this.endColorVar.a = 0f;
			this.rotateSpeed = 0f;
			this.rotateSpeedVar = 600f;
			this.blendAdditive = true;
			return this;
		}

		// Token: 0x06000605 RID: 1541 RVA: 0x00032724 File Offset: 0x00030924
		public override void draw()
		{
			this.preDraw();
			OpenGL.glBlendFunc(BlendingFactor.GL_SRC_ALPHA, BlendingFactor.GL_ONE);
			OpenGL.glEnable(0);
			OpenGL.glBindTexture(this.drawer.image.texture.name());
			OpenGL.glVertexPointer(3, 5, 0, FrameworkTypes.toFloatArray(this.drawer.vertices));
			OpenGL.glTexCoordPointer(2, 5, 0, FrameworkTypes.toFloatArray(this.drawer.texCoordinates));
			OpenGL.glEnableClientState(13);
			OpenGL.glBindBuffer(2, this.colorsID);
			OpenGL.glColorPointer(4, 5, 0, this.colors);
			OpenGL.glDrawElements(7, this.particleIdx * 6, this.drawer.indices);
			OpenGL.glBindBuffer(2, 0U);
			OpenGL.glDisableClientState(13);
			this.postDraw();
		}
	}
}
