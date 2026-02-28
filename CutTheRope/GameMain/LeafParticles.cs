using CutTheRope.Framework;
using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Rotatable debris particle system used for bamboo tube exit effects.
    /// </summary>
    internal sealed class LeafParticles : RotateableMultiParticles
    {
        /// <summary>
        /// Initialises the particle system.
        /// </summary>
        /// <param name="totalParticles">Maximum live particle count.</param>
        /// <param name="angle">Emission direction in degrees.</param>
        /// <param name="grid">Texture atlas containing the debris quad at index 3.</param>
        /// <param name="bgx">
        /// Optional horizontal gravity bias. The magnitude is stored in <c>baseGravityX</c>
        /// and is smoothly reduced to zero over the system lifetime by <see cref="Update"/>.
        /// </param>
        public LeafParticles Init(int totalParticles, float angle, Image grid, float bgx = 0f)
        {
            if (InitWithTotalParticlesandImageGrid(totalParticles, grid) == null)
            {
                return null;
            }

            duration = 5f;
            gravity.X = bgx;
            gravity.Y = 75f;
            baseGravityX = bgx < 0f ? 0f - bgx : bgx;
            this.angle = angle;
            angleVar = 45f;
            speed = 100f;
            speedVar = 10f;
            radialAccel = 0f;
            radialAccelVar = 1f;
            tangentialAccel = 0f;
            tangentialAccelVar = 1f;
            posVar.X = 0f;
            posVar.Y = 0f;
            life = 5f;
            lifeVar = 0f;
            size = 1f;
            sizeVar = 0f;
            emissionRate = 100f;
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
            rotateSpeed = 0f;
            rotateSpeedVar = 600f;
            blendAdditive = false;
            return this;
        }

        /// <summary>
        /// Ticks the particle system and gradually decays the horizontal gravity bias to zero.
        /// Guards against division by zero when <c>life</c> has expired.
        /// </summary>
        public override void Update(float delta)
        {
            base.Update(delta);
            float gravitySpeed = life <= 0f ? 0f : baseGravityX / life;
            float gravityX = gravity.X;
            _ = Mover.MoveVariableToTarget(ref gravityX, 0f, gravitySpeed, delta);
            gravity.X = gravityX;
        }

        /// <summary>
        /// Overrides the base particle initialisation to select the debris quad (index 3)
        /// from the atlas and randomise the particle size.
        /// Desktop note: size is scaled 3× versus iOS to compensate for higher pixel density.
        /// </summary>
        public override void InitParticle(ref Particle particle)
        {
            base.InitParticle(ref particle);

            Quad2D qt = imageGrid.texture.quads[3];
            Quad3D qv = Quad3D.MakeQuad3D(0f, 0f, 0f, 0f, 0f);
            drawer.SetTextureQuadatVertexQuadatIndex(qt, qv, particleCount);

            particle.width = ((RND_MINUS1_1 * 4f) + 12f) * 3;
            particle.height = ((RND_MINUS1_1 * 4f) + 22f) * 3;
        }

        /// <summary>
        /// Absolute value of the initial horizontal gravity bias, used to compute the decay rate per frame.
        /// </summary>
        private float baseGravityX;
    }
}
