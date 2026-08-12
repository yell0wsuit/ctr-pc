using System;
using System.ComponentModel;
using System.Diagnostics;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Platform;

namespace CutTheRopeDX.Desktop
{
    /// <summary>Forwards host-application calls to the MonoGame <see cref="Game1"/> instance.</summary>
    internal sealed class DesktopHostApp : IHostApp
    {
        /// <inheritdoc />
        public bool CanExit => true;

        /// <inheritdoc />
        public string LevelEditorUrl => null;

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

        /// <inheritdoc />
        public void OpenUrl(string url)
        {
            try
            {
                ProcessStartInfo psi = new()
                {
                    FileName = url,
                    UseShellExecute = true
                };
                _ = Process.Start(psi);
            }
            catch (Win32Exception ex)
            {
                int errorCode = ex.ErrorCode;
            }
            catch (Exception)
            {
            }
        }
    }
}
