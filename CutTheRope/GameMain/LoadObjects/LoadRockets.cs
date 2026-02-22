using System.Xml.Linq;

using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;
using CutTheRope.Helpers;
using System.Globalization;

namespace CutTheRope.GameMain
{
    internal sealed partial class GameScene
    {
        private void LoadRocket(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            Rocket rocket = Rocket.Rocket_createWithResIDQuad(Resources.Img.ObjRocket, 10);
            rocket.scaleX = rocket.scaleY = 0.7f;
            rocket.DoRestoreCutTransparency();
            rocket.delegateRocketDelegate = this;

            Vector quadCenter = Image.GetQuadCenter(Resources.Img.ObjRocket, 10);
            Vector quadSize = Image.GetQuadSize(Resources.Img.ObjRocket, 10);
            quadSize.X *= 0.6f;
            quadSize.Y *= 0.05f;
            rocket.bb = MakeRectangle(quadCenter.X - (quadSize.X / 2f), quadCenter.Y - (quadSize.Y / 2f), quadSize.X, quadSize.Y);

            rocket.x = ((string.IsNullOrEmpty(xmlNode.AttributeAsNSString("x")) ? 0 : int.Parse(xmlNode.AttributeAsNSString("x"))) * scale) + offsetX + mapOffsetX;
            rocket.y = ((string.IsNullOrEmpty(xmlNode.AttributeAsNSString("y")) ? 0 : int.Parse(xmlNode.AttributeAsNSString("y"))) * scale) + offsetY + mapOffsetY;
            rocket.rotation = (string.IsNullOrEmpty(xmlNode.AttributeAsNSString("angle")) ? 0f : float.Parse(xmlNode.AttributeAsNSString("angle"), CultureInfo.InvariantCulture)) - DEG_180;
            rocket.impulse = string.IsNullOrEmpty(xmlNode.AttributeAsNSString("impulse")) ? 0f : float.Parse(xmlNode.AttributeAsNSString("impulse"), CultureInfo.InvariantCulture);
            rocket.impulseFactor = string.IsNullOrEmpty(xmlNode.AttributeAsNSString("impulseFactor")) ? 0f : float.Parse(xmlNode.AttributeAsNSString("impulseFactor"), CultureInfo.InvariantCulture);
            if (rocket.impulseFactor == 0f)
            {
                rocket.impulseFactor = 0.6f;
            }
            rocket.time = string.IsNullOrEmpty(xmlNode.AttributeAsNSString("time")) ? 0f : float.Parse(xmlNode.AttributeAsNSString("time"), CultureInfo.InvariantCulture);
            rocket.isRotatable = xmlNode.AttributeAsNSString("isRotatable") == "true";
            rocket.startRotation = rocket.rotation;
            rocket.ParseMover(xmlNode);
            rocket.RotateWithBB(rocket.rotation);
            rocket.UpdateRotation();
            rocket.anchor = 18;
            rocket.state = Rocket.STATE_ROCKET_IDLE;

            rockets.Add(rocket);
            rocket.point.pos.X = rocket.x;
            rocket.point.pos.Y = rocket.y;

            if (rocket.isRotatable)
            {
                Image marker = Image.Image_createWithResIDQuad(Resources.Img.ObjRocket, 0);
                marker.parentAnchor = marker.anchor = 18;
                marker.DoRestoreCutTransparency();
                marker.x = rocket.x;
                marker.y = rocket.y;
                _ = decalsLayer.AddChild(marker);
            }
        }
    }
}
