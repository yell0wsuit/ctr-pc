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
                this.containedObjects = (DynamicArray)new DynamicArray().init();
                this.soundPlaying = -1;
                this.vinilStickerL = Image.Image_createWithResIDQuad(103, 2);
                this.vinilStickerL.anchor = 20;
                this.vinilStickerL.parentAnchor = 18;
                this.vinilStickerL.rotationCenterX = (float)this.vinilStickerL.width / 2f;
                this.vinilStickerR = Image.Image_createWithResIDQuad(103, 2);
                this.vinilStickerR.scaleX = -1f;
                this.vinilStickerR.anchor = 20;
                this.vinilStickerR.parentAnchor = 18;
                this.vinilStickerR.rotationCenterX = (float)this.vinilStickerR.width / 2f;
                this.vinilCenter = Image.Image_createWithResIDQuad(103, 3);
                this.vinilCenter.anchor = 18;
                this.vinilHighlightL = Image.Image_createWithResIDQuad(103, 1);
                this.vinilHighlightL.anchor = 12;
                this.vinilHighlightR = Image.Image_createWithResIDQuad(103, 1);
                this.vinilHighlightR.scaleX = -1f;
                this.vinilHighlightR.anchor = 9;
                this.vinilControllerL = Image.Image_createWithResIDQuad(103, 5);
                this.vinilControllerL.anchor = 18;
                this.vinilControllerL.rotation = 90f;
                this.vinilControllerR = Image.Image_createWithResIDQuad(103, 5);
                this.vinilControllerR.anchor = 18;
                this.vinilControllerR.rotation = -90f;
                this.vinilActiveControllerL = Image.Image_createWithResIDQuad(103, 4);
                this.vinilActiveControllerL.anchor = this.vinilControllerL.anchor;
                this.vinilActiveControllerL.rotation = this.vinilControllerL.rotation;
                this.vinilActiveControllerL.visible = false;
                this.vinilActiveControllerR = Image.Image_createWithResIDQuad(103, 4);
                this.vinilActiveControllerR.anchor = this.vinilControllerR.anchor;
                this.vinilActiveControllerR.rotation = this.vinilControllerR.rotation;
                this.vinilActiveControllerR.visible = false;
                this.vinil = Image.Image_createWithResIDQuad(103, 0);
                this.vinil.anchor = 18;
                this.passColorToChilds = false;
                this.addChild(this.vinilStickerL);
                this.addChild(this.vinilStickerR);
                this.addChild(this.vinilActiveControllerL);
                this.addChild(this.vinilActiveControllerR);
                this.addChild(this.vinilControllerL);
                this.addChild(this.vinilControllerR);
            }
            return this;
        }

        public virtual void setSize(float value)
        {
            this.size = value;
            float num = this.size / 167f;
            this.vinilHighlightL.scaleX = this.vinilHighlightL.scaleY = this.vinilHighlightR.scaleY = num;
            this.vinilHighlightR.scaleX = 0f - num;
            this.vinil.scaleX = this.vinil.scaleY = num;
            float num2 = (num >= 0.4f) ? num : 0.4f;
            this.vinilStickerL.scaleX = this.vinilStickerL.scaleY = this.vinilStickerR.scaleY = num2;
            this.vinilStickerR.scaleX = 0f - num2;
            float num3 = (num >= 0.75f) ? num : 0.75f;
            this.vinilControllerL.scaleX = this.vinilControllerL.scaleY = this.vinilControllerR.scaleX = this.vinilControllerR.scaleY = num3;
            this.vinilActiveControllerL.scaleX = this.vinilActiveControllerL.scaleY = this.vinilActiveControllerR.scaleX = this.vinilActiveControllerR.scaleY = num3;
            this.vinilCenter.scaleX = 1f - (1f - this.vinilStickerL.scaleX) * 0.5f;
            this.vinilCenter.scaleY = this.vinilCenter.scaleX;
            this.sizeInPixels = (float)this.vinilHighlightL.width * this.vinilHighlightL.scaleX;
            this.updateChildPositions();
        }

        public virtual bool hasOneHandle()
        {
            return !this.vinilControllerL.visible;
        }

        public virtual void setHasOneHandle(bool value)
        {
            this.vinilControllerL.visible = !value;
        }

        public virtual bool isLeftControllerActive()
        {
            return this.vinilActiveControllerL.visible;
        }

        public virtual void setIsLeftControllerActive(bool value)
        {
            this.vinilActiveControllerL.visible = value;
        }

        public virtual bool isRightControllerActive()
        {
            return this.vinilActiveControllerR.visible;
        }

        public virtual void setIsRightControllerActive(bool value)
        {
            this.vinilActiveControllerR.visible = value;
        }

        public virtual bool containsSameObjectWithAnotherCircle()
        {
            foreach (object obj in this.circlesArray)
            {
                RotatedCircle item = (RotatedCircle)obj;
                if (item != this && this.containsSameObjectWithCircle(item))
                {
                    return true;
                }
            }
            return false;
        }

        public override void draw()
        {
            if (this.isRightControllerActive() || this.isLeftControllerActive())
            {
                OpenGL.glDisable(0);
                OpenGL.glBlendFunc(BlendingFactor.GL_ONE, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
                RGBAColor whiteRGBA = RGBAColor.whiteRGBA;
                if ((double)this.color.a != 1.0)
                {
                    whiteRGBA.a = this.color.a;
                }
                GLDrawer.drawAntialiasedCurve2(this.x, this.y, this.sizeInPixels + this.ACTIVE_CIRCLE_WIDTH * this.vinilControllerL.scaleX, 0f, 6.2831855f, 81, (this.ACTIVE_CIRCLE_WIDTH + FrameworkTypes.RTPD(1.0) * 3f) * this.vinilControllerL.scaleX, 5f, whiteRGBA);
                OpenGL.glColor4f(Color.White);
                OpenGL.glEnable(0);
            }
            this.vinilHighlightL.color = this.color;
            this.vinilHighlightR.color = this.color;
            this.vinilControllerL.color = this.color;
            this.vinilControllerR.color = this.color;
            this.vinil.color = this.color;
            this.vinil.draw();
            OpenGL.glDisable(0);
            OpenGL.glBlendFunc(BlendingFactor.GL_SRC_ALPHA, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
            foreach (object obj in this.circlesArray)
            {
                RotatedCircle item = (RotatedCircle)obj;
                if (item != this && item.containsSameObjectWithAnotherCircle() && this.circlesArray.getObjectIndex(item) < this.circlesArray.getObjectIndex(this))
                {
                    GLDrawer.drawCircleIntersection(this.x, this.y, this.sizeInPixels, item.x, item.y, item.sizeInPixels, 81, this.OUTER_CIRCLE_WIDTH * item.vinilHighlightL.scaleX * 0.5f, this.CONTOUR_COLOR);
                }
            }
            OpenGL.glBlendFunc(BlendingFactor.GL_ONE, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
            OpenGL.glColor4f(Color.White);
            OpenGL.glEnable(0);
            this.vinilHighlightL.draw();
            this.vinilHighlightR.draw();
            base.draw();
            this.vinilCenter.draw();
        }

        public virtual void updateChildPositions()
        {
            this.vinil.x = this.vinilCenter.x = this.x;
            this.vinil.y = this.vinilCenter.y = this.y;
            float num = (float)(this.vinilHighlightL.width / 2) * (1f - this.vinilHighlightL.scaleX);
            float num2 = (float)(this.vinilHighlightL.height / 2) * (1f - this.vinilHighlightL.scaleY);
            float num3 = this.sizeInPixels - FrameworkTypes.RTPD((double)(this.CONTROLLER_SHIFT_PARAM1 - this.CONTROLLER_SHIFT_PARAM2 * this.size)) + (1f - this.vinilControllerL.scaleX) * (float)(this.vinilControllerL.width / 2);
            this.vinilHighlightL.x = this.x + num;
            this.vinilHighlightR.x = this.x - num;
            this.vinilHighlightL.y = this.vinilHighlightR.y = this.y - num2;
            this.vinilControllerL.x = this.x - num3;
            this.vinilControllerR.x = this.x + num3;
            this.vinilControllerL.y = this.vinilControllerR.y = this.y;
            this.vinilActiveControllerL.x = this.vinilControllerL.x;
            this.vinilActiveControllerL.y = this.vinilControllerL.y;
            this.vinilActiveControllerR.x = this.vinilControllerR.x;
            this.vinilActiveControllerR.y = this.vinilControllerR.y;
        }

        public virtual bool containsSameObjectWithCircle(RotatedCircle anotherCircle)
        {
            if (this.x == anotherCircle.x && this.y == anotherCircle.y && this.size == anotherCircle.size)
            {
                return false;
            }
            foreach (object obj in this.containedObjects)
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
            rotatedCircle.x = this.x;
            rotatedCircle.y = this.y;
            rotatedCircle.rotation = this.rotation;
            rotatedCircle.circlesArray = this.circlesArray;
            rotatedCircle.containedObjects = this.containedObjects;
            rotatedCircle.operating = -1;
            rotatedCircle.handle1 = CTRMathHelper.vect(rotatedCircle.x - FrameworkTypes.RTPD((double)(this.size * 3f)), rotatedCircle.y);
            rotatedCircle.handle2 = CTRMathHelper.vect(rotatedCircle.x + FrameworkTypes.RTPD((double)(this.size * 3f)), rotatedCircle.y);
            rotatedCircle.handle1 = CTRMathHelper.vectRotateAround(rotatedCircle.handle1, (double)CTRMathHelper.DEGREES_TO_RADIANS(rotatedCircle.rotation), rotatedCircle.x, rotatedCircle.y);
            rotatedCircle.handle2 = CTRMathHelper.vectRotateAround(rotatedCircle.handle2, (double)CTRMathHelper.DEGREES_TO_RADIANS(rotatedCircle.rotation), rotatedCircle.x, rotatedCircle.y);
            rotatedCircle.setSize(this.size);
            rotatedCircle.setHasOneHandle(this.hasOneHandle_);
            rotatedCircle.vinilControllerL.visible = false;
            rotatedCircle.vinilControllerR.visible = false;
            return rotatedCircle;
        }

        public override void dealloc()
        {
            this.vinilCenter.release();
            this.vinilHighlightL.release();
            this.vinilHighlightR.release();
            this.vinil.release();
            this.containedObjects.release();
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

        private float INNER_CIRCLE_WIDTH = FrameworkTypes.RTPD(15.0) * 3f;

        private float OUTER_CIRCLE_WIDTH = FrameworkTypes.RTPD(7.0) * 3f;

        private float ACTIVE_CIRCLE_WIDTH = FrameworkTypes.RTPD(3.0) * 3f;

        private float CONTROLLER_SHIFT_PARAM1 = 67.5f;

        private float CONTROLLER_SHIFT_PARAM2 = 0.089999996f;
    }
}
