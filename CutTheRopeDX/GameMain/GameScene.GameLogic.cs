using System;
using System.Collections.Generic;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Physics;
using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.GameMain
{
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Whether candy <paramref name="ci"/> is gone. The primary candy (index 0) tracks its presence
        /// through the scene singleton <see cref="noCandy"/> (which a split also forces true); every
        /// other candy uses its own <see cref="CandyContext.noCandy"/>. Centralised so the primary-vs-extra
        /// rule lives in one place.
        /// </summary>
        private bool CandyGone(int ci, CandyContext ctx)
        {
            return ci == 0 ? noCandy : ctx.noCandy;
        }

        private CandyContext CandyForPointOrNull(ConstraintedPoint point)
        {
            for (int i = 0; i < candies.Count; i++)
            {
                if (candies[i].point == point)
                {
                    return candies[i];
                }
            }

            return null;
        }

        private bool IsSpiderGrabbableCandyPoint(ConstraintedPoint point)
        {
            if (point == star || point == starL || point == starR)
            {
                return true;
            }

            CandyContext ctx = CandyForPointOrNull(point);
            return ctx != null && ctx.Capabilities.CanBeGrabbedBySpider;
        }

        private IEnumerable<CandyContext> LightEmitters()
        {
            for (int i = 0; i < candies.Count; i++)
            {
                CandyContext ctx = candies[i];
                if (ctx.emitsLight && !ctx.noCandy)
                {
                    yield return ctx;
                }
            }
        }

        private IEnumerable<LightBulb> LightEmitterVisuals()
        {
            foreach (CandyContext ctx in LightEmitters())
            {
                if (ctx.lightBulb != null)
                {
                    yield return ctx.lightBulb;
                }
            }
        }

        private CandyContext FindLightEmitterByNumber(string bulbNumber)
        {
            CandyContext fallback = null;
            for (int i = 0; i < candies.Count; i++)
            {
                CandyContext ctx = candies[i];
                if (!ctx.emitsLight || ctx.lightBulb == null)
                {
                    continue;
                }

                fallback = ctx;
                if (!string.IsNullOrEmpty(bulbNumber)
                    && string.Equals(ctx.lightBulbNumber, bulbNumber, StringComparison.OrdinalIgnoreCase))
                {
                    return ctx;
                }
            }

            return fallback;
        }

        /// <summary>
        /// Completes pending candy teleportation through a bamboo tube or sock for one candy.
        /// </summary>
        public void Teleport(CandyContext ctx)
        {
            if (ctx.targetBambooTube != null)
            {
                ctx.noCandy = false;
                RestoreCandyProperties(ctx);
                ctx.targetBambooTube.ThrowCandy(ctx.point);
                ctx.targetBambooTube.ThrowParticlesOut(particlesAniPool);
                ctx.candy.PlayTimeline(2);
                if (ctx.HasActiveRocket)
                {
                    Vector holeOut = ctx.targetBambooTube.HoleOut;
                    Vector tubeCenter = Vect(ctx.targetBambooTube.x, ctx.targetBambooTube.y);
                    ctx.activeRocket.rotation = RADIANS_TO_DEGREES(VectAngleNormalized(VectSub(tubeCenter, holeOut)));
                    ctx.activeRocket.startRotation = ctx.activeRocket.rotation;
                    ctx.activeRocket.startCandyRotation = 0f;
                    GameObject rocketCandyVisual = ctx.candyMain ?? ctx.candy;
                    rocketCandyVisual.rotation = 0f;
                    ctx.activeRocket.additionalAngle = 0f;
                    ctx.activeRocket.UpdateRotation();
                    ctx.activeRocket.point.posDelta = vectZero;
                    ctx.activeRocket.point.pos = ctx.point.pos;
                    ctx.activeRocket.point.prevPos = ctx.activeRocket.point.pos;
                    ctx.activeRocket.point.v = vectZero;
                }
                else
                {
                    ctx.point.disableGravity = false;
                }

                ctx.targetBambooTube = null;
                return;
            }

            if (ctx.targetSock != null)
            {
                ctx.targetSock.light.PlayTimeline(0);
                ctx.targetSock.light.visible = true;
                Vector v = Vect(0f, ActivePhysicsConstants.SockExitOffsetY);
                v = VectRotate(v, DEGREES_TO_RADIANS(ctx.targetSock.rotation));
                ctx.point.pos.X = ctx.targetSock.x;
                ctx.point.pos.Y = ctx.targetSock.y;
                ctx.point.pos = VectAdd(ctx.point.pos, v);
                ctx.point.prevPos.X = ctx.point.pos.X;
                ctx.point.prevPos.Y = ctx.point.pos.Y;
                ctx.point.v = VectMult(VectRotate(Vect(0f, -1f), DEGREES_TO_RADIANS(ctx.targetSock.rotation)), ctx.savedSockSpeed);
                ctx.point.posDelta = VectDiv(ctx.point.v, 60f);
                ctx.point.prevPos = VectSub(ctx.point.pos, ctx.point.posDelta);

                if (ctx.HasActiveRocket)
                {
                    ctx.activeRocket.point.pos = ctx.point.pos;
                    ctx.activeRocket.point.prevPos = ctx.point.prevPos;
                    ctx.activeRocket.point.v = ctx.point.v;
                    ctx.activeRocket.point.posDelta = ctx.point.posDelta;
                    ctx.activeRocket.rotation = ctx.targetSock.rotation + DEG_90;
                    ctx.activeRocket.startRotation = ctx.targetSock.rotation + DEG_90;
                    ctx.activeRocket.startCandyRotation = ctx.candyMain.rotation;
                    ctx.activeRocket.additionalAngle = 0f;
                    ctx.activeRocket.UpdateRotation();
                }

                ctx.targetSock = null;
                ctx.lightBulb?.SyncFromContext(ctx);
            }
        }

        /// <summary>
        /// Starts the level restart dimming animation.
        /// </summary>
        public void AnimateLevelRestart()
        {
            restartState = 0;
            dimTime = 0.15f;
        }

        /// <summary>
        /// Releases all ropes attached to the active candy body or candy half.
        /// </summary>
        /// <param name="left"><see langword="true"/> to release ropes attached to the left candy half; <see langword="false"/> to release the right half or whole candy.</param>
        public void ReleaseAllRopes(bool left)
        {
            int grabCount = bungees.Count;
            for (int i = 0; i < grabCount; i++)
            {
                Grab grab = bungees[i];
                Bungee rope = grab.rope;
                if (rope != null && (rope.tail == star || (rope.tail == starL && left) || (rope.tail == starR && !left)))
                {
                    if (rope.cut == -1)
                    {
                        rope.SetCut(rope.parts.Count - 2);
                    }
                    else
                    {
                        rope.hideTailParts = true;
                    }
                    if (grab.hasSpider && grab.spiderActive)
                    {
                        SpiderBusted(grab);
                    }
                    if (grab.gun && grab.gunCup != null && RGBAColor.RGBAEqual(RGBAColor.solidOpaqueRGBA, grab.gunCup.color))
                    {
                        grab.gunCup.PlayTimeline(Grab.GUN_CUP_DROP_AND_HIDE);
                    }
                }
            }
        }

        /// <summary>Cuts/hides all uncut ropes whose tail is the given candy point.</summary>
        public void ReleaseRopesForPoint(ConstraintedPoint candyPoint)
        {
            int grabCount = bungees.Count;
            for (int i = 0; i < grabCount; i++)
            {
                Grab grab = bungees[i];
                Bungee rope = grab.rope;
                if (rope != null && rope.tail == candyPoint)
                {
                    if (rope.cut == -1)
                    {
                        rope.SetCut(rope.parts.Count - 2);
                    }
                    else
                    {
                        rope.hideTailParts = true;
                    }
                    if (grab.hasSpider && grab.spiderActive)
                    {
                        SpiderBusted(grab);
                    }
                    if (grab.gun && grab.gunCup != null
                        && RGBAColor.RGBAEqual(RGBAColor.solidOpaqueRGBA, grab.gunCup.color))
                    {
                        grab.gunCup.PlayTimeline(Grab.GUN_CUP_DROP_AND_HIDE);
                    }
                }
            }
            // candiesConnected elastic: not in `bungees`. When one of its candy endpoints is
            // released, cut it at the matching end (tail end vs head end), like the engine's
            // releaseRopeForTheCandy. If already cut, hide the dangling tail segments.
            if (candyConnector != null
                && (candyPoint == candyConnector.tail || candyPoint == candyConnector.bungeeAnchor))
            {
                if (candyConnector.cut == -1)
                {
                    int cutPart = candyPoint == candyConnector.tail ? candyConnector.parts.Count - 2 : 0;
                    candyConnector.SetCut(cutPart);
                }
                else
                {
                    candyConnector.hideTailParts = true;
                }
            }
        }

        /// <summary>True when any candy is currently captured in the lantern group (group single-occupancy).</summary>
        private bool AnyCandyInLantern()
        {
            for (int i = 0; i < candies.Count; i++)
            {
                if (candies[i].inLantern)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>The candy currently flown by <paramref name="rocket"/>, or null if none.</summary>
        private CandyContext RocketBoundCandy(Rocket rocket)
        {
            for (int i = 0; i < candies.Count; i++)
            {
                if (candies[i].activeRocket == rocket)
                {
                    return candies[i];
                }
            }
            return null;
        }

        /// <summary>The candy held by <paramref name="hand"/>, or null if the hand holds none.</summary>
        private CandyContext HandHeldCandy(MechanicalHand hand)
        {
            for (int i = 0; i < candies.Count; i++)
            {
                if (candies[i].capturingHand == hand)
                {
                    return candies[i];
                }
            }
            return null;
        }

        /// <summary>
        /// The nearest grabbable candy to <paramref name="hand"/> (not eaten, not in a lantern, not in a
        /// sock) and its distance. Returns null with <paramref name="distance"/> = float.MaxValue if none.
        /// </summary>
        private CandyContext NearestGrabbableCandy(MechanicalHand hand, out float distance)
        {
            CandyContext nearest = null;
            distance = float.MaxValue;
            for (int i = 0; i < candies.Count; i++)
            {
                CandyContext ctx = candies[i];
                if (!ctx.IsHandGrabbable || ctx.inLantern || ctx.targetSock != null)
                {
                    continue;
                }
                float d = VectDistance(hand.cPoint.pos, ctx.point.pos);
                if (d < distance)
                {
                    distance = d;
                    nearest = ctx;
                }
            }
            return nearest;
        }

        /// <summary>Exhausts the rocket bound to <paramref name="ctx"/> (one-time consume) and clears the binding.</summary>
        private static void ExhaustRocketForCandy(CandyContext ctx)
        {
            if (ctx.activeRocket == null)
            {
                return;
            }
            ctx.activeRocket.state = Rocket.STATE_ROCKET_EXAUST;
            ctx.activeRocket.StopAnimation();
            ctx.activeRocket = null;
        }

        /// <summary>
        /// Nudges a flying rocket's <see cref="Rocket.additionalAngle"/> to fly perpendicular to
        /// <paramref name="rope"/>, picking whichever of the two perpendiculars is the smaller turn.
        /// Shared by the grab ropes and the candy connector (iOS steers off both the same way).
        /// </summary>
        private static void AlignRocketAngleToRope(Rocket rocket, Bungee rope, float delta)
        {
            ConstraintedPoint anchor = rope.bungeeAnchor;
            ConstraintedPoint tail = rope.parts[^1];
            Vector ropeVector = VectSub(anchor.pos, tail.pos);
            Vector v1 = VectPerp(ropeVector);
            Vector v2 = VectRperp(ropeVector);
            float fa = RADIANS_TO_DEGREES(VectAngleNormalized(v1) - DEGREES_TO_RADIANS(rocket.rotation));
            float fb = RADIANS_TO_DEGREES(VectAngleNormalized(v2) - DEGREES_TO_RADIANS(rocket.rotation));
            rocket.additionalAngle = AngleTo0_360(rocket.additionalAngle);
            fa = NearestAngleTofrom(rocket.additionalAngle, fa);
            fb = NearestAngleTofrom(rocket.additionalAngle, fb);
            float da = MinAngleBetweenAandB(rocket.additionalAngle, fa);
            float db = MinAngleBetweenAandB(rocket.additionalAngle, fb);
            float target = da < db ? fa : fb;
            _ = Mover.MoveVariableToTarget(ref rocket.additionalAngle, target, 90f, delta);
        }

        /// <summary>Exhausts every candy's bound rocket (win/loss cleanup).</summary>
        private void ExhaustAllActiveRockets()
        {
            for (int i = 0; i < candies.Count; i++)
            {
                ExhaustRocketForCandy(candies[i]);
            }
        }

        /// <summary>
        /// Calculates time, star, and total score bonuses for the completed level.
        /// </summary>
        public void CalculateScore()
        {
            timeBonus = (int)MAX(0f, 30f - time) * 100;
            timeBonus /= 10;
            timeBonus *= 10;
            starBonus = 1000 * starsCollected;
            score = (int)Ceil(timeBonus + starBonus);
        }

        /// <summary>
        /// Handles the level-won sequence, including candy consumption, scoring, cleanup, and delegate notification.
        /// </summary>
        public void GameWon()
        {
            if (!GameOutcomeTransition.CanTriggerTerminalOutcome(gameWonTriggered, gameLostTriggered))
            {
                return;
            }
            gameWonTriggered = true;
            outcomeTransitionActive = true;

            EndActiveFingerTraces();
            dd.CancelAllDispatches();

            // Hide and reset sleep state for every Om Nom except one mid post-eat sleep: that
            // one keeps sleeping (and its zzz keeps looping) through the win transition, so it
            // is left untouched to avoid hiding/replaying its overlay.
            for (int ti = 0; ti < targets.Count; ti++)
            {
                TargetContext t = targets[ti];
                if (t.postEatSleepActive)
                {
                    continue;
                }
                SetNightSleepVisibility(t, false);
                t.sleepPulseActive = false;
                t.sleepSoundTimer = 0f;
                t.postEatSleepScheduled = false;
                if (t.targetObject != null)
                {
                    t.targetObject.scaleX = t.baseScaleX;
                    t.targetObject.scaleY = t.baseScaleY;
                    t.targetObject.rotationCenterX = 0f;
                    t.targetObject.rotationCenterY = 0f;
                }
            }

            if (GameWinChewing.ShouldPlayPrimaryChewingOnGameWon(targets.Count))
            {
                targetAnimationController?.PlayChewing();
                CTRSoundMgr.PlayOmNomSound(Resources.Snd.MonsterChewing, targetAnimationController?.SkinDefinition);
            }
            if (candyBubble != null)
            {
                PopCandyBubble(false);
            }
            noCandy = true;
            candy.passTransformationsToChilds = true;
            candyMain.scaleX = candyMain.scaleY = 1f;
            candyTop.scaleX = candyTop.scaleY = 1f;
            Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            timeline.AddKeyFrame(KeyFrame.MakePos((int)candy.x, (int)candy.y, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
            float targetX = targetObject != null ? targetObject.x : candy.x;
            float targetY = targetObject != null ? targetObject.y : candy.y;
            timeline.AddKeyFrame(KeyFrame.MakePos((int)targetX, (int)(targetY + 10), KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.1f));
            timeline.AddKeyFrame(KeyFrame.MakeScale(0.71f, 0.71f, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
            timeline.AddKeyFrame(KeyFrame.MakeScale(0, 0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.1f));
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.1f));
            candy.AddTimelinewithID(timeline, 0);
            candy.PlayTimeline(0);
            timeline.delegateTimelineDelegate = aniPool;
            _ = aniPool.AddChild(candy);
            dd.CallObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(Selector_gameWon), null, 2);
            CalculateScore();
            ReleaseAllRopes(false);
            ExhaustAllActiveRockets();
            DetachActiveSnails();
            DetachActiveHands();

            // Make the mouse retreat and lock it from advancing to next mouse
            if (miceManager != null && mice != null)
            {
                foreach (object obj in mice)
                {
                    if (obj is Mouse mouse && mouse.IsActive)
                    {
                        mouse.BeginRetreat();
                        break;
                    }
                }
            }
            miceManager?.LockActiveMouse();
        }

        /// <summary>
        /// Handles the level-lost sequence and schedules the restart animation.
        /// </summary>
        public void GameLost()
        {
            if (!GameOutcomeTransition.CanTriggerTerminalOutcome(gameWonTriggered, gameLostTriggered))
            {
                return;
            }
            gameLostTriggered = true;
            outcomeTransitionActive = true;

            EndActiveFingerTraces();
            dd.CancelAllDispatches();

            // Hide and reset sleep state for every Om Nom except one mid post-eat sleep: that
            // one keeps sleeping (and its zzz keeps looping) through the loss transition, so it
            // is left untouched to avoid hiding/replaying its overlay.
            for (int ti = 0; ti < targets.Count; ti++)
            {
                TargetContext t = targets[ti];
                if (t.postEatSleepActive)
                {
                    continue;
                }
                SetNightSleepVisibility(t, false);
                t.sleepPulseActive = false;
                t.sleepSoundTimer = 0f;
                t.postEatSleepScheduled = false;
                if (t.targetObject != null)
                {
                    t.targetObject.scaleX = t.baseScaleX;
                    t.targetObject.scaleY = t.baseScaleY;
                    t.targetObject.rotationCenterX = 0f;
                    t.targetObject.rotationCenterY = 0f;
                }
            }

            // Every Om Nom reacts sad on loss, except one that is already asleep after eating:
            // it stays asleep rather than waking to react. A still-chewing (pre-sleep) Om Nom
            // is not yet asleep, so it reacts sad normally.
            for (int ti = 0; ti < targets.Count; ti++)
            {
                TargetContext t = targets[ti];
                if (t.postEatSleepActive)
                {
                    continue;
                }
                t.controller?.PlaySad();
                CTRSoundMgr.PlayOmNomSound(Resources.Snd.MonsterSad, t.controller?.SkinDefinition);
            }
            dd.CallObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(Selector_animateLevelRestart), null, 1);
            gameSceneDelegate.GameLost();
            // Rockets are exhausted per-candy at each loss site (breakCandy in the C reference only
            // stops the lost candy's own rocket; gameLoseIm stops none). A surviving candy's rocket
            // keeps burning through the restart animation, matching the original.
            DetachActiveHands();

            // Make the mouse retreat and lock it from advancing to next mouse
            if (miceManager != null && mice != null)
            {
                foreach (object obj in mice)
                {
                    if (obj is Mouse mouse && mouse.IsActive)
                    {
                        mouse.BeginRetreat();
                        break;
                    }
                }
            }
            miceManager?.LockActiveMouse();
        }

        /// <summary>
        /// Pops the bubble currently holding the candy or one of its split halves.
        /// </summary>
        /// <param name="left"><see langword="true"/> to pop the left candy bubble when the candy is split; <see langword="false"/> to pop the right or whole-candy bubble.</param>
        public void PopCandyBubble(bool left)
        {
            if (twoParts == 2)
            {
                if (ghosts != null)
                {
                    foreach (Ghost ghost in ghosts)
                    {
                        if (ghost != null)
                        {
                            if (ghost.bubble == candyBubble)
                            {
                                ghost.cyclingEnabled = true;
                                ghost.ResetToState(1);
                            }
                            if (shouldRestoreSecondGhost && ghost.bubble == candyBubbleR)
                            {
                                ghost.cyclingEnabled = true;
                                ghost.ResetToState(1);
                                candyBubbleR = null;
                                shouldRestoreSecondGhost = false;
                            }
                        }
                    }
                }
                candyBubble = null;
                candyBubbleAnimation.visible = false;
                if (isCandyInGhostBubbleAnimationLoaded)
                {
                    candyGhostBubbleAnimation.visible = false;
                }
                PopBubbleAtXY(candy.x, candy.y);
                return;
            }
            if (left)
            {
                if (ghosts != null)
                {
                    foreach (Ghost ghost2 in ghosts)
                    {
                        if (ghost2 != null && ghost2.bubble == candyBubbleL)
                        {
                            ghost2.cyclingEnabled = true;
                            ghost2.ResetToState(1);
                        }
                    }
                }
                candyBubbleL = null;
                candyBubbleAnimationL.visible = false;
                if (isCandyInGhostBubbleAnimationLeftLoaded)
                {
                    candyGhostBubbleAnimationL.visible = false;
                }
                PopBubbleAtXY(candyL.x, candyL.y);
                return;
            }
            if (ghosts != null)
            {
                foreach (Ghost ghost3 in ghosts)
                {
                    if (ghost3 != null && ghost3.bubble == candyBubbleR)
                    {
                        ghost3.cyclingEnabled = true;
                        ghost3.ResetToState(1);
                    }
                }
            }
            candyBubbleR = null;
            candyBubbleAnimationR.visible = false;
            if (isCandyInGhostBubbleAnimationRightLoaded)
            {
                candyGhostBubbleAnimationR.visible = false;
            }
            PopBubbleAtXY(candyR.x, candyR.y);
        }

        /// <summary>Pops the bubble carrying a specific additional candy (candies[1+]).</summary>
        public void PopCandyBubble(CandyContext ctx)
        {
            if (ctx == null || ctx.bubble == null)
            {
                return;
            }
            if (ctx.bubbleHasGhost)
            {
                EnableGhostCycleForBubble(ctx.bubble);
            }
            if (ctx.bubble is Bubble bubble)
            {
                bubble.capturedByBulb = false;
            }
            ctx.bubble = null;
            ctx.bubbleHasGhost = false;
            ctx.lightBulb?.SyncFromContext(ctx);
            _ = (ctx.candyBubbleAnimation?.visible = false);
            _ = (ctx.candyGhostBubbleAnimation?.visible = false);
            PopBubbleAtXY(ctx.candy.x, ctx.candy.y);
        }

        /// <summary>
        /// Plays bubble-pop effects at a world position.
        /// </summary>
        /// <param name="bx">World-space X position for the pop effect.</param>
        /// <param name="by">World-space Y position for the pop effect.</param>
        public void PopBubbleAtXY(float bx, float by)
        {
            CTRSoundMgr.PlaySound(Resources.Snd.BubbleBreak);
            Animation animation = Animation.Animation_createWithResID(Resources.Img.ObjBubble);
            animation.DoRestoreCutTransparency();
            animation.x = bx;
            animation.y = by;
            animation.anchor = 18;
            int i = animation.AddAnimationDelayLoopFirstLast(0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 18, 29);
            animation.GetTimeline(i).delegateTimelineDelegate = aniPool;
            animation.PlayTimeline(0);
            _ = aniPool.AddChild(animation);
        }

        /// <summary>
        /// Spawns the candy-break particle burst at a world position and plays the break sound.
        /// </summary>
        /// <param name="bx">World-space X for the burst.</param>
        /// <param name="by">World-space Y for the burst.</param>
        private void SpawnCandyBreakParticles(float bx, float by)
        {
            int selectedCandySkin = Preferences.GetIntForKey("PREFS_SELECTED_CANDY");
            string candyResource = CandySkinHelper.GetCandyResource(selectedCandySkin);
            Image image2 = Image.Image_createWithResID(candyResource);
            image2.DoRestoreCutTransparency();
            CandyBreak candyBreak = (CandyBreak)new CandyBreak().InitWithTotalParticlesandImageGrid(5, image2);
            if (gravityButton != null && !gravityNormal)
            {
                candyBreak.gravity.Y = -ActivePhysicsConstants.CandyBreakGravityY;
                candyBreak.angle = 90f;
            }
            candyBreak.particlesDelegate = new Particles.ParticlesFinished(aniPool.ParticlesFinished);
            candyBreak.x = bx;
            candyBreak.y = by;
            candyBreak.StartSystem(5);
            _ = aniPool.AddChild(candyBreak);
            CTRSoundMgr.PlaySound(Resources.Snd.CandyBreak);
        }

        /// <summary>
        /// Flags every ghost so its captured-candy reacts to a candy break this frame.
        /// </summary>
        private void MarkGhostsCandyBreak()
        {
            if (ghosts == null)
            {
                return;
            }
            foreach (object objGhost in ghosts)
            {
                Ghost ghost = (Ghost)objGhost;
                _ = (ghost?.candyBreak = true);
            }
        }

        /// <summary>
        /// Schedules the loss sequence after a delay (e.g. while a candy-break animation plays) and
        /// immediately marks the outcome transition active. A destroyed candy sets <c>noCandy</c> but
        /// defers <see cref="GameLost"/>; without marking the transition, another candy eaten during
        /// that window would satisfy the win check and trigger a false win in a multi-candy level.
        /// </summary>
        /// <param name="delay">Seconds to wait before running the loss sequence.</param>
        private void ScheduleGameLost(float delay)
        {
            outcomeTransitionActive = true;
            dd.CallObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(Selector_gameLost), null, delay);
        }

        /// <summary>
        /// Destroys a whole candy that touched a hazard (spike, axe, ...): pops its bubble, marks it
        /// gone, releases its ropes, detaches transports, schedules the loss, and flags ghosts. The
        /// per-index effect calls differ deliberately: candies[0] uses the singleton paths
        /// (candyBubble, ReleaseAllRopes + gun-cup drop, singleton noCandy) which are NOT equivalent
        /// to the per-candy paths used by candies[1..].
        /// </summary>
        /// <param name="index">Index of the candy in <c>candies</c>.</param>
        /// <param name="ctx">The candy being destroyed.</param>
        private void BreakCandyFromHazard(int index, CandyContext ctx)
        {
            if (index == 0)
            {
                if (candyBubble != null)
                {
                    PopCandyBubble(false);
                }
            }
            else
            {
                PopCandyBubble(ctx);
            }
            ctx.candy.x = ctx.point.pos.X;
            ctx.candy.y = ctx.point.pos.Y;
            if (index == 0)
            {
                noCandy = true;
            }
            else
            {
                ctx.noCandy = true;
            }
            ExhaustRocketForCandy(ctx);
            SpawnCandyBreakParticles(ctx.candy.x, ctx.candy.y);
            if (index == 0)
            {
                ReleaseAllRopes(false);
            }
            else
            {
                ReleaseRopesForPoint(ctx.point);
            }
            DetachHandsForPoint(ctx.point);
            DetachSnailsForPoint(ctx.point);
            if (restartState != 0)
            {
                ScheduleGameLost(0.3f);
            }
            MarkGhostsCandyBreak();
        }

        /// <summary>
        /// Destroys one half of the split candy (candies[0], <c>twoParts != 2</c>) that touched a
        /// hazard. Keeps the half-aware singleton effect calls verbatim.
        /// </summary>
        /// <param name="left"><see langword="true"/> when the left half was hit; otherwise the right half.</param>
        private void BreakSplitCandyHalf(bool left)
        {
            if (left)
            {
                if (candyBubbleL != null)
                {
                    PopCandyBubble(true);
                }
            }
            else if (candyBubbleR != null)
            {
                PopCandyBubble(false);
            }
            float breakX, breakY;
            if (left)
            {
                breakX = candyL.x;
                breakY = candyL.y;
                noCandyL = true;
            }
            else
            {
                breakX = candyR.x;
                breakY = candyR.y;
                noCandyR = true;
            }
            ExhaustRocketForCandy(candies[0]);
            SpawnCandyBreakParticles(breakX, breakY);
            ReleaseAllRopes(left);
            DetachHandsForPoint(candies[0].point);
            DetachSnailsForPoint(candies[0].point);
            if (restartState != 0 && (!noCandyL || !noCandyR))
            {
                ScheduleGameLost(0.3f);
            }
            MarkGhostsCandyBreak();
        }

        /// <summary>
        /// Cuts all ropes for the specified candy number, except the one belonging to the given Grab.
        /// Matches iOS destroyRopesForCandy:except:.
        /// </summary>
        /// <param name="candyNumber">Candy number whose ropes should be destroyed.</param>
        /// <param name="except">Grab whose rope should be preserved.</param>
        private void DestroyRopesForCandy(int candyNumber, Grab except)
        {
            for (int i = 0; i < bungees.Count; i++)
            {
                Grab grab = bungees[i];
                if (grab != except && grab.candyNumber == candyNumber && grab.rope != null && grab.rope.cut == -1)
                {
                    grab.rope.SetCut(grab.rope.parts.Count - 2);
                }
            }
        }

        /// <summary>
        /// Clears the highlighted state from all uncut bungee ropes.
        /// </summary>
        public void ResetBungeeHighlight()
        {
            for (int i = 0; i < bungees.Count; i++)
            {
                Bungee rope = bungees[i].rope;
                if (rope != null && rope.cut == -1)
                {
                    rope.highlighted = false;
                }
            }
        }

        /// <summary>
        /// Detaches all active snails from the candy.
        /// </summary>
        public void DetachActiveSnails()
        {
            if (snailobjects == null || snailobjects.Count <= 0)
            {
                return;
            }

            for (int i = snailobjects.Count - 1; i >= 0; i--)
            {
                Snail snail = snailobjects[i];
                if (snail != null && snail.state == Snail.SNAIL_STATE_ACTIVE)
                {
                    snail.Detach();
                }
            }
        }

        /// <summary>
        /// Number of active snails currently riding the given candy point.
        /// </summary>
        /// <param name="point">Candy physics point to count attached snails for.</param>
        /// <returns>The count of snails in the active state whose attached point is <paramref name="point"/>; 0 if none or <paramref name="point"/> is null.</returns>
        public int ActiveSnailCountForPoint(ConstraintedPoint point)
        {
            if (snailobjects == null || snailobjects.Count <= 0 || point == null)
            {
                return 0;
            }

            int count = 0;
            foreach (Snail snail in snailobjects)
            {
                if (snail != null && snail.state == Snail.SNAIL_STATE_ACTIVE && snail.AttachedPoint() == point)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>Detaches active snails riding the given candy point (no-op if null).</summary>
        public void DetachSnailsForPoint(ConstraintedPoint point)
        {
            if (snailobjects == null || snailobjects.Count <= 0 || point == null)
            {
                return;
            }

            for (int i = snailobjects.Count - 1; i >= 0; i--)
            {
                Snail snail = snailobjects[i];
                if (snail != null && snail.state == Snail.SNAIL_STATE_ACTIVE && snail.AttachedPoint() == point)
                {
                    snail.Detach();
                }
            }
        }

        /// <summary>
        /// Releases all mechanical hands currently holding a candy. Once a candy's
        /// <see cref="CandyContext.capturingHand"/> is cleared the ant conveyor is free to pick it up
        /// again, so no global conveyor unblock is needed.
        /// </summary>
        public void DetachActiveHands()
        {
            if (hands == null || hands.Count <= 0)
            {
                return;
            }

            foreach (MechanicalHand hand in hands)
            {
                if (hand != null && hand.state == MechanicalHand.STATE_HAND_CANDY)
                {
                    CandyContext held = HandHeldCandy(hand);
                    ConstraintedPoint heldPoint = held?.point ?? star;
                    hand.cPoint.RemoveConstraint(heldPoint);
                    hand.state = MechanicalHand.STATE_HAND_RELEASE;
                    hand.doRotateCandy = false;
                    hand.releaseSoundPlayed = false;
                    hand.AnimateReleaseWithAnimationsPool(aniPool);
                    _ = (held?.capturingHand = null);
                }
            }
        }

        /// <summary>Releases only the mechanical hand holding the candy at <paramref name="point"/> (no-op if null).</summary>
        public void DetachHandsForPoint(ConstraintedPoint point)
        {
            if (hands == null || hands.Count <= 0 || point == null)
            {
                return;
            }

            foreach (MechanicalHand hand in hands)
            {
                if (hand != null && hand.state == MechanicalHand.STATE_HAND_CANDY)
                {
                    CandyContext held = HandHeldCandy(hand);
                    ConstraintedPoint heldPoint = held?.point ?? star;
                    if (heldPoint != point)
                    {
                        continue;
                    }
                    hand.cPoint.RemoveConstraint(heldPoint);
                    hand.state = MechanicalHand.STATE_HAND_RELEASE;
                    hand.doRotateCandy = false;
                    hand.releaseSoundPlayed = false;
                    hand.AnimateReleaseWithAnimationsPool(aniPool);
                    _ = (held?.capturingHand = null);
                }
            }
        }

        /// <summary>
        /// Handles game-scene button actions such as toggling gravity.
        /// </summary>
        /// <param name="_">Game scene button identifier.</param>
        public void OnButtonPressed(GameSceneButtonId _)
        {
            if (MaterialPoint.globalGravity.Y == globalGravityY)
            {
                MaterialPoint.globalGravity.Y = -globalGravityY;
                gravityNormal = false;
                CTRSoundMgr.PlaySound(Resources.Snd.GravityOn);
            }
            else
            {
                MaterialPoint.globalGravity.Y = globalGravityY;
                gravityNormal = true;
                CTRSoundMgr.PlaySound(Resources.Snd.GravityOff);
            }
            if (earthAnims == null)
            {
                return;
            }
            foreach (object obj in earthAnims)
            {
                Image earthAnim = (Image)obj;
                if (gravityNormal)
                {
                    earthAnim.PlayTimeline(0);
                }
                else
                {
                    earthAnim.PlayTimeline(1);
                }
            }
        }

        /// <inheritdoc />
        void IButtonDelegation.OnButtonPressed(ButtonId buttonId)
        {
            OnButtonPressed(GameSceneButtonId.FromButtonId(buttonId));
        }

        /// <summary>
        /// Rotates every spike object matching the supplied toggle ID.
        /// </summary>
        /// <param name="sid">Spike toggle identifier to match.</param>
        public void RotateAllSpikesWithID(int sid)
        {
            foreach (object obj in spikes)
            {
                Spikes spike = (Spikes)obj;
                if (spike.GetToggled() == sid)
                {
                    spike.RotateSpikes();
                }
            }
        }

        /// <summary>
        /// Re-enables ghost cycling for any ghost that owns the specified bubble.
        /// </summary>
        /// <param name="bubbleObj">Bubble object whose owning ghost should resume cycling.</param>
        private void EnableGhostCycleForBubble(GameObject bubbleObj)
        {
            if (bubbleObj is not Bubble bubble || ghosts == null)
            {
                return;
            }
            foreach (object obj in ghosts)
            {
                Ghost ghost = (Ghost)obj;
                if (ghost != null && ghost.bubble == bubble)
                {
                    ghost.cyclingEnabled = true;
                    ghost.ResetToState(1);
                }
            }
        }

        /// <summary>
        /// Disables ghost cycling for any ghost that owns the specified bubble.
        /// </summary>
        /// <param name="bubbleObj">Bubble object whose owning ghost should stop cycling.</param>
        /// <returns><see langword="true"/> if at least one ghost was affected; otherwise, <see langword="false"/>.</returns>
        private bool DisableGhostCycleForBubble(GameObject bubbleObj)
        {
            if (bubbleObj is not Bubble bubble || ghosts == null)
            {
                return false;
            }
            bool affected = false;
            foreach (object obj in ghosts)
            {
                Ghost ghost = (Ghost)obj;
                if (ghost != null && ghost.bubble == bubble)
                {
                    ghost.cyclingEnabled = false;
                    affected = true;
                }
            }
            return affected;
        }
    }
}
