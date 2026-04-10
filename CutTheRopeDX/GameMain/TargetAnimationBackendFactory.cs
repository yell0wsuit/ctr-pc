namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Factory for target animation backends used by the active Om Nom skin.
    /// </summary>
    internal static class TargetAnimationBackendFactory
    {
        /// <summary>
        /// Creates the target animation backend for the currently selected Om Nom skin.
        /// </summary>
        /// <param name="isNightLevel">Whether the current level uses night-mode animation variants.</param>
        /// <param name="isXmas">Whether the current level uses Christmas animation variants.</param>
        /// <returns>The original backend for the classic skin, or a Flash XML backend for XML-backed skins.</returns>
        public static ITargetAnimationBackend CreateOriginal(bool isNightLevel, bool isXmas)
        {
            int skinIndex = OmNomSkinRegistry.GetSelectedSkinIndex();
            return OmNomSkinRegistry.IsClassicSkin(skinIndex)
                ? new OriginalTargetAnimationBackend(isNightLevel, isXmas)
                : new FlashXmlTargetAnimationBackend(
                OmNomSkinRegistry.GetXmlSkinDefinition(skinIndex));
        }
    }
}
