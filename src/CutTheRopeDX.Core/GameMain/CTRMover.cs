using System;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Helpers;

using static CutTheRopeDX.Helpers.ParsingHelpers;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Game-specific <see cref="Mover"/> subclass that parses path strings from level data,
    /// supporting both circular ("R…") and polyline ("x,y,…") path definitions.
    /// </summary>
    /// <param name="l">Loop mode for the mover animation.</param>
    /// <param name="m_">Movement speed magnitude.</param>
    /// <param name="r_">Rotation speed in degrees per second.</param>
    internal sealed class CTRMover(int l, float m_, float r_) : Mover(l, m_, r_)
    {
        /// <summary>
        /// Returns the number of path points <see cref="SetPathFromStringandStart"/> will emit for
        /// <paramref name="p"/>. Callers size the mover from this: <see cref="Mover.AddPathPoint"/>
        /// indexes its array unguarded, so a capacity derived from the unscaled radius overruns.
        /// </summary>
        /// <param name="p">The path string from level data.</param>
        /// <returns>The path point capacity required for <paramref name="p"/>.</returns>
        public static int PathPointCapacity(string p)
        {
            return string.IsNullOrEmpty(p) || p[0] != 'R'
                ? 100
                : MAX(1, CirclePathRadius(p) / 2) + 1;
        }

        /// <summary>Scales the radius of a circular ("R…") path into world units.</summary>
        /// <param name="p">The path string from level data.</param>
        /// <returns>The scaled circle radius.</returns>
        private static int CirclePathRadius(string p)
        {
            int radius = (int)RTD(ParseIntOrZero(p[2..]));
            return (int)RTD(radius * ActivePhysicsConstants.MoverPathScale);
        }

        /// <inheritdoc />
        public override void SetPathFromStringandStart(string p, Vector s)
        {
            if (p[0] == 'R')
            {
                bool flag = p[1] == 'C';
                int radius = CirclePathRadius(p);
                int pointCount = radius / 2;
                if (pointCount <= 0)
                {
                    AddPathPoint(s);
                    return;
                }
                float angleStep = MathF.Tau / pointCount;
                if (!flag)
                {
                    angleStep = 0f - angleStep;
                }
                float theta = 0f;
                for (int i = 0; i < pointCount; i++)
                {
                    float x = s.X + (radius * Cosf(theta));
                    float y = s.Y + (radius * Sinf(theta));
                    AddPathPoint(Vect(x, y));
                    theta += angleStep;
                }
                return;
            }
            AddPathPoint(s);
            if (p[^1] == ',')
            {
                p = p[..(p.Length - 1)];
            }
            string[] list = p.Split(',');
            for (int j = 0; j < list.Length; j += 2)
            {
                string xOffsetString = list[j];
                string yOffsetString = list[j + 1];
                AddPathPoint(Vect(
                    s.X + (ParseFloatOrZero(xOffsetString) * ActivePhysicsConstants.MoverPathScale),
                    s.Y + (ParseFloatOrZero(yOffsetString) * ActivePhysicsConstants.MoverPathScale)));
            }
        }
    }
}
