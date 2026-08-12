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

        /// <summary>Creates the WebGL2 context on the canvas and returns its framebuffer id.</summary>
        [JSImport("createContext", "glcontext")]
        public static partial int CreateContext(string canvasId);

        /// <summary>Returns the canvas backing size as [width, height], resizing it to the DPR-capped CSS size.</summary>
        [JSImport("canvasSize", "glcontext")]
        public static partial int[] CanvasSize(string canvasId);

        /// <summary>Returns the document base URL for resolving app-relative resources.</summary>
        [JSImport("documentBaseUrl", "glcontext")]
        public static partial string DocumentBaseUrl();
    }
}
