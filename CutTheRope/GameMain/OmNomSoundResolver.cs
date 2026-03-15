using System.Collections.Generic;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Resolves classic Om Nom sound resources to skin-specific variants when a skin declares them.
    /// </summary>
    internal static class OmNomSoundResolver
    {
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

        public static string ResolveSelectedSkinSoundResource(string classicSoundResourceName)
        {
            return ResolveSoundResource(GetSelectedXmlSkinDefinition(), classicSoundResourceName);
        }

        private static OmNomSkinDefinition GetSelectedXmlSkinDefinition()
        {
            int skinIndex = OmNomSkinRegistry.GetSelectedSkinIndex();
            return OmNomSkinRegistry.IsClassicSkin(skinIndex)
                ? null
                : OmNomSkinRegistry.GetXmlSkinDefinition(skinIndex);
        }

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
