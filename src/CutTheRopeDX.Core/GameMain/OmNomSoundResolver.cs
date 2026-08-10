using System.Collections.Generic;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Resolves classic Om Nom sound resources to skin-specific variants when a skin declares them.
    /// </summary>
    internal static class OmNomSoundResolver
    {
        /// <summary>
        /// Maps classic Om Nom sound resource names to themed resource identifier suffixes.
        /// </summary>
        private static readonly Dictionary<string, string> ThemedIdentifierSuffixesByClassicSound = new()
        {
            [Resources.Snd.MonsterChewing] = "Chewing",
            [Resources.Snd.MonsterClose] = "MouthClose",
            [Resources.Snd.MonsterOpen] = "MouthOpen",
            [Resources.Snd.MonsterSad] = "Sad",
            [Resources.Snd.MonsterExcited] = "Excited",
            [Resources.Snd.MonsterGreeting] = "Greeting",
            [Resources.Snd.MonsterSleep1] = "Sleep01",
            [Resources.Snd.MonsterSleep2] = "Sleep02",
            [Resources.Snd.MonsterSleep3] = "Sleep03",
        };

        /// <summary>
        /// Resolves a classic Om Nom sound to a themed replacement when the selected skin supports it.
        /// </summary>
        /// <param name="skinDefinition">Skin definition to resolve against, or <see langword="null" /> for the classic skin.</param>
        /// <param name="classicSoundResourceName">Classic sound resource name.</param>
        /// <returns>The themed sound resource, the original classic resource, or <see langword="null" /> when an opt-in-only sound is unavailable.</returns>
        public static string ResolveSoundResource(OmNomSkinDefinition skinDefinition, string classicSoundResourceName)
        {
            if (string.IsNullOrWhiteSpace(classicSoundResourceName))
            {
                return classicSoundResourceName;
            }

            bool isOptInOnlySound = classicSoundResourceName is Resources.Snd.MonsterExcited or Resources.Snd.MonsterGreeting;

            return skinDefinition == null
                ? isOptInOnlySound
                    ? null
                    : classicSoundResourceName
                : isOptInOnlySound && !skinDefinition.HasUniqueSound(classicSoundResourceName)
                ? null
                : !skinDefinition.HasUniqueSound(classicSoundResourceName)
                ? classicSoundResourceName
                : ResolveThemedSoundResourceName(skinDefinition.Name, classicSoundResourceName);
        }

        /// <summary>
        /// Resolves a classic Om Nom sound using the currently selected skin.
        /// </summary>
        /// <param name="classicSoundResourceName">Classic sound resource name.</param>
        /// <returns>The selected skin's themed sound resource, the original classic resource, or <see langword="null" /> for unavailable opt-in-only sounds.</returns>
        public static string ResolveSelectedSkinSoundResource(string classicSoundResourceName)
        {
            return ResolveSoundResource(GetSelectedXmlSkinDefinition(), classicSoundResourceName);
        }

        /// <summary>
        /// Gets the XML-backed selected skin definition, excluding the classic skin.
        /// </summary>
        /// <returns>The selected XML skin definition, or <see langword="null" /> when the classic skin is selected.</returns>
        private static OmNomSkinDefinition GetSelectedXmlSkinDefinition()
        {
            int skinIndex = OmNomSkinRegistry.GetSelectedSkinIndex();
            return OmNomSkinRegistry.IsClassicSkin(skinIndex)
                ? null
                : OmNomSkinRegistry.GetXmlSkinDefinition(skinIndex);
        }

        /// <summary>
        /// Builds and resolves the themed sound resource name for a specific skin and classic sound.
        /// </summary>
        /// <param name="skinName">Name of the themed Om Nom skin.</param>
        /// <param name="classicSoundResourceName">Classic sound resource name.</param>
        /// <returns>The resolved themed sound resource name, or the original classic resource name when no themed resource exists.</returns>
        private static string ResolveThemedSoundResourceName(string skinName, string classicSoundResourceName)
        {
            if (string.IsNullOrWhiteSpace(skinName)
                || !ThemedIdentifierSuffixesByClassicSound.TryGetValue(classicSoundResourceName, out string suffix))
            {
                return classicSoundResourceName;
            }

            string themedSoundIdentifier = "TT" + skinName + suffix;
            return Resources.TryResolveSoundIdentifier(themedSoundIdentifier, out string themedSoundResourceName)
                ? themedSoundResourceName
                : classicSoundResourceName;
        }
    }
}
