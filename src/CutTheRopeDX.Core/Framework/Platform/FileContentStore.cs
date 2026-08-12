using System.IO;

namespace CutTheRopeDX.Framework.Platform
{
    /// <summary>Content store backed by the deployed content directory.</summary>
    /// <param name="root">Absolute path to the content root.</param>
    internal sealed class FileContentStore(string root) : IContentStore
    {
        private string Resolve(string relativePath)
        {
            return Path.Combine(root, relativePath.Replace('\\', '/').Replace('/', Path.DirectorySeparatorChar));
        }

        /// <inheritdoc />
        public byte[] Read(string relativePath)
        {
            return File.ReadAllBytes(Resolve(relativePath));
        }
    }
}
