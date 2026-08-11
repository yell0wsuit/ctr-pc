using System.Linq;

using CutTheRopeDX.Framework.Platform;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class ResidencyTrackerTests
    {
        [Fact]
        public void NewTrackerHasNothingPendingAndIsSatisfied()
        {
            ResidencyTracker tracker = new();
            Assert.Equal(0, tracker.PendingCount);
            Assert.True(tracker.AllResident);
        }

        [Fact]
        public void RequestedPathsBecomePending()
        {
            ResidencyTracker tracker = new();
            tracker.Request(["a", "b"]);
            Assert.Equal(2, tracker.PendingCount);
            Assert.False(tracker.AllResident);
        }

        [Fact]
        public void RequestingTheSamePathTwiceCountsOnce()
        {
            ResidencyTracker tracker = new();
            tracker.Request(["a", "a"]);
            Assert.Equal(1, tracker.PendingCount);
        }

        [Fact]
        public void ResidentPathsAreNotRequestedAgain()
        {
            ResidencyTracker tracker = new();
            tracker.Request(["a"]);
            _ = tracker.MarkResident("a");
            tracker.Request(["a"]);
            Assert.Equal(0, tracker.PendingCount);
            Assert.True(tracker.IsResident("a"));
        }

        [Fact]
        public void TakePendingDrainsAndDoesNotReturnTheSameWorkTwice()
        {
            ResidencyTracker tracker = new();
            tracker.Request(["a", "b"]);
            Assert.Equal(["a", "b"], tracker.TakePending().OrderBy(p => p));
            Assert.Empty(tracker.TakePending());
        }

        [Fact]
        public void TakenWorkStillCountsAsPendingUntilMarkedResident()
        {
            ResidencyTracker tracker = new();
            tracker.Request(["a"]);
            _ = tracker.TakePending();
            Assert.Equal(1, tracker.PendingCount);
            Assert.False(tracker.AllResident);

            _ = tracker.MarkResident("a");
            Assert.Equal(0, tracker.PendingCount);
            Assert.True(tracker.AllResident);
        }

        [Fact]
        public void MarkResidentReportsWhetherItChangedAnything()
        {
            ResidencyTracker tracker = new();
            tracker.Request(["a"]);
            Assert.True(tracker.MarkResident("a"));
            Assert.False(tracker.MarkResident("a"));
        }
    }
}
