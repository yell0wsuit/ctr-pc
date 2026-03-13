using System.Xml.Linq;

using CutTheRope.Framework.Core;

using static CutTheRope.Helpers.ParsingHelpers;

namespace CutTheRope.GameMain
{
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Parses an ant-path XML node and adds the resulting <see cref="AntsPath"/> and its segments
        /// to the scene's conveyor lists.
        /// </summary>
        private void LoadAnts(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            float moveSpeed = ParseFloatOrZero(xmlNode.Attribute("moveSpeed")?.Value);
            string path = xmlNode.Attribute("path")?.Value ?? string.Empty;
            float x = ParseFloatOrZero(xmlNode.Attribute("x")?.Value);
            float y = ParseFloatOrZero(xmlNode.Attribute("y")?.Value);

            float scaledSpeed = moveSpeed * scale;
            Vector position = new(x, y);

            AntsPath antsPath = new(
                position,
                path,
                scaledSpeed,
                offsetX + mapOffsetX,
                offsetY + mapOffsetY,
                scale);

            antsPaths.Add(antsPath);

            foreach (AntsPathSegment segment in antsPath.Segments)
            {
                antsPathsSegments.Add(segment);
            }
        }
    }
}
