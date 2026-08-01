using CutTheRopeDX.Framework.Core;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public sealed class ViewControllerChildTests
    {
        /// <summary>ViewController's constructors are protected; this exposes one for testing.</summary>
        private sealed class TestController : ViewController
        {
        }

        [Fact]
        public void GetChild_ReturnsNull_WhenChildNotRegistered()
        {
            TestController controller = new();

            Assert.Null(controller.GetChild(3));
        }
    }
}
