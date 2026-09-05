using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Guards that a box scan stays safe to run on a worker thread. The browser host runs
    /// it there while the game thread draws, so anything the scan reaches has to be either
    /// immutable or private to the call.
    /// </summary>
    public class LevelResourceScannerConcurrencyTests
    {
        [Fact]
        public async Task ConcurrentBoxScansAgreeOnTheSameResources()
        {
            HashSet<string> expected = LevelResourceScanner.GetBoxResources(0);
            Assert.NotEmpty(expected);

            HashSet<string>[] scans = await Task.WhenAll(
                Enumerable.Range(0, 16).Select(
                    _ => Task.Run(() => LevelResourceScanner.GetBoxResources(0))));

            foreach (HashSet<string> scan in scans)
            {
                Assert.True(
                    scan.SetEquals(expected),
                    "A concurrent box scan disagreed with the single-threaded result, which "
                    + "means the scan reached state another thread was changing.");
            }
        }

        [Fact]
        public async Task ConcurrentScansOfDifferentBoxesStayIndependent()
        {
            HashSet<string> firstAlone = LevelResourceScanner.GetBoxResources(0);
            HashSet<string> secondAlone = LevelResourceScanner.GetBoxResources(1);

            (HashSet<string> first, HashSet<string> second) = (
                await Task.WhenAll(
                    Task.Run(() => LevelResourceScanner.GetBoxResources(0)),
                    Task.Run(() => LevelResourceScanner.GetBoxResources(1))))
                is [HashSet<string> a, HashSet<string> b]
                ? (a, b)
                : default;

            Assert.True(first.SetEquals(firstAlone));
            Assert.True(second.SetEquals(secondAlone));
        }
    }
}
