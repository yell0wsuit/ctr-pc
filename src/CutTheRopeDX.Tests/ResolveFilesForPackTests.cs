using System;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class ResolveFilesForPackTests
    {
        [Fact]
        public void NullPackResolvesToNothing()
        {
            Assert.Empty(ResourceMgr.ResolveFilesForPack(null));
        }

        [Fact]
        public void EmptyPackResolvesToNothing()
        {
            Assert.Empty(ResourceMgr.ResolveFilesForPack([]));
        }

        [Fact]
        public void StopsAtTheNullTerminator()
        {
            string[] pack = [Resources.Img.ObjCandy01New, null, Resources.Img.ObjSpider];
            string[] resolved = [.. ResourceMgr.ResolveFilesForPack(pack)];
            Assert.DoesNotContain(resolved, p => p.Contains("spider"));
        }

        [Fact]
        public void ImageResourceResolvesToItsPngAndAtlasJson()
        {
            string[] resolved = [.. ResourceMgr.ResolveFilesForPack([Resources.Img.ObjCandy01New])];
            Assert.Contains(resolved, p => p.EndsWith(".png", StringComparison.Ordinal));
            Assert.Contains(resolved, p => p.EndsWith(".json", StringComparison.Ordinal));
            Assert.All(resolved, p => Assert.StartsWith("images/", p));
        }

        [Fact]
        public void PathsUseForwardSlashesAndNoContentPrefix()
        {
            foreach (string path in ResourceMgr.ResolveFilesForPack([Resources.Img.ObjCandy01New]))
            {
                Assert.DoesNotContain('\\', path);
                Assert.False(path.StartsWith("content/", StringComparison.Ordinal));
            }
        }

        [Fact]
        public void SoundEffectResolvesToTheSfxDirectory()
        {
            Assert.Equal(
                ["sounds/sfx/tap.ogg"],
                ResourceMgr.ResolveFilesForPack([Resources.Snd.Tap]));
        }

        [Fact]
        public void MusicResolvesToTheMusicDirectory()
        {
            Assert.Equal(
                ["sounds/menu_music.ogg"],
                ResourceMgr.ResolveFilesForPack([Resources.Music.MenuMusic]));
        }

        [Fact]
        public void FontDoesNotResolveAsATexture()
        {
            Assert.Equal(
                [CTRResourceMgr.XNA_ResName(Resources.Fnt.BigFont)],
                ResourceMgr.ResolveFilesForPack([Resources.Fnt.BigFont]));
        }

        [Fact]
        public void ResolvingIsPureAndRepeatable()
        {
            string[] pack = [Resources.Img.ObjCandy01New];
            Assert.Equal(
                ResourceMgr.ResolveFilesForPack(pack),
                ResourceMgr.ResolveFilesForPack(pack));
        }
    }
}
