using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Helpers;

namespace CutTheRopeDX.GameMain
{
    internal sealed partial class GameScene
    {
        private const float PostEatSleepDelay = 2f;

        private void SchedulePostEatSleep(TargetContext target)
        {
            if (target.Feeding.Phase != TargetFeedingPhase.Chewing
                || !GameWinChewing.ShouldSchedulePostEatSleep(
                    targets.Count,
                    nightLevel,
                    target.controller?.UsesFlashXmlAnimations == true))
            {
                return;
            }

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
            // Any outcome owning the level cancels the nap, including one that finished during the
            // delay. Not CanReactToCandy: this target is mid-chew, so it is already fed, and that
            // predicate answers a different question (may Om Nom react to a *new* candy).
            if (target == null
                || gameplayFlow.HasOutcome
                || !GameWinChewing.ShouldSchedulePostEatSleep(
                    targets.Count,
                    nightLevel,
                    target.controller?.UsesFlashXmlAnimations == true))
            {
                return;
            }

            if (!target.Feeding.TryFallAsleep())
            {
                return;
            }

            target.NightSleep.StartPostEatPresentation(NightSleepSoundInterval);
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
                if (!target.Feeding.IsAsleep || target.targetObject == null)
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

                if (target.NightSleep.AdvanceSound(delta, NightSleepSoundInterval))
                {
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
