using System.Xml.Linq;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Visual;

using static CutTheRopeDX.Helpers.ParsingHelpers;

namespace CutTheRopeDX.GameMain
{
    internal sealed partial class GameScene
    {
        /// <summary>Quad holding Paddington's suitcase on the Christmas support sheet.</summary>
        private const int PaddingtonSupportQuad = 1;

        /// <summary>
        /// Downward nudge applied to the suitcase so Om Nom sits on its lid rather than inside it.
        /// The iOS release offsets by 32/64/128 px across its resolution tiers, all of which are
        /// 32 px in its 205 px quad space; scaled onto this port's 640 px quads that is 100.
        /// </summary>
        private const float PaddingtonSupportOffsetY = 100f;

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

            int targetType = ParseIntOrZero(xmlNode.Attribute("targetType")?.Value ?? string.Empty);

            bool isClassicSkin = OmNomSkinRegistry.IsClassicSkin(
                OmNomSkinRegistry.ResolveTargetSkinIndex(
                    targetType,
                    OmNomSkinRegistry.GetSelectedSkinIndex(),
                    OmNomSkinRegistry.TotalSkinCount));
            bool isPaddington = SpecialEvents.IsJanuary && isClassicSkin;

            bool isPrimaryTarget = targets.Count == 0;
            bool paddingtonGreetingPending =
                isPaddington && isPrimaryTarget && !nightLevel && CTRRootController.IsShowGreeting();

            // Paddington seats Om Nom on the bear's suitcase instead of the pack's usual platform.
            string supportResource = isPaddington ? Resources.Img.CharSupportsXmas : Resources.Img.CharSupports;
            int requestedQuad = isPaddington ? PaddingtonSupportQuad : sittingPlatform;

            // Clamp quad index to valid range; fall back to first quad for invalid values.
            CTRTexture2D supportTexture = Application.GetTexture(supportResource);
            int quadIndex = (requestedQuad >= 0 && requestedQuad < supportTexture.quadRects.Length) ? requestedQuad : 0;

            support = Image.Image_createWithResIDQuad(supportResource, quadIndex);
            support.DoRestoreCutTransparency();
            support.anchor = 18;

            ITargetAnimationBackend targetAnimationBackend = TargetAnimationBackendFactory.CreateForTarget(
                targetType, nightLevel, SpecialEvents.IsXmas, isPaddington, paddingtonGreetingPending);
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
            if (isPaddington)
            {
                support.y += PaddingtonSupportOffsetY;
            }

            // Mouth hitbox, center-relative so skins of any size keep the same mouth line.
            // Desktop: derived from classic char_animations (640x640): bb = (264, 350, 108, 2).
            // Mobile: WP7 bb (90, 110, 25, 1) scaled x3 onto the same 640x640 sheet = (270, 330, 75, 3).
            targetObj.bb = ActivePhysicsConstants.UseMobilePhysicsModel
                ? MakeRectangle((targetObj.width >> 1) - 50f, (targetObj.height >> 1) + 10f, 75f, 3f)
                : MakeRectangle((targetObj.width >> 1) - 56f, (targetObj.height >> 1) + 30f, 108f, 2f);

            controller.Initialize(this);

            // Register this Om Nom as an independent target. targets[0] stays the primary.
            targets.Add(new TargetContext(BLINK_SKIP, RND_RANGE(5, 20))
            {
                controller = controller,
                targetObject = targetObj,
                support = support,
                baseScaleX = targetBaseScaleX,
                baseScaleY = targetBaseScaleY,
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
