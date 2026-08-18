using System;
using System.IO;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Compares a rendered layout description against a recorded golden file. On first run for a
    /// case the file is written and the test fails, so a baseline is always reviewed by a human
    /// before it starts guarding anything.
    /// </summary>
    internal static class LayoutBaseline
    {
        /// <summary>
        /// Asserts <paramref name="described"/> matches the golden file for
        /// <paramref name="caseName"/>.
        /// </summary>
        /// <param name="caseName">Stable name for this layout case; becomes the file name.</param>
        /// <param name="described">The rendered layout description.</param>
        public static void Assert(string caseName, string described)
        {
            string path = Path.Combine(AppContext.BaseDirectory, "Baselines", caseName + ".txt");
            if (!File.Exists(path))
            {
                _ = Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, described);
                Xunit.Assert.Fail(
                    $"No baseline for {caseName}. One has been written to {path}. Read it, confirm it "
                    + "describes the layout the game actually has, copy it into "
                    + "src/CutTheRopeDX.Tests/Baselines/, and re-run.");
            }

            Xunit.Assert.Equal(File.ReadAllText(path), described);
        }
    }
}
