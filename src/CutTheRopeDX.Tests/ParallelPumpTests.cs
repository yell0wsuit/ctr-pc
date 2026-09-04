using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

using CutTheRopeDX.Browser;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class ParallelPumpTests
    {
        [Fact]
        public async Task EveryItemIsHandedOutExactlyOnce()
        {
            string[] work = [.. Enumerable.Range(0, 500).Select(i => $"asset-{i}")];
            ConcurrentBag<string> handled = [];

            await ParallelPump.RunAsync(
                work,
                concurrency: 8,
                async path =>
                {
                    // Yielding is what lets the workers interleave, which is the only
                    // condition under which a shared cursor can hand the same index out
                    // twice or skip one entirely.
                    await Task.Yield();
                    handled.Add(path);
                },
                static _ => { });

            Assert.Equal(work.Length, handled.Count);
            Assert.Equal(work.Length, handled.Distinct().Count());
        }

        [Fact]
        public async Task ProgressCountsEveryCompletedItem()
        {
            string[] work = [.. Enumerable.Range(0, 500).Select(i => $"asset-{i}")];
            int highest = 0;

            await ParallelPump.RunAsync(
                work,
                concurrency: 8,
                static async _ => await Task.Yield(),
                done => highest = Math.Max(highest, done));

            Assert.Equal(work.Length, highest);
        }
    }
}
