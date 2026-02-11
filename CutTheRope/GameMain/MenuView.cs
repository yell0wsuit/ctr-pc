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
            Renderer.GlColor4f(Color.White);
            Renderer.OpenGLEnable(Renderer.GL_TEXTURE_2D);
            Renderer.OpenGLEnable(Renderer.GL_BLEND);
            Renderer.GlBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
            base.PreDraw();
            base.PostDraw();
            Renderer.GlDisable(Renderer.GL_TEXTURE_2D);
            Renderer.GlDisable(Renderer.GL_BLEND);
        }
    }
}
