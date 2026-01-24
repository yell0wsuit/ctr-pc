using CutTheRope.Desktop;
using CutTheRope.Framework.Core;

namespace CutTheRope.Framework.Helpers
{
    internal sealed class Camera2D : FrameworkTypes
    {
        public Camera2D InitWithSpeedandType(float s, CAMERATYPE t)
        {
            speed = s;
            type = t;
            return this;
        }

        public void MoveToXYImmediate(float x, float y, bool immediate)
        {
            target.XAxis = x;
            target.YAxis = y;
            if (immediate)
            {
                pos = target;
                return;
            }
            if (type == CAMERATYPE.CAMERASPEEDDELAY)
            {
                offset = VectMult(VectSub(target, pos), speed);
                return;
            }
            if (type == CAMERATYPE.CAMERASPEEDPIXELS)
            {
                offset = VectMult(VectNormalize(VectSub(target, pos)), speed);
            }
        }

        public void Update(float delta)
        {
            if (!VectEqual(pos, target))
            {
                pos = VectAdd(pos, VectMult(offset, delta));
                // pos = Vect(Round(pos.x), Round(pos.y));
                if (!SameSign(offset.XAxis, target.XAxis - pos.XAxis) || !SameSign(offset.YAxis, target.YAxis - pos.YAxis))
                {
                    pos = target;
                }
            }
        }

        public void ApplyCameraTransformation()
        {
            OpenGL.GlTranslatef((double)(0f - pos.XAxis), (double)(0f - pos.YAxis), 0.0);
        }

        public void CancelCameraTransformation()
        {
            OpenGL.GlTranslatef(pos.XAxis, pos.YAxis, 0.0);
        }

        public CAMERATYPE type;

        public float speed;

        public Vector pos;

        public Vector target;

        public Vector offset;
    }
}
