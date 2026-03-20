using System;
using System.Collections.Generic;

using CutTheRope.Framework;
using CutTheRope.Framework.Core;

namespace CutTheRope.GameMain
{
    internal sealed class LightningTraceParticles : FrameworkTypes
    {
        private const int Capacity = 100;
        private const int FirstQuad = 24;
        private const int QuadCount = 3;

        private readonly List<SparkParticle> particles = [];

        private Vector emitterPosition;
        private float emitterRotation;
        private float emissionRate;
        private float emitCounter;

        public bool HasLiveParticles => emissionRate > 0f || particles.Count > 0;

        public void Reset()
        {
            particles.Clear();
            emitCounter = 0f;
            emissionRate = 0f;
        }

        public void SetPosition(Vector position)
        {
            emitterPosition = position;
        }

        public void SetRotation(float rotation)
        {
            emitterRotation = rotation;
        }

        public void SetEmissionRate(float rate)
        {
            emissionRate = MAX(0f, rate);
        }

        public void Update(float delta)
        {
            if (emissionRate > 0f)
            {
                float emissionInterval = 1f / emissionRate;
                emitCounter += delta;
                while (particles.Count < Capacity && emitCounter > emissionInterval)
                {
                    particles.Add(CreateParticle());
                    emitCounter -= emissionInterval;
                }
            }
            else
            {
                emitCounter = 0f;
            }

            for (int i = particles.Count - 1; i >= 0; i--)
            {
                SparkParticle particle = particles[i];
                particle.Life -= delta;
                if (particle.Life <= 0f)
                {
                    particles.RemoveAt(i);
                    continue;
                }

                particle.Position = VectAdd(particle.Position, VectMult(particle.Velocity, delta));
                particle.Rotation = RADIANS_TO_DEGREES(MathF.Atan2(particle.Velocity.Y, particle.Velocity.X) + 1.5708f);
                particles[i] = particle;
            }
        }

        public void AppendSprites(List<FingerTraceSpritePose> sprites)
        {
            foreach (SparkParticle particle in particles)
            {
                float alpha = FIT_TO_BOUNDARIES(particle.Life / particle.MaxLife, 0f, 1f);
                sprites.Add(new FingerTraceSpritePose(
                    FingerTraceSpriteKind.Spark,
                    Resources.Img.FingerTraces,
                    particle.QuadIndex,
                    particle.Position,
                    particle.Rotation,
                    particle.Scale,
                    alpha,
                    FingerTraceBlendMode.Alpha));
            }
        }

        private SparkParticle CreateParticle()
        {
            float angle = DEGREES_TO_RADIANS(emitterRotation + (70f * RND_MINUS1_1));
            Vector direction = new(Cosf(angle), Sinf(angle));
            float speed = 800f + (500f * RND_MINUS1_1);
            float life = MAX(0.05f, 0.25f + (0.05f * RND_MINUS1_1));

            return new SparkParticle
            {
                Position = emitterPosition,
                Velocity = VectMult(direction, speed),
                Rotation = RADIANS_TO_DEGREES(MathF.Atan2(direction.Y, direction.X) + 1.5708f),
                Scale = MAX(0.05f, 0.1f + (0.02f * RND_MINUS1_1)),
                Life = life,
                MaxLife = life,
                QuadIndex = FirstQuad + NextInt(QuadCount),
            };
        }

        private static int NextInt(int upperExclusive)
        {
            return upperExclusive <= 1
                ? 0
                : (int)(Arc4random() % (uint)upperExclusive);
        }

        private struct SparkParticle
        {
            public Vector Position;
            public Vector Velocity;
            public float Rotation;
            public float Scale;
            public float Life;
            public float MaxLife;
            public int QuadIndex;
        }
    }
}
