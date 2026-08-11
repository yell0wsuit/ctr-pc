using System;
using System.Collections.Generic;
using System.Numerics;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.Framework.Visual;

using SkiaSharp;

namespace CutTheRopeDX.Browser
{
    /// <summary>
    /// The GL-ES-1 style rendering surface, implemented over Skia's GPU canvas.
    /// </summary>
    /// <remarks>
    /// Vertices are baked on the CPU by <see cref="QuadBaking"/> before Skia ever sees
    /// them, so the matrix stack, skew, and premultiplied tint behave exactly as they do on
    /// desktop and Skia only ever receives finished triangles. Quads accumulate into a
    /// batch that flushes when the texture or blend mode changes, mirroring the desktop
    /// backend's one-draw-per-batch behavior.
    /// </remarks>
    /// <param name="surface">The Skia surface wrapping the WebGL2 framebuffer.</param>
    internal sealed class SkiaRenderBackend(SkiaSurface surface) : IRenderBackend
    {
        private const int GL_BLEND = 1;
        private const int GL_SCISSOR_TEST = 4;
        private const int MODE_PROJECTION = 15;

        private readonly MatrixStack _matrices = new();
        private readonly List<SKPoint> _positions = [];
        private readonly List<SKPoint> _texCoords = [];
        private readonly List<SKColor> _colors = [];

        private SKSurface _renderTarget;
        private int _renderTargetWidth;
        private int _renderTargetHeight;
        private SkiaTexture _boundTexture;
        private SkiaTexture _batchTexture;
        private SKBlendMode _blendMode = SKBlendMode.SrcOver;
        private SKBlendMode _batchBlendMode = SKBlendMode.SrcOver;
        private Color _drawColor = new(255, 255, 255, 255);
        private SKColor _clearColor = SKColors.Black;
        private bool _blendEnabled = true;
        private bool _projectionMode;

        /// <summary>The canvas currently targeted by draw calls.</summary>
        internal SKCanvas Target => _renderTarget?.Canvas ?? surface.Canvas;

        /// <inheritdoc />
        public bool IsAvailable => true;

        /// <inheritdoc />
        public void Enable(int cap)
        {
            if (cap == GL_BLEND)
            {
                _blendEnabled = true;
            }
        }

        /// <inheritdoc />
        public void Disable(int cap)
        {
            if (cap == GL_BLEND)
            {
                _blendEnabled = false;
            }
            else if (cap == GL_SCISSOR_TEST)
            {
                FlushQuads();
                Target.Restore();
            }
        }

        /// <inheritdoc />
        public void SetViewport(int x, int y, int width, int height)
        {
            if (width <= 0 || height <= 0)
            {
                return;
            }

            FlushQuads();
            surface.Resize(width, height);
            if (width == _renderTargetWidth && height == _renderTargetHeight)
            {
                return;
            }

            _renderTarget?.Dispose();
            _renderTarget = SKSurface.Create(
                surface.Context,
                budgeted: true,
                new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul))
                ?? throw new InvalidOperationException("Could not create the Skia render target.");
            _renderTargetWidth = width;
            _renderTargetHeight = height;
        }

        /// <inheritdoc />
        public void SetMatrixMode(int mode)
        {
            _projectionMode = mode == MODE_PROJECTION;
        }

        /// <inheritdoc />
        public void LoadIdentity()
        {
            _matrices.LoadIdentity(_projectionMode);
        }

        /// <inheritdoc />
        public void SetOrthographic(
            float left, float right, float bottom, float top, float near, float far)
        {
            _matrices.SetOrthographic(left, right, bottom, top, near, far);
        }

        /// <inheritdoc />
        public void PushMatrix()
        {
            _matrices.Push();
        }

        /// <inheritdoc />
        public void PopMatrix()
        {
            _matrices.Pop();
        }

        /// <inheritdoc />
        public void Scale(float x, float y, float z)
        {
            _matrices.Scale(x, y);
        }

        /// <inheritdoc />
        public void Rotate(float angle, float x, float y, float z)
        {
            _matrices.RotateDegrees(angle);
        }

