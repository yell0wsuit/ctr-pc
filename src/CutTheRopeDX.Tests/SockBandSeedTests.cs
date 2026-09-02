using System.Collections.Generic;
using System.Globalization;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers how a player's hat seed is read from, and written to, hatseed.txt.
    /// </summary>
    public class SockBandSeedTests
    {
        [Fact]
        public void AnExistingFileSuppliesTheSeed()
        {
            FakeStore store = new();
            store.Write("hatseed.txt", "12345");

            Assert.Equal(12345UL, SockBandSeed.Read(store));
        }

        [Fact]
        public void AnExistingFileIsLeftAlone()
        {
            FakeStore store = new();
            store.Write("hatseed.txt", "12345");
            store.Writes = 0;

            _ = SockBandSeed.Read(store);

            Assert.Equal(0, store.Writes);
        }

        [Fact]
        public void SurroundingWhitespaceIsIgnored()
        {
            // The file is meant to be edited by hand, and a text editor leaves a trailing newline.
            FakeStore store = new();
            store.Write("hatseed.txt", "  12345\n");

            Assert.Equal(12345UL, SockBandSeed.Read(store));
        }

        [Fact]
        public void AMissingFileIsCreated()
        {
            FakeStore store = new();

            ulong seed = SockBandSeed.Read(store);

            Assert.Equal(seed.ToString(CultureInfo.InvariantCulture), store.Read("hatseed.txt")?.Trim());
        }

        [Fact]
        public void AMissingFileYieldsTheSameSeedOnTheNextRun()
        {
            FakeStore store = new();

            ulong first = SockBandSeed.Read(store);
            ulong second = SockBandSeed.Read(store);

            Assert.Equal(first, second);
        }

        [Fact]
        public void UnreadableContentsAreReplaced()
        {
            FakeStore store = new();
            store.Write("hatseed.txt", "not a number");

            ulong seed = SockBandSeed.Read(store);

            Assert.Equal(seed.ToString(CultureInfo.InvariantCulture), store.Read("hatseed.txt")?.Trim());
        }

        [Fact]
        public void AMissingStoreStillYieldsASeed()
        {
            // Headless runs install no preference store at all, and a level with hats still has to load.
            Assert.Equal(SockBandSeed.Read(null), SockBandSeed.Read(null));
        }

        /// <summary>In-memory stand-in for the save directory.</summary>
        private sealed class FakeStore : IPreferenceStore
        {
            internal int Writes { get; set; }

            public string Read(string name)
            {
                return blobs.TryGetValue(name, out string value) ? value : null;
            }

            public void Write(string name, string contents)
            {
                blobs[name] = contents;
                Writes++;
            }

            public IEnumerable<string> EnumerateBoxSlots()
            {
                return [];
            }

            private readonly Dictionary<string, string> blobs = [];
        }
    }
}
