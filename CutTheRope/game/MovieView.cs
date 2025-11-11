using CutTheRope.desktop;
using CutTheRope.iframework.core;

namespace CutTheRope.game
{
    internal sealed class MovieView : MenuView
    {
        public override void Update(float t)
        {
            Application.SharedMovieMgr().Start();
            Global.MouseCursor.Enable(Application.SharedMovieMgr().IsPaused());
        }

        public override void Draw()
        {
            Global.XnaGame.DrawMovie();
        }
    }
}
