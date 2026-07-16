using Microsoft.Xna.Framework.Content.Pipeline.Audio;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

using MonoGame.Framework.Content.Pipeline.Builder;

using Xunit;

namespace CutTheRopeDX.Content.Tests
{
    public sealed class MusicBuilderRulesTests
    {
        [Fact]
        public void WavMusicUsesBestQualitySongProcessor()
        {
            IContentCollection collection = new GameContentBuilder().GetContentCollection();

            bool handled = collection.GetContentInfo(
                "sounds/menu_music.wav",
                out List<ContentInfo>? contentInfos);

            ContentInfo contentInfo = Assert.Single(contentInfos!);
            SongProcessor processor = Assert.IsType<SongProcessor>(contentInfo.Processor);
            Assert.True(handled);
            Assert.True(contentInfo.ShouldBuild);
            Assert.Equal(ConversionQuality.Best, processor.Quality);
        }

        [Fact]
        public void OggMusicIsNotIncluded()
        {
            IContentCollection collection = new GameContentBuilder().GetContentCollection();

            bool handled = collection.GetContentInfo(
                "sounds/menu_music.ogg",
                out List<ContentInfo>? contentInfos);

            Assert.False(handled);
            Assert.Empty(contentInfos!);
        }
    }
}
