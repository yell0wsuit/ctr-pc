using CutTheRopeDX.Framework.Physics;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class CandyLifecycleTests
    {
        private static CandyBody Body(CandyBodyRole role)
        {
            return new CandyBody(new ConstraintedPoint(), role);
        }

        private static CandyLifecycle PresentLifecycle()
        {
            return CandyLifecycle.CreatePresent(Body(CandyBodyRole.Whole));
        }

        [Fact]
        public void PresentCandyCanBeRemovedAsEaten()
        {
            CandyLifecycle lifecycle = PresentLifecycle();

            Assert.True(lifecycle.TryRemove(CandyRemovalReason.Eaten, out _));
            Assert.Equal(CandyPresence.Removed, lifecycle.Presence);
            Assert.Equal(CandyRemovalReason.Eaten, lifecycle.RemovalReason);
            Assert.True(lifecycle.WasEaten);
            Assert.False(lifecycle.HasFailedRemoval);
        }

        [Theory]
        [InlineData((int)CandyRemovalReason.Hazard)]
        [InlineData((int)CandyRemovalReason.Spider)]
        [InlineData((int)CandyRemovalReason.OffScreen)]
        public void LossRemovalNeverCountsAsEaten(int reasonValue)
        {
            CandyRemovalReason reason = (CandyRemovalReason)reasonValue;
            CandyLifecycle lifecycle = PresentLifecycle();

            Assert.True(lifecycle.TryRemove(reason, out _));
            Assert.False(lifecycle.WasEaten);
            Assert.True(lifecycle.HasFailedRemoval);
        }

        [Fact]
        public void RemovedCandyIsTerminal()
        {
            CandyLifecycle lifecycle = PresentLifecycle();
            Assert.True(lifecycle.TryRemove(CandyRemovalReason.Hazard, out _));

            Assert.False(lifecycle.TryRemove(CandyRemovalReason.Eaten, out _));
            Assert.Equal(CandyRemovalReason.Hazard, lifecycle.RemovalReason);
        }

        [Fact]
        public void RemovingAWholeCandyAtomicallyClearsItsAttachmentState()
        {
            CandyLifecycle lifecycle = PresentLifecycle();
            _ = lifecycle.Attachments.CaptureInLantern();

            Assert.True(lifecycle.TryRemove(CandyRemovalReason.Hazard, out CandyAttachmentSnapshot detached));

            Assert.True(detached.InLantern);
            Assert.False(lifecycle.Attachments.HasAny);
            Assert.False(lifecycle.IsGravitySuppressed);
        }

        [Fact]
        public void SplitExposesBothPresentHalvesInsteadOfWholeBody()
        {
            CandyBody whole = Body(CandyBodyRole.Whole);
            CandyHalf left = new(Body(CandyBodyRole.LeftHalf));
            CandyHalf right = new(Body(CandyBodyRole.RightHalf));
            CandyLifecycle lifecycle = CandyLifecycle.CreateSplit(whole, new SplitCandyState(left, right));

            Assert.Equal(CandyPresence.Split, lifecycle.Presence);
            Assert.Equal([left.Body, right.Body], lifecycle.ActiveBodies);
        }

        [Fact]
        public void BambooFactoryPreservesTransportPayload()
        {
            CandyTransportSession session = CandyTransportSession.ForBamboo(candy: null, tube: null);

            Assert.Equal(CandyTransportKind.Bamboo, session.Kind);
            Assert.Null(session.Candy);
            Assert.Null(session.BambooTube);
            Assert.Null(session.Sock);
        }

        [Fact]
        public void SockFactoryPreservesTransportPayloadAndExitSpeed()
        {
            CandyTransportSession session = CandyTransportSession.ForSock(candy: null, sock: null, savedExitSpeed: 123f);

            Assert.Equal(CandyTransportKind.Sock, session.Kind);
            Assert.Null(session.Candy);
            Assert.Null(session.BambooTube);
            Assert.Null(session.Sock);
            Assert.Equal(123f, session.SavedExitSpeed);
        }

        [Fact]
        public void HiddenTransportSuppressesBodyButDoesNotCountAsEaten()
        {
            CandyLifecycle lifecycle = PresentLifecycle();
            CandyTransportSession session = CandyTransportSession.ForBamboo(candy: null, tube: null);

            Assert.True(lifecycle.TryHide(session, out _));
            Assert.Equal(CandyPresence.Hidden, lifecycle.Presence);
            Assert.Empty(lifecycle.ActiveBodies);
            Assert.False(lifecycle.WasEaten);
        }

        [Fact]
        public void MatchingTransportCompletionRestoresWholeBody()
        {
            CandyLifecycle lifecycle = PresentLifecycle();
            CandyTransportSession session = CandyTransportSession.ForSock(null, null, 123f);
            Assert.True(lifecycle.TryHide(session, out _));

            Assert.True(lifecycle.TryCompleteTransport(session));
            Assert.Equal(CandyPresence.Present, lifecycle.Presence);
            Assert.Null(lifecycle.Transport);
            Assert.Equal([lifecycle.WholeBody], lifecycle.ActiveBodies);
        }

        [Fact]
        public void ContextOwnsTheSuppliedWholeBodyAndAPresentLifecycle()
        {
            CandyBody whole = Body(CandyBodyRole.Whole);

            CandyContext ctx = new(whole);

            Assert.Same(whole, ctx.WholeBody);
            Assert.Same(whole, ctx.Lifecycle.WholeBody);
            Assert.Equal(CandyPresence.Present, ctx.Lifecycle.Presence);
            Assert.Equal([whole], ctx.Lifecycle.ActiveBodies);
            Assert.NotNull(ctx.Lifecycle.Attachments);
            Assert.True(ctx.Lifecycle.CanEnterTransport);
            Assert.False(ctx.Lifecycle.IsGravitySuppressed);
        }

        [Fact]
        public void LanternAndTransportStateAuthoritativelyAnswerInteractionQuestions()
        {
            CandyLifecycle lifecycle = PresentLifecycle();

            _ = lifecycle.Attachments.CaptureInLantern();

            Assert.False(lifecycle.CanEnterTransport);
            Assert.True(lifecycle.IsGravitySuppressed);

            lifecycle.Attachments.ReleaseFromLantern();
            CandyTransportSession session = CandyTransportSession.ForBamboo(null, null);
            Assert.True(lifecycle.TryHide(session, out _));
            Assert.False(lifecycle.CanEnterTransport);
            Assert.True(lifecycle.IsGravitySuppressed);
        }

        [Fact]
        public void ContextOutcomePreservesCapabilitiesAndRemovalReason()
        {
            CandyContext ctx = new(Body(CandyBodyRole.Whole)) { Capabilities = CandyCapabilities.LightBulb };
            Assert.True(ctx.Lifecycle.TryRemove(CandyRemovalReason.Hazard, out _));

            CandyOutcomeView outcome = ctx.ToOutcomeView();

            Assert.Equal(CandyPresence.Removed, outcome.Presence);
            Assert.Equal(CandyRemovalReason.Hazard, outcome.RemovalReason);
            Assert.False(outcome.CanBeEaten);
            Assert.False(outcome.HasFailedSplitHalf);
        }

        [Fact]
        public void ContextOutcomeReportsAnEatableCandyThatIsStillPresent()
        {
            CandyContext ctx = new(Body(CandyBodyRole.Whole));

            CandyOutcomeView outcome = ctx.ToOutcomeView();

            Assert.Equal(CandyPresence.Present, outcome.Presence);
            Assert.Null(outcome.RemovalReason);
            Assert.True(outcome.CanBeEaten);
            Assert.False(outcome.HasFailedSplitHalf);
        }

        [Fact]
        public void SplitLifecycleReportsAFailedHalfWithoutBeingRemoved()
        {
            CandyHalf left = new(Body(CandyBodyRole.LeftHalf));
            CandyHalf right = new(Body(CandyBodyRole.RightHalf));
            CandyLifecycle lifecycle =
                CandyLifecycle.CreateSplit(Body(CandyBodyRole.Whole), new SplitCandyState(left, right));

            Assert.False(lifecycle.HasFailedSplitHalf);
            Assert.True(left.TryRemove(CandyRemovalReason.OffScreen));

            Assert.True(lifecycle.HasFailedSplitHalf);
            Assert.Equal(CandyPresence.Split, lifecycle.Presence);
            Assert.Null(lifecycle.RemovalReason);
        }

        [Fact]
        public void StaleCompletionCannotCompleteNewerSession()
        {
            CandyLifecycle lifecycle = PresentLifecycle();
            CandyTransportSession oldSession = CandyTransportSession.ForBamboo(null, null);
            CandyTransportSession newSession = CandyTransportSession.ForSock(null, null, 123f);
            _ = lifecycle.TryHide(oldSession, out _);
            Assert.True(lifecycle.TryCompleteTransport(oldSession));
            _ = lifecycle.TryHide(newSession, out _);

            Assert.False(lifecycle.TryCompleteTransport(oldSession));
            Assert.Same(newSession, lifecycle.Transport);
            Assert.Equal(CandyPresence.Hidden, lifecycle.Presence);
        }
    }
}
