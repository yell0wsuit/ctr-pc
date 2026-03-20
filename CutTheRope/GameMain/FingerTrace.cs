using System.Collections.Generic;

using CutTheRope.Desktop;
using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;

using Microsoft.Xna.Framework;

namespace CutTheRope.GameMain
{
    internal enum FingerTraceSpriteKind
    {
        Body,
        Head,
        Glow,
        Spark,
    }

    internal enum FingerTraceBlendMode
    {
        Alpha,
        Additive,
    }

    internal readonly record struct FingerTraceSpritePose(
        FingerTraceSpriteKind Kind,
        string TextureResourceName,
        int QuadIndex,
        Vector Position,
        float Rotation,
        float Scale,
        float Alpha,
        FingerTraceBlendMode BlendMode);

    internal sealed class FingerTraceSnapshot(IReadOnlyList<Vector> sampledPoints, IReadOnlyList<FingerTraceSpritePose> sprites)
    {
        public IReadOnlyList<Vector> SampledPoints { get; } = sampledPoints;

        public IReadOnlyList<FingerTraceSpritePose> Sprites { get; } = sprites;
    }

    internal struct TraceSegment(Vector start, Vector end, float life)
    {
        public Vector Start = start;
        public Vector End = end;
        public float Life = life;
    }

    internal abstract class FingerTrace : FrameworkTypes
    {
        private readonly Dictionary<string, Image> imageCache = [];
        private readonly List<TraceSegment> segments = [];

        private bool isActive;
        private bool hasLastPoint;
        private Vector lastPoint;
        private FingerTraceSnapshot snapshot = new([], []);

        public bool IsAlive => isActive || segments.Count > 0 || HasLiveParticles;

        public void Begin(Vector position)
        {
            Reset();
            isActive = true;
            hasLastPoint = true;
            lastPoint = position;
            RefreshSnapshot();
        }

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

        public void End()
        {
            isActive = false;
            hasLastPoint = false;
            RefreshSnapshot();
        }

        public void Reset()
        {
            isActive = false;
            hasLastPoint = false;
            segments.Clear();
            ResetCore();
            snapshot = new([], []);
        }

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

        public FingerTraceSnapshot GetSnapshot()
        {
            return snapshot;
        }

        public virtual void AddSegment(float startX, float startY, float endX, float endY)
        {
            StoreSegment(new Vector(startX, startY), new Vector(endX, endY), 0.1f);
        }

        public void ClearSegments()
        {
            segments.Clear();
            RefreshSnapshot();
        }

        public void SetMaxSize(float size)
        {
            MaxSize = size;
        }

        protected IReadOnlyList<TraceSegment> Segments => segments;

        protected float MaxSize { get; private set; } = 8f;

        protected void StoreSegment(Vector start, Vector end, float life)
        {
            segments.Add(new TraceSegment(start, end, life));
        }

        protected virtual bool HasLiveParticles => false;

        protected virtual void UpdateCore(float delta)
        {
        }

        protected virtual void ResetCore()
        {
        }

        protected abstract void BuildSnapshot(List<Vector> sampledPoints, List<FingerTraceSpritePose> sprites);

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

        private void RefreshSnapshot()
        {
            List<Vector> sampledPoints = [];
            List<FingerTraceSpritePose> sprites = [];
            BuildSnapshot(sampledPoints, sprites);
            snapshot = new([.. sampledPoints], [.. sprites]);
        }

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
            image.color = RGBAColor.MakeRGBA(1f, 1f, 1f, sprite.Alpha);
            image.Draw();
        }
    }
}
