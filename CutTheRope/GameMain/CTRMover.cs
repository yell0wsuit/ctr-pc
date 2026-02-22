using System;
using System.Globalization;

using CutTheRope.Framework.Core;
using CutTheRope.Framework.Helpers;

namespace CutTheRope.GameMain
{
    internal sealed class CTRMover(int l, float m_, float r_) : Mover(l, m_, r_)
    {
        public override void SetPathFromStringandStart(string p, Vector s)
        {
            if (p[0] == 'R')
            {
                bool flag = p[1] == 'C';
                int radius = (int)RTD(ParseIntOrZero(p[2..]));
                radius *= 3;
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
                AddPathPoint(Vect(s.X + ((string.IsNullOrEmpty(xOffsetString) ? 0f : float.Parse(xOffsetString, CultureInfo.InvariantCulture)) * 3f), s.Y + ((string.IsNullOrEmpty(yOffsetString) ? 0f : float.Parse(yOffsetString, CultureInfo.InvariantCulture)) * 3f)));
            }
        }
    }
}
