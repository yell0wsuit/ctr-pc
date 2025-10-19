using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.platform;
using CutTheRope.ios;
using CutTheRope.windows;

namespace CutTheRope.iframework
{
	// Token: 0x0200001D RID: 29
	internal class FrameworkTypes : MathHelper
	{
		// Token: 0x17000020 RID: 32
		// (get) Token: 0x06000108 RID: 264 RVA: 0x00005BCC File Offset: 0x00003DCC
		public GLCanvas canvas
		{
			get
			{
				return Application.sharedCanvas();
			}
		}

		// Token: 0x06000109 RID: 265 RVA: 0x00005BD4 File Offset: 0x00003DD4
		public static float[] toFloatArray(Quad2D[] quads)
		{
			float[] array = new float[quads.Count<Quad2D>() * 8];
			for (int i = 0; i < quads.Length; i++)
			{
				quads[i].toFloatArray().CopyTo(array, i * 8);
			}
			return array;
		}

		// Token: 0x0600010A RID: 266 RVA: 0x00005C14 File Offset: 0x00003E14
		public static float[] toFloatArray(Quad3D[] quads)
		{
			float[] array = new float[quads.Count<Quad3D>() * 12];
			for (int i = 0; i < quads.Length; i++)
			{
				quads[i].toFloatArray().CopyTo(array, i * 12);
			}
			return array;
		}

		// Token: 0x0600010B RID: 267 RVA: 0x00005C55 File Offset: 0x00003E55
		public static Rectangle MakeRectangle(double xParam, double yParam, double width, double height)
		{
			return FrameworkTypes.MakeRectangle((float)xParam, (float)yParam, (float)width, (float)height);
		}

		// Token: 0x0600010C RID: 268 RVA: 0x00005C64 File Offset: 0x00003E64
		public static Rectangle MakeRectangle(float xParam, float yParam, float width, float height)
		{
			return new Rectangle(xParam, yParam, width, height);
		}

		// Token: 0x0600010D RID: 269 RVA: 0x00005C6F File Offset: 0x00003E6F
		public static float transformToRealX(float x)
		{
			return x * FrameworkTypes.VIEW_SCREEN_WIDTH / FrameworkTypes.SCREEN_WIDTH + FrameworkTypes.VIEW_OFFSET_X;
		}

		// Token: 0x0600010E RID: 270 RVA: 0x00005C84 File Offset: 0x00003E84
		public static float transformToRealY(float y)
		{
			return y * FrameworkTypes.VIEW_SCREEN_HEIGHT / FrameworkTypes.SCREEN_HEIGHT + FrameworkTypes.VIEW_OFFSET_Y;
		}

		// Token: 0x0600010F RID: 271 RVA: 0x00005C99 File Offset: 0x00003E99
		public static float transformFromRealX(float x)
		{
			return (x - FrameworkTypes.VIEW_OFFSET_X) * FrameworkTypes.SCREEN_WIDTH / FrameworkTypes.VIEW_SCREEN_WIDTH;
		}

		// Token: 0x06000110 RID: 272 RVA: 0x00005CAE File Offset: 0x00003EAE
		public static float transformFromRealY(float y)
		{
			return (y - FrameworkTypes.VIEW_OFFSET_Y) * FrameworkTypes.SCREEN_HEIGHT / FrameworkTypes.VIEW_SCREEN_HEIGHT;
		}

		// Token: 0x06000111 RID: 273 RVA: 0x00005CC3 File Offset: 0x00003EC3
		public static float transformToRealW(float w)
		{
			return w * FrameworkTypes.VIEW_SCREEN_WIDTH / FrameworkTypes.SCREEN_WIDTH;
		}

		// Token: 0x06000112 RID: 274 RVA: 0x00005CD2 File Offset: 0x00003ED2
		public static float transformToRealH(float h)
		{
			return h * FrameworkTypes.VIEW_SCREEN_HEIGHT / FrameworkTypes.SCREEN_HEIGHT;
		}

