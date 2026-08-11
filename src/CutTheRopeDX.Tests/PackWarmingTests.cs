using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class PackWarmingTests
    {
        private sealed class RecordingStore : IContentStore
        {
            public List<string> Requested { get; } = [];

            public bool IsResident(string relativePath)
            {
                return true;
            }

            public byte[] Read(string relativePath)
            {
                return [];
            }

            public Task EnsureResidentAsync(IEnumerable<string> relativePaths)
            {
                Requested.AddRange(relativePaths);
                return Task.CompletedTask;
            }
        }

        [Fact]
        public void WarmPackRequestsEveryResolvedFile()
        {
            IContentStore previous = PlatformServices.Content;
            RecordingStore store = new();
            PlatformServices.Content = store;
            try
            {
                string[] pack = [Resources.Img.ObjCandy01New];
                ResourceMgr.WarmPack(pack);
                Assert.Equal(
                    ResourceMgr.ResolveFilesForPack(pack).OrderBy(path => path),
                    store.Requested.OrderBy(path => path));
            }
            finally
            {
                PlatformServices.Content = previous;
            }
        }

        [Fact]
        public void WarmPackToleratesNull()
        {
            IContentStore previous = PlatformServices.Content;
            RecordingStore store = new();
            PlatformServices.Content = store;
            try
            {
                ResourceMgr.WarmPack(null);
                Assert.Empty(store.Requested);
            }
            finally
            {
                PlatformServices.Content = previous;
            }
        }
    }
}
