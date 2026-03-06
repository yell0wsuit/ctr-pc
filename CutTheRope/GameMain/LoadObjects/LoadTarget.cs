using System;
using System.Globalization;
using System.Xml.Linq;

using CutTheRope.Framework.Core;
using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Visual;

using static CutTheRope.Helpers.ParsingHelpers;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Handles loading Om Nom from XML level data
    /// Om Nom is the objective the candy must reach to complete the level
    /// </summary>
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Loads Om Nom from XML node data
        /// Sets up Om Nom animations, blink animation, and greeting if needed
        /// </summary>
        private void LoadTarget(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            int pack = ((CTRRootController)Application.SharedRootController()).GetPack();
            string supportResourceName = PackConfig.GetSupportResourceName(pack);

            // Clamp quad index to valid range; fall back to first quad if pack index exceeds available quads
            CTRTexture2D supportTexture = Application.GetTexture(supportResourceName);
            int quadIndex = (pack >= 0 && pack < supportTexture.quadRects.Length) ? pack : 0;

            support = Image.Image_createWithResIDQuad(supportResourceName, quadIndex);
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

            if (targetAnimationBackend is FlashXmlTargetAnimationBackend)
            {
                Console.WriteLine(
                    $"[OmNomFlashRootPos] xml=({sourceX},{sourceY}); mapScale={scale.ToString("0.###", CultureInfo.InvariantCulture)}; offset=({offsetX.ToString("0.###", CultureInfo.InvariantCulture)},{offsetY.ToString("0.###", CultureInfo.InvariantCulture)}); mapOffset=({mapOffsetX},{mapOffsetY}); dxWorld=({transformedX.ToString("0.###", CultureInfo.InvariantCulture)},{transformedY.ToString("0.###", CultureInfo.InvariantCulture)}); baseScale=({targetBaseScaleX.ToString("0.###", CultureInfo.InvariantCulture)},{targetBaseScaleY.ToString("0.###", CultureInfo.InvariantCulture)});");
            }

            targetObject.bb = MakeRectangle(264f, 350f, 108f, 2f);
            blinkTimer = BLINK_SKIP;

            // Show greeting if needed (skip for night levels)
            if (CTRRootController.IsShowGreeting())
            {
                if (!nightLevel)
                {
                    dd.CallObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(Selector_showGreeting), null, 1.3f);
                }

                CTRRootController.SetShowGreeting(false);
            }

            targetAnimationController.Initialize(this);
            idlesTimer = RND_RANGE(5, 20);
        }
    }
}
