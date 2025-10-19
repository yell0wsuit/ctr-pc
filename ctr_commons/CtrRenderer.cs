using System;
using System.Collections.Generic;
using System.Diagnostics;
using CutTheRope.game;
using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.platform;
using CutTheRope.iframework.visual;
using CutTheRope.ios;
using CutTheRope.windows;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace CutTheRope.ctr_commons
{
	// Token: 0x0200009B RID: 155
	internal class CtrRenderer : NSObject
	{
		// Token: 0x06000615 RID: 1557 RVA: 0x00032A03 File Offset: 0x00030C03
		public static void onSurfaceCreated()
		{
			if (CtrRenderer.state == 0)
			{
				CtrRenderer.state = 1;
			}
		}

		// Token: 0x06000616 RID: 1558 RVA: 0x00032A12 File Offset: 0x00030C12
		public static void onSurfaceChanged(int width, int height)
		{
			CtrRenderer.Java_com_zeptolab_ctr_CtrRenderer_nativeResize(width, height, false);
		}

		// Token: 0x06000617 RID: 1559 RVA: 0x00032A1C File Offset: 0x00030C1C
		public static void onPause()
		{
			if (CtrRenderer.state == 2 || CtrRenderer.state == 5)
			{
				CtrRenderer.Java_com_zeptolab_ctr_CtrRenderer_nativePause();
				CtrRenderer.state = 3;
			}
		}

		// Token: 0x06000618 RID: 1560 RVA: 0x00032A39 File Offset: 0x00030C39
		public static void onPlaybackFinished()
		{
		}

		// Token: 0x06000619 RID: 1561 RVA: 0x00032A3B File Offset: 0x00030C3B
		public static void onPlaybackStarted()
		{
			CtrRenderer.state = 5;
		}

		// Token: 0x0600061A RID: 1562 RVA: 0x00032A43 File Offset: 0x00030C43
		public static void onResume()
		{
			if (CtrRenderer.state == 3)
			{
				CtrRenderer.state = 4;
				CtrRenderer.onResumeTimeStamp = DateTimeJavaHelper.currentTimeMillis();
				CtrRenderer.DRAW_NOTHING = false;
			}
		}

		// Token: 0x0600061B RID: 1563 RVA: 0x00032A63 File Offset: 0x00030C63
		public static void onDestroy()
		{
			if (CtrRenderer.state != 1)
			{
				CtrRenderer.Java_com_zeptolab_ctr_CtrRenderer_nativeDestroy();
				CtrRenderer.state = 1;
			}
		}

		// Token: 0x0600061C RID: 1564 RVA: 0x00032A78 File Offset: 0x00030C78
		public static void update(float gameTime)
		{
			CtrRenderer.Java_com_zeptolab_ctr_CtrRenderer_nativeTick(16f);
		}

		// Token: 0x0600061D RID: 1565 RVA: 0x00032A84 File Offset: 0x00030C84
		public static void onDrawFrame()
		{
			bool flag = false;
			if (!CtrRenderer.DRAW_NOTHING && CtrRenderer.state != 0)
			{
				if (CtrRenderer.state == 1)
				{
					CtrRenderer.state = 2;
				}
				if (CtrRenderer.state != 3)
				{
					if (CtrRenderer.state == 4)
					{
						if (DateTimeJavaHelper.currentTimeMillis() - CtrRenderer.onResumeTimeStamp >= 500L)
						{
							CtrRenderer.Java_com_zeptolab_ctr_CtrRenderer_nativeResume();
							CtrRenderer.Java_com_zeptolab_ctr_CtrRenderer_nativeRender();
							flag = true;
							CtrRenderer.state = 2;
						}
					}
					else if (CtrRenderer.state == 2)
					{
						long timestamp = Stopwatch.GetTimestamp();
						long num2 = timestamp - CtrRenderer.prevTick;
						CtrRenderer.prevTick = timestamp;
						if (num2 < 1L)
						{
							num2 = 1L;
						}
						CtrRenderer.fpsDeltas[CtrRenderer.fpsDeltasPos++] = num2;
						int num3 = CtrRenderer.fpsDeltas.Length;
						if (CtrRenderer.fpsDeltasPos >= num3)
						{
							CtrRenderer.fpsDeltasPos = 0;
						}
						long num4 = 0L;
						for (int i = 0; i < num3; i++)
						{
							num4 += CtrRenderer.fpsDeltas[i];
						}
						if (num4 < 1L)
						{
							num4 = 1L;
						}
						int fps = (int)(1000000000L * (long)num3 / num4);
						CtrRenderer.playedTicks += CtrRenderer.DELTA_NANOS;
						if (timestamp - CtrRenderer.playedTicks < CtrRenderer.DELTA_NANOS_THRES)
						{
							if (CtrRenderer.playedTicks < timestamp)
							{
								CtrRenderer.playedTicks = timestamp;
							}
						}
						else if (CtrRenderer.state == 2)
						{
							CtrRenderer.playedTicks += CtrRenderer.DELTA_NANOS;
							if (timestamp - CtrRenderer.playedTicks > CtrRenderer.DELTA_NANOS_THRES)
							{
								CtrRenderer.playedTicks = timestamp - CtrRenderer.DELTA_NANOS_THRES;
							}
						}
						if (CtrRenderer.state == 2)
						{
							CtrRenderer.Java_com_zeptolab_ctr_CtrRenderer_nativeRender();
							CtrRenderer.Java_com_zeptolab_ctr_CtrRenderer_nativeDrawFps(fps);
							flag = true;
						}
					}
				}
			}
			if (!flag)
			{
				try
				{
					OpenGL.glClearColor(Color.Black);
					OpenGL.glClear(0);
				}
				catch (Exception)
				{
				}
			}
		}

		// Token: 0x0600061E RID: 1566 RVA: 0x00032C20 File Offset: 0x00030E20
		public static void Java_com_zeptolab_ctr_CtrRenderer_nativeInit(Language language)
		{
			if (CtrRenderer.gApp != null)
			{
				FrameworkTypes._LOG("Application already created");
				return;
			}
			ResDataPhoneFull.LANGUAGE = language;
			CutTheRope.iframework.helpers.MathHelper.fmInit();
			CtrRenderer.gApp = new CTRApp();
			CtrRenderer.gApp.init();
			CtrRenderer.gApp.applicationDidFinishLaunching(null);
		}

		// Token: 0x0600061F RID: 1567 RVA: 0x00032C5F File Offset: 0x00030E5F
		public static void Java_com_zeptolab_ctr_CtrRenderer_nativeDestroy()
		{
			if (CtrRenderer.gApp == null)
			{
				FrameworkTypes._LOG("Application already destroyed");
				return;
			}
			Application.sharedSoundMgr().stopAllSounds();
			Application.sharedPreferences().savePreferences();
			NSObject.NSREL(CtrRenderer.gApp);
			CtrRenderer.gApp = null;
			CtrRenderer.gPaused = false;
		}

		// Token: 0x06000620 RID: 1568 RVA: 0x00032C9D File Offset: 0x00030E9D
		public static void Java_com_zeptolab_ctr_CtrRenderer_nativePause()
		{
			if (!CtrRenderer.gPaused)
			{
				CTRSoundMgr._pause();
				Application.sharedMovieMgr().pause();
				CtrRenderer.gPaused = true;
				if (CtrRenderer.gApp != null)
				{
					CtrRenderer.gApp.applicationWillResignActive(null);
				}
				Texture2D.suspendAll();
			}
		}

		// Token: 0x06000621 RID: 1569 RVA: 0x00032CD2 File Offset: 0x00030ED2
		public static void Java_com_zeptolab_ctr_CtrRenderer_nativeResume()
		{
			if (CtrRenderer.gPaused)
			{
				CTRSoundMgr._unpause();
				Application.sharedMovieMgr().resume();
				Texture2D.suspendAll();
				Texture2D.resumeAll();
				CtrRenderer.gPaused = false;
				if (CtrRenderer.gApp != null)
				{
					CtrRenderer.gApp.applicationDidBecomeActive(null);
				}
			}
		}

		// Token: 0x06000622 RID: 1570 RVA: 0x00032D0C File Offset: 0x00030F0C
		public static void Java_com_zeptolab_ctr_CtrRenderer_nativeResize(int width, int height, bool isLowMem)
		{
			FrameworkTypes.REAL_SCREEN_WIDTH = (float)width;
			FrameworkTypes.REAL_SCREEN_HEIGHT = (float)height;
			FrameworkTypes.SCREEN_RATIO = FrameworkTypes.REAL_SCREEN_HEIGHT / FrameworkTypes.REAL_SCREEN_WIDTH;
			FrameworkTypes.IS_WVGA = width > 500 || height > 500;
			FrameworkTypes.IS_QVGA = width < 280 || height < 280;
			if (isLowMem)
			{
				FrameworkTypes.IS_WVGA = false;
			}
			FrameworkTypes.VIEW_SCREEN_WIDTH = FrameworkTypes.REAL_SCREEN_WIDTH;
			FrameworkTypes.VIEW_SCREEN_HEIGHT = FrameworkTypes.SCREEN_HEIGHT * FrameworkTypes.REAL_SCREEN_WIDTH / FrameworkTypes.SCREEN_WIDTH;
			if (FrameworkTypes.VIEW_SCREEN_HEIGHT > FrameworkTypes.REAL_SCREEN_HEIGHT)
			{
				FrameworkTypes.VIEW_SCREEN_HEIGHT = FrameworkTypes.REAL_SCREEN_HEIGHT;
				FrameworkTypes.VIEW_SCREEN_WIDTH = FrameworkTypes.SCREEN_WIDTH * FrameworkTypes.REAL_SCREEN_HEIGHT / FrameworkTypes.SCREEN_HEIGHT;
			}
			FrameworkTypes.VIEW_OFFSET_X = ((float)width - FrameworkTypes.VIEW_SCREEN_WIDTH) / 2f;
			FrameworkTypes.VIEW_OFFSET_Y = ((float)height - FrameworkTypes.VIEW_SCREEN_HEIGHT) / 2f;
			FrameworkTypes.SCREEN_HEIGHT_EXPANDED = FrameworkTypes.SCREEN_HEIGHT * FrameworkTypes.REAL_SCREEN_HEIGHT / FrameworkTypes.VIEW_SCREEN_HEIGHT;
			FrameworkTypes.SCREEN_WIDTH_EXPANDED = FrameworkTypes.SCREEN_WIDTH * FrameworkTypes.REAL_SCREEN_WIDTH / FrameworkTypes.VIEW_SCREEN_WIDTH;
			FrameworkTypes.SCREEN_OFFSET_Y = (FrameworkTypes.SCREEN_HEIGHT_EXPANDED - FrameworkTypes.SCREEN_HEIGHT) / 2f;
			FrameworkTypes.SCREEN_OFFSET_X = (FrameworkTypes.SCREEN_WIDTH_EXPANDED - FrameworkTypes.SCREEN_WIDTH) / 2f;
			FrameworkTypes.SCREEN_BG_SCALE_Y = FrameworkTypes.SCREEN_HEIGHT_EXPANDED / FrameworkTypes.SCREEN_HEIGHT;
			FrameworkTypes.SCREEN_BG_SCALE_X = FrameworkTypes.SCREEN_WIDTH_EXPANDED / FrameworkTypes.SCREEN_WIDTH;
			if (FrameworkTypes.IS_WVGA)
			{
				FrameworkTypes.SCREEN_WIDE_BG_SCALE_Y = (float)((double)FrameworkTypes.SCREEN_HEIGHT_EXPANDED * 1.5 / 800.0);
				FrameworkTypes.SCREEN_WIDE_BG_SCALE_X = FrameworkTypes.SCREEN_BG_SCALE_X;
				return;
			}
			FrameworkTypes.SCREEN_WIDE_BG_SCALE_Y = FrameworkTypes.SCREEN_BG_SCALE_Y;
			FrameworkTypes.SCREEN_WIDE_BG_SCALE_X = FrameworkTypes.SCREEN_BG_SCALE_X;
		}

		// Token: 0x06000623 RID: 1571 RVA: 0x00032EA0 File Offset: 0x000310A0
		public static void Java_com_zeptolab_ctr_CtrRenderer_nativeRender()
		{
			OpenGL.glClearColor(Color.Black);
			OpenGL.glClear(0);
			if (CtrRenderer.gApp != null)
			{
				Application.sharedRootController().performDraw();
			}
		}

		// Token: 0x06000624 RID: 1572 RVA: 0x00032EC3 File Offset: 0x000310C3
		public static float transformX(float x)
		{
			return Global.ScreenSizeManager.TransformViewToGameX(x);
		}

		// Token: 0x06000625 RID: 1573 RVA: 0x00032ED0 File Offset: 0x000310D0
		public static float transformY(float y)
		{
			return Global.ScreenSizeManager.TransformViewToGameY(y);
		}

		// Token: 0x06000626 RID: 1574 RVA: 0x00032EDD File Offset: 0x000310DD
		public static void Java_com_zeptolab_ctr_CtrRenderer_nativeTouchProcess(IList<TouchLocation> touches)
		{
			if (touches.Count > 0)
			{
				Application.sharedCanvas().touchesEndedwithEvent(touches);
				Application.sharedCanvas().touchesBeganwithEvent(touches);
				Application.sharedCanvas().touchesMovedwithEvent(touches);
			}
		}

		// Token: 0x06000627 RID: 1575 RVA: 0x00032F0C File Offset: 0x0003110C
		public static bool Java_com_zeptolab_ctr_CtrRenderer_nativeBackPressed()
		{
			GLCanvas gLCanvas = Application.sharedCanvas();
			return gLCanvas != null && gLCanvas.backButtonPressed();
		}

		// Token: 0x06000628 RID: 1576 RVA: 0x00032F2C File Offset: 0x0003112C
		public static bool Java_com_zeptolab_ctr_CtrRenderer_nativeMenuPressed()
		{
			GLCanvas gLCanvas = Application.sharedCanvas();
			return gLCanvas != null && gLCanvas.menuButtonPressed();
		}

		// Token: 0x06000629 RID: 1577 RVA: 0x00032F4C File Offset: 0x0003114C
		public static void Java_com_zeptolab_ctr_CtrRenderer_nativeDrawFps(int fps)
		{
			GLCanvas gLCanvas = Application.sharedCanvas();
			if (gLCanvas != null)
			{
				gLCanvas.drawFPS((float)fps);
			}
		}

		// Token: 0x0600062A RID: 1578 RVA: 0x00032F6C File Offset: 0x0003116C
		public static void Java_com_zeptolab_ctr_CtrRenderer_nativeTick(float delta)
		{
			if (CtrRenderer.gApp != null && !CtrRenderer.gPaused)
			{
				float delta2 = delta / 1000f;
				NSTimer.fireTimers(delta2);
				Application.sharedRootController().performTick(delta2);
			}
		}

		// Token: 0x04000843 RID: 2115
		private const int UNKNOWN = 0;

		// Token: 0x04000844 RID: 2116
		private const int UNINITIALIZED = 1;

		// Token: 0x04000845 RID: 2117
		private const int RUNNING = 2;

		// Token: 0x04000846 RID: 2118
		private const int PAUSED = 3;

		// Token: 0x04000847 RID: 2119
		private const int NEED_RESUME = 4;

		// Token: 0x04000848 RID: 2120
		private const int NEED_PAUSE = 5;

		// Token: 0x04000849 RID: 2121
		private const long TICK_DELTA = 16L;

		// Token: 0x0400084A RID: 2122
		private const long NANOS_IN_SECOND = 1000000000L;

		// Token: 0x0400084B RID: 2123
		private const long NANOS_IN_MILLI = 1000000L;

		// Token: 0x0400084C RID: 2124
		private static int state = 0;

		// Token: 0x0400084D RID: 2125
		private static long onResumeTimeStamp = 0L;

		// Token: 0x0400084E RID: 2126
		private static long playedTicks = 0L;

		// Token: 0x0400084F RID: 2127
		private static long prevTick = 0L;

		// Token: 0x04000850 RID: 2128
		private static long DELTA_NANOS = 18181818L;

		// Token: 0x04000851 RID: 2129
		private static long DELTA_NANOS_THRES = (long)((double)CtrRenderer.DELTA_NANOS * 0.35);

		// Token: 0x04000852 RID: 2130
		private static bool DRAW_NOTHING = false;

		// Token: 0x04000853 RID: 2131
		private static CTRApp gApp;

		// Token: 0x04000854 RID: 2132
		private static bool gPaused = false;

		// Token: 0x04000855 RID: 2133
		private static long[] fpsDeltas = new long[10];

		// Token: 0x04000856 RID: 2134
		private static int fpsDeltasPos = 0;
	}
}
