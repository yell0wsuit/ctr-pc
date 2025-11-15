using System;
using System.Collections.Generic;

using CutTheRope.desktop;
using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.visual;

using Microsoft.Xna.Framework;

namespace CutTheRope.game
{
    internal sealed class RotatedCircle2 : BaseElement
    {
        public void SetSize(float value)
        {
            size = value;
            float num = size / (vinilTL.width + (vinilTR.width * (1f - vinilTL.scaleX)));
            vinilHighlightL.scaleX = vinilHighlightL.scaleY = vinilHighlightR.scaleY = num;
            vinilHighlightR.scaleX = 0f - num;
            vinilBL.scaleX = vinilBL.scaleY = vinilBR.scaleY = num;
            vinilBR.scaleX = 0f - num;
            vinilTL.scaleX = num;
            vinilTL.scaleY = 0f - num;
            vinilTR.scaleX = vinilTR.scaleY = 0f - num;
            float num2 = num >= 0.4f ? num : 0.4f;
            vinilStickerL.scaleX = vinilStickerL.scaleY = vinilStickerR.scaleY = num2;
            vinilStickerR.scaleX = 0f - num2;
            float num3 = num >= 0.75f ? num : 0.75f;
            vinilControllerL.scaleX = vinilControllerL.scaleY = vinilControllerR.scaleX = vinilControllerR.scaleY = num3;
            vinilActiveControllerL.scaleX = vinilActiveControllerL.scaleY = vinilActiveControllerR.scaleX = vinilActiveControllerR.scaleY = num3;
            vinilCenter.scaleX = 1f - ((1f - vinilStickerL.scaleX) * 0.5f);
            vinilCenter.scaleY = vinilCenter.scaleX;
            sizeInPixels = vinilHighlightL.width * vinilHighlightL.scaleX;
            UpdateChildPositions();
        }

        public void SetHasOneHandle(bool value)
        {
            vinilControllerL.visible = !value;
        }

        public bool HasOneHandle()
        {
            return !vinilControllerL.visible;
        }

        public void SetIsLeftControllerActive(bool value)
        {
            vinilActiveControllerL.visible = value;
        }

        public bool IsLeftControllerActive()
        {
            return vinilActiveControllerL.visible;
        }

        public void SetIsRightControllerActive(bool value)
        {
            vinilActiveControllerR.visible = value;
        }

        public bool IsRightControllerActive()
        {
            return vinilActiveControllerR.visible;
        }

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

        public RotatedCircle2()
        {
            containedObjects = [];
            soundPlaying = -1;
            vinilStickerL = Image.Image_createWithResIDQuad(103, 2);
            vinilStickerL.anchor = 20;
            vinilStickerL.rotationCenterX = vinilStickerL.width / 2f;
            vinilStickerR = Image.Image_createWithResIDQuad(103, 2);
            vinilStickerR.scaleX = -1f;
            vinilStickerR.anchor = 20;
            vinilStickerR.rotationCenterX = vinilStickerR.width / 2f;
            vinilCenter = Image.Image_createWithResIDQuad(103, 3);
            vinilCenter.anchor = 18;
            vinilHighlightL = Image.Image_createWithResIDQuad(103, 1);
            vinilHighlightL.anchor = 12;
            vinilHighlightR = Image.Image_createWithResIDQuad(103, 1);
            vinilHighlightR.scaleX = -1f;
            vinilHighlightR.anchor = 9;
            vinilControllerL = Image.Image_createWithResIDQuad(103, 5);
            vinilControllerL.anchor = 18;
            vinilControllerL.rotation = 90f;
            vinilControllerR = Image.Image_createWithResIDQuad(103, 5);
            vinilControllerR.anchor = 18;
            vinilControllerR.rotation = -90f;
            vinilActiveControllerL = Image.Image_createWithResIDQuad(103, 4);
            vinilActiveControllerL.anchor = vinilControllerL.anchor;
            vinilActiveControllerL.rotation = vinilControllerL.rotation;
            vinilActiveControllerL.visible = false;
            vinilActiveControllerR = Image.Image_createWithResIDQuad(103, 4);
            vinilActiveControllerR.anchor = vinilControllerR.anchor;
            vinilActiveControllerR.rotation = vinilControllerR.rotation;
            vinilActiveControllerR.visible = false;
            vinilBL = Image.Image_createWithResIDQuad(103, 0);
            vinilBL.anchor = 12;
            vinilBR = Image.Image_createWithResIDQuad(103, 0);
            vinilBR.scaleX = -1f;
            vinilBR.anchor = 9;
            vinilTL = Image.Image_createWithResIDQuad(103, 0);
            vinilTL.scaleY = -1f;
            vinilTL.anchor = 36;
            vinilTR = Image.Image_createWithResIDQuad(103, 0);
            vinilTR.scaleX = vinilTR.scaleY = -1f;
            vinilTR.anchor = 33;
            passColorToChilds = false;
            _ = AddChild(vinilActiveControllerL);
            _ = AddChild(vinilActiveControllerR);
            _ = AddChild(vinilControllerL);
            _ = AddChild(vinilControllerR);
        }

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
            rotatedCircle.handle1 = VectRotateAround(rotatedCircle.handle1, (double)DEGREES_TO_RADIANS(rotatedCircle.rotation), rotatedCircle.x, rotatedCircle.y);
            rotatedCircle.handle2 = VectRotateAround(rotatedCircle.handle2, (double)DEGREES_TO_RADIANS(rotatedCircle.rotation), rotatedCircle.x, rotatedCircle.y);
            rotatedCircle.SetSize(size);
            rotatedCircle.SetHasOneHandle(HasOneHandle());
            rotatedCircle.vinilControllerL.visible = false;
            rotatedCircle.vinilControllerR.visible = false;
            return rotatedCircle;
        }

