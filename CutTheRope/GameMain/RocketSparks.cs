using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Particle system that emits small spark particles from the rocket's exhaust.
    /// Particles are randomly selected from quad indices 6–9 of the rocket sprite sheet.
    /// </summary>
    internal class RocketSparks : RotatableScalableMultiParticles
    {
        /// <summary>
        /// Initializes the spark particle system with the given particle count, emission angle,
        /// and image grid. Configures particle lifetime, speed, color fade, and additive blending.
        /// </summary>
        /// <param name="p">The maximum number of particles.</param>
        /// <param name="a">The base emission angle in radians.</param>
        /// <param name="grid">The image grid containing spark particle quads.</param>
        /// <returns>This instance if initialization succeeds; otherwise, <c>null</c>.</returns>
        public virtual Particles InitWithTotalParticlesAngleandImageGrid(int p, float a, Image grid)
        {
            if (InitWithTotalParticlesandImageGrid(p, grid) == null)
            {
                return null;
            }
            duration = -1f;
            gravity.X = 0f;
            gravity.Y = 0f;
            angle = a;
            angleVar = 10f;
            speed = 50f;
            speedVar = 10f;
            radialAccel = 0f;
            radialAccelVar = 0f;
            tangentialAccel = 0f;
            tangentialAccelVar = 0f;
            posVar.X = 5f;
            posVar.Y = 5f;
            life = 0.5f;
            lifeVar = 0.1f;
            size = 0.5f;
            sizeVar = 0f;
            endSize = size;
            emissionRate = 20f;
            startColor.RedColor = 1f;
            startColor.GreenColor = 1f;
            startColor.BlueColor = 1f;
            startColor.AlphaChannel = 1f;
            startColorVar.RedColor = 0f;
            startColorVar.GreenColor = 0f;
            startColorVar.BlueColor = 0f;
            startColorVar.AlphaChannel = 0f;
            endColor.RedColor = 0f;
            endColor.GreenColor = 0f;
            endColor.BlueColor = 0f;
            endColor.AlphaChannel = 0f;
            endColorVar.RedColor = 0f;
            endColorVar.GreenColor = 0f;
            endColorVar.BlueColor = 0f;
            endColorVar.AlphaChannel = 0f;
            blendAdditive = true;
            return this;
        }

        /// <summary>
        /// Initializes an individual spark particle by assigning it a random quad (indices 6–9)
        /// from the rocket sprite sheet and setting its dimensions accordingly.
        /// </summary>
        /// <param name="particle">The particle to initialize.</param>
        public override void InitParticle(ref Particle particle)
        {
            base.InitParticle(ref particle);
            int num = RND_RANGE(6, 9);
            Quad2D quad2D = imageGrid.texture.quads[num];
            Quad3D quad3D = Quad3D.MakeQuad3D(0f, 0f, 0f, 0f, 0f);
            drawer.SetTextureQuadatVertexQuadatIndex(quad2D, quad3D, particleCount);
            Vector quadSize = Image.GetQuadSize(Resources.Img.ObjRocket, num);
            particle.width = quadSize.X;
            particle.height = quadSize.Y;
        }
    }
}
