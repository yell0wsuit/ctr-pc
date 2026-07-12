using System.Xml.Linq;

using CutTheRopeDX.Framework.Core;

using static CutTheRopeDX.Helpers.ParsingHelpers;

namespace CutTheRopeDX.GameMain
{
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Christmas magic sock.
        /// </summary>
        private Sock XmasSock;

        /// <summary>
        /// Loads a sock object from XML node data
        /// </summary>
        /// <param name="xmlNode">The XML node describing the sock.</param>
        /// <param name="scale">The level scale factor applied to object coordinates.</param>
        /// <param name="offsetX">The base X offset applied to loaded objects.</param>
        /// <param name="offsetY">The base Y offset applied to loaded objects.</param>
        /// <param name="mapOffsetX">The additional map X offset applied during loading.</param>
        /// <param name="mapOffsetY">The additional map Y offset applied during loading.</param>
        private void LoadSock(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            CTRRootController cTRRootController = (CTRRootController)Application.SharedRootController();
            XmasSock = SpecialEvents.IsXmas ? Sock.Sock_createWithResID(Resources.Img.ObjSock) : Sock.Sock_createWithResID(Resources.Img.ObjHat);
            Sock sock = XmasSock;
            sock.CreateAnimations();
            sock.scaleX = sock.scaleY = 0.7f;
            sock.DoRestoreCutTransparency();
            sock.x = (ParseCoordinateIntOrZero(xmlNode.Attribute("x")?.Value) * scale) + offsetX + mapOffsetX;
            sock.y = (ParseCoordinateIntOrZero(xmlNode.Attribute("y")?.Value) * scale) + offsetY + mapOffsetY;
            sock.group = ParseIntOrZero(xmlNode.Attribute("group")?.Value);
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
            sock.rotation += DEG_90;
            if (sock.mover != null)
            {
                sock.mover.angle_ += DEG_90;
                sock.mover.angle_initial = sock.mover.angle_;
                if (cTRRootController.GetPack() == 3 && cTRRootController.GetLevel() == 24)
                {
                    sock.mover.use_angle_initial = true;
                }
            }
            sock.UpdateRotation();
            socks.Add(sock);
        }
    }
}
