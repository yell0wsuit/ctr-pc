using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Helpers;

namespace CutTheRopeDX.GameMain
{
    internal sealed partial class GameScene
    {
        private const float PostEatSleepDelay = 2f;

        private void SchedulePostEatSleep(TargetContext target)
        {
            if (target.postEatSleepScheduled
                || !GameWinChewing.ShouldSchedulePostEatSleep(
                    targets.Count,
                    nightLevel,
                    target.controller?.UsesFlashXmlAnimations == true))
            {
                return;
            }

            target.postEatSleepScheduled = true;
            dd.CallObjectSelectorParamafterDelay(
                new DelayedDispatcher.DispatchFunc(Selector_startPostEatSleep),
                new PostEatSleepRequest(target),
                PostEatSleepDelay);
        }

        private void Selector_startPostEatSleep(FrameworkTypes param)
        {
            if (param is not PostEatSleepRequest request)
            {
                return;
            }

            TargetContext target = request.Target;
            if (target == null
                || !target.asleep
                || outcomeTransitionActive
                || !GameWinChewing.ShouldSchedulePostEatSleep(
                    targets.Count,
                    nightLevel,
                    target.controller?.UsesFlashXmlAnimations == true))
            {
                return;
            }

            target.postEatSleepActive = true;
            target.sleepSoundTimer = NightSleepSoundInterval;
            SetNightSleepVisibility(target, false);
            target.controller?.PlaySleepingWithoutIdleToSleepTrim();
        }

        private void UpdatePostEatSleep(float delta)
        {
            if (nightLevel)
            {
                return;
            }

            for (int ti = 0; ti < targets.Count; ti++)
            {
                TargetContext target = targets[ti];
                if (!target.postEatSleepActive || target.targetObject == null)
                {
                    continue;
                }

                bool shouldShowSleepOverlay = target.controller?.IsSleepingAnimationPlaying() == true;
                SetNightSleepVisibility(target, shouldShowSleepOverlay);
                if (!shouldShowSleepOverlay)
                {
                    continue;
                }

                target.controller?.UpdateSleepOverlays(delta);
                target.controller?.SyncSleepOverlayPosition(target.targetObject.x, target.targetObject.y);

                target.sleepSoundTimer += delta;
                if (target.sleepSoundTimer > NightSleepSoundInterval)
                {
                    target.sleepSoundTimer = 0f;
                    CTRSoundMgr.PlayRandomOmNomSound(
                        target.controller?.SkinDefinition,
                        Resources.Snd.MonsterSleep1,
                        Resources.Snd.MonsterSleep2,
                        Resources.Snd.MonsterSleep3);
                }
            }
        }
    }
}
