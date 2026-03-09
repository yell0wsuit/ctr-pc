namespace CutTheRope.GameMain
{
    internal static class TargetAnimationBackendFactory
    {
        public static ITargetAnimationBackend CreateOriginal(bool isNightLevel, bool isXmas)
        {
            int skinIndex = OmNomSkinRegistry.GetSelectedSkinIndex();
            if (OmNomSkinRegistry.IsClassicSkin(skinIndex))
            {
                return new OriginalTargetAnimationBackend(isNightLevel, isXmas);
            }

            return new FlashXmlTargetAnimationBackend(
                OmNomSkinRegistry.GetXmlSkinDefinition(skinIndex));
        }
    }
}
