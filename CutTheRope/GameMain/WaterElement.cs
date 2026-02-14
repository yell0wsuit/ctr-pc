using System;

using CutTheRope.Desktop;
using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Visual;

using Microsoft.Xna.Framework;

namespace CutTheRope.GameMain
{
    internal sealed class WaterElement : Image, ITimelineDelegate
    {
        private Vector topShadowSize;
        private Vector bottomShadowSize;
        private Vector topTileSize;
        private Vector backTileSize;
        private float xOffsetTop;
        private float xOffsetBack;
        private WaterBubbles bubbles;
        private AnimationsPool aniPool;
        private ScissorElement scissorElement;
        private DelayedDispatcher dd;
        private Image spotLight;
        private bool isReleasing;

        public static bool IsWaterTextureAvailable()
        {
            try
            {
                _ = Application.GetTexture(Resources.Img.WaterTile);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static WaterElement CreateWithWidthHeight(float w, float h)
        {
            try
            {
                return new WaterElement().InitWithWidthHeight(w, h);
            }
            catch
            {
                return null;
            }
        }

        private static Image CreateLightWithXPosquadalphaColordelegate(float x, int quad, RGBAColor color, ITimelineDelegate d)
        {
            Image light = Image_createWithResIDQuad(Resources.Img.WaterTile, quad);
            light.parentAnchor = 9;
            light.anchor = 9;
            light.x = x;
            light.color = RGBAColor.transparentRGBA;
            // light.DoRestoreCutTransparency();

            Timeline pulse = new Timeline().InitWithMaxKeyFramesOnTrack(5);
            pulse.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0f));
            pulse.AddKeyFrame(KeyFrame.MakeColor(color, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.7f));
            pulse.AddKeyFrame(KeyFrame.MakeColor(color, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.6f));
            pulse.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.7f));
            pulse.delegateTimelineDelegate = d;
            _ = light.AddTimeline(pulse);

