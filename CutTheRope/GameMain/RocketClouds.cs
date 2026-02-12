using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Particle system that emits cloud/smoke particles from the rocket's exhaust.
    /// Extends <see cref="RocketSparks"/> with wider position variance, shorter lifetime,
    /// and uses quad index 5 for a cloud-like appearance.
    /// </summary>
    internal sealed class RocketClouds : RocketSparks
    {
        /// <summary>
        /// Initializes the cloud particle system with the given particle count, emission angle,
        /// and image grid. Configures wider spread, shorter lifetime, and growing particle size
        /// compared to the base spark system.
        /// </summary>
        /// <param name="p">The maximum number of particles.</param>
        /// <param name="a">The base emission angle in radians.</param>
        /// <param name="grid">The image grid containing cloud particle quads.</param>
        /// <returns>This instance if initialization succeeds; otherwise, <c>null</c>.</returns>
        public override Particles InitWithTotalParticlesAngleandImageGrid(int p, float a, Image grid)
        {
            if (InitWithTotalParticlesandImageGrid(p, grid) == null)
            {
                return null;
            }
            duration = -1f;
            gravity.X = 0f;
            gravity.Y = 0f;
            angle = a;
            angleVar = 15f;
            speed = 50f;
            speedVar = 10f;
            radialAccel = 0f;
            radialAccelVar = 0f;
            tangentialAccel = 0f;
            tangentialAccelVar = 0f;
            posVar.X = 10f;
            posVar.Y = 10f;
            life = 0.4f;
            lifeVar = 0.1f;
            size = 0.8f;
            sizeVar = 0f;
            endSize = 1f;
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
        /// Initializes an individual cloud particle using quad index 5 from the rocket sprite
        /// sheet and sets its dimensions accordingly.
        /// </summary>
        /// <param name="particle">The particle to initialize.</param>
        public override void InitParticle(ref Particle particle)
        {
            base.InitParticle(ref particle);
            Quad2D quad2D = imageGrid.texture.quads[5];
            Quad3D quad3D = Quad3D.MakeQuad3D(0f, 0f, 0f, 0f, 0f);
            drawer.SetTextureQuadatVertexQuadatIndex(quad2D, quad3D, particleCount);
            Vector quadSize = Image.GetQuadSize(Resources.Img.ObjRocket, 5);
            particle.width = quadSize.X;
            particle.height = quadSize.Y;
        }
    }
}
