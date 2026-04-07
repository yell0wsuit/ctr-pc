using CutTheRope.Desktop;
using CutTheRope.Framework.Core;

using Microsoft.Xna.Framework;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Base menu view that applies standard menu rendering state.
    /// </summary>
    internal class MenuView : View
    {
        /// <inheritdoc />
        public override void Update(float t)
        {
            Global.MouseCursor.Enable(true);
            base.Update(t);
        }

        /// <inheritdoc />
        public override void Draw()
        {
            Renderer.SetColor(Color.White);
            Renderer.Enable(Renderer.GL_TEXTURE_2D);
            Renderer.Enable(Renderer.GL_BLEND);
            Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
            base.PreDraw();
            base.PostDraw();
            Renderer.Disable(Renderer.GL_TEXTURE_2D);
            Renderer.Disable(Renderer.GL_BLEND);
        }
    }
}
