using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Platform;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers what a preference save writes, and what it does when the store refuses.
    /// </summary>
    /// <remarks>
    /// <see cref="Preferences"/> is static and the store behind it is a process-wide service,
    /// so these swap in their own store and put the previous one back. Tests in other classes
    /// run alongside these and change preferences of their own, which a save here will pick up
    /// and write. So every assertion is scoped to one box slot no other test uses, and the
    /// store fails only writes of that slot - concurrent traffic then neither perturbs these
    /// results nor is perturbed by them.
    /// <para>
    /// The retry backoff window itself is deliberately not covered. Its attempt counter is
    /// static too, and a save taken by another test resets or exhausts it, so any assertion
    /// about when a retry is allowed races with the rest of the suite. What is covered is the
    /// part that matters to a player: a failed write is retried rather than dropped, and
    /// giving up leaves the change owed rather than losing it.
    /// </para>
    /// </remarks>
    public sealed class PreferenceSaveTests
    {
        /// <summary>A slot high enough that no pack in the game occupies it.</summary>
        private const int Slot = 17;

        private const string SlotBlob = "ctrsave_slot17.json";

        /// <summary>Records what it is asked to write, and can refuse one named blob.</summary>
        private sealed class RecordingStore : IPreferenceStore
        {
            private readonly Dictionary<string, string> _blobs = [];

            public List<string> Writes { get; } = [];

            /// <summary>How many further writes of <see cref="FailBlob"/> will be refused.</summary>
            public int FailuresRemaining { get; set; }

            public string FailBlob { get; set; }

            public string Read(string name)
            {
                return _blobs.TryGetValue(name, out string value) ? value : null;
            }

            public void Write(string name, string contents)
            {
                if (FailuresRemaining > 0 && string.Equals(name, FailBlob, StringComparison.Ordinal))
                {
                    FailuresRemaining--;
                    throw new IOException("the store is refusing writes");
                }

                Writes.Add(name);
                _blobs[name] = contents;
            }

            public IEnumerable<string> EnumerateBoxSlots()
            {
                return _blobs.Keys.Where(
                    name => name.StartsWith("ctrsave_slot", StringComparison.Ordinal));
            }
        }

        private static void WithStore(Action<RecordingStore> body)
        {
            IPreferenceStore previous = PlatformServices.Preferences;
            RecordingStore store = new();
            PlatformServices.Preferences = store;
            try
            {
                body(store);
            }
            finally
            {
                PlatformServices.Preferences = previous;
            }
        }

        private static void Save(RecordingStore store)
        {
            // Requested immediately before it is taken, so a save another test happens to take
            // in parallel cannot consume the one under assertion.
            Preferences.RequestSave();
            Preferences.Update(force: true);
            _ = store;
        }

        [Fact]
        public void AChangedBlobIsWritten()
        {
            WithStore(store =>
            {
                Preferences.SetBoxIntForKey(Slot, 7, "STARS_TEST");
                Save(store);

                Assert.Contains(SlotBlob, store.Writes);
            });
        }

        [Fact]
        public void AnUnchangedBlobIsNotWrittenAgain()
        {
            WithStore(store =>
            {
                Preferences.SetBoxIntForKey(Slot, 7, "STARS_TEST");
                Save(store);
                Assert.Contains(SlotBlob, store.Writes);

                store.Writes.Clear();
                Save(store);

                // The whole point of the change: a save no longer rewrites every blob.
                Assert.DoesNotContain(SlotBlob, store.Writes);
            });
        }

        [Fact]
        public void RemovingAKeyMarksTheBlobForWriting()
        {
            WithStore(store =>
            {
                Preferences.SetBoxIntForKey(Slot, 5, "STARS_TEST");
                Save(store);
                store.Writes.Clear();

                Preferences.RemoveBoxKey(Slot, "STARS_TEST");
                Save(store);

                // Taking a key away is a change. Skipping the blob here would leave the
                // removed key on disk to be read back on the next launch, which is how a
                // reset can appear to work and then hand the old progress back.
                Assert.Contains(SlotBlob, store.Writes);
            });
        }

        [Fact]
        public void RemovingAKeyThatWasNotThereChangesNothing()
        {
            WithStore(store =>
            {
                Preferences.SetBoxIntForKey(Slot, 5, "STARS_TEST");
                Save(store);
                store.Writes.Clear();

                Preferences.RemoveBoxKey(Slot, "STARS_ABSENT");
                Save(store);

                Assert.DoesNotContain(SlotBlob, store.Writes);
            });
        }

        [Fact]
        public void AWriteThatFailsOnceIsRetriedRatherThanLost()
        {
            WithStore(store =>
            {
                store.FailBlob = SlotBlob;
                store.FailuresRemaining = 1;
                Preferences.SetBoxIntForKey(Slot, 11, "STARS_TEST");

                Save(store);
                Assert.DoesNotContain(SlotBlob, store.Writes);

                // The old behaviour cleared the request on failure and the value never landed.
                Preferences.Update(force: true);
                Assert.Contains(SlotBlob, store.Writes);
            });
        }

        [Fact]
        public void GivingUpPausesTheSaveRatherThanLosingTheChange()
        {
            WithStore(store =>
            {
                store.FailBlob = SlotBlob;
                store.FailuresRemaining = 100;
                Preferences.SetBoxIntForKey(Slot, 99, "STARS_TEST");

                for (int attempt = 0; attempt < 12; attempt++)
                {
                    Preferences.RequestSave();
                    Preferences.Update(force: true);
                }

                Assert.DoesNotContain(SlotBlob, store.Writes);

                // The change is still owed, so the next working save writes it.
                store.FailuresRemaining = 0;
                Save(store);

                Assert.Contains(SlotBlob, store.Writes);
                Assert.Equal(99, Preferences.GetBoxIntForKey(Slot, "STARS_TEST"));
            });
        }

    }
}
