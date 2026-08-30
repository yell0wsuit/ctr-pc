using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.Framework.Visual;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class BaseElementBlendStateTests
    {
        [Fact]
        public void AdditiveSubtreeRestoresBlendStateBeforeFollowingSiblingDraws()
        {
            RecordingRenderBackend renderer = new();
            PlatformServices.Render = renderer;

            try
            {
                Renderer.SetBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONEMINUSSRCALPHA);
                BaseElement root = new();
                _ = root.AddChild(new BaseElement { blendingMode = 2 });
                BlendProbeElement followingSibling = new(renderer);
                _ = root.AddChild(followingSibling);

                root.Draw();

                Assert.Equal(BlendingFactor.GLSRCALPHA, followingSibling.BlendSourceAtDraw);
                Assert.Equal(BlendingFactor.GLONEMINUSSRCALPHA, followingSibling.BlendDestinationAtDraw);
            }
            finally
            {
                PlatformServices.Render = new ThrowingRenderBackend();
            }
        }

        private sealed class BlendProbeElement(RecordingRenderBackend renderer) : BaseElement
        {
            public BlendingFactor BlendSourceAtDraw { get; private set; }

            public BlendingFactor BlendDestinationAtDraw { get; private set; }

            public override void Draw()
            {
                BlendSourceAtDraw = renderer.LastBlendSource;
                BlendDestinationAtDraw = renderer.LastBlendDestination;
            }
        }
    }
}
