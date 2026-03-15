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
