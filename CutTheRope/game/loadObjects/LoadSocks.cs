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
    /// Handles loading sock objects from XML level data
    /// Socks can be teleport destinations for the candy
    /// </summary>
    internal sealed partial class GameScene
    {
        private void LoadSock(XMLNode item, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            if (item.Name != "sock") return;

            Sock sock = Sock.Sock_createWithResID(85);
            sock.CreateAnimations();
            sock.scaleX = sock.scaleY = 0.7f;
            sock.DoRestoreCutTransparency();
            sock.x = (item["x"].IntValue() * scale) + offsetX + mapOffsetX;
            sock.y = (item["y"].IntValue() * scale) + offsetY + mapOffsetY;
            sock.group = item["group"].IntValue();
            sock.anchor = 10;
            sock.rotationCenterY -= (sock.height / 2f) - 85f;
            if (sock.group == 0)
            {
                sock.SetDrawQuad(0);
            }
            else
            {
                sock.SetDrawQuad(1);
            }
            sock.state = Sock.SOCK_IDLE;
            sock.ParseMover(item);
            sock.rotation += 90f;
            if (sock.mover != null)
            {
                sock.mover.angle_ += 90.0;
                sock.mover.angle_initial = sock.mover.angle_;
                CTRRootController cTRRootController = (CTRRootController)Application.SharedRootController();
                if (cTRRootController.GetPack() == 3 && cTRRootController.GetLevel() == 24)
                {
                    sock.mover.use_angle_initial = true;
                }
            }
            sock.UpdateRotation();
            _ = socks.AddObject(sock);
        }
    }
}
