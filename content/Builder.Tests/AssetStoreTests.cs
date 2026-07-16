using System.Security.Cryptography;
using System.Text;

using CutTheRopeDX.Content.Assets;

using Xunit;

namespace CutTheRopeDX.Content.Tests
{
    public sealed class AssetStoreTests
    {
        [Fact]
        public void FindMissingReturnsOnlyAbsentPaths()
        {
            using TestTemporaryDirectory temporaryDirectory = new();
            _ = temporaryDirectory.Write("sounds/present.wav", "present");
            AssetManifest manifest = ReadManifest(
                temporaryDirectory,
                ("sounds/present.wav", Hash("present")),
                ("sounds/missing.wav", Hash("missing")));
            AssetStore store = new(temporaryDirectory.Path, manifest);

            IReadOnlyList<string> missing = store.FindMissing();

            Assert.Equal(["sounds/missing.wav"], missing);

            _ = temporaryDirectory.Write("sounds/missing.wav", "missing");

            Assert.Empty(store.FindMissing());
        }

        [Fact]
        public void VerifyReportsEveryMissingAndMismatchedPath()
        {
            using TestTemporaryDirectory temporaryDirectory = new();
            _ = temporaryDirectory.Write("sounds/matching.wav", "matching");
            _ = temporaryDirectory.Write("sounds/wrong.wav", "wrong");
            AssetManifest manifest = ReadManifest(
                temporaryDirectory,
                ("sounds/matching.wav", Hash("matching")),
                ("sounds/wrong.wav", Hash("expected")),
                ("sounds/missing.wav", Hash("missing")));
            AssetStore store = new(temporaryDirectory.Path, manifest);

            AssetVerificationResult result = store.Verify();

            Assert.Equal(["sounds/missing.wav"], result.Missing);
            Assert.Equal(["sounds/wrong.wav"], result.Mismatched);
            Assert.False(result.Success);
        }

        [Fact]
        public void VerifySucceedsWhenEveryAssetMatches()
        {
            using TestTemporaryDirectory temporaryDirectory = new();
            _ = temporaryDirectory.Write("sounds/menu_music.wav", "music");
            AssetManifest manifest = ReadManifest(
                temporaryDirectory,
                ("sounds/menu_music.wav", Hash("music")));
            AssetStore store = new(temporaryDirectory.Path, manifest);

            AssetVerificationResult result = store.Verify();

            Assert.True(result.Success);
            Assert.Empty(result.Missing);
            Assert.Empty(result.Mismatched);
        }

        private static AssetManifest ReadManifest(
            TestTemporaryDirectory temporaryDirectory,
            params (string Path, string Hash)[] entries)
        {
            string files = string.Join(
                ",",
                entries.Select(entry => $"\"{entry.Path}\":\"{entry.Hash}\""));
            string path = temporaryDirectory.Write(
                "file_manifest.json",
                $"{{\"files\":{{{files}}}}}");
            return AssetManifest.Read(path);
        }

        private static string Hash(string contents)
        {
            return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(contents)));
        }
    }
}
