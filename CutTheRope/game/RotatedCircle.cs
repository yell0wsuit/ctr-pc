using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.visual;
using CutTheRope.ios;
using CutTheRope.windows;
using Microsoft.Xna.Framework;
using System;

namespace CutTheRope.game
{
    internal class RotatedCircle : BaseElement
    {
        public override NSObject init()
        {
            if (base.init() != null)
            {
                containedObjects = (DynamicArray)new DynamicArray().init();
                soundPlaying = -1;
                vinilStickerL = Image.Image_createWithResIDQuad(103, 2);
                vinilStickerL.anchor = 20;
                vinilStickerL.parentAnchor = 18;
                vinilStickerL.rotationCenterX = vinilStickerL.width / 2f;
                vinilStickerR = Image.Image_createWithResIDQuad(103, 2);
                vinilStickerR.scaleX = -1f;
                vinilStickerR.anchor = 20;
                vinilStickerR.parentAnchor = 18;
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
                vinil = Image.Image_createWithResIDQuad(103, 0);
                vinil.anchor = 18;
                passColorToChilds = false;
                addChild(vinilStickerL);
                addChild(vinilStickerR);
                addChild(vinilActiveControllerL);
                addChild(vinilActiveControllerR);
                addChild(vinilControllerL);
                addChild(vinilControllerR);
            }
            return this;
        }

