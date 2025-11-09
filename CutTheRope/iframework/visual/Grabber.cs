using CutTheRope.ios;
using CutTheRope.windows;
using System;

namespace CutTheRope.iframework.visual
{
    internal class Grabber : NSObject
    {
        public override NSObject init()
        {
            base.init();
            return this;
        }

        public override void dealloc()
        {
            base.dealloc();
        }

        public virtual CTRTexture2D grab()
        {
            return (CTRTexture2D)new CTRTexture2D().initFromPixels(0, 0, (int)SCREEN_WIDTH, (int)SCREEN_HEIGHT);
        }

        public static void drawGrabbedImage(CTRTexture2D t, int x, int y)
        {
            if (t != null)
            {
                float[] pointer = [0f, 0f, t._maxS, 0f, 0f, t._maxT, t._maxS, t._maxT];
                float[] array = new float[12];
                array[0] = x;
                array[1] = y;
                array[3] = t._realWidth + x;
                array[4] = y;
                array[6] = x;
                array[7] = t._realHeight + y;
                array[9] = t._realWidth + x;
                array[10] = t._realHeight + y;
                float[] pointer2 = array;
                OpenGL.glEnable(0);
                OpenGL.glBindTexture(t.name());
                OpenGL.glVertexPointer(3, 5, 0, pointer2);
                OpenGL.glTexCoordPointer(2, 5, 0, pointer);
                OpenGL.glDrawArrays(8, 0, 4);
            }
        }
    }
}
