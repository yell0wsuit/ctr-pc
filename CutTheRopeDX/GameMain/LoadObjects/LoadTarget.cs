using System.Xml.Linq;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Visual;

using static CutTheRopeDX.Helpers.ParsingHelpers;

namespace CutTheRopeDX.GameMain
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

            int targetType = ParseIntOrZero(xmlNode.Attribute("targetType")?.Value ?? string.Empty);
            ITargetAnimationBackend targetAnimationBackend = TargetAnimationBackendFactory.CreateForTarget(
                targetType, nightLevel, SpecialEvents.IsXmas);
            TargetAnimationController controller = TargetAnimationController.Create(targetAnimationBackend);
            GameObject targetObj = controller.TargetObject;
            targetBaseScaleX = controller.GetTargetBaseScaleX();
            targetBaseScaleY = controller.GetTargetBaseScaleY();
            targetObj.scaleX = targetBaseScaleX;
            targetObj.scaleY = targetBaseScaleY;

            string xAttribute = xmlNode.Attribute("x")?.Value ?? string.Empty;
            int sourceX = ParseCoordinateIntOrZero(xAttribute);
            float transformedX = (sourceX * scale) + offsetX + mapOffsetX;
            targetObj.x = support.x = transformedX;

            string yAttribute = xmlNode.Attribute("y")?.Value ?? string.Empty;
            int sourceY = ParseCoordinateIntOrZero(yAttribute);
            float transformedY = (sourceY * scale) + offsetY + mapOffsetY;
            targetObj.y = support.y = transformedY;

            // Mouth hitbox: 56 px left of center, 30 px below center.
            // Derived from classic char_animations (640x640): bb = (264, 350, 108, 2).
            targetObj.bb = MakeRectangle((targetObj.width >> 1) - 56f, (targetObj.height >> 1) + 30f, 108f, 2f);

            controller.Initialize(this);

            // Register this Om Nom as an independent target. targets[0] stays the primary.
            targets.Add(new TargetContext
            {
                controller = controller,
                targetObject = targetObj,
                support = support,
                baseScaleX = targetBaseScaleX,
                baseScaleY = targetBaseScaleY,
                mouthOpen = false,
                mouthCloseTimer = 0f,
                asleep = false,
                blinkTimer = BLINK_SKIP,
                idlesTimer = RND_RANGE(5, 20),
            });

            // Show greeting if needed (skip for night levels).
            // Skins with startWithGreeting already play greeting on init, so skip the delayed call.
            if (CTRRootController.IsShowGreeting())
            {
                if (!nightLevel && !controller.StartsWithGreeting)
                {
                    dd.CallObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(Selector_showGreeting), null, 1.3f);
                }

                CTRRootController.SetShowGreeting(false);
            }

            support = targets[0].support;
            targetBaseScaleX = targets[0].baseScaleX;
            targetBaseScaleY = targets[0].baseScaleY;
        }
    }
}
