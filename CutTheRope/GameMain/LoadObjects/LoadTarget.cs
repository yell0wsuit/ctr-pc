using System.Collections.Generic;
using System.Xml.Linq;

using CutTheRope.Framework.Core;
using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Handles loading Om Nom from XML level data
    /// Om Nom is the objective the candy must reach to complete the level
    /// </summary>
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Loads Om Nom from XML node data
        /// Sets up Om Nom animations, blink animation, and greeting if needed
        /// </summary>
        private void LoadTarget(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            int pack = ((CTRRootController)Application.SharedRootController()).GetPack();
            string supportResourceName = PackConfig.GetSupportResourceName(pack);

            // Clamp quad index to valid range; fall back to first quad if pack index exceeds available quads
            CTRTexture2D supportTexture = Application.GetTexture(supportResourceName);
            int quadIndex = (pack >= 0 && pack < supportTexture.quadRects.Length) ? pack : 0;

            support = Image.Image_createWithResIDQuad(supportResourceName, quadIndex);
            support.DoRestoreCutTransparency();
            support.anchor = 18;

            target = CharAnimations.CharAnimations_createWithResID(Resources.Img.CharAnimations);
            target.DoRestoreCutTransparency();
            target.passColorToChilds = false;
            string xAttribute = xmlNode.Attribute("x")?.Value ?? string.Empty;
            target.x = support.x = (ParseIntOrZero(xAttribute) * scale) + offsetX + mapOffsetX;
            string yAttribute = xmlNode.Attribute("y")?.Value ?? string.Empty;
            target.y = support.y = (ParseIntOrZero(yAttribute) * scale) + offsetY + mapOffsetY;

            target.AddImage(Resources.Img.CharAnimations2);
            target.AddImage(Resources.Img.CharAnimations3);
            if (nightLevel)
            {
                target.AddImage(Resources.Img.CharAnimationsSleeping);
            }
            if (SpecialEvents.IsXmas)
            {
                target.AddImage(Resources.Img.CharGreetingXmas);
                target.AddImage(Resources.Img.CharIdleXmas);
            }
            target.bb = MakeRectangle(264f, 350f, 108f, 2f);

            // Setup main animation
            target.AddAnimationWithIDDelayLoopFirstLast(0, 0.05f, Timeline.LoopType.TIMELINE_REPLAY, 0, 18);
            target.AddAnimationWithIDDelayLoopFirstLast(1, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 43, 67);

            // Setup complex looping animation sequence
            int loopStartFrame = 68;
            target.AddAnimationWithIDDelayLoopCountSequence(2, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 32, loopStartFrame,
            [
                loopStartFrame + 1,
                loopStartFrame + 2,
                loopStartFrame + 3,
                loopStartFrame + 4,
                loopStartFrame + 5,
                loopStartFrame + 6,
                loopStartFrame + 7,
                loopStartFrame + 8,
                loopStartFrame + 9,
                loopStartFrame + 10,
                loopStartFrame + 11,
                loopStartFrame + 12,
                loopStartFrame + 13,
                loopStartFrame + 14,
                loopStartFrame + 15,
                loopStartFrame,
                loopStartFrame + 1,
                loopStartFrame + 2,
                loopStartFrame + 3,
                loopStartFrame + 4,
                loopStartFrame + 5,
                loopStartFrame + 6,
                loopStartFrame + 7,
                loopStartFrame + 8,
                loopStartFrame + 9,
                loopStartFrame + 10,
                loopStartFrame + 11,
                loopStartFrame + 12,
                loopStartFrame + 13,
                loopStartFrame + 14,
                loopStartFrame + 15
            ]);

            if (SpecialEvents.IsXmas)
            {
                target.AddAnimationWithIDDelayLoopFirstLast(Resources.Img.CharGreetingXmas, 11, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP,
                    0,
                    33);
                target.AddAnimationWithIDDelayLoopFirstLast(Resources.Img.CharIdleXmas, 12, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP,
                    0,
                    30);
                target.AddAnimationWithIDDelayLoopFirstLast(Resources.Img.CharIdleXmas, 13, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP,
                    31,
                    61);
            }

            target.AddAnimationWithIDDelayLoopFirstLast(7, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 19, 27);
            target.AddAnimationWithIDDelayLoopFirstLast(8, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 28, 31);
            target.AddAnimationWithIDDelayLoopFirstLast(9, 0.05f, Timeline.LoopType.TIMELINE_REPLAY, 32, 40);
            target.AddAnimationWithIDDelayLoopFirstLast(6, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 28, 31);
            target.AddAnimationWithIDDelayLoopFirstLast(Resources.Img.CharAnimations2, 10, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 47, 76);
            target.AddAnimationWithIDDelayLoopFirstLast(Resources.Img.CharAnimations2, 3, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 0, 19);
            target.AddAnimationWithIDDelayLoopFirstLast(Resources.Img.CharAnimations2, 4, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 20, 46);
            target.AddAnimationWithIDDelayLoopFirstLast(Resources.Img.CharAnimations3, 5, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 0, 12);

            if (nightLevel)
            {
                target.AddAnimationWithIDDelayLoopFirstLast(Resources.Img.CharAnimationsSleeping, CharAnimationSleeping, SleepAnimFrameDelay, Timeline.LoopType.TIMELINE_NO_LOOP, SleepAnimStart, SleepAnimEnd);

                List<int> zzzFrames = [];
                for (int frame = SleepZzzStart; frame <= SleepZzzEnd; frame++)
                {
                    zzzFrames.Add(frame);
                }
                List<int> zzzHold = [];
                for (int i = 0; i < 15; i++)
                {
                    zzzHold.Add(SleepZzzStart);
                }
                List<int> zzzPrimarySequence = [];
                zzzPrimarySequence.AddRange(zzzFrames);
                zzzPrimarySequence.AddRange(zzzHold);
                List<int> zzzSecondarySequence = [];
                zzzSecondarySequence.AddRange(zzzHold);
                zzzSecondarySequence.AddRange(zzzFrames);

                List<int> zzzPrimaryTail = zzzPrimarySequence.Count > 1 ? zzzPrimarySequence.GetRange(1, zzzPrimarySequence.Count - 1) : [];
                List<int> zzzSecondaryTail = zzzSecondarySequence.Count > 1 ? zzzSecondarySequence.GetRange(1, zzzSecondarySequence.Count - 1) : [];

                sleepAnimPrimary = Animation.Animation_createWithResID(Resources.Img.CharAnimationsSleeping);
                sleepAnimPrimary.anchor = sleepAnimPrimary.parentAnchor = 18;
                sleepAnimPrimary.DoRestoreCutTransparency();
                sleepAnimPrimary.AddAnimationWithIDDelayLoopCountSequence(0, 1f / 30f, Timeline.LoopType.TIMELINE_REPLAY, zzzPrimarySequence.Count, zzzPrimarySequence[0], zzzPrimaryTail);
                sleepAnimPrimary.PlayTimeline(0);
                sleepAnimPrimary.visible = false;

                sleepAnimSecondary = Animation.Animation_createWithResID(Resources.Img.CharAnimationsSleeping);
                sleepAnimSecondary.anchor = sleepAnimSecondary.parentAnchor = 18;
                sleepAnimSecondary.DoRestoreCutTransparency();
                sleepAnimSecondary.AddAnimationWithIDDelayLoopCountSequence(0, 1f / 30f, Timeline.LoopType.TIMELINE_REPLAY, zzzSecondarySequence.Count, zzzSecondarySequence[0], zzzSecondaryTail);
                sleepAnimSecondary.PlayTimeline(0);
                sleepAnimSecondary.visible = false;
            }
            else
            {
                sleepAnimPrimary = null;
                sleepAnimSecondary = null;
            }

            // Setup animation transitions
            target.SwitchToAnimationatEndOfAnimationDelay(9, 6, 0.05f);
            target.SwitchToAnimationatEndOfAnimationDelay(Resources.Img.CharAnimations2, 4, Resources.Img.CharAnimations, 8, 0.05f);
            target.SwitchToAnimationatEndOfAnimationDelay(Resources.Img.CharAnimations, 0, Resources.Img.CharAnimations2, 10, 0.05f);
            target.SwitchToAnimationatEndOfAnimationDelay(Resources.Img.CharAnimations, 0, Resources.Img.CharAnimations, 1, 0.05f);
            target.SwitchToAnimationatEndOfAnimationDelay(Resources.Img.CharAnimations, 0, Resources.Img.CharAnimations, 2, 0.05f);
            target.SwitchToAnimationatEndOfAnimationDelay(Resources.Img.CharAnimations, 0, Resources.Img.CharAnimations2, 3, 0.05f);
            target.SwitchToAnimationatEndOfAnimationDelay(Resources.Img.CharAnimations, 0, Resources.Img.CharAnimations2, 4, 0.05f);

            if (SpecialEvents.IsXmas)
            {
                target.SwitchToAnimationatEndOfAnimationDelay(Resources.Img.CharAnimations, 0, Resources.Img.CharGreetingXmas, 11, 0.05f);
                target.SwitchToAnimationatEndOfAnimationDelay(Resources.Img.CharAnimations, 0, Resources.Img.CharIdleXmas, 12, 0.05f);
                target.SwitchToAnimationatEndOfAnimationDelay(Resources.Img.CharAnimations, 0, Resources.Img.CharIdleXmas, 13, 0.05f);
            }

            // Show greeting if needed (skip for night levels)
            if (CTRRootController.IsShowGreeting())
            {
                if (!nightLevel)
                {
                    dd.CallObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(Selector_showGreeting), null, 1.3f);
                }
                CTRRootController.SetShowGreeting(false);
            }

            target.PlayTimeline(0);
            target.GetTimeline(0).delegateTimelineDelegate = this;
            target.SetPauseAtIndexforAnimation(8, 7);

            // Setup blink animation
            blink = Animation.Animation_createWithResID(Resources.Img.CharAnimations);
            blink.parentAnchor = 9;
            blink.visible = false;
            blink.AddAnimationWithIDDelayLoopCountSequence(0, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 4, 41, [41, 42, 42, 42]);
            blink.SetActionTargetParamSubParamAtIndexforAnimation("ACTION_SET_VISIBLE", blink, 0, 0, 2, 0);
            blinkTimer = 3;
            blink.DoRestoreCutTransparency();
            _ = target.AddChild(blink);
            idlesTimer = RND_RANGE(5, 20);
        }
    }
}
