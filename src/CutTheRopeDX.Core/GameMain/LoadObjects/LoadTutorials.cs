using System.Collections.Generic;
using System.Xml.Linq;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.GameMain.Tutorials;

namespace CutTheRopeDX.GameMain
{
    internal sealed partial class GameScene
    {
        /// <summary>Validates and loads all tutorial elements as one ordered batch.</summary>
        /// <param name="nodes">Tutorial elements in XML order.</param>
        /// <param name="scale">Map-to-world coordinate scale.</param>
        /// <param name="offsetX">Base world-space X offset.</param>
        /// <param name="offsetY">Base world-space Y offset.</param>
        /// <param name="mapOffsetX">Additional authored-map X offset.</param>
        /// <param name="mapOffsetY">Additional authored-map Y offset.</param>
        private void LoadTutorials(
            IEnumerable<XElement> nodes,
            float scale,
            float offsetX,
            float offsetY,
            int mapOffsetX,
            int mapOffsetY)
        {
            CTRRootController rootController = (CTRRootController)Application.SharedRootController();
            TutorialPromptLoader loader = new(
                tutorialDirector,
                new TutorialVisualFactory(),
                rootController.GetMapName(),
                LanguageHelper.CurrentCode,
                levelAuthorsSplitCandy,
                scale,
                offsetX,
                offsetY,
                mapOffsetX,
                mapOffsetY);
            _ = loader.LoadAll(nodes);
        }
    }
}
