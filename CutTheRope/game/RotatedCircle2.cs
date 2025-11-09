using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.visual;
using CutTheRope.ios;
using CutTheRope.windows;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace CutTheRope.game
{
    internal class RotatedCircle2 : BaseElement
    {
        public virtual void setSize(float value)
        {
            size = value;
            float num = size / (vinilTL.width + vinilTR.width * (1f - vinilTL.scaleX));
            vinilHighlightL.scaleX = vinilHighlightL.scaleY = vinilHighlightR.scaleY = num;
            vinilHighlightR.scaleX = 0f - num;
            vinilBL.scaleX = vinilBL.scaleY = vinilBR.scaleY = num;
            vinilBR.scaleX = 0f - num;
            vinilTL.scaleX = num;
            vinilTL.scaleY = 0f - num;
            vinilTR.scaleX = vinilTR.scaleY = 0f - num;
            float num2 = (num >= 0.4f) ? num : 0.4f;
            vinilStickerL.scaleX = vinilStickerL.scaleY = vinilStickerR.scaleY = num2;
            vinilStickerR.scaleX = 0f - num2;
            float num3 = (num >= 0.75f) ? num : 0.75f;
            vinilControllerL.scaleX = vinilControllerL.scaleY = vinilControllerR.scaleX = vinilControllerR.scaleY = num3;
            vinilActiveControllerL.scaleX = vinilActiveControllerL.scaleY = vinilActiveControllerR.scaleX = vinilActiveControllerR.scaleY = num3;
            vinilCenter.scaleX = 1f - (1f - vinilStickerL.scaleX) * 0.5f;
            vinilCenter.scaleY = vinilCenter.scaleX;
            sizeInPixels = vinilHighlightL.width * vinilHighlightL.scaleX;
            updateChildPositions();
        }

        public virtual void setHasOneHandle(bool value)
        {
            vinilControllerL.visible = !value;
        }

        public virtual bool hasOneHandle()
        {
            return !vinilControllerL.visible;
        }

        public virtual void setIsLeftControllerActive(bool value)
        {
            vinilActiveControllerL.visible = value;
        }

        public virtual bool isLeftControllerActive()
        {
            return vinilActiveControllerL.visible;
        }

        public virtual void setIsRightControllerActive(bool value)
        {
            vinilActiveControllerR.visible = value;
        }

        public virtual bool isRightControllerActive()
        {
            return vinilActiveControllerR.visible;
        }

        public virtual bool containsSameObjectWithAnotherCircle()
        {
            for (int i = 0; i < circlesArray.count(); i++)
            {
                RotatedCircle2 rotatedCircle = (RotatedCircle2)circlesArray[i];
                if (rotatedCircle != this && containsSameObjectWithCircle(rotatedCircle))
                {
                    return true;
                }
            }
            return false;
        }

        public override NSObject init()
        {
            if (base.init() != null)
            {
                circlesArray = null;
                containedObjects = new List<BaseElement>();
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
                addChild(vinilActiveControllerL);
                addChild(vinilActiveControllerR);
                addChild(vinilControllerL);
                addChild(vinilControllerR);
            }
            return this;
        }

        public virtual NSObject copy()
        {
            RotatedCircle2 rotatedCircle = (RotatedCircle2)new RotatedCircle2().init();
            rotatedCircle.x = x;
            rotatedCircle.y = y;
            rotatedCircle.rotation = rotation;
            rotatedCircle.circlesArray = circlesArray;
            rotatedCircle.containedObjects = containedObjects;
            rotatedCircle.operating = -1;
            rotatedCircle.handle1 = new Vector(rotatedCircle.x - size, rotatedCircle.y);
            rotatedCircle.handle2 = new Vector(rotatedCircle.x + size, rotatedCircle.y);
            rotatedCircle.handle1 = vectRotateAround(rotatedCircle.handle1, (double)DEGREES_TO_RADIANS(rotatedCircle.rotation), rotatedCircle.x, rotatedCircle.y);
            rotatedCircle.handle2 = vectRotateAround(rotatedCircle.handle2, (double)DEGREES_TO_RADIANS(rotatedCircle.rotation), rotatedCircle.x, rotatedCircle.y);
            rotatedCircle.setSize(size);
            rotatedCircle.setHasOneHandle(hasOneHandle());
            rotatedCircle.vinilControllerL.visible = false;
            rotatedCircle.vinilControllerR.visible = false;
            return rotatedCircle;
        }

        public override void draw()
        {
            if (isRightControllerActive() || isLeftControllerActive())
            {
                OpenGL.glDisable(0);
                OpenGL.glBlendFunc(BlendingFactor.GL_ONE, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
                GLDrawer.drawAntialiasedCurve2(x, y, sizeInPixels + 3f * Math.Abs(vinilTR.scaleX), 0f, 6.2831855f, 51, 2f, 1f * Math.Abs(vinilTR.scaleX), RGBAColor.whiteRGBA);
            }
            OpenGL.glEnable(0);
            OpenGL.glBlendFunc(BlendingFactor.GL_ONE, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
            vinilTL.color = vinilTR.color = vinilBL.color = vinilBR.color = RGBAColor.solidOpaqueRGBA;
            vinilTL.draw();
            vinilTR.draw();
            vinilBL.draw();
            vinilBR.draw();
            OpenGL.glDisable(0);
            OpenGL.glBlendFunc(BlendingFactor.GL_SRC_ALPHA, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
            if (isRightControllerActive() || isLeftControllerActive() || color.a < 1.0)
            {
                RGBAColor whiteRGBA = RGBAColor.whiteRGBA;
                whiteRGBA.a = 1f - color.a;
                GLDrawer.drawAntialiasedCurve2(x, y, sizeInPixels + 1f, 0f, 6.2831855f, 51, 2f, 1f * Math.Abs(vinilTR.scaleX), whiteRGBA);
            }
            for (int i = 0; i < circlesArray.count(); i++)
            {
                RotatedCircle2 rotatedCircle = (RotatedCircle2)circlesArray[i];
                if (rotatedCircle != this && rotatedCircle.containsSameObjectWithAnotherCircle() && circlesArray.getObjectIndex(rotatedCircle) < circlesArray.getObjectIndex(this))
                {
                    GLDrawer.drawCircleIntersection(x, y, sizeInPixels, rotatedCircle.x, rotatedCircle.y, rotatedCircle.sizeInPixels, 51, 7f * rotatedCircle.vinilHighlightL.scaleX * 0.5f, CONTOUR_COLOR);
                }
            }
            OpenGL.glEnable(0);
            OpenGL.glBlendFunc(BlendingFactor.GL_ONE, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
            OpenGL.glColor4f(Color.White);
            OpenGL.glEnable(0);
            vinilHighlightL.color = color;
            vinilHighlightR.color = color;
            vinilHighlightL.draw();
            vinilHighlightR.draw();
            vinilStickerL.x = vinilStickerR.x = x;
            vinilStickerL.y = vinilStickerR.y = y;
            vinilStickerL.rotation = vinilStickerR.rotation = rotation;
            vinilStickerL.draw();
            vinilStickerR.draw();
            OpenGL.glDisable(0);
            OpenGL.glBlendFunc(BlendingFactor.GL_ONE, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
            GLDrawer.drawAntialiasedCurve2(x, y, vinilStickerL.width * vinilStickerL.scaleX, 0f, 6.2831855f, 51, 1f, vinilStickerL.scaleX * 1.5f, INNER_CIRCLE_COLOR1);
            GLDrawer.drawAntialiasedCurve2(x, y, (vinilStickerL.width - 2) * vinilStickerL.scaleX, 0f, 6.2831855f, 51, 0f, vinilStickerL.scaleX * 1f, INNER_CIRCLE_COLOR2);
            OpenGL.glColor4f(Color.White);
            OpenGL.glEnable(0);
            vinilControllerL.color = color;
            vinilControllerR.color = color;
            base.draw();
            vinilCenter.draw();
        }

        public override void dealloc()
        {
            vinilCenter = null;
            vinilHighlightL = null;
            vinilHighlightR = null;
            vinilBL = null;
            vinilBR = null;
            vinilTL = null;
            vinilTR = null;
            vinilStickerL = null;
            vinilStickerR = null;
            containedObjects.Clear();
            containedObjects = null;
            base.dealloc();
        }

        public virtual void updateChildPositions()
        {
            vinilCenter.x = x;
            vinilCenter.y = y;
            float num = vinilHighlightL.width / 2 * (1f - vinilHighlightL.scaleX);
            float num2 = vinilHighlightL.height / 2 * (1f - vinilHighlightL.scaleY);
            float num3 = (vinilBL.width + 4) / 2f * (1f - vinilBL.scaleX);
            float num4 = (vinilBL.height + 4) / 2f * (1f - vinilBL.scaleY);
            float num5 = (Math.Abs(vinilControllerR.scaleX) < 1f) ? ((1f - Math.Abs(vinilControllerR.scaleX)) * 10f) : 0f;
            float num6 = (Math.Abs(vinilTL.scaleX) < 0.45f) ? ((0.45f - Math.Abs(vinilTL.scaleX)) * 10f + 1f) : 0f;
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

        public virtual bool containsSameObjectWithCircle(RotatedCircle2 anotherCircle)
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

        private const float CONTROLLER_MIN_SCALE = 0.75f;

        private const float STICKER_MIN_SCALE = 0.4f;

        private const float CENTER_SCALE_FACTOR = 0.5f;

        private const float HUNDRED_PERCENT_SCALE_SIZE = 160f;

        private const int CIRCLE_VERTEX_COUNT = 50;

        private const float INNER_CIRCLE_WIDTH = 15f;

        private const float OUTER_CIRCLE_WIDTH = 7f;

        private const float ACTIVE_CIRCLE_WIDTH = 2f;

        private const float CONTROLLER_RATIO_PARAM = 0.58f;

        private const float CIRCLE_CONTROLLER_OFFS = 10f;

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

        public DynamicArray circlesArray;

        public List<BaseElement> containedObjects;

        public bool removeOnNextUpdate;

        private Image vinilStickerL;

        private Image vinilStickerR;

        private Image vinilHighlightL;

        private Image vinilHighlightR;

        private Image vinilControllerL;

        private Image vinilControllerR;

        private Image vinilActiveControllerL;

        private Image vinilActiveControllerR;

        private Image vinilCenter;

        private Image vinilTL;

        private Image vinilTR;

        private Image vinilBL;

        private Image vinilBR;
    }
}
