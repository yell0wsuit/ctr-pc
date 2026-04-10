using System;
using System.Collections.Generic;
using System.Diagnostics;

using CutTheRopeDX.Desktop;
using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;
using CutTheRopeDX.Helpers;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace CutTheRopeDX.Commons
{
    /// <summary>
    /// Bridges the game's shared runtime to the platform rendering and lifecycle callbacks.
    /// </summary>
    internal sealed class CtrRenderer : FrameworkTypes
    {
        /// <summary>
        /// Marks the rendering surface as created so the runtime can finish initialization on the next frame.
        /// </summary>
        public static void OnSurfaceCreated()
        {
            if (state == 0)
            {
                state = 1;
            }
        }

        /// <summary>
        /// Updates the logical screen metrics after the platform surface size changes.
        /// </summary>
        /// <param name="width">The new surface width in pixels.</param>
        /// <param name="height">The new surface height in pixels.</param>
        public static void OnSurfaceChanged(int width, int height)
        {
            Java_com_zeptolab_ctr_CtrRenderer_nativeResize(width, height, false);
        }

        /// <summary>
        /// Pauses rendering and runtime subsystems when the platform host is paused.
        /// </summary>
        public static void OnPause()
        {
            if (state is 2 or 5)
            {
                Java_com_zeptolab_ctr_CtrRenderer_nativePause();
                state = 3;
            }
        }

        /// <summary>
        /// Handles playback completion notifications.
        /// </summary>
        public static void OnPlaybackFinished()
        {
        }

        /// <summary>
        /// Marks the renderer as being in playback mode.
        /// </summary>
        public static void OnPlaybackStarted()
        {
            state = 5;
        }

        /// <summary>
        /// Schedules the runtime to resume after the platform host becomes active again.
        /// </summary>
        public static void OnResume()
        {
            if (state == 3)
            {
                state = 4;
                onResumeTimeStamp = DateTimeJavaHelper.CurrentTimeMillis();
                DRAW_NOTHING = false;
            }
        }

        /// <summary>
        /// Tears down the runtime when the platform surface is being destroyed.
        /// </summary>
        public static void OnDestroy()
        {
            if (state != 1)
            {
                Java_com_zeptolab_ctr_CtrRenderer_nativeDestroy();
                state = 1;
            }
        }

        /// <summary>
        /// Advances the game runtime using the fixed frame delta expected by the original renderer.
        /// </summary>
        public static void Update()
        {
            Java_com_zeptolab_ctr_CtrRenderer_nativeTick(16f);
        }

        /// <summary>
        /// Renders a frame or clears the backbuffer when rendering is currently suspended.
        /// </summary>
        public static void OnDrawFrame()
        {
            bool didRenderFrame = false;
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
                            didRenderFrame = true;
                            state = 2;
                        }
                    }
                    else if (state == 2)
                    {
                        long timestamp = Stopwatch.GetTimestamp();
                        long frameDeltaNanos = timestamp - prevTick;
                        prevTick = timestamp;
                        if (frameDeltaNanos < 1L)
                        {
                            frameDeltaNanos = 1L;
                        }
                        fpsDeltas[fpsDeltasPos++] = frameDeltaNanos;
                        int sampleCount = fpsDeltas.Length;
                        if (fpsDeltasPos >= sampleCount)
                        {
                            fpsDeltasPos = 0;
                        }
                        long totalDeltaNanos = 0L;
                        for (int i = 0; i < sampleCount; i++)
                        {
                            totalDeltaNanos += fpsDeltas[i];
                        }
                        if (totalDeltaNanos < 1L)
                        {
                            totalDeltaNanos = 1L;
                        }
                        int fps = (int)(1000000000L * sampleCount / totalDeltaNanos);
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
                            didRenderFrame = true;
                        }
                    }
                }
            }
            if (!didRenderFrame)
            {
                try
                {
                    Renderer.SetClearColor(Color.Black);
                    Renderer.Clear(0);
                }
                catch (Exception)
                {
                }
            }
        }

        /// <summary>
        /// Initializes the shared application runtime with the selected language.
        /// </summary>
        /// <param name="language">The language to assign to the runtime before launch.</param>
        public static void Java_com_zeptolab_ctr_CtrRenderer_nativeInit(Language language)
        {
            if (gApp != null)
            {
                LOG();
                return;
            }
            LanguageHelper.Current = language;
            FmInit();
            gApp = new CTRApp();
            gApp.ApplicationDidFinishLaunching();
        }

        /// <summary>
        /// Destroys the shared application runtime and saves any pending preferences.
        /// </summary>
        public static void Java_com_zeptolab_ctr_CtrRenderer_nativeDestroy()
        {
            if (gApp == null)
            {
                LOG();
                return;
            }
            Application.SharedSoundMgr().StopAllSounds();
            Preferences.RequestSave();
            gApp = null;
            gPaused = false;
        }

        /// <summary>
        /// Suspends audio, movie playback, textures, and app state.
        /// </summary>
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

        /// <summary>
        /// Resumes audio, movie playback, textures, and app state after a pause.
        /// </summary>
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

        /// <summary>
        /// Recalculates the shared screen metrics for the current surface size.
        /// </summary>
        /// <param name="width">The surface width in pixels.</param>
        /// <param name="height">The surface height in pixels.</param>
        /// <param name="isLowMem">Whether the resize should use the low-memory layout path.</param>
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
                SCREEN_WIDE_BG_SCALE_Y = SCREEN_HEIGHT_EXPANDED * 1.5f / 800;
                SCREEN_WIDE_BG_SCALE_X = SCREEN_BG_SCALE_X;
                return;
            }
            SCREEN_WIDE_BG_SCALE_Y = SCREEN_BG_SCALE_Y;
            SCREEN_WIDE_BG_SCALE_X = SCREEN_BG_SCALE_X;
        }

        /// <summary>
        /// Clears the frame and delegates drawing to the root controller.
        /// </summary>
        public static void Java_com_zeptolab_ctr_CtrRenderer_nativeRender()
        {
            Renderer.SetClearColor(Color.Black);
            Renderer.Clear(0);
            if (gApp != null)
            {
                Application.SharedRootController().PerformDraw();
            }
        }

        /// <summary>
        /// Converts a view-space X coordinate into game-space coordinates.
        /// </summary>
        /// <param name="x">The view-space X coordinate.</param>
        /// <returns>The transformed game-space X coordinate.</returns>
        public static float TransformX(float x)
        {
            return Global.ScreenSizeManager.TransformViewToGameX(x);
        }

        /// <summary>
        /// Converts a view-space Y coordinate into game-space coordinates.
        /// </summary>
        /// <param name="y">The view-space Y coordinate.</param>
        /// <returns>The transformed game-space Y coordinate.</returns>
        public static float TransformY(float y)
        {
            return Global.ScreenSizeManager.TransformViewToGameY(y);
        }

        /// <summary>
        /// Forwards touch input from the platform layer to the shared canvas.
        /// </summary>
        /// <param name="touches">The touch locations reported for the current frame.</param>
        public static void Java_com_zeptolab_ctr_CtrRenderer_nativeTouchProcess(IList<TouchLocation> touches)
        {
            if (touches.Count > 0)
            {
                Application.SharedCanvas().TouchesEndedwithEvent(touches);
                Application.SharedCanvas().TouchesBeganwithEvent(touches);
                Application.SharedCanvas().TouchesMovedwithEvent(touches);
            }
        }

        /// <summary>
        /// Forwards the back-button action to the shared canvas.
        /// </summary>
        /// <returns><see langword="true"/> if the canvas handled the action; otherwise, <see langword="false"/>.</returns>
        public static bool Java_com_zeptolab_ctr_CtrRenderer_nativeBackPressed()
        {
            GLCanvas gLCanvas = Application.SharedCanvas();
            return gLCanvas != null && gLCanvas.BackButtonPressed();
        }

        /// <summary>
        /// Forwards the menu-button action to the shared canvas.
        /// </summary>
        /// <returns><see langword="true"/> if the canvas handled the action; otherwise, <see langword="false"/>.</returns>
        public static bool Java_com_zeptolab_ctr_CtrRenderer_nativeMenuPressed()
        {
            GLCanvas gLCanvas = Application.SharedCanvas();
            return gLCanvas != null && gLCanvas.MenuButtonPressed();
        }

        /// <summary>
        /// Draws the current frames-per-second counter on the shared canvas.
        /// </summary>
        /// <param name="fps">The frames-per-second value to display.</param>
        public static void Java_com_zeptolab_ctr_CtrRenderer_nativeDrawFps(int fps)
        {
            GLCanvas gLCanvas = Application.SharedCanvas();
            gLCanvas?.DrawFPS(fps);
        }

        /// <summary>
        /// Advances timers and the root controller by the specified frame delta.
        /// </summary>
        /// <param name="delta">The frame delta in milliseconds.</param>
        public static void Java_com_zeptolab_ctr_CtrRenderer_nativeTick(float delta)
        {
            if (gApp != null && !gPaused)
            {
                float deltaSeconds = delta / 1000f;
                TimerManager.Update(deltaSeconds);
                Application.SharedRootController().PerformTick(deltaSeconds);
            }
        }

        /// <summary>
        /// Tracks the current renderer lifecycle state.
        /// </summary>
        private static int state;

        /// <summary>
        /// Stores the timestamp recorded when a resume was requested.
        /// </summary>
        private static long onResumeTimeStamp;

        /// <summary>
        /// Accumulates the simulated playback timeline used for frame pacing.
        /// </summary>
        private static long playedTicks;

        /// <summary>
        /// Stores the stopwatch timestamp captured for the previous rendered frame.
        /// </summary>
        private static long prevTick;

        /// <summary>
        /// The nominal frame duration in nanoseconds for the fixed-step renderer.
        /// </summary>
        private static readonly long DELTA_NANOS = 18181818L;

        /// <summary>
        /// The maximum timing drift tolerated before the pacing timeline is clamped.
        /// </summary>
        private static readonly long DELTA_NANOS_THRES = (long)(DELTA_NANOS * 0.35);

        /// <summary>
        /// Indicates whether frame rendering should be skipped temporarily.
        /// </summary>
        private static bool DRAW_NOTHING;

        /// <summary>
        /// Holds the shared application instance owned by the renderer bridge.
        /// </summary>
        private static CTRApp gApp;

        /// <summary>
        /// Indicates whether the runtime is currently paused.
        /// </summary>
        private static bool gPaused;

        /// <summary>
        /// Stores recent frame deltas for FPS calculation.
        /// </summary>
        private static readonly long[] fpsDeltas = new long[10];

        /// <summary>
        /// Tracks the insertion index within <see cref="fpsDeltas"/>.
        /// </summary>
        private static int fpsDeltasPos;
    }
}
