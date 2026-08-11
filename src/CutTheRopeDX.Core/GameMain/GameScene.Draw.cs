using System.Linq;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.GameMain
{
    internal sealed partial class GameScene
    {
        /// <inheritdoc />
        public override void Draw()
        {
            Renderer.Clear(0);
            PreDraw();
            camera.ApplyCameraTransformation();
            Renderer.Enable(Renderer.GL_TEXTURE_2D);
            Renderer.Disable(Renderer.GL_BLEND);
            if (backTexture != null)
            {
                // Recompute in case internal resolution or texture dimensions changed.
                float desiredScale = GetBackgroundWidthScale(backTexture);
                if (ABS(desiredScale - backgroundScale) > 0.0001f)
                {
                    UpdateBackgroundScale();
                }
            }
            float backScale = back?.scaleX ?? backgroundScale;
            if (backScale <= 0f || float.IsNaN(backScale) || float.IsInfinity(backScale))
            {
                backScale = 1f;
            }
            // Keep parallax math consistent with the background scale.
            Vector pos = VectDiv(camera.pos, backScale);
            back.UpdateWithCameraPos(pos);
            float offsetX = Canvas.xOffsetScaled;
            float offsetY = 0f;
            Renderer.PushMatrix();
            Renderer.Translate(offsetX, offsetY, 0f);
            Renderer.Scale(back.scaleX, back.scaleY, 1f);
            Renderer.Translate(-offsetX, -offsetY, 0f);
            Renderer.Translate(Canvas.xOffsetScaled, 0f, 0f);
            back.Draw();
            if (mapHeight > SCREEN_HEIGHT)
            {
                int pack = ((CTRRootController)Application.SharedRootController()).GetPack();
                int p2Y = PackConfig.GetBoxBackgroundP2Y(pack);
                if (p2Y > 0)
                {
                    string[] boxBackgrounds = PackConfig.GetBoxBackgrounds(pack);
                    string p2ResourceName = boxBackgrounds.Skip(1).FirstOrDefault(name => !string.IsNullOrWhiteSpace(name));
                    if (!string.IsNullOrWhiteSpace(p2ResourceName))
                    {
                        CTRTexture2D p2Texture = Application.GetTexture(p2ResourceName);
                        CTRRectangle p2Rect = p2Texture.quadRects != null
                            ? p2Texture.quadRects[0]
                            : new CTRRectangle(0, 0, p2Texture._realWidth, p2Texture._realHeight);

                        // Enable blending for p2 to avoid dark seams where alpha overlaps p1.
                        Renderer.Enable(Renderer.GL_BLEND);
                        Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
                        // Draw p2 at configured Y position (p1 is handled by TileMap)
                        DrawHelper.DrawImagePart(p2Texture, p2Rect, 0f, p2Y);
                        Renderer.Disable(Renderer.GL_BLEND);
                    }
                }
            }
            Renderer.Enable(Renderer.GL_BLEND);
            Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
            gravityState.DrawEarthAnimations();
            Renderer.Translate(-Canvas.xOffsetScaled, 0f, 0f);
            Renderer.PopMatrix();
            Renderer.Enable(Renderer.GL_BLEND);
            Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
            pollenDrawer.Draw();
            gravityState.DrawButtons();
            miceManager?.DrawHoles();
            Renderer.SetColor(Color.White);
            Renderer.Enable(Renderer.GL_TEXTURE_2D);
            Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
            decalsLayer?.Draw();
            support.Draw();
            waterLayer?.DrawBack();
            targetObject?.Draw();
            targetAnimationController?.DrawSleepOverlays();
            // Draw additional Om Noms. targets[0] is the primary, drawn above.
            for (int ti = 1; ti < targets.Count; ti++)
            {
                TargetContext t = targets[ti];
                t.support?.Draw();
                t.targetObject?.Draw();
                t.controller?.DrawSleepOverlays();
            }
            foreach (object tutorialText in tutorials)
            {
                ((Text)tutorialText).Draw();
            }
            foreach (object tutorialImage in tutorialImages)
            {
                ((GameObject)tutorialImage).Draw();
            }
            if (antsPaths != null)
            {
                foreach (AntsPath antsPath in antsPaths)
                {
                    antsPath?.Draw();
                }
            }
            foreach (object razor in razors)
            {
                ((Razor)razor).Draw();
            }
            foreach (object rotatedCircle in rotatedCircles)
            {
                ((RotatedCircle)rotatedCircle).Draw();
            }
            conveyors.Draw();
            foreach (object bubble in bubbles)
            {
                ((GameObject)bubble).Draw();
            }
            foreach (object pump in pumps)
            {
                ((GameObject)pump).Draw();
            }
            foreach (object spike in spikes)
            {
                ((Spikes)spike).Draw();
            }
            foreach (object bouncer in bouncers)
            {
                ((Bouncer)bouncer).Draw();
            }
            foreach (BambooTube bambooTube in bambooTubes)
            {
                bambooTube?.Draw();
            }
            MechanicalHand activeHand = null;
            if (hands != null)
            {
                foreach (MechanicalHand hand in hands)
                {
                    if (hand != null)
                    {
                        hand.Draw();
                        if (hand.State == MechanicalHandState.HoldingCandy)
                        {
                            activeHand = hand;
                        }
                    }
                }
            }
            activeHand?.TheClaw().DrawActiveHand();
            miceManager?.DrawMice();
            foreach (object sockObj in socks)
            {
                Sock sock = (Sock)sockObj;
                sock.y -= 85f;
                sock.Draw();
                sock.y += 85f;
            }
            foreach (SteamTube steamTube in tubes)
            {
                steamTube?.DrawBack();
            }

            foreach (Lantern lantern in Lantern.GetAllLanterns())
            {
                lantern.Draw();
            }

            Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
            if (ghosts != null)
            {
                foreach (object objGhost in ghosts)
                {
                    Ghost ghost = (Ghost)objGhost;
                    ghost?.Draw();
                }
            }

            Renderer.SetBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONEMINUSSRCALPHA);
            // Two passes, as in the reference engine: every grab's backing layer (the rail a
            // moveable grab slides along, the hook back plate) is drawn before any rope. A single
            // interleaved pass lets a later grab's rail paint over an earlier grab's rope.
            foreach (object bungeeObj in bungees)
            {
                Grab grab = (Grab)bungeeObj;
                // Reset blend mode per grab to avoid state leakage from child draws.
                Renderer.SetBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONEMINUSSRCALPHA);
                grab.GunSource?.SetDisabled(candies[0].Lifecycle.Attachments.InLantern);
                grab.DrawBack();
            }
            foreach (object bungeeObj in bungees)
            {
                Grab grab = (Grab)bungeeObj;
                Renderer.SetBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONEMINUSSRCALPHA);
                grab.Draw();
            }

            // candiesConnected elastic: not a Grab, so draw it directly after the grab ropes.
            Renderer.SetBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONEMINUSSRCALPHA);
            candyConnector?.Draw();
            Renderer.SetColor(Color.White);

            foreach (object bungeeGun in bungees)
            {
                Grab grab = (Grab)bungeeGun;
                GunSource gun = grab.GunSource;
                if (gun == null || !gun.HasFired)
                {
                    continue;
                }

                Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
                gun.Cup?.Draw();
            }

            Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
            foreach (LightBulb bulb in LightEmitterVisuals())
            {
                bulb?.DrawLight();
            }
            foreach (object starObj in stars)
            {
                ((GameObject)starObj).Draw();
            }
            particlesAniPool.Draw();
            if (rockets != null)
            {
                foreach (Rocket rocket in rockets)
                {
                    if (rocket == null)
                    {
                        continue;
                    }
                    bool hiddenForTransit = false;
                    for (int ci = 0; ci < candies.Count; ci++)
                    {
                        CandyContext ctx = candies[ci];
                        if (rocket == ctx.Lifecycle.Attachments.Rocket && ctx.Lifecycle.Presence == CandyPresence.Hidden)
                        {
                            hiddenForTransit = true;
                            break;
                        }
                    }
                    if (!hiddenForTransit)
                    {
                        rocket.Draw();
                    }
                }
            }
            // Draw every whole candy body + its blink in one pass. A body only exists while it is
            // active, so the old removed/mid-transport guards are the enumerator's job now. Light
            // emitters draw themselves later, and a candy inside a lantern is drawn by the lantern.
            foreach (CandyBody body in ActiveCandyBodies())
            {
                CandyContext ctx = body.Owner;
                if (body.Role != CandyBodyRole.Whole || ctx.emitsLight || ctx.Lifecycle.Attachments.InLantern)
                {
                    continue;
                }
                body.Visual.x = body.Point.pos.X;
                body.Visual.y = body.Point.pos.Y;
                body.Visual.Draw();
                if (body.BlinkAnimation?.GetCurrentTimeline() != null)
                {
                    Renderer.SetBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONE);
                    body.BlinkAnimation.Draw();
                    Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
                }
            }
            if (hands != null)
            {
                foreach (MechanicalHand hand in hands)
                {
                    if (hand != null && hand.State == MechanicalHandState.HoldingCandy)
                    {
                        hand.TheClaw().DrawFingers();
                    }
                }
            }
            if (snailobjects != null)
            {
                foreach (Snail snail in snailobjects)
                {
                    snail?.Draw();
                }
            }
            // Split halves draw on top of the hands and snails, the z-order the split levels ship with.
            foreach (CandyBody body in ActiveCandyBodies())
            {
                if (body.Role != CandyBodyRole.Whole)
                {
                    body.Visual.Draw();
                }
            }
            waterLayer?.DrawFront(camera.pos.Y);
            foreach (LightBulb bulb in LightEmitterVisuals())
            {
                bulb?.DrawBottleAndFirefly();
            }
            foreach (SteamTube steamTube2 in tubes)
            {
                steamTube2?.DrawFront();
            }
            foreach (object bungeeSpider in bungees)
            {
                Grab bungee3 = (Grab)bungeeSpider;
                if (bungee3.Spider is SpiderRider drawnRider && drawnRider.IsAttached)
                {
                    drawnRider.Animation.Draw();
                }
            }
            aniPool.Draw();
            Renderer.SetColor(Color.White);
            DrawCuts();
            Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
            camera.CancelCameraTransformation();
            staticAniPool.Draw();
            PostDraw();
        }

        /// <summary>
        /// Renders finger cut trails as triangle strips with Bézier-smoothed paths
        /// that grow in width from start to end.
        /// </summary>
        public void DrawCuts()
        {
            foreach (PointerGestureState gesture in pointerGestures)
            {
                gesture.Trace?.Draw();
            }
        }
    }
}
