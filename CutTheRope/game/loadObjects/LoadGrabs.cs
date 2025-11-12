using CutTheRope.desktop;
using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.sfe;
using CutTheRope.iframework.visual;
using CutTheRope.ios;

namespace CutTheRope.game
{
    /// <summary>
    /// Handles loading grab/hook objects from XML level data
    /// Grabs are rope attachment points and can have spiders or bees
    /// </summary>
    internal sealed partial class GameScene
    {
        private void LoadGrab(XMLNode item, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            if (item.Name != "grab") return;

            float hx = (item["x"].IntValue() * scale) + offsetX + mapOffsetX;
            float hy = (item["y"].IntValue() * scale) + offsetY + mapOffsetY;
            float len = item["length"].IntValue() * scale;
            float num12 = item["radius"].FloatValue();
            bool wheel = item["wheel"].IsEqualToString("true");
            float k = item["moveLength"].FloatValue() * scale;
            bool v = item["moveVertical"].IsEqualToString("true");
            float o = item["moveOffset"].FloatValue() * scale;
            bool spider = item["spider"].IsEqualToString("true");
            bool flag = item["part"].IsEqualToString("L");
            bool flag2 = item["hidePath"].IsEqualToString("true");
            Grab grab = (Grab)new Grab().Init();
            grab.initial_x = grab.x = hx;
            grab.initial_y = grab.y = hy;
            grab.initial_rotation = 0f;
            grab.wheel = wheel;
            grab.SetSpider(spider);
            grab.ParseMover(item);
            if (grab.mover != null)
            {
                grab.SetBee();
                if (!flag2)
                {
                    int num13 = 3;
                    bool flag3 = item["path"].HasPrefix(NSS("R"));
                    for (int l = 0; l < grab.mover.pathLen - 1; l++)
                    {
                        if (!flag3 || l % num13 == 0)
                        {
                            pollenDrawer.FillWithPolenFromPathIndexToPathIndexGrab(l, l + 1, grab);
                        }
                    }
                    if (grab.mover.pathLen > 2)
                    {
                        pollenDrawer.FillWithPolenFromPathIndexToPathIndexGrab(0, grab.mover.pathLen - 1, grab);
                    }
                }
            }
            if (num12 != -1f)
            {
                num12 *= scale;
            }
            if (num12 == -1f)
            {
                ConstraintedPoint constraintedPoint = star;
                if (twoParts != 2)
                {
                    constraintedPoint = flag ? starL : starR;
                }
                Bungee bungee = (Bungee)new Bungee().InitWithHeadAtXYTailAtTXTYandLength(null, hx, hy, constraintedPoint, constraintedPoint.pos.x, constraintedPoint.pos.y, len);
                bungee.bungeeAnchor.pin = bungee.bungeeAnchor.pos;
                grab.SetRope(bungee);
            }
            grab.SetRadius(num12);
            grab.SetMoveLengthVerticalOffset(k, v, o);
            _ = bungees.AddObject(grab);
        }
    }
}
