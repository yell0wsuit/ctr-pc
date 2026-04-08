using System;

using CutTheRope.Framework.Core;

namespace CutTheRope.Commons
{
    /// <summary>
    /// Application entry point for the shared Cut the Rope runtime.
    /// </summary>
    internal sealed class CTRApp : Application
    {
        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        /// <summary>
        /// Persists application preferences before the process terminates.
        /// </summary>
        /// <remarks>This method is unused. Leftover from mobile version.</remarks>
        public static void ApplicationWillTerminate()
        {
            Preferences.RequestSave();
        }

        /// <summary>
        /// Handles platform memory-pressure notifications.
        /// </summary>
        /// <remarks>This method is unused. Leftover from mobile version.</remarks>
        public void ApplicationDidReceiveMemoryWarning()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Starts a challenge session using the provided game configuration payload.
        /// </summary>
        /// <param name="gameConfig">The serialized game configuration for the challenge.</param>
        /// <remarks>This method is unused.</remarks>
        public void ChallengeStartedWithGameConfig(string gameConfig)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Saves preferences and suspends the root controller when the application loses focus.
        /// </summary>
        public static void ApplicationWillResignActive()
        {
            Preferences.RequestSave();
            if (root != null && !root.IsSuspended())
            {
                root.Suspend();
            }
        }

        /// <summary>
        /// Resumes the root controller when the application becomes active again.
        /// </summary>
        public static void ApplicationDidBecomeActive()
        {
            if (root != null && root.IsSuspended())
            {
                root.Resume();
            }
        }
    }
}
