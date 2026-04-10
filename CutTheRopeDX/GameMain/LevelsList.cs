using System;

using CutTheRopeDX.Framework;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Provides generated level map filenames indexed by pack and level.
    /// </summary>
    internal sealed class LevelsList : FrameworkTypes
    {
        /// <summary>
        /// Builds the level filename table from the configured pack and level counts.
        /// </summary>
        static LevelsList()
        {
            int packCount = PackConfig.GetPackCount();
            int maxLevels = Math.Max(1, PackConfig.MaxLevelsPerPack);

            LEVEL_NAMES = new string[packCount, maxLevels];

            for (int pack = 0; pack < packCount; pack++)
            {
                int levelCount = PackConfig.GetLevelCount(pack);
                for (int level = 0; level < levelCount; level++)
                {
                    LEVEL_NAMES[pack, level] = string.Concat(pack + 1, "_", level + 1, ".xml");
                }
            }
        }

        /// <summary>
        /// Level map filenames indexed by zero-based pack and zero-based level.
        /// </summary>
        public static string[,] LEVEL_NAMES;
    }
}
