using System;
using System.Collections.Generic;

using CutTheRopeDX.Commons;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>One surface size in the characterization matrix.</summary>
    /// <param name="Name">Short name, used in golden file names.</param>
    /// <param name="Width">Surface width in pixels.</param>
    /// <param name="Height">Surface height in pixels.</param>
    internal readonly record struct LayoutSurface(string Name, int Width, int Height);

    /// <summary>
    /// The surface sizes every size-parameterized characterization test runs against, and the
    /// helper that changes the surface without leaking the change into later tests.
    /// </summary>
    internal static class LayoutSurfaces
    {
        /// <summary>
        /// The matrix. Native is the design size; the rest cover the letterbox, pillarbox,
        /// crop-overflow and out-of-range cases the presentation math distinguishes.
        /// </summary>
        public static IReadOnlyList<LayoutSurface> All { get; } =
        [
            new("Native", 2560, 1440),
            new("SixteenNine", 1280, 720),
            new("FourThree", 1024, 768),
            new("Ultrawide", 2560, 1080),
            new("Superwide", 3840, 1080),
            new("Square", 1000, 1000),
            new("Portrait", 720, 1280),
            new("TallPortrait", 400, 1280),
        ];

        /// <summary>The matrix as xunit theory data.</summary>
        /// <returns>Name, width and height for each surface.</returns>
        public static TheoryData<string, int, int> Theory()
        {
            TheoryData<string, int, int> data = [];
            foreach (LayoutSurface surface in All)
            {
                data.Add(surface.Name, surface.Width, surface.Height);
            }
            return data;
        }

        /// <summary>
        /// Runs <paramref name="body"/> with the surface set to the given size, then always
        /// restores the default. The engine's screen state is process-wide and the suite runs
        /// serial, so a test that leaves a surface size behind corrupts every test after it.
        /// </summary>
        /// <param name="width">Surface width to run at.</param>
        /// <param name="height">Surface height to run at.</param>
        /// <param name="body">Work to run at that size.</param>
        public static void WithSurface(int width, int height, Action body)
        {
            try
            {
                CtrRenderer.OnSurfaceChanged(width, height);
                body();
            }
            finally
            {
                // Restore through the resize entry point, not just the presentation: a body that
                // called OnSurfaceChanged also moved REAL_SCREEN_*, VIEW_SCREEN_* and the
                // expanded-screen globals, and those would otherwise leak into every later test.
                CtrRenderer.OnSurfaceChanged(HeadlessHost.DefaultWidth, HeadlessHost.DefaultHeight);
            }
        }
    }
}
