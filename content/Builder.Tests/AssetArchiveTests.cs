using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

using CutTheRopeDX.Content.Assets;

using Xunit;

namespace CutTheRopeDX.Content.Tests
{
    public sealed class AssetArchiveTests
    {
        [Fact]
        public void VerifyAndInstallUseExactCanonicalPaths()
        {
            using TestTemporaryDirectory temporaryDirectory = new();
            string archivePath = CreateArchive(
                temporaryDirectory,
                ("sounds/menu_music.wav", "music"),
                ("images/menu_logo.png", "logo"));
            AssetManifest manifest = ReadManifest(
                temporaryDirectory,
                ("sounds/menu_music.wav", Hash("music")),
                ("images/menu_logo.png", Hash("logo")));
            string destination = Path.Combine(temporaryDirectory.Path, "content");
            AssetArchive archive = new(archivePath, manifest);

            AssetVerificationResult result = archive.Verify();
            archive.Install(destination, ["sounds/menu_music.wav", "images/menu_logo.png"]);

            Assert.True(result.Success);
            Assert.Equal(
                "music",
                File.ReadAllText(Path.Combine(destination, "sounds", "menu_music.wav")));
            Assert.Equal(
                "logo",
                File.ReadAllText(Path.Combine(destination, "images", "menu_logo.png")));
        }

        [Fact]
        public void VerifyReportsHashMismatch()
        {
            using TestTemporaryDirectory temporaryDirectory = new();
            string archivePath = CreateArchive(
                temporaryDirectory,
                ("sounds/menu_music.wav", "wrong"));
            AssetManifest manifest = ReadManifest(
                temporaryDirectory,
                ("sounds/menu_music.wav", Hash("expected")));
            AssetArchive archive = new(archivePath, manifest);

            AssetVerificationResult result = archive.Verify();

            Assert.Empty(result.Missing);
            Assert.Equal(["sounds/menu_music.wav"], result.Mismatched);
            Assert.False(result.Success);
        }

        [Fact]
        public void VerifyReportsAbsentCanonicalPath()
        {
            using TestTemporaryDirectory temporaryDirectory = new();
            string archivePath = CreateArchive(
                temporaryDirectory,
                ("sounds/menu_music_windows.wav", "music"));
            AssetManifest manifest = ReadManifest(
                temporaryDirectory,
                ("sounds/menu_music.wav", Hash("music")));
            AssetArchive archive = new(archivePath, manifest);

            AssetVerificationResult result = archive.Verify();

            Assert.Equal(["sounds/menu_music.wav"], result.Missing);
            Assert.Empty(result.Mismatched);
        }

        [Fact]
        public void InstallCopiesOnlyRequestedMissingFiles()
        {
            using TestTemporaryDirectory temporaryDirectory = new();
            string archivePath = CreateArchive(
                temporaryDirectory,
                ("sounds/menu_music.wav", "archive music"),
                ("images/menu_logo.png", "archive logo"));
            AssetManifest manifest = ReadManifest(
                temporaryDirectory,
                ("sounds/menu_music.wav", Hash("archive music")),
                ("images/menu_logo.png", Hash("archive logo")));
            string destination = Path.Combine(temporaryDirectory.Path, "content");
            _ = Directory.CreateDirectory(Path.Combine(destination, "sounds"));
            File.WriteAllText(
                Path.Combine(destination, "sounds", "menu_music.wav"),
                "local music");
            AssetArchive archive = new(archivePath, manifest);

            archive.Install(destination, ["sounds/menu_music.wav"]);

            Assert.Equal(
                "local music",
                File.ReadAllText(Path.Combine(destination, "sounds", "menu_music.wav")));
            Assert.False(File.Exists(Path.Combine(destination, "images", "menu_logo.png")));
        }

        private static string CreateArchive(
            TestTemporaryDirectory temporaryDirectory,
            params (string Path, string Contents)[] entries)
        {
            string archivePath = Path.Combine(temporaryDirectory.Path, "assets.zip");
            using ZipArchive archive = ZipFile.Open(archivePath, ZipArchiveMode.Create);

            foreach ((string path, string contents) in entries)
            {
                ZipArchiveEntry entry = archive.CreateEntry(path);
                using StreamWriter writer = new(entry.Open());
                writer.Write(contents);
            }

            return archivePath;
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
