using CutTheRope.Desktop;

using Microsoft.Xna.Framework;

namespace CutTheRope.Framework.Visual
{
    internal sealed class CircleElement : BaseElement
    {
        public CircleElement()
        {
            vertextCount = 32;
            solid = true;
        }

        public override void Draw()
        {
            PreDraw();
            OpenGL.GlDisable(OpenGL.GL_TEXTURE_2D);
            _ = MIN(width, height);
            OpenGL.GlEnable(OpenGL.GL_TEXTURE_2D);
            OpenGL.GlColor4f(Color.White);
            PostDraw();
        }

        public bool solid;

        public int vertextCount;
    }
}
