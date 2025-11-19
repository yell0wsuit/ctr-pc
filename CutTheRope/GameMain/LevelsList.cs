using System;

using CutTheRope.Framework;

namespace CutTheRope.GameMain
{
    internal sealed class LevelsList : FrameworkTypes
    {
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

        public static string[,] LEVEL_NAMES;
    }
}
