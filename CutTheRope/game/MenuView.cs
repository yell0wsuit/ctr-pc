using CutTheRope.desktop;
using CutTheRope.iframework.core;

namespace CutTheRope.game
{
    internal class MenuView : View
    {
        public override void update(float t)
        {
            Global.MouseCursor.Enable(true);
            base.update(t);
        }

        public override void draw()
        {
            OpenGL.glColor4f(Color.White);
            OpenGL.glEnable(0);
            OpenGL.glEnable(1);
            OpenGL.glBlendFunc(BlendingFactor.GL_ONE, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
            base.preDraw();
            base.postDraw();
            OpenGL.glDisable(0);
            OpenGL.glDisable(1);
        }
    }
}
