using System;
using System.IO;
using System.Linq;

using CutTheRopeDX.Framework.Core;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class PreferenceStoreTests : IDisposable
    {
        private readonly string _dir = Path.Combine(
            Path.GetTempPath(), "ctrdx-prefs-" + Guid.NewGuid().ToString("N"));

        public void Dispose()
        {
            if (Directory.Exists(_dir))
            {
                Directory.Delete(_dir, recursive: true);
            }
            GC.SuppressFinalize(this);
        }

        [Fact]
        public void ReadingAnAbsentNameReturnsNull()
        {
            FilePreferenceStore store = new(_dir);
            Assert.Null(store.Read("ctr_preferences.json"));
        }

        [Fact]
        public void WriteThenReadRoundTrips()
        {
            FilePreferenceStore store = new(_dir);
            store.Write("ctr_preferences.json", /*lang=json,strict*/ "{\"a\":1}");
            Assert.Equal(/*lang=json,strict*/ "{\"a\":1}", store.Read("ctr_preferences.json"));
        }

        [Fact]
        public void WriteCreatesTheDirectory()
        {
            FilePreferenceStore store = new(_dir);
            store.Write("ctr_preferences.json", "{}");
            Assert.True(Directory.Exists(_dir));
        }

        [Fact]
        public void EnumerateBoxSlotsFindsOnlySlotFiles()
        {
            FilePreferenceStore store = new(_dir);
            store.Write("ctrsave_slot0.json", "{}");
            store.Write("ctrsave_slot1.json", "{}");
            store.Write("ctr_preferences.json", "{}");

            Assert.Equal(
                ["ctrsave_slot0.json", "ctrsave_slot1.json"],
                store.EnumerateBoxSlots().OrderBy(n => n));
        }

        [Fact]
        public void EnumerateBoxSlotsIsEmptyWhenNothingWasWritten()
        {
            FilePreferenceStore store = new(_dir);
            Assert.Empty(store.EnumerateBoxSlots());
        }
    }
}
