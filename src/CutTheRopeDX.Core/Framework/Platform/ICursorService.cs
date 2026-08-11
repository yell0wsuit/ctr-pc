namespace CutTheRopeDX.Framework.Platform
{
    /// <summary>Hardware cursor control. Optional; null when absent (headless/web).</summary>
    internal interface ICursorService
    {
        void Enable(bool enabled);
        void ReleaseButtons();
    }
}
