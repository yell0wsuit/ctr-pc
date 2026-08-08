using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Visual;

using static CutTheRopeDX.Framework.FrameworkTypes;
using static CutTheRopeDX.Framework.Helpers.CTRMathHelper;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// A wheel the player spins to reel a hook's rope in or out. Orthogonal to the rope source: two
    /// shipped grabs are a wheel and an auto-radius hook at once.
    /// </summary>
    internal sealed class WheelControl
    {
        /// <summary>Half-width of the square tap zone that claims a touch for the wheel.</summary>
        public const float TapHalfExtent = 110f;

        private Vector lastTouch;

        /// <summary>Gets the touch index currently spinning the wheel, or -1 when idle.</summary>
        public int OperatingTouch { get; private set; } = -1;

        /// <summary>Gets whether the arm scale needs recomputing.</summary>
        public bool IsDirty { get; private set; } = true;

        /// <summary>Gets or sets the wheel base image.</summary>
        public Image Base { get; set; }

        /// <summary>Gets or sets the wheel arm image.</summary>
        public Image Arm { get; set; }

        /// <summary>Gets or sets the wheel indicator image.</summary>
        public Image Indicator { get; set; }

        /// <summary>Gets or sets the wheel highlight image.</summary>
        public Image Highlight { get; set; }

        /// <summary>Records the touch point a rotation will be measured from.</summary>
        /// <param name="point">World-space touch point.</param>
        public void HandleTouch(Vector point)
        {
            lastTouch = point;
        }

        /// <summary>Claims a touch that lands on the wheel.</summary>
        /// <param name="grab">The hook this wheel belongs to.</param>
        /// <param name="worldX">Touch X in world space.</param>
        /// <param name="worldY">Touch Y in world space.</param>
        /// <param name="touchIndex">Touch index.</param>
        /// <returns><see langword="true"/> when the wheel took the touch.</returns>
        public bool TryBeginOperating(Grab grab, float worldX, float worldY, int touchIndex)
        {
            if (!PointInRect(
                    worldX, worldY,
                    grab.x - TapHalfExtent, grab.y - TapHalfExtent,
                    TapHalfExtent * 2f, TapHalfExtent * 2f))
            {
                return false;
            }

            HandleTouch(Vect(worldX, worldY));
            OperatingTouch = touchIndex;
            return true;
        }

        /// <summary>Releases the wheel if this touch was spinning it.</summary>
        /// <param name="touchIndex">Touch index being released.</param>
        public void EndOperating(int touchIndex)
        {
            if (OperatingTouch == touchIndex)
            {
                OperatingTouch = -1;
            }
        }

        /// <summary>Spins the wheel toward a new touch point and reels the rope accordingly.</summary>
        /// <param name="grab">The hook this wheel belongs to.</param>
        /// <param name="point">Current world-space touch point.</param>
        public void HandleRotate(Grab grab, Vector point)
        {
            if (lastTouch.X - point.X == 0f && lastTouch.Y - point.Y == 0f)
            {
                return;
            }

            CTRSoundMgr.PlaySound(Resources.Snd.Wheel);
            float rotateDelta = Grab.GetRotateAngleForStartEndCenter(lastTouch, point, Vect(grab.x, grab.y));
            if (rotateDelta > DEG_180)
            {
                rotateDelta -= DEG_360;
            }
            else if (rotateDelta < -DEG_180)
            {
                rotateDelta += DEG_360;
            }

            Arm.rotation += rotateDelta;
            Indicator.rotation += rotateDelta;
            Highlight.rotation += rotateDelta;

            float maxWheelDelta = ActivePhysicsConstants.GrabWheelRotateDeltaMax;
            float minWheelDelta = ActivePhysicsConstants.GrabWheelRotateDeltaMin;
            rotateDelta = rotateDelta > 0f
                ? MIN(MAX(minWheelDelta, rotateDelta), maxWheelDelta)
                : MAX(MIN(0f - minWheelDelta, rotateDelta), 0f - maxWheelDelta);

            Bungee rope = grab.Rope;
            if (rope != null)
            {
                float ropeLength = rope.GetLength();
                if (rotateDelta > 0f)
                {
                    if (ropeLength < ActivePhysicsConstants.GrabRopeRollMaxLength)
                    {
                        rope.Roll(rotateDelta);
                    }
                }
                else if (rotateDelta != 0f && rope.parts.Count > 3)
                {
                    _ = rope.RollBack(0f - rotateDelta);
                }

                IsDirty = true;
            }

            lastTouch = point;
        }

        /// <summary>Recomputes the arm scale from the rope's current length.</summary>
        /// <param name="grab">The hook this wheel belongs to.</param>
        public void UpdateArmScale(Grab grab)
        {
            if (!IsDirty)
            {
                return;
            }

            float wheelScaleLength = grab.Rope == null ? 0f : grab.Rope.GetLength() * 0.7f;
            if (wheelScaleLength == 0f)
            {
                Arm.scaleX = Arm.scaleY = 0f;
                return;
            }

            Arm.scaleX = Arm.scaleY = MAX(
                0f,
                MIN(1.2f, 1 - RT(wheelScaleLength / 1400f, wheelScaleLength / 700)));
        }
    }
}
