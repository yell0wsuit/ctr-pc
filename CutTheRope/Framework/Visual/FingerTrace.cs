using System.Collections.Generic;

using CutTheRope.Desktop;
using CutTheRope.Framework.Core;

using Microsoft.Xna.Framework;

namespace CutTheRope.Framework.Visual
{
    /// <summary>
    /// Identifies the logical sprite role produced by a finger trace snapshot.
    /// </summary>
    internal enum FingerTraceSpriteKind
    {
        Body,
        Head,
        Glow,
        Spark,
    }

    /// <summary>
    /// Describes the blend mode to use when drawing a trace sprite pose.
    /// </summary>
    internal enum FingerTraceBlendMode
    {
        Alpha,
        Additive,
    }

    /// <summary>
    /// Captures one sprite draw request produced by a trace for rendering or inspection.
    /// </summary>
    /// <param name="Kind">Logical sprite role for rendering behavior.</param>
    /// <param name="TextureResourceName">Texture resource containing the sprite quad.</param>
    /// <param name="QuadIndex">Quad index in the texture atlas.</param>
    /// <param name="Position">World-space sprite position.</param>
    /// <param name="Rotation">Sprite rotation.</param>
    /// <param name="Scale">Uniform sprite scale factor.</param>
    /// <param name="Alpha">Sprite opacity multiplier.</param>
    /// <param name="BlendMode">Blend mode used to draw the sprite.</param>
    /// <param name="TranslateY">Local Y translation applied before drawing.</param>
    internal readonly record struct FingerTraceSpritePose(
        FingerTraceSpriteKind Kind,
        string TextureResourceName,
        int QuadIndex,
        Vector Position,
        float Rotation,
        float Scale,
        float Alpha,
        FingerTraceBlendMode BlendMode,
        float TranslateY = 0f);

    /// <summary>
    /// Immutable view of a trace after it has built its current sampled path and sprite list.
    /// </summary>
    internal sealed class FingerTraceSnapshot(IReadOnlyList<Vector> sampledPoints, IReadOnlyList<FingerTraceSpritePose> sprites)
    {
        /// <summary>
        /// Gets the sampled path points used for previewing or testing the current trace state.
        /// </summary>
        public IReadOnlyList<Vector> SampledPoints { get; } = sampledPoints;

        /// <summary>
        /// Gets the sprites emitted by the trace for the current frame.
        /// </summary>
        public IReadOnlyList<FingerTraceSpritePose> Sprites { get; } = sprites;
    }

    /// <summary>
    /// Represents a single live trace segment with a <paramref name="start"/> point, <paramref name="end"/> point, and remaining lifetime.
    /// </summary>
    internal struct TraceSegment(Vector start, Vector end, float life)
    {
        /// <summary>
        /// Segment start point in world space.
        /// </summary>
        public Vector Start = start;

        /// <summary>
        /// Segment end point in world space.
        /// </summary>
        public Vector End = end;

        /// <summary>
        /// Remaining segment lifetime in seconds.
        /// </summary>
        public float Life = life;
    }

    /// <summary>
    /// Base class for CTR2-style finger traces that manage timed segments and optional particle output.
    /// </summary>
    internal abstract class FingerTrace : FrameworkTypes
    {
        /// <summary>
        /// Cached trace sprite images keyed by texture resource name.
        /// </summary>
        private readonly Dictionary<string, Image> imageCache = [];

        /// <summary>
        /// Live trace segments currently fading out.
        /// </summary>
        private readonly List<TraceSegment> segments = [];

        /// <summary>
        /// Whether the trace is currently accepting appended points.
        /// </summary>
        private bool isActive;

        /// <summary>
        /// Whether <see cref="lastPoint" /> contains a valid previous touch position.
        /// </summary>
        private bool hasLastPoint;

        /// <summary>
        /// Last touch position used to create the next segment.
        /// </summary>
        private Vector lastPoint;

        /// <summary>
        /// Most recent immutable snapshot of sampled trace points and sprites.
        /// </summary>
        private FingerTraceSnapshot snapshot = new([], []);

