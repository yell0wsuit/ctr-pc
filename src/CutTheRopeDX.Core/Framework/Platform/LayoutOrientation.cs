namespace CutTheRopeDX.Framework.Platform
{
    /// <summary>
    /// Which way round the current viewport is. Square counts as landscape.
    /// </summary>
    internal enum LayoutOrientation
    {
        /// <summary>Width is greater than or equal to height.</summary>
        Landscape,

        /// <summary>Height is greater than width.</summary>
        Portrait,
    }
}
