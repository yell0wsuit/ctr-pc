using CutTheRopeDX.Framework.Visual;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers the geometry walker that renders an element subtree as comparable text. Position
    /// and size only: visibility, color, draw order and texture selection are outside what it can
    /// detect.
    /// </summary>
    public sealed class ElementGeometryWalkerTests
    {
        [Fact]
        public void DescribeRendersOneLinePerElementIndentedByDepth()
        {
            BaseElement root = new() { width = 100, height = 50 };
            root.SetName("root");
            BaseElement child = new() { x = 10f, y = 20f, width = 30, height = 40 };
            child.SetName("child");
            _ = root.AddChild(child);

            string described = ElementGeometryWalker.Describe(root);

            Assert.Equal(
                "root 0 0 100 50 1 1 0\n  child 10 20 30 40 1 1 0\n",
                described);
        }

        [Fact]
        public void DescribeRecordsScaleAndRotation()
        {
            BaseElement root = new() { width = 10, height = 10 };
            root.SetName("root");
            root.scaleX = 2f;
            root.scaleY = 3f;
            root.rotation = 45f;

            Assert.Equal("root 0 0 10 10 2 3 45\n", ElementGeometryWalker.Describe(root));
        }

        [Fact]
        public void DescribeResolvesHiddenElementsRatherThanRecordingStaleCoordinates()
        {
            // A drawn tree never reaches hidden children, so their drawX/drawY keep whatever they
            // last held. The walker resolves every node itself, including hidden elements.
            BaseElement root = new() { x = 100f, y = 200f, width = 10, height = 10 };
            root.SetName("root");
            BaseElement hidden = new()
            {
                x = 5f,
                y = 5f,
                width = 5,
                height = 5,
                visible = false,
                parentAnchor = 9,
            };
            hidden.SetName("hidden");
            _ = root.AddChild(hidden);

            Assert.Equal(
                "root 100 200 10 10 1 1 0\n  hidden 105 205 5 5 1 1 0\n",
                ElementGeometryWalker.Describe(root));
        }

        [Fact]
        public void DescribeNeedsNoRendererEvenForAScaledTree()
        {
            // PreDraw would push a matrix here, and the test suite's render backend throws on
            // that. Describing must not go anywhere near it.
            BaseElement root = new() { width = 10, height = 10 };
            root.SetName("scaled");
            root.scaleX = 2f;

            Assert.Equal("scaled 0 0 10 10 2 1 0\n", ElementGeometryWalker.Describe(root));
        }

        [Fact]
        public void DescribeIsStableAcrossRepeatedCalls()
        {
            BaseElement root = new() { width = 10, height = 10 };
            root.SetName("root");

            Assert.Equal(ElementGeometryWalker.Describe(root), ElementGeometryWalker.Describe(root));
        }

        [Fact]
        public void UnnamedElementsAreIdentifiedByTypeSoLinesStayDistinguishable()
        {
            BaseElement root = new() { width = 10, height = 10 };

            Assert.StartsWith("BaseElement ", ElementGeometryWalker.Describe(root));
        }
    }
}