        public override void Draw()
        {
            if (IsRightControllerActive() || IsLeftControllerActive())
            {
                OpenGL.GlDisable(0);
                OpenGL.GlBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
                GLDrawer.DrawAntialiasedCurve2(x, y, sizeInPixels + (3f * Math.Abs(vinilTR.scaleX)), 0f, 6.2831855f, 51, 2f, 1f * Math.Abs(vinilTR.scaleX), RGBAColor.whiteRGBA);
            }
            OpenGL.GlEnable(0);
            OpenGL.GlBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
            vinilTL.color = vinilTR.color = vinilBL.color = vinilBR.color = RGBAColor.solidOpaqueRGBA;
            vinilTL.Draw();
            vinilTR.Draw();
            vinilBL.Draw();
            vinilBR.Draw();
            OpenGL.GlDisable(0);
            OpenGL.GlBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONEMINUSSRCALPHA);
            if (IsRightControllerActive() || IsLeftControllerActive() || color.a < 1.0)
            {
                RGBAColor whiteRGBA = RGBAColor.whiteRGBA;
                whiteRGBA.a = 1f - color.a;
                GLDrawer.DrawAntialiasedCurve2(x, y, sizeInPixels + 1f, 0f, 6.2831855f, 51, 2f, 1f * Math.Abs(vinilTR.scaleX), whiteRGBA);
            }
            for (int i = 0; i < circlesArray.Count; i++)
            {
                RotatedCircle2 rotatedCircle = circlesArray[i];
                if (rotatedCircle != this && rotatedCircle.ContainsSameObjectWithAnotherCircle() && circlesArray.GetObjectIndex(rotatedCircle) < circlesArray.GetObjectIndex(this))
                {
                    GLDrawer.DrawCircleIntersection(x, y, sizeInPixels, rotatedCircle.x, rotatedCircle.y, rotatedCircle.sizeInPixels, 51, 7f * rotatedCircle.vinilHighlightL.scaleX * 0.5f, CONTOUR_COLOR);
                }
            }
            OpenGL.GlEnable(0);
            OpenGL.GlBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
            OpenGL.GlColor4f(Color.White);
            OpenGL.GlEnable(0);
            vinilHighlightL.color = color;
            vinilHighlightR.color = color;
            vinilHighlightL.Draw();
            vinilHighlightR.Draw();
            vinilStickerL.x = vinilStickerR.x = x;
            vinilStickerL.y = vinilStickerR.y = y;
            vinilStickerL.rotation = vinilStickerR.rotation = rotation;
            vinilStickerL.Draw();
            vinilStickerR.Draw();
            OpenGL.GlDisable(0);
            OpenGL.GlBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
            GLDrawer.DrawAntialiasedCurve2(x, y, vinilStickerL.width * vinilStickerL.scaleX, 0f, 6.2831855f, 51, 1f, vinilStickerL.scaleX * 1.5f, INNER_CIRCLE_COLOR1);
            GLDrawer.DrawAntialiasedCurve2(x, y, (vinilStickerL.width - 2) * vinilStickerL.scaleX, 0f, 6.2831855f, 51, 0f, vinilStickerL.scaleX * 1f, INNER_CIRCLE_COLOR2);
            OpenGL.GlColor4f(Color.White);
            OpenGL.GlEnable(0);
            vinilControllerL.color = color;
            vinilControllerR.color = color;
            base.Draw();
            vinilCenter.Draw();
        }

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

        public void UpdateChildPositions()
        {
            vinilCenter.x = x;
            vinilCenter.y = y;
            float num = vinilHighlightL.width / 2 * (1f - vinilHighlightL.scaleX);
            float num2 = vinilHighlightL.height / 2 * (1f - vinilHighlightL.scaleY);
            float num3 = (vinilBL.width + 4) / 2f * (1f - vinilBL.scaleX);
            float num4 = (vinilBL.height + 4) / 2f * (1f - vinilBL.scaleY);
            float num5 = Math.Abs(vinilControllerR.scaleX) < 1f ? (1f - Math.Abs(vinilControllerR.scaleX)) * 10f : 0f;
            float num6 = Math.Abs(vinilTL.scaleX) < 0.45f ? ((0.45f - Math.Abs(vinilTL.scaleX)) * 10f) + 1f : 0f;
            float num7 = Math.Abs(vinilBL.height * vinilBL.scaleY) - Math.Abs(vinilControllerR.height * 0.58f * vinilControllerR.scaleY / 2f) - num5 - num6;
            vinilHighlightL.x = x + num;
            vinilHighlightR.x = x - num;
            vinilHighlightL.y = vinilHighlightR.y = y - num2;
            vinilBL.x = vinilTL.x = x + num3;
            vinilBL.y = vinilBR.y = y - num4;
            vinilBR.x = vinilTR.x = x - num3;
            vinilTL.y = vinilTR.y = y + num4;
            vinilControllerL.x = x - num7;
            vinilControllerR.x = x + num7;
            vinilControllerL.y = vinilControllerR.y = y;
            vinilActiveControllerL.x = vinilControllerL.x;
            vinilActiveControllerL.y = vinilControllerL.y;
            vinilActiveControllerR.x = vinilControllerR.x;
            vinilActiveControllerR.y = vinilControllerR.y;
        }

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

        private RGBAColor CIRCLE_COLOR1 = RGBAColor.MakeRGBA(0.306, 0.298, 0.454, 1.0);

        private RGBAColor CIRCLE_COLOR2 = RGBAColor.MakeRGBA(0.239, 0.231, 0.356, 1.0);

        private RGBAColor CIRCLE_COLOR3 = RGBAColor.MakeRGBA(0.29, 0.286, 0.419, 1.0);

        private RGBAColor INNER_CIRCLE_COLOR1 = RGBAColor.MakeRGBA(0.6901960784313725, 0.4196078431372549, 0.07450980392156863, 1.0);

        private RGBAColor INNER_CIRCLE_COLOR2 = RGBAColor.MakeRGBA(0.9294117647058824, 0.611764705882353, 0.07450980392156863, 1.0);

        private RGBAColor CONTOUR_COLOR = RGBAColor.MakeRGBA(1.0, 1.0, 1.0, 0.2);

        public float size;

        public float sizeInPixels;

        public int operating;

        public int soundPlaying;

        public Vector lastTouch;

        public Vector handle1;

        public Vector handle2;

        public Vector inithanlde1;

        public Vector inithanlde2;

        public DynamicArray<RotatedCircle2> circlesArray;

        public List<BaseElement> containedObjects;

        public bool removeOnNextUpdate;

        private Image vinilStickerL;

        private Image vinilStickerR;

        private Image vinilHighlightL;

        private Image vinilHighlightR;

        private readonly Image vinilControllerL;

        private readonly Image vinilControllerR;

        private readonly Image vinilActiveControllerL;

        private readonly Image vinilActiveControllerR;

        private Image vinilCenter;

        private Image vinilTL;

        private Image vinilTR;

        private Image vinilBL;

        private Image vinilBR;
    }
}
