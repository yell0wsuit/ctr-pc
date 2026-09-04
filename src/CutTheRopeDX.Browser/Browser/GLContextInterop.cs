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

        /// <summary>Starts reporting canvas shape changes through the host event ring.</summary>
        [JSImport("watchCanvas", "glcontext")]
        public static partial void WatchCanvas(string canvasId);

        /// <summary>Returns the document base URL for resolving app-relative resources.</summary>
        [JSImport("documentBaseUrl", "glcontext")]
        public static partial string DocumentBaseUrl();
    }
}
