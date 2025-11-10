using CutTheRope.iframework.core;

namespace CutTheRope.iframework.visual
{
    internal struct Particle
    {
        public Vector startPos;

        public Vector pos;

        public Vector dir;

        public float radialAccel;

        public float tangentialAccel;

        public RGBAColor color;

        public RGBAColor deltaColor;

        public float size;

        public float deltaSize;

        public float life;

        public float deltaAngle;

        public float angle;

        public float width;

        public float height;
    }
}
