using System;
using System.Collections.Generic;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.Helpers;

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

        private readonly List<Image> animationParts = [];
        private FlashXmlStageRoot animationRoot;

        /// <summary>Builds a switcher showing its running face.</summary>
        /// <returns>The new switcher.</returns>
        public static PauseSwitcher Create()
        {
            PauseSwitcher switcher = new();
            _ = switcher.InitWithTexture(Application.GetTexture(Resources.Img.ObjPause));
            switcher.SetDrawQuad(RunningQuad);
            switcher.anchor = 18;
            switcher.AttachAnimation();
            return switcher;
        }

        /// <summary>Switches the face to frozen and plays the stopping burst.</summary>
        public void ShowFrozen()
        {
            SetDrawQuad(FrozenQuad);
            PlayAnimation(FreezeTimeline);
        }

        /// <summary>Switches the face to running and plays the restarting burst.</summary>
        public void ShowRunning()
        {
            SetDrawQuad(RunningQuad);
            PlayAnimation(UnfreezeTimeline);
        }

        /// <summary>Builds the centered Flash XML burst child used by both button states.</summary>
        private void AttachAnimation()
        {
            FlashXmlAnimationDefinition definition = FlashXmlImporter.ParseFile(
                ContentPaths.GetAnimationXmlAbsolutePath("obj_pause_ani.xml"));
            animationRoot = new FlashXmlStageRoot();
            _ = animationRoot.InitWithTexture(Application.GetTexture(Resources.Img.ObjPause));
            animationRoot.SetDrawQuad(0);
            animationRoot.color = RGBAColor.transparentRGBA;
            animationRoot.passColorToChilds = false;
            animationRoot.width = (int)MathF.Round(definition.StageWidth);
            animationRoot.height = (int)MathF.Round(definition.StageHeight);
            animationRoot.anchor = 9;
            animationRoot.parentAnchor = 18;
            animationRoot.blendingMode = 2;
            animationRoot.visible = false;
            FlashXmlTargetAnimationBackend.BuildParts(definition, animationRoot, animationParts, -1, -1);
            FlashXmlTargetAnimationBackend.BuildRootTimelines(definition, animationRoot, -1, -1);
            _ = AddChildwithID(animationRoot, 0);
        }

        /// <summary>Plays one exported button burst timeline.</summary>
        /// <param name="timelineId">Timeline to play.</param>
        private void PlayAnimation(int timelineId)
        {
            animationRoot.visible = true;
            FlashXmlTargetAnimationBackend.PlayTimeline(animationParts, timelineId);
            FlashXmlTargetAnimationBackend.PlayRootTimeline(animationRoot, timelineId);
        }
    }
}
