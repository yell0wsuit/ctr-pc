using CutTheRope.Framework.Core;
using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    internal class Bubble : GameObject, IConveyorItem
    {
        public static Bubble Bubble_create(CTRTexture2D t)
        {
            return (Bubble)new Bubble().InitWithTexture(t);
        }

        public static Bubble Bubble_createWithResID(int r)
        {
            return Bubble_create(Application.GetTexture(ResourceNameTranslator.TranslateLegacyId(r)));
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
            if (!withoutShadow && ConveyorId == -1)
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

        public int ConveyorId { get; set; } = -1;

        public float? ConveyorBaseScaleX { get; set; }

        public float? ConveyorBaseScaleY { get; set; }
    }
}
