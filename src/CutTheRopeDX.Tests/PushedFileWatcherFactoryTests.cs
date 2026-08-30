using System;

using CutTheRopeDX.Framework.Platform;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>Tests for <see cref="PushedFileWatcherFactory"/>.</summary>
    public sealed class PushedFileWatcherFactoryTests
    {
        /// <summary>A single watch fires when NotifyChanged is called for its path.</summary>
        [Fact]
        public void SingleWatchFires()
        {
            PushedFileWatcherFactory factory = new();
            int invocations = 0;
            _ = factory.Watch("dir", "file.txt", () => invocations++);

            factory.NotifyChanged("dir", "file.txt");

            Assert.Equal(1, invocations);
        }

        /// <summary>Two callbacks on the same path both fire when NotifyChanged is called.</summary>
        [Fact]
        public void MultipleCallbacksOnSamePathBothFire()
        {
            PushedFileWatcherFactory factory = new();
            int invocations1 = 0;
            int invocations2 = 0;

            _ = factory.Watch("dir", "file.txt", () => invocations1++);
            _ = factory.Watch("dir", "file.txt", () => invocations2++);

            factory.NotifyChanged("dir", "file.txt");

            Assert.Equal(1, invocations1);
            Assert.Equal(1, invocations2);
        }

        /// <summary>A watch on a different path does not fire when NotifyChanged is called for another path.</summary>
        [Fact]
        public void DifferentPathDoesNotFire()
        {
            PushedFileWatcherFactory factory = new();
            int invocations = 0;

            _ = factory.Watch("dir", "file1.txt", () => invocations++);

            factory.NotifyChanged("dir", "file2.txt");

            Assert.Equal(0, invocations);
        }

        /// <summary>Disposing one registration leaves the other still firing.</summary>
        [Fact]
        public void DisposingOneRegistrationPreservesOthers()
        {
            PushedFileWatcherFactory factory = new();
            int invocations1 = 0;
            int invocations2 = 0;

            IDisposable watch1 = factory.Watch("dir", "file.txt", () => invocations1++);
            IDisposable watch2 = factory.Watch("dir", "file.txt", () => invocations2++);

            watch1.Dispose();
            factory.NotifyChanged("dir", "file.txt");

            Assert.Equal(0, invocations1);
            Assert.Equal(1, invocations2);
        }

        /// <summary>Disposing a registration from inside its own callback neither throws nor prevents other callbacks from firing.</summary>
        [Fact]
        public void DisposingFromInsideCallbackIsIdempotent()
        {
            PushedFileWatcherFactory factory = new();
            IDisposable watch1 = null;
            int invocations2 = 0;
            bool threwException = false;

            watch1 = factory.Watch("dir", "file.txt", () =>
            {
                try
                {
                    watch1.Dispose();
                }
                catch
                {
                    threwException = true;
                    throw;
                }
            });
            _ = factory.Watch("dir", "file.txt", () => invocations2++);

            factory.NotifyChanged("dir", "file.txt");

            Assert.False(threwException);
            Assert.Equal(1, invocations2);
        }

        /// <summary>Watch returns null when directory is empty.</summary>
        [Fact]
        public void WatchReturnsNullForEmptyDirectory()
        {
            PushedFileWatcherFactory factory = new();
            IDisposable result = factory.Watch("", "file.txt", () => { });
            Assert.Null(result);
        }

        /// <summary>Watch returns null when file name is empty.</summary>
        [Fact]
        public void WatchReturnsNullForEmptyFileName()
        {
            PushedFileWatcherFactory factory = new();
            IDisposable result = factory.Watch("dir", "", () => { });
            Assert.Null(result);
        }

        /// <summary>Watch returns null when callback is null.</summary>
        [Fact]
        public void WatchReturnsNullForNullCallback()
        {
            PushedFileWatcherFactory factory = new();
            IDisposable result = factory.Watch("dir", "file.txt", null);
            Assert.Null(result);
        }

        /// <summary>NotifyChanged on a path nothing watches does not throw.</summary>
        [Fact]
        public void NotifyChangedOnUnwatchedPathDoesNotThrow()
        {
            PushedFileWatcherFactory factory = new();
            // Should not throw
            factory.NotifyChanged("dir", "file.txt");
        }

        /// <summary>Disposing a registration twice is idempotent.</summary>
        [Fact]
        public void DisposingRegistrationTwiceIsIdempotent()
        {
            PushedFileWatcherFactory factory = new();
            IDisposable watch = factory.Watch("dir", "file.txt", () => { });

            watch.Dispose();
            // Should not throw
            watch.Dispose();
        }

        /// <summary>After all registrations for a path are disposed, NotifyChanged on that path does not throw.</summary>
        [Fact]
        public void NotifyChangedAfterAllDisposedDoesNotThrow()
        {
            PushedFileWatcherFactory factory = new();
            IDisposable watch = factory.Watch("dir", "file.txt", () => { });

            watch.Dispose();

            // Should not throw
            factory.NotifyChanged("dir", "file.txt");
        }

        /// <summary>Each notification is independent; callbacks do not accumulate across calls.</summary>
        [Fact]
        public void EachNotificationIsIndependent()
        {
            PushedFileWatcherFactory factory = new();
            int invocations = 0;

            _ = factory.Watch("dir", "file.txt", () => invocations++);

            factory.NotifyChanged("dir", "file.txt");
            Assert.Equal(1, invocations);

            factory.NotifyChanged("dir", "file.txt");
            Assert.Equal(2, invocations);
        }
    }
}
