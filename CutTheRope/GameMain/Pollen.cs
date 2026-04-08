namespace CutTheRope.GameMain
{
    /// <summary>
    /// Runtime state for one animated pollen particle.
    /// </summary>
    internal struct Pollen
    {
        /// <summary>Path point index that owns or originated this pollen particle.</summary>
        public int parentIndex;

        /// <summary>World-space X position.</summary>
        public float x;

        /// <summary>World-space Y position.</summary>
        public float y;

        /// <summary>Current horizontal scale.</summary>
        public float scaleX;

        /// <summary>Current vertical scale.</summary>
        public float scaleY;

        /// <summary>Scale value to return to on the horizontal axis.</summary>
        public float startScaleX;

        /// <summary>Scale value to return to on the vertical axis.</summary>
        public float startScaleY;

        /// <summary>Current target scale on the horizontal axis.</summary>
        public float endScaleX;

        /// <summary>Current target scale on the vertical axis.</summary>
        public float endScaleY;

        /// <summary>Current particle alpha.</summary>
        public float alpha;

        /// <summary>Alpha value to return to after reaching <see cref="endAlpha"/>.</summary>
        public float startAlpha;

        /// <summary>Current target alpha.</summary>
        public float endAlpha;
    }
}
