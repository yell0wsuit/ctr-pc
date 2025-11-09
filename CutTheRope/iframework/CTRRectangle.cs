using System;

namespace CutTheRope.iframework
{
    internal struct CTRRectangle
    {
        public CTRRectangle(double xParam, double yParam, double width, double height)
        {
            this.x = (float)xParam;
            this.y = (float)yParam;
            this.w = (float)width;
            this.h = (float)height;
        }

        public CTRRectangle(float xParam, float yParam, float width, float height)
        {
            this.x = xParam;
            this.y = yParam;
            this.w = width;
            this.h = height;
        }

        public bool isValid()
        {
            return this.x != 0f || this.y != 0f || this.w != 0f || this.h != 0f;
        }

        public float x;

        public float y;

        public float w;

        public float h;
    }
}
