namespace CutTheRope.iframework
{
    internal struct CTRRectangle
    {
        public CTRRectangle(double xParam, double yParam, double width, double height)
        {
            x = (float)xParam;
            y = (float)yParam;
            w = (float)width;
            h = (float)height;
        }

        public CTRRectangle(float xParam, float yParam, float width, float height)
        {
            x = xParam;
            y = yParam;
            w = width;
            h = height;
        }

        public bool isValid()
        {
            return x != 0f || y != 0f || w != 0f || h != 0f;
        }

        public float x;

        public float y;

        public float w;

        public float h;
    }
}
