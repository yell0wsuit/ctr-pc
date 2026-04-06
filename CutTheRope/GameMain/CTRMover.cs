using System;

using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Helpers;

using static CutTheRope.Helpers.ParsingHelpers;

namespace CutTheRope.GameMain
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
        /// <inheritdoc />
        public override void SetPathFromStringandStart(string p, Vector s)
        {
            if (p[0] == 'R')
            {
                bool flag = p[1] == 'C';
                int radius = (int)RTD(ParseIntOrZero(p[2..]));
                radius = (int)RTD(radius * ActivePhysicsConstants.MoverPathScale);
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
