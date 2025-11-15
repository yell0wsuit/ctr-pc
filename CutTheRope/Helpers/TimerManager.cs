using System;
using System.Collections.Generic;
using System.Threading;

using CutTheRope.iframework;
using CutTheRope.iframework.helpers;

namespace CutTheRope.Helpers
{
    internal static class TimerManager
    {
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

        public static void StopTimer(int timerId)
        {
            if (timerId < 0)
            {
                return;
            }

            EnsureInitialized();
            _ = timers.Remove(timerId);
        }

        public static void RegisterDelayedObjectCall(DelayedDispatcher.DispatchFunc callback, FrameworkTypes parameter, double interval)
        {
            ArgumentNullException.ThrowIfNull(callback);

            ArgumentOutOfRangeException.ThrowIfLessThan(interval, 0.0);

            EnsureInitialized();
            delayedDispatcher.CallObjectSelectorParamafterDelay(callback, parameter, interval);
        }

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

        private static readonly Dictionary<int, TimerEntry> timers = [];
        private static readonly List<int> updateKeys = [];
        private static readonly Lock initLock = new();
        private static DelayedDispatcher delayedDispatcher;
        private static bool initialized;
        private static int nextTimerId;

        private sealed class TimerEntry(DelayedDispatcher.DispatchFunc callback, FrameworkTypes parameter, float delay)
        {
            public float Delay { get; } = delay;

            public float Accumulator { get; set; }

            public DelayedDispatcher.DispatchFunc Callback { get; } = callback;

            public FrameworkTypes Parameter { get; } = parameter;

            public void Invoke()
            {
                Callback(Parameter);
            }
        }
    }
}
