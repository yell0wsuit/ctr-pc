using System;
using System.ComponentModel;
using System.Diagnostics;

using CutTheRope.Desktop;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Platform;

namespace CutTheRope.Framework
{
    /// <summary>
    /// Base class for most framework types, providing screen-coordinate transforms,
    /// resolution helpers, and the disposable pattern.
    /// </summary>
    internal class FrameworkTypes : CTRMathHelper, IDisposable
    {
        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~FrameworkTypes()
        {
            Dispose(false);
        }

        /// <summary>Releases resources. Override in derived classes to free owned resources.</summary>
        /// <param name="disposing">True when called from <see cref="Dispose()"/>; false from finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
        }

        /// <summary>Gets the shared <see cref="GLCanvas"/> instance from the application.</summary>
        public static GLCanvas Canvas => Application.SharedCanvas();

        public static float[] ToFloatArray(Quad2D[] quads)
        {
            float[] array = new float[quads.Length * 8];
            for (int i = 0; i < quads.Length; i++)
            {
                quads[i].ToFloatArray().CopyTo(array, i * 8);
            }
            return array;
        }

        public static float[] ToFloatArray(Quad3D[] quads)
        {
            float[] array = new float[quads.Length * 12];
            for (int i = 0; i < quads.Length; i++)
            {
                quads[i].ToFloatArray().CopyTo(array, i * 12);
            }
            return array;
        }

        /// <summary>Creates a <see cref="CTRRectangle"/> from position and size.</summary>
        public static CTRRectangle MakeRectangle(float xParam, float yParam, float width, float height)
        {
            return new CTRRectangle(xParam, yParam, width, height);
        }

        public static float TransformToRealX(float x)
        {
            return (x * VIEW_SCREEN_WIDTH / SCREEN_WIDTH) + VIEW_OFFSET_X;
        }

        public static float TransformToRealY(float y)
        {
            return (y * VIEW_SCREEN_HEIGHT / SCREEN_HEIGHT) + VIEW_OFFSET_Y;
        }

        public static float TransformFromRealX(float x)
        {
            return (x - VIEW_OFFSET_X) * SCREEN_WIDTH / VIEW_SCREEN_WIDTH;
        }

        public static float TransformFromRealY(float y)
        {
            return (y - VIEW_OFFSET_Y) * SCREEN_HEIGHT / VIEW_SCREEN_HEIGHT;
        }

        public static float TransformToRealW(float w)
        {
            return w * VIEW_SCREEN_WIDTH / SCREEN_WIDTH;
        }

        public static float TransformToRealH(float h)
        {
            return h * VIEW_SCREEN_HEIGHT / SCREEN_HEIGHT;
        }

        public static float TransformFromRealW(float w)
        {
            return w * SCREEN_WIDTH / VIEW_SCREEN_WIDTH;
        }

        public static float TransformFromRealH(float h)
        {
            return h * SCREEN_HEIGHT / VIEW_SCREEN_HEIGHT;
        }

        /// <summary>Returns the achievement identifier string unchanged (pass-through).</summary>
        public static string ACHIEVEMENT_STRING(string s)
        {
            return s;
        }

        /// <summary>No-op logging stub.</summary>
        public static void LOG()
        {
        }

        public static float WVGAH(float H, float L)
        {
            return IS_WVGA ? H : L;
        }

        public static float WVGAD(float V)
        {
            return IS_WVGA ? V * 2 : V;
        }

        /// <summary>Returns <paramref name="H"/> on retina displays, <paramref name="L"/> otherwise.</summary>
        public static float RT(float H, float L)
        {
            return IS_RETINA ? H : L;
        }

        /// <summary>Doubles <paramref name="V"/> on retina displays; returns it unchanged otherwise.</summary>
        public static float RTD(float V)
        {
            return IS_RETINA ? V * 2 : V;
        }

        /// <summary>Doubles <paramref name="V"/> on retina or iPad displays; returns it unchanged otherwise.</summary>
        public static float RTPD(float V)
        {
            return IS_RETINA | IS_IPAD ? V * 2 : V;
        }

        public static float CHOOSE3(float P1, float P2)
        {
            return WVGAH(P2, P1);
        }

        public const int BLENDING_MODE_SRC_ALPHA = 0;

        public const int BLENDING_MODE_ONE = 1;

        public const int BLENDING_MODE_ADDITIVE = 2;

        public const int UNDEFINED = -1;

        /// <summary>Epsilon used for floating-point equality comparisons.</summary>
        public const float FLOAT_PRECISION = 1E-06f;

        /// <summary>Horizontal alignment flag: left.</summary>
        public const int LEFT = 1;

