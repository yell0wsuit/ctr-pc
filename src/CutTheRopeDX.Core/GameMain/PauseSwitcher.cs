using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Helpers;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// The button that stops and restarts time. Its face shows one of two quads and it plays a
    /// burst animation on each change of state.
    /// </summary>
    internal sealed class PauseSwitcher : GameObject
    {
        /// <summary>Face shown while time is running.</summary>
        private const int RunningQuad = 0;

        /// <summary>Face shown while time is frozen.</summary>
        private const int FrozenQuad = 1;

        /// <summary>Burst animation timeline played when time stops.</summary>
        private const int FreezeTimeline = 0;

        /// <summary>Burst animation timeline played when time restarts.</summary>
        private const int UnfreezeTimeline = 1;

        /// <summary>Builds a switcher showing its running face.</summary>
        /// <returns>The new switcher.</returns>
        public static PauseSwitcher Create()
        {
            PauseSwitcher switcher = new();
            _ = switcher.InitWithTexture(Application.GetTexture(Resources.Img.ObjPause));
            switcher.SetDrawQuad(RunningQuad);
            switcher.SetBBFromFirstQuad();
            switcher.anchor = 18;
            return switcher;
        }

        /// <summary>Switches the face to frozen and plays the stopping burst.</summary>
        public void ShowFrozen()
        {
            SetDrawQuad(FrozenQuad);
            PlayTimeline(FreezeTimeline);
        }

        /// <summary>Switches the face to running and plays the restarting burst.</summary>
        public void ShowRunning()
        {
            SetDrawQuad(RunningQuad);
            PlayTimeline(UnfreezeTimeline);
        }
    }
}
