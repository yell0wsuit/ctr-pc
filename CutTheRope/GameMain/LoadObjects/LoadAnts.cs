using System.Xml.Linq;

using CutTheRope.Framework.Core;

using static CutTheRope.Helpers.ParsingHelpers;

namespace CutTheRope.GameMain
{
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Parses an ant-path XML node and adds the resulting <see cref="AntsPath"/> and its segments
        /// to the scene's ant conveyor lists.
        /// </summary>
        /// <param name="xmlNode">The XML node describing the ants path.</param>
        /// <param name="scale">The level scale factor applied to path coordinates and speed.</param>
        /// <param name="offsetX">The base X offset applied to loaded objects.</param>
        /// <param name="offsetY">The base Y offset applied to loaded objects.</param>
        /// <param name="mapOffsetX">The additional map X offset applied during loading.</param>
        /// <param name="mapOffsetY">The additional map Y offset applied during loading.</param>
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
