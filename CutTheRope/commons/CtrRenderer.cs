using System;
using System.Collections.Generic;
using System.Diagnostics;

using CutTheRope.Desktop;
using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Platform;
using CutTheRope.Framework.Visual;
using CutTheRope.GameMain;
using CutTheRope.Helpers;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace CutTheRope.Commons
{
    internal sealed class CtrRenderer : FrameworkTypes
    {
        public static void OnSurfaceCreated()
        {
            if (state == 0)
            {
                state = 1;
            }
        }

        public static void OnSurfaceChanged(int width, int height)
        {
            Java_com_zeptolab_ctr_CtrRenderer_nativeResize(width, height, false);
        }

        public static void OnPause()
        {
            if (state is 2 or 5)
            {
                Java_com_zeptolab_ctr_CtrRenderer_nativePause();
                state = 3;
            }
        }

        public static void OnPlaybackFinished()
        {
        }

        public static void OnPlaybackStarted()
        {
            state = 5;
        }

        public static void OnResume()
        {
            if (state == 3)
            {
                state = 4;
                onResumeTimeStamp = DateTimeJavaHelper.CurrentTimeMillis();
                DRAW_NOTHING = false;
            }
        }

        public static void OnDestroy()
        {
            if (state != 1)
            {
                Java_com_zeptolab_ctr_CtrRenderer_nativeDestroy();
                state = 1;
            }
        }

        public static void Update(float gameTime)
        {
            Java_com_zeptolab_ctr_CtrRenderer_nativeTick(16f);
        }

        public static void OnDrawFrame()
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
                        if (DateTimeJavaHelper.CurrentTimeMillis() - onResumeTimeStamp >= 500L)
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
                    OpenGL.GlClearColor(Color.Black);
                    OpenGL.GlClear(0);
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
                LOG("Application already created");
                return;
            }
            LANGUAGE = language;
            FmInit();
            gApp = new CTRApp();
            gApp.ApplicationDidFinishLaunching();
        }

        public static void Java_com_zeptolab_ctr_CtrRenderer_nativeDestroy()
        {
            if (gApp == null)
            {
                LOG("Application already destroyed");
                return;
            }
            Application.SharedSoundMgr().StopAllSounds();
            Preferences.RequestSave();
            gApp = null;
            gPaused = false;
        }

        public static void Java_com_zeptolab_ctr_CtrRenderer_nativePause()
        {
            if (!gPaused)
            {
                CTRSoundMgr.Pause();
                Application.SharedMovieMgr().Pause();
                gPaused = true;
                CTRApp.ApplicationWillResignActive();
                CTRTexture2D.SuspendAll();
            }
        }

        public static void Java_com_zeptolab_ctr_CtrRenderer_nativeResume()
        {
            if (gPaused)
            {
                CTRSoundMgr.Unpause();
                Application.SharedMovieMgr().Resume();
                CTRTexture2D.SuspendAll();
                CTRTexture2D.ResumeAll();
                gPaused = false;
                CTRApp.ApplicationDidBecomeActive();
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
            OpenGL.GlClearColor(Color.Black);
            OpenGL.GlClear(0);
            if (gApp != null)
            {
                Application.SharedRootController().PerformDraw();
            }
        }

        public static float TransformX(float x)
        {
            return Global.ScreenSizeManager.TransformViewToGameX(x);
        }

        public static float TransformY(float y)
        {
            return Global.ScreenSizeManager.TransformViewToGameY(y);
        }

        public static void Java_com_zeptolab_ctr_CtrRenderer_nativeTouchProcess(IList<TouchLocation> touches)
        {
            if (touches.Count > 0)
            {
                Application.SharedCanvas().TouchesEndedwithEvent(touches);
                Application.SharedCanvas().TouchesBeganwithEvent(touches);
                Application.SharedCanvas().TouchesMovedwithEvent(touches);
            }
        }

        public static bool Java_com_zeptolab_ctr_CtrRenderer_nativeBackPressed()
        {
            GLCanvas gLCanvas = Application.SharedCanvas();
            return gLCanvas != null && gLCanvas.BackButtonPressed();
        }

        public static bool Java_com_zeptolab_ctr_CtrRenderer_nativeMenuPressed()
        {
            GLCanvas gLCanvas = Application.SharedCanvas();
            return gLCanvas != null && gLCanvas.MenuButtonPressed();
        }

        public static void Java_com_zeptolab_ctr_CtrRenderer_nativeDrawFps(int fps)
        {
            GLCanvas gLCanvas = Application.SharedCanvas();
            gLCanvas?.DrawFPS(fps);
        }

        public static void Java_com_zeptolab_ctr_CtrRenderer_nativeTick(float delta)
        {
            if (gApp != null && !gPaused)
            {
                float delta2 = delta / 1000f;
                TimerManager.Update(delta2);
                Application.SharedRootController().PerformTick(delta2);
            }
        }

        private static int state;

        private static long onResumeTimeStamp;

        private static long playedTicks;

        private static long prevTick;

        private static readonly long DELTA_NANOS = 18181818L;

        private static readonly long DELTA_NANOS_THRES = (long)(DELTA_NANOS * 0.35);

        private static bool DRAW_NOTHING;

        private static CTRApp gApp;

        private static bool gPaused;

        private static readonly long[] fpsDeltas = new long[10];

        private static int fpsDeltasPos;
    }
}
