namespace CutTheRope.GameMain
{
    internal static class TargetAnimationBackendFactory
    {
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
