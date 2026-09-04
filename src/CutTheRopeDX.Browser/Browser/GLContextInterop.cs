using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;

namespace CutTheRopeDX.Browser
{
    /// <summary>Thin managed wrapper over the glcontext.js WebGL2 module.</summary>
    internal static partial class GLContextInterop
    {
        /// <summary>Imports glcontext.js. Must be awaited once before any other call.</summary>
        public static Task ImportAsync()
        {
            return JSHost.ImportAsync("glcontext", "../glcontext.js");
        }

        /// <summary>
        /// Hands the page canvas to the managed owner thread's worker and returns
        /// <c>[cssWidth, cssHeight, backingWidth, backingHeight]</c>, or an empty
        /// array when the transfer failed.
        /// </summary>
        /// <remarks>
        /// Transfer is permanent. Nothing may fall back to browser-thread rendering
        /// after this succeeds, so every check that can fail runs before it.
        /// </remarks>
        [JSImport("transferCanvasToThread", "glcontext")]
        public static partial int[] TransferCanvasToThread(string canvasId, int threadId);

        /// <summary>Returns the canvas backing size during legacy resize polling.</summary>
        [JSImport("canvasSize", "glcontext")]
        public static partial int[] CanvasSize(string canvasId);

        /// <summary>
        /// Starts reporting canvas shape changes through <see cref="CanvasChangeCount"/>, so the
        /// loop no longer has to measure the DOM to find out whether anything moved.
        /// </summary>
        [JSImport("watchCanvas", "glcontext")]
        public static partial void WatchCanvas(string canvasId);

        /// <summary>
        /// A counter the watcher bumps whenever the canvas backing size or the device pixel ratio
        /// changes. Equal values mean neither has moved since the last look.
        /// </summary>
        [JSImport("canvasChangeCount", "glcontext")]
        public static partial int CanvasChangeCount();

        /// <summary>
        /// Returns the device pixel ratio the last <see cref="CanvasSize"/> call applied to the
        /// canvas backing store. Call it after <see cref="CanvasSize"/> so the two describe the
        /// same measurement.
        /// </summary>
        /// <returns>Physical pixels per logical pixel, clamped to at most 2.</returns>
        [JSImport("canvasDevicePixelRatio", "glcontext")]
        public static partial double CanvasDevicePixelRatio();

        /// <summary>Returns the document base URL for resolving app-relative resources.</summary>
        [JSImport("documentBaseUrl", "glcontext")]
        public static partial string DocumentBaseUrl();
    }
}
