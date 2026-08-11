using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Platform;

namespace CutTheRopeDX.GameMain
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
            PlatformServices.Cursor?.Enable(Application.SharedMovieMgr().IsPaused());
        }

        /// <inheritdoc />
        public override void Draw()
        {
            PlatformServices.Host?.DrawMovie();
        }
    }
}
