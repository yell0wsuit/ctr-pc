using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.Framework.Platform
{
    /// <summary>
    /// The asset-loading operations that require a graphics device. Swapping this is what
    /// lets the game run headless; everything else in the engine is device-independent.
    /// </summary>
    internal interface IAssetPlatform
    {
        /// <summary>Pixel dimensions of an image, or <see langword="null"/> when it is missing.</summary>
        /// <param name="contentPath">Content-relative path, e.g. <c>images/obj_candy</c>.</param>
        /// <returns>The image's pixel size, or <see langword="null"/> when the asset is missing.</returns>
        (int W, int H)? ImageDimensions(string contentPath);

        /// <summary>The backing texture, or <see langword="null"/> when running without a device.</summary>
        /// <param name="contentPath">Content-relative path, e.g. <c>images/obj_candy</c>.</param>
        /// <returns>The loaded texture handle, or <see langword="null"/>.</returns>
        ITextureHandle ImageTexture(string contentPath);

        /// <summary>Releases the cached content manager backing an image, if any.</summary>
        /// <param name="contentPath">Content-relative path, e.g. <c>images/obj_candy</c>.</param>
        void FreeImage(string contentPath);

        /// <summary>Loads a font by logical resource name.</summary>
        /// <param name="resourceName">Logical font resource name.</param>
        /// <returns>The loaded font.</returns>
        FontGeneric Font(string resourceName);

        /// <summary>Clears any cached font resources held by the platform's font loader.</summary>
        void ClearFontCache();
    }
}
