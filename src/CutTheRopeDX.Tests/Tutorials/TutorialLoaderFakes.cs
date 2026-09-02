using System.Collections.Generic;
using System.Xml.Linq;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;
using CutTheRopeDX.GameMain.Tutorials;

namespace CutTheRopeDX.Tests.Tutorials
{
    /// <summary>
    /// Stands in for the scene's real text and sign visuals, so loader tests can run the
    /// production parse without a graphics device, and records what was instantiated.
    /// </summary>
    internal sealed class FakeVisualFactory : ITutorialVisualFactory
    {
        internal List<XElement> CreatedNodes { get; } = [];

        /// <summary>Colors the loader asked each sign to be built with, in creation order.</summary>
        internal List<RGBAColor?> SignColors { get; } = [];

        public BaseElement CreateText(XElement node, float x, float y, float width)
        {
            CreatedNodes.Add(node);
            return new BaseElement { x = x, y = y, width = (int)width };
        }

        public BaseElement CreateSign(XElement node, int quad, float x, float y, RGBAColor? color)
        {
            CreatedNodes.Add(node);
            SignColors.Add(color);
            return new CTRGameObject { x = x, y = y };
        }
    }

    /// <summary>A world with nothing in it, for tests that never sample candy state.</summary>
    internal sealed class EmptyWorld : ITutorialWorld
    {
        public IReadOnlyList<CandyBody> ActiveBodies => [];

        public IReadOnlyList<TutorialRocketState> Rockets => [];

        public bool Holds(TutorialEvent tutorialEvent, CandyBody body)
        {
            return false;
        }
    }
}
