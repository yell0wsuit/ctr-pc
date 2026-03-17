using System;

using CutTheRope.Desktop;
using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    internal sealed class Star : CTRGameObject, ITransporterItem, ITransporterBindAware
    {
        private const int ImgObjStarIdleGlow = 0;
        private const int ImgObjStarNightGlow = 0;
        private const int ImgObjStarNightIdleOffStart = 1;
        private const int ImgObjStarNightIdleOffEnd = 18;
        private const int ImgObjStarNightLightDownStart = 19;
        private const int ImgObjStarNightLightDownEnd = 24;
        private const int ImgObjStarNightLightUpStart = 25;
        private const int ImgObjStarNightLightUpEnd = 30;
        private const float NightFadeStep = 0.1f;

        private const int TimedFullQuad = 19;  // frame_0019: full timed ring
        private const int TimedEmptyQuad = 20; // frame_0055: empty timed ring

        public static Star Star_create(CTRTexture2D t)
        {
            return (Star)new Star().InitWithTexture(t);
        }

        public static Star Star_createWithResID(string resourceName)
        {
            return Star_create(Application.GetTexture(resourceName));
        }

        public Star()
        {
            timedAnim = null;
            nightMode = false;
            isLit = null;
            idleSprite = null;
            dimmedIdleSprite = null;
            glowSprite = null;
            lightUpAnim = null;
            lightDownAnim = null;
        }

        public override void Update(float delta)
        {
            if (timeout > 0 && time > 0)
            {
                _ = Mover.MoveVariableToTarget(ref time, 0f, 1f, delta);
            }
            if (nightMode)
            {
                if (isLit == true)
                {
                    AdjustNightAlpha(glowSprite, NightFadeStep);
                    AdjustNightAlpha(dimmedIdleSprite, -NightFadeStep);
                    AdjustNightAlpha(idleSprite, NightFadeStep);
                }
                else
                {
                    AdjustNightAlpha(glowSprite, -NightFadeStep);
                    AdjustNightAlpha(dimmedIdleSprite, NightFadeStep);
                    AdjustNightAlpha(idleSprite, -NightFadeStep);
                }
            }
            base.Update(delta);
        }

        public override void Draw()
        {
            if (timedAnim != null && timeout > 0)
            {
                timedAnim.PreDraw();
                // Draw empty ring (always visible as background)
                timedAnim.DrawQuad(TimedEmptyQuad);
                // Draw full ring with radial clipping based on remaining time
                float fraction = time / timeout;
                if (fraction > 0f)
                {
                    DrawTimedFullRadial(TimedFullQuad, fraction);
                }
                timedAnim.PostDraw();
            }

            // Each child element has its own blendingMode set, so just use standard rendering
            PreDraw();
            PostDraw();

            // Reset blend state after drawing to prevent affecting subsequent elements (like candy)
            Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
        }

        public void CreateAnimations()
        {
            if (timeout > 0)
            {
                timedAnim = Animation_createWithResID(Resources.Img.ObjStarIdle);
                timedAnim.anchor = timedAnim.parentAnchor = 18;
                timedAnim.SetDrawQuad(TimedEmptyQuad);
                time = timeout;
                timedAnim.visible = false;
                _ = AddChild(timedAnim);
                Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
                timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
                timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5f));
                timedAnim.AddTimelinewithID(timeline, 1);
                Timeline timeline2 = new Timeline().InitWithMaxKeyFramesOnTrack(2);
                timeline2.AddKeyFrame(KeyFrame.MakeScale(1, 1, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
                timeline2.AddKeyFrame(KeyFrame.MakeScale(0, 0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.25f));
                timeline2.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
                timeline2.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.25f));
                AddTimelinewithID(timeline2, 1);
            }
            bb = new CTRRectangle(22f, 20f, 30f, 30f);

            Timeline timeline3 = new Timeline().InitWithMaxKeyFramesOnTrack(5);
            timeline3.AddKeyFrame(KeyFrame.MakePos((int)x, (int)y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0f));
            timeline3.AddKeyFrame(KeyFrame.MakePos((int)x, (int)y - 3, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.5f));
            timeline3.AddKeyFrame(KeyFrame.MakePos((int)x, (int)y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.5f));
            timeline3.AddKeyFrame(KeyFrame.MakePos((int)x, (int)y + 3, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.5f));
            timeline3.AddKeyFrame(KeyFrame.MakePos((int)x, (int)y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.5f));
            timeline3.SetTimelineLoopType(Timeline.LoopType.TIMELINE_REPLAY);
            AddTimelinewithID(timeline3, 0);
            PlayTimeline(0);
            Timeline.UpdateTimeline(timeline3, RND_RANGE(0, 20) / 10);

            // Add glow sprite
            if (!nightMode)
            {
                glowSprite = GameObject_createWithResIDQuad(Resources.Img.ObjStarIdle, ImgObjStarIdleGlow);
                glowSprite.anchor = glowSprite.parentAnchor = 18;
                glowSprite.blendingMode = -1; // Normal blending
                _ = AddChild(glowSprite);
            }
            else
            {
                glowSprite = GameObject_createWithResIDQuad(Resources.Img.ObjStarNight, ImgObjStarNightGlow);
                glowSprite.anchor = glowSprite.parentAnchor = 18;
                glowSprite.color = RGBAColor.MakeRGBA(1f, 1f, 1f, 0.4f);
                glowSprite.blendingMode = 1; // Normal blending
                _ = AddChild(glowSprite);
            }

            Animation animation = Animation_createWithResID(Resources.Img.ObjStarIdle);
            animation.DoRestoreCutTransparency();
            _ = animation.AddAnimationDelayLoopFirstLast(0.05f, Timeline.LoopType.TIMELINE_REPLAY, 1, 18);
            animation.PlayTimeline(0);
            Timeline.UpdateTimeline(animation.GetTimeline(0), RND_RANGE(0, 20) / 10f);
            animation.anchor = animation.parentAnchor = 18;
            idleSprite = animation;
            _ = AddChild(animation);

            if (nightMode)
            {
                dimmedIdleSprite = Animation_createWithResID(Resources.Img.ObjStarNight);
                dimmedIdleSprite.anchor = dimmedIdleSprite.parentAnchor = 18;
                dimmedIdleSprite.blendingMode = 1; // Normal blending
                _ = dimmedIdleSprite.AddAnimationDelayLoopFirstLast(0.05f, Timeline.LoopType.TIMELINE_REPLAY, ImgObjStarNightIdleOffStart, ImgObjStarNightIdleOffEnd);
                dimmedIdleSprite.PlayTimeline(0);
                dimmedIdleSprite.color = RGBAColor.transparentRGBA;
                _ = AddChild(dimmedIdleSprite);

                lightUpAnim = Animation_createWithResID(Resources.Img.ObjStarNight);
                lightUpAnim.anchor = lightUpAnim.parentAnchor = 18;
                lightUpAnim.blendingMode = 2; // Additive blending (SRC_ALPHA, ONE)
                _ = lightUpAnim.AddAnimationDelayLoopFirstLast(0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, ImgObjStarNightLightUpStart, ImgObjStarNightLightUpEnd);
                lightUpAnim.visible = false;
                _ = AddChild(lightUpAnim);

                lightDownAnim = Animation_createWithResID(Resources.Img.ObjStarNight);
                lightDownAnim.anchor = lightDownAnim.parentAnchor = 18;
                lightDownAnim.blendingMode = 1; // Normal blending
                _ = lightDownAnim.AddAnimationDelayLoopFirstLast(0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, ImgObjStarNightLightDownStart, ImgObjStarNightLightDownEnd);
                lightDownAnim.visible = false;
                _ = AddChild(lightDownAnim);

                UpdateNightVisibility();
            }
        }

        public void EnableNightMode()
        {
            nightMode = true;
        }

        public void SetLitState(bool lit)
        {
            if (!nightMode)
            {
                isLit = true;
                return;
            }

            if (isLit == lit)
            {
                return;
            }

            bool isInitial = isLit == null;
            isLit = lit;

            if (lit)
            {
                if (lightUpAnim != null && !isInitial)
                {
                    lightUpAnim.visible = true;
                    Timeline timeline = lightUpAnim.GetTimeline(0);
                    _ = (timeline?.OnFinished = () =>
                        {
                            _ = (lightUpAnim?.visible = false);
                        });
                    lightUpAnim.PlayTimeline(0);

                    // Play star light sound
                    CTRSoundMgr.PlayRandomSound(Resources.Snd.StarLight1, Resources.Snd.StarLight2);
                }
            }
            else if (lightDownAnim != null && !isInitial)
            {
                lightDownAnim.visible = true;
                Timeline timeline = lightDownAnim.GetTimeline(0);
                _ = (timeline?.OnFinished = () =>
                    {
                        _ = (lightDownAnim?.visible = false);
                    });
                lightDownAnim.PlayTimeline(0);
            }
            else if (isInitial)
            {
                _ = (glowSprite?.color = RGBAColor.transparentRGBA);
                _ = (idleSprite?.color = RGBAColor.transparentRGBA);
            }

            UpdateNightVisibility();
        }

        public bool IsLit => isLit == true;

        public void WillBind()
        {
            if (GetCurrentTimeline() != null)
            {
                StopCurrentTimeline();
            }

            IsDrawnByTransporter = true;
        }

        private static void AdjustNightAlpha(BaseElement element, float delta)
        {
            if (element == null)
            {
                return;
            }
            float next = MathF.Min(1f, MathF.Max(0f, element.color.AlphaChannel + delta));
            element.color = RGBAColor.MakeRGBA(element.color.RedColor, element.color.GreenColor, element.color.BlueColor, next);
        }

        private void UpdateNightVisibility()
        {
            if (!nightMode)
            {
                return;
            }
            _ = (glowSprite?.visible = true);
            _ = (idleSprite?.visible = true);
            _ = (dimmedIdleSprite?.visible = true);
        }

        private void DrawTimedFullRadial(int quadIndex, float fraction)
        {
            float px = timedAnim.drawX;
            float py = timedAnim.drawY;
            if (timedAnim.restoreCutTransparency)
            {
                px += timedAnim.texture.quadOffsets[quadIndex].X;
                py += timedAnim.texture.quadOffsets[quadIndex].Y;
            }
            DrawHelper.DrawRadialClippedQuad(timedAnim.texture, quadIndex, px, py, fraction);
        }

        public float time;

        public float timeout;

        public Animation timedAnim;

        private bool nightMode;

        private bool? isLit;

        public float PositionOnTransporter { get; set; }

        public Vector BindPoint => Vect(x, y);

        public void SetBindPoint(Vector point)
        {
            x = point.X;
            y = point.Y;
        }

        public float CollisionRadius => 60f;

        public float MinScale => 0.5f;

        public float MaxScale => 1.0f;

        public float TransporterScale { get; set; } = 1.0f;

        public bool IsDrawnByTransporter { get; set; }

        private Animation idleSprite;

        private Animation dimmedIdleSprite;

        private GameObject glowSprite;

        private Animation lightUpAnim;

        private Animation lightDownAnim;
    }
}
