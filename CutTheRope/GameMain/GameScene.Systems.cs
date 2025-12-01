using System;

using CutTheRope.Framework.Core;
using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Sfe;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Handles pump physics interaction with a constrained point and game object
        /// </summary>
        public static void HandlePumpFlowPtSkin(Pump p, ConstraintedPoint s, GameObject c)
        {
            float num = 624f;
            if (GameObject.RectInObject(p.x - num, p.y - num, p.x + num, p.y + num, c))
            {
                Vector v = Vect(c.x, c.y);
                Vector vector = default;
                vector.x = p.x - (p.bb.w / 2f);
                Vector vector2 = default;
                vector2.x = p.x + (p.bb.w / 2f);
                vector.y = vector2.y = p.y;
                if (p.angle != 0.0)
                {
                    v = VectRotateAround(v, 0.0 - p.angle, p.x, p.y);
                }
                if (v.y < vector.y && RectInRect((float)(v.x - (c.bb.w / 2.0)), (float)(v.y - (c.bb.h / 2.0)), (float)(v.x + (c.bb.w / 2.0)), (float)(v.y + (c.bb.h / 2.0)), vector.x, vector.y - num, vector2.x, vector2.y))
                {
                    float num2 = num * 2f * (num - (vector.y - v.y)) / num;
                    Vector v2 = Vect(0f, 0f - num2);
                    v2 = VectRotate(v2, p.angle);
                    s.ApplyImpulseDelta(v2, 0.016f);
                }
            }
        }

        /// <summary>
        /// Handles bouncer physics interaction with a constrained point
        /// </summary>
        public static void HandleBouncePtDelta(Bouncer b, ConstraintedPoint s, float delta)
        {
            if (!b.skip)
            {
                b.skip = true;
                Vector vector = VectSub(s.prevPos, s.pos);
                int num = VectRotateAround(s.prevPos, (double)(0f - b.angle), b.x, b.y).y >= b.y ? 1 : -1;
                float s2 = MAX((double)(VectLength(vector) * 40f), 840.0) * num;
                Vector impulse = VectMult(VectPerp(VectForAngle(b.angle)), s2);
                s.pos = VectRotateAround(s.pos, (double)(0f - b.angle), b.x, b.y);
                s.prevPos = VectRotateAround(s.prevPos, (double)(0f - b.angle), b.x, b.y);
                s.prevPos.y = s.pos.y;
                s.pos = VectRotateAround(s.pos, b.angle, b.x, b.y);
                s.prevPos = VectRotateAround(s.prevPos, b.angle, b.x, b.y);
                s.ApplyImpulseDelta(impulse, delta);
                b.PlayTimeline(0);
                CTRSoundMgr.PlaySound(Resources.Snd.Bouncer);
            }
        }

        /// <summary>
        /// Applies steam tube forces and interacts with candy pieces inside the flow area.
        /// PC vs WP7 differences:
        /// - num3 (tube width): 10f * tubeScale (WP7: 10f unscaled)
        /// - num4 (vertical offset): 1f * tubeScale (WP7: 1f unscaled)
        /// - num5 (collision radius): 17.5f * tubeScale (WP7: 17.5f unscaled)
        /// - Gravity force: -32f/weight * sqrt(tubeScale) (WP7: no sqrt scaling)
        /// - Damping factor (num): Always 5f (same in both)
        /// </summary>
        public void OperateSteamTube(SteamTube tube)
        {
            float tubeScale = tube.GetHeightScale();
            float num = 5f;  // Damping factor (velocity reduction)
            float num2 = DEGREES_TO_RADIANS(tube.rotation);
            float num3 = 10f * tubeScale;  // Tube width for horizontal centering
            float currentHeightModulated = tube.GetCurrentHeightModulated();
            float num4 = 1f * tubeScale;  // Vertical offset for collision box
            float num5 = 17.5f * tubeScale;  // Candy collision radius (STAR_RADIUS scaled)
            Vector vector = Vect(tube.x - (num3 / 2f), tube.y - currentHeightModulated - num4);
            Vector vector2 = Vect(tube.x + (num3 / 2f), tube.y - num5);
            if (twoParts == 2)
            {
                Vector vector3 = Vect(star.pos.x, star.pos.y);
                Vector vector4 = Vect(star.v.x, star.v.y);
                vector3 = VectRotateAround(vector3, 0.0 - num2, tube.x, tube.y);
                vector4 = VectRotate(vector4, 0.0 - num2);
                if (RectInRect(vector3.x - num5, vector3.y - (num5 / 2f), vector3.x + num5, vector3.y + num5, vector.x, vector.y, vector2.x, vector2.y))
                {
                    foreach (Bouncer bouncer in bouncers)
                    {
                        bouncer.skip = false;
                    }
                    float num6 = 0f;
                    if (tube.rotation == 0f)
                    {
                        float num7 = tube.x - vector3.x;
                        num6 = ABS(num7) > num3 / 4f
                            ? ((0f - vector4.x) / num) + (0.25f * num7)
                            : ABS(vector4.x) < 1f ? 0f - vector4.x : (0f - vector4.x) / num;
                    }
                    // Gravity compensation force. sqrt(tubeScale) accounts for increased flow area.
                    // WP7: -32f / star.weight (no sqrt scaling)
                    float num8 = -32f / star.weight * MathF.Sqrt(tubeScale);
                    if (tube.rotation != 0f)
                    {
                        num *= 15f;
                        if (tube.rotation == 180f)
                        {
                            num8 /= 2f;
                        }
                        else
                        {
                            num8 /= 4f;
                        }
                    }
                    Vector vector5 = Vect(num6, ((0f - vector4.y) / num) + num8);
                    float num9 = tube.y - vector3.y;
                    if (num9 > currentHeightModulated + num5)
                    {
                        vector5 = VectMult(vector5, Math.Exp((double)(-2f * (num9 - (currentHeightModulated + num5)))));
                    }
                    vector5 = VectRotate(vector5, num2);
                    star.ApplyImpulseDelta(vector5, 0.016f);
                    return;
                }
            }
            else
            {
                Vector vector6 = Vect(starL.pos.x, starL.pos.y);
                Vector vector7 = Vect(starL.v.x, starL.v.y);
                vector6 = VectRotateAround(vector6, 0.0 - num2, tube.x, tube.y);
                vector7 = VectRotate(vector7, 0.0 - num2);
                if (RectInRect(vector6.x - num5, vector6.y - (num5 / 2f), vector6.x + num5, vector6.y + num5, vector.x, vector.y, vector2.x, vector2.y))
                {
                    foreach (Bouncer bouncer2 in bouncers)
                    {
                        bouncer2.skip = false;
                    }
                    float num10 = 0f;
                    if (tube.rotation == 0f)
                    {
                        float num11 = tube.x - vector6.x;
                        num10 = ABS(num11) > num3 / 4f
                            ? ((0f - vector7.x) / num) + (0.25f * num11)
                            : ABS(vector7.x) < 1f ? 0f - vector7.x : (0f - vector7.x) / num;
                    }
                    // Gravity compensation force (left candy piece)
                    float num12 = -32f / starL.weight * MathF.Sqrt(tubeScale);
                    if (tube.rotation != 0f)
                    {
                        num *= 15f;
                        if (tube.rotation == 180f)
                        {
                            num12 /= 2f;
                        }
                        else
                        {
                            num12 /= 4f;
                        }
                    }
                    Vector vector8 = Vect(num10, ((0f - vector7.y) / num) + num12);
                    float num13 = tube.y - vector6.y;
                    if (num13 > currentHeightModulated + num5)
                    {
                        vector8 = VectMult(vector8, Math.Exp((double)(-2f * (num13 - (currentHeightModulated + num5)))));
                    }
                    vector8 = VectRotate(vector8, num2);
                    starL.ApplyImpulseDelta(vector8, 0.016f);
                }
                vector6 = Vect(starR.pos.x, starR.pos.y);
                vector7 = Vect(starR.v.x, starR.v.y);
                vector6 = VectRotateAround(vector6, 0.0 - num2, tube.x, tube.y);
                vector7 = VectRotate(vector7, 0.0 - num2);
                if (RectInRect(vector6.x - num5, vector6.y - (num5 / 2f), vector6.x + num5, vector6.y + num5, vector.x, vector.y, vector2.x, vector2.y))
                {
                    foreach (Bouncer bouncer3 in bouncers)
                    {
                        bouncer3.skip = false;
                    }
                    float num14 = 0f;
                    if (tube.rotation == 0f)
                    {
                        float num15 = tube.x - vector6.x;
                        num14 = ABS(num15) > num3 / 4f
                            ? ((0f - vector7.x) / num) + (0.25f * num15)
                            : ABS(vector7.x) < 1f ? 0f - vector7.x : (0f - vector7.x) / num;
                    }
                    // Gravity compensation force (right candy piece)
                    float num16 = -32f / starR.weight * MathF.Sqrt(tubeScale);
                    if (tube.rotation != 0f)
                    {
                        num *= 15f;
                        if (tube.rotation == 180f)
                        {
                            num16 /= 2f;
                        }
                        else
                        {
                            num16 /= 4f;
                        }
                    }
                    Vector vector9 = Vect(num14, ((0f - vector7.y) / num) + num16);
                    float num17 = tube.y - vector6.y;
                    if (num17 > currentHeightModulated + num5)
                    {
                        vector9 = VectMult(vector9, Math.Exp((double)(-2f * (num17 - (currentHeightModulated + num5)))));
                    }
                    vector9 = VectRotate(vector9, num2);
                    starR.ApplyImpulseDelta(vector9, 0.016f);
                }
            }
        }

        /// <summary>
        /// Operates a pump - creates particles and applies force
        /// </summary>
        public void OperatePump(Pump p)
        {
            p.PlayTimeline(0);
            CTRSoundMgr.PlayRandomSound(Resources.Snd.Pump1, Resources.Snd.Pump2, Resources.Snd.Pump3, Resources.Snd.Pump4);
            Image grid = Image.Image_createWithResID(Resources.Img.ObjPump);
            PumpDirt pumpDirt = new PumpDirt().InitWithTotalParticlesAngleandImageGrid(5, RADIANS_TO_DEGREES((float)p.angle) - 90f, grid);
            pumpDirt.particlesDelegate = new Particles.ParticlesFinished(aniPool.ParticlesFinished);
            Vector v = Vect(p.x + 80f, p.y);
            v = VectRotateAround(v, p.angle - 1.5707963267948966, p.x, p.y);
            pumpDirt.x = v.x;
            pumpDirt.y = v.y;
            pumpDirt.StartSystem(5);
            _ = aniPool.AddChild(pumpDirt);
            if (!noCandy)
            {
                HandlePumpFlowPtSkin(p, star, candy);
            }
            if (twoParts != 2)
            {
                if (!noCandyL)
                {
                    HandlePumpFlowPtSkin(p, starL, candyL);
                }
                if (!noCandyR)
                {
                    HandlePumpFlowPtSkin(p, starR, candyR);
                }
            }
        }

        /// <summary>
        /// Cuts ropes with a razor or line. Returns number of ropes cut.
        /// </summary>
        public int CutWithRazorOrLine1Line2Immediate(Razor r, Vector v1, Vector v2, bool im)
        {
            int num = 0;
            for (int i = 0; i < bungees.Count; i++)
            {
                Grab grab = bungees.ObjectAtIndex(i);
                Bungee rope = grab.rope;
                if (rope != null && rope.cut == -1)
                {
                    for (int j = 0; j < rope.parts.Count - 1; j++)
                    {
                        ConstraintedPoint constraintedPoint = rope.parts[j];
                        ConstraintedPoint constraintedPoint2 = rope.parts[j + 1];
                        bool flag = false;
                        if (r == null)
                        {
                            flag = (!grab.wheel || !LineInRect(v1.x, v1.y, v2.x, v2.y, grab.x - 110f, grab.y - 110f, 220f, 220f)) && LineInLine(v1.x, v1.y, v2.x, v2.y, constraintedPoint.pos.x, constraintedPoint.pos.y, constraintedPoint2.pos.x, constraintedPoint2.pos.y);
                        }
                        else if (constraintedPoint.prevPos.x != 2.1474836E+09f)
                        {
                            float num2 = MinOf4(constraintedPoint.pos.x, constraintedPoint.prevPos.x, constraintedPoint2.pos.x, constraintedPoint2.prevPos.x);
                            float y1t = MinOf4(constraintedPoint.pos.y, constraintedPoint.prevPos.y, constraintedPoint2.pos.y, constraintedPoint2.prevPos.y);
                            float x1r = MaxOf4(constraintedPoint.pos.x, constraintedPoint.prevPos.x, constraintedPoint2.pos.x, constraintedPoint2.prevPos.x);
                            float y1b = MaxOf4(constraintedPoint.pos.y, constraintedPoint.prevPos.y, constraintedPoint2.pos.y, constraintedPoint2.prevPos.y);
                            flag = RectInRect(num2, y1t, x1r, y1b, r.drawX, r.drawY, r.drawX + r.width, r.drawY + r.height);
                        }
                        if (flag)
                        {
                            num++;
                            if (grab.hasSpider && grab.spiderActive)
                            {
                                SpiderBusted(grab);
                            }
                            string ropeSound = rope.relaxed switch
                            {
                                0 => Resources.Snd.RopeBleak1,
                                1 => Resources.Snd.RopeBleak2,
                                2 => Resources.Snd.RopeBleak3,
                                _ => Resources.Snd.RopeBleak4
                            };
                            CTRSoundMgr.PlaySound(ropeSound);
                            rope.SetCut(j);
                            if (im)
                            {
                                rope.cutTime = 0f;
                                rope.RemovePart(j);
                            }
                            return num;
                        }
                    }
                }
            }
            return num;
        }

        /// <summary>
        /// Called when a spider is busted - handles animation and achievements
        /// </summary>
        public void SpiderBusted(Grab g)
        {
            int num = Preferences.GetIntForKey("PREFS_SPIDERS_BUSTED") + 1;
            Preferences.SetIntForKey(num, "PREFS_SPIDERS_BUSTED", false);
            if (num == 40)
            {
                CTRRootController.PostAchievementName("681486608", ACHIEVEMENT_STRING("\"Spider Busted\""));
            }
            if (num == 200)
            {
                CTRRootController.PostAchievementName("1058341284", ACHIEVEMENT_STRING("\"Spider Tammer\""));
            }
            CTRSoundMgr.PlaySound(Resources.Snd.SpiderFall);
            g.hasSpider = false;
            Image image = Image.Image_createWithResIDQuad(Resources.Img.ObjSpider, 11);
            image.DoRestoreCutTransparency();
            Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(3);
            if (gravityButton != null && !gravityNormal)
            {
                timeline.AddKeyFrame(KeyFrame.MakePos(g.spider.x, g.spider.y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.0));
                timeline.AddKeyFrame(KeyFrame.MakePos(g.spider.x, g.spider.y + 50.0, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.3));
                timeline.AddKeyFrame(KeyFrame.MakePos(g.spider.x, (double)(g.spider.y - SCREEN_HEIGHT), KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 1.0));
            }
            else
            {
                timeline.AddKeyFrame(KeyFrame.MakePos(g.spider.x, g.spider.y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.0));
                timeline.AddKeyFrame(KeyFrame.MakePos(g.spider.x, g.spider.y - 50.0, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.3));
                timeline.AddKeyFrame(KeyFrame.MakePos(g.spider.x, (double)(g.spider.y + SCREEN_HEIGHT), KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 1.0));
            }
            timeline.AddKeyFrame(KeyFrame.MakeRotation(0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline.AddKeyFrame(KeyFrame.MakeRotation(RND_RANGE(-120, 120), KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 1.0));
            image.AddTimelinewithID(timeline, 0);
            image.PlayTimeline(0);
            image.x = g.spider.x;
            image.y = g.spider.y;
            image.anchor = 18;
            timeline.delegateTimelineDelegate = aniPool;
            _ = aniPool.AddChild(image);
        }

        /// <summary>
        /// Called when a spider successfully captures the candy
        /// </summary>
        public void SpiderWon(Grab sg)
        {
            CTRSoundMgr.PlaySound(Resources.Snd.SpiderWin);
            int num = bungees.Count;
            for (int i = 0; i < num; i++)
            {
                Grab grab = bungees.ObjectAtIndex(i);
                Bungee rope = grab.rope;
                if (rope != null && rope.tail == star)
                {
                    if (rope.cut == -1)
                    {
                        rope.SetCut(rope.parts.Count - 2);
                        rope.forceWhite = false;
                    }
                    if (grab.hasSpider && grab.spiderActive && sg != grab)
                    {
                        SpiderBusted(grab);
                    }
                }
            }
            sg.hasSpider = false;
            // spiderTookCandy = true;
            noCandy = true;
            Image image = Image.Image_createWithResIDQuad(Resources.Img.ObjSpider, 12);
            image.DoRestoreCutTransparency();
            candy.anchor = candy.parentAnchor = 18;
            candy.x = 0f;
            candy.y = -5f;
            _ = image.AddChild(candy);
            Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(3);
            if (gravityButton != null && !gravityNormal)
            {
                timeline.AddKeyFrame(KeyFrame.MakePos(sg.spider.x, sg.spider.y - 10.0, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.0));
                timeline.AddKeyFrame(KeyFrame.MakePos(sg.spider.x, sg.spider.y + 70.0, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.3));
                timeline.AddKeyFrame(KeyFrame.MakePos(sg.spider.x, (double)(sg.spider.y - SCREEN_HEIGHT), KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 1.0));
            }
            else
            {
                timeline.AddKeyFrame(KeyFrame.MakePos(sg.spider.x, sg.spider.y - 10.0, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.0));
                timeline.AddKeyFrame(KeyFrame.MakePos(sg.spider.x, sg.spider.y - 70.0, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.3));
                timeline.AddKeyFrame(KeyFrame.MakePos(sg.spider.x, (double)(sg.spider.y + SCREEN_HEIGHT), KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 1.0));
            }
            image.AddTimelinewithID(timeline, 0);
            image.PlayTimeline(0);
            image.x = sg.spider.x;
            image.y = sg.spider.y - 10f;
            image.anchor = 18;
            timeline.delegateTimelineDelegate = aniPool;
            _ = aniPool.AddChild(image);
            if (restartState != 0)
            {
                dd.CallObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(Selector_gameLost), null, 2.0);
            }
        }

        /// <summary>
        /// Finds the nearest bungee segment using bezier drawing points
        /// </summary>
        public Bungee GetNearestBungeeSegmentByBeziersPointsatXYgrab(ref Vector s, float tx, float ty, ref Grab grab)
        {
            float num = 60f;
            Bungee result = null;
            float num2 = num;
            Vector v = Vect(tx, ty);
            for (int i = 0; i < bungees.Count; i++)
            {
                Grab grab2 = bungees.ObjectAtIndex(i);
                Bungee rope = grab2.rope;
                if (rope != null)
                {
                    for (int j = 0; j < rope.drawPtsCount; j += 2)
                    {
                        Vector vector = Vect(rope.drawPts[j], rope.drawPts[j + 1]);
                        float num3 = VectDistance(vector, v);
                        if (num3 < num && num3 < num2)
                        {
                            num2 = num3;
                            result = rope;
                            s = vector;
                            grab = grab2;
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Finds the nearest bungee segment using constraint points
        /// </summary>
        public static Bungee GetNearestBungeeSegmentByConstraintsforGrab(ref Vector s, Grab g)
        {
            float num4 = 2.1474836E+09f;
            Bungee result = null;
            float num2 = num4;
            Vector v = s;
            Bungee rope = g.rope;
            if (rope == null || rope.cut != -1)
            {
                return null;
            }
            for (int i = 0; i < rope.parts.Count - 1; i++)
            {
                ConstraintedPoint constraintedPoint = rope.parts[i];
                float num3 = VectDistance(constraintedPoint.pos, v);
                if (num3 < num2 && (!g.wheel || !PointInRect(constraintedPoint.pos.x, constraintedPoint.pos.y, g.x - 110f, g.y - 110f, 220f, 220f)))
                {
                    num2 = num3;
                    result = rope;
                    s = constraintedPoint.pos;
                }
            }
            return result;
        }
    }
}
