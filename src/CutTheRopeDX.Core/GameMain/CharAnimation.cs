using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Om Nom character animation that handles timeline-switch actions emitted by the animation graph.
    /// </summary>
    internal sealed class CharAnimation : Animation
    {
        /// <summary>
        /// Creates an Om Nom character animation from a texture.
        /// </summary>
        /// <param name="t">Texture used by the character animation.</param>
        /// <returns>The initialized character animation.</returns>
        public static CharAnimation CharAnimation_create(CTRTexture2D t)
        {
            return (CharAnimation)new CharAnimation().InitWithTexture(t);
        }

        /// <summary>
        /// Creates an Om Nom character animation from a texture resource name.
        /// </summary>
        /// <param name="resourceName">Texture resource name to load.</param>
        /// <returns>The initialized character animation.</returns>
        public static CharAnimation CharAnimation_createWithResID(string resourceName)
        {
            return CharAnimation_create(Application.GetTexture(resourceName));
        }

        /// <inheritdoc />
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
