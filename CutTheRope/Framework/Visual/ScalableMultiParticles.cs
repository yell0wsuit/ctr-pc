namespace CutTheRope.Framework.Visual
{
    internal class ScalableMultiParticles : MultiParticles
    {
        public override void InitParticle(ref Particle particle)
        {
            Image imageGrid = this.imageGrid;
            int num = RND(imageGrid.texture.quadsCount - 1);
            Quad2D qt = imageGrid.texture.quads[num];
            Quad3D qv = Quad3D.MakeQuad3D(0f, 0f, 0f, 0f, 0f);
            CTRRectangle rectangle = imageGrid.texture.quadRects[num];
            drawer.SetTextureQuadatVertexQuadatIndex(qt, qv, particleCount);
            base.InitParticle(ref particle);
            particle.width = rectangle.w;
            particle.height = rectangle.h;
        }
    }
}