        /// <summary>
        /// Gets a value indicating whether the trace still has visible geometry or live particles.
        /// </summary>
        public bool IsAlive => isActive || segments.Count > 0 || HasLiveParticles;

        /// <summary>
        /// Starts a new trace at the specified world <paramref name="position"/>, clearing any previous segments.
        /// </summary>
        /// <param name="position">The initial touch position in world space.</param>
        public void Begin(Vector position)
        {
            Reset();
            isActive = true;
            hasLastPoint = true;
            lastPoint = position;
            RefreshSnapshot();
        }

        /// <summary>
        /// Appends a new touch <paramref name="position"/> to the trace, generating a segment from the last point.
        /// </summary>
        /// <param name="position">The next touch position in world space.</param>
        public void Append(Vector position)
        {
            if (!isActive)
            {
                Begin(position);
                return;
            }

            if (!hasLastPoint)
            {
                hasLastPoint = true;
                lastPoint = position;
                RefreshSnapshot();
                return;
            }

            AddSegment(lastPoint.X, lastPoint.Y, position.X, position.Y);
            lastPoint = position;
            RefreshSnapshot();
        }

        /// <summary>
        /// Marks the trace as finished so only the remaining timed segments continue to fade out.
        /// </summary>
        public void End()
        {
            isActive = false;
            hasLastPoint = false;
            RefreshSnapshot();
        }

        /// <summary>
        /// Clears the trace state, all live segments, and any subclass-specific transient state.
        /// </summary>
        public void Reset()
        {
            isActive = false;
            hasLastPoint = false;
            segments.Clear();
            ResetCore();
            snapshot = new([], []);
        }

        /// <summary>
        /// Advances segment lifetimes and subclass-specific state by one frame.
        /// </summary>
        /// <param name="delta">Elapsed time in seconds.</param>
        public void Update(float delta)
        {
            for (int i = 0; i < segments.Count; i++)
            {
                TraceSegment segment = segments[i];
                segment.Life -= delta;
                segments[i] = segment;
            }

            int expiredCount = 0;
            while (expiredCount < segments.Count && segments[expiredCount].Life <= 0f)
            {
                expiredCount++;
            }

            if (expiredCount > 0)
            {
                segments.RemoveRange(0, expiredCount);
            }

            UpdateCore(delta);
            RefreshSnapshot();
        }

        /// <summary>
        /// Draws the current sprite snapshot using the requested per-sprite blend modes.
        /// </summary>
        public virtual void Draw()
        {
            if (snapshot.Sprites.Count == 0)
            {
                return;
            }

            FingerTraceBlendMode? currentBlendMode = null;
            foreach (FingerTraceSpritePose sprite in snapshot.Sprites)
            {
                if (sprite.Alpha <= 0f)
                {
                    continue;
                }

                if (currentBlendMode != sprite.BlendMode)
                {
                    currentBlendMode = sprite.BlendMode;
                    Renderer.SetBlendFunc(
                        sprite.BlendMode == FingerTraceBlendMode.Additive
                            ? BlendingFactor.GLSRCALPHA
                            : BlendingFactor.GLONE,
                        sprite.BlendMode == FingerTraceBlendMode.Additive
                            ? BlendingFactor.GLONE
                            : BlendingFactor.GLONEMINUSSRCALPHA);
                }

                DrawSpritePose(sprite);
            }

            Renderer.SetColor(Color.White);
            Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
        }

        /// <summary>
        /// Returns the most recently built immutable snapshot for testing or preview use.
        /// </summary>
        /// <returns>The latest immutable trace snapshot.</returns>
        public FingerTraceSnapshot GetSnapshot()
        {
            return snapshot;
        }

        /// <summary>
        /// Adds a new trace segment with the default CTR2 lifetime used by the base class.
        /// </summary>
        /// <param name="startX">Segment start X.</param>
        /// <param name="startY">Segment start Y.</param>
        /// <param name="endX">Segment end X.</param>
        /// <param name="endY">Segment end Y.</param>
        public virtual void AddSegment(float startX, float startY, float endX, float endY)
        {
            StoreSegment(new Vector(startX, startY), new Vector(endX, endY), 0.1f);
        }

