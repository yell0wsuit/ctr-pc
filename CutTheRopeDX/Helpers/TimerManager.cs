using System;
using System.Collections.Generic;
using System.Threading;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Helpers;

namespace CutTheRopeDX.Helpers
{
    /// <summary>
    /// Manages recurring timers and one-shot delayed callbacks, updated each frame.
    /// </summary>
    internal static class TimerManager
    {
        /// <summary>
        /// Schedules a recurring timer that fires at the given interval.
        /// </summary>
        /// <param name="callback">Function to invoke each time the timer fires.</param>
        /// <param name="parameter">Parameter passed to the callback.</param>
        /// <param name="interval">Time in seconds between invocations.</param>
        /// <returns>A timer identifier that can be passed to <see cref="StopTimer"/> to cancel it.</returns>
        public static int Schedule(DelayedDispatcher.DispatchFunc callback, FrameworkTypes parameter, float interval)
        {
            ArgumentNullException.ThrowIfNull(callback);

            ArgumentOutOfRangeException.ThrowIfLessThan(interval, 0f);

            EnsureInitialized();

            TimerEntry entry = new(callback, parameter, interval);
            int id = nextTimerId++;
            timers.Add(id, entry);
            return id;
        }

        /// <summary>
        /// Stops and removes a previously scheduled recurring timer.
        /// </summary>
        /// <param name="timerId">Timer identifier returned by <see cref="Schedule"/>. Negative values are ignored.</param>
        public static void StopTimer(int timerId)
        {
            if (timerId < 0)
            {
                return;
            }

            EnsureInitialized();
            _ = timers.Remove(timerId);
        }

        /// <summary>
        /// Registers a one-shot delayed callback that fires once after the specified interval.
        /// </summary>
        /// <param name="callback">Function to invoke after the delay.</param>
        /// <param name="parameter">Parameter passed to the callback.</param>
        /// <param name="interval">Delay in seconds before the callback fires.</param>
        public static void RegisterDelayedObjectCall(DelayedDispatcher.DispatchFunc callback, FrameworkTypes parameter, float interval)
        {
            ArgumentNullException.ThrowIfNull(callback);

            ArgumentOutOfRangeException.ThrowIfLessThan(interval, 0);

            EnsureInitialized();
            delayedDispatcher.CallObjectSelectorParamafterDelay(callback, parameter, interval);
        }

        /// <summary>
        /// Advances all recurring timers and delayed callbacks by the elapsed frame time.
        /// </summary>
        /// <param name="delta">Elapsed time in seconds since the last update.</param>
        public static void Update(float delta)
        {
            EnsureInitialized();

            if (timers.Count == 0)
            {
                delayedDispatcher.Update(delta);
                return;
            }

            updateKeys.Clear();
            updateKeys.AddRange(timers.Keys);

            foreach (int key in updateKeys)
            {
                if (!timers.TryGetValue(key, out TimerEntry entry))
                {
                    continue;
                }

                entry.Accumulator += delta;
                if (entry.Accumulator >= entry.Delay)
                {
                    entry.Accumulator -= entry.Delay;
                    entry.Invoke();
                }
            }

            delayedDispatcher.Update(delta);
        }

        /// <summary>
        /// Lazily initializes the delayed dispatcher in a thread-safe manner.
        /// </summary>
        private static void EnsureInitialized()
        {
            if (initialized)
            {
                return;
            }

            lock (initLock)
            {
                if (initialized)
                {
                    return;
                }

                delayedDispatcher = new DelayedDispatcher();
                initialized = true;
            }
        }

        /// <summary>
        /// Active recurring timers keyed by their identifier.
        /// </summary>
        private static readonly Dictionary<int, TimerEntry> timers = [];

        /// <summary>
        /// Temporary list of timer keys used during update iteration to allow safe removal.
        /// </summary>
        private static readonly List<int> updateKeys = [];

        /// <summary>
        /// Lock used for thread-safe lazy initialization.
        /// </summary>
        private static readonly Lock initLock = new();

        /// <summary>
        /// Dispatcher that handles one-shot delayed callbacks.
        /// </summary>
        private static DelayedDispatcher delayedDispatcher;

        /// <summary>
        /// Whether the manager has been initialized.
        /// </summary>
        private static bool initialized;

        /// <summary>
        /// Next timer identifier to assign.
        /// </summary>
        private static int nextTimerId;

        /// <summary>
        /// Holds state for a single recurring timer.
        /// </summary>
        /// <param name="callback">Function to invoke when the timer fires.</param>
        /// <param name="parameter">Parameter passed to the callback.</param>
        /// <param name="delay">Time in seconds between invocations.</param>
        private sealed class TimerEntry(DelayedDispatcher.DispatchFunc callback, FrameworkTypes parameter, float delay)
        {
            /// <summary>
            /// Time in seconds between invocations.
            /// </summary>
            public float Delay { get; } = delay;

            /// <summary>
            /// Accumulated time since the last invocation.
            /// </summary>
            public float Accumulator { get; set; }

            /// <summary>
            /// Function to invoke when the timer fires.
            /// </summary>
            public DelayedDispatcher.DispatchFunc Callback { get; } = callback;

            /// <summary>
            /// Parameter passed to the callback on each invocation.
            /// </summary>
            public FrameworkTypes Parameter { get; } = parameter;

            /// <summary>
            /// Executes the callback with the stored parameter.
            /// </summary>
            public void Invoke()
            {
                Callback(Parameter);
            }
        }
    }
}
