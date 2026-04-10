namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Preview rendering modes for Om Nom skin selection slots.
    /// </summary>
    internal enum OmNomSlotPreviewMode
    {
        /// <summary>Static classic Om Nom preview.</summary>
        ClassicStatic,

        /// <summary>Animated classic Om Nom preview.</summary>
        ClassicAnimated,

        /// <summary>Flash XML-backed Om Nom skin preview.</summary>
        Xml,
    }

    /// <summary>
    /// Chooses the preview mode for Om Nom skin selection slots.
    /// </summary>
    internal static class OmNomSlotPreviewPolicy
    {
        /// <summary>
        /// Resolves the preview mode for a skin slot.
        /// </summary>
        /// <param name="skinIndex">Skin slot index.</param>
        /// <param name="selectedIndex">Currently selected skin index.</param>
        /// <returns>The preview mode to use for the slot.</returns>
        public static OmNomSlotPreviewMode Resolve(int skinIndex, int selectedIndex)
        {
            return skinIndex == 0
                ? selectedIndex == 0
                    ? OmNomSlotPreviewMode.ClassicAnimated
                    : OmNomSlotPreviewMode.ClassicStatic
                : OmNomSlotPreviewMode.Xml;
        }
    }
}
