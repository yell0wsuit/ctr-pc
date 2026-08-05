namespace CutTheRopeDX.Launcher
{
    /// <summary>
    /// Decides when to tell the user that the game fell back to OpenGL.
    /// </summary>
    /// <remarks>
    /// Pure, so it can be tested without the file system or a dialog. What it reads is stored by
    /// <see cref="LauncherState"/>.
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
        /// A launch that died inside the probe leaves a marker that is not a backend name, so the recovery
        /// launch after it sees no last-seen value and does warn. That is intended: the player has not been
        /// told yet, and a driver crashing on probe is precisely the case they can act on.
        /// </para>
        /// <para>
        /// Never shown when the backend was asked for. Someone passing <c>--gl</c> already knows.
        /// </para>
        /// </remarks>
        public static bool ShouldWarn(GraphicsBackend chosen, GraphicsBackend? lastSeen, bool wasForced)
        {
            return !wasForced && chosen == GraphicsBackend.OpenGl && lastSeen != GraphicsBackend.OpenGl;
        }
    }
}
