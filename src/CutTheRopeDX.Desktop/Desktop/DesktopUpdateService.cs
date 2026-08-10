using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.Helpers;

namespace CutTheRopeDX.Desktop
{
    /// <summary>Forwards update-service calls to the existing static <see cref="UpdateChecker"/>.</summary>
    internal sealed class DesktopUpdateService : IUpdateService
    {
        /// <inheritdoc />
        public void StartIfNeeded()
        {
            UpdateChecker.StartIfNeeded();
        }

        /// <inheritdoc />
        public void Cancel()
        {
            UpdateChecker.Cancel();
        }

        /// <inheritdoc />
        public bool TryConsumeUpdate(out UpdateInfo info)
        {
            return UpdateChecker.TryConsumeUpdate(out info);
        }
    }
}
