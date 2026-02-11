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
            target.X = x;
            target.Y = y;
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
                if (!SameSign(offset.X, target.X - pos.X) || !SameSign(offset.Y, target.Y - pos.Y))
                {
                    pos = target;
                }
            }
        }

        public void ApplyCameraTransformation()
        {
            Renderer.Translate(-pos.X, -pos.Y, 0f);
        }

        public void CancelCameraTransformation()
        {
            Renderer.Translate(pos.X, pos.Y, 0f);
        }

        public CAMERATYPE type;

        public float speed;

        public Vector pos;

        public Vector target;

        public Vector offset;
    }
}
