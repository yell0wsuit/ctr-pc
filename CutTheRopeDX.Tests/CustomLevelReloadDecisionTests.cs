using System.Collections.Generic;

using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class CustomLevelReloadDecisionTests
    {
        [Fact]
        public void DecideIdenticalResourcesIsInstant()
        {
            CustomLevelReloadKind kind = CustomLevelReloadDecision.Decide(
                ["spider", "sock"],
                new HashSet<string> { "spider", "sock" });

            Assert.Equal(CustomLevelReloadKind.Instant, kind);
        }

        [Fact]
        public void DecideSubsetOfLoadedIsInstant()
        {
            CustomLevelReloadKind kind = CustomLevelReloadDecision.Decide(
                ["spider"],
                new HashSet<string> { "spider", "sock", "bouncer" });

            Assert.Equal(CustomLevelReloadKind.Instant, kind);
        }

        [Fact]
        public void DecideNoRequirementsIsInstant()
        {
            CustomLevelReloadKind kind = CustomLevelReloadDecision.Decide(
                [],
                new HashSet<string> { "spider" });

            Assert.Equal(CustomLevelReloadKind.Instant, kind);
        }

        [Fact]
        public void DecideOneNewResourceNeedsLoadingScreen()
        {
            CustomLevelReloadKind kind = CustomLevelReloadDecision.Decide(
                ["spider", "rocket"],
                new HashSet<string> { "spider", "sock" });

            Assert.Equal(CustomLevelReloadKind.LoadingScreen, kind);
        }

        [Fact]
        public void DecideDisjointResourcesNeedsLoadingScreen()
        {
            CustomLevelReloadKind kind = CustomLevelReloadDecision.Decide(
                ["rocket"],
                new HashSet<string> { "spider" });

            Assert.Equal(CustomLevelReloadKind.LoadingScreen, kind);
        }

        [Fact]
        public void DecideNullRequirementsIsInstant()
        {
            CustomLevelReloadKind kind = CustomLevelReloadDecision.Decide(
                null,
                new HashSet<string> { "spider" });

            Assert.Equal(CustomLevelReloadKind.Instant, kind);
        }
    }
}