        /// <inheritdoc />
        public void Skew(float skewXDegrees, float skewYDegrees)
        {
            _matrices.Skew(skewXDegrees, skewYDegrees);
        }

        /// <inheritdoc />
        public void Translate(float x, float y, float z)
        {
            _matrices.Translate(x, y);
        }

        /// <inheritdoc />
        public Matrix4x4 GetModelViewMatrix()
        {
            return _matrices.ModelView;
        }

        /// <inheritdoc />
        public void SetColor(Color c)
        {
            _drawColor = c;
        }

        /// <inheritdoc />
        public Color GetCurrentColor()
        {
            return _drawColor;
        }

        /// <inheritdoc />
        public void SetClearColor(Color c)
        {
            _clearColor = new SKColor(c.R, c.G, c.B, c.A);
        }

        /// <inheritdoc />
        public void Clear(int mask)
        {
            FlushQuads();
            Target.Clear(_clearColor);
        }

        /// <inheritdoc />
        public void SetBlendFunc(BlendingFactor sfactor, BlendingFactor dfactor)
        {
            SKBlendMode mode = (sfactor, dfactor) switch
            {
                (BlendingFactor.GLSRCALPHA, BlendingFactor.GLONE) => SKBlendMode.Plus,
                _ => SKBlendMode.SrcOver,
            };
            if (mode != _blendMode)
            {
                FlushQuads();
                _blendMode = mode;
            }
        }

        /// <inheritdoc />
        public void BindTexture(CTRTexture2D t)
        {
            SkiaTexture texture = t?.textureHandle_ as SkiaTexture;
            if (!ReferenceEquals(texture, _boundTexture))
            {
                FlushQuads();
                _boundTexture = texture;
            }
        }

        /// <inheritdoc />
        public void SetScissor(float x, float y, float width, float height)
        {
            FlushQuads();
            _ = Target.Save();
            Target.ClipRect(SKRect.Create(x, y, width, height));
        }

        /// <inheritdoc />
        public void DrawTriangleStrip(VertexPositionColor[] vertices, int vertexCount)
        {
            if (vertexCount < 3)
            {
                return;
            }
            BindTexture(null);
            EnsureBatchCompatible();
            for (int i = 0; i + 2 < vertexCount; i++)
            {
                AppendColorOnly(vertices[i]);
                AppendColorOnly(vertices[i + 1]);
                AppendColorOnly(vertices[i + 2]);
            }
        }

        /// <inheritdoc />
        public void DrawTriangleStrip(VertexPositionNormalTexture[] vertices, int vertexCount)
        {
            Color tint = QuadBaking.BakePremultipliedTint(_drawColor);
            VertexPositionColorTexture[] baked = new VertexPositionColorTexture[vertexCount];
            for (int i = 0; i < vertexCount; i++)
            {
                baked[i] = QuadBaking.Bake(vertices[i], _matrices.ModelView, tint);
            }
            DrawTriangleStrip(baked, vertexCount);
        }

        /// <inheritdoc />
        public void DrawTriangleStrip(VertexPositionColorTexture[] vertices, int vertexCount)
        {
            if (vertexCount < 3)
            {
                return;
            }
            EnsureBatchCompatible();
            for (int i = 0; i + 2 < vertexCount; i++)
            {
                Append(vertices[i]);
                Append(vertices[i + 1]);
                Append(vertices[i + 2]);
            }
        }

        /// <inheritdoc />
        public void DrawTriangleList(
            VertexPositionNormalTexture[] vertices, short[] indices, int indexCount)
        {
            Color tint = QuadBaking.BakePremultipliedTint(_drawColor);
            EnsureBatchCompatible();
            for (int i = 0; i < indexCount; i++)
            {
                Append(QuadBaking.Bake(vertices[indices[i]], _matrices.ModelView, tint));
            }
        }

        /// <inheritdoc />
        public void DrawTriangleList(
            VertexPositionColorTexture[] vertices, short[] indices, int indexCount)
        {
            EnsureBatchCompatible();
            for (int i = 0; i < indexCount; i++)
            {
                Append(vertices[indices[i]]);
            }
        }

