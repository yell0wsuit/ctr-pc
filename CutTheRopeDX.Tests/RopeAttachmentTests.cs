using CutTheRopeDX.Framework.Physics;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class RopeAttachmentTests
    {
        private static Bungee MakeRope()
        {
            _ = HeadlessGame.Boot();
            ConstraintedPoint tail = new();
            tail.SetWeight(1f);
            return new Bungee().InitWithHeadAtXYTailAtTXTYandLength(null, 0f, 0f, tail, 0f, 100f, 100f);
        }

        [Fact]
        public void NewAttachmentIsIdle()
        {
            RopeAttachment attachment = new();

            Assert.Equal(RopeAttachmentState.Idle, attachment.State);
            Assert.Null(attachment.Rope);
            Assert.False(attachment.IsIntact);
            Assert.False(attachment.IsSimulated);
        }

        [Fact]
        public void TryAttachMovesToIntactAndExhaustsTheSource()
        {
            RopeAttachment attachment = new();
            Bungee rope = MakeRope();

            Assert.True(attachment.TryAttach(rope));
            Assert.Equal(RopeAttachmentState.Intact, attachment.State);
            Assert.Same(rope, attachment.Rope);
            Assert.True(attachment.IsIntact);
            Assert.True(attachment.IsSimulated);
            Assert.True(attachment.SourceExhausted);
        }

        [Fact]
        public void TryAttachFailsWhenARopeIsAlreadyHeld()
        {
            RopeAttachment attachment = new();
            Bungee first = MakeRope();
            _ = attachment.TryAttach(first);

            Assert.False(attachment.TryAttach(MakeRope()));
            Assert.Same(first, attachment.Rope);
        }

        [Fact]
        public void CutRopeDerivesSeveringThenInertWhenTheFadeEnds()
        {
            RopeAttachment attachment = new();
            Bungee rope = MakeRope();
            _ = attachment.TryAttach(rope);

            rope.SetCut(0);
            Assert.Equal(RopeAttachmentState.Severing, attachment.State);
            Assert.False(attachment.IsIntact);
            Assert.True(attachment.IsSimulated);

            rope.cutTime = 0f;
            Assert.Equal(RopeAttachmentState.Inert, attachment.State);
            Assert.False(attachment.IsSimulated);
        }

        [Fact]
        public void ImmediateCutGoesStraightToInert()
        {
            // GameScene.Systems.cs zeroes cutTime for an immediate cut; no fade, no Severing frame.
            RopeAttachment attachment = new();
            Bungee rope = MakeRope();
            _ = attachment.TryAttach(rope);

            rope.SetCut(0);
            rope.cutTime = 0f;

            Assert.Equal(RopeAttachmentState.Inert, attachment.State);
        }

        [Fact]
        public void ReleaseReturnsToIdleButKeepsTheSourceExhausted()
        {
            // A ghost-morphed radius hook whose radius already faded must not re-attach.
            RopeAttachment attachment = new();
            _ = attachment.TryAttach(MakeRope());

            attachment.Release();

            Assert.Equal(RopeAttachmentState.Idle, attachment.State);
            Assert.Null(attachment.Rope);
            Assert.True(attachment.SourceExhausted);
        }

        [Fact]
        public void ReleaseOnIdleIsHarmless()
        {
            RopeAttachment attachment = new();

            attachment.Release();

            Assert.Equal(RopeAttachmentState.Idle, attachment.State);
        }
    }
}
