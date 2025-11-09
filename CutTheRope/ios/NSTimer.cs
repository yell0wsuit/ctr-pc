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
            NSTimer.Timers = new List<NSTimer.Entry>();
            NSTimer.dd = new DelayedDispatcher();
            NSTimer.is_init = true;
        }

        public static void registerDelayedObjectCall(DelayedDispatcher.DispatchFunc f, NSObject p, double interval)
        {
            if (!NSTimer.is_init)
            {
                NSTimer.Init();
            }
            NSTimer.dd.callObjectSelectorParamafterDelay(f, p, interval);
        }

        public static int schedule(DelayedDispatcher.DispatchFunc f, NSObject p, float interval)
        {
            if (!NSTimer.is_init)
            {
                NSTimer.Init();
            }
            NSTimer.Entry entry = new();
            entry.f = f;
            entry.p = p;
            entry.fireTime = 0f;
            entry.delay = interval;
            NSTimer.Timers.Add(entry);
            return NSTimer.Timers.Count<NSTimer.Entry>() - 1;
        }

        public static void fireTimers(float delta)
        {
            if (!NSTimer.is_init)
            {
                NSTimer.Init();
            }
            NSTimer.dd.update(delta);
            for (int i = 0; i < NSTimer.Timers.Count; i++)
            {
                NSTimer.Entry entry = NSTimer.Timers[i];
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
            NSTimer.Timers.RemoveAt(Number);
        }

        private static List<NSTimer.Entry> Timers;

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
