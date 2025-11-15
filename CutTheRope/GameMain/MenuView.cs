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
            OpenGL.GlEnable(0);
            OpenGL.GlEnable(1);
            OpenGL.GlBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
            base.PreDraw();
            base.PostDraw();
            OpenGL.GlDisable(0);
            OpenGL.GlDisable(1);
        }
    }
}