		// Token: 0x06000113 RID: 275 RVA: 0x00005CE1 File Offset: 0x00003EE1
		public static float transformFromRealW(float w)
		{
			return w * FrameworkTypes.SCREEN_WIDTH / FrameworkTypes.VIEW_SCREEN_WIDTH;
		}

		// Token: 0x06000114 RID: 276 RVA: 0x00005CF0 File Offset: 0x00003EF0
		public static float transformFromRealH(float h)
		{
			return h * FrameworkTypes.SCREEN_HEIGHT / FrameworkTypes.VIEW_SCREEN_HEIGHT;
		}

		// Token: 0x06000115 RID: 277 RVA: 0x00005CFF File Offset: 0x00003EFF
		public static string ACHIEVEMENT_STRING(string s)
		{
			return s;
		}

		// Token: 0x06000116 RID: 278 RVA: 0x00005D02 File Offset: 0x00003F02
		public static void _LOG(string str)
		{
		}

		// Token: 0x06000117 RID: 279 RVA: 0x00005D04 File Offset: 0x00003F04
		public static float WVGAH(double H, double L)
		{
			return (float)(FrameworkTypes.IS_WVGA ? H : L);
		}

		// Token: 0x06000118 RID: 280 RVA: 0x00005D12 File Offset: 0x00003F12
		public static float WVGAD(double V)
		{
			return (float)(FrameworkTypes.IS_WVGA ? (V * 2.0) : V);
		}

		// Token: 0x06000119 RID: 281 RVA: 0x00005D2A File Offset: 0x00003F2A
		public static float RT(double H, double L)
		{
			return (float)(FrameworkTypes.IS_RETINA ? H : L);
		}

		// Token: 0x0600011A RID: 282 RVA: 0x00005D38 File Offset: 0x00003F38
		public static float RTD(double V)
		{
			return (float)(FrameworkTypes.IS_RETINA ? (V * 2.0) : V);
		}

		// Token: 0x0600011B RID: 283 RVA: 0x00005D50 File Offset: 0x00003F50
		public static float RTPD(double V)
		{
			return (float)((FrameworkTypes.IS_RETINA | FrameworkTypes.IS_IPAD) ? (V * 2.0) : V);
		}

		// Token: 0x0600011C RID: 284 RVA: 0x00005D6E File Offset: 0x00003F6E
		public static float CHOOSE3(double P1, double P2, double P3)
		{
			return FrameworkTypes.WVGAH(P2, P1);
		}

		// Token: 0x0400009C RID: 156
		public const int BLENDING_MODE_SRC_ALPHA = 0;

		// Token: 0x0400009D RID: 157
		public const int BLENDING_MODE_ONE = 1;

		// Token: 0x0400009E RID: 158
		public const int BLENDING_MODE_ADDITIVE = 2;

		// Token: 0x0400009F RID: 159
		public const int UNDEFINED = -1;

		// Token: 0x040000A0 RID: 160
		public const float FLOAT_PRECISION = 1E-06f;

		// Token: 0x040000A1 RID: 161
		public const int LEFT = 1;

		// Token: 0x040000A2 RID: 162
		public const int HCENTER = 2;

		// Token: 0x040000A3 RID: 163
		public const int RIGHT = 4;

		// Token: 0x040000A4 RID: 164
		public const int TOP = 8;

		// Token: 0x040000A5 RID: 165
		public const int VCENTER = 16;

		// Token: 0x040000A6 RID: 166
		public const int BOTTOM = 32;

		// Token: 0x040000A7 RID: 167
		public const int CENTER = 18;

		// Token: 0x040000A8 RID: 168
		public const bool YES = true;

		// Token: 0x040000A9 RID: 169
		public const bool NO = false;

		// Token: 0x040000AA RID: 170
		public const bool TRUE = true;

		// Token: 0x040000AB RID: 171
		public const bool FALSE = false;

		// Token: 0x040000AC RID: 172
		public const int GL_COLOR_BUFFER_BIT = 0;

		// Token: 0x040000AD RID: 173
		public static float SCREEN_WIDTH = 320f;

		// Token: 0x040000AE RID: 174
		public static float SCREEN_HEIGHT = 480f;

