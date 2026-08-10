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
    }
}
