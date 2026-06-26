namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Factory for target animation backends used by the active Om Nom skin.
    /// </summary>
    internal static class TargetAnimationBackendFactory
    {
        /// <summary>
        /// Creates the target animation backend for a level <c>targetType</c> value.
        /// </summary>
        /// <param name="targetType">The level-supplied target type; <c>0</c> or out-of-range
        /// defers to the player's selected skin.</param>
        /// <param name="isNightLevel">Whether the current level uses night-mode animation variants.</param>
        /// <param name="isXmas">Whether the current level uses Christmas animation variants.</param>
        /// <returns>The original backend for the classic skin, or a Flash XML backend for XML-backed skins.</returns>
        public static ITargetAnimationBackend CreateForTarget(int targetType, bool isNightLevel, bool isXmas)
        {
            int skinIndex = OmNomSkinRegistry.ResolveTargetSkinIndex(
                targetType,
                OmNomSkinRegistry.GetSelectedSkinIndex(),
                OmNomSkinRegistry.TotalSkinCount);

            return OmNomSkinRegistry.IsClassicSkin(skinIndex)
                ? new OriginalTargetAnimationBackend(isNightLevel, isXmas)
                : new FlashXmlTargetAnimationBackend(OmNomSkinRegistry.GetXmlSkinDefinition(skinIndex));
        }
    }
}
