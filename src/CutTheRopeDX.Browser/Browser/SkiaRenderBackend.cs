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
    /// them, so the matrix stack and skew behave exactly as they do on desktop and Skia only
    /// ever receives finished triangles. Quads accumulate into a batch that flushes when the
    /// texture or blend mode changes, mirroring the desktop backend's one-draw-per-batch
    /// behavior.
    /// <para>
    /// Skia takes straight-alpha <see cref="SKColor"/> values and premultiplies them internally.
    /// Ordinary sprite tints and the source-alpha blend paths are already straight. Explicit
    /// vertex colors submitted with One/InverseSourceAlpha are premultiplied for the desktop
    /// fixed-function pipeline, so this backend converts those colors back to straight exactly
    /// once before giving them to Skia.
    /// </para>
    /// <para>
    /// A <c>GL_SRC_ALPHA</c> source factor needs more than a blend mode: it multiplies the
    /// premultiplied fragment by its own alpha again, which no Skia blend mode reproduces. Those
    /// batches therefore fold the factor into the source itself, weighting the texture by its
    /// alpha through <see cref="AlphaWeightMatrix"/> and the tint through
    /// <see cref="VertexColorEncoding.ForRendererTint"/>.
    /// </para>
    /// </remarks>
    /// <param name="surface">The Skia surface wrapping the WebGL2 framebuffer.</param>
    internal sealed class SkiaRenderBackend(SkiaSurface surface) : IRenderBackend
    {
        private const int GL_BLEND = 1;
        private const int GL_SCISSOR_TEST = 4;
        private const int MODE_PROJECTION = 15;

        /// <summary>
        /// Rewrites a color to opaque grey carrying its own alpha, so multiplying a texture by it
        /// weights the texture's color by its alpha and leaves its alpha untouched. Skia applies
        /// color matrices to straight colors, so the row that fixes alpha at one is what keeps the
        /// weighting off the alpha channel, and the destination factor still sees the source alpha
        /// the fixed-function pipeline would have produced.
        /// </summary>
        private static readonly float[] AlphaWeightMatrix =
        [
            0f, 0f, 0f, 1f, 0f,
            0f, 0f, 0f, 1f, 0f,
            0f, 0f, 0f, 1f, 0f,
            0f, 0f, 0f, 0f, 1f,
        ];

        /// <summary>
        /// Filtering for every image this backend samples, both the sprite batches and the
        /// presented render target. It matches the <c>SamplerState.LinearClamp</c> the desktop
        /// backend draws quads and presents with, down to carrying no mipmaps: the textures are
        /// uploaded without them, and the game only ever scales by the modest factor between its
        /// internal resolution and the window. Skia defaults to nearest when a draw does not say
        /// otherwise, which would leave every scaled or rotated sprite aliased.
        /// </summary>
        private static readonly SKSamplingOptions LinearSampling =
            new(SKFilterMode.Linear, SKMipmapMode.None);

        private readonly MatrixStack _matrices = new();
        private readonly List<SKPoint> _positions = [];
        private readonly List<SKPoint> _texCoords = [];
        private readonly List<SKColor> _colors = [];

        private SKSurface _renderTarget;
        private int _renderTargetWidth;
        private int _renderTargetHeight;
        private SkiaTexture _boundTexture;
        private SkiaTexture _batchTexture;
        private SKBlendMode _batchBlendMode = SKBlendMode.SrcOver;
        private bool _batchWeightsSourceByAlpha;
        private Color _drawColor = new(255, 255, 255, 255);
        private SKColor _clearColor = SKColors.Black;
        private bool _blendEnabled = true;
        private bool _projectionMode;
        private bool _scissorSaved;
        private BlendingFactor _requestedSourceFactor = BlendingFactor.GLONE;
        private BlendingFactor _requestedDestinationFactor = BlendingFactor.GLONEMINUSSRCALPHA;

        /// <summary>The canvas currently targeted by draw calls.</summary>
        internal SKCanvas Target => _renderTarget?.Canvas ?? surface.Canvas;

        /// <summary>The blend mode most recently requested by <see cref="SetBlendFunc"/>.</summary>
        private SKBlendMode RequestedBlendMode { get; set; } = SKBlendMode.SrcOver;

        /// <summary>
        /// The blend mode a draw issued right now would use, with blending disabled folded in.
        /// Disabling blending is <c>Src</c>, which ignores the destination the way the desktop
        /// backend's opaque blend state does. Batches capture this when geometry is appended,
        /// never when they flush: Core brackets each view as enable, draw, disable, so reading it
        /// at flush time would blend every batch as though blending were off.
        /// </summary>
        private SKBlendMode EffectiveBlendMode =>
            _blendEnabled ? RequestedBlendMode : SKBlendMode.Src;

        /// <summary>
        /// The source blend factor a draw issued right now would use. Disabling blending leaves
        /// the fragment untouched, which is the <c>GL_ONE</c> source factor.
        /// </summary>
        private BlendingFactor EffectiveSourceFactor =>
            _blendEnabled ? _requestedSourceFactor : BlendingFactor.GLONE;

        /// <summary>Whether a draw issued right now consumes a source weighted by its own alpha.</summary>
        private bool WeightsSourceByAlpha =>
            VertexColorEncoding.ScalesSourceByAlpha(EffectiveSourceFactor);

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
                EndScissor();
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
            if (width == _renderTargetWidth && height == _renderTargetHeight)
            {
                return;
            }

            DropScissor();
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
            _requestedSourceFactor = sfactor;
            _requestedDestinationFactor = dfactor;
            RequestedBlendMode = (sfactor, dfactor) switch
            {
                (BlendingFactor.GLSRCALPHA, BlendingFactor.GLONE) => SKBlendMode.Plus,
                _ => SKBlendMode.SrcOver,
            };
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
            // GL's scissor is one absolute rectangle that each call replaces, and Core relies on
            // that: the pack selector narrows the scissor to reveal Om Nom, then sets it straight
            // back to the scrolling container's rectangle. Skia's ClipRect only ever intersects,
            // so the previous clip is popped first or the two rectangles would compound into a
            // sliver, and each successive draw would clip smaller than the last.
            EndScissor();
            _ = Target.Save();
            Target.ClipRect(SKRect.Create(x, y, width, height));
            _scissorSaved = true;
        }

        /// <summary>Pops the clip <see cref="SetScissor"/> pushed, if one is outstanding.</summary>
        private void EndScissor()
        {
            if (_scissorSaved)
            {
                Target.Restore();
                _scissorSaved = false;
            }
        }

        /// <summary>
        /// Forgets an outstanding scissor without popping it, for when the canvas holding it is
        /// about to be replaced and restoring would unbalance whichever canvas comes next.
        /// </summary>
        private void DropScissor()
        {
            _scissorSaved = false;
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
            Color tint = _drawColor;
            VertexPositionColorTexture[] baked = new VertexPositionColorTexture[vertexCount];
            for (int i = 0; i < vertexCount; i++)
            {
                baked[i] = QuadBaking.Bake(vertices[i], _matrices.ModelView, tint);
            }
            DrawBakedTriangleStrip(baked, vertexCount);
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
                AppendTransformed(vertices[i]);
                AppendTransformed(vertices[i + 1]);
                AppendTransformed(vertices[i + 2]);
            }
        }

        /// <summary>
        /// Appends a strip whose positions <see cref="QuadBaking"/> has already transformed.
        /// </summary>
        /// <param name="vertices">Vertices already in view space.</param>
        /// <param name="vertexCount">Number of vertices to submit.</param>
        private void DrawBakedTriangleStrip(VertexPositionColorTexture[] vertices, int vertexCount)
        {
            if (vertexCount < 3)
            {
                return;
            }
            EnsureBatchCompatible();
            for (int i = 0; i + 2 < vertexCount; i++)
            {
                AppendRendererTint(vertices[i]);
                AppendRendererTint(vertices[i + 1]);
                AppendRendererTint(vertices[i + 2]);
            }
        }

        /// <inheritdoc />
        public void DrawTriangleList(
            VertexPositionNormalTexture[] vertices, short[] indices, int indexCount)
        {
            Color tint = _drawColor;
            EnsureBatchCompatible();
            for (int i = 0; i < indexCount; i++)
            {
                AppendRendererTint(
                    QuadBaking.Bake(vertices[indices[i]], _matrices.ModelView, tint));
            }
        }

        /// <inheritdoc />
        public void DrawTriangleList(
            VertexPositionColorTexture[] vertices, short[] indices, int indexCount)
        {
            EnsureBatchCompatible();
            for (int i = 0; i < indexCount; i++)
            {
                AppendTransformed(vertices[indices[i]]);
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
                BlendMode = EffectiveBlendMode,
            };
            for (int i = 0; i + 1 < vertexCount; i++)
            {
                paint.Color = ToSkiaExplicitColor(vertices[i].Color);
                SKPoint from = ToViewSpace(vertices[i].Position);
                SKPoint to = ToViewSpace(vertices[i + 1].Position);
                Target.DrawLine(from.X, from.Y, to.X, to.Y, paint);
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
            DropScissor();
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
            DropScissor();
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
            ScreenPresentation presentation = ScreenPresentation.Instance;
            SKRect destination = SKRect.Create(
                presentation.ScaledViewX,
                presentation.ScaledViewY,
                presentation.ScaledViewWidth,
                presentation.ScaledViewHeight);
            surface.Canvas.Clear(SKColors.Black);
            surface.Canvas.DrawImage(snapshot, destination, LinearSampling, paint: null);
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
                Color = SKColors.White,
                BlendMode = _batchBlendMode,
            };
            using SKShader image = _batchTexture is null
                ? null
                : SKShader.CreateImage(
                    _batchTexture.Image,
                    SKShaderTileMode.Clamp,
                    SKShaderTileMode.Clamp,
                    LinearSampling);
            using SKColorFilter alphaWeight = image is null || !_batchWeightsSourceByAlpha
                ? null
                : SKColorFilter.CreateColorMatrix(AlphaWeightMatrix);
            using SKShader alphaOnly = alphaWeight is null
                ? null
                : image.WithColorFilter(alphaWeight);
            using SKShader weighted = alphaOnly is null
                ? null
                : SKShader.CreateBlend(SKBlendMode.Modulate, image, alphaOnly);
            paint.Shader = weighted ?? image;

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
            // Core leaves the pack selector's scissor set when its draw ends, so the clip is
            // released here rather than leaking a canvas save into the next frame.
            EndScissor();
            surface.Flush();
        }

        private void EnsureBatchCompatible()
        {
            // Source weighting is tracked alongside the blend mode rather than derived from it:
            // SourceAlpha/InverseSourceAlpha and One/InverseSourceAlpha both draw as SrcOver and
            // differ only in the weighting, so a batch that ignored it would render one pair with
            // the other's shader.
            if (!ReferenceEquals(_batchTexture, _boundTexture)
                || _batchBlendMode != EffectiveBlendMode
                || _batchWeightsSourceByAlpha != WeightsSourceByAlpha)
            {
                FlushQuads();
                _batchTexture = _boundTexture;
                _batchBlendMode = EffectiveBlendMode;
                _batchWeightsSourceByAlpha = WeightsSourceByAlpha;
            }
        }

        private void AppendRendererTint(in VertexPositionColorTexture vertex)
        {
            Append(
                vertex,
                VertexColorEncoding.ForRendererTint(vertex.Color, EffectiveSourceFactor));
        }

        private void AppendExplicitVertex(in VertexPositionColorTexture vertex)
        {
            Append(vertex, DecodeExplicitColor(vertex.Color));
        }

        private void Append(in VertexPositionColorTexture vertex, Color straightColor)
        {
            _positions.Add(new SKPoint(vertex.Position.X, vertex.Position.Y));
            _colors.Add(ToSkiaColor(straightColor));

            float width = _batchTexture?.Width ?? 1;
            float height = _batchTexture?.Height ?? 1;
            _texCoords.Add(new SKPoint(
                vertex.TextureCoordinate.X * width, vertex.TextureCoordinate.Y * height));
        }

        /// <summary>
        /// Appends a vertex Core handed over untransformed, applying the matrix stack on the way
        /// in. Only the sprite paths arrive pre-baked; every other draw reaches the backend in
        /// model space and would otherwise ignore the camera and any enclosing transform.
        /// </summary>
        /// <param name="vertex">The untransformed vertex.</param>
        private void AppendTransformed(in VertexPositionColorTexture vertex)
        {
            AppendExplicitVertex(new VertexPositionColorTexture(
                Vector3.Transform(vertex.Position, _matrices.ModelView),
                vertex.Color,
                vertex.TextureCoordinate));
        }

        private void AppendColorOnly(in VertexPositionColor vertex)
        {
            _positions.Add(ToViewSpace(vertex.Position));
            _colors.Add(ToSkiaExplicitColor(vertex.Color));
        }

        private SKColor ToSkiaExplicitColor(Color color)
        {
            return ToSkiaColor(DecodeExplicitColor(color));
        }

        private Color DecodeExplicitColor(Color color)
        {
            return VertexColorEncoding.ForExplicitVertex(
                color,
                _requestedSourceFactor,
                _requestedDestinationFactor);
        }

        private static SKColor ToSkiaColor(Color straight)
        {
            return new SKColor(straight.R, straight.G, straight.B, straight.A);
        }

        /// <summary>Transforms a model-space position by the current matrix stack.</summary>
        /// <param name="position">The untransformed position.</param>
        private SKPoint ToViewSpace(in Vector3 position)
        {
            Vector3 transformed = Vector3.Transform(position, _matrices.ModelView);
            return new SKPoint(transformed.X, transformed.Y);
        }
    }
}
