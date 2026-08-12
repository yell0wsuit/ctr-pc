using System;

using SkiaSharp;

namespace CutTheRopeDX.Browser
{
    /// <summary>
    /// Owns the Skia GPU context and the surface wrapping the WebGL2 default framebuffer.
    /// Rebuilds the surface when the canvas resizes; the GRContext outlives that.
    /// </summary>
    internal sealed class SkiaSurface : IDisposable
    {
        private const int ResourceCacheBytes = 128 * 1024 * 1024;

        private readonly GRGlInterface _glInterface;
        private readonly int _fboId;

        private GRBackendRenderTarget _renderTarget;
        private SKSurface _surface;

        public SkiaSurface(int fboId, int width, int height)
        {
            _fboId = fboId;
            _glInterface = GRGlInterface.Create()
                ?? throw new InvalidOperationException("Could not create the Skia GL interface.");
            Context = GRContext.CreateGl(_glInterface)
                ?? throw new InvalidOperationException("Could not create the Skia GL context.");
            Context.SetResourceCacheLimit(ResourceCacheBytes);
            Rebuild(width, height);
        }

        /// <summary>The canvas drawing into the WebGL2 default framebuffer.</summary>
        public SKCanvas Canvas => _surface.Canvas;

        /// <summary>The Skia GPU context, needed to upload textures and create render targets.</summary>
        public GRContext Context { get; }

        /// <summary>Current surface width in backing pixels.</summary>
        public int Width { get; private set; }

        /// <summary>Current surface height in backing pixels.</summary>
        public int Height { get; private set; }

        /// <summary>Rebuilds the surface when the canvas size changed; a no-op otherwise.</summary>
        public void Resize(int width, int height)
        {
            if (width == Width && height == Height)
            {
                return;
            }
            Rebuild(width, height);
        }

        private void Rebuild(int width, int height)
        {
            _surface?.Dispose();
            _renderTarget?.Dispose();

            Width = width;
            Height = height;

            GRGlFramebufferInfo info = new((uint)_fboId, SKColorType.Rgba8888.ToGlSizedFormat());
            _renderTarget = new GRBackendRenderTarget(width, height, 0, 8, info);
            _surface = SKSurface.Create(
                Context, _renderTarget, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888)
                ?? throw new InvalidOperationException("Could not create the Skia surface.");
        }

        /// <summary>Submits the frame's GL commands.</summary>
        public void Flush()
        {
            _surface.Canvas.Flush();
            Context.Flush();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _surface?.Dispose();
            _renderTarget?.Dispose();
            Context?.Dispose();
            _glInterface?.Dispose();
        }
    }
}
