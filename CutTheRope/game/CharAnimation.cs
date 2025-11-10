using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.visual;

namespace CutTheRope.game
{
    internal class CharAnimation : Animation
    {
        public static CharAnimation CharAnimation_create(CTRTexture2D t)
        {
            return (CharAnimation)new CharAnimation().initWithTexture(t);
        }

        public static CharAnimation CharAnimation_createWithResID(int r)
        {
            return CharAnimation_create(Application.getTexture(r));
        }

        public override bool handleAction(ActionData a)
        {
            if (a.actionName == "ACTION_PLAY_TIMELINE")
            {
                if (a.actionParam == 1)
                {
                    parent.color = RGBAColor.transparentRGBA;
                }
                playTimeline(a.actionSubParam);
                return true;
            }
            return base.handleAction(a);
        }
    }
}