		// Token: 0x040000AF RID: 175
		public static float REAL_SCREEN_WIDTH = 480f;

		// Token: 0x040000B0 RID: 176
		public static float REAL_SCREEN_HEIGHT = 800f;

		// Token: 0x040000B1 RID: 177
		public static float SCREEN_OFFSET_Y = 0f;

		// Token: 0x040000B2 RID: 178
		public static float SCREEN_OFFSET_X = 0f;

		// Token: 0x040000B3 RID: 179
		public static float SCREEN_BG_SCALE_Y = 1f;

		// Token: 0x040000B4 RID: 180
		public static float SCREEN_BG_SCALE_X = 1f;

		// Token: 0x040000B5 RID: 181
		public static float SCREEN_WIDE_BG_SCALE_Y = 1f;

		// Token: 0x040000B6 RID: 182
		public static float SCREEN_WIDE_BG_SCALE_X = 1f;

		// Token: 0x040000B7 RID: 183
		public static float SCREEN_HEIGHT_EXPANDED = FrameworkTypes.SCREEN_HEIGHT;

		// Token: 0x040000B8 RID: 184
		public static float SCREEN_WIDTH_EXPANDED = FrameworkTypes.SCREEN_WIDTH;

		// Token: 0x040000B9 RID: 185
		public static float VIEW_SCREEN_WIDTH = 480f;

		// Token: 0x040000BA RID: 186
		public static float VIEW_SCREEN_HEIGHT = 800f;

		// Token: 0x040000BB RID: 187
		public static float VIEW_OFFSET_X = 0f;

		// Token: 0x040000BC RID: 188
		public static float VIEW_OFFSET_Y = 0f;

		// Token: 0x040000BD RID: 189
		public static float SCREEN_RATIO;

		// Token: 0x040000BE RID: 190
		public static float PORTRAIT_SCREEN_WIDTH = 480f;

		// Token: 0x040000BF RID: 191
		public static float PORTRAIT_SCREEN_HEIGHT = 320f;

		// Token: 0x040000C0 RID: 192
		public static bool IS_IPAD = false;

		// Token: 0x040000C1 RID: 193
		public static bool IS_RETINA = false;

		// Token: 0x040000C2 RID: 194
		public static bool IS_WVGA = false;

		// Token: 0x040000C3 RID: 195
		public static bool IS_QVGA = false;

		// Token: 0x020000A7 RID: 167
		public class FlurryAPI
		{
			// Token: 0x06000658 RID: 1624 RVA: 0x00033B64 File Offset: 0x00031D64
			public static void logEvent(NSString s)
			{
			}
		}

		// Token: 0x020000A8 RID: 168
		public class AndroidAPI
		{
			// Token: 0x0600065A RID: 1626 RVA: 0x00033B6E File Offset: 0x00031D6E
			public static void openUrl(NSString url)
			{
				FrameworkTypes.AndroidAPI.openUrl(url.ToString());
			}

			// Token: 0x0600065B RID: 1627 RVA: 0x00033B7C File Offset: 0x00031D7C
			public static void openUrl(string url)
			{
				try
				{
					Process.Start(url);
				}
				catch (Win32Exception ex)
				{
					int errorCode = ex.ErrorCode;
				}
				catch (Exception)
				{
				}
			}

			// Token: 0x0600065C RID: 1628 RVA: 0x00033BC0 File Offset: 0x00031DC0
			public static void showBanner()
			{
			}

			// Token: 0x0600065D RID: 1629 RVA: 0x00033BC2 File Offset: 0x00031DC2
			public static void showVideoBanner()
			{
			}

			// Token: 0x0600065E RID: 1630 RVA: 0x00033BC4 File Offset: 0x00031DC4
			public static void hideBanner()
			{
			}

			// Token: 0x0600065F RID: 1631 RVA: 0x00033BC6 File Offset: 0x00031DC6
			public static void disableBanners()
			{
			}

			// Token: 0x06000660 RID: 1632 RVA: 0x00033BC8 File Offset: 0x00031DC8
			public static void exitApp()
			{
				Global.XnaGame.Exit();
			}
		}
	}
}
