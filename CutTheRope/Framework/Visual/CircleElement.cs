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
            OpenGLRenderer.GlDisable(OpenGLRenderer.GL_TEXTURE_2D);
            _ = MIN(width, height);
            OpenGLRenderer.GlEnable(OpenGLRenderer.GL_TEXTURE_2D);
            OpenGLRenderer.GlColor4f(Color.White);
            PostDraw();
        }

        public bool solid;

        public int vertextCount;
    }
}
