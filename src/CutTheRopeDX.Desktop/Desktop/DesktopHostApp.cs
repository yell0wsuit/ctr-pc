using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Platform;

namespace CutTheRopeDX.Desktop
{
    /// <summary>Forwards host-application calls to the MonoGame <see cref="Game1"/> instance.</summary>
    internal sealed class DesktopHostApp : IHostApp
    {
        /// <inheritdoc />
        public void Exit()
        {
            Global.XnaGame.Exit();
        }

        /// <inheritdoc />
        public bool IsKeyPressed(KeyCode key)
        {
            return Global.XnaGame.IsKeyPressed(key);
        }

        /// <inheritdoc />
        public void DrawMovie()
        {
            Global.XnaGame.DrawMovie();
        }
    }
}
