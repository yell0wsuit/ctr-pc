namespace CutTheRopeDX.Framework.Helpers
{
    /// <summary>
    /// Movement modes supported by <see cref="Camera2D"/>.
    /// </summary>
    public enum CAMERATYPE
    {
        /// <summary>
        /// Moves toward the target using a fixed speed measured in pixels per second.
        /// </summary>
        CAMERASPEEDPIXELS,

        /// <summary>
        /// Moves toward the target using a proportional offset based on the current distance.
        /// </summary>
        CAMERASPEEDDELAY
    }
}
