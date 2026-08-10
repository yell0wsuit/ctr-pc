namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Text for the in-game level label shown when a level starts.
    /// </summary>
    /// <param name="Primary">Large upper line, or <see langword="null"/> when no label should be shown.</param>
    /// <param name="Secondary">Small lower line, or <see langword="null"/> when only the primary line applies.</param>
    internal readonly record struct LevelLabelText(string Primary, string Secondary);

    /// <summary>
    /// Decides which lines the in-game level label shows. Performs no localization lookups.
    /// </summary>
    internal static class LevelLabel
    {
        /// <summary>
        /// Builds the label lines for a level start.
        /// </summary>
        /// <param name="isCustomLevel">Whether a custom level is being playtested.</param>
        /// <param name="displayName">Resolved level name, or <see langword="null"/>/empty when the level has none.</param>
        /// <param name="levelWord">Localized word for "Level".</param>
        /// <param name="packAndLevelNumbers">Pack and level numbers, such as <c>1 - 1</c>.</param>
        /// <returns>The lines to render.</returns>
        public static LevelLabelText Resolve(
            bool isCustomLevel,
            string displayName,
            string levelWord,
            string packAndLevelNumbers)
        {
            bool hasName = !string.IsNullOrWhiteSpace(displayName);

            // Playtest runs have no meaningful pack or level number, so the numbering is dropped
            // entirely and an unnamed level shows no label at all.
            return isCustomLevel
                ? hasName ? new LevelLabelText(displayName, null) : new LevelLabelText(null, null)
                : hasName
                    ? new LevelLabelText(displayName, levelWord + " " + packAndLevelNumbers)
                    : new LevelLabelText(packAndLevelNumbers, levelWord);
        }
    }
}
