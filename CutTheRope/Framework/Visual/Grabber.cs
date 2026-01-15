using CutTheRope.Desktop;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.Framework.Visual
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
                VertexPositionNormalTexture[] vertices = BuildTexturedQuad(pointer2, pointer);
                OpenGL.DrawTriangleStrip(vertices);
            }
        }

        private static VertexPositionNormalTexture[] BuildTexturedQuad(float[] positions, float[] texCoords)
        {
            return
            [
                new VertexPositionNormalTexture(new Vector3(positions[0], positions[1], positions[2]), Vector3.UnitZ, new Vector2(texCoords[0], texCoords[1])),
                new VertexPositionNormalTexture(new Vector3(positions[3], positions[4], positions[5]), Vector3.UnitZ, new Vector2(texCoords[2], texCoords[3])),
                new VertexPositionNormalTexture(new Vector3(positions[6], positions[7], positions[8]), Vector3.UnitZ, new Vector2(texCoords[4], texCoords[5])),
                new VertexPositionNormalTexture(new Vector3(positions[9], positions[10], positions[11]), Vector3.UnitZ, new Vector2(texCoords[6], texCoords[7]))
            ];
        }
    }
}