        /// <summary>Horizontal alignment flag: center.</summary>
        public const int HCENTER = 2;

        /// <summary>Horizontal alignment flag: right.</summary>
        public const int RIGHT = 4;

        /// <summary>Vertical alignment flag: top.</summary>
        public const int TOP = 8;

        /// <summary>Vertical alignment flag: center.</summary>
        public const int VCENTER = 16;

        /// <summary>Vertical alignment flag: bottom.</summary>
        public const int BOTTOM = 32;

        /// <summary>Combined alignment: horizontal center | vertical center.</summary>
        public const int CENTER = 18;

        public const int GL_COLOR_BUFFER_BIT = 0;

        /// <summary>Logical screen width in game coordinates.</summary>
        public static float SCREEN_WIDTH = 320f;

        /// <summary>Logical screen height in game coordinates.</summary>
        public static float SCREEN_HEIGHT = 480f;

        /// <summary>Actual device screen width in pixels.</summary>
        public static float REAL_SCREEN_WIDTH = 480f;

        /// <summary>Actual device screen height in pixels.</summary>
        public static float REAL_SCREEN_HEIGHT = 800f;

        /// <summary>Vertical offset applied when the screen is letterboxed.</summary>
        public static float SCREEN_OFFSET_Y;

        /// <summary>Horizontal offset applied when the screen is pillarboxed.</summary>
        public static float SCREEN_OFFSET_X;

        /// <summary>Vertical scale factor for background images.</summary>
        public static float SCREEN_BG_SCALE_Y = 1f;

        /// <summary>Horizontal scale factor for background images.</summary>
        public static float SCREEN_BG_SCALE_X = 1f;

        /// <summary>Vertical scale factor for wide background images.</summary>
        public static float SCREEN_WIDE_BG_SCALE_Y = 1f;

        /// <summary>Horizontal scale factor for wide background images.</summary>
        public static float SCREEN_WIDE_BG_SCALE_X = 1f;

        /// <summary>Expanded logical screen height (accounts for aspect-ratio adjustments).</summary>
        public static float SCREEN_HEIGHT_EXPANDED = SCREEN_HEIGHT;

        /// <summary>Expanded logical screen width (accounts for aspect-ratio adjustments).</summary>
        public static float SCREEN_WIDTH_EXPANDED = SCREEN_WIDTH;

        /// <summary>Viewport width used for coordinate transforms.</summary>
        public static float VIEW_SCREEN_WIDTH = 480f;

        /// <summary>Viewport height used for coordinate transforms.</summary>
        public static float VIEW_SCREEN_HEIGHT = 800f;

        /// <summary>Horizontal viewport offset.</summary>
        public static float VIEW_OFFSET_X;

        /// <summary>Vertical viewport offset.</summary>
        public static float VIEW_OFFSET_Y;

        /// <summary>Current screen aspect ratio.</summary>
        public static float SCREEN_RATIO;

        /// <summary>Portrait-mode screen width.</summary>
        public static float PORTRAIT_SCREEN_WIDTH = 480f;

        /// <summary>Portrait-mode screen height.</summary>
        public static float PORTRAIT_SCREEN_HEIGHT = 320f;

        /// <summary>True when running at iPad resolution.</summary>
        public static bool IS_IPAD;

        /// <summary>True when running on a retina (2x) display.</summary>
        public static bool IS_RETINA;

        /// <summary>True when running at WVGA (800x480) resolution.</summary>
        public static bool IS_WVGA;

        /// <summary>True when running at QVGA (320x240) resolution.</summary>
        public static bool IS_QVGA;

        public sealed class FlurryAPI
        {
            public static void LogEvent()
            {
            }
        }

        /// <summary>Opens the specified URL in the default system browser.</summary>
        public static void OpenUrl(string url)
        {
            try
            {
                ProcessStartInfo psi = new()
                {
                    FileName = url,
                    UseShellExecute = true
                };
                _ = Process.Start(psi);
            }
            catch (Win32Exception ex)
            {
                int errorCode = ex.ErrorCode;
            }
            catch (Exception)
            {
            }
        }

        /// <summary>Stub API surface retained from the original Android version.</summary>
        public sealed class AndroidAPI
        {
            /// <summary>No-op: Display a banner ad.</summary>
            public static void ShowBanner()
            {
            }

            public static void ShowVideoBanner()
            {
            }

            public static void HideBanner()
            {
            }

            /// <summary>No-op: Disable all banner ads.</summary>
            public static void DisableBanners()
            {
            }

            public static void ExitApp()
            {
                Global.XnaGame.Exit();
            }
        }
    }
}
