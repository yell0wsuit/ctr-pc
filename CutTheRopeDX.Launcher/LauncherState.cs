using System;
using System.IO;

namespace CutTheRopeDX.Launcher
{
    /// <summary>
    /// The one thing the launcher remembers between runs: what happened last time it looked for Vulkan.
    /// </summary>
    /// <remarks>
    /// Deliberately not a cache of the answer. The probe runs on every launch so that a driver installed
    /// since last time is noticed straight away, which a stored result would hide until it was cleared.
    /// What is stored is narrower: a marker written immediately before the probe and replaced immediately
    /// after, so that a launch which never got to write the second value can be recognised as one that
    /// died inside the probe.
    /// <para>
    /// That case is not hypothetical and not catchable. A graphics driver that faults inside
    /// <c>vkCreateInstance</c> takes the process with it, and an access violation in native code is not an
    /// exception a <c>catch</c> can see. Without this marker such a machine never starts the game: every
    /// launch probes, and every probe kills it.
    /// </para>
    /// </remarks>
    internal static class LauncherState
    {
        /// <summary>Written before the probe; still present next launch only if that probe was fatal.</summary>
        public const string ProbingMarker = "probing";

        /// <summary>
        /// Whether a recorded value means the previous launch died inside the probe.
        /// </summary>
        /// <param name="recorded">Value from <see cref="Read"/>.</param>
        /// <returns><see langword="true"/> when the probe must be skipped this time.</returns>
        public static bool ProbeWasFatal(string recorded)
        {
            return string.Equals(recorded, ProbingMarker, StringComparison.Ordinal);
        }

        /// <summary>
        /// Backend recorded by the previous launch, if it completed.
        /// </summary>
        /// <param name="recorded">Value from <see cref="Read"/>.</param>
        /// <returns>The backend, or <see langword="null"/> when nothing usable is recorded.</returns>
        public static GraphicsBackend? LastBackend(string recorded)
        {
            return Enum.TryParse(recorded, out GraphicsBackend backend) ? backend : null;
        }

        /// <summary>
        /// Reads the recorded value.
        /// </summary>
        /// <returns>The stored text, or <see langword="null"/> when nothing is stored or it cannot be read.</returns>
        /// <remarks>Any failure reads as "nothing recorded", which only costs an extra probe.</remarks>
        public static string Read()
        {
            try
            {
                string path = RecordPath();
                return path is null || !File.Exists(path) ? null : File.ReadAllText(path).Trim();
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Records that a probe is about to run.
        /// </summary>
        /// <remarks>
        /// Must reach disk before the probe starts, which is why this writes rather than buffers. If the
        /// probe returns, <see cref="WriteBackend"/> replaces this and it is never seen again.
        /// </remarks>
        public static void WriteProbing()
        {
            Write(ProbingMarker);
        }

        /// <summary>
        /// Records the backend this launch settled on, clearing any probing marker.
        /// </summary>
        /// <param name="backend">Backend that was chosen.</param>
        public static void WriteBackend(GraphicsBackend backend)
        {
            Write(backend.ToString());
        }

        /// <summary>
        /// Clears the record, so the next launch probes as though it were the first.
        /// </summary>
        /// <remarks>
        /// Used when a backend is asked for explicitly: that launch proves nothing about what the machine
        /// can do on its own, so it must not leave a marker behind that looks like a fatal probe.
        /// </remarks>
        public static void Clear()
        {
            try
            {
                string path = RecordPath();
                if (path is not null && File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Writes <paramref name="value"/>, silently doing nothing when it cannot be stored.
        /// </summary>
        /// <param name="value">Value to store.</param>
        /// <remarks>
        /// A read-only or missing profile directory must never stop the game starting. The cost of failing
        /// here is only that a fatal probe is not remembered, which is the behaviour without this file.
        /// </remarks>
        private static void Write(string value)
        {
            try
            {
                string path = RecordPath();
                if (path is null)
                {
                    return;
                }
                _ = Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, value);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Directory the record is kept in, replacing the user's local application data.
        /// </summary>
        /// <remarks>
        /// For tests, which would otherwise write into the profile of whoever ran them and read state left
        /// by a real install of the game. Never set outside them.
        /// </remarks>
        internal static string OverrideDirectory { get; set; }

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
            string root = OverrideDirectory
                ?? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return string.IsNullOrEmpty(root) ? null : Path.Combine(root, "CutTheRopeDX", "graphics-backend");
        }
    }
}
