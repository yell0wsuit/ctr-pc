using System.Collections.Generic;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Resolves classic Om Nom sound resources to skin-specific variants when a skin declares them.
    /// </summary>
    internal static class OmNomSoundResolver
    {
        private static readonly Dictionary<string, string> SuffixesByClassicSound = new()
        {
            [Resources.Snd.MonsterChewing] = "chewing",
            [Resources.Snd.MonsterClose] = "mouthClose",
            [Resources.Snd.MonsterOpen] = "mouthOpen",
            [Resources.Snd.MonsterSad] = "sad",
            [Resources.Snd.MonsterExcited] = "excited",
            [Resources.Snd.MonsterGreeting] = "greeting",
            [Resources.Snd.MonsterSleep1] = "sleep01",
            [Resources.Snd.MonsterSleep2] = "sleep02",
            [Resources.Snd.MonsterSleep3] = "sleep03",
        };

        public static string ResolveSoundResource(OmNomSkinDefinition skinDefinition, string classicSoundResourceName)
        {
            if (string.IsNullOrWhiteSpace(classicSoundResourceName))
            {
                return classicSoundResourceName;
            }

            bool isOptInOnlySound = classicSoundResourceName is Resources.Snd.MonsterExcited or Resources.Snd.MonsterGreeting;

            if (skinDefinition == null)
            {
                return isOptInOnlySound
                    ? null
                    : classicSoundResourceName;
            }

            if (isOptInOnlySound && !skinDefinition.HasUniqueSound(classicSoundResourceName))
            {
                return null;
            }

            if (!skinDefinition.HasUniqueSound(classicSoundResourceName))
            {
                return classicSoundResourceName;
            }

            if (string.IsNullOrWhiteSpace(skinDefinition.Name)
                || !SuffixesByClassicSound.TryGetValue(classicSoundResourceName, out string suffix))
            {
                return classicSoundResourceName;
            }

            string resolvedSound = skinDefinition.Name + "_" + suffix;
            return Resources.IsSound(resolvedSound)
                ? resolvedSound
                : classicSoundResourceName;
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
    }
}
