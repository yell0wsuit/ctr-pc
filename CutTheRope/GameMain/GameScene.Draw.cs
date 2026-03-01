using System.Collections.Generic;
using System.Linq;

using CutTheRope.Desktop;
using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Visual;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.GameMain
{
    internal sealed partial class GameScene
    {
        private static VertexPositionColor[] s_stripVerticesCache;

        private static VertexPositionColor[] GetStripVertexCache(int vertexCount)
        {
            if (s_stripVerticesCache == null || s_stripVerticesCache.Length < vertexCount)
            {
                s_stripVerticesCache = new VertexPositionColor[vertexCount];
            }
            return s_stripVerticesCache;
        }

        /// <summary>
        /// Renders the scene, including background layers and all gameplay elements.
        /// </summary>
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
            if (earthAnims != null)
            {
                foreach (object obj in earthAnims)
                {
                    ((Image)obj).Draw();
                }
            }
            Renderer.Translate(-Canvas.xOffsetScaled, 0f, 0f);
            Renderer.PopMatrix();
            Renderer.Enable(Renderer.GL_BLEND);
            Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
            pollenDrawer.Draw();
            gravityButton?.Draw();
            miceManager?.DrawHoles();
            Renderer.SetColor(Color.White);
            Renderer.Enable(Renderer.GL_TEXTURE_2D);
            Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
            decalsLayer?.Draw();
            support.Draw();
            waterLayer?.DrawBack();
            target.Draw();
            if (sleepAnimPrimary?.visible == true)
            {
                sleepAnimPrimary.Draw();
            }
            if (sleepAnimSecondary?.visible == true)
            {
                sleepAnimSecondary.Draw();
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
                        if (hand.state == MechanicalHand.STATE_HAND_CANDY)
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
            foreach (object bungeeObj in bungees)
            {
                Grab grab = (Grab)bungeeObj;
                // Reset blend mode per grab to avoid state leakage from child draws.
                Renderer.SetBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONEMINUSSRCALPHA);
                grab.DrawBack();
                grab.Draw();
            }
            foreach (object bungeeGun in bungees)
            {
                Grab grab = (Grab)bungeeGun;
                if (grab.gun)
                {
                    if (!grab.gunFired)
                    {
                        // Gun arrow tracks the candy position
                        Vector vector = VectSub(Vect(grab.x, grab.y), star.pos);
                        grab.gunArrow.rotation = RADIANS_TO_DEGREES(VectAngleNormalized(vector));
                    }
                    else
                    {
                        // Update gunCup position/rotation when fired
                        int currentTimeline = grab.gunCup.GetCurrentTimelineIndex();
                        if (currentTimeline != Grab.GUN_CUP_DROP_AND_HIDE)
                        {
                            grab.gunCup.x = star.pos.X;
                            grab.gunCup.y = star.pos.Y;
                            grab.gunCup.rotation = grab.gunInitialRotation + candy.rotation - grab.gunCandyInitialRotation;
                        }
                        grab.DrawGunCup();
                    }
                }
            }
            Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
            foreach (LightBulb bulb in lightBulbs)
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
                    if (rocket != null && !(rocket == activeRocket && (targetSock != null || targetBambooTube != null)))
                    {
                        rocket.Draw();
                    }
                }
            }
            if (!noCandy && targetSock == null)
            {
                if (!isCandyInLantern)
                {
                    candy.x = star.pos.X;
                    candy.y = star.pos.Y;
                }
                candy.Draw();
                if (candyBlink.GetCurrentTimeline() != null && !isCandyInLantern)
                {
                    Renderer.SetBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONE);
                    candyBlink.Draw();
                    Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
                }
            }
            if (hands != null)
            {
                foreach (MechanicalHand hand in hands)
                {
                    if (hand != null && hand.state == MechanicalHand.STATE_HAND_CANDY)
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
            if (twoParts != 2)
            {
                if (!noCandyL)
                {
                    candyL.x = starL.pos.X;
                    candyL.y = starL.pos.Y;
                    candyL.Draw();
                }
                if (!noCandyR)
                {
                    candyR.x = starR.pos.X;
                    candyR.y = starR.pos.Y;
                    candyR.Draw();
                }
            }
            waterLayer?.DrawFront(camera.pos.Y);
            foreach (LightBulb bulb in lightBulbs)
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
                if (bungee3.hasSpider)
                {
                    bungee3.DrawSpider();
                }
            }
            aniPool.Draw();
            Renderer.SetBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONEMINUSSRCALPHA);
            Renderer.Disable(Renderer.GL_TEXTURE_2D);
            Renderer.SetColor(Color.White);
            DrawCuts();
            Renderer.Enable(Renderer.GL_TEXTURE_2D);
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
            const float maxSize = 12f;

            for (int i = 0; i < 5; i++)
            {
                int cutCount = fingerCuts[i].Count;
                if (cutCount <= 0)
                {
                    continue;
                }

                // Collect control points from finger cut segments
                Vector[] pts = new Vector[cutCount + 1];
                int ptCount = 0;
                for (int j = 0; j < cutCount; j++)
                {
                    FingerCut cut = fingerCuts[i][j];
                    if (j == 0)
                    {
                        pts[ptCount++] = cut.start;
                    }

                    pts[ptCount++] = cut.end;
                }

                // Deduplicate consecutive identical points
                List<Vector> dedupedPts = [];
                Vector prev = default;
                bool allSame = true;
                for (int k = 0; k < pts.Length; k++)
                {
                    if (k == 0)
                    {
                        dedupedPts.Add(pts[k]);
                    }
                    else if (pts[k].X != prev.X || pts[k].Y != prev.Y)
                    {
                        dedupedPts.Add(pts[k]);
                        allSame = false;
                    }
                    prev = pts[k];
                }

                if (allSame)
                {
                    continue;
                }

                pts = [.. dedupedPts];
                int numSegments = pts.Length - 1;
                int pointsPerSegment = 2;
                int numVertices = numSegments * pointsPerSegment;
                float bstep = 1f / numVertices;

                // Sample points along the Bézier curve
                float[] curveXY = new float[numVertices * 2];
                float a = 0f;
                int curveIdx = 0;
                for (; ; )
                {
                    if (a > 1f)
                    {
                        a = 1f;
                    }

                    Vector pointOnCurve = DrawHelper.CalcPathBezier(pts, numSegments + 1, a);
                    if (curveIdx > curveXY.Length - 2)
                    {
                        break;
                    }

                    curveXY[curveIdx++] = pointOnCurve.X;
                    curveXY[curveIdx++] = pointOnCurve.Y;

                    if (a == 1f)
                    {
                        break;
                    }

                    a += bstep;
                }

                // Build triangle strip vertices with growing perpendicular width
                float step = maxSize / numVertices;
                float perpSize = 1f;
                float[] stripXY = new float[numVertices * 4];
                int stripIdx = 0;
                for (int k = 0; k < numVertices - 1; k++)
                {
                    float startSize = perpSize;
                    float endSize = k == numVertices - 2 ? 1f : perpSize + step;
                    Vector start = Vect(curveXY[k * 2], curveXY[(k * 2) + 1]);
                    Vector end = Vect(curveXY[(k + 1) * 2], curveXY[((k + 1) * 2) + 1]);
                    Vector normalized = VectNormalize(VectSub(end, start));
                    Vector rPerp = VectRperp(normalized);
                    Vector lPerp = VectPerp(normalized);

                    if (stripIdx == 0)
                    {
                        Vector startRight = VectAdd(start, VectMult(rPerp, startSize));
                        Vector startLeft = VectAdd(start, VectMult(lPerp, startSize));
                        stripXY[stripIdx++] = startLeft.X;
                        stripXY[stripIdx++] = startLeft.Y;
                        stripXY[stripIdx++] = startRight.X;
                        stripXY[stripIdx++] = startRight.Y;
                    }

                    Vector endRight = VectAdd(end, VectMult(rPerp, endSize));
                    Vector endLeft = VectAdd(end, VectMult(lPerp, endSize));
                    stripXY[stripIdx++] = endLeft.X;
                    stripXY[stripIdx++] = endLeft.Y;
                    stripXY[stripIdx++] = endRight.X;
                    stripXY[stripIdx++] = endRight.Y;
                    perpSize += step;
                }

                // Render the triangle strip
                Renderer.SetColor(Color.White);
                int vertexCount = stripIdx / 2;
                VertexPositionColor[] vertices = GetStripVertexCache(vertexCount);
                int positionIndex = 0;
                for (int v = 0; v < vertexCount; v++)
                {
                    Vector3 position = new(stripXY[positionIndex++], stripXY[positionIndex++], 0f);
                    vertices[v] = new VertexPositionColor(position, Color.White);
                }
                Renderer.DrawTriangleStrip(vertices, vertexCount);
            }
        }
    }
}
