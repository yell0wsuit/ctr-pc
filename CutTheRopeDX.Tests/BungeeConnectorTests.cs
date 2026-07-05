using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

using CutTheRopeDX.Desktop;
using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Physics;
using CutTheRopeDX.GameMain;

using Xunit;

using static CutTheRopeDX.Framework.Helpers.CTRMathHelper;

namespace CutTheRopeDX.Tests
{
    public class BungeeConnectorTests
    {
        private static ConstraintedPoint PointAt(float x, float y, float weight)
        {
            ConstraintedPoint p = new();
            p.SetWeight(weight);
            p.pos = Vect(x, y);
            return p;
        }

        private static GameScene SceneWithConnector(Bungee connector)
        {
            GameScene scene = (GameScene)RuntimeHelpers.GetUninitializedObject(typeof(GameScene));
            typeof(GameScene).GetField("bungees", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(scene, new List<Grab>());
            typeof(GameScene).GetField("candyConnector", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(scene, connector);
            return scene;
        }

        [Fact]
        public void Init_PreservesHeadWeight_WhenHeadPassedIn()
        {
            ConstraintedPoint head = PointAt(100f, 100f, 1f);
            ConstraintedPoint tail = PointAt(100f, 160f, 1f);

            _ = new Bungee().InitWithHeadAtXYTailAtTXTYandLength(
                head, head.pos.X, head.pos.Y, tail, tail.pos.X, tail.pos.Y, 60f);

            Assert.Equal(1f, head.weight);
        }

        [Fact]
        public void Init_SetsAnchorWeight_WhenHeadAutoCreated()
        {
            ConstraintedPoint tail = PointAt(100f, 160f, 1f);

            Bungee bungee = new Bungee().InitWithHeadAtXYTailAtTXTYandLength(
                null, 100f, 100f, tail, tail.pos.X, tail.pos.Y, 60f);

            Assert.Equal(0.02f, bungee.bungeeAnchor.weight);
        }

        [Fact]
        public void Update_SkipsBothCandyEnds_WhenHeadNotOwned()
        {
            ConstraintedPoint head = PointAt(100f, 100f, 1f);
            ConstraintedPoint tail = PointAt(100f, 220f, 1f);
            Bungee connector = new Bungee().InitWithHeadAtXYTailAtTXTYandLength(
                head, head.pos.X, head.pos.Y, tail, tail.pos.X, tail.pos.Y, 60f);

            connector.Update(0.016f, 1f);

            Assert.Equal(UNDEFINED_COORDINATE, head.prevPos.X);
            Assert.Equal(UNDEFINED_COORDINATE, tail.prevPos.X);
            Assert.NotEqual(UNDEFINED_COORDINATE, connector.parts[1].prevPos.X);
        }

        [Fact]
        public void Update_IntegratesHead_WhenOwned()
        {
            ConstraintedPoint tail = PointAt(100f, 220f, 1f);
            Bungee grabRope = new Bungee().InitWithHeadAtXYTailAtTXTYandLength(
                null, 100f, 100f, tail, tail.pos.X, tail.pos.Y, 60f);

            grabRope.Update(0.016f, 1f);

            Assert.NotEqual(UNDEFINED_COORDINATE, grabRope.bungeeAnchor.prevPos.X);
        }

        [Fact]
        public void ReleaseRopesForPoint_CutsConnectorAtTailEnd_WhenTailCandyReleased()
        {
            ConstraintedPoint head = PointAt(100f, 100f, 1f);
            ConstraintedPoint tail = PointAt(100f, 220f, 1f);
            Bungee connector = new Bungee().InitWithHeadAtXYTailAtTXTYandLength(
                head, head.pos.X, head.pos.Y, tail, tail.pos.X, tail.pos.Y, 60f);
            GameScene scene = SceneWithConnector(connector);

            scene.ReleaseRopesForPoint(tail);

            Assert.Equal(connector.parts.Count - 2, connector.cut);
        }

        [Fact]
        public void ReleaseRopesForPoint_CutsConnectorAtHeadEnd_WhenHeadCandyReleased()
        {
            ConstraintedPoint head = PointAt(100f, 100f, 1f);
            ConstraintedPoint tail = PointAt(100f, 220f, 1f);
            Bungee connector = new Bungee().InitWithHeadAtXYTailAtTXTYandLength(
                head, head.pos.X, head.pos.Y, tail, tail.pos.X, tail.pos.Y, 60f);
            GameScene scene = SceneWithConnector(connector);

            scene.ReleaseRopesForPoint(head);

            Assert.Equal(0, connector.cut);
        }

        [Fact]
        public void ReleaseRopesForPoint_HidesConnectorTailParts_WhenConnectorAlreadyCut()
        {
            ConstraintedPoint head = PointAt(100f, 100f, 1f);
            ConstraintedPoint tail = PointAt(100f, 220f, 1f);
            Bungee connector = new Bungee().InitWithHeadAtXYTailAtTXTYandLength(
                head, head.pos.X, head.pos.Y, tail, tail.pos.X, tail.pos.Y, 60f);
            connector.SetCut(0);
            GameScene scene = SceneWithConnector(connector);

            scene.ReleaseRopesForPoint(tail);

            Assert.True(connector.hideTailParts);
        }

        [Fact]
        public void RemovePart_PreservesEndpointWeights_WhenEndpointsNotOwned()
        {
            ConstraintedPoint head = PointAt(100f, 100f, 1f);
            ConstraintedPoint tail = PointAt(100f, 220f, 1f);
            Bungee connector = new Bungee().InitWithHeadAtXYTailAtTXTYandLength(
                head, head.pos.X, head.pos.Y, tail, tail.pos.X, tail.pos.Y, 60f);

            connector.RemovePart(connector.parts.Count / 2);

            // The shared candy points must keep their mass; only limp segment points go weightless.
            Assert.Equal(1f, head.weight);
            Assert.Equal(1f, tail.weight);
        }

        [Fact]
        public void RemovePart_WeakensOwnedAnchor()
        {
            ConstraintedPoint tail = PointAt(100f, 220f, 1f);
            Bungee grabRope = new Bungee().InitWithHeadAtXYTailAtTXTYandLength(
                null, 100f, 100f, tail, tail.pos.X, tail.pos.Y, 60f);

            grabRope.RemovePart(grabRope.parts.Count / 2);

            // A bungee-owned anchor still goes limp after a cut (unchanged behavior).
            Assert.Equal(1E-05f, grabRope.bungeeAnchor.weight);
        }

        [Fact]
        public void BuildChainSpritePlan_UsesSeparatePointAndMidpointSprites()
        {
            Vector[] points =
            [
                Vect(0f, 0f),
                Vect(50f, 0f),
                Vect(100f, 0f)
            ];

            Bungee.ChainSprite[] sprites = Bungee.BuildChainSpritePlan(points, 3, 2, Vect(56f, 56f), Vect(56f, 56f));

            Assert.Equal(7, sprites.Length);
            Assert.All(sprites[..4], sprite => Assert.Equal(0, sprite.QuadIndex));
            Assert.All(sprites[4..], sprite => Assert.Equal(1, sprite.QuadIndex));
            Assert.Equal(0f, sprites[0].Center.X);
            Assert.Equal(25f, sprites[1].Center.X);
            Assert.Equal(50f, sprites[2].Center.X);
            Assert.Equal(75f, sprites[3].Center.X);
            Assert.Equal(12.5f, sprites[4].Center.X);
            Assert.Equal(37.5f, sprites[5].Center.X);
            Assert.Equal(62.5f, sprites[6].Center.X);
        }

        [Fact]
        public void GetCutFadeAlpha_MatchesRopeFadeTiming()
        {
            Bungee bungee = new()
            {
                cut = -1,
                cutTime = 0.975f,
                forceWhite = false
            };

            Assert.Equal(1f, Bungee.GetCutFadeAlpha(bungee));

            bungee.cut = 0;
            bungee.forceWhite = true;
            Assert.Equal(1f, Bungee.GetCutFadeAlpha(bungee));

            bungee.forceWhite = false;
            Assert.Equal(0.5f, Bungee.GetCutFadeAlpha(bungee), 5);
        }

        [Fact]
        public void GetChainFadeBlendFactors_UsesStraightAlphaBlend()
        {
            (BlendingFactor source, BlendingFactor destination) = Bungee.GetChainFadeBlendFactors();

            Assert.Equal(BlendingFactor.GLSRCALPHA, source);
            Assert.Equal(BlendingFactor.GLONEMINUSSRCALPHA, destination);
        }

        [Fact]
        public void SetCutOnlyByAxe_MarksChainAndBlocksFingerCut()
        {
            Bungee bungee = new();

            bungee.SetCutOnlyByAxe();

            Assert.True(bungee.breakable);
            Assert.True(bungee.cutOnlyByAxe);
        }

        [Fact]
        public void BuildChainSpriteColors_AppliesFadeAlphaAndPerLinkMasking()
        {
            RGBAColor[] colors = Bungee.BuildChainSpriteColors(2, 0.5f, seed: 12345);

            Assert.Equal(8, colors.Length);

            // Alpha is always the fade value, and each link's four vertices share one color.
            Assert.All(colors, color => Assert.Equal(0.5f, color.AlphaChannel));
            for (int link = 0; link < 2; link++)
            {
                RGBAColor first = colors[link * 4];
                for (int v = 1; v < 4; v++)
                {
                    RGBAColor vertex = colors[(link * 4) + v];
                    Assert.Equal(first.RedColor, vertex.RedColor);
                    Assert.Equal(first.GreenColor, vertex.GreenColor);
                    Assert.Equal(first.BlueColor, vertex.BlueColor);
                }

                // Each link is either opaque white or a grey mask shade (r == g == b, <= 1).
                Assert.Equal(first.RedColor, first.GreenColor);
                Assert.Equal(first.GreenColor, first.BlueColor);
                Assert.InRange(first.RedColor, 0f, 1f);
            }
        }

        [Fact]
        public void BuildChainSpriteColors_IsStableForSameSeed()
        {
            RGBAColor[] first = Bungee.BuildChainSpriteColors(6, 1f, seed: 999);
            RGBAColor[] second = Bungee.BuildChainSpriteColors(6, 1f, seed: 999);

            for (int i = 0; i < first.Length; i++)
            {
                Assert.Equal(first[i].RedColor, second[i].RedColor);
                Assert.Equal(first[i].GreenColor, second[i].GreenColor);
                Assert.Equal(first[i].BlueColor, second[i].BlueColor);
            }
        }
    }
}
