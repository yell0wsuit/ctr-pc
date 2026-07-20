using System;
using System.IO;

using CutTheRopeDX.Helpers;

using Microsoft.Xna.Framework.Content;

namespace CutTheRopeDX.Desktop
{
    /// <summary>
    /// Loads compiled content through managed file streams.
    /// </summary>
    /// <remarks>
    /// MonoGame 3.8.5's DesktopVK asset stream ignores the destination offset passed to
    /// <see cref="Stream.Read(byte[], int, int)"/>. LZ4 content requires non-zero-offset
    /// reads, so use a managed <see cref="FileStream"/> until the runtime implementation
    /// is corrected.
    /// </remarks>
    internal sealed class DesktopContentManager(IServiceProvider serviceProvider)
        : ContentManager(serviceProvider, ContentPaths.RootDirectory)
    {
        /// <inheritdoc />
        protected override Stream OpenStream(string assetName)
        {
            return ContentPaths.OpenStream(assetName + ".xnb");
        }
    }
}
