using System;
using System.Collections.Generic;

using CutTheRopeDX.Desktop;
using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Visual;

using Microsoft.Xna.Framework;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Alternate rotatable vinyl circle visual built from quadrant sprites.
    /// </summary>
    internal sealed class RotatedCircle2 : BaseElement
    {
        /// <summary>
        /// Sets the circle size and rescales all quadrant, sticker, highlight, and controller visuals.
        /// </summary>
        /// <param name="value">Circle size in world units.</param>
        public void SetSize(float value)
        {
            size = value;
            float baseScale = size / (vinilTL.width + (vinilTR.width * (1f - vinilTL.scaleX)));
            vinilHighlightL.scaleX = vinilHighlightL.scaleY = vinilHighlightR.scaleY = baseScale;
            vinilHighlightR.scaleX = 0f - baseScale;
            vinilBL.scaleX = vinilBL.scaleY = vinilBR.scaleY = baseScale;
            vinilBR.scaleX = 0f - baseScale;
            vinilTL.scaleX = baseScale;
            vinilTL.scaleY = 0f - baseScale;
            vinilTR.scaleX = vinilTR.scaleY = 0f - baseScale;
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
        /// Sets whether the circle should expose only one controller handle.
        /// </summary>
        /// <param name="value">Whether to hide the left controller handle.</param>
        public void SetHasOneHandle(bool value)
        {
            vinilControllerL.visible = !value;
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
        /// Sets whether the left controller handle is active.
        /// </summary>
        /// <param name="value">Whether to show the left active-controller visual.</param>
        public void SetIsLeftControllerActive(bool value)
        {
            vinilActiveControllerL.visible = value;
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
        /// Sets whether the right controller handle is active.
        /// </summary>
        /// <param name="value">Whether to show the right active-controller visual.</param>
        public void SetIsRightControllerActive(bool value)
        {
            vinilActiveControllerR.visible = value;
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
        /// Checks whether this circle shares any contained object with another circle in <see cref="circlesArray"/>.
        /// </summary>
        /// <returns>Whether another circle contains at least one of the same objects.</returns>
        public bool ContainsSameObjectWithAnotherCircle()
        {
            for (int i = 0; i < circlesArray.Count; i++)
            {
                RotatedCircle2 rotatedCircle = circlesArray[i];
                if (rotatedCircle != this && ContainsSameObjectWithCircle(rotatedCircle))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Creates the quadrant-based vinyl circle visuals, controller handles, and contained-object collection.
        /// </summary>
        public RotatedCircle2()
        {
            containedObjects = [];
            soundPlaying = -1;
            vinilStickerL = Image.Image_createWithResIDQuad(VinylTexture, 2);
            vinilStickerL.anchor = 20;
            vinilStickerL.rotationCenterX = vinilStickerL.width / 2f;
            vinilStickerR = Image.Image_createWithResIDQuad(VinylTexture, 2);
            vinilStickerR.scaleX = -1f;
            vinilStickerR.anchor = 20;
            vinilStickerR.rotationCenterX = vinilStickerR.width / 2f;
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
            vinilBL = Image.Image_createWithResIDQuad(VinylTexture, 0);
            vinilBL.anchor = 12;
            vinilBR = Image.Image_createWithResIDQuad(VinylTexture, 0);
            vinilBR.scaleX = -1f;
            vinilBR.anchor = 9;
            vinilTL = Image.Image_createWithResIDQuad(VinylTexture, 0);
            vinilTL.scaleY = -1f;
            vinilTL.anchor = 36;
            vinilTR = Image.Image_createWithResIDQuad(VinylTexture, 0);
            vinilTR.scaleX = vinilTR.scaleY = -1f;
            vinilTR.anchor = 33;
            passColorToChilds = false;
            _ = AddChild(vinilActiveControllerL);
            _ = AddChild(vinilActiveControllerR);
            _ = AddChild(vinilControllerL);
            _ = AddChild(vinilControllerR);
        }

        /// <summary>
        /// Creates a detached copy with the same position, size, handles, contained objects, and circle list.
        /// </summary>
        /// <returns>The copied rotated circle.</returns>
        public RotatedCircle2 Copy()
        {
            RotatedCircle2 rotatedCircle = new()
            {
                x = x,
                y = y,
                rotation = rotation,
                circlesArray = circlesArray,
                containedObjects = containedObjects,
                operating = -1
            };
            rotatedCircle.handle1 = new Vector(rotatedCircle.x - size, rotatedCircle.y);
            rotatedCircle.handle2 = new Vector(rotatedCircle.x + size, rotatedCircle.y);
            rotatedCircle.handle1 = VectRotateAround(rotatedCircle.handle1, DEGREES_TO_RADIANS(rotatedCircle.rotation), rotatedCircle.x, rotatedCircle.y);
            rotatedCircle.handle2 = VectRotateAround(rotatedCircle.handle2, DEGREES_TO_RADIANS(rotatedCircle.rotation), rotatedCircle.x, rotatedCircle.y);
            rotatedCircle.SetSize(size);
            rotatedCircle.SetHasOneHandle(HasOneHandle());
            rotatedCircle.vinilControllerL.visible = false;
            rotatedCircle.vinilControllerR.visible = false;
            return rotatedCircle;
        }

        /// <inheritdoc />
        public override void Draw()
        {
            if (IsRightControllerActive() || IsLeftControllerActive())
            {
                Renderer.Disable(Renderer.GL_TEXTURE_2D);
                Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
                DrawHelper.DrawAntialiasedCurve2(x, y, sizeInPixels + (3f * MathF.Abs(vinilTR.scaleX)), 0f, MathF.Tau, 51, 2f, 1f * MathF.Abs(vinilTR.scaleX), RGBAColor.whiteRGBA);
            }
            Renderer.Enable(Renderer.GL_TEXTURE_2D);
            Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
            vinilTL.color = vinilTR.color = vinilBL.color = vinilBR.color = RGBAColor.solidOpaqueRGBA;
            vinilTL.Draw();
            vinilTR.Draw();
            vinilBL.Draw();
            vinilBR.Draw();
            Renderer.Disable(Renderer.GL_TEXTURE_2D);
            Renderer.SetBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONEMINUSSRCALPHA);
            if (IsRightControllerActive() || IsLeftControllerActive() || color.AlphaChannel < 1)
            {
                RGBAColor whiteRGBA = RGBAColor.whiteRGBA;
                whiteRGBA.AlphaChannel = 1f - color.AlphaChannel;
                DrawHelper.DrawAntialiasedCurve2(x, y, sizeInPixels + 1f, 0f, MathF.Tau, 51, 2f, 1f * MathF.Abs(vinilTR.scaleX), whiteRGBA);
            }
            for (int i = 0; i < circlesArray.Count; i++)
            {
                RotatedCircle2 rotatedCircle = circlesArray[i];
                if (rotatedCircle != this && rotatedCircle.ContainsSameObjectWithAnotherCircle() && circlesArray.IndexOf(rotatedCircle) < circlesArray.IndexOf(this))
                {
                    DrawHelper.DrawCircleIntersection(x, y, sizeInPixels, rotatedCircle.x, rotatedCircle.y, rotatedCircle.sizeInPixels, 51, 7f * rotatedCircle.vinilHighlightL.scaleX * 0.5f, CONTOUR_COLOR);
                }
            }
            Renderer.Enable(Renderer.GL_TEXTURE_2D);
            Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
            Renderer.SetColor(Color.White);
            Renderer.Enable(Renderer.GL_TEXTURE_2D);
            vinilHighlightL.color = color;
            vinilHighlightR.color = color;
            vinilHighlightL.Draw();
            vinilHighlightR.Draw();
            vinilStickerL.x = vinilStickerR.x = x;
            vinilStickerL.y = vinilStickerR.y = y;
            vinilStickerL.rotation = vinilStickerR.rotation = rotation;
            vinilStickerL.Draw();
            vinilStickerR.Draw();
            Renderer.Disable(Renderer.GL_TEXTURE_2D);
            Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
            DrawHelper.DrawAntialiasedCurve2(x, y, vinilStickerL.width * vinilStickerL.scaleX, 0f, MathF.Tau, 51, 1f, vinilStickerL.scaleX * 1.5f, INNER_CIRCLE_COLOR1);
            DrawHelper.DrawAntialiasedCurve2(x, y, (vinilStickerL.width - 2) * vinilStickerL.scaleX, 0f, MathF.Tau, 51, 0f, vinilStickerL.scaleX * 1f, INNER_CIRCLE_COLOR2);
            Renderer.SetColor(Color.White);
            Renderer.Enable(Renderer.GL_TEXTURE_2D);
            vinilControllerL.color = color;
            vinilControllerR.color = color;
            base.Draw();
            vinilCenter.Draw();
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
                vinilBL?.Dispose();
                vinilBL = null;
                vinilBR?.Dispose();
                vinilBR = null;
                vinilTL?.Dispose();
                vinilTL = null;
                vinilTR?.Dispose();
                vinilTR = null;
                vinilStickerL?.Dispose();
                vinilStickerL = null;
                vinilStickerR?.Dispose();
                vinilStickerR = null;
                containedObjects?.Clear();
                containedObjects = null;
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Updates child visual positions from the current circle position and size.
        /// </summary>
        public void UpdateChildPositions()
        {
            vinilCenter.x = x;
            vinilCenter.y = y;
            float highlightXOffset = vinilHighlightL.width / 2 * (1f - vinilHighlightL.scaleX);
            float highlightYOffset = vinilHighlightL.height / 2 * (1f - vinilHighlightL.scaleY);
            float cornerXOffset = (vinilBL.width + 4) / 2f * (1f - vinilBL.scaleX);
            float cornerYOffset = (vinilBL.height + 4) / 2f * (1f - vinilBL.scaleY);
            float rightControllerInset = MathF.Abs(vinilControllerR.scaleX) < 1f ? (1f - MathF.Abs(vinilControllerR.scaleX)) * 10f : 0f;
            float topLeftInset = MathF.Abs(vinilTL.scaleX) < 0.45f ? ((0.45f - MathF.Abs(vinilTL.scaleX)) * 10f) + 1f : 0f;
            float controllerXOffset = MathF.Abs(vinilBL.height * vinilBL.scaleY) - MathF.Abs(vinilControllerR.height * 0.58f * vinilControllerR.scaleY / 2f) - rightControllerInset - topLeftInset;
            vinilHighlightL.x = x + highlightXOffset;
            vinilHighlightR.x = x - highlightXOffset;
            vinilHighlightL.y = vinilHighlightR.y = y - highlightYOffset;
            vinilBL.x = vinilTL.x = x + cornerXOffset;
            vinilBL.y = vinilBR.y = y - cornerYOffset;
            vinilBR.x = vinilTR.x = x - cornerXOffset;
            vinilTL.y = vinilTR.y = y + cornerYOffset;
            vinilControllerL.x = x - controllerXOffset;
            vinilControllerR.x = x + controllerXOffset;
            vinilControllerL.y = vinilControllerR.y = y;
            vinilActiveControllerL.x = vinilControllerL.x;
            vinilActiveControllerL.y = vinilControllerL.y;
            vinilActiveControllerR.x = vinilControllerR.x;
            vinilActiveControllerR.y = vinilControllerR.y;
        }

        /// <summary>
        /// Checks whether this circle and another circle share a contained object.
        /// </summary>
        /// <param name="anotherCircle">Circle to compare against.</param>
        /// <returns>Whether both circles contain at least one identical contained object.</returns>
        public bool ContainsSameObjectWithCircle(RotatedCircle2 anotherCircle)
        {
            if (x == anotherCircle.x && y == anotherCircle.y && size == anotherCircle.size)
            {
                return false;
            }
            for (int i = 0; i < containedObjects.Count; i++)
            {
                GameObject item = (GameObject)containedObjects[i];
                if (anotherCircle.containedObjects.IndexOf(item) != -1)
                {
                    return true;
                }
            }
            return false;
        }

        // private RGBAColor CIRCLE_COLOR1 = RGBAColor.MakeRGBA(0.306, 0.298, 0.454, 1);

        // private RGBAColor CIRCLE_COLOR2 = RGBAColor.MakeRGBA(0.239, 0.231, 0.356, 1);

        // private RGBAColor CIRCLE_COLOR3 = RGBAColor.MakeRGBA(0.29, 0.286, 0.419, 1);

        /// <summary>Primary inner circle color.</summary>
        private RGBAColor INNER_CIRCLE_COLOR1 = RGBAColor.MakeRGBA(0.6901960784313725f, 0.4196078431372549f, 0.07450980392156863f, 1);

        /// <summary>Secondary inner circle color.</summary>
        private RGBAColor INNER_CIRCLE_COLOR2 = RGBAColor.MakeRGBA(0.9294117647058824f, 0.611764705882353f, 0.07450980392156863f, 1);

        /// <summary>Color used when drawing overlapping circle contours.</summary>
        private RGBAColor CONTOUR_COLOR = RGBAColor.MakeRGBA(1, 1, 1, 0.2f);

        /// <summary>Logical circle size in world units.</summary>
        public float size;

        /// <summary>Rendered circle radius in pixels.</summary>
        public float sizeInPixels;

        /// <summary>Identifier for the controller currently being operated, or -1 when idle.</summary>
        public int operating;

        /// <summary>Identifier for the sound currently playing for this circle, or -1 when idle.</summary>
        public int soundPlaying;

        // public Vector lastTouch;

        /// <summary>World-space position of the first controller handle.</summary>
        public Vector handle1;

        /// <summary>World-space position of the second controller handle.</summary>
        public Vector handle2;

        // public Vector inithanlde1;

        // public Vector inithanlde2;

        /// <summary>Shared list of alternate rotated circles in the level.</summary>
        public List<RotatedCircle2> circlesArray;

        /// <summary>Objects currently contained by this circle.</summary>
        public List<BaseElement> containedObjects;

        // public bool removeOnNextUpdate;

        /// <summary>Left sticker visual.</summary>
        private Image vinilStickerL;

        /// <summary>Right sticker visual.</summary>
        private Image vinilStickerR;

        /// <summary>Left highlight visual.</summary>
        private Image vinilHighlightL;

        /// <summary>Right highlight visual.</summary>
        private Image vinilHighlightR;

        /// <summary>Left controller handle visual.</summary>
        private readonly Image vinilControllerL;

        /// <summary>Right controller handle visual.</summary>
        private readonly Image vinilControllerR;

        /// <summary>Left active-controller handle visual.</summary>
        private readonly Image vinilActiveControllerL;

        /// <summary>Right active-controller handle visual.</summary>
        private readonly Image vinilActiveControllerR;

        /// <summary>Center vinyl visual.</summary>
        private Image vinilCenter;

        /// <summary>Top-left vinyl quadrant visual.</summary>
        private Image vinilTL;

        /// <summary>Top-right vinyl quadrant visual.</summary>
        private Image vinilTR;

        /// <summary>Bottom-left vinyl quadrant visual.</summary>
        private Image vinilBL;

        /// <summary>Texture resource used for rotated circle visuals.</summary>
        private const string VinylTexture = Resources.Img.ObjVinil;

        /// <summary>Bottom-right vinyl quadrant visual.</summary>
        private Image vinilBR;
    }
}
