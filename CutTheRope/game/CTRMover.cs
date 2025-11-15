using System.Collections.Generic;

using CutTheRope.Helpers;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;

namespace CutTheRope.game
{
    internal sealed class CTRMover(int l, float m_, float r_) : Mover(l, m_, r_)
    {
        public override void SetPathFromStringandStart(string p, Vector s)
        {
            if (p.CharacterAtIndex(0) == 'R')
            {
                bool flag = p.CharacterAtIndex(1) == 'C';
                int num = (int)RTD(p.SubstringFromIndex(2).IntValue());
                num *= 3;
                int num2 = num / 2;
                float num3 = (float)(6.283185307179586 / num2);
                if (!flag)
                {
                    num3 = 0f - num3;
                }
                float num4 = 0f;
                for (int i = 0; i < num2; i++)
                {
                    float x = s.x + (num * Cosf(num4));
                    float y = s.y + (num * Sinf(num4));
                    AddPathPoint(Vect(x, y));
                    num4 += num3;
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
                string nSString2 = list[j];
                string nSString3 = list[j + 1];
                AddPathPoint(Vect(s.x + (nSString2.FloatValue() * 3f), s.y + (nSString3.FloatValue() * 3f)));
            }
        }
    }
}
