using System.Reflection;

using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.GameMain;
using CutTheRopeDX.Tests.Interactions;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public sealed class DelayedOperationOwnershipTests
    {
        private const BindingFlags Instance = BindingFlags.Instance | BindingFlags.NonPublic;

        [Theory]
        [InlineData("parkedGhostBubble", "ParkedGhostBubble")]
        [InlineData("pendingLanternCapture", "PendingLanternCapture")]
        public void GameSceneStoresEachDelayedOperationAsOneNullableTicket(
            string fieldName,
            string ticketTypeName)
        {
            FieldInfo field = typeof(GameScene).GetField(fieldName, Instance);

            Assert.NotNull(field);
            Assert.Equal(ticketTypeName, field.FieldType.Name);
        }

        [Theory]
        [InlineData("pendingSecondGhostBubble")]
        [InlineData("pendingSecondGhostBubbleOwner")]
        [InlineData("pendingLanternCapturePoint")]
        public void GameSceneHasNoIndependentlyAssignableDelayedOperationPayloadFields(string fieldName)
        {
            Assert.Null(typeof(GameScene).GetField(fieldName, Instance));
        }

        [Fact]
        public void ParkedGhostBubbleCancellationReleasesItsExactBubble()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 200)
                .OmNom(160, 440)
                .Bubble(20, 40)
                .Build();
            GameObject bubble = scene.Bubbles()[0];
            ParkedGhostBubble parked = new(scene.Candy().WholeBody, bubble);
            GameObject released = null;

            parked.Cancel(candidate => released = candidate);

            Assert.Same(bubble, released);
        }

        [Fact]
        public void ReplacingAParkedGhostBubbleCancelsThePreviousTicket()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 200)
                .OmNom(160, 440)
                .Bubble(20, 40)
                .Bubble(40, 40)
                .Build();
            CandyBody owner = scene.Candy().WholeBody;
            GameObject oldBubble = scene.Bubbles()[0];
            ParkedGhostBubble replacement = new(owner, scene.Bubbles()[1]);
            ParkedGhostBubble parked = new(owner, oldBubble);
            GameObject released = null;

            ParkedGhostBubble result = parked.ReplaceWith(
                replacement,
                candidate => released = candidate);

            Assert.Same(oldBubble, released);
            Assert.Same(replacement, result);
        }

        [Fact]
        public void PoppingTheOwnerBubbleCancelsItsParkedGhostTicket()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 200)
                .OmNom(160, 440)
                .Bubble(160, 200)
                .Bubble(20, 40)
                .Build();
            CandyContext candy = scene.Candy();
            _ = Act.CaptureInBubble(scene, candy, bubbleIndex: 0);
            scene.ParkSecondGhostBubble(candy.WholeBody, scene.Bubbles()[1]);

            scene.PopCandyBubble(candy.WholeBody);

            Assert.Null(scene.PendingSecondGhostBubble());
        }

        [Fact]
        public void ObsoleteLanternCallbackCannotCompleteAReplacementTicket()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 200)
                .OmNom(160, 440)
                .Lantern(20, 40)
                .Build();
            CandyContext candy = scene.Candy();
            Lantern lantern = Lantern.GetAllLanterns()[0];
            PendingLanternCapture obsolete = new(candy.WholeBody.Point, lantern);
            PendingLanternCapture replacement = new(candy.WholeBody.Point, lantern);
            scene.SetPendingLanternCapture(replacement);

            scene.CompletePendingLanternCapture(obsolete);

            Assert.Same(replacement, scene.PendingLanternCapture());
        }
    }
}
