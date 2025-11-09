using System;

namespace CutTheRope.iframework.visual
{
    internal class KeyFrameValue
    {
        public KeyFrameValue()
        {
            this.action = new ActionParams();
            this.scale = new ScaleParams();
            this.pos = new PosParams();
            this.rotation = new RotationParams();
            this.color = new ColorParams();
        }

        public PosParams pos;

        public ScaleParams scale;

        public RotationParams rotation;

        public ColorParams color;

        public ActionParams action;
    }
}
