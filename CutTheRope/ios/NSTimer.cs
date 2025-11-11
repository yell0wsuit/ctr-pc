using CutTheRope.iframework.helpers;
using System.Collections.Generic;

namespace CutTheRope.ios
{
    internal sealed class NSTimer : NSObject
    {
        private static new void Init()
        {
            Timers = [];
            dd = new DelayedDispatcher();
            is_init = true;
        }

        public static void RegisterDelayedObjectCall(DelayedDispatcher.DispatchFunc f, NSObject p, double interval)
        {
            if (!is_init)
            {
                Init();
            }
            dd.CallObjectSelectorParamafterDelay(f, p, interval);
        }

        public static int Schedule(DelayedDispatcher.DispatchFunc f, NSObject p, float interval)
        {
            if (!is_init)
            {
                Init();
            }
            Entry entry = new()
            {
                f = f,
                p = p,
                fireTime = 0f,
                delay = interval
            };
            Timers.Add(entry);
            return Timers.Count - 1;
        }

        public static void FireTimers(float delta)
        {
            if (!is_init)
            {
                Init();
            }
            dd.Update(delta);
            for (int i = 0; i < Timers.Count; i++)
            {
                Entry entry = Timers[i];
                entry.fireTime += delta;
                if (entry.fireTime >= entry.delay)
                {
                    entry.f(entry.p);
                    entry.fireTime -= entry.delay;
                }
            }
        }

        public static void StopTimer(int Number)
        {
            Timers.RemoveAt(Number);
        }

        private static List<Entry> Timers;

        private static DelayedDispatcher dd;

        private static bool is_init;

        private sealed class Entry
        {
            public DelayedDispatcher.DispatchFunc f;

            public NSObject p;

            public float fireTime;

            public float delay;
        }
    }
}
