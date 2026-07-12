using System.Xml.Linq;

using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class NormalRopeLoadTests
    {
        [Fact]
        public void ShouldCreate_FalseWhenTargetCandyIsInLantern()
        {
            Assert.False(NormalRopeLoad.ShouldCreate(targetCandyInLantern: true));
            Assert.True(NormalRopeLoad.ShouldCreate(targetCandyInLantern: false));
        }

        [Fact]
        public void CandyStartsInLantern_TrueRegardlessOfObjectOrder()
        {
            XElement map = XElement.Parse("""
                <level>
                  <objects>
                    <grab radius="-1" />
                    <lantern candyCaptured="true" />
                  </objects>
                </level>
                """);

            Assert.True(NormalRopeLoad.CandyStartsInLantern(map));
        }
    }
}
