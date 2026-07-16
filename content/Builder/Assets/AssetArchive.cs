using System.IO.Compression;
using System.Security.Cryptography;

namespace CutTheRopeDX.Content.Assets
{
    /// <summary>
    /// Verifies and installs assets from a downloaded ZIP archive.
    /// </summary>
    /// <param name="archivePath">Path to the asset ZIP archive.</param>
    /// <param name="manifest">Expected canonical archive paths and hashes.</param>
    public sealed class AssetArchive(string archivePath, AssetManifest manifest)
    {
        private readonly string _archivePath = Path.GetFullPath(archivePath);
        private readonly AssetManifest _manifest = manifest;

        /// <summary>
        /// Verifies every manifest entry in the archive.
        /// </summary>
        /// <returns>All missing and mismatched canonical paths.</returns>
        public AssetVerificationResult Verify()
        {
            List<string> missing = [];
            List<string> mismatched = [];

            using ZipArchive archive = ZipFile.OpenRead(_archivePath);

            foreach ((string relativePath, string expectedHash) in _manifest.Files)
            {
                ZipArchiveEntry? entry = archive.GetEntry(relativePath);

                if (entry is null)
                {
                    missing.Add(relativePath);
                }
                else
                {
                    using Stream stream = entry.Open();
                    string actualHash = Convert.ToHexStringLower(SHA256.HashData(stream));

                    if (!string.Equals(
                        actualHash,
                        expectedHash,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        mismatched.Add(relativePath);
                    }
                }
            }

            return new AssetVerificationResult(missing, mismatched);
        }

        /// <summary>
        /// Installs requested archive entries without replacing existing local files.
        /// </summary>
        /// <param name="contentDirectory">Destination content root.</param>
        /// <param name="relativePaths">Canonical manifest paths to install.</param>
        public void Install(string contentDirectory, IReadOnlyCollection<string> relativePaths)
        {
            string destinationRoot = Path.GetFullPath(contentDirectory);
            using ZipArchive archive = ZipFile.OpenRead(_archivePath);

            foreach (string relativePath in relativePaths)
            {
                string destinationPath = Path.Combine(
                    destinationRoot,
                    relativePath.Replace('/', Path.DirectorySeparatorChar));

                if (File.Exists(destinationPath))
                {
                    continue;
                }

                ZipArchiveEntry entry = archive.GetEntry(relativePath)
                    ?? throw new InvalidDataException(
                        $"Asset archive is missing '{relativePath}'.");
                _ = Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);

                using Stream source = entry.Open();
                using FileStream destination = File.Create(destinationPath);
                source.CopyTo(destination);
            }
        }
    }
}
