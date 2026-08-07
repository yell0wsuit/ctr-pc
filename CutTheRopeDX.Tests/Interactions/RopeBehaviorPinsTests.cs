using System;
using System.Collections.Generic;
using System.Linq;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;

using Xunit;

using static CutTheRopeDX.Framework.Helpers.CTRMathHelper;

namespace CutTheRopeDX.Tests.Interactions
{
    /// <summary>
    /// Pins rope behavior as it is, measured rather than reasoned about, so a rope
    /// refactor can be checked against the original behavior instead of against intent.
    /// <para>
    /// Every number here was captured from a run, not derived. If one of these changes, the
    /// behavior changed - decide whether that was intended before editing the expectation.
    /// </para>
    /// <para>
    /// Headless never runs the draw path, so rendering is pinned through the sprite resources,
    /// quads, layer composition and visibility state that the draw path consumes. Two behaviors
    /// that live in <c>Draw</c> are pinned as headless sees them: a rope's <c>drawPts</c> stay empty,
    /// and a kicked suction cup's hook position never moves. Both are called out where they appear.
    /// </para>
    /// </summary>
    public class RopeBehaviorPinsTests
    {
        /// <summary>Decimal places compared on positions: tight enough to catch a real change.</summary>
        private const int Places = 3;

        private static Vector CandyPos(GameScene scene)
        {
            return scene.Candy().WholeBody.Point.pos;
        }

        private static void AssertPos(float x, float y, Vector actual)
        {
            Assert.Equal(x, actual.X, Places);
            Assert.Equal(y, actual.Y, Places);
        }

        private static IEnumerable<Image> DescendantImages(BaseElement parent)
        {
            foreach (BaseElement child in parent.GetChilds().Values)
            {
                if (child is Image image)
                {
                    yield return image;
                }

                foreach (Image descendant in DescendantImages(child))
                {
                    yield return descendant;
                }
            }
        }

        private static Image FindSprite(BaseElement parent, string resource, int quad)
        {
            return Assert.Single(DescendantImages(parent),
                image => image.GetType() == typeof(Image)
                    && ReferenceEquals(Application.GetTexture(resource), image.texture)
                    && image.quadToDraw == quad);
        }

        private static void AssertSprite(string resource, int quad, Image image)
        {
            Assert.Same(Application.GetTexture(resource), image.texture);
            Assert.Equal(quad, image.quadToDraw);
        }

        // ---------------------------------------------------------------- rendering decisions

        /// <summary>Fixed hooks choose their atlas from the rope material and keep paired quads.</summary>
        [Fact]
        public void FixedHookSpritesMatchTheRopeMaterial()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 200)
                .Grab(120, 60, length: 60, moveLength: -1f)
                .Grab(200, 60, length: 60, moveLength: -1f, breakable: false)
                .Build();

            Grab normal = scene.Grabs()[0];
            Grab chain = scene.Grabs()[1];

            Assert.Same(Application.GetTexture(Resources.Img.ObjHook), normal.back.texture);
            Assert.True(normal.back.quadToDraw is 0 or 2);
            AssertSprite(Resources.Img.ObjHook, normal.back.quadToDraw + 1, normal.front);
            Assert.False(normal.RopeOf().cutOnlyByAxe);

