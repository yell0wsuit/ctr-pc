using System;

using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Particle system for the pump dirt/flow effect.
    /// </summary>
    internal sealed class PumpDirt : MultiParticles
    {
        /// <summary>
        /// Per-frame drag applied to particle velocity (at 60 FPS).
        /// </summary>
        private const float FlowDragPerFrame = 0.9f;

        /// <summary>
        /// Target frame rate used to normalize drag and travel distance.
        /// </summary>
        private const float TargetFps = 60f;

        /// <summary>
        /// Initializes the pump dirt system with default parameters.
        /// </summary>
        public PumpDirt InitWithTotalParticlesAngleandImageGrid(int p, float a, Image grid)
        {
            if (InitWithTotalParticlesandImageGrid(p, grid) == null)
            {
                return null;
            }
            duration = 0.6f;
            gravity.X = 0f;
            gravity.Y = 0f;
            angle = a;
            angleVar = 10f;
            speed = 1000f;
            speedVar = 100f;
            radialAccel = 0f;
            radialAccelVar = 0f;
            tangentialAccel = 0f;
            tangentialAccelVar = 0f;
            posVar.X = 0f;
            posVar.Y = 0f;
            life = 0.6f;
            lifeVar = 0f;
            size = 2f;
            sizeVar = 1f;
            emissionRate = 100f;
            startColor.RedColor = 1f;
            startColor.GreenColor = 1f;
            startColor.BlueColor = 1f;
            startColor.AlphaChannel = 0.6f;
            startColorVar.RedColor = 0f;
            startColorVar.GreenColor = 0f;
            startColorVar.BlueColor = 0f;
            startColorVar.AlphaChannel = 0f;
            endColor.RedColor = 1f;
            endColor.GreenColor = 1f;
            endColor.BlueColor = 1f;
            endColor.AlphaChannel = 0f;
            endColorVar.RedColor = 0f;
            endColorVar.GreenColor = 0f;
            endColorVar.BlueColor = 0f;
            endColorVar.AlphaChannel = 0f;
            blendAdditive = true;
            return this;
        }

        /// <summary>
        /// Initializes the pump dirt system and configures the travel length.
        /// </summary>
        public PumpDirt InitWithTotalParticlesAngleandImageGrid(int p, float a, Image grid, float flowLength)
        {
            PumpDirt result = InitWithTotalParticlesAngleandImageGrid(p, a, grid);
            if (result == null)
            {
                return null;
            }
            ConfigureForFlowLength(flowLength);
            return result;
        }

        public override void InitParticle(ref Particle particle)
        {
            base.InitParticle(ref particle);
            int num = RND_RANGE(6, 8);
            Quad2D qt = imageGrid.texture.quads[num];
            Quad3D qv = Quad3D.MakeQuad3D(0f, 0f, 0f, 0f, 0f);
            drawer.SetTextureQuadatVertexQuadatIndex(qt, qv, particleCount);
            CTRRectangle rectangle = imageGrid.texture.quadRects[num];
            particle.width = rectangle.w * particle.size;
            particle.height = rectangle.h * particle.size;
        }

        public override void UpdateParticle(ref Particle p, float delta)
        {
            if (p.life > 0f)
            {
                float frameDrag = MathF.Pow(FlowDragPerFrame, delta * TargetFps);
                p.dir = VectMult(p.dir, frameDrag);
                Vector v = VectMult(p.dir, delta);
                v = VectAdd(v, gravity);
                p.pos = VectAdd(p.pos, v);
                p.color.RedColor += p.deltaColor.RedColor * delta;
                p.color.GreenColor += p.deltaColor.GreenColor * delta;
                p.color.BlueColor += p.deltaColor.BlueColor * delta;
                p.color.AlphaChannel += p.deltaColor.AlphaChannel * delta;
                p.life -= delta;
                drawer.vertices[particleIdx] = Quad3D.MakeQuad3D((double)(p.pos.X - (p.width / 2f)), (double)(p.pos.Y - (p.height / 2f)), 0.0, p.width, p.height);
                for (int i = 0; i < 4; i++)
                {
                    colors[(particleIdx * 4) + i] = p.color;
                }
                particleIdx++;
                return;
            }
            if (particleIdx != particleCount - 1)
            {
                particles[particleIdx] = particles[particleCount - 1];
                drawer.vertices[particleIdx] = drawer.vertices[particleCount - 1];
                drawer.texCoordinates[particleIdx] = drawer.texCoordinates[particleCount - 1];
            }
            particleCount--;
        }

        /// <summary>
        /// Adjusts speed so particles travel approximately the requested flow length.
        /// </summary>
        private void ConfigureForFlowLength(float flowLength)
        {
            if (life <= 0f)
            {
                return;
            }
            float travel = MathF.Max(0f, flowLength);
            float frames = life * TargetFps;
            if (frames <= 0f)
            {
                return;
            }
            float denom = 1f - FlowDragPerFrame;
            float sum = MathF.Abs(denom) < 0.0001f
                ? frames
                : FlowDragPerFrame * (1f - MathF.Pow(FlowDragPerFrame, frames)) / denom;
            if (sum <= 0f)
            {
                return;
            }
            speed = travel * TargetFps / sum;
        }

        /// <summary>
        /// Updates the particle system and emits new particles while active.
        /// </summary>
        public override void Update(float delta)
        {
            base.Update(delta);
            if (active && emissionRate != 0f)
            {
                float num = 1f / emissionRate;
                emitCounter += delta;
                while (particleCount < totalParticles && emitCounter > num)
                {
                    _ = AddParticle();
                    emitCounter -= num;
                }
                elapsed += delta;
                if (duration != -1f && duration < elapsed)
                {
                    StopSystem();
                }
            }
            particleIdx = 0;
            while (particleIdx < particleCount)
            {
                UpdateParticle(ref particles[particleIdx], delta);
            }
        }
    }
}
