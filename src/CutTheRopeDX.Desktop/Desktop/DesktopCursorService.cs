using CutTheRopeDX.Framework.Platform;

namespace CutTheRopeDX.Desktop
{
    /// <summary>Forwards cursor-service calls to <see cref="Global.MouseCursor"/>.</summary>
    internal sealed class DesktopCursorService : ICursorService
    {
        /// <inheritdoc />
        public void Enable(bool enabled)
        {
            Global.MouseCursor.Enable(enabled);
        }

        /// <inheritdoc />
        public void ReleaseButtons()
        {
            Global.MouseCursor.ReleaseButtons();
        }
    }
}
