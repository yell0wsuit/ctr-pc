using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.Desktop
{
    internal sealed class Global
    {
        // (get) Token: 0x0600004B RID: 75 RVA: 0x000036C8 File Offset: 0x000018C8
        // (set) Token: 0x0600004C RID: 76 RVA: 0x000036CF File Offset: 0x000018CF
        public static SpriteBatch SpriteBatch { get; set; }

        // (get) Token: 0x0600004D RID: 77 RVA: 0x000036D7 File Offset: 0x000018D7
        // (set) Token: 0x0600004E RID: 78 RVA: 0x000036DE File Offset: 0x000018DE
        public static GraphicsDevice GraphicsDevice { get; set; }

        // (get) Token: 0x0600004F RID: 79 RVA: 0x000036E6 File Offset: 0x000018E6
        // (set) Token: 0x06000050 RID: 80 RVA: 0x000036ED File Offset: 0x000018ED
        public static GraphicsDeviceManager GraphicsDeviceManager { get; set; }

        // (get) Token: 0x06000051 RID: 81 RVA: 0x000036F5 File Offset: 0x000018F5
        // (set) Token: 0x06000052 RID: 82 RVA: 0x000036FC File Offset: 0x000018FC
        public static ScreenSizeManager ScreenSizeManager { get; set; } = new(2560, 1440);

        // (get) Token: 0x06000053 RID: 83 RVA: 0x00003704 File Offset: 0x00001904
        public static MouseCursor MouseCursor { get; } = new();

        // (get) Token: 0x06000054 RID: 84 RVA: 0x0000370B File Offset: 0x0000190B
        // (set) Token: 0x06000055 RID: 85 RVA: 0x00003712 File Offset: 0x00001912
        public static Game1 XnaGame { get; set; }
    }
}
