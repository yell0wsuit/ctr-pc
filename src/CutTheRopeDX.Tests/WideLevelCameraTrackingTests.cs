using System.Reflection;

using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.GameMain;
using CutTheRopeDX.Tests.Interactions;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers a level wider than the design box. Such a level is laid out centered on the box, so
    /// its left edge is at a negative world X, and the design box alone never shows all of it -
    /// the camera has to travel, and the positions it must reach are not the ones a range measured
    /// from world zero would offer.
    /// </summary>
    public sealed class WideLevelCameraTrackingTests
    {
        /// <summary>
        /// Authored sizes and the camera positions that show each level's left and right edges.
        /// The second is proportionally narrower than a 16:9 viewport, which is the shape a fit
        /// that reasoned from aspect alone would wrongly call already-visible and hold still.
        /// </summary>
        /// <returns>Map width, map height, left-edge and right-edge camera positions.</returns>
        public static TheoryData<int, int, float, float> WideLevels()
        {
            return new TheoryData<int, int, float, float>
            {
                { 1400, 480, -820f, 820f },
                { 1200, 853, -520f, 520f },
            };
        }

        [Theory]
        [MemberData(nameof(WideLevels))]
        public void TheIntroPanStartsAtTheLevelsFarEdgeEvenWhenThatIsNegative(
            int mapWidth, int mapHeight, float leftEdge, float rightEdge)
        {
            GameScene scene = WideLevel(mapWidth, mapHeight, candyX: mapWidth - 40);

            Camera2D camera = ReadCamera(scene);

            // Candy on the right half, so the pan starts from the left edge - which on a level
            // this wide is off to the negative side of the design box.
            Assert.Equal(leftEdge, camera.pos.X, 0.01);
            Assert.Equal(leftEdge, camera.RenderPos.X, 0.01);
            Assert.True(rightEdge > leftEdge);
        }

        [Theory]
        [MemberData(nameof(WideLevels))]
        public void TrackingReachesTheLevelsFarEdgeFromTheOppositeStart(
            int mapWidth, int mapHeight, float leftEdge, float rightEdge)
        {
            GameScene scene = WideLevel(mapWidth, mapHeight, candyX: 40);

            Camera2D camera = ReadCamera(scene);
            Assert.Equal(rightEdge, camera.pos.X, 0.01);

            HeadlessGame.StepFrames(scene, 400);

            // The candy is past the left edge of what the camera can show, so the tracking pins
            // against that edge - and the whole traverse must actually be available to it, in the
            // drawn position as much as in the tracked one.
            Assert.Equal(leftEdge, camera.pos.X, 0.01);
            Assert.Equal(leftEdge, camera.RenderPos.X, 0.01);
        }

        private static GameScene WideLevel(int mapWidth, int mapHeight, int candyX)
        {
            return Scenario.New()
                .MapSize(mapWidth, mapHeight)
                .Candy(candyX, 60)
                .Rope(candyX, 30, length: 40)
                .OmNom(mapWidth / 2, mapHeight - 60)
                .Build();
        }

        private static Camera2D ReadCamera(GameScene scene)
        {
            return (Camera2D)typeof(GameScene)
                .GetField("camera", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(scene);
        }
    }
}
