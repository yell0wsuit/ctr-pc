using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    internal sealed class CharAnimation : Animation
    {
        public static CharAnimation CharAnimation_create(CTRTexture2D t)
        {
            return (CharAnimation)new CharAnimation().InitWithTexture(t);
        }

        public static CharAnimation CharAnimation_createWithResID(int r)
        {
            return CharAnimation_create(Application.GetTexture(ResourceNameTranslator.TranslateLegacyId(r)));
        }

        public static CharAnimation CharAnimation_createWithResID(string resourceName)
        {
            return CharAnimation_create(Application.GetTexture(resourceName));
        }

        public override bool HandleAction(ActionData a)
        {
            if (a.actionName == "ACTION_PLAY_TIMELINE")
            {
                if (a.actionParam == 1)
                {
                    parent.color = RGBAColor.transparentRGBA;
                }
                PlayTimeline(a.actionSubParam);
                return true;
            }
            return base.HandleAction(a);
        }
    }
}
