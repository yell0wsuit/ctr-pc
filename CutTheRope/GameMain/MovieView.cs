using CutTheRope.Desktop;
using CutTheRope.Framework.Core;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Menu view variant that drives and draws movie playback.
    /// </summary>
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
