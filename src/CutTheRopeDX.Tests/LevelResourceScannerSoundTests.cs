using System.Xml.Linq;

using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Guards that the level scanner reports the sound effects and action-triggered
    /// textures a level can reach, so they warm on the loading screen instead of
    /// stalling the game thread the first time an object acts.
    /// </summary>
    public class LevelResourceScannerSoundTests
    {
        private static string[] Scan(string mapXml)
        {
            return LevelResourceScanner.GetRequiredResources(XElement.Parse(mapXml));
        }

        private static string[] ScanWithObject(string objectXml)
        {
            return Scan($"<map><gameDesign>{objectXml}</gameDesign></map>");
        }

        [Fact]
        public void EveryLevelRequiresTheFingerTraceTextures()
        {
            string[] resources = Scan("<map><gameDesign /></map>");

            Assert.Contains(Resources.Img.FingerTraces, resources);
            Assert.Contains(Resources.Img.FingerTraceGlow, resources);
        }

        [Fact]
        public void AHatPastTheAuthoredGroupsRequiresTheBandAtlas()
        {
            // The band is drawn from its own atlas, and a level that draws it without listing it
            // reads the texture off disk on the game thread the first time a hat appears.
            string[] resources = ScanWithObject("<sock x=\"20\" y=\"40\" group=\"2\" />");

            Assert.Contains(Resources.Img.ObjHat, resources);
            Assert.Contains(Resources.Img.ObjHatMaskable, resources);
        }

        [Fact]
        public void AHatRequiresItsTeleportSound()
        {
            string[] resources = ScanWithObject("<sock x=\"20\" y=\"40\" group=\"0\" />");

            Assert.Contains(Resources.Snd.Teleport, resources);
        }

        [Fact]
        public void AHatWithinTheAuthoredGroupsNeedsNoBandAtlas()
        {
            string[] resources = ScanWithObject("<sock x=\"20\" y=\"40\" group=\"1\" />");

            Assert.Contains(Resources.Img.ObjHat, resources);
            Assert.DoesNotContain(Resources.Img.ObjHatMaskable, resources);
        }

        [Fact]
        public void EveryLevelRequiresTheCoreGameplaySounds()
        {
            string[] resources = Scan("<map><gameDesign /></map>");

            Assert.Contains(Resources.Snd.CandyBreak, resources);
            Assert.Contains(Resources.Snd.RopeBleak1, resources);
            Assert.Contains(Resources.Snd.RopeGet, resources);
            Assert.Contains(Resources.Snd.Star1, resources);
            Assert.Contains(Resources.Snd.Win, resources);
        }

        [Fact]
        public void BouncerLevelRequiresTheBouncerSound()
        {
            Assert.Contains(Resources.Snd.Bouncer, ScanWithObject("<bouncer1 />"));
        }

        [Fact]
        public void PumpLevelRequiresEveryPumpSoundVariant()
        {
            string[] resources = ScanWithObject("<pump />");

            Assert.Contains(Resources.Snd.Pump1, resources);
            Assert.Contains(Resources.Snd.Pump2, resources);
            Assert.Contains(Resources.Snd.Pump3, resources);
            Assert.Contains(Resources.Snd.Pump4, resources);
        }

        [Fact]
        public void RocketLevelRequiresTheRocketSounds()
        {
            string[] resources = ScanWithObject("<rocket />");

            Assert.Contains(Resources.Snd.ExpRocketStart, resources);
            Assert.Contains(Resources.Snd.ExpRocketFlyLooped, resources);
            Assert.Contains(Resources.Snd.ExpRocketInWater, resources);
        }

        [Fact]
        public void SpiderGrabRequiresTheSpiderSounds()
        {
            string[] resources = ScanWithObject("<grab spider=\"true\" />");

            Assert.Contains(Resources.Snd.SpiderActivate, resources);
            Assert.Contains(Resources.Snd.SpiderFall, resources);
            Assert.Contains(Resources.Snd.SpiderWin, resources);
        }

        [Fact]
        public void WheelGrabRequiresTheWheelSound()
        {
            Assert.Contains(Resources.Snd.Wheel, ScanWithObject("<grab wheel=\"true\" />"));
        }

        [Fact]
        public void PauseSwitcherRequiresItsTexturesAndSounds()
        {
            string[] resources = ScanWithObject("<pauseSwitcher />");

            Assert.Contains(Resources.Img.ObjPause, resources);
            Assert.Contains(Resources.Img.FxPause, resources);
            Assert.Contains(Resources.Snd.PauseDown, resources);
            Assert.Contains(Resources.Snd.PauseUp, resources);
        }

        [Fact]
        public void GravitySwitchRequiresTheGravitySounds()
        {
            string[] resources = ScanWithObject("<gravitySwitch />");

            Assert.Contains(Resources.Snd.GravityOn, resources);
            Assert.Contains(Resources.Snd.GravityOff, resources);
        }

        [Fact]
        public void ConnectedCandiesRequireTheCandyLinkSound()
        {
            Assert.Contains(Resources.Snd.CandyLink, ScanWithObject("<candyL /><candyR />"));
        }

        [Fact]
        public void WaterLevelRequiresTheSplashSound()
        {
            Assert.Contains(
                Resources.Snd.ExpWaterSplash,
                Scan("<map><gameDesign water=\"100\" /></map>"));
        }

        [Fact]
        public void RotatedCircleRequiresTheScratchSounds()
        {
            string[] resources = ScanWithObject("<rotatedCircle />");

            Assert.Contains(Resources.Snd.ScratchIn, resources);
            Assert.Contains(Resources.Snd.ScratchOut, resources);
        }

        [Fact]
        public void ConveyorRequiresItsMoveSounds()
        {
            string[] resources = ScanWithObject("<transporter />");

            Assert.Contains(Resources.Snd.TransporterMove, resources);
            Assert.Contains(Resources.Snd.TransporterDrop, resources);
            Assert.Contains(Resources.Snd.Conv01, resources);
            Assert.Contains(Resources.Snd.Conv04, resources);
        }

        [Fact]
        public void PlainLevelDoesNotRequireUnrelatedObjectSounds()
        {
            string[] resources = Scan("<map><gameDesign /></map>");

            Assert.DoesNotContain(Resources.Snd.ExpRocketStart, resources);
            Assert.DoesNotContain(Resources.Snd.Bouncer, resources);
            Assert.DoesNotContain(Resources.Snd.MouseIdle, resources);
            Assert.DoesNotContain(Resources.Snd.SteamStart, resources);
        }

        [Fact]
        public void EveryReportedResourceIsAKnownResourceName()
        {
            // The loader silently drops names the resource table rejects, so a typo here
            // would resurrect the very lazy-load hitch this scan exists to prevent.
            string[] resources = Scan(
                """
                <map>
                  <gameDesign water="100" nightLevel="true">
                    <target />
                    <grab spider="true" wheel="true" gun="true" kickable="true" bee="true" />
                    <bubble /><pump /><sock /><ghost /><rocket /><axe /><load /><pipe />
                    <ants /><lantern /><mouse /><transporter /><steamTube /><rotatedCircle />
                    <bouncer1 /><spike1 /><electro /><hand /><lightBulb /><star />
                    <gravitySwitch /><pauseSwitcher /><candyL /><candyR />
                    <tutorialText /><tutorial01 />
                  </gameDesign>
                </map>
                """);

            Assert.All(resources, static resourceName => Assert.True(
                Resources.IsValidResourceName(resourceName),
                $"'{resourceName}' is not a known resource name."));
        }
    }
}
