using CutTheRopeDX.Desktop;
using CutTheRopeDX.Framework.Core;

namespace CutTheRopeDX.Framework.Helpers
{
    /// <summary>
    /// 2D camera that tracks a target point and applies translation to the renderer.
    /// </summary>
    internal sealed class Camera2D : FrameworkTypes
    {
        /// <summary>
        /// Initializes the camera with the specified movement speed and camera mode.
        /// </summary>
        /// <param name="s">Camera movement speed or proportional factor.</param>
        /// <param name="t">Movement mode used when approaching the target.</param>
        /// <returns>The initialized camera instance.</returns>
        public Camera2D InitWithSpeedandType(float s, CAMERATYPE t)
        {
            speed = s;
            type = t;
            return this;
        }

        /// <summary>
        /// Sets a new camera target and optionally snaps to it immediately.
        /// </summary>
        /// <param name="x">Target X coordinate.</param>
        /// <param name="y">Target Y coordinate.</param>
        /// <param name="immediate"><see langword="true" /> to snap instantly; <see langword="false" /> to move using the current camera mode.</param>
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

        /// <summary>
        /// Advances the camera position toward its target for one frame.
        /// </summary>
        /// <param name="delta">Elapsed frame time in seconds.</param>
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

        /// <summary>
        /// Applies the current camera translation to the renderer.
        /// </summary>
        public void ApplyCameraTransformation()
        {
            Renderer.Translate(-pos.X, -pos.Y, 0f);
        }

        /// <summary>
        /// Reverses the current camera translation on the renderer.
        /// </summary>
        public void CancelCameraTransformation()
        {
            Renderer.Translate(pos.X, pos.Y, 0f);
        }

        /// <summary>
        /// Current movement mode used when approaching the target.
        /// </summary>
        public CAMERATYPE type;

        /// <summary>
        /// Camera movement speed or proportional factor, depending on <see cref="type"/>.
        /// </summary>
        public float speed;

        /// <summary>
        /// Current camera position.
        /// </summary>
        public Vector pos;

        /// <summary>
        /// Target position the camera is moving toward.
        /// </summary>
        public Vector target;

        /// <summary>
        /// Per-frame movement offset applied while moving toward <see cref="target"/>.
        /// </summary>
        public Vector offset;
    }
}
