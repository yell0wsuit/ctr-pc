using CutTheRope.iframework.helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CutTheRope.ios
{
    // Token: 0x0200001A RID: 26
    internal class NSTimer : NSObject
    {
        // Token: 0x060000F4 RID: 244 RVA: 0x00005699 File Offset: 0x00003899
        private static void Init()
        {
            NSTimer.Timers = new List<NSTimer.Entry>();
            NSTimer.dd = new DelayedDispatcher();
            NSTimer.is_init = true;
        }

        // Token: 0x060000F5 RID: 245 RVA: 0x000056B5 File Offset: 0x000038B5
        public static void registerDelayedObjectCall(DelayedDispatcher.DispatchFunc f, NSObject p, double interval)
        {
            if (!NSTimer.is_init)
            {
                NSTimer.Init();
            }
            NSTimer.dd.callObjectSelectorParamafterDelay(f, p, interval);
        }

        // Token: 0x060000F6 RID: 246 RVA: 0x000056D0 File Offset: 0x000038D0
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

        // Token: 0x060000F7 RID: 247 RVA: 0x00005728 File Offset: 0x00003928
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

        // Token: 0x060000F8 RID: 248 RVA: 0x000057AD File Offset: 0x000039AD
        public static void stopTimer(int Number)
        {
            NSTimer.Timers.RemoveAt(Number);
        }

        // Token: 0x04000093 RID: 147
        private static List<NSTimer.Entry> Timers;

        // Token: 0x04000094 RID: 148
        private static DelayedDispatcher dd;

        // Token: 0x04000095 RID: 149
        private static bool is_init;

        // Token: 0x020000A6 RID: 166
        private class Entry
        {
            // Token: 0x04000888 RID: 2184
            public DelayedDispatcher.DispatchFunc f;

            // Token: 0x04000889 RID: 2185
            public NSObject p;

            // Token: 0x0400088A RID: 2186
            public float fireTime;

            // Token: 0x0400088B RID: 2187
            public float delay;
        }
    }
}
