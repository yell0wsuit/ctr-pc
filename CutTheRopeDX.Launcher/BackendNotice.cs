using System;
using System.IO;

namespace CutTheRopeDX.Launcher
{
    /// <summary>
    /// Decides when to tell the user that the game fell back to OpenGL, and remembers that it was said.
    /// </summary>
    /// <remarks>
    /// The decision is separated from the storage so it can be tested without touching the file system.
    /// </remarks>
    internal static class BackendNotice
    {
        /// <summary>
        /// Whether the fallback is worth interrupting the launch for.
        /// </summary>
        /// <param name="chosen">Backend the launcher settled on.</param>
        /// <param name="lastSeen">Backend recorded on the previous launch, or <see langword="null"/> when none is.</param>
        /// <param name="wasForced">Whether the backend came from a switch or environment variable.</param>
        /// <returns><see langword="true"/> when the dialog should be shown.</returns>
        /// <remarks>
        /// Only the change is worth reporting. Warning on every launch would train the player to dismiss
        /// the dialog without reading it, and the message it carries matters exactly once: when the machine
        /// stops being able to do something it previously could, or was expected to.
        /// <para>
        /// Never shown when the backend was asked for. Someone passing <c>--gl</c> already knows.
        /// </para>
        /// </remarks>
        public static bool ShouldWarn(GraphicsBackend chosen, GraphicsBackend? lastSeen, bool wasForced)
        {
            return !wasForced && chosen == GraphicsBackend.OpenGl && lastSeen != GraphicsBackend.OpenGl;
        }

        /// <summary>
        /// Reads the backend recorded by the previous launch.
        /// </summary>
        /// <returns>The recorded backend, or <see langword="null"/> when nothing usable is stored.</returns>
        /// <remarks>Any failure reads as "nothing recorded", which at worst shows the dialog once more.</remarks>
        public static GraphicsBackend? ReadLastSeen()
        {
            try
            {
                string path = RecordPath();
                return path is null || !File.Exists(path)
                    ? null
                    : Enum.TryParse(File.ReadAllText(path).Trim(), out GraphicsBackend backend) ? backend : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Records the backend this launch settled on.
        /// </summary>
        /// <param name="backend">Backend to record.</param>
        /// <remarks>
        /// Silent on failure. A read-only or missing profile directory must not stop the game starting, and
        /// the only cost is that the dialog appears again next time.
        /// </remarks>
        public static void WriteLastSeen(GraphicsBackend backend)
        {
            try
            {
                string path = RecordPath();
                if (path is null)
                {
                    return;
                }
                _ = Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, backend.ToString());
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Path of the record, under the user's local application data.
        /// </summary>
        /// <returns>The full path, or <see langword="null"/> when the platform reports no such location.</returns>
        /// <remarks>
        /// Kept out of the install directory on purpose: that is frequently read-only, and the answer is a
        /// property of the machine and user rather than of the copy of the game.
        /// </remarks>
        private static string RecordPath()
        {
            string root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return string.IsNullOrEmpty(root) ? null : Path.Combine(root, "CutTheRopeDX", "graphics-backend");
        }
    }
}
