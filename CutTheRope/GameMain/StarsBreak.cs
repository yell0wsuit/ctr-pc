using CutTheRope.Desktop;
using CutTheRope.Framework.Visual;

using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.GameMain
{
    internal sealed class StarsBreak : RotateableMultiParticles
    {
        public override Particles InitWithTotalParticlesandImageGrid(int p, Image grid)
        {
            if (base.InitWithTotalParticlesandImageGrid(p, grid) == null)
            {
                return null;
            }
            duration = 2f;
            gravity.x = 0f;
            gravity.y = 200f;
            angle = -90f;
            angleVar = 50f;
            speed = 150f;
            speedVar = 70f;
            radialAccel = 0f;
            radialAccelVar = 1f;
            tangentialAccel = 0f;
            tangentialAccelVar = 1f;
            x = SCREEN_WIDTH / 2f;
            y = SCREEN_HEIGHT / 2f;
            posVar.x = SCREEN_WIDTH / 2f;
            posVar.y = SCREEN_HEIGHT / 2f;
            life = 4f;
            lifeVar = 0f;
            size = 1f;
            sizeVar = 0f;
            emissionRate = 100f;
            startColor.RedColor = 1f;
            startColor.GreenColor = 1f;
            startColor.BlueColor = 1f;
            startColor.AlphaChannel = 1f;
            startColorVar.RedColor = 0f;
            startColorVar.GreenColor = 0f;
            startColorVar.BlueColor = 0f;
            startColorVar.AlphaChannel = 0f;
            endColor.RedColor = 1f;
            endColor.GreenColor = 1f;
            endColor.BlueColor = 1f;
            endColor.AlphaChannel = 1f;
            endColorVar.RedColor = 0f;
            endColorVar.GreenColor = 0f;
            endColorVar.BlueColor = 0f;
            endColorVar.AlphaChannel = 0f;
            rotateSpeed = 0f;
            rotateSpeedVar = 600f;
            blendAdditive = true;
            return this;
        }

        public override void Draw()
        {
            PreDraw();
            OpenGL.GlBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONE);
            OpenGL.GlEnable(OpenGL.GL_TEXTURE_2D);
            OpenGL.GlBindTexture(drawer.image.texture.Name());
            int quadCount = particleIdx;
            if (quadCount > 0)
            {
                VertexPositionColorTexture[] vertexBuffer = GetVertexBuffer(quadCount * 4);
                OpenGL.FillTexturedColoredVertices(drawer.vertices, drawer.texCoordinates, colors, vertexBuffer, quadCount);
                OpenGL.DrawTriangleList(vertexBuffer, drawer.indices, quadCount * 6);
            }
            PostDraw();
        }

        private VertexPositionColorTexture[] verticesCache;

        private VertexPositionColorTexture[] GetVertexBuffer(int vertexCount)
        {
            if (verticesCache == null || verticesCache.Length < vertexCount)
            {
                verticesCache = new VertexPositionColorTexture[vertexCount];
            }
            return verticesCache;
        }
    }
}
