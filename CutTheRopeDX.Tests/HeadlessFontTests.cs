using CutTheRopeDX.Framework.Platform;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public sealed class HeadlessFontTests
    {
        [Fact]
        public void TotalCharmaps_IsAtLeastOne()
        {
            // Text.UpdateDrawerValues indexes an array sized by this; zero throws.
            Assert.True(new HeadlessFont().TotalCharmaps() >= 1);
        }

        [Fact]
        public void StringWidth_IsProportionalToLength()
        {
            HeadlessFont font = new();

            Assert.True(font.StringWidth("aaaa") > font.StringWidth("aa"));
        }
    }
}
