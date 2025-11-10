using CutTheRope.iframework.helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CutTheRope.ios
{
    internal class NSTimer : NSObject
    {
        private static void Init()
        {
            Timers = new List<Entry>();
            dd = new DelayedDispatcher();
            is_init = true;
        }

        public static void registerDelayedObjectCall(DelayedDispatcher.DispatchFunc f, NSObject p, double interval)
        {
            if (!is_init)
            {
                Init();
            }
            dd.callObjectSelectorParamafterDelay(f, p, interval);
        }

        public static int schedule(DelayedDispatcher.DispatchFunc f, NSObject p, float interval)
        {
            if (!is_init)
            {
                Init();
            }
            Entry entry = new();
            entry.f = f;
            entry.p = p;
            entry.fireTime = 0f;
            entry.delay = interval;
            Timers.Add(entry);
            return Timers.Count() - 1;
        }

        public static void fireTimers(float delta)
        {
            if (!is_init)
            {
                Init();
            }
            dd.update(delta);
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

        public static void stopTimer(int Number)
        {
            Timers.RemoveAt(Number);
        }

        private static List<Entry> Timers;

        private static DelayedDispatcher dd;

        private static bool is_init;

        private class Entry
        {
            public DelayedDispatcher.DispatchFunc f;

            public NSObject p;

            public float fireTime;

            public float delay;
        }
    }
}
