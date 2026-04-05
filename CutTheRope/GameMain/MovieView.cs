using CutTheRope.Desktop;
using CutTheRope.Framework.Core;

namespace CutTheRope.GameMain
{
    internal sealed class MovieView : MenuView
    {
        /// <inheritdoc />
        public override void Update(float t)
        {
            Application.SharedMovieMgr().Start();
            Global.MouseCursor.Enable(Application.SharedMovieMgr().IsPaused());
        }

        /// <inheritdoc />
        public override void Draw()
        {
            Global.XnaGame.DrawMovie();
        }
    }
}
