using CutTheRope.Framework.Core;
using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    internal class Bubble : GameObject, ITransporterItem, ITransporterBindAware
    {
        public static Bubble Bubble_create(CTRTexture2D t)
        {
            return (Bubble)new Bubble().InitWithTexture(t);
        }

        /// <summary>
        /// Creates a bubble using a texture resource name and applies the specified quad.
        /// </summary>
        /// <param name="resourceName">Texture resource name.</param>
        /// <param name="q">Quad index to draw.</param>
        public static Bubble Bubble_createWithResIDQuad(string resourceName, int q)
        {
            Bubble bubble = Bubble_create(Application.GetTexture(resourceName));
            bubble.SetDrawQuad(q);
            return bubble;
        }

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

        public bool popped;

        public float initial_rotation;

        public float initial_x;

        public float initial_y;

        public RotatedCircle initial_rotatedCircle;

        public bool withoutShadow;

        public bool capturedByBulb;

        public float PositionOnTransporter { get; set; }

        public Vector BindPoint => Vect(x, y);

        public void SetBindPoint(Vector point)
        {
            x = point.X;
            y = point.Y;
        }

        public float CollisionRadius => 85f;

        public float MinScale => 0.5f;

        public float MaxScale => 1.0f;

        public float TransporterScale { get; set; } = 1.0f;

        public bool IsDrawnByTransporter { get; set; }

        public void WillBind()
        {
            IsDrawnByTransporter = true;
        }
    }
}
