using CutTheRope.Desktop;
using CutTheRope.Framework;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    internal sealed class CandyBreak : RotateableMultiParticles
    {
        public override Particles InitWithTotalParticlesandImageGrid(int p, Image grid)
        {
            if (base.InitWithTotalParticlesandImageGrid(p, grid) == null)
            {
                return null;
            }
            duration = 2f;
            gravity.x = 0f;
            gravity.y = 500f;
            angle = -90f;
            angleVar = 50f;
            speed = 150f;
            speedVar = 70f;
            radialAccel = 0f;
            radialAccelVar = 1f;
            tangentialAccel = 0f;
            tangentialAccelVar = 1f;
            posVar.x = 0f;
            posVar.y = 0f;
            life = 3f;
            lifeVar = 0f;
            size = 1f;
            sizeVar = 0f;
            emissionRate = 100f;
            startColor.r = 1f;
            startColor.g = 1f;
            startColor.b = 1f;
            startColor.a = 1f;
            startColorVar.r = 0f;
            startColorVar.g = 0f;
            startColorVar.b = 0f;
            startColorVar.a = 0f;
            endColor.r = 1f;
            endColor.g = 1f;
            endColor.b = 1f;
            endColor.a = 1f;
            endColorVar.r = 0f;
            endColorVar.g = 0f;
            endColorVar.b = 0f;
            endColorVar.a = 0f;
            rotateSpeed = 0f;
            rotateSpeedVar = 600f;
            blendAdditive = false;
            return this;
        }

        public override void InitParticle(ref Particle particle)
        {
            base.InitParticle(ref particle);
            int num = RND_RANGE(3, 7);
            Quad2D qt = imageGrid.texture.quads[num];
            Quad3D qv = Quad3D.MakeQuad3D(0f, 0f, 0f, 0f, 0f);
            drawer.SetTextureQuadatVertexQuadatIndex(qt, qv, particleCount);
            CTRRectangle rectangle = imageGrid.texture.quadRects[num];
            particle.width = rectangle.w * particle.size;
            particle.height = rectangle.h * particle.size;
        }

        public override void Draw()
        {
            PreDraw();
            OpenGL.GlBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
            OpenGL.GlEnable(0);
            OpenGL.GlBindTexture(drawer.image.texture.Name());
            OpenGL.GlVertexPointer(3, 5, 0, ToFloatArray(drawer.vertices));
            OpenGL.GlTexCoordPointer(2, 5, 0, ToFloatArray(drawer.texCoordinates));
            OpenGL.GlBindBuffer(2, colorsID);
            OpenGL.GlDrawElements(7, particleIdx * 6, drawer.indices);
            OpenGL.GlBindBuffer(2, 0U);
            OpenGL.GlDisableClientState(13);
            PostDraw();
        }
    }
}
