using System;
using System.Collections.Generic;

using CutTheRope.Desktop;
using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    internal sealed class Star : CTRGameObject, IConveyorItem, IConveyorSizeProvider, IConveyorPaddingProvider
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
        private const float ConveyorSizeScale = 0.9f;
        private const float ConveyorPaddingJs = 8f;
        private const float JsPm = 1.2f;

        public static Star Star_create(CTRTexture2D t)
        {
            return (Star)new Star().InitWithTexture(t);
        }

        public static Star Star_createWithResID(int r)
        {
            return Star_create(Application.GetTexture(ResourceNameTranslator.TranslateLegacyId(r)));
        }

        public static Star Star_createWithResID(string resourceName)
        {
            return Star_create(Application.GetTexture(resourceName));
        }

        public static Star Star_createWithResIDQuad(int r, int q)
        {
            Star star = Star_create(Application.GetTexture(ResourceNameTranslator.TranslateLegacyId(r)));
            star.SetDrawQuad(q);
            return star;
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
            bobTime = 0f;
        }

        public override void Update(float delta)
        {
            if (timeout > 0.0 && time > 0.0)
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
            UpdateBobOffset(delta);
        }

        public override void Draw()
        {
            timedAnim?.Draw();

            // Each child element has its own blendingMode set, so just use standard rendering
            PreDraw();
            PostDraw();

            // Reset blend state after drawing to prevent affecting subsequent elements (like candy)
            OpenGL.GlBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
        }

        public void CreateAnimations()
        {
            if (timeout > 0.0)
            {
                timedAnim = Animation_createWithResID(Resources.Img.ObjStarIdle);
                timedAnim.anchor = timedAnim.parentAnchor = 18;
                float d = timeout / 37f;
                timedAnim.AddAnimationWithIDDelayLoopFirstLast(0, d, Timeline.LoopType.TIMELINE_NO_LOOP, 19, 55);
                timedAnim.PlayTimeline(0);
                time = timeout;
                timedAnim.visible = false;
                _ = AddChild(timedAnim);
                Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
                timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
                timedAnim.AddTimelinewithID(timeline, 1);
                Timeline timeline2 = new Timeline().InitWithMaxKeyFramesOnTrack(2);
                timeline2.AddKeyFrame(KeyFrame.MakeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline2.AddKeyFrame(KeyFrame.MakeScale(0.0, 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.25));
                timeline2.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline2.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.25));
                AddTimelinewithID(timeline2, 1);
            }
            bb = new CTRRectangle(22f, 20f, 30f, 30f);
            bobTime = (float)(RND_RANGE(0, 20) / 10.0);

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
            Timeline.UpdateTimeline(animation.GetTimeline(0), (float)(RND_RANGE(0, 20) / 10.0));
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

        private void UpdateBobOffset(float delta)
        {
            bobTime += delta;
            bool onConveyor = ConveyorId != -1;
            float offset = onConveyor ? 0f : (float)(3.0 * Sinf(3f * bobTime));
            Dictionary<int, BaseElement> childs = GetChilds();
            foreach (KeyValuePair<int, BaseElement> kvp in childs)
            {
                BaseElement child = kvp.Value;
                if (child != null)
                {
                    child.y = offset;
                }
            }
        }

        public Vector GetConveyorSize()
        {
            CTRRectangle bounds = bb.Equals(default) ? new CTRRectangle(22f, 20f, 30f, 30f) : bb;
            return Vect(bounds.w * ConveyorSizeScale, bounds.h * ConveyorSizeScale);
        }

        public float GetConveyorPadding()
        {
            return ConveyorPaddingJs * RotatedCircle.PM / JsPm;
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
                    if (timeline != null)
                    {
                        timeline.OnFinished = () =>
                        {
                            if (lightUpAnim != null)
                            {
                                lightUpAnim.visible = false;
                            }
                        };
                    }
                    lightUpAnim.PlayTimeline(0);

                    // Play star light sound
                    CTRSoundMgr.PlayRandomSound(Resources.Snd.StarLight1, Resources.Snd.StarLight2);
                }
            }
            else if (lightDownAnim != null && !isInitial)
            {
                lightDownAnim.visible = true;
                Timeline timeline = lightDownAnim.GetTimeline(0);
                if (timeline != null)
                {
                    timeline.OnFinished = () =>
                    {
                        if (lightDownAnim != null)
                        {
                            lightDownAnim.visible = false;
                        }
                    };
                }
                lightDownAnim.PlayTimeline(0);
            }
            else if (isInitial)
            {
                if (glowSprite != null)
                {
                    glowSprite.color = RGBAColor.transparentRGBA;
                }
                if (idleSprite != null)
                {
                    idleSprite.color = RGBAColor.transparentRGBA;
                }
            }

            UpdateNightVisibility();
        }

        public bool IsLit => isLit == true;

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
            if (glowSprite != null)
            {
                glowSprite.visible = true;
            }
            if (idleSprite != null)
            {
                idleSprite.visible = true;
            }
            if (dimmedIdleSprite != null)
            {
                dimmedIdleSprite.visible = true;
            }
        }

        public float time;

        public float timeout;

        public Animation timedAnim;

        private bool nightMode;

        private bool? isLit;

        public int ConveyorId { get; set; } = -1;

        public float? ConveyorBaseScaleX { get; set; }

        public float? ConveyorBaseScaleY { get; set; }

        private float bobTime;

        private Animation idleSprite;

        private Animation dimmedIdleSprite;

        private GameObject glowSprite;

        private Animation lightUpAnim;

        private Animation lightDownAnim;
    }
}
