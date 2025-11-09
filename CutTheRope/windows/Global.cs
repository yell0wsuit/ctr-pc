using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace CutTheRope.windows
{
    // Token: 0x0200000C RID: 12
    internal class Global
    {
        // Token: 0x1700000A RID: 10
        // (get) Token: 0x0600004B RID: 75 RVA: 0x000036C8 File Offset: 0x000018C8
        // (set) Token: 0x0600004C RID: 76 RVA: 0x000036CF File Offset: 0x000018CF
        public static SpriteBatch SpriteBatch
        {
            get
            {
                return Global.spriteBatch_;
            }
            set
            {
                Global.spriteBatch_ = value;
            }
        }

        // Token: 0x1700000B RID: 11
        // (get) Token: 0x0600004D RID: 77 RVA: 0x000036D7 File Offset: 0x000018D7
        // (set) Token: 0x0600004E RID: 78 RVA: 0x000036DE File Offset: 0x000018DE
        public static GraphicsDevice GraphicsDevice
        {
            get
            {
                return Global.graphicsDevice_;
            }
            set
            {
                Global.graphicsDevice_ = value;
            }
        }

        // Token: 0x1700000C RID: 12
        // (get) Token: 0x0600004F RID: 79 RVA: 0x000036E6 File Offset: 0x000018E6
        // (set) Token: 0x06000050 RID: 80 RVA: 0x000036ED File Offset: 0x000018ED
        public static GraphicsDeviceManager GraphicsDeviceManager
        {
            get
            {
                return Global.graphicsDeviceManager_;
            }
            set
            {
                Global.graphicsDeviceManager_ = value;
            }
        }

        // Token: 0x1700000D RID: 13
        // (get) Token: 0x06000051 RID: 81 RVA: 0x000036F5 File Offset: 0x000018F5
        // (set) Token: 0x06000052 RID: 82 RVA: 0x000036FC File Offset: 0x000018FC
        public static ScreenSizeManager ScreenSizeManager
        {
            get
            {
                return Global.screenSizeManager_;
            }
            set
            {
                Global.screenSizeManager_ = value;
            }
        }

        // Token: 0x1700000E RID: 14
        // (get) Token: 0x06000053 RID: 83 RVA: 0x00003704 File Offset: 0x00001904
        public static MouseCursor MouseCursor
        {
            get
            {
                return Global.mouseCursor_;
            }
        }

        // Token: 0x1700000F RID: 15
        // (get) Token: 0x06000054 RID: 84 RVA: 0x0000370B File Offset: 0x0000190B
        // (set) Token: 0x06000055 RID: 85 RVA: 0x00003712 File Offset: 0x00001912
        public static Game1 XnaGame
        {
            get
            {
                return Global.game_;
            }
            set
            {
                Global.game_ = value;
            }
        }

        // Token: 0x0400004F RID: 79
        private static SpriteBatch spriteBatch_;

        // Token: 0x04000050 RID: 80
        private static GraphicsDevice graphicsDevice_;

        // Token: 0x04000051 RID: 81
        private static GraphicsDeviceManager graphicsDeviceManager_;

        // Token: 0x04000052 RID: 82
        private static ScreenSizeManager screenSizeManager_ = new(2560, 1440);

        // Token: 0x04000053 RID: 83
        private static MouseCursor mouseCursor_ = new();

        // Token: 0x04000054 RID: 84
        private static Game1 game_;
    }
}
