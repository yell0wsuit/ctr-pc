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
            float num = Canvas.xOffsetScaled;
            float num2 = 0f;
            Renderer.PushMatrix();
            Renderer.Translate(num, num2, 0f);
            Renderer.Scale(back.scaleX, back.scaleY, 1f);
            Renderer.Translate(-num, -num2, 0f);
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
            support.Draw();
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
            kickStainsPool.Draw();
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

        public void DrawCuts()
        {
            for (int i = 0; i < 5; i++)
            {
                int num = fingerCuts[i].Count;
                if (num > 0)
                {
                    float num2 = RTD(6.0);
                    float num3 = 1f;
                    int num4 = 0;
                    int j = 0;
                    Vector[] array = new Vector[num + 1];
                    int num5 = 0;
                    while (j < num)
                    {
                        FingerCut fingerCut = fingerCuts[i].ObjectAtIndex(j);
                        if (j == 0)
                        {
                            array[num5++] = fingerCut.start;
                        }
                        array[num5++] = fingerCut.end;
                        j++;
                    }
                    List<Vector> list = [];
                    Vector vector = default;
                    bool flag = true;
                    for (int k = 0; k < array.Length; k++)
                    {
                        if (k == 0)
                        {
                            list.Add(array[k]);
                        }
                        else if (array[k].X != vector.X || array[k].Y != vector.Y)
                        {
                            list.Add(array[k]);
                            flag = false;
                        }
                        vector = array[k];
                    }
                    if (!flag)
                    {
                        array = [.. list];
                        num = array.Length - 1;
                        int num6 = num * 2;
                        float[] array2 = new float[num6 * 2];
                        float num7 = 1f / num6;
                        float num8 = 0f;
                        int num9 = 0;
                        for (; ; )
                        {
                            if ((double)num8 > 1.0)
                            {
                                num8 = 1f;
                            }
                            Vector vector2 = DrawHelper.CalcPathBezier(array, num + 1, num8);
                            if (num9 > array2.Length - 2)
                            {
                                break;
                            }
                            array2[num9++] = vector2.X;
                            array2[num9++] = vector2.Y;
                            if ((double)num8 == 1.0)
                            {
                                break;
                            }
                            num8 += num7;
                        }
                        float num10 = num2 / num6;
                        float[] array3 = new float[num6 * 4];
                        for (int l = 0; l < num6 - 1; l++)
                        {
                            float s = num3;
                            float s2 = l == num6 - 2 ? 1f : num3 + num10;
                            Vector vector3 = Vect(array2[l * 2], array2[(l * 2) + 1]);
                            Vector vector8 = Vect(array2[(l + 1) * 2], array2[((l + 1) * 2) + 1]);
                            Vector vector9 = VectNormalize(VectSub(vector8, vector3));
                            Vector v4 = VectRperp(vector9);
                            Vector v5 = VectPerp(vector9);
                            if (num4 == 0)
                            {
                                Vector vector4 = VectAdd(vector3, VectMult(v4, s));
                                Vector vector5 = VectAdd(vector3, VectMult(v5, s));
                                array3[num4++] = vector5.X;
                                array3[num4++] = vector5.Y;
                                array3[num4++] = vector4.X;
                                array3[num4++] = vector4.Y;
                            }
                            Vector vector6 = VectAdd(vector8, VectMult(v4, s2));
                            Vector vector7 = VectAdd(vector8, VectMult(v5, s2));
                            array3[num4++] = vector7.X;
                            array3[num4++] = vector7.Y;
                            array3[num4++] = vector6.X;
                            array3[num4++] = vector6.Y;
                            num3 += num10;
                        }
                        Renderer.SetColor(Color.White);
                        int vertexCount = num4 / 2;
                        VertexPositionColor[] vertices = GetStripVertexCache(vertexCount);
                        int positionIndex = 0;
                        for (int vertex = 0; vertex < vertexCount; vertex++)
                        {
                            Vector3 position = new(array3[positionIndex++], array3[positionIndex++], 0f);
                            vertices[vertex] = new VertexPositionColor(position, Color.White);
                        }
                        Renderer.DrawTriangleStrip(vertices, vertexCount);
                    }
                }
            }
        }
    }
}
