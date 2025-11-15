using System;

using CutTheRope.iframework.core;

namespace CutTheRope.commons
{
    internal sealed class CTRApp : Application
    {
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        public static void ApplicationWillTerminate()
        {
            Preferences.RequestSave();
        }

        public void ApplicationDidReceiveMemoryWarning()
        {
            throw new NotImplementedException();
        }

        public void ChallengeStartedWithGameConfig(string gameConfig)
        {
            throw new NotImplementedException();
        }

        public static void ApplicationWillResignActive()
        {
            Preferences.RequestSave();
            if (root != null && !root.IsSuspended())
            {
                root.Suspend();
            }
        }

        public static void ApplicationDidBecomeActive()
        {
            if (root != null && root.IsSuspended())
            {
                root.Resume();
            }
        }
    }
}
