namespace CutTheRope.iframework.visual
{
    internal class KeyFrameValue
    {
        public KeyFrameValue()
        {
            action = new ActionParams();
            scale = new ScaleParams();
            pos = new PosParams();
            rotation = new RotationParams();
            color = new ColorParams();
        }

        public PosParams pos;

        public ScaleParams scale;

        public RotationParams rotation;

        public ColorParams color;

        public ActionParams action;
    }
}