        /// <summary>
        /// Removes all currently stored segments and rebuilds the exposed snapshot.
        /// </summary>
        public void ClearSegments()
        {
            segments.Clear();
            RefreshSnapshot();
        }

        /// <summary>
        /// Sets the maximum <paramref name="size"/> hint used by subclasses that scale their trace visuals.
        /// </summary>
        /// <param name="size">Maximum trace size in world units.</param>
        public void SetMaxSize(float size)
        {
            MaxSize = size;
        }

        /// <summary>
        /// Gets the live trace segments in their current age-sorted order.
        /// </summary>
        protected IReadOnlyList<TraceSegment> Segments => segments;

        /// <summary>
        /// Gets the subclass-configurable maximum size hint for the trace.
        /// </summary>
        protected float MaxSize { get; private set; } = 8f;

        /// <summary>
        /// Stores a segment with an explicit lifetime chosen by the subclass.
        /// </summary>
        /// <param name="start">Segment start point.</param>
        /// <param name="end">Segment end point.</param>
        /// <param name="life">Remaining life in seconds.</param>
        protected void StoreSegment(Vector start, Vector end, float life)
        {
            segments.Add(new TraceSegment(start, end, life));
        }

        /// <summary>
        /// Gets a value indicating whether the subclass still has live particle output after segments fade.
        /// </summary>
        protected virtual bool HasLiveParticles => false;

        /// <summary>
        /// Updates subclass-specific state after the base segment lifetimes have been advanced.
        /// </summary>
        /// <param name="delta">Elapsed time in seconds.</param>
        protected virtual void UpdateCore(float delta)
        {
        }

        /// <summary>
        /// Resets subclass-specific state when the trace is cleared or restarted.
        /// </summary>
        protected virtual void ResetCore()
        {
        }

        /// <summary>
        /// Rebuilds the sampled path and sprite snapshot for the current trace state.
        /// </summary>
        /// <param name="sampledPoints">Receives sampled path points.</param>
        /// <param name="sprites">Receives sprite poses to draw for the current frame.</param>
        protected abstract void BuildSnapshot(List<Vector> sampledPoints, List<FingerTraceSpritePose> sprites);

        /// <summary>
        /// Gets or creates the cached image used to draw sprites from the specified resource.
        /// </summary>
        /// <param name="resourceName">Texture resource name.</param>
        /// <returns>The cached or newly created image for <paramref name="resourceName"/>.</returns>
        protected Image GetImage(string resourceName)
        {
            if (!imageCache.TryGetValue(resourceName, out Image image))
            {
                image = Image.Image_createWithResID(resourceName);
                image.DoRestoreCutTransparency();
                image.anchor = CENTER;
                imageCache[resourceName] = image;
            }

            return image;
        }

        /// <summary>
        /// Rebuilds the immutable trace snapshot from the current live trace state.
        /// </summary>
        private void RefreshSnapshot()
        {
            List<Vector> sampledPoints = [];
            List<FingerTraceSpritePose> sprites = [];
            BuildSnapshot(sampledPoints, sprites);
            snapshot = new([.. sampledPoints], [.. sprites]);
        }

        /// <summary>
        /// Draws a single cached <paramref name="sprite"/> pose using the standard image-based trace path.
        /// </summary>
        /// <param name="sprite">The sprite pose to draw.</param>
        protected void DrawSpritePose(FingerTraceSpritePose sprite)
        {
            Image image = GetImage(sprite.TextureResourceName);
            image.SetDrawQuad(sprite.QuadIndex);
            image.anchor = CENTER;
            image.x = sprite.Position.X;
            image.y = sprite.Position.Y;
            image.rotation = sprite.Rotation;
            image.scaleX = sprite.Scale;
            image.scaleY = sprite.Scale;
            image.translateY = sprite.TranslateY;
            image.color = RGBAColor.MakeRGBA(1f, 1f, 1f, sprite.Alpha);
            image.Draw();
        }
    }
}
