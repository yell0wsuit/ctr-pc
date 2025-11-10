namespace CutTheRope.iframework.visual
{
    internal class ScalableMultiParticles : MultiParticles
    {
        public override void initParticle(ref Particle particle)
        {
            Image imageGrid = this.imageGrid;
            int num = RND(imageGrid.texture.quadsCount - 1);
            Quad2D qt = imageGrid.texture.quads[num];
            Quad3D qv = Quad3D.MakeQuad3D(0f, 0f, 0f, 0f, 0f);
            CTRRectangle rectangle = imageGrid.texture.quadRects[num];
            drawer.setTextureQuadatVertexQuadatIndex(qt, qv, particleCount);
            base.initParticle(ref particle);
            particle.width = rectangle.w;
            particle.height = rectangle.h;
        }
    }
}