        public virtual void setSize(float value)
        {
            size = value;
            float num = size / 167f;
            vinilHighlightL.scaleX = vinilHighlightL.scaleY = vinilHighlightR.scaleY = num;
            vinilHighlightR.scaleX = 0f - num;
            vinil.scaleX = vinil.scaleY = num;
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

        public virtual bool hasOneHandle()
        {
            return !vinilControllerL.visible;
        }

        public virtual void setHasOneHandle(bool value)
        {
            vinilControllerL.visible = !value;
        }

        public virtual bool isLeftControllerActive()
        {
            return vinilActiveControllerL.visible;
        }

        public virtual void setIsLeftControllerActive(bool value)
        {
            vinilActiveControllerL.visible = value;
        }

        public virtual bool isRightControllerActive()
        {
            return vinilActiveControllerR.visible;
        }

        public virtual void setIsRightControllerActive(bool value)
        {
            vinilActiveControllerR.visible = value;
        }

        public virtual bool containsSameObjectWithAnotherCircle()
        {
            foreach (object obj in circlesArray)
            {
                RotatedCircle item = (RotatedCircle)obj;
                if (item != this && containsSameObjectWithCircle(item))
                {
                    return true;
                }
            }
            return false;
        }

        public override void draw()
        {
            if (isRightControllerActive() || isLeftControllerActive())
            {
                OpenGL.glDisable(0);
                OpenGL.glBlendFunc(BlendingFactor.GL_ONE, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
                RGBAColor whiteRGBA = RGBAColor.whiteRGBA;
                if (color.a != 1.0)
                {
                    whiteRGBA.a = color.a;
                }
                GLDrawer.drawAntialiasedCurve2(x, y, sizeInPixels + ACTIVE_CIRCLE_WIDTH * vinilControllerL.scaleX, 0f, 6.2831855f, 81, (ACTIVE_CIRCLE_WIDTH + RTPD(1.0) * 3f) * vinilControllerL.scaleX, 5f, whiteRGBA);
                OpenGL.glColor4f(Color.White);
                OpenGL.glEnable(0);
            }
            vinilHighlightL.color = color;
            vinilHighlightR.color = color;
            vinilControllerL.color = color;
            vinilControllerR.color = color;
            vinil.color = color;
            vinil.draw();
            OpenGL.glDisable(0);
            OpenGL.glBlendFunc(BlendingFactor.GL_SRC_ALPHA, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
            foreach (object obj in circlesArray)
            {
                RotatedCircle item = (RotatedCircle)obj;
                if (item != this && item.containsSameObjectWithAnotherCircle() && circlesArray.getObjectIndex(item) < circlesArray.getObjectIndex(this))
                {
                    GLDrawer.drawCircleIntersection(x, y, sizeInPixels, item.x, item.y, item.sizeInPixels, 81, OUTER_CIRCLE_WIDTH * item.vinilHighlightL.scaleX * 0.5f, CONTOUR_COLOR);
                }
            }
            OpenGL.glBlendFunc(BlendingFactor.GL_ONE, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
            OpenGL.glColor4f(Color.White);
            OpenGL.glEnable(0);
            vinilHighlightL.draw();
            vinilHighlightR.draw();
            base.draw();
            vinilCenter.draw();
        }

        public virtual void updateChildPositions()
        {
            vinil.x = vinilCenter.x = x;
            vinil.y = vinilCenter.y = y;
            float num = vinilHighlightL.width / 2 * (1f - vinilHighlightL.scaleX);
            float num2 = vinilHighlightL.height / 2 * (1f - vinilHighlightL.scaleY);
            float num3 = sizeInPixels - RTPD((double)(CONTROLLER_SHIFT_PARAM1 - CONTROLLER_SHIFT_PARAM2 * size)) + (1f - vinilControllerL.scaleX) * (vinilControllerL.width / 2);
            vinilHighlightL.x = x + num;
            vinilHighlightR.x = x - num;
            vinilHighlightL.y = vinilHighlightR.y = y - num2;
            vinilControllerL.x = x - num3;
            vinilControllerR.x = x + num3;
            vinilControllerL.y = vinilControllerR.y = y;
            vinilActiveControllerL.x = vinilControllerL.x;
            vinilActiveControllerL.y = vinilControllerL.y;
            vinilActiveControllerR.x = vinilControllerR.x;
            vinilActiveControllerR.y = vinilControllerR.y;
        }

        public virtual bool containsSameObjectWithCircle(RotatedCircle anotherCircle)
        {
            if (x == anotherCircle.x && y == anotherCircle.y && size == anotherCircle.size)
            {
                return false;
            }
            foreach (object obj in containedObjects)
            {
                GameObject containedObject = (GameObject)obj;
                if (anotherCircle.containedObjects.getObjectIndex(containedObject) != -1)
                {
                    return true;
                }
            }
            return false;
        }

        public virtual NSObject copy()
        {
            RotatedCircle rotatedCircle = (RotatedCircle)new RotatedCircle().init();
            rotatedCircle.x = x;
            rotatedCircle.y = y;
            rotatedCircle.rotation = rotation;
            rotatedCircle.circlesArray = circlesArray;
            rotatedCircle.containedObjects = containedObjects;
            rotatedCircle.operating = -1;
            rotatedCircle.handle1 = vect(rotatedCircle.x - RTPD((double)(size * 3f)), rotatedCircle.y);
            rotatedCircle.handle2 = vect(rotatedCircle.x + RTPD((double)(size * 3f)), rotatedCircle.y);
            rotatedCircle.handle1 = vectRotateAround(rotatedCircle.handle1, (double)DEGREES_TO_RADIANS(rotatedCircle.rotation), rotatedCircle.x, rotatedCircle.y);
            rotatedCircle.handle2 = vectRotateAround(rotatedCircle.handle2, (double)DEGREES_TO_RADIANS(rotatedCircle.rotation), rotatedCircle.x, rotatedCircle.y);
            rotatedCircle.setSize(size);
            rotatedCircle.setHasOneHandle(hasOneHandle_);
            rotatedCircle.vinilControllerL.visible = false;
            rotatedCircle.vinilControllerR.visible = false;
            return rotatedCircle;
        }

        public override void dealloc()
        {
            vinilCenter.release();
            vinilHighlightL.release();
            vinilHighlightR.release();
            vinil.release();
            containedObjects.release();
            base.dealloc();
        }

        public const int PM = 3;

        public const float CONTROLLER_MIN_SCALE = 0.75f;

        public const float STICKER_MIN_SCALE = 0.4f;

        public const float CENTER_SCALE_FACTOR = 0.5f;

        public const float HUNDRED_PERCENT_SCALE_SIZE = 167f;

        public const int CIRCLE_VERTEX_COUNT = 80;

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

        public DynamicArray containedObjects;

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

        private Image vinil;

        private bool hasOneHandle_;

        private RGBAColor CONTOUR_COLOR = RGBAColor.MakeRGBA(1.0, 1.0, 1.0, 0.2);

        private float INNER_CIRCLE_WIDTH = RTPD(15.0) * 3f;

        private float OUTER_CIRCLE_WIDTH = RTPD(7.0) * 3f;

        private float ACTIVE_CIRCLE_WIDTH = RTPD(3.0) * 3f;

        private float CONTROLLER_SHIFT_PARAM1 = 67.5f;

        private float CONTROLLER_SHIFT_PARAM2 = 0.089999996f;
    }
}
