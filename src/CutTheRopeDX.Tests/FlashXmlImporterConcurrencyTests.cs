using System.Linq;
using System.Threading.Tasks;

using CutTheRopeDX.GameMain;
using CutTheRopeDX.Helpers;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Guards the animation parse cache against concurrent first use. The browser host
    /// preparses skin definitions on a worker thread while the game thread can reach the
    /// same files on demand, so both have to converge on one shared definition.
    /// </summary>
    public class FlashXmlImporterConcurrencyTests
    {
        [Fact]
        public async Task ConcurrentFirstParsesShareOneDefinition()
        {
            string path = ContentPaths.GetAnimationXmlAbsolutePath("fx_sleep.xml");

            FlashXmlAnimationDefinition[] parsed = await Task.WhenAll(
                Enumerable.Range(0, 16).Select(
                    _ => Task.Run(() => FlashXmlImporter.ParseFile(path))));

            // Reference equality is the assertion that matters: a cache that parses per
            // caller, or publishes more than once, hands back distinct definitions and
            // multiplies the work this preparse exists to avoid.
            foreach (FlashXmlAnimationDefinition definition in parsed)
            {
                Assert.NotNull(definition);
                Assert.Same(parsed[0], definition);
            }
        }

        [Fact]
        public async Task ConcurrentParsesOfDifferentFilesStayDistinct()
        {
            string sleep = ContentPaths.GetAnimationXmlAbsolutePath("fx_sleep.xml");
            string bubbles = ContentPaths.GetAnimationXmlAbsolutePath("fx_bubbles.xml");

            FlashXmlAnimationDefinition[] parsed = await Task.WhenAll(
                Task.Run(() => FlashXmlImporter.ParseFile(sleep)),
                Task.Run(() => FlashXmlImporter.ParseFile(bubbles)),
                Task.Run(() => FlashXmlImporter.ParseFile(sleep)),
                Task.Run(() => FlashXmlImporter.ParseFile(bubbles)));

            Assert.Same(parsed[0], parsed[2]);
            Assert.Same(parsed[1], parsed[3]);
            Assert.NotSame(parsed[0], parsed[1]);
        }
    }
}
