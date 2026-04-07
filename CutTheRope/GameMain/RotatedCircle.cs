using System;
using System.Collections.Generic;

using CutTheRope.Desktop;
using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Visual;

using Microsoft.Xna.Framework;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Rotatable vinyl circle object that tracks contained game objects and exposes one or two controller handles.
    /// </summary>
    internal sealed class RotatedCircle : BaseElement
    {
        /// <summary>
        /// Creates the vinyl circle visuals, controller handles, and contained-object collection.
        /// </summary>
        public RotatedCircle()
        {
            containedObjects = [];
            soundPlaying = -1;
            vinilStickerL = Image.Image_createWithResIDQuad(VinylTexture, 2);
            vinilStickerL.anchor = 20;
            vinilStickerL.parentAnchor = 18;
            vinilStickerL.rotationCenterX = vinilStickerL.width / 2f;
            vinilStickerL.x = 1f;
            vinilStickerR = Image.Image_createWithResIDQuad(VinylTexture, 2);
            vinilStickerR.scaleX = -1f;
            vinilStickerR.anchor = 20;
            vinilStickerR.parentAnchor = 18;
            vinilStickerR.rotationCenterX = vinilStickerR.width / 2f;
            vinilStickerR.x = -1f;
            vinilCenter = Image.Image_createWithResIDQuad(VinylTexture, 3);
            vinilCenter.anchor = 18;
            vinilHighlightL = Image.Image_createWithResIDQuad(VinylTexture, 1);
            vinilHighlightL.anchor = 12;
            vinilHighlightR = Image.Image_createWithResIDQuad(VinylTexture, 1);
            vinilHighlightR.scaleX = -1f;
            vinilHighlightR.anchor = 9;
            vinilControllerL = Image.Image_createWithResIDQuad(VinylTexture, 5);
            vinilControllerL.anchor = 18;
            vinilControllerL.rotation = DEG_90;
            vinilControllerR = Image.Image_createWithResIDQuad(VinylTexture, 5);
            vinilControllerR.anchor = 18;
            vinilControllerR.rotation = -DEG_90;
            vinilActiveControllerL = Image.Image_createWithResIDQuad(VinylTexture, 4);
            vinilActiveControllerL.anchor = vinilControllerL.anchor;
            vinilActiveControllerL.rotation = vinilControllerL.rotation;
            vinilActiveControllerL.visible = false;
            vinilActiveControllerR = Image.Image_createWithResIDQuad(VinylTexture, 4);
            vinilActiveControllerR.anchor = vinilControllerR.anchor;
            vinilActiveControllerR.rotation = vinilControllerR.rotation;
            vinilActiveControllerR.visible = false;
            vinil = Image.Image_createWithResIDQuad(VinylTexture, 0);
            vinil.anchor = 18;
            passColorToChilds = false;
            _ = AddChild(vinilStickerL);
            _ = AddChild(vinilStickerR);
            _ = AddChild(vinilActiveControllerL);
            _ = AddChild(vinilActiveControllerR);
            _ = AddChild(vinilControllerL);
            _ = AddChild(vinilControllerR);
        }

        /// <summary>
        /// Sets the circle size and rescales all visual parts and controllers.
        /// </summary>
        /// <param name="value">Circle size in world units.</param>
        public void SetSize(float value)
        {
            size = value;
            float baseScale = size / 167f;
            vinilHighlightL.scaleX = vinilHighlightL.scaleY = vinilHighlightR.scaleY = baseScale;
            vinilHighlightR.scaleX = 0f - baseScale;
            vinil.scaleX = vinil.scaleY = baseScale;
            float stickerScale = baseScale >= 0.4f ? baseScale : 0.4f;
            vinilStickerL.scaleX = vinilStickerL.scaleY = vinilStickerR.scaleY = stickerScale;
            vinilStickerR.scaleX = 0f - stickerScale;
            float controllerScale = baseScale >= 0.75f ? baseScale : 0.75f;
            vinilControllerL.scaleX = vinilControllerL.scaleY = vinilControllerR.scaleX = vinilControllerR.scaleY = controllerScale;
            vinilActiveControllerL.scaleX = vinilActiveControllerL.scaleY = vinilActiveControllerR.scaleX = vinilActiveControllerR.scaleY = controllerScale;
            vinilCenter.scaleX = 1f - ((1f - vinilStickerL.scaleX) * 0.5f);
            vinilCenter.scaleY = vinilCenter.scaleX;
            sizeInPixels = vinilHighlightL.width * vinilHighlightL.scaleX;
            UpdateChildPositions();
        }

        /// <summary>
        /// Gets whether the circle is using a single visible controller handle.
        /// </summary>
        /// <returns>Whether the left controller handle is hidden.</returns>
        public bool HasOneHandle()
        {
            return !vinilControllerL.visible;
        }

        /// <summary>
        /// Sets whether the circle should expose only one controller handle.
        /// </summary>
        /// <param name="value">Whether to hide the left controller handle.</param>
        public void SetHasOneHandle(bool value)
        {
            vinilControllerL.visible = !value;
        }

        /// <summary>
        /// Gets whether the left controller handle is active.
        /// </summary>
        /// <returns>Whether the left active-controller visual is visible.</returns>
        public bool IsLeftControllerActive()
        {
            return vinilActiveControllerL.visible;
        }

        /// <summary>
        /// Sets whether the left controller handle is active.
        /// </summary>
        /// <param name="value">Whether to show the left active-controller visual.</param>
        public void SetIsLeftControllerActive(bool value)
        {
            vinilActiveControllerL.visible = value;
        }

        /// <summary>
        /// Gets whether the right controller handle is active.
        /// </summary>
        /// <returns>Whether the right active-controller visual is visible.</returns>
        public bool IsRightControllerActive()
        {
            return vinilActiveControllerR.visible;
        }

        /// <summary>
        /// Sets whether the right controller handle is active.
        /// </summary>
        /// <param name="value">Whether to show the right active-controller visual.</param>
        public void SetIsRightControllerActive(bool value)
        {
            vinilActiveControllerR.visible = value;
        }

        /// <summary>
        /// Checks whether this circle shares any contained object with another circle in <see cref="circlesArray"/>.
        /// </summary>
        /// <returns>Whether another circle contains at least one of the same objects.</returns>
        public bool ContainsSameObjectWithAnotherCircle()
        {
            foreach (object obj in circlesArray)
            {
                RotatedCircle item = (RotatedCircle)obj;
                if (item != this && ContainsSameObjectWithCircle(item))
                {
                    return true;
                }
            }
            return false;
        }

        /// <inheritdoc />
        public override void Draw()
        {
            if (IsRightControllerActive() || IsLeftControllerActive())
            {
                Renderer.Disable(Renderer.GL_TEXTURE_2D);
                Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
                RGBAColor whiteRGBA = RGBAColor.whiteRGBA;
                if (color.AlphaChannel != 1)
                {
                    whiteRGBA.AlphaChannel = color.AlphaChannel;
                }
                DrawHelper.DrawAntialiasedCurve2(x, y, sizeInPixels + (ACTIVE_CIRCLE_WIDTH * vinilControllerL.scaleX), 0f, MathF.Tau, 81, (ACTIVE_CIRCLE_WIDTH + (RTPD(1) * 3f)) * vinilControllerL.scaleX, 5f, whiteRGBA);
                Renderer.SetColor(Color.White);
                Renderer.Enable(Renderer.GL_TEXTURE_2D);
            }
            vinilHighlightL.color = color;
            vinilHighlightR.color = color;
            vinilControllerL.color = color;
            vinilControllerR.color = color;
            vinil.color = color;
            vinil.Draw();
            Renderer.Disable(Renderer.GL_TEXTURE_2D);
            Renderer.SetBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONEMINUSSRCALPHA);
            foreach (object obj in circlesArray)
            {
                RotatedCircle item = (RotatedCircle)obj;
                if (item != this && item.ContainsSameObjectWithAnotherCircle() && circlesArray.IndexOf(item) < circlesArray.IndexOf(this))
                {
                    DrawHelper.DrawCircleIntersection(x, y, sizeInPixels, item.x, item.y, item.sizeInPixels, 81, OUTER_CIRCLE_WIDTH * item.vinilHighlightL.scaleX * 0.5f, CONTOUR_COLOR);
                }
            }
            Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
            Renderer.SetColor(Color.White);
            Renderer.Enable(Renderer.GL_TEXTURE_2D);
            vinilHighlightL.Draw();
            vinilHighlightR.Draw();
            base.Draw();
            vinilCenter.Draw();
        }

        /// <summary>
        /// Updates child visual positions from the current circle position and size.
        /// </summary>
        public void UpdateChildPositions()
        {
            vinil.x = vinilCenter.x = x;
            vinil.y = vinilCenter.y = y;
            float highlightXOffset = vinilHighlightL.width / 2 * (1f - vinilHighlightL.scaleX);
            float highlightYOffset = vinilHighlightL.height / 2 * (1f - vinilHighlightL.scaleY);
            float controllerXOffset = sizeInPixels - RTPD(CONTROLLER_SHIFT_PARAM1 - (CONTROLLER_SHIFT_PARAM2 * size)) + ((1f - vinilControllerL.scaleX) * (vinilControllerL.width / 2));
            vinilHighlightL.x = x + highlightXOffset;
            vinilHighlightR.x = x - highlightXOffset;
            vinilHighlightL.y = vinilHighlightR.y = y - highlightYOffset;
            vinilControllerL.x = x - controllerXOffset;
            vinilControllerR.x = x + controllerXOffset;
            vinilControllerL.y = vinilControllerR.y = y;
            vinilActiveControllerL.x = vinilControllerL.x;
            vinilActiveControllerL.y = vinilControllerL.y;
            vinilActiveControllerR.x = vinilControllerR.x;
            vinilActiveControllerR.y = vinilControllerR.y;
        }

        /// <summary>
        /// Checks whether this circle and another circle share a contained game object.
        /// </summary>
        /// <param name="anotherCircle">Circle to compare against.</param>
        /// <returns>Whether both circles contain at least one identical game object.</returns>
        public bool ContainsSameObjectWithCircle(RotatedCircle anotherCircle)
        {
            if (x == anotherCircle.x && y == anotherCircle.y && size == anotherCircle.size)
            {
                return false;
            }
            foreach (object obj in containedObjects)
            {
                GameObject containedObject = (GameObject)obj;
                if (anotherCircle.containedObjects.IndexOf(containedObject) != -1)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Creates a detached copy with the same position, size, handles, contained objects, and circle list.
        /// </summary>
        /// <returns>The copied rotated circle.</returns>
        public RotatedCircle Copy()
        {
            RotatedCircle rotatedCircle = new()
            {
                x = x,
                y = y,
                rotation = rotation,
                circlesArray = circlesArray,
                containedObjects = containedObjects,
                operating = -1
            };
            rotatedCircle.handle1 = Vect(rotatedCircle.x - RTPD(size * 3f), rotatedCircle.y);
            rotatedCircle.handle2 = Vect(rotatedCircle.x + RTPD(size * 3f), rotatedCircle.y);
            rotatedCircle.handle1 = VectRotateAround(rotatedCircle.handle1, DEGREES_TO_RADIANS(rotatedCircle.rotation), rotatedCircle.x, rotatedCircle.y);
            rotatedCircle.handle2 = VectRotateAround(rotatedCircle.handle2, DEGREES_TO_RADIANS(rotatedCircle.rotation), rotatedCircle.x, rotatedCircle.y);
            rotatedCircle.SetSize(size);
            rotatedCircle.SetHasOneHandle(HasOneHandle());
            rotatedCircle.vinilControllerL.visible = false;
            rotatedCircle.vinilControllerR.visible = false;
            return rotatedCircle;
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                vinilCenter?.Dispose();
                vinilCenter = null;
                vinilHighlightL?.Dispose();
                vinilHighlightL = null;
                vinilHighlightR?.Dispose();
                vinilHighlightR = null;
                vinil?.Dispose();
                vinil = null;
                vinilControllerL?.Dispose();
                vinilControllerL = null;
                vinilControllerR?.Dispose();
                vinilControllerR = null;
                vinilActiveControllerL?.Dispose();
                vinilActiveControllerL = null;
                vinilActiveControllerR?.Dispose();
                vinilActiveControllerR = null;
                vinilStickerL?.Dispose();
                vinilStickerL = null;
                vinilStickerR?.Dispose();
                vinilStickerR = null;
            }
            base.Dispose(disposing);
        }

        /// <summary>Pointer multiplier used by rotated circle sizing and placement.</summary>
        public const int PM = 3;

        /// <summary>Minimum controller visual scale.</summary>
        public const float CONTROLLER_MIN_SCALE = 0.75f;

        /// <summary>Minimum sticker visual scale.</summary>
        public const float STICKER_MIN_SCALE = 0.4f;

        /// <summary>Scale factor applied to the center visual relative to sticker scale.</summary>
        public const float CENTER_SCALE_FACTOR = 0.5f;

        /// <summary>Circle size that maps to full texture scale.</summary>
        public const float HUNDRED_PERCENT_SCALE_SIZE = 167f;

        /// <summary>Vertex count used when drawing circle outlines.</summary>
        public const int CIRCLE_VERTEX_COUNT = 80;

        /// <summary>Logical circle size in world units.</summary>
        public float size;

        /// <summary>Rendered circle radius in pixels.</summary>
        public float sizeInPixels;

        /// <summary>Identifier for the controller currently being operated, or -1 when idle.</summary>
        public int operating;

        /// <summary>Identifier for the sound currently playing for this circle, or -1 when idle.</summary>
        public int soundPlaying;

        /// <summary>Last touch position used while rotating the circle.</summary>
        public Vector lastTouch;

        /// <summary>World-space position of the first controller handle.</summary>
        public Vector handle1;

        /// <summary>World-space position of the second controller handle.</summary>
        public Vector handle2;

        /// <summary>Initial world-space position of the first controller handle.</summary>
        public Vector inithanlde1;

        /// <summary>Initial world-space position of the second controller handle.</summary>
        public Vector inithanlde2;

        /// <summary>Shared list of rotated circles in the level.</summary>
        public List<RotatedCircle> circlesArray;

        /// <summary>Game objects currently contained by this circle.</summary>
        public List<GameObject> containedObjects;

        /// <summary>Whether this circle should be removed on the next update.</summary>
        public bool removeOnNextUpdate;

        /// <summary>Left sticker visual.</summary>
        private Image vinilStickerL;

        /// <summary>Right sticker visual.</summary>
        private Image vinilStickerR;

        /// <summary>Left highlight visual.</summary>
        private Image vinilHighlightL;

        /// <summary>Right highlight visual.</summary>
        private Image vinilHighlightR;

        /// <summary>Left controller handle visual.</summary>
        private Image vinilControllerL;

        /// <summary>Right controller handle visual.</summary>
        private Image vinilControllerR;

        /// <summary>Left active-controller handle visual.</summary>
        private Image vinilActiveControllerL;

        /// <summary>Right active-controller handle visual.</summary>
        private Image vinilActiveControllerR;

        /// <summary>Center vinyl visual.</summary>
        private Image vinilCenter;

        /// <summary>Main vinyl body visual.</summary>
        private Image vinil;

        /// <summary>Texture resource used for rotated circle visuals.</summary>
        private const string VinylTexture = Resources.Img.ObjVinil;

        /// <summary>Color used when drawing overlapping circle contours.</summary>
        private RGBAColor CONTOUR_COLOR = RGBAColor.MakeRGBA(1, 1, 1, 0.2f);

        // private readonly float INNER_CIRCLE_WIDTH = RTPD(15) * 3f;

        /// <summary>Width of the outer intersection contour.</summary>
        private readonly float OUTER_CIRCLE_WIDTH = RTPD(7) * 3f;

        /// <summary>Width of the active controller outline.</summary>
        private readonly float ACTIVE_CIRCLE_WIDTH = RTPD(3) * 3f;

        /// <summary>Controller placement offset base parameter.</summary>
        private readonly float CONTROLLER_SHIFT_PARAM1 = 67.5f;

        /// <summary>Controller placement offset size multiplier.</summary>
        private readonly float CONTROLLER_SHIFT_PARAM2 = 0.089999996f;
    }
}
