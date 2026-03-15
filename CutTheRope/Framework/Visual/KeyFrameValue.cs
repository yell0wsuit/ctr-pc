namespace CutTheRope.Framework.Visual
{
    internal sealed class KeyFrameValue
    {
        public KeyFrameValue()
        {
            action = new ActionParams();
            scale = new ScaleParams();
            pos = new PosParams();
            rotation = new RotationParams();
            skew = new SkewParams();
            color = new ColorParams();
        }

        public PosParams pos;

        public ScaleParams scale;

        public RotationParams rotation;

        public SkewParams skew;

        public ColorParams color;

        public ActionParams action;
    }
}
