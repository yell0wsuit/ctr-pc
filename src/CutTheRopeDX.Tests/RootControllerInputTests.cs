using CutTheRopeDX.Framework.Core;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class RootControllerInputTests
    {
        [Fact]
        public void MouseMoveBeforeAChildControllerIsActiveIsIgnored()
        {
            RootController root = new(null);

            Assert.False(root.MouseMoved(10f, 20f));
        }

        [Fact]
        public void RootCannotBecomeItsOwnInputTarget()
        {
            RootController root = new(null);

            root.SetCurrentController(root);

            Assert.Null(root.GetCurrentController());
        }

        [Fact]
        public void InputBeforeAChildControllerIsActiveIsIgnored()
        {
            RootController root = new(null);
            TouchLocation[] touches = [];

            Assert.False(root.BackButtonPressed());
            Assert.False(root.MenuButtonPressed());
            Assert.False(root.TouchesBeganwithEvent(touches));
            Assert.False(root.TouchesMovedwithEvent(touches));
            Assert.False(root.TouchesEndedwithEvent(touches));
            Assert.False(root.TouchesCancelledwithEvent(touches));
        }
    }
}
