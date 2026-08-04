namespace CutTheRopeDX.Desktop.Graphics
{
    /// <summary>
    /// The outcome of a graphics backend decision.
    /// </summary>
    /// <param name="NeedsProbe">Whether the caller must run the Vulkan probe to reach a final answer.</param>
    /// <param name="UseSoftware">Whether to route rendering through the bundled SwiftShader library.</param>
    /// <param name="ShowNotice">Whether to warn the user that rendering will be done in software.</param>
    /// <param name="ModeToPersist">Value to store under <see cref="GraphicsMode.PreferenceKey"/>, or <see langword="null"/> when nothing should be written.</param>
    internal readonly record struct GraphicsDecision(
        bool NeedsProbe,
        bool UseSoftware,
        bool ShowNotice,
        string ModeToPersist);
}
