using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Physics;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;
using CutTheRopeDX.Tests.Interactions;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public sealed class GhostOwnershipTests
    {
        private const BindingFlags Instance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        [Theory]
        [InlineData("ghostState")]
        [InlineData("bubble")]
        [InlineData("grab")]
        [InlineData("bouncer")]
        [InlineData("cyclingEnabled")]
        [InlineData("candyBreak")]
        public void GhostHasNoIndependentlyAssignableLegacyState(string fieldName)
        {
            Assert.Null(typeof(Ghost).GetField(fieldName, Instance));
        }

        [Fact]
        public void GhostStoresOneCurrentApparitionAcrossForms()
        {
            (Ghost ghost, List<Bubble> bubbles, List<Grab> grabs, List<Bouncer> bouncers) = CreateGhost();

            Assert.Equal(GhostForm.Idle, ghost.Form);
            Assert.Null(ghost.Apparition);

            ghost.ResetToForm(GhostForm.Bubble);
            GhostBubble bubble = Assert.IsType<GhostBubble>(ghost.Apparition);

            ghost.ResetToForm(GhostForm.Grab);
            GhostGrab grab = Assert.IsType<GhostGrab>(ghost.Apparition);

            ghost.ResetToForm(GhostForm.Bouncer);

            Assert.Equal(GhostForm.Bouncer, ghost.Form);
            _ = Assert.IsType<GhostBouncer>(ghost.Apparition);
            Assert.Same(bubble, Assert.Single(bubbles));
            Assert.Same(grab, Assert.Single(grabs));
            _ = Assert.Single(bouncers);
            _ = Assert.Single(
                typeof(Ghost).GetFields(Instance),
                field => field.FieldType == typeof(IGhostApparition));
        }

        [Fact]
        public void RapidMorphsRetireEveryOutgoingApparition()
        {
            (Ghost ghost, List<Bubble> bubbles, List<Grab> grabs, List<Bouncer> _) = CreateGhost();
            ghost.ResetToForm(GhostForm.Bubble);
            ghost.ResetToForm(GhostForm.Grab);
            ghost.ResetToForm(GhostForm.Bouncer);

            bubbles[0].Update(0.2f);
            grabs[0].Update(0.2f);
            ghost.Update(0f);

            Assert.Empty(bubbles);
            Assert.Empty(grabs);
            _ = Assert.IsType<GhostBouncer>(ghost.Apparition);
        }

        [Fact]
        public void BubbleToGrabImmediatelyChangesOwnershipThenRetiresTheOutgoingBubble()
        {
            (Ghost ghost, List<Bubble> bubbles, List<Grab> grabs, _) = CreateGhost();
            ghost.ResetToForm(GhostForm.Bubble);
            GhostBubble outgoingBubble = Assert.IsType<GhostBubble>(ghost.Apparition);

            ghost.ResetToForm(GhostForm.Grab);

            Assert.Equal(GhostForm.Grab, ghost.Form);
            GhostGrab currentGrab = Assert.IsType<GhostGrab>(ghost.Apparition);
            Assert.Same(currentGrab, Assert.Single(grabs));
            Assert.Same(outgoingBubble, Assert.Single(bubbles));

            outgoingBubble.Update(0.2f);
            ghost.Update(0f);

            Assert.Empty(bubbles);
            Assert.Equal(GhostForm.Grab, ghost.Form);
            Assert.Same(currentGrab, ghost.Apparition);
        }

        [Fact]
        public void MorphPhaseNamesOutgoingAndIncomingFormsUntilAppearanceFinishes()
        {
            (Ghost ghost, _, _, _) = CreateGhost();

            ghost.ResetToForm(GhostForm.Bubble);

            Assert.Equal(new GhostMorphPhase(GhostForm.Idle, GhostForm.Bubble), ghost.MorphPhase);

            ghost.Apparition.Element.Update(0.4f);
            ghost.Update(0f);

            Assert.Null(ghost.MorphPhase);
        }

        [Fact]
        public void CapturedBubbleItselfBlocksCyclingUntilReleased()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 200)
                .OmNom(20, 460)
                .Ghost(20, 40)
                .Build();
            CandyContext candy = scene.Candy();
            Ghost ghost = Assert.Single(scene.Ghosts());
            ghost.ResetToForm(GhostForm.Bubble);
            Bubble bubble = Act.CaptureInBubble(scene, candy);

            Assert.Same(ghost.Apparition, bubble);
            Assert.False(ghost.OnTouchDownXY(ghost.x, ghost.y));
            Assert.Equal(GhostForm.Bubble, ghost.Form);

            scene.PopCandyBubble(candy.WholeBody);

            Assert.Equal(GhostForm.Idle, ghost.Form);
            Assert.Null(ghost.Apparition);
        }

        [Fact]
        public void PoppedGhostBubbleClearsCandyOwnershipAndRetiresWithoutLeaking()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 200)
                .OmNom(20, 460)
                .Ghost(20, 40)
                .Build();
            CandyContext candy = scene.Candy();
            Ghost ghost = Assert.Single(scene.Ghosts());
            ghost.ResetToForm(GhostForm.Bubble);
            Bubble poppedBubble = Act.CaptureInBubble(scene, candy);

            scene.PopCandyBubble(candy.WholeBody);

            Assert.Null(candy.WholeBody.Bubble);
            Assert.Equal(GhostForm.Idle, ghost.Form);
            Assert.Null(ghost.Apparition);
            Assert.Contains(poppedBubble, scene.Bubbles());

            poppedBubble.Update(0.2f);
            ghost.Update(0f);

            Assert.DoesNotContain(poppedBubble, scene.Bubbles());
        }

        [Fact]
        public void CutAutoRopeReturnsGhostToIdleAndRetiresTheGrabAndRope()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 200)
                .OmNom(20, 460)
                .Ghost(20, 40, radius: -1)
                .Build();
            Ghost ghost = Assert.Single(scene.Ghosts());
            ghost.ResetToForm(GhostForm.Grab);
            GhostGrab outgoingGrab = Assert.IsType<GhostGrab>(ghost.Apparition);
            Bungee outgoingRope = outgoingGrab.Rope;
            Assert.NotNull(outgoingRope);
            Assert.Contains(scene.RegisteredRopes(), entry => entry.Rope == outgoingRope);

            outgoingRope.cut = 0;
            ghost.Update(0f);

            Assert.Equal(GhostForm.Idle, ghost.Form);
            Assert.Null(ghost.Apparition);

            outgoingGrab.Update(0.2f);
            ghost.Update(0f);

            Assert.DoesNotContain(outgoingGrab, scene.Grabs());
            Assert.Null(outgoingGrab.Rope);
            Assert.DoesNotContain(scene.RegisteredRopes(), entry => entry.Rope == outgoingRope);
        }

        [Fact]
        public void TwoRapidAutoRopeToBouncerMorphsReleaseCandyPhysicsBeforePumpImpulse()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 200)
                .OmNom(20, 460)
                .Ghost(80, 100, radius: -1, bouncer: true)
                .Ghost(240, 100, radius: -1, bouncer: true)
                .Pump(160, 300)
                .Build();
            CandyBody candy = scene.Candy().WholeBody;
            int originalConstraintCount = candy.Point.constraints.Count;
            List<Ghost> ghosts = scene.Ghosts();
            Assert.Equal(2, ghosts.Count);

            foreach (Ghost ghost in ghosts)
            {
                ghost.ResetToForm(GhostForm.Grab);
            }
            List<GhostGrab> outgoingGrabs = [.. ghosts.Select(ghost => Assert.IsType<GhostGrab>(ghost.Apparition))];
            List<Bungee> outgoingRopes = [.. outgoingGrabs.Select(grab => Assert.IsType<Bungee>(grab.Rope))];
            Assert.Equal(originalConstraintCount + 2, candy.Point.constraints.Count);

            foreach (Ghost ghost in ghosts)
            {
                ghost.ResetToForm(GhostForm.Bouncer);
            }
            foreach (GhostGrab outgoing in outgoingGrabs)
            {
                outgoing.Update(0.2f);
            }
            foreach (Ghost ghost in ghosts)
            {
                ghost.Update(0f);
                Assert.Equal(GhostForm.Bouncer, ghost.Form);
                _ = Assert.IsType<GhostBouncer>(ghost.Apparition);
            }

            Assert.Empty(scene.Grabs());
            Assert.DoesNotContain(scene.RegisteredRopes(), entry => outgoingRopes.Contains(entry.Rope));
            Assert.Equal(originalConstraintCount, candy.Point.constraints.Count);

            Pump pump = Assert.Single(scene.Pumps());
            BaseElement.CalculateTopLeft(candy.Visual);
            Vector beforePump = candy.Point.pos;
            float distanceBelowPump = pump.y - beforePump.Y;
            float verticalImpulse = 2f * (ActivePhysicsConstants.PumpFlowLength - distanceBelowPump);
            float expectedY = beforePump.Y - (verticalImpulse * 0.016f / ActivePhysicsConstants.TimeScale);

            scene.OperatePump(pump);
            for (int i = 0; i < Bungee.BUNGEE_RELAXION_TIMES; i++)
            {
                ConstraintedPoint.SatisfyConstraints(candy.Point);
            }

            Assert.Equal(beforePump.X, candy.Point.pos.X, 3);
            Assert.Equal(expectedY, candy.Point.pos.Y, 3);
            Assert.Equal(originalConstraintCount, candy.Point.constraints.Count);
        }

        [Fact]
        public void RocketConsumedBubbleDoesNotBlockGhostCycling()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 200)
                .OmNom(20, 460)
                .Rocket(160, 200, impulse: 0f)
                .Ghost(20, 40)
                .Build();
            CandyContext candy = scene.Candy();
            Ghost ghost = Assert.Single(scene.Ghosts());
            ghost.ResetToForm(GhostForm.Bubble);
            _ = Act.BindRocket(scene, candy);

            Bubble consumed = Act.PushBubbleAgainst(scene, candy);

            Assert.True(consumed.popped);
            Assert.Null(candy.WholeBody.Bubble);
            Assert.True(ghost.OnTouchDownXY(ghost.x, ghost.y));
            Assert.Equal(GhostForm.Grab, ghost.Form);
        }

        private static (Ghost Ghost, List<Bubble> Bubbles, List<Grab> Grabs, List<Bouncer> Bouncers) CreateGhost()
        {
            _ = HeadlessGame.Boot();
            List<Bubble> bubbles = [];
            List<Grab> grabs = [];
            List<Bouncer> bouncers = [];
            Ghost ghost = new Ghost().InitWithPositionPossibleFormsGrabRadiusBouncerAngleBubblesBungeesBouncers(
                new Vector(100f, 100f),
                GhostForm.Bubble | GhostForm.Grab | GhostForm.Bouncer,
                80f,
                0f,
                bubbles,
                grabs,
                bouncers,
                null);
            return (ghost, bubbles, grabs, bouncers);
        }
    }
}
