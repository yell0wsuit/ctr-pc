using System.Xml.Linq;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Visual;

using static CutTheRopeDX.Helpers.ParsingHelpers;

namespace CutTheRopeDX.GameMain
{
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Loads a rocket object from XML node data.
        /// </summary>
        /// <param name="xmlNode">The XML node describing the rocket.</param>
        /// <param name="scale">The level scale factor applied to object coordinates.</param>
        /// <param name="offsetX">The base X offset applied to loaded objects.</param>
        /// <param name="offsetY">The base Y offset applied to loaded objects.</param>
        /// <param name="mapOffsetX">The additional map X offset applied during loading.</param>
        /// <param name="mapOffsetY">The additional map Y offset applied during loading.</param>
        private void LoadRocket(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            Rocket rocket = Rocket.Rocket_createWithResIDQuad(Resources.Img.ObjRocket, 10);
            rocket.scaleX = rocket.scaleY = 0.7f;
            rocket.DoRestoreCutTransparency();
            rocket.delegateRocketDelegate = this;

            // Catch-slat bb (0.6 x quad width, 0.05 x quad height of the rocket body quad),
            // pinned from original XML quad data and center-relative so atlas repacks can't move it.
            float catchWidth = ActivePhysicsConstants.RocketCatchBoxWidth;
            float catchHeight = ActivePhysicsConstants.RocketCatchBoxHeight;
            float catchCenterX = (rocket.width / 2f) + ActivePhysicsConstants.RocketCatchBoxCenterOffsetX;
            float catchCenterY = (rocket.height / 2f) + ActivePhysicsConstants.RocketCatchBoxCenterOffsetY;
            rocket.bb = MakeRectangle(catchCenterX - (catchWidth / 2f), catchCenterY - (catchHeight / 2f), catchWidth, catchHeight);

            rocket.x = (ParseCoordinateIntOrZero(xmlNode.Attribute("x")?.Value) * scale) + offsetX + mapOffsetX;
            rocket.y = (ParseCoordinateIntOrZero(xmlNode.Attribute("y")?.Value) * scale) + offsetY + mapOffsetY;
            rocket.rotation = ParseFloatOrZero(xmlNode.Attribute("angle")?.Value) - DEG_180;
            rocket.impulse = ParseFloatOrZero(xmlNode.Attribute("impulse")?.Value);
            rocket.impulseFactor = ParseFloatOrZero(xmlNode.Attribute("impulseFactor")?.Value);
            if (rocket.impulseFactor == 0f)
            {
                rocket.impulseFactor = 0.6f;
            }
            rocket.time = ParseFloatOrZero(xmlNode.Attribute("time")?.Value);
            _ = bool.TryParse(xmlNode.Attribute("isRotatable")?.Value, out bool isRotatable);
            rocket.isRotatable = isRotatable;
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
