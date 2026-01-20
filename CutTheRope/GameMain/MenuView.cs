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
            OpenGL.GlColor4f(Color.White);
            OpenGL.GlEnable(OpenGL.GL_TEXTURE_2D);
            OpenGL.GlEnable(OpenGL.GL_BLEND);
            OpenGL.GlBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
            base.PreDraw();
            base.PostDraw();
            OpenGL.GlDisable(OpenGL.GL_TEXTURE_2D);
            OpenGL.GlDisable(OpenGL.GL_BLEND);
        }
    }
}
