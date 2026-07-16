using System.Security.Cryptography;

namespace CutTheRopeDX.Content.Assets
{
    /// <summary>
    /// Locates and verifies assets in a local content source tree.
    /// </summary>
    /// <param name="contentDirectory">Root directory containing source assets.</param>
    /// <param name="manifest">Expected asset paths and hashes.</param>
    public sealed class AssetStore(string contentDirectory, AssetManifest manifest)
    {
        private readonly string _contentDirectory = Path.GetFullPath(contentDirectory);
        private readonly AssetManifest _manifest = manifest;

        /// <summary>
        /// Finds manifest entries that do not exist locally.
        /// </summary>
        /// <returns>The missing POSIX-relative paths.</returns>
        public IReadOnlyList<string> FindMissing()
        {
            List<string> missing = [];

            foreach (string relativePath in _manifest.Files.Keys)
            {
                if (!File.Exists(GetLocalPath(relativePath)))
                {
                    missing.Add(relativePath);
                }
            }

            return missing;
        }

        /// <summary>
        /// Verifies every local asset against the manifest.
        /// </summary>
        /// <returns>All missing and mismatched paths.</returns>
        public AssetVerificationResult Verify()
        {
            List<string> missing = [];
            List<string> mismatched = [];

            foreach ((string relativePath, string expectedHash) in _manifest.Files)
            {
                string localPath = GetLocalPath(relativePath);

                if (!File.Exists(localPath))
                {
                    missing.Add(relativePath);
                }
                else if (!string.Equals(
                    CalculateSha256(localPath),
                    expectedHash,
                    StringComparison.OrdinalIgnoreCase))
                {
                    mismatched.Add(relativePath);
                }
            }

            return new AssetVerificationResult(missing, mismatched);
        }

        internal string GetLocalPath(string relativePath)
        {
            return Path.Combine(
                _contentDirectory,
                relativePath.Replace('/', Path.DirectorySeparatorChar));
        }

        internal static string CalculateSha256(string path)
        {
            using FileStream stream = File.OpenRead(path);
            return Convert.ToHexStringLower(SHA256.HashData(stream));
        }
    }

    /// <summary>
    /// Contains the result of verifying a local asset tree.
    /// </summary>
    /// <param name="Missing">Manifest paths absent from the local tree.</param>
    /// <param name="Mismatched">Manifest paths whose hashes do not match.</param>
    public sealed record AssetVerificationResult(
        IReadOnlyList<string> Missing,
        IReadOnlyList<string> Mismatched)
    {
        /// <summary>
        /// Gets a value indicating whether every manifest entry passed verification.
        /// </summary>
        public bool Success => Missing.Count == 0 && Mismatched.Count == 0;
    }
}
