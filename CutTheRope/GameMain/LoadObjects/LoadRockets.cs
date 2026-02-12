using System.Xml.Linq;

using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;
using CutTheRope.Helpers;

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

            rocket.x = (xmlNode.AttributeAsNSString("x").IntValue() * scale) + offsetX + mapOffsetX;
            rocket.y = (xmlNode.AttributeAsNSString("y").IntValue() * scale) + offsetY + mapOffsetY;
            rocket.rotation = xmlNode.AttributeAsNSString("angle").FloatValue() - 180f;
            rocket.impulse = xmlNode.AttributeAsNSString("impulse").FloatValue();
            rocket.impulseFactor = xmlNode.AttributeAsNSString("impulseFactor").FloatValue();
            if (rocket.impulseFactor == 0f)
            {
                rocket.impulseFactor = 0.6f;
            }
            rocket.time = xmlNode.AttributeAsNSString("time").FloatValue();
            rocket.isRotatable = xmlNode.AttributeAsNSString("isRotatable").IsEqualToString("true");
            rocket.startRotation = rocket.rotation;
            rocket.ParseMover(xmlNode);
            rocket.RotateWithBB(rocket.rotation);
            rocket.UpdateRotation();
            rocket.anchor = 18;
            rocket.state = Rocket.STATE_ROCKET_IDLE;

            _ = rockets.AddObject(rocket);
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
