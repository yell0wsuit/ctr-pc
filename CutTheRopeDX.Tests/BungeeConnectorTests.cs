using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

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
    }
}
