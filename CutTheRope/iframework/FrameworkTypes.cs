using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.platform;
using CutTheRope.ios;
using CutTheRope.windows;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace CutTheRope.iframework
{
    internal class FrameworkTypes : CTRMathHelper
    {
        // (get) Token: 0x06000108 RID: 264 RVA: 0x00005BCC File Offset: 0x00003DCC
        public GLCanvas canvas
        {
            get
            {
                return Application.sharedCanvas();
            }
        }

        public static float[] toFloatArray(Quad2D[] quads)
        {
            float[] array = new float[quads.Count() * 8];
            for (int i = 0; i < quads.Length; i++)
            {
                quads[i].toFloatArray().CopyTo(array, i * 8);
            }
            return array;
        }

        public static float[] toFloatArray(Quad3D[] quads)
        {
            float[] array = new float[quads.Count() * 12];
            for (int i = 0; i < quads.Length; i++)
            {
                quads[i].toFloatArray().CopyTo(array, i * 12);
            }
            return array;
        }

        public static CTRRectangle MakeRectangle(double xParam, double yParam, double width, double height)
        {
            return MakeRectangle((float)xParam, (float)yParam, (float)width, (float)height);
        }

        public static CTRRectangle MakeRectangle(float xParam, float yParam, float width, float height)
        {
            return new CTRRectangle(xParam, yParam, width, height);
        }

        public static float transformToRealX(float x)
        {
            return x * VIEW_SCREEN_WIDTH / SCREEN_WIDTH + VIEW_OFFSET_X;
        }

        public static float transformToRealY(float y)
        {
            return y * VIEW_SCREEN_HEIGHT / SCREEN_HEIGHT + VIEW_OFFSET_Y;
        }

        public static float transformFromRealX(float x)
        {
            return (x - VIEW_OFFSET_X) * SCREEN_WIDTH / VIEW_SCREEN_WIDTH;
        }

        public static float transformFromRealY(float y)
        {
            return (y - VIEW_OFFSET_Y) * SCREEN_HEIGHT / VIEW_SCREEN_HEIGHT;
        }

        public static float transformToRealW(float w)
        {
            return w * VIEW_SCREEN_WIDTH / SCREEN_WIDTH;
        }

        public static float transformToRealH(float h)
        {
            return h * VIEW_SCREEN_HEIGHT / SCREEN_HEIGHT;
        }

        public static float transformFromRealW(float w)
        {
            return w * SCREEN_WIDTH / VIEW_SCREEN_WIDTH;
        }

        public static float transformFromRealH(float h)
        {
            return h * SCREEN_HEIGHT / VIEW_SCREEN_HEIGHT;
        }

        public static string ACHIEVEMENT_STRING(string s)
        {
            return s;
        }

        public static void _LOG(string str)
        {
        }

        public static float WVGAH(double H, double L)
        {
            return (float)(IS_WVGA ? H : L);
        }

        public static float WVGAD(double V)
        {
            return (float)(IS_WVGA ? (V * 2.0) : V);
        }

        public static float RT(double H, double L)
        {
            return (float)(IS_RETINA ? H : L);
        }

        public static float RTD(double V)
        {
            return (float)(IS_RETINA ? (V * 2.0) : V);
        }

        public static float RTPD(double V)
        {
            return (float)((IS_RETINA | IS_IPAD) ? (V * 2.0) : V);
        }

        public static float CHOOSE3(double P1, double P2, double P3)
        {
            return WVGAH(P2, P1);
        }

        public const int BLENDING_MODE_SRC_ALPHA = 0;

        public const int BLENDING_MODE_ONE = 1;

        public const int BLENDING_MODE_ADDITIVE = 2;

        public const int UNDEFINED = -1;

        public const float FLOAT_PRECISION = 1E-06f;

        public const int LEFT = 1;

        public const int HCENTER = 2;

        public const int RIGHT = 4;

        public const int TOP = 8;

        public const int VCENTER = 16;

        public const int BOTTOM = 32;

        public const int CENTER = 18;

        public const bool YES = true;

        public const bool NO = false;

        public const bool TRUE = true;

        public const bool FALSE = false;

        public const int GL_COLOR_BUFFER_BIT = 0;

        public static float SCREEN_WIDTH = 320f;

        public static float SCREEN_HEIGHT = 480f;

        public static float REAL_SCREEN_WIDTH = 480f;

        public static float REAL_SCREEN_HEIGHT = 800f;

        public static float SCREEN_OFFSET_Y = 0f;

        public static float SCREEN_OFFSET_X = 0f;

        public static float SCREEN_BG_SCALE_Y = 1f;

        public static float SCREEN_BG_SCALE_X = 1f;

        public static float SCREEN_WIDE_BG_SCALE_Y = 1f;

        public static float SCREEN_WIDE_BG_SCALE_X = 1f;

        public static float SCREEN_HEIGHT_EXPANDED = SCREEN_HEIGHT;

        public static float SCREEN_WIDTH_EXPANDED = SCREEN_WIDTH;

        public static float VIEW_SCREEN_WIDTH = 480f;

        public static float VIEW_SCREEN_HEIGHT = 800f;

        public static float VIEW_OFFSET_X = 0f;

        public static float VIEW_OFFSET_Y = 0f;

        public static float SCREEN_RATIO;

        public static float PORTRAIT_SCREEN_WIDTH = 480f;

        public static float PORTRAIT_SCREEN_HEIGHT = 320f;

        public static bool IS_IPAD = false;

        public static bool IS_RETINA = false;

        public static bool IS_WVGA = false;

        public static bool IS_QVGA = false;

        public class FlurryAPI
        {
            public static void logEvent(NSString s)
            {
            }
        }

        public class AndroidAPI
        {
            public static void openUrl(NSString url)
            {
                openUrl(url.ToString());
            }

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

            public static void showBanner()
            {
            }

            public static void showVideoBanner()
            {
            }

            public static void hideBanner()
            {
            }

            public static void disableBanners()
            {
            }

            public static void exitApp()
            {
                Global.XnaGame.Exit();
            }
        }
    }
}
