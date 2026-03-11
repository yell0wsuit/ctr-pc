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

            if (classicSoundResourceName == Resources.Snd.MonsterExcited)
            {
                if (skinDefinition == null)
                {
                    return null;
                }

                if (skinDefinition.HasUniqueSound(classicSoundResourceName)
                    && string.IsNullOrWhiteSpace(skinDefinition.UniqueSoundSet))
                {
                    return null;
                }
            }

            if (skinDefinition == null || !skinDefinition.HasUniqueSound(classicSoundResourceName))
            {
                return classicSoundResourceName;
            }

            if (string.IsNullOrWhiteSpace(skinDefinition.UniqueSoundSet)
                || !SuffixesByClassicSound.TryGetValue(classicSoundResourceName, out string suffix))
            {
                return classicSoundResourceName;
            }

            string resolvedSound = skinDefinition.UniqueSoundSet + "_" + suffix;
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
