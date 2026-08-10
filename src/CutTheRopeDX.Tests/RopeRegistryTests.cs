using System.Linq;

using CutTheRopeDX.Framework.Physics;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>Verifies the rope index the three merged sweeps run over.</summary>
    public class RopeRegistryTests
    {
        private static Bungee MakeRope(ConstraintedPoint tail)
        {
            _ = HeadlessGame.Boot();
            return new Bungee().InitWithHeadAtXYTailAtTXTYandLength(null, 0f, 0f, tail, 0f, 100f, 100f);
        }

        private static Grab NewHook()
        {
            _ = HeadlessGame.Boot();
            return new Grab();
        }

        [Fact]
        public void GrabRopesCarryTheirOwnerConnectorDoesNot()
        {
            RopeRegistry registry = new();
            Grab hook = NewHook();
            Bungee hookRope = MakeRope(new ConstraintedPoint());
            Bungee connector = MakeRope(new ConstraintedPoint());

            registry.Register(hookRope, hook);
            registry.RegisterConnector(connector);

            RopeEntry hookEntry = registry.All.Single(e => e.Rope == hookRope);
            RopeEntry connectorEntry = registry.All.Single(e => e.Rope == connector);

            Assert.Same(hook, hookEntry.Owner);
            Assert.False(hookEntry.IsConnector);
            Assert.Null(connectorEntry.Owner);
            Assert.True(connectorEntry.IsConnector);
        }

        [Fact]
        public void UnregisterRemovesTheEntry()
        {
            RopeRegistry registry = new();
            Bungee rope = MakeRope(new ConstraintedPoint());
            registry.Register(rope, NewHook());

            registry.Unregister(rope);

            Assert.Empty(registry.All);
        }

        [Fact]
        public void GrabRopeCutsAtItsTailEndOnly()
        {
            ConstraintedPoint candy = new();
            Bungee rope = MakeRope(candy);
            RopeEntry entry = new(rope, NewHook());

            Assert.Equal(rope.parts.Count - 2, entry.CutPartForCandy(candy));
            Assert.Null(entry.CutPartForCandy(new ConstraintedPoint()));
        }

        [Fact]
        public void ConnectorCutsAtWhicheverEndTheCandyIsOn()
        {
            // The one asymmetry the merged destroy-on-candy sweep has to preserve.
            ConstraintedPoint tailCandy = new();
            Bungee connector = MakeRope(tailCandy);
            RopeEntry entry = new(connector, null);

            Assert.Equal(connector.parts.Count - 2, entry.CutPartForCandy(tailCandy));
            Assert.Equal(0, entry.CutPartForCandy(connector.bungeeAnchor));
        }
    }
}
