using CutTheRopeDX.Content.Assets;

using Xunit;

namespace CutTheRopeDX.Content.Tests
{
    public sealed class AssetManifestTests
    {
        [Fact]
        public void ReadReturnsFilesFromManifest()
        {
            using TestTemporaryDirectory temporaryDirectory = new();
            string path = temporaryDirectory.Write(
                "file_manifest.json",
                                     /*lang=json,strict*/
                                     """{"files":{"sounds/menu_music.wav":"abc123"}}""");

            AssetManifest manifest = AssetManifest.Read(path);

            Assert.Equal("abc123", manifest.Files["sounds/menu_music.wav"]);
        }

        [Fact]
        public void ReadReturnsEmptyFilesWhenObjectIsMissing()
        {
            using TestTemporaryDirectory temporaryDirectory = new();
            string path = temporaryDirectory.Write("file_manifest.json", /*lang=json,strict*/ """{"version":1}""");

            AssetManifest manifest = AssetManifest.Read(path);

            Assert.Empty(manifest.Files);
        }

        [Fact]
        public void ReadRetainsPosixPathSeparators()
        {
            using TestTemporaryDirectory temporaryDirectory = new();
            string path = temporaryDirectory.Write(
                "file_manifest.json",
                                     /*lang=json,strict*/
                                     """{"files":{"sounds/sfx/tap.wav":"abc123"}}""");

            AssetManifest manifest = AssetManifest.Read(path);

            Assert.Contains("sounds/sfx/tap.wav", manifest.Files.Keys);
        }
    }
}
