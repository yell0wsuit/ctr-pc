namespace CutTheRope.Framework
{
    internal struct CTRRectangle(float xParam, float yParam, float width, float height)
    {
        public readonly bool IsValid()
        {
            return x != 0f || y != 0f || w != 0f || h != 0f;
        }

        public float x = xParam;

        public float y = yParam;

        public float w = width;

        public float h = height;
    }
}
