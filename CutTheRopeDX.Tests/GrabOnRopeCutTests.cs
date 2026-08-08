using CutTheRopeDX.Framework;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>Verifies the single hook-side reaction to its own rope being cut.</summary>
    public class GrabOnRopeCutTests
    {
        private static Grab NewHook()
        {
            // Grab's constructor reaches Application.SharedRootController().
            _ = HeadlessGame.Boot();
            return new Grab();
        }

        [Fact]
        public void PlainHookHasNoCutExclusionZone()
        {
            Grab hook = NewHook();

            Assert.Null(hook.CutExclusionZone);
        }

        [Fact]
        public void WheelHookExcludesItsTapZoneFromCuts()
        {
            Grab hook = NewHook();
            hook.x = 100f;
            hook.y = 200f;
            hook.Wheel = new WheelControl();

            Assert.True(hook.CutExclusionZone.HasValue);
            CTRRectangle zone = hook.CutExclusionZone.Value;
            Assert.Equal(100f - WheelControl.TapHalfExtent, zone.x);
            Assert.Equal(WheelControl.TapHalfExtent * 2f, zone.w);
        }

        [Fact]
        public void GunHookExcludesItsCutRadius()
        {
            Grab hook = NewHook();
            hook.x = 100f;
            hook.y = 200f;
            hook.Source = new GunSource();

            Assert.True(hook.CutExclusionZone.HasValue);
            CTRRectangle zone = hook.CutExclusionZone.Value;
            Assert.Equal(100f - Grab.GUN_CUT_RADIUS, zone.x);
            Assert.Equal(Grab.GUN_CUT_RADIUS * 2f, zone.w);
        }

        [Fact]
        public void OnRopeCutBustsAWalkingSpider()
        {
            Grab hook = NewHook();
            hook.Spider = new SpiderRider();
            hook.Spider.Arm(ropeAttachedToCandy: true);
            hook.Spider.Activate();

            hook.OnRopeCut(RopeCutReason.Severed);

            Assert.Equal(SpiderRiderState.Busted, hook.Spider.State);
        }

        [Fact]
        public void OnRopeCutLeavesADormantSpiderAlone()
        {
            Grab hook = NewHook();
            hook.Spider = new SpiderRider();

            hook.OnRopeCut(RopeCutReason.Severed);

            Assert.Equal(SpiderRiderState.Dormant, hook.Spider.State);
        }
    }
}
