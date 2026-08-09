using System;
using System.Reflection;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public sealed class PointerGestureStateTests
    {
        private const BindingFlags InstanceFields = BindingFlags.Instance
            | BindingFlags.Public
            | BindingFlags.NonPublic;

        [Fact]
        public void GameSceneOwnsOnePointerGestureCollectionInsteadOfParallelArrays()
        {
            FieldInfo[] fields = typeof(GameScene).GetFields(InstanceFields);

            FieldInfo gestures = Assert.Single(
                fields,
                field => field.FieldType == typeof(PointerGestureState[]));
            Assert.Equal("pointerGestures", gestures.Name);
            Assert.DoesNotContain(
                fields,
                field => field.Name is "dragging"
                    or "startPos"
                    or "prevStartPos"
                    or "fingerTraceDownPos"
                    or "fingerTraceDragging"
                    or "fingerCuts"
                    or "fingerTraces");
        }

        [Fact]
        public void BeginMoveAndEndAdvanceOneGestureAtomically()
        {
            PointerGestureState gesture = new(FingerTraceFactory.Create(0));

            gesture.Begin(new Vector(2f, 3f), new Vector(12f, 13f));

            Assert.True(gesture.IsDragging);
            Assert.Equal(2f, gesture.StartPosition.X);
            Assert.Equal(3f, gesture.StartPosition.Y);
            Assert.Equal(12f, gesture.TraceDownPosition.X);
            Assert.Equal(13f, gesture.TraceDownPosition.Y);
            Assert.False(gesture.IsTraceDragging);

            Assert.True(gesture.Move(new Vector(5f, 7f), new Vector(15f, 17f), out Vector segmentStart));
            Assert.Equal(2f, segmentStart.X);
            Assert.Equal(3f, segmentStart.Y);
            Assert.False(gesture.IsTraceDragging);

            Assert.True(gesture.Move(new Vector(8f, 11f), new Vector(18f, 21f), out _));
            Assert.Equal(5f, gesture.PreviousStartPosition.X);
            Assert.Equal(7f, gesture.PreviousStartPosition.Y);
            Assert.True(gesture.IsTraceDragging);

            gesture.End();

            Assert.False(gesture.IsDragging);
            Assert.False(gesture.IsTraceDragging);
            Assert.False(gesture.Move(default, default, out _));
        }

        [Fact]
        public void CancelEndsEveryActivePartOfTheGesture()
        {
            PointerGestureState gesture = new(FingerTraceFactory.Create(0));
            gesture.Begin(default, default);
            _ = gesture.Move(new Vector(10f, 0f), new Vector(10f, 0f), out _);

            Assert.True(gesture.Trace.IsAlive);

            gesture.Cancel();

            Assert.False(gesture.IsDragging);
            Assert.False(gesture.IsTraceDragging);
            Assert.False(gesture.Trace.IsAlive);
        }

        [Fact]
        public void ResetClearsAllGestureStateForANewScene()
        {
            PointerGestureState gesture = new(FingerTraceFactory.Create(0));
            gesture.Begin(new Vector(2f, 3f), new Vector(12f, 13f));
            _ = gesture.Move(new Vector(12f, 3f), new Vector(22f, 13f), out _);
            gesture.Cuts.Add(new GameScene.FingerCut());

            gesture.Reset();

            Assert.False(gesture.IsDragging);
            Assert.False(gesture.IsTraceDragging);
            Assert.Equal(default, gesture.StartPosition);
            Assert.Equal(default, gesture.PreviousStartPosition);
            Assert.Equal(default, gesture.TraceDownPosition);
            Assert.Empty(gesture.Cuts);
            Assert.False(gesture.Trace.IsAlive);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(5)]
        public void SceneInputRejectsInvalidPointerBeforeGestureAccess(int pointerIndex)
        {
            _ = HeadlessGame.Boot();
            GameScene scene = HeadlessGame.LoadLevel(pack: 1, level: 4);
            HeadlessGame.StepFrames(scene, 60);

            Exception exception = Record.Exception(() =>
            {
                Assert.True(scene.TouchDownXYIndex(10_000f, 10_000f, pointerIndex));
                Assert.True(scene.TouchMoveXYIndex(10_000f, 10_000f, pointerIndex));
                Assert.True(scene.TouchUpXYIndex(10_000f, 10_000f, pointerIndex));
                Assert.False(scene.TouchDraggedXYIndex(10_000f, 10_000f, pointerIndex));
            });

            Assert.Null(exception);
        }
    }
}
