using System;

using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests.Interactions
{
    /// <summary>
    /// Transport is a lifecycle state, not a pair of target fields: entering a bamboo tube or a magic
    /// hat hides the candy's whole body inside one <see cref="CandyTransportSession"/>, and only that
    /// exact session can bring it back. These drive the real scene so the session the scene enqueues
    /// is the one the lifecycle is holding.
    /// </summary>
    public sealed class CandyTransportLifecycleTests
    {
        [Fact]
        public void BambooEntryHidesTheWholeBodyInABambooSession()
        {
            (GameScene scene, CandyContext candy) = TubeRig();
            BambooTube tube = scene.BambooTubes()[0];

            Act.EnterBambooTube(scene, candy, TubeMouth.CatchesFalling);

            Assert.Equal(CandyPresence.Hidden, candy.Lifecycle.Presence);
            CandyTransportSession session = candy.Lifecycle.Transport;
            Assert.NotNull(session);
            Assert.Equal(CandyTransportKind.Bamboo, session.Kind);
            Assert.Same(tube, session.BambooTube);
            Assert.Same(candy, session.Candy);
            Assert.Null(session.Sock);
        }

        [Fact]
        public void BambooEntryLeavesNoActiveBodyForTheTransit()
        {
            (GameScene scene, CandyContext candy) = TubeRig();

            Act.EnterBambooTube(scene, candy, TubeMouth.CatchesFalling);

            Assert.Empty(candy.Lifecycle.ActiveBodies);
        }

        [Fact]
        public void BambooExitRestoresThePresentWholeBody()
        {
            (GameScene scene, CandyContext candy) = TubeRig();
            Act.EnterBambooTube(scene, candy, TubeMouth.CatchesFalling);
            CandyBody body = candy.WholeBody;

            Assert.True(
                Interaction.StepUntil(scene, () => candy.Lifecycle.Presence == CandyPresence.Present),
                "the tube never released the candy");

            Assert.Null(candy.Lifecycle.Transport);
            Assert.Equal([body], candy.Lifecycle.ActiveBodies);
            Assert.Same(body, candy.WholeBody);
        }

        [Fact]
        public void HatEntryHidesTheWholeBodyInASockSessionCarryingItsExitSpeed()
        {
            (GameScene scene, CandyContext candy) = HatRig();
            // Act.EnterHat drives the candy into the first hat; a hat throws to its partner, so the
            // session carries the *exit* hat, which is where the completion puts the candy back.
            Sock exitHat = scene.Hats()[1];

            // The exit speed is taken from the entry velocity, so the candy has to be falling for
            // there to be one to carry.
            Interaction.Drop(candy);
            Act.EnterHat(scene, candy);

            Assert.Equal(CandyPresence.Hidden, candy.Lifecycle.Presence);
            CandyTransportSession session = candy.Lifecycle.Transport;
            Assert.NotNull(session);
            Assert.Equal(CandyTransportKind.Sock, session.Kind);
            Assert.Same(exitHat, session.Sock);
            Assert.Same(candy, session.Candy);
            Assert.Null(session.BambooTube);
            Assert.True(session.SavedExitSpeed > 0f, "the hat session kept no exit speed");
        }

        [Fact]
        public void HatExitRestoresThePresentWholeBody()
        {
            (GameScene scene, CandyContext candy) = HatRig();
            Act.EnterHat(scene, candy);
            CandyBody body = candy.WholeBody;

            Assert.True(
                Interaction.StepUntil(scene, () => candy.Lifecycle.Presence == CandyPresence.Present),
                "the hat never released the candy");

            Assert.Null(candy.Lifecycle.Transport);
            Assert.Equal([body], candy.Lifecycle.ActiveBodies);
        }

        [Fact]
        public void ATubeSwallowingOneCandyLeavesTheOtherPresentWithItsCarrier()
        {
            GameScene scene = Scenario.New()
                .Candy(60, 200, number: "1")
                .Candy(260, 200, number: "2")
                .OmNom(20, 460)
                .BambooTube(20, 40, TubeMouth.CatchesFalling)
                .Rocket(260, 200, impulse: 0f)
                .Build();
            CandyContext swallowed = scene.Candies()[0];
            CandyContext kept = scene.Candies()[1];
            Interaction.Hover(swallowed);
            Interaction.Hover(kept);
            Rocket rocket = Act.BindRocket(scene, kept);

            Act.EnterBambooTube(scene, swallowed, TubeMouth.CatchesFalling);

            Assert.Equal(CandyPresence.Hidden, swallowed.Lifecycle.Presence);
            Assert.Equal(CandyPresence.Present, kept.Lifecycle.Presence);
            Assert.Null(kept.Lifecycle.Transport);
            Assert.Same(rocket, kept.Lifecycle.Attachments.Rocket);
            Assert.True(rocket.visible);
        }

        [Fact]
        public void AStaleTransportCallbackCannotCompleteTheSessionThatReplacedIt()
        {
            (GameScene scene, CandyContext candy) = TubeRig();
            Act.EnterBambooTube(scene, candy, TubeMouth.CatchesFalling);
            CandyTransportSession stale = candy.Lifecycle.Transport;

            Assert.True(
                Interaction.StepUntil(scene, () => candy.Lifecycle.Presence == CandyPresence.Present),
                "the tube never released the candy");

            Act.EnterBambooTube(scene, candy, TubeMouth.CatchesFalling);
            CandyTransportSession current = candy.Lifecycle.Transport;
            Assert.NotSame(stale, current);

            // The dispatcher can still be holding the first transit's callback; replaying it must not
            // end the transit that replaced it.
            scene.Teleport(stale);

            Assert.Equal(CandyPresence.Hidden, candy.Lifecycle.Presence);
            Assert.Same(current, candy.Lifecycle.Transport);
        }

        private static (GameScene Scene, CandyContext Candy) TubeRig()
        {
            return Rig(s => s.BambooTube(20, 40, TubeMouth.CatchesFalling));
        }

        private static (GameScene Scene, CandyContext Candy) HatRig()
        {
            // A hat throws to its partner, so a lone hat swallows nothing.
            return Rig(s => s.Hat(20, 40, group: 1).Hat(300, 40, group: 1));
        }

        private static (GameScene Scene, CandyContext Candy) Rig(Func<Scenario, Scenario> transporter)
        {
            GameScene scene = transporter(Scenario.New().Candy(160, 200).OmNom(20, 460)).Build();
            CandyContext candy = scene.Candy();
            Interaction.Hover(candy);
            return (scene, candy);
        }
    }
}
