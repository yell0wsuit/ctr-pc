using System.Collections.Generic;

using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class CustomLevelReloadDecisionTests
    {
        [Fact]
        public void Decide_IdenticalResources_IsInstant()
        {
            CustomLevelReloadKind kind = CustomLevelReloadDecision.Decide(
                ["spider", "sock"],
                new HashSet<string> { "spider", "sock" });

            Assert.Equal(CustomLevelReloadKind.Instant, kind);
        }

        [Fact]
        public void Decide_SubsetOfLoaded_IsInstant()
        {
            CustomLevelReloadKind kind = CustomLevelReloadDecision.Decide(
                ["spider"],
                new HashSet<string> { "spider", "sock", "bouncer" });

            Assert.Equal(CustomLevelReloadKind.Instant, kind);
        }

        [Fact]
        public void Decide_NoRequirements_IsInstant()
        {
            CustomLevelReloadKind kind = CustomLevelReloadDecision.Decide(
                [],
                new HashSet<string> { "spider" });

            Assert.Equal(CustomLevelReloadKind.Instant, kind);
        }

        [Fact]
        public void Decide_OneNewResource_NeedsLoadingScreen()
        {
            CustomLevelReloadKind kind = CustomLevelReloadDecision.Decide(
                ["spider", "rocket"],
                new HashSet<string> { "spider", "sock" });

            Assert.Equal(CustomLevelReloadKind.LoadingScreen, kind);
        }

        [Fact]
        public void Decide_DisjointResources_NeedsLoadingScreen()
        {
            CustomLevelReloadKind kind = CustomLevelReloadDecision.Decide(
                ["rocket"],
                new HashSet<string> { "spider" });

            Assert.Equal(CustomLevelReloadKind.LoadingScreen, kind);
        }

        [Fact]
        public void Decide_NullRequirements_IsInstant()
        {
            CustomLevelReloadKind kind = CustomLevelReloadDecision.Decide(
                null,
                new HashSet<string> { "spider" });

            Assert.Equal(CustomLevelReloadKind.Instant, kind);
        }
    }
}
