using System;

namespace CutTheRope.iframework
{
    internal struct PointSprite
    {
        public PointSprite(float xx, float yy, float s)
        {
            this.x = xx;
            this.y = yy;
            this.size = s;
        }

        public float x;

        public float y;

        public float size;
    }
}
