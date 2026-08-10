using System;

namespace CutTheRopeDX.Framework.Platform
{
    /// <summary>
    /// Opaque platform texture. Core passes it around; only the active render
    /// backend knows the concrete type behind it.
    /// </summary>
    internal interface ITextureHandle : IDisposable
    {
        int Width { get; }
        int Height { get; }
    }
}
