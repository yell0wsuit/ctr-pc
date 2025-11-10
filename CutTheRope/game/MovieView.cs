using CutTheRope.desktop;
using CutTheRope.iframework.core;

namespace CutTheRope.game
{
    internal class MovieView : MenuView
    {
        public override void update(float t)
        {
            Application.sharedMovieMgr().start();
            Global.MouseCursor.Enable(Application.sharedMovieMgr().isPaused());
        }

        public override void draw()
        {
            Global.XnaGame.DrawMovie();
        }
    }
}
