using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.GameMain;

using Xunit;

using static CutTheRopeDX.Framework.Helpers.CTRMathHelper;

namespace CutTheRopeDX.Tests.Interactions
{
    /// <summary>
    /// The elastic that <c>candiesConnected</c> strings between two candies: how a stroke that
    /// crosses it is resolved, and the relaxation Time Travel runs around it.
    /// </summary>
    public sealed class CandyConnectorPhysicsTests
    {
        [Fact]
        public void AStrokeAcrossBothCutsTheHookRopeNotTheConnector()
        {
            // The connector registers during LoadMetadata, ahead of every hook, and the cut routine
            // stops at the first rope it severs - so in registration order a stroke that crossed
            // both severed the link instead of the rope the player aimed at.
            GameScene scene = ConnectedScene(timeTravel: false);
            Grab hook = scene.Grabs()[0];
            Bungee hookRope = hook.Rope;
            Bungee connector = scene.Connector();
            HeadlessGame.StepFrames(scene, 30);

            (Vector from, Vector to) = StrokeAcross(hookRope, connector);
            int cut = scene.CutWithRazorOrLine1Line2Immediate(null, from, to, false);

            Assert.Equal(1, cut);
            Assert.NotEqual(-1, hookRope.cut);
            Assert.Equal(-1, connector.cut);
        }

        [Fact]
        public void TimeTravelRelaxesCandyPointsAfterIntegration()
        {
            GameScene scene = ConnectedScene(timeTravel: true);
            Assert.True(ActivePhysicsConstants.RelaxCandyPointsAfterIntegration);
            Assert.NotNull(scene.Connector());

            _ = ConnectedScene(timeTravel: false);
            Assert.False(ActivePhysicsConstants.RelaxCandyPointsAfterIntegration);
        }

        [Fact]
        public void RelaxingTheEndpointsLeavesTheConnectorSpanAlone()
        {
            // The extra relaxation is a fidelity port, not a tuning change: Bungee.Update's own
            // 30x pass has already satisfied the connector by the time it runs, so it only
            // corrects the sliver the candy integration adds afterwards.
            //
            // The control is the mobile model without the Time Travel flag, not the desktop one. Time
            // Travel is a mode of mobile physics, so a desktop control would differ by the whole model -
            // rope rest length included - and would say nothing about the relaxation pass on its own.
            Assert.Equal(
                SettledConnectorSpan(timeTravel: false, mobilePhysics: true),
                SettledConnectorSpan(timeTravel: true),
                1);
        }

        /// <summary>Runs a connected pair for two seconds and reports the gap between the candies.</summary>
        /// <param name="timeTravel">Whether the map opts into the Time Travel physics.</param>
        /// <param name="mobilePhysics">Whether the map takes the mobile model without the Time Travel flag.</param>
        /// <returns>The distance between the two candy points, in world units.</returns>
        private static float SettledConnectorSpan(bool timeTravel, bool mobilePhysics = false)
        {
            GameScene scene = ConnectedScene(timeTravel, mobilePhysics);
            HeadlessGame.StepFrames(scene, 120);
            return VectDistance(
                scene.Candies()[0].WholeBody.Point.pos,
                scene.Candies()[1].WholeBody.Point.pos);
        }

        /// <summary>
        /// Builds a stroke that crosses both ropes, running through a point on each and overshooting
        /// past both so neither intersection lands on an endpoint.
        /// </summary>
        /// <param name="first">The first rope to cross.</param>
        /// <param name="second">The second rope to cross.</param>
        /// <returns>The stroke's start and end points.</returns>
        private static (Vector From, Vector To) StrokeAcross(Bungee first, Bungee second)
        {
            Vector a = first.parts[first.parts.Count / 2].pos;
            Vector b = second.parts[second.parts.Count / 2].pos;
            Vector along = VectSub(b, a);
            return (VectSub(a, VectMult(along, 0.3f)), VectAdd(b, VectMult(along, 0.3f)));
        }

        /// <summary>
        /// Builds the reference layout: two candies joined by a connector, the upper one also hung
        /// from a hook, so one stroke can cross the hook's rope and the connector alike.
        /// </summary>
        /// <param name="timeTravel">Whether the map opts into the Time Travel physics.</param>
        /// <param name="mobilePhysics">Whether the map takes the mobile model without the Time Travel flag.</param>
        /// <returns>The built scene.</returns>
        private static GameScene ConnectedScene(bool timeTravel, bool mobilePhysics = false)
        {
            Scenario scenario = Scenario.New()
                .MapSize(320, 480)
                .Design("candiesConnected", "true")
                .Design("candiesConnectedLength", "70")
                .Candy(284, 135, "first")
                .Candy(284, 241, "second")
                .Rope(284, 25, length: 70, candyNumber: "first")
                .OmNom(40, 240);
            if (mobilePhysics || timeTravel)
            {
                // Time Travel's tuning is a mode of the mobile model, so asking for it means asking
                // for both. A caller can also take the mobile model on its own, which is what gives
                // the Time Travel cases a control that differs by nothing else.
                _ = scenario.Design("useMobilePhysics", "true");
            }
            if (timeTravel)
            {
                _ = scenario.Design("useTimeTravelRocketPhysics", "true");
            }
            return scenario.Build();
        }
    }
}