            Timeline delayedStart = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            delayedStart.AddKeyFrame(KeyFrame.MakeSingleAction(light, ACTION_PLAY_TIMELINE, 0, 0, RND_RANGE(0, 20) / 10f));
            _ = light.AddTimeline(delayedStart);
            light.PlayTimeline(1);
            return light;
        }

        public WaterElement InitWithWidthHeight(float w, float h)
        {
            if (InitWithTexture(Application.GetTexture(Resources.Img.WaterTile)) == null)
            {
                return null;
            }

            width = (int)w;
            height = (int)h;
            topShadowSize = GetQuadSize(Resources.Img.WaterTile, 1);
            bottomShadowSize = GetQuadSize(Resources.Img.WaterTile, 0);
            topTileSize = GetQuadSize(Resources.Img.WaterTile, 3);
            backTileSize = GetQuadSize(Resources.Img.WaterTile, 2);
            xOffsetBack = backTileSize.X;

            const int ambientLights = 10;
            for (int i = 0; i <= ambientLights; i++)
            {
                RGBAColor alphaColor = (i is 0 or ambientLights)
                    ? RGBAColor.MakeRGBA(1f, 1f, 1f, 0.5f)
                    : RGBAColor.solidOpaqueRGBA;
                Image light = CreateLightWithXPosquadalphaColordelegate(SCREEN_WIDTH / ambientLights * (i - 1f), 7, alphaColor, this);
                _ = AddChild(light);
            }

            spotLight = CreateLightWithXPosquadalphaColordelegate(
                RND_RANGE((int)SCREEN_WIDTH / 4, (int)SCREEN_WIDTH * 3 / 4),
                6,
                RGBAColor.MakeRGBA(1f, 1f, 1f, 0.6f),
                this);
            spotLight.SetName("spot");
            _ = AddChild(spotLight);

            Timeline reveal = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            reveal.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0f));
            reveal.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.1f));
            _ = AddTimeline(reveal);
            PlayTimeline(0);

            aniPool = new AnimationsPool
            {
                parentAnchor = 9
            };
            _ = AddChild(aniPool);

            Image bubbleGrid = Image_createWithResID(Resources.Img.WaterTile);
            if (new WaterBubbles().InitWithTotalParticlesandImageGrid(40, bubbleGrid) is WaterBubbles waterBubbles)
            {
                bubbles = waterBubbles;
                bubbles.width = width;
                bubbles.height = height;
                bubbles.x = width / 2f;
                bubbles.particlesDelegate = aniPool.ParticlesFinished;
                bubbles.StartSystem(1);

                scissorElement = new ScissorElement
                {
                    parentAnchor = 9,
                    width = width,
                    height = height,
                    y = topTileSize.Y
                };
                _ = scissorElement.AddChild(bubbles);
                _ = aniPool.AddChild(scissorElement);
            }

            return this;
        }

        public void DrawBack()
        {
            if (isReleasing)
            {
                return;
            }

            Renderer.SetColor(color.ToWhiteAlphaXNA());
            float bottomY = drawY + height > SCREEN_HEIGHT ? drawY + height : SCREEN_HEIGHT;
            DrawHelper.DrawImageTiled(texture, 0, drawX, bottomY - bottomShadowSize.Y + SCREEN_OFFSET_Y, width, topShadowSize.Y);
            DrawHelper.DrawImageTiled(texture, 2, drawX - MathF.Ceiling(xOffsetBack), drawY, width + MathF.Floor(xOffsetBack), backTileSize.Y);
            Renderer.SetColor(Color.White);
        }

        public void AddParticlesAtXY(float tx, float ty)
        {
            if (isReleasing || bubbles == null)
            {
                return;
            }

            float originalX = bubbles.x;
            float originalY = bubbles.y;
            bubbles.x = tx;
            bubbles.y = ty;
            bubbles.posVar.X = 10f;
            bubbles.posVar.Y = 10f;
            for (int i = 0; i < 3; i++)
            {
                _ = bubbles.AddParticle();
            }

            bubbles.x = originalX;
            bubbles.y = originalY;
            bubbles.posVar.X = width / 2f;
            bubbles.posVar.Y = 0f;
        }

        public void AddWaterParticlesAtXY(float tx, float ty)
        {
            if (isReleasing || aniPool == null)
            {
                return;
            }

            Image image = Image_createWithResID(Resources.Img.WaterTile);
            // image.DoRestoreCutTransparency();
            if (new WaterDrops().InitWithTotalParticlesandImageGrid(10, image) is WaterDrops drops)
            {
                drops.color = RGBAColor.blackRGBA;
                drops.x = tx;
                drops.y = ty;
                drops.particlesDelegate = aniPool.ParticlesFinished;
                _ = aniPool.AddChild(drops);
                drops.StartSystem(10);
            }
        }

        public void DrawFront(float cameraY)
        {
            if (isReleasing)
            {
                return;
            }

            PreDraw();
            DrawHelper.DrawImageTiled(texture, 1, drawX, drawY, width, topShadowSize.Y);

            Renderer.SetBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONE);
            if (scissorElement != null)
            {
                scissorElement.y = topTileSize.Y - cameraY;
                scissorElement.height = height;
            }
            PostDraw();

            Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
            Renderer.SetColor(color.ToWhiteAlphaXNA());
            DrawHelper.DrawImageTiled(texture, 3, drawX - MathF.Ceiling(xOffsetTop), drawY, width + MathF.Floor(xOffsetTop), topTileSize.Y);
            Renderer.SetColor(Color.White);
        }

        public override void Draw()
        {
            DrawFront(0f);
        }

        public override void Update(float delta)
        {
            if (isReleasing)
            {
                return;
            }

            base.Update(delta);
            if (Mover.MoveVariableToTarget(ref xOffsetBack, 0f, 100f, delta))
            {
                xOffsetBack = backTileSize.X;
            }
            if (Mover.MoveVariableToTarget(ref xOffsetTop, topTileSize.X, 100f, delta))
            {
                xOffsetTop = 0f;
            }
            _ = (bubbles?.y = height + y);
            dd?.Update(delta);
        }

        public void PrepareToRelease()
        {
            isReleasing = true;
            dd?.CancelAllDispatches();
        }

        public void TimelinereachedKeyFramewithIndex(Timeline t, KeyFrame k, int i)
        {
        }

        public void TimelineFinished(Timeline t)
        {
            if (isReleasing)
            {
                return;
            }

            dd ??= new DelayedDispatcher();
            dd.CallObjectSelectorParamafterDelay(Selector_playFirstTimeline, t.element, RND_RANGE(0, 20) / 20f);

            if (ReferenceEquals(t.element, spotLight))
            {
                t.element.x = RND_RANGE((int)SCREEN_WIDTH / 4, (int)SCREEN_WIDTH * 3 / 4);
            }
        }

        private static void Selector_playFirstTimeline(FrameworkTypes param)
        {
            if (param is BaseElement element)
            {
                element.PlayTimeline(0);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                PrepareToRelease();
                dd?.Dispose();
                dd = null;
                bubbles = null;
                scissorElement = null;
                aniPool = null;
                spotLight = null;
            }
            base.Dispose(disposing);
        }
    }
}
