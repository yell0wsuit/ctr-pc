using System.Runtime.CompilerServices;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public sealed class CandyAttachmentsTests
    {
        private static T Uninitialized<T>() where T : class
        {
            return (T)RuntimeHelpers.GetUninitializedObject(typeof(T));
        }

        [Fact]
        public void OwnerCheckedReleasesCannotClearAnotherDevicesAttachment()
        {
            CandyAttachments attachments = new();
            Rocket firstRocket = Uninitialized<Rocket>();
            Rocket otherRocket = Uninitialized<Rocket>();
            MechanicalHand firstHand = Uninitialized<MechanicalHand>();
            MechanicalHand otherHand = Uninitialized<MechanicalHand>();

            Assert.True(attachments.BindRocket(firstRocket));
            Assert.True(attachments.CaptureByHand(firstHand));

            Assert.False(attachments.TryReleaseRocket(otherRocket));
            Assert.False(attachments.TryReleaseHand(otherHand));
            Assert.Same(firstRocket, attachments.Rocket);
            Assert.Same(firstHand, attachments.Hand);

            Assert.True(attachments.TryReleaseRocket(firstRocket));
            Assert.True(attachments.TryReleaseHand(firstHand));
            Assert.Null(attachments.Rocket);
            Assert.Null(attachments.Hand);
        }

        [Fact]
        public void ResetAntsClearsTheWholeAntInteractionTogether()
        {
            CandyAttachments attachments = new();
            AntsPathSegment segment = new(new Vector(0f, 0f), new Vector(100f, 0f), 20f, 1f);

            Assert.True(attachments.BeginAntCarry(segment, new Vector(25f, 5f), 0.3f, 0.01f));
            attachments.SetAntWaitingForExit(true);
            attachments.AdvanceAntCarry(0.5f);

            attachments.ResetAnts();

            Assert.Null(attachments.AntSegment);
            Assert.Null(attachments.LastAntSegment);
            Assert.Equal(0f, attachments.AntCooldown);
            Assert.False(attachments.AntWaitingForExit);
            Assert.Equal(default, attachments.AntInteractionPoint);
            Assert.Equal(0f, attachments.AntInteractionTime);
        }

        [Fact]
        public void DetachAllReturnsFormerOwnersAndLeavesNoAttachmentState()
        {
            CandyAttachments attachments = new();
            Rocket rocket = Uninitialized<Rocket>();
            MechanicalHand hand = Uninitialized<MechanicalHand>();
            AntsPathSegment segment = new(new Vector(0f, 0f), new Vector(100f, 0f), 20f, 1f);
            _ = attachments.CaptureInLantern();
            _ = attachments.BindRocket(rocket);
            _ = attachments.CaptureByHand(hand);
            _ = attachments.BeginAntCarry(segment, new Vector(25f, 5f), 0.3f, 0.01f);
            CandyAttachmentSnapshot detached = attachments.DetachAll();

            Assert.Same(rocket, detached.Rocket);
            Assert.Same(hand, detached.Hand);
            Assert.Same(segment, detached.AntSegment);
            Assert.True(detached.InLantern);
            Assert.False(attachments.HasAny);
            Assert.False(attachments.SuppressGravity);
            Assert.Null(attachments.LastAntSegment);
            Assert.Equal(0f, attachments.AntCooldown);
        }

        [Fact]
        public void DetachForTransportKeepsRocketButReleasesConflictingCarriers()
        {
            CandyAttachments attachments = new();
            Rocket rocket = Uninitialized<Rocket>();
            MechanicalHand hand = Uninitialized<MechanicalHand>();
            AntsPathSegment segment = new(new Vector(0f, 0f), new Vector(100f, 0f), 20f, 1f);
            _ = attachments.BindRocket(rocket);
            _ = attachments.CaptureByHand(hand);
            _ = attachments.BeginAntCarry(segment, new Vector(25f, 5f), 0.3f, 0.01f);
            CandyAttachmentSnapshot detached = attachments.DetachForTransport();

            Assert.Same(hand, detached.Hand);
            Assert.Same(segment, detached.AntSegment);
            Assert.Null(detached.Rocket);
            Assert.Same(rocket, attachments.Rocket);
            Assert.Null(attachments.Hand);
            Assert.Null(attachments.AntSegment);
        }

        [Fact]
        public void CaptureInLanternAtomicallyReleasesEveryConflictingCarrier()
        {
            CandyAttachments attachments = new();
            Rocket rocket = Uninitialized<Rocket>();
            MechanicalHand hand = Uninitialized<MechanicalHand>();
            AntsPathSegment segment = new(new Vector(0f, 0f), new Vector(100f, 0f), 20f, 1f);
            _ = attachments.BindRocket(rocket);
            _ = attachments.CaptureByHand(hand);
            _ = attachments.BeginAntCarry(segment, new Vector(25f, 5f), 0.3f, 0.01f);
            CandyAttachmentSnapshot detached = attachments.CaptureInLantern();

            Assert.Same(rocket, detached.Rocket);
            Assert.Same(hand, detached.Hand);
            Assert.Same(segment, detached.AntSegment);
            Assert.False(detached.InLantern);
            Assert.True(attachments.InLantern);
            Assert.Null(attachments.Rocket);
            Assert.Null(attachments.Hand);
            Assert.Null(attachments.AntSegment);
            Assert.True(attachments.SuppressGravity);
        }
    }
}
