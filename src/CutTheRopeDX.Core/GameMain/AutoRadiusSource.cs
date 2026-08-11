using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Visual;

using static CutTheRopeDX.Framework.Helpers.CTRMathHelper;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// A hook that creates its rope when a candy body enters its radius. One shot: attaching starts
    /// a fade that ends with the radius set to -1, after which the hook can never attach again.
    /// </summary>
    internal sealed class AutoRadiusSource : RopeSource
    {
        /// <summary>Alpha lost per second once the radius starts fading.</summary>
        private const float FadeRatePerSecond = 1.5f;

        private Vector anchor;

        /// <summary>Initializes a radius source and builds its circle.</summary>
        /// <param name="radius">The attach radius in world units.</param>
        /// <param name="anchor">The hook's world position.</param>
        public AutoRadiusSource(float radius, Vector anchor)
        {
            Radius = radius;
            this.anchor = anchor;
            RadiusAlpha = 1f;
            IsFading = false;

            VertexCount = (int)MAX(16f, radius);
            VertexCount /= 2;
            if (VertexCount % 2 != 0)
            {
                VertexCount++;
            }

            Vertices = new float[VertexCount * 2];
            DrawHelper.CalcCircle(anchor.X, anchor.Y, radius, VertexCount, Vertices);
        }

        /// <summary>Gets the attach radius, or -1 once the radius has finished fading out.</summary>
        public float Radius { get; private set; }

        /// <summary>Gets the alpha multiplier for the radius circle.</summary>
        public float RadiusAlpha { get; private set; }

        /// <summary>Gets whether the radius circle is fading out after an attach.</summary>
        public bool IsFading { get; private set; }

        /// <summary>Gets the cached circle vertex positions.</summary>
        public float[] Vertices { get; }

        /// <summary>Gets the number of circle vertices stored in <see cref="Vertices"/>.</summary>
        public int VertexCount { get; }

        /// <inheritdoc />
        public override bool CanAttach => Radius != -1f;

        /// <summary>Gets whether the radius circle should be drawn this frame.</summary>
        public bool ShouldDrawCircle => Radius != -1f || IsFading;

        /// <summary>Starts the post-attach fade of the radius circle.</summary>
        public void BeginFade()
        {
            IsFading = true;
        }

        /// <summary>Determines whether a candy point is inside the attach range.</summary>
        /// <param name="hookPosition">The hook's world position.</param>
        /// <param name="candyPosition">The candy point's world position.</param>
        /// <returns><see langword="true"/> when the candy is within radius plus grab padding.</returns>
        public bool InRange(Vector hookPosition, Vector candyPosition)
        {
            return VectDistance(hookPosition, candyPosition)
                <= Radius + ActivePhysicsConstants.CandyGrabPadding;
        }

        /// <inheritdoc />
        public override void Update(float delta)
        {
            if (!IsFading)
            {
                return;
            }

            RadiusAlpha -= FadeRatePerSecond * delta;
            if (RadiusAlpha <= 0f)
            {
                Radius = -1f;
                IsFading = false;
            }
        }

        /// <inheritdoc />
        public override void OnAnchorMoved(Vector position)
        {
            anchor = position;
            DrawHelper.CalcCircle(anchor.X, anchor.Y, Radius, VertexCount, Vertices);
        }
    }
}
