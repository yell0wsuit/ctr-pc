using System.Xml.Linq;

using CutTheRope.Helpers;
using CutTheRope.iframework.core;

namespace CutTheRope.game
{
    /// <summary>
    /// Handles loading sock objects from XML level data
    /// Socks can be teleport destinations for the candy
    /// </summary>
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Loads a sock object from XML node data
        /// </summary>
        private void LoadSock(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            CTRRootController cTRRootController = (CTRRootController)Application.SharedRootController();
            Sock sock = Sock.Sock_createWithResID(85);
            sock.CreateAnimations();
            sock.scaleX = sock.scaleY = 0.7f;
            sock.DoRestoreCutTransparency();
            sock.x = (xmlNode.AttributeAsNSString("x").IntValue() * scale) + offsetX + mapOffsetX;
            sock.y = (xmlNode.AttributeAsNSString("y").IntValue() * scale) + offsetY + mapOffsetY;
            sock.group = xmlNode.AttributeAsNSString("group").IntValue();
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
            sock.ParseMover(xmlNode);
            sock.rotation += 90f;
            if (sock.mover != null)
            {
                sock.mover.angle_ += 90.0;
                sock.mover.angle_initial = sock.mover.angle_;
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
