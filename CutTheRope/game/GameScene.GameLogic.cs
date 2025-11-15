using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.sfe;
using CutTheRope.iframework.visual;

namespace CutTheRope.game
{
    internal sealed partial class GameScene
    {
        public void Teleport()
        {
            if (targetSock != null)
            {
                targetSock.light.PlayTimeline(0);
                targetSock.light.visible = true;
                Vector v = Vect(0f, -16f);
                v = VectRotate(v, (double)DEGREES_TO_RADIANS(targetSock.rotation));
                star.pos.x = targetSock.x;
                star.pos.y = targetSock.y;
                star.pos = VectAdd(star.pos, v);
                star.prevPos.x = star.pos.x;
                star.prevPos.y = star.pos.y;
                star.v = VectMult(VectRotate(Vect(0f, -1f), (double)DEGREES_TO_RADIANS(targetSock.rotation)), savedSockSpeed);
                star.posDelta = VectDiv(star.v, 60f);
                star.prevPos = VectSub(star.pos, star.posDelta);
                targetSock = null;
            }
        }

        public void AnimateLevelRestart()
        {
            restartState = 0;
            dimTime = 0.15f;
        }

        public void ReleaseAllRopes(bool left)
        {
            int num = bungees.Count;
            for (int i = 0; i < num; i++)
            {
                Grab grab = bungees.ObjectAtIndex(i);
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
                }
            }
        }

        public void CalculateScore()
        {
            timeBonus = (int)MAX(0f, 30f - time) * 100;
            timeBonus /= 10;
            timeBonus *= 10;
            starBonus = 1000 * starsCollected;
            score = (int)Ceil(timeBonus + starBonus);
        }

        public void GameWon()
        {
            dd.CancelAllDispatches();
            target.PlayTimeline(6);
            CTRSoundMgr.PlaySound(15);
            if (candyBubble != null)
            {
                PopCandyBubble(false);
            }
            noCandy = true;
            candy.passTransformationsToChilds = true;
            candyMain.scaleX = candyMain.scaleY = 1f;
            candyTop.scaleX = candyTop.scaleY = 1f;
            Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            timeline.AddKeyFrame(KeyFrame.MakePos(candy.x, candy.y, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline.AddKeyFrame(KeyFrame.MakePos(target.x, target.y + 10.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.1));
            timeline.AddKeyFrame(KeyFrame.MakeScale(0.71, 0.71, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline.AddKeyFrame(KeyFrame.MakeScale(0.0, 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.1));
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.1));
            candy.AddTimelinewithID(timeline, 0);
            candy.PlayTimeline(0);
            timeline.delegateTimelineDelegate = aniPool;
            _ = aniPool.AddChild(candy);
            dd.CallObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(Selector_gameWon), null, 2.0);
            CalculateScore();
            ReleaseAllRopes(false);
        }

        public void GameLost()
        {
            dd.CancelAllDispatches();
            target.PlayAnimationtimeline(102, 5);
            CTRSoundMgr.PlaySound(18);
            dd.CallObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(Selector_animateLevelRestart), null, 1.0);
            gameSceneDelegate.GameLost();
        }

        public void PopCandyBubble(bool left)
        {
            if (twoParts == 2)
            {
                candyBubble = null;
                candyBubbleAnimation.visible = false;
                PopBubbleAtXY(candy.x, candy.y);
                return;
            }
            if (left)
            {
                candyBubbleL = null;
                candyBubbleAnimationL.visible = false;
                PopBubbleAtXY(candyL.x, candyL.y);
                return;
            }
            candyBubbleR = null;
            candyBubbleAnimationR.visible = false;
            PopBubbleAtXY(candyR.x, candyR.y);
        }

        public void PopBubbleAtXY(float bx, float by)
        {
            CTRSoundMgr.PlaySound(12);
            Animation animation = Animation.Animation_createWithResID(73);
            animation.DoRestoreCutTransparency();
            animation.x = bx;
            animation.y = by;
            animation.anchor = 18;
            int i = animation.AddAnimationDelayLoopFirstLast(0.05, Timeline.LoopType.TIMELINE_NO_LOOP, 0, 11);
            animation.GetTimeline(i).delegateTimelineDelegate = aniPool;
            animation.PlayTimeline(0);
            _ = aniPool.AddChild(animation);
        }

        public void ResetBungeeHighlight()
        {
            for (int i = 0; i < bungees.Count; i++)
            {
                Bungee rope = bungees.ObjectAtIndex(i).rope;
                if (rope != null && rope.cut == -1)
                {
                    rope.highlighted = false;
                }
            }
        }

        public void OnButtonPressed(int n)
        {
            if (MaterialPoint.globalGravity.y == 784.0)
            {
                MaterialPoint.globalGravity.y = -784f;
                gravityNormal = false;
                CTRSoundMgr.PlaySound(39);
            }
            else
            {
                MaterialPoint.globalGravity.y = 784f;
                gravityNormal = true;
                CTRSoundMgr.PlaySound(38);
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
    }
}
