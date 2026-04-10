using System;
using System.ComponentModel;
using System.Diagnostics;

using CutTheRopeDX.Desktop;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Platform;

namespace CutTheRopeDX.Framework
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

        /// <summary>
        /// Releases resources. Override in derived classes to free owned resources.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> when called from <see cref="Dispose()"/>; <see langword="false"/> from finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
        }

        /// <summary>
        /// Gets the shared <see cref="GLCanvas"/> instance from the application.
        /// </summary>
        public static GLCanvas Canvas => Application.SharedCanvas();

        /// <summary>
        /// Converts an array of <see cref="Quad2D"/> into a flat float array.
        /// </summary>
        /// <param name="quads">Quads to convert.</param>
        /// <returns>A flat float array containing 8 floats per quad.</returns>
        public static float[] ToFloatArray(Quad2D[] quads)
        {
            float[] array = new float[quads.Length * 8];
            for (int i = 0; i < quads.Length; i++)
            {
                quads[i].ToFloatArray().CopyTo(array, i * 8);
            }
            return array;
        }

        /// <summary>
        /// Converts an array of <see cref="Quad3D"/> into a flat float array.
        /// </summary>
        /// <param name="quads">Quads to convert.</param>
        /// <returns>A flat float array containing 12 floats per quad.</returns>
        public static float[] ToFloatArray(Quad3D[] quads)
        {
            float[] array = new float[quads.Length * 12];
            for (int i = 0; i < quads.Length; i++)
            {
                quads[i].ToFloatArray().CopyTo(array, i * 12);
            }
            return array;
        }

        /// <summary>
        /// Creates a <see cref="CTRRectangle"/> from position and size.
        /// </summary>
        /// <param name="xParam">X position.</param>
        /// <param name="yParam">Y position.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        /// <returns>A new <see cref="CTRRectangle"/> with the given position and size.</returns>
        public static CTRRectangle MakeRectangle(float xParam, float yParam, float width, float height)
        {
            return new CTRRectangle(xParam, yParam, width, height);
        }

        /// <summary>
        /// Transforms a logical X coordinate to a real screen X coordinate.
        /// </summary>
        /// <param name="x">Logical X coordinate.</param>
        /// <returns>The corresponding real screen X coordinate.</returns>
        public static float TransformToRealX(float x)
        {
            return (x * VIEW_SCREEN_WIDTH / SCREEN_WIDTH) + VIEW_OFFSET_X;
        }

        /// <summary>
        /// Transforms a logical Y coordinate to a real screen Y coordinate.
        /// </summary>
        /// <param name="y">Logical Y coordinate.</param>
        /// <returns>The corresponding real screen Y coordinate.</returns>
        public static float TransformToRealY(float y)
        {
            return (y * VIEW_SCREEN_HEIGHT / SCREEN_HEIGHT) + VIEW_OFFSET_Y;
        }

        /// <summary>
        /// Transforms a real screen X coordinate to a logical X coordinate.
        /// </summary>
        /// <param name="x">Real screen X coordinate.</param>
        /// <returns>The corresponding logical X coordinate.</returns>
        public static float TransformFromRealX(float x)
        {
            return (x - VIEW_OFFSET_X) * SCREEN_WIDTH / VIEW_SCREEN_WIDTH;
        }

        /// <summary>
        /// Transforms a real screen Y coordinate to a logical Y coordinate.
        /// </summary>
        /// <param name="y">Real screen Y coordinate.</param>
        /// <returns>The corresponding logical Y coordinate.</returns>
        public static float TransformFromRealY(float y)
        {
            return (y - VIEW_OFFSET_Y) * SCREEN_HEIGHT / VIEW_SCREEN_HEIGHT;
        }

        /// <summary>
        /// Transforms a logical width to a real screen width.
        /// </summary>
        /// <param name="w">Logical width.</param>
        /// <returns>The corresponding real screen width.</returns>
        public static float TransformToRealW(float w)
        {
            return w * VIEW_SCREEN_WIDTH / SCREEN_WIDTH;
        }

        /// <summary>
        /// Transforms a logical height to a real screen height.
        /// </summary>
        /// <param name="h">Logical height.</param>
        /// <returns>The corresponding real screen height.</returns>
        public static float TransformToRealH(float h)
        {
            return h * VIEW_SCREEN_HEIGHT / SCREEN_HEIGHT;
        }

        /// <summary>
        /// Transforms a real screen width to a logical width.
        /// </summary>
        /// <param name="w">Real screen width.</param>
        /// <returns>The corresponding logical width.</returns>
        public static float TransformFromRealW(float w)
        {
            return w * SCREEN_WIDTH / VIEW_SCREEN_WIDTH;
        }

        /// <summary>
        /// Transforms a real screen height to a logical height.
        /// </summary>
        /// <param name="h">Real screen height.</param>
        /// <returns>The corresponding logical height.</returns>
        public static float TransformFromRealH(float h)
        {
            return h * SCREEN_HEIGHT / VIEW_SCREEN_HEIGHT;
        }

        /// <summary>
        /// Returns the achievement identifier string unchanged (pass-through).
        /// </summary>
        /// <param name="s">Achievement identifier string.</param>
        /// <returns>The same string passed in.</returns>
        public static string ACHIEVEMENT_STRING(string s)
        {
            return s;
        }

        /// <summary>
        /// No-op logging stub.
        /// </summary>
        public static void LOG()
        {
        }

        /// <summary>
        /// Returns <paramref name="H"/> on WVGA displays, <paramref name="L"/> otherwise.
        /// </summary>
        /// <param name="H">Value for WVGA resolution.</param>
        /// <param name="L">Value for non-WVGA resolution.</param>
        /// <returns><paramref name="H"/> when running at WVGA; otherwise <paramref name="L"/>.</returns>
        public static float WVGAH(float H, float L)
        {
            return IS_WVGA ? H : L;
        }

        /// <summary>
        /// Doubles <paramref name="V"/> on WVGA displays; returns it unchanged otherwise.
        /// </summary>
        /// <param name="V">Value to scale.</param>
        /// <returns><c>V * 2</c> on WVGA; otherwise <paramref name="V"/>.</returns>
        public static float WVGAD(float V)
        {
            return IS_WVGA ? V * 2 : V;
        }

        /// <summary>
        /// Returns <paramref name="H"/> on retina displays, <paramref name="L"/> otherwise.
        /// </summary>
        /// <param name="H">Value for retina resolution.</param>
        /// <param name="L">Value for non-retina resolution.</param>
        /// <returns><paramref name="H"/> on retina displays; otherwise <paramref name="L"/>.</returns>
        public static float RT(float H, float L)
        {
            return IS_RETINA ? H : L;
        }

        /// <summary>
        /// Doubles <paramref name="V"/> on retina displays; returns it unchanged otherwise.
        /// </summary>
        /// <param name="V">Value to scale.</param>
        /// <returns><c>V * 2</c> on retina displays; otherwise <paramref name="V"/>.</returns>
        public static float RTD(float V)
        {
            return IS_RETINA ? V * 2 : V;
        }

        /// <summary>
        /// Doubles <paramref name="V"/> on retina or iPad displays; returns it unchanged otherwise.
        /// </summary>
        /// <param name="V">Value to scale.</param>
        /// <returns><c>V * 2</c> on retina or iPad displays; otherwise <paramref name="V"/>.</returns>
        public static float RTPD(float V)
        {
            return IS_RETINA | IS_IPAD ? V * 2 : V;
        }

        /// <summary>
        /// Returns the WVGA or non-WVGA value via <see cref="WVGAH"/>.
        /// </summary>
        /// <param name="P1">Value for non-WVGA resolution.</param>
        /// <param name="P2">Value for WVGA resolution.</param>
        /// <returns><paramref name="P2"/> on WVGA; otherwise <paramref name="P1"/>.</returns>
        public static float CHOOSE3(float P1, float P2)
        {
            return WVGAH(P2, P1);
        }

        /// <summary>
        /// Blending mode: source alpha.
        /// </summary>
        public const int BLENDING_MODE_SRC_ALPHA = 0;

        /// <summary>
        /// Blending mode: one (premultiplied alpha).
        /// </summary>
        public const int BLENDING_MODE_ONE = 1;

        /// <summary>
        /// Blending mode: additive.
        /// </summary>
        public const int BLENDING_MODE_ADDITIVE = 2;

        /// <summary>
        /// Sentinel value indicating an undefined or unset parameter.
        /// </summary>
        public const int UNDEFINED = -1;

        /// <summary>
        /// Epsilon used for floating-point equality comparisons.
        /// </summary>
        public const float FLOAT_PRECISION = 1E-06f;

        /// <summary>
        /// Horizontal alignment flag: left.
        /// </summary>
        public const int LEFT = 1;

        /// <summary>
        /// Horizontal alignment flag: center.
        /// </summary>
        public const int HCENTER = 2;

        /// <summary>
        /// Horizontal alignment flag: right.
        /// </summary>
        public const int RIGHT = 4;

        /// <summary>
        /// Vertical alignment flag: top.
        /// </summary>
        public const int TOP = 8;

        /// <summary>
        /// Vertical alignment flag: center.
        /// </summary>
        public const int VCENTER = 16;

        /// <summary>
        /// Vertical alignment flag: bottom.
        /// </summary>
        public const int BOTTOM = 32;

        /// <summary>
        /// Combined alignment: horizontal center | vertical center.
        /// </summary>
        public const int CENTER = 18;

        /// <summary>
        /// OpenGL color buffer bit constant.
        /// </summary>
        public const int GL_COLOR_BUFFER_BIT = 0;

        /// <summary>
        /// Logical screen width in game coordinates.
        /// </summary>
        public static float SCREEN_WIDTH = 320f;

        /// <summary>
        /// Logical screen height in game coordinates.
        /// </summary>
        public static float SCREEN_HEIGHT = 480f;

        /// <summary>
        /// Actual device screen width in pixels.
        /// </summary>
        public static float REAL_SCREEN_WIDTH = 480f;

        /// <summary>
        /// Actual device screen height in pixels.
        /// </summary>
        public static float REAL_SCREEN_HEIGHT = 800f;

        /// <summary>
        /// Vertical offset applied when the screen is letterboxed.
        /// </summary>
        public static float SCREEN_OFFSET_Y;

        /// <summary>
        /// Horizontal offset applied when the screen is pillarboxed.
        /// </summary>
        public static float SCREEN_OFFSET_X;

        /// <summary>
        /// Vertical scale factor for background images.
        /// </summary>
        public static float SCREEN_BG_SCALE_Y = 1f;

        /// <summary>
        /// Horizontal scale factor for background images.
        /// </summary>
        public static float SCREEN_BG_SCALE_X = 1f;

        /// <summary>
        /// Vertical scale factor for wide background images.
        /// </summary>
        public static float SCREEN_WIDE_BG_SCALE_Y = 1f;

        /// <summary>
        /// Horizontal scale factor for wide background images.
        /// </summary>
        public static float SCREEN_WIDE_BG_SCALE_X = 1f;

        /// <summary>
        /// Expanded logical screen height (accounts for aspect-ratio adjustments).
        /// </summary>
        public static float SCREEN_HEIGHT_EXPANDED = SCREEN_HEIGHT;

        /// <summary>
        /// Expanded logical screen width (accounts for aspect-ratio adjustments).
        /// </summary>
        public static float SCREEN_WIDTH_EXPANDED = SCREEN_WIDTH;

        /// <summary>
        /// Viewport width used for coordinate transforms.
        /// </summary>
        public static float VIEW_SCREEN_WIDTH = 480f;

        /// <summary>
        /// Viewport height used for coordinate transforms.
        /// </summary>
        public static float VIEW_SCREEN_HEIGHT = 800f;

        /// <summary>
        /// Horizontal viewport offset.
        /// </summary>
        public static float VIEW_OFFSET_X;

        /// <summary>
        /// Vertical viewport offset.
        /// </summary>
        public static float VIEW_OFFSET_Y;

        /// <summary>
        /// Current screen aspect ratio.
        /// </summary>
        public static float SCREEN_RATIO;

        /// <summary>
        /// Portrait-mode screen width.
        /// </summary>
        public static float PORTRAIT_SCREEN_WIDTH = 480f;

        /// <summary>
        /// Portrait-mode screen height.
        /// </summary>
        public static float PORTRAIT_SCREEN_HEIGHT = 320f;

        /// <summary>
        /// <see langword="true"/> when running at iPad resolution.
        /// </summary>
        public static bool IS_IPAD;

        /// <summary>
        /// <see langword="true"/> when running on a retina (2x) display.
        /// </summary>
        public static bool IS_RETINA;

        /// <summary>
        /// <see langword="true"/> when running at WVGA (800x480) resolution.
        /// </summary>
        public static bool IS_WVGA;

        /// <summary>
        /// <see langword="true"/> when running at QVGA (320x240) resolution.
        /// </summary>
        public static bool IS_QVGA;

        /// <summary>
        /// Stub API surface retained from the original analytics integration.
        /// </summary>
        public sealed class FlurryAPI
        {
            /// <summary>
            /// No-op: Log an analytics event.
            /// </summary>
            public static void LogEvent()
            {
            }
        }

        /// <summary>
        /// Opens the specified URL in the default system browser.
        /// </summary>
        /// <param name="url">URL to open.</param>
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

        /// <summary>
        /// Stub API surface retained from the original Android version.
        /// </summary>
        public sealed class AndroidAPI
        {
            /// <summary>
            /// No-op: Display a banner ad.
            /// </summary>
            public static void ShowBanner()
            {
            }

            /// <summary>
            /// No-op: Display a video banner ad.
            /// </summary>
            public static void ShowVideoBanner()
            {
            }

            /// <summary>
            /// No-op: Hide the banner ad.
            /// </summary>
            public static void HideBanner()
            {
            }

            /// <summary>
            /// No-op: Disable all banner ads.
            /// </summary>
            public static void DisableBanners()
            {
            }

            /// <summary>
            /// Exits the application.
            /// </summary>
            public static void ExitApp()
            {
                Global.XnaGame.Exit();
            }
        }
    }
}
