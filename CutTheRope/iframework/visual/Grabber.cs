using CutTheRope.desktop;

namespace CutTheRope.iframework.visual
{
    internal sealed class Grabber : FrameworkTypes
    {
        public static CTRTexture2D Grab()
        {
            return new CTRTexture2D().InitFromPixels(0, 0, (int)SCREEN_WIDTH, (int)SCREEN_HEIGHT);
        }

        public static void DrawGrabbedImage(CTRTexture2D t, int x, int y)
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
                OpenGL.GlEnable(0);
                OpenGL.GlBindTexture(t.Name());
                OpenGL.GlVertexPointer(3, 5, 0, pointer2);
                OpenGL.GlTexCoordPointer(2, 5, 0, pointer);
                OpenGL.GlDrawArrays(8, 0, 4);
            }
        }
    }
}
