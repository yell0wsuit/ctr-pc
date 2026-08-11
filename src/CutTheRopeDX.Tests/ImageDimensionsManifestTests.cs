using System.IO;
using System.Text.Json;

using CutTheRopeDX.Helpers;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public sealed class ImageDimensionsManifestTests
    {
        private static JsonElement LoadManifest()
        {
            string path = Path.Combine(ContentPaths.GetContentRootAbsolute(), "images", "image_dimensions.json");
            Assert.True(File.Exists(path), "image_dimensions.json missing from content output: " + path);
            using JsonDocument doc = JsonDocument.Parse(File.ReadAllText(path));
            return doc.RootElement.GetProperty("images").Clone();
        }

        [Fact]
        public void ManifestCoversAtlaslessGameplayImages()
        {
            JsonElement images = LoadManifest();

            // obj_axe has no TexturePacker atlas but is a gameplay hazard.
            JsonElement axe = images.GetProperty("obj_axe");
            Assert.True(axe.GetProperty("w").GetInt32() > 0);
            Assert.True(axe.GetProperty("h").GetInt32() > 0);
        }

        [Fact]
        public void ManifestCoversBackgroundsUnderSubdirectory()
        {
            JsonElement images = LoadManifest();

            JsonElement bg = images.GetProperty("backgrounds/bgr_01_p1");
            Assert.True(bg.GetProperty("w").GetInt32() > 0);
        }
    }
}
