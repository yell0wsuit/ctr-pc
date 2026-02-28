using System;
using System.Collections.Generic;
using System.Globalization;

using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Manages one complete ant path: its segments, the ant instances walking along it,
    /// and the per-frame update of ant positions, scales, colours and segment interaction state.
    /// Ported from decompiled iOS code.
    /// </summary>
    internal sealed class AntsPath : FrameworkTypes
    {
        /// <summary>
        /// Creates a path at <paramref name="position"/> from a comma-separated coordinate string.
        /// </summary>
        /// <param name="position">World-space origin of the path object.</param>
        /// <param name="pathString">
        /// Comma-separated flat list of X,Y offsets defining the polyline vertices relative to
        /// <paramref name="position"/>. Consecutive pairs form the endpoints of each segment.
        /// </param>
        /// <param name="speed">Ant march speed in world units per second (pre-scaled by the level scale).</param>
        /// <param name="offsetX">Horizontal world-space map/camera offset applied to all vertices.</param>
        /// <param name="offsetY">Vertical world-space map/camera offset applied to all vertices.</param>
        /// <param name="scale">Level scale factor; combined with the device multiplier for all size constants.</param>
        public AntsPath(Vector position, string pathString, float speed, float offsetX, float offsetY, float scale)
        {
            this.speed = speed;
            startPos = position;
            // this.offsetX = offsetX;
            // this.offsetY = offsetY;
            Scale = scale;

            path.Add(0f);
            path.Add(0f);

            if (!string.IsNullOrWhiteSpace(pathString))
            {
                string[] pieces = pathString.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (string piece in pieces)
                {
                    if (float.TryParse(piece, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
                    {
                        path.Add(value);
                    }
                }
            }

            CreateSegmentsAndAntsOffsetXOffsetYscale(offsetX, offsetY, scale);
        }

        /// <summary>Ordered list of segments that make up this path.</summary>
        public IReadOnlyList<AntsPathSegment> Segments => segmentsInternal;

        /// <summary>All ant instances currently alive on this path.</summary>
        public IReadOnlyList<Ant> Ants => antsInternal;

        /// <summary>
        /// Accumulated seconds for which at least one segment has been carrying the candy.
        /// Resets to zero when no segment is interacting.
        /// </summary>
        public float InteractionTime { get; private set; }

        /// <summary>Total world-space length of the path in units (sum of all segment lengths).</summary>
        public float PathLength { get; private set; }

        /// <summary>
        /// Path offset at which the first ant spawns. Zero for looped paths;
        /// half the start-hole sprite width for open paths (so ants emerge from the hole edge).
        /// </summary>
        public float StartAntOffset { get; private set; }

        /// <summary>True when the path's first and last endpoints coincide (within 0.01 units).</summary>
        public bool Looped { get; private set; }

        /// <summary>The raw level-data scale factor passed to the constructor.</summary>
        public float Scale { get; }

        /// <summary>
        /// Rebuilds all segments and ant instances from the stored path data.
        /// Called by the constructor; can be called again to reinitialise after parameter changes.
        /// </summary>
        public void CreateSegmentsAndAntsOffsetXOffsetYscale(float ox, float oy, float scale)
        {
            segmentsInternal.Clear();
            antsInternal.Clear();

            float deviceScale = GetDeviceScaledFactor(scale);

            for (int i = 1; i + 2 < path.Count; i += 2)
            {
                Vector start = AntConveyorLogic.ComputePathPoint(
                    startPos,
                    new Vector(path[i - 1], path[i]),
                    ox,
                    oy,
                    scale);

                Vector end = AntConveyorLogic.ComputePathPoint(
                    startPos,
                    new Vector(path[i + 1], path[i + 2]),
                    ox,
                    oy,
                    scale);

                AntsPathSegment segment = new(start, end, speed, deviceScale)
                {
                    container = this,
                    canInteract = true
                };

                segmentsInternal.Add(segment);
            }

            for (int i = 0; i < segmentsInternal.Count; i++)
            {
                AntsPathSegment current = segmentsInternal[i];
                if (i > 0)
                {
                    current.prevSegment = segmentsInternal[i - 1];
                }

                if (i < segmentsInternal.Count - 1)
                {
                    current.nextSegment = segmentsInternal[i + 1];
                }
            }

            if (segmentsInternal.Count == 0)
            {
                PathLength = 0f;
                numberOfAnts = 0;
                return;
            }

            AntsPathSegment first = segmentsInternal[0];
            AntsPathSegment last = segmentsInternal[^1];

            if (VectDistance(first.startPoint, last.endPoint) <= 0.01f)
            {
                first.prevSegment = last;
                last.nextSegment = first;
                Looped = true;
                StartAntOffset = 0f;
                startHole = null;
                endHole = null;
            }
            else
            {
                Looped = false;

                startHole = Image.Image_createWithResID(Resources.Img.AntHole);
                startHole.anchor = CENTER;
                startHole.x = first.startPoint.X;
                startHole.y = first.startPoint.Y;
                startHole.rotation = first.angleDeg;

                Vector firstDir = SegmentDirection(first);
                float halfWidth = startHole.width * 0.5f;
                startHole.x += firstDir.X * -halfWidth;
                startHole.y += firstDir.Y * -halfWidth;
                StartAntOffset = halfWidth;

                endHole = Image.Image_createWithResID(Resources.Img.AntHole);
                endHole.anchor = CENTER;
                endHole.x = last.endPoint.X;
                endHole.y = last.endPoint.Y;
                endHole.rotation = last.angleDeg;
            }

            PathLength = 0f;
            foreach (AntsPathSegment segment in segmentsInternal)
            {
                PathLength += segment.Length;
            }

            float gap = AntConveyorLogic.GetSpawnGap(deviceScale);
            numberOfAnts = gap > 0f ? (int)(PathLength / gap) : 0;

            for (int i = 0; i < numberOfAnts; i++)
            {
                float offset = (i * gap) - StartAntOffset;
                antsInternal.Add(CreateAntForOffset(offset));
            }
        }

        /// <summary>
        /// Returns the world-space position corresponding to <paramref name="offset"/> units along the path.
        /// Extrapolates linearly beyond either endpoint.
        /// </summary>
        public Vector PositionForOffset(float offset)
        {
            if (segmentsInternal.Count == 0)
            {
                return vectZero;
            }

            if (offset < 0f)
            {
                AntsPathSegment first = segmentsInternal[0];
                Vector dir = SegmentDirection(first);
                return VectAdd(first.startPoint, VectMult(dir, offset));
            }

            float accumulated = 0f;
            foreach (AntsPathSegment segment in segmentsInternal)
            {
                float length = segment.Length;
                if (offset < accumulated + length)
                {
                    float local = offset - accumulated;
                    Vector dir = SegmentDirection(segment);
                    return VectAdd(segment.startPoint, VectMult(dir, local));
                }

                accumulated += length;
            }

            AntsPathSegment last = segmentsInternal[^1];
            Vector lastDir = SegmentDirection(last);
            return VectAdd(last.endPoint, VectMult(lastDir, offset - PathLength));
        }

        /// <summary>
        /// Returns the visual heading in degrees [0, 360) at <paramref name="offset"/> units along the path.
        /// Blends smoothly between adjacent segment angles within <see cref="AntConveyorLogic.GetEdgeFadeDistance"/>
        /// of each junction, matching the iOS angle-lerp behaviour.
        /// </summary>
        public float AngleDegForOffset(float offset)
        {
            if (segmentsInternal.Count == 0)
            {
                return 0f;
            }

            AntsPathSegment segment = SegmentForOffset(offset) ?? segmentsInternal[^1];
            float angle = segment.angleDeg;
            Vector pos = PositionForOffset(offset);
            float blendDistance = AntConveyorLogic.GetEdgeFadeDistance(GetDeviceScaledFactor(Scale));

            if (segment.nextSegment != null)
            {
                float distToEnd = VectDistance(segment.endPoint, pos);
                if (distToEnd < blendDistance)
                {
                    float t = 1f - (distToEnd / blendDistance);
                    return AngleTo0_360(LerpAngleDeg(angle, segment.nextSegment.angleDeg, t * 0.5f));
                }
            }

            if (segment.prevSegment != null)
            {
                float distToStart = VectDistance(segment.startPoint, pos);
                if (distToStart < blendDistance)
                {
                    float t = 1f - (distToStart / blendDistance);
                    angle = LerpAngleDeg(angle, segment.prevSegment.angleDeg, t * 0.5f);
                }
            }

            return AngleTo0_360(angle);
        }

        /// <summary>
        /// Returns the per-ant scale factor at <paramref name="offset"/> units along the path.
        /// Always 1 on looped paths; fades linearly from 0.2 to 1.0 within
        /// <see cref="AntConveyorLogic.GetEdgeFadeDistance"/> of each endpoint on open paths.
        /// </summary>
        public float ScaleForOffset(float offset)
        {
            if (Looped)
            {
                return 1f;
            }

            float fadeDistance = AntConveyorLogic.GetEdgeFadeDistance(GetDeviceScaledFactor(Scale));
            float fromStart = StartAntOffset + offset;
            float fromEnd = MathF.Abs(PathLength - offset);
            float minDist = MathF.Min(fromStart, fromEnd);

            return minDist >= fadeDistance ? 1f : (minDist / fadeDistance * 0.8f) + 0.2f;
        }

        /// <summary>
        /// Returns the per-ant colour (RGBA) at <paramref name="offset"/> units along the path.
        /// Fully opaque in the middle of open paths and on looped paths;
        /// fades to transparent near the endpoints to match the iOS edge-fade effect.
        /// </summary>
        public RGBAColor ColorForOffset(float offset)
        {
            if (Looped)
            {
                return RGBAColor.solidOpaqueRGBA;
            }

            float fadeDistance = AntConveyorLogic.GetEdgeFadeDistance(GetDeviceScaledFactor(Scale));
            float fromStart = StartAntOffset + offset;
            float fromEnd = MathF.Abs(PathLength - offset);
            float minDist = MathF.Min(fromStart, fromEnd);

            if (minDist >= fadeDistance)
            {
                return RGBAColor.solidOpaqueRGBA;
            }

            float fade = minDist / fadeDistance;
            return new RGBAColor(fade, fade, fade, fade);
        }

        /// <summary>
        /// Advances ant offsets, updates animation and rendering state, spawns new ants at the
        /// tail end of open paths, and steps all segment interaction states.
        /// </summary>
        public void Update(float delta)
        {
            float minOffset = antsInternal.Count > 0 ? antsInternal[^1].offset : 0f;
            List<Ant> toRemove = [];

            foreach (Ant ant in antsInternal)
            {
                ant.animation.Update(delta);
                ant.offset += speed * delta;

                if (ant.offset < minOffset)
                {
                    minOffset = ant.offset;
                }

                if (ant.offset >= PathLength)
                {
                    if (!Looped)
                    {
                        toRemove.Add(ant);
                    }
                    else
                    {
                        while (ant.offset >= PathLength)
                        {
                            ant.offset -= PathLength;
                        }
                    }
                }

                float pathScaleFactor = ScaleForOffset(ant.offset);
                ant.animation.scaleX = -(pathScaleFactor * ant.baseScale);
                ant.animation.scaleY = pathScaleFactor * ant.baseScale;
                ant.animation.color = ColorForOffset(ant.offset);
            }

            if (toRemove.Count > 0)
            {
                foreach (Ant ant in toRemove)
                {
                    _ = antsInternal.Remove(ant);
                }
            }

            if (!Looped && antsInternal.Count > 0)
            {
                float gap = AntConveyorLogic.GetSpawnGap(GetDeviceScaledFactor(Scale));
                if (minOffset >= gap - StartAntOffset)
                {
                    antsInternal.Add(CreateAntForOffset(minOffset - gap));
                }
            }

            bool hasInteraction = false;
            foreach (AntsPathSegment segment in segmentsInternal)
            {
                segment.Update(delta);
                hasInteraction |= segment.interacting;
            }

            InteractionTime = hasInteraction ? InteractionTime + delta : 0f;

            foreach (Ant ant in antsInternal)
            {
                Vector pos = PositionForOffset(ant.offset);
                ant.animation.x = pos.X;
                ant.animation.y = pos.Y;
                ant.animation.rotation = AngleDegForOffset(ant.offset);
            }
        }

        /// <summary>Draws the path holes (if open) and all visible ant sprites.</summary>
        public void Draw()
        {
            startHole?.Draw();
            endHole?.Draw();

            foreach (Ant ant in antsInternal)
            {
                ant.animation.Draw();
            }
        }

        /// <summary>
        /// Creates a new ant at the given path offset, randomising its start frame and base scale.
        /// </summary>
        private Ant CreateAntForOffset(float offset)
        {
            Ant ant = new();
            Animation anim = Animation.Animation_createWithResID(Resources.Img.ObjAnt);
            int maxFrame = anim.texture?.quadsCount > 0
                ? Math.Min(5, anim.texture.quadsCount - 1)
                : 0;

            anim.AddAnimationWithIDDelayLoopFirstLast(0, 0.05f, Timeline.LoopType.TIMELINE_REPLAY, 0, maxFrame);
            anim.PlayTimeline(0);
            anim.anchor = CENTER;

            if (maxFrame > 0)
            {
                anim.JumpTo(RND_RANGE(0, maxFrame));
            }

            ant.animation = anim;
            ant.offset = offset;
            ant.baseScale = 1f + (RND_MINUS1_1 * 0.2f);

            Vector pos = PositionForOffset(offset);
            ant.animation.x = pos.X;
            ant.animation.y = pos.Y;

            float visualScale = ScaleForOffset(offset);
            ant.animation.scaleX = -(ant.baseScale * visualScale);
            ant.animation.scaleY = ant.baseScale * visualScale;
            ant.animation.color = ColorForOffset(offset);

            return ant;
        }

        /// <summary>
        /// Returns the segment containing <paramref name="offset"/>, or null if the offset is past the end.
        /// </summary>
        private AntsPathSegment SegmentForOffset(float offset)
        {
            float accumulated = 0f;
            foreach (AntsPathSegment segment in segmentsInternal)
            {
                float length = segment.Length;
                if (offset < accumulated + length)
                {
                    return segment;
                }

                accumulated += length;
            }

            return null;
        }

        /// <summary>
        /// Linearly interpolates between two angles in degrees, taking the shortest arc across the 360° wrap.
        /// </summary>
        private static float LerpAngleDeg(float from, float to, float t)
        {
            float delta = to - from;
            if (delta > 180f)
            {
                delta -= 360f;
            }
            else if (delta < -180f)
            {
                delta += 360f;
            }

            return from + (delta * t);
        }

        /// <summary>Returns the unit direction vector of <paramref name="segment"/>, or zero for zero-length segments.</summary>
        private static Vector SegmentDirection(AntsPathSegment segment)
        {
            return segment.Length <= 0f ? vectZero : VectDiv(VectSub(segment.endPoint, segment.startPoint), segment.Length);
        }

        /// <summary>Returns the effective scale factor for conveyor geometry (level scale; device multiplier is 1 on PC).</summary>
        private static float GetDeviceScaledFactor(float pathScale)
        {
            return pathScale;
        }

        private readonly Vector startPos;
        private readonly List<float> path = [];
        private readonly List<AntsPathSegment> segmentsInternal = [];
        private readonly List<Ant> antsInternal = [];
        private readonly float speed;
        // private readonly float offsetX;
        // private readonly float offsetY;
        private int numberOfAnts;
        private Image startHole;
        private Image endHole;
    }
}
