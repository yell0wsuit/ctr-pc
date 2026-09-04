using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;

namespace CutTheRopeDX.Browser
{
    /// <summary>Typed managed boundary for the opt-in render-probe JavaScript module.</summary>
    internal static partial class RenderProbeInterop
    {
        /// <summary>Imports render-probe.js. Must be awaited once before any other call.</summary>
        public static Task ImportAsync()
        {
            return JSHost.ImportAsync("renderprobe", "../render-probe.js");
        }

        /// <summary>Returns whether the page requested the render probe.</summary>
        [JSImport("isRequested", "renderprobe")]
        public static partial bool IsRequested();

        /// <summary>Returns whether managed code is executing on a window or worker global.</summary>
        [JSImport("executionContext", "renderprobe")]
        public static partial string ExecutionContext();

        /// <summary>Checks the deterministic probe pixel and WebGL error values.</summary>
        [JSImport("isExpectedPixel", "renderprobe")]
        public static partial bool IsExpectedPixel(int[] values);
    }
}
