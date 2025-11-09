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
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CutTheRope.ctr_commons
{
    internal class CtrRenderer : NSObject
    {
        public static void onSurfaceCreated()
        {
            if (state == 0)
            {
                state = 1;
            }
        }

        public static void onSurfaceChanged(int width, int height)
        {
            Java_com_zeptolab_ctr_CtrRenderer_nativeResize(width, height, false);
        }

        public static void onPause()
        {
            if (state == 2 || state == 5)
            {
                Java_com_zeptolab_ctr_CtrRenderer_nativePause();
                state = 3;
            }
        }

        public static void onPlaybackFinished()
        {
        }

        public static void onPlaybackStarted()
        {
            state = 5;
        }

        public static void onResume()
        {
            if (state == 3)
            {
                state = 4;
                onResumeTimeStamp = DateTimeJavaHelper.currentTimeMillis();
                DRAW_NOTHING = false;
            }
        }

        public static void onDestroy()
        {
            if (state != 1)
            {
                Java_com_zeptolab_ctr_CtrRenderer_nativeDestroy();
                state = 1;
            }
        }

        public static void update(float gameTime)
        {
            Java_com_zeptolab_ctr_CtrRenderer_nativeTick(16f);
        }

        public static void onDrawFrame()
        {
            bool flag = false;
            if (!DRAW_NOTHING && state != 0)
            {
                if (state == 1)
                {
                    state = 2;
                }
                if (state != 3)
                {
                    if (state == 4)
                    {
                        if (DateTimeJavaHelper.currentTimeMillis() - onResumeTimeStamp >= 500L)
                        {
                            Java_com_zeptolab_ctr_CtrRenderer_nativeResume();
                            Java_com_zeptolab_ctr_CtrRenderer_nativeRender();
                            flag = true;
                            state = 2;
                        }
                    }
                    else if (state == 2)
                    {
                        long timestamp = Stopwatch.GetTimestamp();
                        long num2 = timestamp - prevTick;
                        prevTick = timestamp;
                        if (num2 < 1L)
                        {
                            num2 = 1L;
                        }
                        fpsDeltas[fpsDeltasPos++] = num2;
                        int num3 = fpsDeltas.Length;
                        if (fpsDeltasPos >= num3)
                        {
                            fpsDeltasPos = 0;
                        }
                        long num4 = 0L;
                        for (int i = 0; i < num3; i++)
                        {
                            num4 += fpsDeltas[i];
                        }
                        if (num4 < 1L)
                        {
                            num4 = 1L;
                        }
                        int fps = (int)(1000000000L * num3 / num4);
                        playedTicks += DELTA_NANOS;
                        if (timestamp - playedTicks < DELTA_NANOS_THRES)
                        {
                            if (playedTicks < timestamp)
                            {
                                playedTicks = timestamp;
                            }
                        }
                        else if (state == 2)
                        {
                            playedTicks += DELTA_NANOS;
                            if (timestamp - playedTicks > DELTA_NANOS_THRES)
                            {
                                playedTicks = timestamp - DELTA_NANOS_THRES;
                            }
                        }
                        if (state == 2)
                        {
                            Java_com_zeptolab_ctr_CtrRenderer_nativeRender();
                            Java_com_zeptolab_ctr_CtrRenderer_nativeDrawFps(fps);
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

        public static void Java_com_zeptolab_ctr_CtrRenderer_nativeInit(Language language)
        {
            if (gApp != null)
            {
                _LOG("Application already created");
                return;
            }
            LANGUAGE = language;
            fmInit();
            gApp = new CTRApp();
            gApp.init();
            gApp.applicationDidFinishLaunching(null);
        }

        public static void Java_com_zeptolab_ctr_CtrRenderer_nativeDestroy()
        {
            if (gApp == null)
            {
                _LOG("Application already destroyed");
                return;
            }
            Application.sharedSoundMgr().stopAllSounds();
            Application.sharedPreferences().savePreferences();
            NSREL(gApp);
            gApp = null;
            gPaused = false;
        }

        public static void Java_com_zeptolab_ctr_CtrRenderer_nativePause()
        {
            if (!gPaused)
            {
                CTRSoundMgr._pause();
                Application.sharedMovieMgr().pause();
                gPaused = true;
                gApp?.applicationWillResignActive(null);
                CTRTexture2D.suspendAll();
            }
        }

        public static void Java_com_zeptolab_ctr_CtrRenderer_nativeResume()
        {
            if (gPaused)
            {
                CTRSoundMgr._unpause();
                Application.sharedMovieMgr().resume();
                CTRTexture2D.suspendAll();
                CTRTexture2D.resumeAll();
                gPaused = false;
                gApp?.applicationDidBecomeActive(null);
            }
        }

        public static void Java_com_zeptolab_ctr_CtrRenderer_nativeResize(int width, int height, bool isLowMem)
        {
            REAL_SCREEN_WIDTH = width;
            REAL_SCREEN_HEIGHT = height;
            SCREEN_RATIO = REAL_SCREEN_HEIGHT / REAL_SCREEN_WIDTH;
            IS_WVGA = width > 500 || height > 500;
            IS_QVGA = width < 280 || height < 280;
            if (isLowMem)
            {
                IS_WVGA = false;
            }
            VIEW_SCREEN_WIDTH = REAL_SCREEN_WIDTH;
            VIEW_SCREEN_HEIGHT = SCREEN_HEIGHT * REAL_SCREEN_WIDTH / SCREEN_WIDTH;
            if (VIEW_SCREEN_HEIGHT > REAL_SCREEN_HEIGHT)
            {
                VIEW_SCREEN_HEIGHT = REAL_SCREEN_HEIGHT;
                VIEW_SCREEN_WIDTH = SCREEN_WIDTH * REAL_SCREEN_HEIGHT / SCREEN_HEIGHT;
            }
            VIEW_OFFSET_X = (width - VIEW_SCREEN_WIDTH) / 2f;
            VIEW_OFFSET_Y = (height - VIEW_SCREEN_HEIGHT) / 2f;
            SCREEN_HEIGHT_EXPANDED = SCREEN_HEIGHT * REAL_SCREEN_HEIGHT / VIEW_SCREEN_HEIGHT;
            SCREEN_WIDTH_EXPANDED = SCREEN_WIDTH * REAL_SCREEN_WIDTH / VIEW_SCREEN_WIDTH;
            SCREEN_OFFSET_Y = (SCREEN_HEIGHT_EXPANDED - SCREEN_HEIGHT) / 2f;
            SCREEN_OFFSET_X = (SCREEN_WIDTH_EXPANDED - SCREEN_WIDTH) / 2f;
            SCREEN_BG_SCALE_Y = SCREEN_HEIGHT_EXPANDED / SCREEN_HEIGHT;
            SCREEN_BG_SCALE_X = SCREEN_WIDTH_EXPANDED / SCREEN_WIDTH;
            if (IS_WVGA)
            {
                SCREEN_WIDE_BG_SCALE_Y = (float)(SCREEN_HEIGHT_EXPANDED * 1.5 / 800.0);
                SCREEN_WIDE_BG_SCALE_X = SCREEN_BG_SCALE_X;
                return;
            }
            SCREEN_WIDE_BG_SCALE_Y = SCREEN_BG_SCALE_Y;
            SCREEN_WIDE_BG_SCALE_X = SCREEN_BG_SCALE_X;
        }

        public static void Java_com_zeptolab_ctr_CtrRenderer_nativeRender()
        {
            OpenGL.glClearColor(Color.Black);
            OpenGL.glClear(0);
            if (gApp != null)
            {
                Application.sharedRootController().performDraw();
            }
        }

        public static float transformX(float x)
        {
            return Global.ScreenSizeManager.TransformViewToGameX(x);
        }

        public static float transformY(float y)
        {
            return Global.ScreenSizeManager.TransformViewToGameY(y);
        }

        public static void Java_com_zeptolab_ctr_CtrRenderer_nativeTouchProcess(IList<TouchLocation> touches)
        {
            if (touches.Count > 0)
            {
                Application.sharedCanvas().touchesEndedwithEvent(touches);
                Application.sharedCanvas().touchesBeganwithEvent(touches);
                Application.sharedCanvas().touchesMovedwithEvent(touches);
            }
        }

        public static bool Java_com_zeptolab_ctr_CtrRenderer_nativeBackPressed()
        {
            GLCanvas gLCanvas = Application.sharedCanvas();
            return gLCanvas != null && gLCanvas.backButtonPressed();
        }

        public static bool Java_com_zeptolab_ctr_CtrRenderer_nativeMenuPressed()
        {
            GLCanvas gLCanvas = Application.sharedCanvas();
            return gLCanvas != null && gLCanvas.menuButtonPressed();
        }

        public static void Java_com_zeptolab_ctr_CtrRenderer_nativeDrawFps(int fps)
        {
            GLCanvas gLCanvas = Application.sharedCanvas();
            gLCanvas?.drawFPS(fps);
        }

        public static void Java_com_zeptolab_ctr_CtrRenderer_nativeTick(float delta)
        {
            if (gApp != null && !gPaused)
            {
                float delta2 = delta / 1000f;
                NSTimer.fireTimers(delta2);
                Application.sharedRootController().performTick(delta2);
            }
        }

        private const int UNKNOWN = 0;

        private const int UNINITIALIZED = 1;

        private const int RUNNING = 2;

        private const int PAUSED = 3;

        private const int NEED_RESUME = 4;

        private const int NEED_PAUSE = 5;

        private const long TICK_DELTA = 16L;

        private const long NANOS_IN_SECOND = 1000000000L;

        private const long NANOS_IN_MILLI = 1000000L;

        private static int state = 0;

        private static long onResumeTimeStamp = 0L;

        private static long playedTicks = 0L;

        private static long prevTick = 0L;

        private static long DELTA_NANOS = 18181818L;

        private static long DELTA_NANOS_THRES = (long)(DELTA_NANOS * 0.35);

        private static bool DRAW_NOTHING = false;

        private static CTRApp gApp;

        private static bool gPaused = false;

        private static long[] fpsDeltas = new long[10];

        private static int fpsDeltasPos = 0;
    }
}
