using System;
using CutTheRope.iframework.helpers;

namespace CutTheRope.iframework.visual
{
	// Token: 0x02000044 RID: 68
	internal class ScalableMultiParticles : MultiParticles
	{
		// Token: 0x06000238 RID: 568 RVA: 0x0000BEB0 File Offset: 0x0000A0B0
		public override void initParticle(ref Particle particle)
		{
			Image imageGrid = this.imageGrid;
			int num = MathHelper.RND(imageGrid.texture.quadsCount - 1);
			Quad2D qt = imageGrid.texture.quads[num];
			Quad3D qv = Quad3D.MakeQuad3D(0f, 0f, 0f, 0f, 0f);
			Rectangle rectangle = imageGrid.texture.quadRects[num];
			this.drawer.setTextureQuadatVertexQuadatIndex(qt, qv, this.particleCount);
			base.initParticle(ref particle);
			particle.width = rectangle.w;
			particle.height = rectangle.h;
		}
	}
}
