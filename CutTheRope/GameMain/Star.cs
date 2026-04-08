using System;

using CutTheRope.Desktop;
using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Collectible star object with optional timed and night-mode visual states.
    /// </summary>
    internal sealed class Star : CTRGameObject, ITransporterItem, ITransporterBindAware
    {
        /// <summary>Glow quad index for the normal idle star texture.</summary>
        private const int ImgObjStarIdleGlow = 0;

        /// <summary>Glow quad index for the night-mode star texture.</summary>
        private const int ImgObjStarNightGlow = 0;

        /// <summary>First quad index for the night-mode dim idle animation.</summary>
        private const int ImgObjStarNightIdleOffStart = 1;

        /// <summary>Last quad index for the night-mode dim idle animation.</summary>
        private const int ImgObjStarNightIdleOffEnd = 18;

        /// <summary>First quad index for the night-mode light-down animation.</summary>
        private const int ImgObjStarNightLightDownStart = 19;

        /// <summary>Last quad index for the night-mode light-down animation.</summary>
        private const int ImgObjStarNightLightDownEnd = 24;

        /// <summary>First quad index for the night-mode light-up animation.</summary>
        private const int ImgObjStarNightLightUpStart = 25;

        /// <summary>Last quad index for the night-mode light-up animation.</summary>
        private const int ImgObjStarNightLightUpEnd = 30;

        /// <summary>Alpha step used while fading night-mode visual layers.</summary>
        private const float NightFadeStep = 0.1f;

        /// <summary>Quad index for the full timed-star countdown ring.</summary>
        private const int TimedFullQuad = 19;  // frame_0019: full timed ring

        /// <summary>Quad index for the empty timed-star countdown ring.</summary>
        private const int TimedEmptyQuad = 20; // frame_0055: empty timed ring

        /// <summary>
        /// Creates a star from a texture.
        /// </summary>
        /// <param name="t">Texture used by the star.</param>
        /// <returns>The initialized star.</returns>
        public static Star Star_create(CTRTexture2D t)
        {
            return (Star)new Star().InitWithTexture(t);
        }

        /// <summary>
        /// Creates a star from a texture resource name.
        /// </summary>
        /// <param name="resourceName">Texture resource name.</param>
        /// <returns>The initialized star.</returns>
        public static Star Star_createWithResID(string resourceName)
        {
            return Star_create(Application.GetTexture(resourceName));
        }

        /// <summary>
        /// Initializes a star with default timed and night-mode visual state.
        /// </summary>
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <summary>
        /// Creates star animations, timed-ring visuals, idle glow, and night-mode overlays.
        /// </summary>
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
            Timeline.UpdateTimeline(timeline3, RND_RANGE(0, 20) / 10f);

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

        /// <summary>
        /// Enables night-mode visual setup before <see cref="CreateAnimations"/> runs.
        /// </summary>
        public void EnableNightMode()
        {
            nightMode = true;
        }

        /// <summary>
        /// Sets the night-mode light state and plays transition animations when needed.
        /// </summary>
        /// <param name="lit">Whether the star should be lit.</param>
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

        /// <summary>
        /// Gets whether the star is currently lit in night mode.
        /// </summary>
        public bool IsLit => isLit == true;

        /// <inheritdoc />
        public void WillBind()
        {
            if (GetCurrentTimeline() != null)
            {
                StopCurrentTimeline();
            }

            IsDrawnByTransporter = true;
        }

        /// <summary>
        /// Adjusts the alpha channel for a night-mode star visual layer.
        /// </summary>
        /// <param name="element">Element whose alpha should be adjusted.</param>
        /// <param name="delta">Alpha adjustment amount.</param>
        private static void AdjustNightAlpha(BaseElement element, float delta)
        {
            if (element == null)
            {
                return;
            }
            float next = MathF.Min(1f, MathF.Max(0f, element.color.AlphaChannel + delta));
            element.color = RGBAColor.MakeRGBA(element.color.RedColor, element.color.GreenColor, element.color.BlueColor, next);
        }

        /// <summary>
        /// Ensures all night-mode star visual layers are visible after a light-state change.
        /// </summary>
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

        /// <summary>
        /// Draws the remaining portion of the timed-star full countdown ring.
        /// </summary>
        /// <param name="quadIndex">Quad index for the full timed ring.</param>
        /// <param name="fraction">Fraction of the ring to draw.</param>
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

        /// <summary>Remaining time before a timed star expires.</summary>
        public float time;

        /// <summary>Total timeout duration for timed stars, or 0 for untimed stars.</summary>
        public float timeout;

        /// <summary>Timed-star ring animation.</summary>
        public Animation timedAnim;

        /// <summary>Whether this star uses night-mode visuals.</summary>
        private bool nightMode;

        /// <summary>Current night-mode light state, or <see langword="null"/> before initialization.</summary>
        private bool? isLit;

        /// <inheritdoc />
        public float PositionOnTransporter { get; set; }

        /// <inheritdoc />
        public Vector BindPoint => Vect(x, y);

        /// <inheritdoc />
        public void SetBindPoint(Vector point)
        {
            x = point.X;
            y = point.Y;
        }

        /// <inheritdoc />
        public float CollisionRadius => 60f;

        /// <inheritdoc />
        public float MinScale => 0.5f;

        /// <inheritdoc />
        public float MaxScale => 1.0f;

        /// <inheritdoc />
        public float TransporterScale { get; set; } = 1.0f;

        /// <inheritdoc />
        public bool IsDrawnByTransporter { get; set; }

        /// <summary>Normal idle star animation layer.</summary>
        private Animation idleSprite;

        /// <summary>Dimmed night-mode idle star animation layer.</summary>
        private Animation dimmedIdleSprite;

        /// <summary>Glow visual layer.</summary>
        private GameObject glowSprite;

        /// <summary>Night-mode light-up transition animation.</summary>
        private Animation lightUpAnim;

        /// <summary>Night-mode light-down transition animation.</summary>
        private Animation lightDownAnim;
    }
}