            AssertSprite(Resources.Img.ObjHookChain, 0, chain.back);
            AssertSprite(Resources.Img.ObjHookChain, 1, chain.front);
            Assert.True(chain.RopeOf().cutOnlyByAxe);
        }

        /// <summary>Automatic chain hooks use their dedicated atlas; ordinary ones use obj_hook.</summary>
        [Fact]
        public void AutomaticHookSpritesMatchTheRopeMaterial()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 400)
                .Grab(120, 60, radius: 30f, moveLength: -1f)
                .Grab(200, 60, radius: 30f, moveLength: -1f, breakable: false)
                .Build();

            Grab normal = scene.Grabs()[0];
            Grab chain = scene.Grabs()[1];

            AssertSprite(Resources.Img.ObjHook, 4, normal.back);
            AssertSprite(Resources.Img.ObjHook, 5, normal.front);
            AssertSprite(Resources.Img.ObjHookAutoChain, 0, chain.back);
            AssertSprite(Resources.Img.ObjHookAutoChain, 1, chain.front);
        }

        /// <summary>The wheel's manual draw layers use the four regulated-wheel quads.</summary>
        [Fact]
        public void WheelComposesTheRegulatedWheelSprites()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 200)
                .Grab(160, 120, length: 60, wheel: true, moveLength: -1f)
                .Build();

            Grab wheel = scene.Grabs()[0];
            Image wheelBase = FindSprite(wheel, Resources.Img.ObjHook, 11);
            Image arm = FindSprite(wheel, Resources.Img.ObjHook, 12);
            Image highlight = FindSprite(wheel, Resources.Img.ObjHook, 13);
            Image indicator = FindSprite(wheel, Resources.Img.ObjHook, 14);

            AssertSprite(Resources.Img.ObjHook, 11, wheelBase);
            AssertSprite(Resources.Img.ObjHook, 12, arm);
            AssertSprite(Resources.Img.ObjHook, 13, highlight);
            AssertSprite(Resources.Img.ObjHook, 14, indicator);
            Assert.False(wheel.back.visible);
            Assert.False(wheel.front.visible);
            Assert.False(wheelBase.visible);
            Assert.False(arm.visible);
        }

        /// <summary>A rail uses the tiled track and the idle/highlight movable-hook sprites.</summary>
        [Fact]
        public void RailComposesItsTrackAndMoverSprites()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 200)
                .Grab(160, 120, length: 60, moveLength: 80f)
                .Build();

            Grab rail = scene.Grabs()[0];
            HorizontallyTiledImage track = Assert.Single(DescendantImages(rail).OfType<HorizontallyTiledImage>());

            Assert.Same(Application.GetTexture(Resources.Img.ObjHook), track.texture);
            Assert.Equal(6, track.tiles[0]);
            Assert.Equal(8, track.tiles[1]);
            Assert.Equal(7, track.tiles[2]);
            AssertSprite(Resources.Img.ObjHook, 9, FindSprite(rail, Resources.Img.ObjHook, 9));
            AssertSprite(Resources.Img.ObjHook, 10, FindSprite(rail, Resources.Img.ObjHook, 10));
            Assert.False(track.visible);
        }

        /// <summary>A moving-path hook adds the bee body and animated wings from obj_bee.</summary>
        [Fact]
        public void BeeComposesItsBodyAndWingSprites()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 200)
                .Grab(160, 60, length: 70, path: "60,0", moveSpeed: 40f, moveLength: -1f)
                .Build();

            Grab bee = scene.Grabs()[0];
            Image body = FindSprite(bee, Resources.Img.ObjBee, 1);
            Animation wings = Assert.Single(DescendantImages(body).OfType<Animation>());

            AssertSprite(Resources.Img.ObjBee, 1, body);
            Assert.Same(Application.GetTexture(Resources.Img.ObjBee), wings.texture);
            Assert.InRange(wings.quadToDraw, 2, 4);
        }

        /// <summary>The gun owns back, arrow, front and cup layers, and swaps its front when fired.</summary>
        [Fact]
        public void GunComposesItsLayersAndSwitchesToTheFiredFront()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 260)
                .Grab(160, 120, gun: true, moveLength: -1f)
                .Build();

            Grab gun = scene.Grabs()[0];
            Image back = FindSprite(gun, Resources.Img.ObjGun, 0);
            Image arrow = FindSprite(gun, Resources.Img.ObjGun, 1);
            Image front = FindSprite(gun, Resources.Img.ObjGun, 2);
            Image cup = Assert.Single(DescendantImages(gun).OfType<Animation>(),
                image => ReferenceEquals(Application.GetTexture(Resources.Img.ObjGun), image.texture));

            AssertSprite(Resources.Img.ObjGun, 0, back);
            AssertSprite(Resources.Img.ObjGun, 1, arrow);
            AssertSprite(Resources.Img.ObjGun, 2, front);
            AssertSprite(Resources.Img.ObjGun, 0, cup);
            Assert.False(back.visible);
            Assert.False(arrow.visible);
            Assert.False(front.visible);
            Assert.False(cup.visible);

            _ = scene.TouchDownXYIndex((int)gun.x, (int)gun.y, 0);

            AssertSprite(Resources.Img.ObjGun, 3, front);
        }

        /// <summary>A suction cup swaps its mounted sprite pair for the kicked pair.</summary>
        [Fact]
        public void SuctionCupSpritesTrackItsMountedState()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 200)
                .Grab(160, 120, length: 60, kickable: true, moveLength: -1f)
                .Build();

            Grab cup = scene.Grabs()[0];
            AssertSprite(Resources.Img.ObjSticker, 3, cup.back);
            AssertSprite(Resources.Img.ObjSticker, 4, cup.front);

            _ = scene.TouchDownXYIndex((int)cup.x, (int)cup.y, 0);
            _ = scene.TouchUpXYIndex((int)cup.x, (int)cup.y, 0);

            AssertSprite(Resources.Img.ObjSticker, 1, cup.back);
            AssertSprite(Resources.Img.ObjSticker, 2, cup.front);
        }

        // ---------------------------------------------------------------- hanging candy

        /// <summary>
        /// The base case the whole system rests on: a candy on one rope falls, is caught, and
        /// settles. The three samples pin the shape of that curve, not just its end.
        /// </summary>
        [Fact]
        public void CandyOnOneRopeFallsAndSettles()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 120)
                .Grab(160, 60, length: 40, moveLength: -1f)
                .Build();

            HeadlessGame.StepFrames(scene, 1);
            AssertPos(1280f, 364.95117f, CandyPos(scene));

            HeadlessGame.StepFrames(scene, 29);
            AssertPos(1280f, 376.6111f, CandyPos(scene));

            HeadlessGame.StepFrames(scene, 90);
            AssertPos(1280f, 377.20508f, CandyPos(scene));

            Assert.Equal(1, scene.AttachedRopeCount(scene.Candy()));
        }

        /// <summary>Two ropes pulling from opposite sides settle the candy between them.</summary>
        [Fact]
        public void CandyOnTwoRopesSettlesBetweenThem()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 160)
                .Grab(100, 60, length: 60, moveLength: -1f)
                .Grab(220, 60, length: 60, moveLength: -1f)
                .Build();

            HeadlessGame.StepFrames(scene, 120);

            AssertPos(1272.3008f, 392.75272f, CandyPos(scene));
            Assert.Equal(2, scene.AttachedRopeCount(scene.Candy()));
        }

        /// <summary>A hanging rope's segment count, rest length and endpoints.</summary>
        [Fact]
        public void HangingRopeGeometryIsStable()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 200)
                .Grab(160, 60, length: 60, moveLength: -1f)
                .Build();

            Bungee rope = scene.Grabs()[0].RopeOf();

            HeadlessGame.StepFrames(scene, 1);
            Assert.Equal(4, rope.parts.Count);
            Assert.Equal(369, rope.GetLength());

            HeadlessGame.StepFrames(scene, 59);
            Assert.Equal(4, rope.parts.Count);
            Assert.Equal(286, rope.GetLength());
            Assert.Equal(0, rope.relaxed);

            // The anchor of a fixed hook never moves.
            AssertPos(1280f, 180f, rope.bungeeAnchor.pos);
            AssertPos(1280f, 467.26245f, rope.tail.pos);

            // drawPts are filled by the draw pass, which headless never runs.
            Assert.Equal(0, rope.drawPtsCount);
        }

        // ---------------------------------------------------------------- cutting

        /// <summary>Cutting the only rope drops the candy, and the drop rate is pinned.</summary>
        [Fact]
        public void CuttingTheOnlyRopeDropsTheCandy()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 120)
                .Grab(160, 60, length: 40, moveLength: -1f)
                .Build();

            HeadlessGame.StepFrames(scene, 30);
            Vector beforeCut = CandyPos(scene);
            AssertPos(1280f, 376.6111f, beforeCut);

            float midY = (beforeCut.Y + Scenario.WorldY(60)) / 2f;
            int cuts = scene.CutWithRazorOrLine1Line2Immediate(
                null, Vect(beforeCut.X - 200f, midY), Vect(beforeCut.X + 200f, midY), false);

            Assert.Equal(1, cuts);
            Assert.Equal(0, scene.AttachedRopeCount(scene.Candy()));

            HeadlessGame.StepFrames(scene, 30);
            AssertPos(1280f, 558.25903f, CandyPos(scene));
        }

        /// <summary>
        /// One stroke cuts at most one rope, even when it crosses two. The caller repeats the sweep
        /// per frame; the per-call cap is what stops a single drag shearing a whole level.
        /// </summary>
        [Fact]
        public void OneStrokeCutsAtMostOneRope()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 160)
                .Grab(100, 60, length: 60, moveLength: -1f)
                .Grab(220, 60, length: 60, moveLength: -1f)
                .Build();

            HeadlessGame.StepFrames(scene, 30);
            float midY = (CandyPos(scene).Y + Scenario.WorldY(60)) / 2f;

            int cuts = scene.CutWithRazorOrLine1Line2Immediate(
                null, Vect(0f, midY), Vect(3000f, midY), false);

            Assert.Equal(1, cuts);
            Assert.Equal(1, scene.AttachedRopeCount(scene.Candy()));
        }

        /// <summary>A rope that is already cut is not cut a second time by the same stroke.</summary>
        [Fact]
        public void AnAlreadyCutRopeIsNotCutAgain()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 120)
                .Grab(160, 60, length: 40, moveLength: -1f)
                .Build();

            HeadlessGame.StepFrames(scene, 30);
            float midY = (CandyPos(scene).Y + Scenario.WorldY(60)) / 2f;

            Assert.Equal(1, scene.CutWithRazorOrLine1Line2Immediate(
                null, Vect(0f, midY), Vect(3000f, midY), false));
            Assert.Equal(0, scene.CutWithRazorOrLine1Line2Immediate(
                null, Vect(0f, midY), Vect(3000f, midY), false));
        }

        /// <summary>Releasing the candy cuts every rope holding it, each at its tail segment.</summary>
        [Fact]
        public void ReleasingTheCandyCutsEveryRopeAtItsTail()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 160)
                .Grab(100, 60, length: 60, moveLength: -1f)
                .Grab(220, 60, length: 60, moveLength: -1f)
                .Build();

            HeadlessGame.StepFrames(scene, 20);
            Assert.Equal(2, scene.AttachedRopeCount(scene.Candy()));

            scene.ReleaseRopesForPoint(scene.Candy().WholeBody.Point);

            Assert.Equal(0, scene.AttachedRopeCount(scene.Candy()));
            Assert.Equal(2, scene.Grabs()[0].RopeOf().cut);
            Assert.Equal(2, scene.Grabs()[1].RopeOf().cut);
        }

        // ---------------------------------------------------------------- rope sources

        /// <summary>A radius hook attaches on the first frame the candy is in range, exactly once.</summary>
        [Fact]
        public void RadiusHookAttachesOnceOnTheFirstFrameInRange()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 200)
                .Grab(160, 120, radius: 90f, moveLength: -1f)
                .Build();

            HeadlessGame.StepFrames(scene, 1);
            Assert.Equal(1, scene.AttachedRopeCount(scene.Candy()));

            HeadlessGame.StepFrames(scene, 60);
            Assert.Equal(1, scene.AttachedRopeCount(scene.Candy()));
            AssertPos(1280f, 569.65216f, CandyPos(scene));
        }

        /// <summary>A gun holds no rope until tapped, then holds exactly one of a pinned length.</summary>
        [Fact]
        public void GunCreatesOneRopeOnTap()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 260)
                .Grab(160, 120, gun: true, moveLength: -1f)
                .Build();

            HeadlessGame.StepFrames(scene, 5);
            Grab gun = scene.Grabs()[0];
            Assert.Null(gun.RopeOf());

            _ = scene.TouchDownXYIndex((int)gun.x, (int)gun.y, 0);

            Assert.NotNull(gun.RopeOf());
            Assert.Equal(425, gun.RopeOf().GetLength());

            HeadlessGame.StepFrames(scene, 60);
            AssertPos(1280f, 778.813f, CandyPos(scene));
        }

        // ---------------------------------------------------------------- moving anchors

        /// <summary>A bee carries its rope anchor with it, exactly, every frame.</summary>
        [Fact]
        public void BeeCarriesItsRopeAnchor()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 200)
                .Grab(160, 60, length: 70, path: "60,0", moveSpeed: 40f, moveLength: -1f)
                .Build();

            Grab bee = scene.Grabs()[0];
            HeadlessGame.StepFrames(scene, 60);

            Assert.Equal(1406.7236f, bee.x, Places);
            Assert.Equal(180f, bee.y, Places);
            AssertPos(bee.x, bee.y, bee.RopeOf().bungeeAnchor.pos);
            AssertPos(1391.1919f, 118.94437f, CandyPos(scene));
        }

        /// <summary>Dragging a rail hook slides it and takes its rope anchor along.</summary>
        [Fact]
        public void RailDragSlidesTheHookAndItsAnchor()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 200)
                .Grab(160, 60, length: 70, moveLength: 80f)
                .Build();

            Grab rail = scene.Grabs()[0];
            HeadlessGame.StepFrames(scene, 20);
            Assert.Equal(1280f, rail.x, Places);

            int rx = (int)rail.x;
            int ry = (int)rail.y;
            _ = scene.TouchDownXYIndex(rx, ry, 0);
            _ = scene.TouchMoveXYIndex(rx + 150, ry, 0);
            _ = scene.TouchUpXYIndex(rx + 150, ry, 0);

            Assert.Equal(1430f, rail.x, Places);
            AssertPos(1430f, 180f, rail.RopeOf().bungeeAnchor.pos);

            HeadlessGame.StepFrames(scene, 60);
            AssertPos(1527.6074f, 491.27545f, CandyPos(scene));
        }

        // ---------------------------------------------------------------- wheel

        /// <summary>Spinning a wheel reels its rope out, adding a segment.</summary>
        [Fact]
        public void SpinningAWheelAddsARopeSegment()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 200)
                .Grab(160, 120, length: 60, wheel: true, moveLength: -1f)
                .Build();

            HeadlessGame.StepFrames(scene, 30);
            Grab wheel = scene.Grabs()[0];
            int wx = (int)wheel.x;
            int wy = (int)wheel.y;

            Assert.True(scene.TouchDownXYIndex(wx + 80, wy, 0), "the wheel did not claim the touch");
            Assert.Equal(4, wheel.RopeOf().parts.Count);

            for (int i = 1; i <= 24; i++)
            {
                double a = i * (Math.PI / 12.0);
                _ = scene.TouchMoveXYIndex(wx + (int)(80 * Math.Cos(a)), wy + (int)(80 * Math.Sin(a)), 0);
            }

            Assert.Equal(5, wheel.RopeOf().parts.Count);

            _ = scene.TouchUpXYIndex(wx + 80, wy, 0);
        }

        // ---------------------------------------------------------------- suction cup

        /// <summary>
        /// Tapping a stuck cup unpins its rope anchor, which then falls under the candy's weight.
        /// <para>
        /// The hook's own position deliberately does <b>not</b> follow here: on this build the
        /// anchor-to-hook sync lives in the draw pass, which headless never runs. A refactor that
        /// moves that sync into <c>Update</c> will fail this test - that is the point of pinning it.
        /// </para>
        /// </summary>
        [Fact]
        public void KickedCupAnchorFallsWhileTheHookPositionStaysPutHeadless()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 200)
                .Grab(160, 120, length: 60, kickable: true, moveLength: -1f)
                .Build();

            HeadlessGame.StepFrames(scene, 10);
            Grab cup = scene.Grabs()[0];
            Assert.Equal(360f, cup.RopeOf().bungeeAnchor.pos.Y, Places);

            _ = scene.TouchDownXYIndex((int)cup.x, (int)cup.y, 0);
            _ = scene.TouchUpXYIndex((int)cup.x, (int)cup.y, 0);

            HeadlessGame.StepFrames(scene, 60);

            Assert.Equal(648.2077f, cup.RopeOf().bungeeAnchor.pos.Y, Places);
            Assert.Equal(360f, cup.y, Places);
        }
    }
}
