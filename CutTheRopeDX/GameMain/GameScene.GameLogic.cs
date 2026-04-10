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
        /// Checks if the given constraint point is a candy (star, starL, or starR).
        /// Used to distinguish candy from other objects like light bulbs.
        /// </summary>
        /// <param name="point">Constraint point to test.</param>
        /// <returns><see langword="true"/> if the point belongs to a candy body; otherwise, <see langword="false"/>.</returns>
        private bool IsCandyPoint(ConstraintedPoint point)
        {
            return point == star || point == starL || point == starR;
        }

        /// <summary>
        /// Completes pending candy teleportation through a bamboo tube or sock.
        /// </summary>
        public void Teleport()
        {
            if (targetBambooTube != null)
            {
                noCandy = false;
                RestoreCandyProperties();
                targetBambooTube.ThrowCandy(star);
                targetBambooTube.ThrowParticlesOut(particlesAniPool);
                candy.PlayTimeline(2);
                if (activeRocket != null)
                {
                    Vector holeOut = targetBambooTube.HoleOut;
                    Vector tubeCenter = Vect(targetBambooTube.x, targetBambooTube.y);
                    activeRocket.rotation = RADIANS_TO_DEGREES(VectAngleNormalized(VectSub(tubeCenter, holeOut)));
                    activeRocket.startRotation = activeRocket.rotation;
                    activeRocket.startCandyRotation = 0f;
                    candyMain.rotation = 0f;
                    activeRocket.additionalAngle = 0f;
                    activeRocket.UpdateRotation();
                    activeRocket.point.posDelta = vectZero;
                    activeRocket.point.pos = star.pos;
                    activeRocket.point.prevPos = activeRocket.point.pos;
                    activeRocket.point.v = vectZero;
                }
                else
                {
                    star.disableGravity = false;
                }

                targetBambooTube = null;
                return;
            }

            if (targetSock != null)
            {
                targetSock.light.PlayTimeline(0);
                targetSock.light.visible = true;
                Vector v = Vect(0f, -16f);
                v = VectRotate(v, DEGREES_TO_RADIANS(targetSock.rotation));
                star.pos.X = targetSock.x;
                star.pos.Y = targetSock.y;
                star.pos = VectAdd(star.pos, v);
                star.prevPos.X = star.pos.X;
                star.prevPos.Y = star.pos.Y;
                star.v = VectMult(VectRotate(Vect(0f, -1f), DEGREES_TO_RADIANS(targetSock.rotation)), savedSockSpeed);
                star.posDelta = VectDiv(star.v, 60f);
                star.prevPos = VectSub(star.pos, star.posDelta);

                // Reset rocket direction when candy teleports through sock
                if (activeRocket != null)
                {
                    activeRocket.point.pos = star.pos;

                    // Maintain rocket momentum
                    activeRocket.point.prevPos = star.prevPos;
                    activeRocket.point.v = star.v;
                    activeRocket.point.posDelta = star.posDelta;

                    activeRocket.rotation = targetSock.rotation + DEG_90;
                    activeRocket.startRotation = targetSock.rotation + DEG_90;
                    activeRocket.startCandyRotation = candyMain.rotation;
                    activeRocket.additionalAngle = 0f;
                    activeRocket.UpdateRotation();
                }

                targetSock = null;
            }
        }

        /// <summary>
        /// Releases a light bulb from its attached sock and applies the sock exit velocity.
        /// </summary>
        /// <param name="bulb">Light bulb to drop from its attached sock.</param>
        private static void DropLightBulbFromSock(LightBulb bulb)
        {
            if (bulb == null || bulb.attachedSock == null)
            {
                return;
            }

            Sock sock = bulb.attachedSock;
            if (sock.light != null)
            {
                sock.light.PlayTimeline(0);
                sock.light.visible = true;
            }

            Vector v = Vect(0f, -16f);
            v = VectRotate(v, DEGREES_TO_RADIANS(sock.rotation));
            bulb.constraint.pos.X = sock.x;
            bulb.constraint.pos.Y = sock.y;
            bulb.constraint.pos = VectAdd(bulb.constraint.pos, v);
            bulb.constraint.prevPos.X = bulb.constraint.pos.X;
            bulb.constraint.prevPos.Y = bulb.constraint.pos.Y;
            bulb.constraint.v = VectMult(VectRotate(Vect(0f, -1f), DEGREES_TO_RADIANS(sock.rotation)), bulb.sockSpeed);
            bulb.constraint.posDelta = VectDiv(bulb.constraint.v, 60f);
            bulb.constraint.prevPos = VectSub(bulb.constraint.pos, bulb.constraint.posDelta);
            bulb.attachedSock = null;
            bulb.sockSpeed = 0f;
            bulb.SyncToConstraint();
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
        /// Cuts or hides ropes attached to a light bulb.
        /// </summary>
        /// <param name="bulb">Light bulb whose ropes should be released.</param>
        private void ReleaseLightBulbRopes(LightBulb bulb)
        {
            if (bulb == null)
            {
                return;
            }

            int grabCount = bungees.Count;
            for (int i = 0; i < grabCount; i++)
            {
                Grab grab = bungees[i];
                Bungee rope = grab.rope;
                if (rope != null && rope.tail == bulb.constraint)
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
                }
            }
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
            dd.CancelAllDispatches();

            // Hide sleep animations and reset sleep state for night levels
            SetNightSleepVisibility(false);
            sleepPulseActive = false;
            sleepSoundTimer = 0f;
            if (targetObject != null)
            {
                targetObject.scaleX = targetBaseScaleX;
                targetObject.scaleY = targetBaseScaleY;
                targetObject.rotationCenterX = 0f;
                targetObject.rotationCenterY = 0f;
            }

            targetAnimationController?.PlayChewing();
            CTRSoundMgr.PlayOmNomSound(Resources.Snd.MonsterChewing);
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
            if (activeRocket != null)
            {
                activeRocket.state = Rocket.STATE_ROCKET_EXAUST;
                activeRocket.StopAnimation();
            }
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
            if (gameLostTriggered)
            {
                return;
            }
            gameLostTriggered = true;

            dd.CancelAllDispatches();

            // Hide sleep animations and reset sleep state for night levels
            SetNightSleepVisibility(false);
            sleepPulseActive = false;
            sleepSoundTimer = 0f;
            if (targetObject != null)
            {
                targetObject.scaleX = targetBaseScaleX;
                targetObject.scaleY = targetBaseScaleY;
                targetObject.rotationCenterX = 0f;
                targetObject.rotationCenterY = 0f;
            }

            targetAnimationController?.PlaySad();
            CTRSoundMgr.PlayOmNomSound(Resources.Snd.MonsterSad);
            dd.CallObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(Selector_animateLevelRestart), null, 1);
            gameSceneDelegate.GameLost();
            if (activeRocket != null)
            {
                activeRocket.state = Rocket.STATE_ROCKET_EXAUST;
                activeRocket.StopAnimation();
            }
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
        /// Pops the bubble captured by a light bulb and restores any ghost cycling state.
        /// </summary>
        /// <param name="bulb">Light bulb whose captured bubble should be popped.</param>
        private void PopLightBulbBubble(LightBulb bulb)
        {
            if (bulb?.capturingBubble == null)
            {
                return;
            }

            EnableGhostCycleForBubble(bulb.capturingBubble);
            bulb.capturingBubble.capturedByBulb = false;
            bulb.capturingBubble.popped = true;
            bulb.capturingBubble.RemoveChildWithID(0);
            conveyors.Remove(bulb.capturingBubble);
            bulb.capturingBubble = null;
            bulb.capturingGhostBubble = false;

            PopBubbleAtXY(bulb.x, bulb.y);
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
        /// Releases all mechanical hands currently holding the candy and unblocks conveyors if needed.
        /// </summary>
        public void DetachActiveHands()
        {
            if (hands == null || hands.Count <= 0)
            {
                return;
            }

            bool releasedHand = false;
            foreach (MechanicalHand hand in hands)
            {
                if (hand != null && hand.state == MechanicalHand.STATE_HAND_CANDY)
                {
                    hand.cPoint.RemoveConstraint(star);
                    hand.state = MechanicalHand.STATE_HAND_RELEASE;
                    hand.doRotateCandy = false;
                    hand.releaseSoundPlayed = false;
                    hand.AnimateReleaseWithAnimationsPool(aniPool);
                    releasedHand = true;
                }
            }

            if (releasedHand)
            {
                ResetConveyor();
                UnblockConveyor();
            }
        }

        /// <summary>
        /// Handles game-scene button actions such as toggling gravity.
        /// </summary>
        /// <param name="_">Game scene button identifier.</param>
        public void OnButtonPressed(GameSceneButtonId _)
        {
            if (MaterialPoint.globalGravity.Y == ActivePhysicsConstants.GravityEarthY)
            {
                MaterialPoint.globalGravity.Y = -ActivePhysicsConstants.GravityEarthY;
                gravityNormal = false;
                CTRSoundMgr.PlaySound(Resources.Snd.GravityOn);
            }
            else
            {
                MaterialPoint.globalGravity.Y = ActivePhysicsConstants.GravityEarthY;
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
