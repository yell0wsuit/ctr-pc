using System;

using CutTheRopeDX.Commons;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.GameMain;

using static CutTheRopeDX.Commons.PopupBuilder;

namespace CutTheRopeDX.Helpers
{
    /// <summary>
    /// Detects whether the application is running on an outdated Windows version.
    /// </summary>
    internal static class WindowsVersionChecker
    {
        /// <summary>
        /// Check if the current running Windows version is outdated.
        /// </summary>
        /// <returns><see langword="true"/> when running on Windows 8.1 or earlier (i.e. below Windows 10).</returns>
        public static bool IsOutdatedWindows()
        {
            return OperatingSystem.IsWindows() && !OperatingSystem.IsWindowsVersionAtLeast(10);
        }

        /// <summary>
        /// Shows a warning popup when running on an outdated Windows version.
        /// </summary>
        /// <param name="builder">The popup builder used to create and display the popup.</param>
        public static void ShowOutdatedWindowsPopup(PopupBuilder builder)
        {
            PopupTemplate template = PopupTemplate.Create(PopupSize.Large)
                .WithScaleMode(PopupScaleMode.Background)
                .AddText(Application.GetString("OUTDATED_WINDOWS_TITLE"), Resources.Fnt.BigFont, PopupAnchor.Text2, wrapWidth: 900f, offsetY: -200f)
                .AddScrollableText(Application.GetString("OUTDATED_WINDOWS_TEXT"), Resources.Fnt.SmallFont, PopupAnchor.Text3, wrapWidth: 800f, scrollHeight: 400f, offsetY: -20f)
                .AddButton(Application.GetString("OK"), MenuButtonId.PopupOk);

            _ = builder.Show(template);
        }
    }
}
