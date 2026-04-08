using System.Xml.Linq;

using CutTheRope.Framework.Core;
using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Visual;

using static CutTheRope.Helpers.ParsingHelpers;

namespace CutTheRope.GameMain
{
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Loads Om Nom from XML node data
        /// Sets up Om Nom animations, blink animation, and greeting if needed
        /// </summary>
        /// <param name="xmlNode">The XML node describing Om Nom.</param>
        /// <param name="scale">The level scale factor applied to object coordinates.</param>
        /// <param name="offsetX">The base X offset applied to loaded objects.</param>
        /// <param name="offsetY">The base Y offset applied to loaded objects.</param>
        /// <param name="mapOffsetX">The additional map X offset applied during loading.</param>
        /// <param name="mapOffsetY">The additional map Y offset applied during loading.</param>
        private void LoadTarget(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            int pack = ((CTRRootController)Application.SharedRootController()).GetPack();
            int sittingPlatform = PackConfig.GetSittingPlatform(pack);

            // Clamp quad index to valid range; fall back to first quad for invalid values.
            CTRTexture2D supportTexture = Application.GetTexture(Resources.Img.CharSupports);
            int quadIndex = (sittingPlatform >= 0 && sittingPlatform < supportTexture.quadRects.Length) ? sittingPlatform : 0;

            support = Image.Image_createWithResIDQuad(Resources.Img.CharSupports, quadIndex);
            support.DoRestoreCutTransparency();
            support.anchor = 18;

            ITargetAnimationBackend targetAnimationBackend = TargetAnimationBackendFactory.CreateOriginal(nightLevel, SpecialEvents.IsXmas);
            targetAnimationController = TargetAnimationController.Create(targetAnimationBackend);
            targetObject = targetAnimationController.TargetObject;
            targetBaseScaleX = targetAnimationController.GetTargetBaseScaleX();
            targetBaseScaleY = targetAnimationController.GetTargetBaseScaleY();
            targetObject.scaleX = targetBaseScaleX;
            targetObject.scaleY = targetBaseScaleY;

            string xAttribute = xmlNode.Attribute("x")?.Value ?? string.Empty;
            int sourceX = ParseIntOrZero(xAttribute);
            float transformedX = (sourceX * scale) + offsetX + mapOffsetX;
            targetObject.x = support.x = transformedX;

            string yAttribute = xmlNode.Attribute("y")?.Value ?? string.Empty;
            int sourceY = ParseIntOrZero(yAttribute);
            float transformedY = (sourceY * scale) + offsetY + mapOffsetY;
            targetObject.y = support.y = transformedY;

            // Mouth hitbox: 56 px left of center, 30 px below center.
            // Derived from classic char_animations (640x640): bb = (264, 350, 108, 2).
            targetObject.bb = MakeRectangle((targetObject.width >> 1) - 56f, (targetObject.height >> 1) + 30f, 108f, 2f);
            blinkTimer = BLINK_SKIP;

            targetAnimationController.Initialize(this);

            // Show greeting if needed (skip for night levels).
            // Skins with startWithGreeting already play greeting on init, so skip the delayed call.
            if (CTRRootController.IsShowGreeting())
            {
                if (!nightLevel && !targetAnimationController.StartsWithGreeting)
                {
                    dd.CallObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(Selector_showGreeting), null, 1.3f);
                }

                CTRRootController.SetShowGreeting(false);
            }

            idlesTimer = RND_RANGE(5, 20);
        }
    }
}
