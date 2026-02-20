using System;
using System.Collections.Generic;

using CutTheRope.Framework.Core;
using CutTheRope.Framework.Helpers;
using CutTheRope.Helpers;

namespace CutTheRope.GameMain
{
    internal sealed class CTRMover(int l, float m_, float r_) : Mover(l, m_, r_)
    {
        public override void SetPathFromStringandStart(string p, Vector s)
        {
            if (p.CharacterAtIndex(0) == 'R')
            {
                bool flag = p.CharacterAtIndex(1) == 'C';
                int radius = (int)RTD(p.SubstringFromIndex(2).IntValue());
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
            if (p.CharacterAtIndex(p.Length() - 1) == ',')
            {
                p = p.SubstringToIndex(p.Length() - 1);
            }
            List<string> list = p.ComponentsSeparatedByString(',');
            for (int j = 0; j < list.Count; j += 2)
            {
                string xOffsetString = list[j];
                string yOffsetString = list[j + 1];
                AddPathPoint(Vect(s.X + (xOffsetString.FloatValue() * 3f), s.Y + (yOffsetString.FloatValue() * 3f)));
            }
        }
    }
}