        /// <inheritdoc />
        public void DrawLineStrip(VertexPositionColor[] vertices, int vertexCount)
        {
            FlushQuads();
            using SKPaint paint = new()
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1f,
                IsAntialias = true,
                BlendMode = _blendMode,
            };
            for (int i = 0; i + 1 < vertexCount; i++)
            {
                paint.Color = new SKColor(
                    vertices[i].Color.R, vertices[i].Color.G,
                    vertices[i].Color.B, vertices[i].Color.A);
                Target.DrawLine(
                    vertices[i].Position.X, vertices[i].Position.Y,
                    vertices[i + 1].Position.X, vertices[i + 1].Position.Y,
                    paint);
            }
        }

        /// <inheritdoc />
        public ITextureHandle DetachRenderTarget()
        {
            if (_renderTarget is null)
            {
                return null;
            }

            FlushQuads();
            SKImage snapshot = _renderTarget.Snapshot();
            _renderTarget.Dispose();
            _renderTarget = null;
            _renderTargetWidth = 0;
            _renderTargetHeight = 0;
            return new SkiaTexture(snapshot);
        }

        /// <inheritdoc />
        public void ResetRenderTarget()
        {
            FlushQuads();
            _renderTarget?.Dispose();
            _renderTarget = null;
            _renderTargetWidth = 0;
            _renderTargetHeight = 0;
        }

        /// <inheritdoc />
        public void CopyFromRenderTargetToScreen()
        {
            FlushQuads();
            if (_renderTarget is null)
            {
                return;
            }
            using SKImage snapshot = _renderTarget.Snapshot();
            surface.Canvas.DrawImage(
                snapshot, 0f, 0f, SKSamplingOptions.Default, paint: null);
        }

        /// <inheritdoc />
        public VertexPositionColor[] GetLastVertices_PositionColor()
        {
            return [];
        }

        /// <inheritdoc />
        public VertexPositionNormalTexture[] GetLastVertices_PositionNormalTexture()
        {
            return [];
        }

        /// <inheritdoc />
        public void FlushQuads()
        {
            if (_positions.Count == 0)
            {
                return;
            }

            using SKPaint paint = new()
            {
                BlendMode = _blendEnabled ? _batchBlendMode : SKBlendMode.Src,
            };
            if (_batchTexture is not null)
            {
                paint.Shader = SKShader.CreateImage(
                    _batchTexture.Image, SKShaderTileMode.Clamp, SKShaderTileMode.Clamp);
            }

            using SKVertices vertices = SKVertices.CreateCopy(
                SKVertexMode.Triangles,
                [.. _positions],
                _batchTexture is null ? null : [.. _texCoords],
                [.. _colors]);

            Target.DrawVertices(vertices, SKBlendMode.Modulate, paint);

            _positions.Clear();
            _texCoords.Clear();
            _colors.Clear();
        }

        /// <inheritdoc />
        public void BeginFrame()
        {
            _positions.Clear();
            _texCoords.Clear();
            _colors.Clear();
            surface.Canvas.ResetMatrix();
        }

        /// <inheritdoc />
        public void EndFrame()
        {
            FlushQuads();
            surface.Flush();
        }

        private void EnsureBatchCompatible()
        {
            if (!ReferenceEquals(_batchTexture, _boundTexture) || _batchBlendMode != _blendMode)
            {
                FlushQuads();
                _batchTexture = _boundTexture;
                _batchBlendMode = _blendMode;
            }
        }

        private void Append(in VertexPositionColorTexture vertex)
        {
            _positions.Add(new SKPoint(vertex.Position.X, vertex.Position.Y));
            _colors.Add(new SKColor(
                vertex.Color.R, vertex.Color.G, vertex.Color.B, vertex.Color.A));

            float width = _batchTexture?.Width ?? 1;
            float height = _batchTexture?.Height ?? 1;
            _texCoords.Add(new SKPoint(
                vertex.TextureCoordinate.X * width, vertex.TextureCoordinate.Y * height));
        }

        private void AppendColorOnly(in VertexPositionColor vertex)
        {
            _positions.Add(new SKPoint(vertex.Position.X, vertex.Position.Y));
            _colors.Add(new SKColor(
                vertex.Color.R, vertex.Color.G, vertex.Color.B, vertex.Color.A));
        }
    }
}
