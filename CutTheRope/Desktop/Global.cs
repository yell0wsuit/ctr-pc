using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.Desktop
{
    internal sealed class Global
    {
        // (set) Token: 0x0600004C RID: 76 RVA: 0x000036CF File Offset: 0x000018CF
        public static SpriteBatch SpriteBatch { get; set; }

        // (set) Token: 0x0600004E RID: 78 RVA: 0x000036DE File Offset: 0x000018DE
        public static GraphicsDevice GraphicsDevice { get; set; }

        // (set) Token: 0x06000050 RID: 80 RVA: 0x000036ED File Offset: 0x000018ED
        public static GraphicsDeviceManager GraphicsDeviceManager { get; set; }

        // (set) Token: 0x06000052 RID: 82 RVA: 0x000036FC File Offset: 0x000018FC
        public static ScreenSizeManager ScreenSizeManager { get; set; } = new(2560, 1440);

        public static MouseCursor MouseCursor { get; } = new();

        // (set) Token: 0x06000055 RID: 85 RVA: 0x00003712 File Offset: 0x00001912
        public static Game1 XnaGame { get; set; }
    }
}
