using CutTheRope.Desktop;
using CutTheRope.Framework.Core;

using Microsoft.Xna.Framework;

namespace CutTheRope.GameMain
{
    internal class MenuView : View
    {
        public override void Update(float t)
        {
            Global.MouseCursor.Enable(true);
            base.Update(t);
        }

        public override void Draw()
        {
            OpenGLRenderer.GlColor4f(Color.White);
            OpenGLRenderer.GlEnable(OpenGLRenderer.GL_TEXTURE_2D);
            OpenGLRenderer.GlEnable(OpenGLRenderer.GL_BLEND);
            OpenGLRenderer.GlBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
            base.PreDraw();
            base.PostDraw();
            OpenGLRenderer.GlDisable(OpenGLRenderer.GL_TEXTURE_2D);
            OpenGLRenderer.GlDisable(OpenGLRenderer.GL_BLEND);
        }
    }
}
