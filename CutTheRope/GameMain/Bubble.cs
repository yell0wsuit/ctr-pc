using CutTheRope.Framework.Core;
using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Bubble game object that can be bound to a transporter and optionally draw itself with a shadow.
    /// </summary>
    internal class Bubble : GameObject, ITransporterItem, ITransporterBindAware
    {
        /// <summary>
        /// Creates a bubble from a texture.
        /// </summary>
        /// <param name="t">Texture used by the bubble.</param>
        /// <returns>The initialized bubble.</returns>
        public static Bubble Bubble_create(CTRTexture2D t)
        {
            return (Bubble)new Bubble().InitWithTexture(t);
        }

        /// <summary>
        /// Creates a bubble using a texture resource name and applies the specified quad.
        /// </summary>
        /// <param name="resourceName">Texture resource name.</param>
        /// <param name="q">Quad index to draw.</param>
        /// <returns>The initialized bubble.</returns>
        public static Bubble Bubble_createWithResIDQuad(string resourceName, int q)
        {
            Bubble bubble = Bubble_create(Application.GetTexture(resourceName));
            bubble.SetDrawQuad(q);
            return bubble;
        }

        /// <inheritdoc />
        public override void Draw()
        {
            PreDraw();
            if (!withoutShadow && !IsDrawnByTransporter)
            {
                if (quadToDraw == -1)
                {
                    DrawHelper.DrawImage(texture, drawX, drawY);
                }
                else
                {
                    DrawQuad(quadToDraw);
                }
            }
            PostDraw();
        }

        /// <summary>Whether the bubble has been popped.</summary>
        public bool popped;

        /// <summary>Initial bubble rotation used when restoring state.</summary>
        public float initial_rotation;

        /// <summary>Initial X position used when restoring state.</summary>
        public float initial_x;

        /// <summary>Initial Y position used when restoring state.</summary>
        public float initial_y;

        /// <summary>Initial rotated-circle binding used when restoring state.</summary>
        public RotatedCircle initial_rotatedCircle;

        /// <summary>Whether the bubble should skip drawing its own shadow.</summary>
        public bool withoutShadow;

        /// <summary>Whether the bubble is currently captured by a light bulb.</summary>
        public bool capturedByBulb;

        /// <inheritdoc />
        public float PositionOnTransporter { get; set; }

        /// <inheritdoc />
        public Vector BindPoint => Vect(x, y);

        /// <inheritdoc />
        public void SetBindPoint(Vector point)
        {
            x = point.X;
            y = point.Y;
        }

        /// <inheritdoc />
        public float CollisionRadius => 85f;

        /// <inheritdoc />
        public float MinScale => 0.5f;

        /// <inheritdoc />
        public float MaxScale => 1.0f;

        /// <inheritdoc />
        public float TransporterScale { get; set; } = 1.0f;

        /// <inheritdoc />
        public bool IsDrawnByTransporter { get; set; }

        /// <inheritdoc />
        public void WillBind()
        {
            IsDrawnByTransporter = true;
        }
    }
}
