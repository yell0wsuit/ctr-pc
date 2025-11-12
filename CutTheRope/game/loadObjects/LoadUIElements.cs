using CutTheRope.desktop;
using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.sfe;
using CutTheRope.iframework.visual;
using CutTheRope.ios;

namespace CutTheRope.game
{
    /// <summary>
    /// GameScene.LoadUIElements - Partial class handling UI element loading from XML
    /// Includes gravity switch and target character
    /// </summary>
    internal sealed partial class GameScene
    {
        private void LoadGravitySwitch(XMLNode item, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            if (item.Name != "gravitySwitch") return;

            gravityButton = CreateGravityButtonWithDelegate(this);
            gravityButton.visible = false;
            gravityButton.touchable = false;
            _ = AddChild(gravityButton);
            gravityButton.x = (item["x"].IntValue() * scale) + offsetX + mapOffsetX;
            gravityButton.y = (item["y"].IntValue() * scale) + offsetY + mapOffsetY;
            gravityButton.anchor = 18;
        }

        private void LoadTarget(XMLNode item, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            if (item.Name != "target") return;

            int pack = ((CTRRootController)Application.SharedRootController()).GetPack();
            support = Image.Image_createWithResIDQuad(100, pack);
            support.Retain();
            support.DoRestoreCutTransparency();
            support.anchor = 18;
            target = CharAnimations.CharAnimations_createWithResID(80);
            target.DoRestoreCutTransparency();
            target.passColorToChilds = false;
            NSString nSString3 = item["x"];
            target.x = support.x = (nSString3.IntValue() * scale) + offsetX + mapOffsetX;
            NSString nSString4 = item["y"];
            target.y = support.y = (nSString4.IntValue() * scale) + offsetY + mapOffsetY;
            target.AddImage(101);
            target.AddImage(102);
            target.bb = MakeRectangle(264.0, 350.0, 108.0, 2.0);
            target.AddAnimationWithIDDelayLoopFirstLast(0, 0.05f, Timeline.LoopType.TIMELINE_REPLAY, 0, 18);
            target.AddAnimationWithIDDelayLoopFirstLast(1, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 43, 67);
            int num14 = 68;
            target.AddAnimationWithIDDelayLoopCountSequence(2, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 32, num14,
            [
                num14 + 1, num14 + 2, num14 + 3, num14 + 4, num14 + 5, num14 + 6, num14 + 7, num14 + 8,
                num14 + 9, num14 + 10, num14 + 11, num14 + 12, num14 + 13, num14 + 14, num14 + 15, num14,
                num14 + 1, num14 + 2, num14 + 3, num14 + 4, num14 + 5, num14 + 6, num14 + 7, num14 + 8,
                num14 + 9, num14 + 10, num14 + 11, num14 + 12, num14 + 13, num14 + 14, num14 + 15
            ]);
            target.AddAnimationWithIDDelayLoopFirstLast(7, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 19, 27);
            target.AddAnimationWithIDDelayLoopFirstLast(8, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 28, 31);
            target.AddAnimationWithIDDelayLoopFirstLast(9, 0.05f, Timeline.LoopType.TIMELINE_REPLAY, 32, 40);
            target.AddAnimationWithIDDelayLoopFirstLast(6, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 28, 31);
            target.AddAnimationWithIDDelayLoopFirstLast(101, 10, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 47, 76);
            target.AddAnimationWithIDDelayLoopFirstLast(101, 3, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 0, 19);
            target.AddAnimationWithIDDelayLoopFirstLast(101, 4, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 20, 46);
            target.AddAnimationWithIDDelayLoopFirstLast(102, 5, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 0, 12);
            target.SwitchToAnimationatEndOfAnimationDelay(9, 6, 0.05f);
            target.SwitchToAnimationatEndOfAnimationDelay(101, 4, 80, 8, 0.05f);
            target.SwitchToAnimationatEndOfAnimationDelay(80, 0, 101, 10, 0.05f);
            target.SwitchToAnimationatEndOfAnimationDelay(80, 0, 80, 1, 0.05f);
            target.SwitchToAnimationatEndOfAnimationDelay(80, 0, 80, 2, 0.05f);
            target.SwitchToAnimationatEndOfAnimationDelay(80, 0, 101, 3, 0.05f);
            target.SwitchToAnimationatEndOfAnimationDelay(80, 0, 101, 4, 0.05f);
            target.Retain();

            if (CTRRootController.IsShowGreeting())
            {
                dd.CallObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(Selector_showGreeting), null, 1.3f);
                CTRRootController.SetShowGreeting(false);
            }
            target.PlayTimeline(0);
            target.GetTimeline(0).delegateTimelineDelegate = this;
            target.SetPauseAtIndexforAnimation(8, 7);
            blink = Animation.Animation_createWithResID(80);
            blink.parentAnchor = 9;
            blink.visible = false;
            blink.AddAnimationWithIDDelayLoopCountSequence(0, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 4, 41, [41, 42, 42, 42]);
            blink.SetActionTargetParamSubParamAtIndexforAnimation("ACTION_SET_VISIBLE", blink, 0, 0, 2, 0);
            blinkTimer = 3;
            blink.DoRestoreCutTransparency();
            _ = target.AddChild(blink);
            idlesTimer = RND_RANGE(5, 20);
        }
    }
}
