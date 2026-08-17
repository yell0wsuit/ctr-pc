using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Platform;

namespace CutTheRopeDX.Framework.Helpers
{
    /// <summary>
    /// 2D camera that tracks a target point and applies translation to the renderer.
    /// </summary>
    internal sealed class Camera2D : FrameworkTypes
    {
        /// <summary>
        /// Uniform world-to-view scale. One means world units and view units coincide.
        /// </summary>
        public float Scale { get; set; } = 1f;

        /// <summary>
        /// World-space top-left of the region the camera shows, which is what drawing and hit
        /// testing use. Derived by <see cref="ApplyFit"/> from <see cref="pos"/>; equal to it
        /// until a fit says otherwise.
        /// </summary>
        /// <remarks>
        /// Kept apart from <see cref="pos"/> because the two answer different questions.
        /// <see cref="pos"/> is where the camera has been driven to, in the fixed frame levels are
        /// authored in, and the tracking code owns it. This is where that lands once the viewport
        /// has had its say. Deriving one from the other in both directions would make the fit read
        /// its own output back, and any viewport with slack would then walk the camera a little
        /// further every frame.
        /// </remarks>
        public Vector RenderPos => renderPos;

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
                renderPos = pos;
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
        /// Adopts a computed fit: takes its scale and the origin of its visible region. Writes
        /// only <see cref="RenderPos"/>, never <see cref="pos"/>, so applying a fit twice lands in
        /// the same place as applying it once.
        /// </summary>
        /// <param name="fit">The fit to adopt.</param>
        public void ApplyFit(CameraFit fit)
        {
            Scale = fit.Scale;
            renderPos.X = fit.VisibleWorld.x;
            renderPos.Y = fit.VisibleWorld.y;
        }

        /// <summary>
        /// Applies the current camera translation to the renderer.
        /// </summary>
        public void ApplyCameraTransformation()
        {
            Renderer.Scale(Scale, Scale, 1f);
            Renderer.Translate(-renderPos.X, -renderPos.Y, 0f);
        }

        /// <summary>
        /// Reverses the current camera translation on the renderer.
        /// </summary>
        public void CancelCameraTransformation()
        {
            Renderer.Translate(renderPos.X, renderPos.Y, 0f);
            Renderer.Scale(1f / Scale, 1f / Scale, 1f);
        }

        /// <summary>
        /// Converts a point in screen space into world space.
        /// </summary>
        /// <param name="sx">Screen-space X coordinate.</param>
        /// <param name="sy">Screen-space Y coordinate.</param>
        /// <returns>The corresponding world-space point.</returns>
        public Vector ScreenToWorld(float sx, float sy)
        {
            return Vect(ScreenToWorldX(sx), ScreenToWorldY(sy));
        }

        /// <summary>
        /// Converts a screen-space X coordinate into world space.
        /// </summary>
        /// <param name="sx">Screen-space X coordinate.</param>
        /// <returns>The corresponding world-space X coordinate.</returns>
        public float ScreenToWorldX(float sx)
        {
            return (sx / Scale) + renderPos.X;
        }

        /// <summary>
        /// Converts a screen-space Y coordinate into world space.
        /// </summary>
        /// <param name="sy">Screen-space Y coordinate.</param>
        /// <returns>The corresponding world-space Y coordinate.</returns>
        public float ScreenToWorldY(float sy)
        {
            return (sy / Scale) + renderPos.Y;
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
        /// Current camera position in the fixed frame levels are authored in. The tracking code
        /// drives this; the viewport never does.
        /// </summary>
        public Vector pos;

        /// <summary>
        /// Backing field for <see cref="RenderPos"/>.
        /// </summary>
        private Vector renderPos;

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
