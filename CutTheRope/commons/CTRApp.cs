using CutTheRope.iframework.core;
using CutTheRope.ios;
using System;

namespace CutTheRope.commons
{
    internal sealed class CTRApp : Application
    {
        public override void Dealloc()
        {
            throw new NotImplementedException();
        }

        public static void ApplicationWillTerminate(UIApplication application)
        {
            Preferences.RequestSave();
        }

        public void ApplicationDidReceiveMemoryWarning(UIApplication application)
        {
            throw new NotImplementedException();
        }

        public void ChallengeStartedWithGameConfig(NSString gameConfig)
        {
            throw new NotImplementedException();
        }

        public static void ApplicationWillResignActive(UIApplication application)
        {
            Preferences.RequestSave();
            if (root != null && !root.IsSuspended())
            {
                root.Suspend();
            }
        }

        public static void ApplicationDidBecomeActive(UIApplication application)
        {
            if (root != null && root.IsSuspended())
            {
                root.Resume();
            }
        }
    }
}
