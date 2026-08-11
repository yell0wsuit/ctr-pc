using System.Collections.Generic;
using System.Threading.Tasks;

namespace CutTheRopeDX.Framework.Platform
{
    /// <summary>
    /// Raw content byte access, split into an asynchronous residency phase and a
    /// synchronous read.
    /// </summary>
    /// <remarks>
    /// Core reads content synchronously from deep inside constructors and frame updates,
    /// and a browser has no synchronous file read. The split is what reconciles the two:
    /// the host awaits <see cref="EnsureResidentAsync"/> between frames, and
    /// <see cref="Read"/> then always succeeds. A read miss is a residency bug rather
    /// than a runtime condition, so <see cref="Read"/> throws rather than returning null:
    /// failing loudly beats rendering nothing.
    /// </remarks>
    internal interface IContentStore
    {
        /// <summary>Whether the content can be read synchronously right now.</summary>
        /// <param name="relativePath">Path relative to the content root, e.g. <c>maps/1_1.xml</c>.</param>
        bool IsResident(string relativePath);

        /// <summary>Reads content bytes, throwing when they are not resident.</summary>
        /// <param name="relativePath">Path relative to the content root.</param>
        byte[] Read(string relativePath);

        /// <summary>Makes every listed path readable by <see cref="Read"/>.</summary>
        /// <param name="relativePaths">Paths relative to the content root.</param>
        Task EnsureResidentAsync(IEnumerable<string> relativePaths);
    }
}
