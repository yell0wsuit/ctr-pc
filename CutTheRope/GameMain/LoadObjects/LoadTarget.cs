using System.Xml.Linq;

using CutTheRope.Framework.Core;
using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Visual;

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

            CharAnimations target = CharAnimations.CharAnimations_createWithResID(Resources.Img.CharAnimations);
            target.DoRestoreCutTransparency();
            target.passColorToChilds = false;

            string xAttribute = xmlNode.Attribute("x")?.Value ?? string.Empty;
            target.x = support.x = (ParseIntOrZero(xAttribute) * scale) + offsetX + mapOffsetX;

            string yAttribute = xmlNode.Attribute("y")?.Value ?? string.Empty;
            target.y = support.y = (ParseIntOrZero(yAttribute) * scale) + offsetY + mapOffsetY;

            target.bb = MakeRectangle(264f, 350f, 108f, 2f);

            targetAnimationController = TargetAnimationController.Create(target, nightLevel, SpecialEvents.IsXmas);
            targetObject = targetAnimationController.TargetObject;
            sleepAnimPrimary = targetAnimationController.SleepAnimationPrimary;
            sleepAnimSecondary = targetAnimationController.SleepAnimationSecondary;
            blink = targetAnimationController.Blink;
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
