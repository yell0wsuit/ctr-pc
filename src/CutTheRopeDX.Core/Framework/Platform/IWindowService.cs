namespace CutTheRopeDX.Framework.Platform
{
    /// <summary>OS-window commands Core may issue. Optional; null when headless.</summary>
    internal interface IWindowService
    {
        bool IsFullScreen { get; }
        void ToggleFullScreen();
        void ApplyWindowSize(int width);
    }
}
