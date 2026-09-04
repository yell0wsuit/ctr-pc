using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;

namespace CutTheRopeDX.Browser
{
    /// <summary>Typed managed boundary for the browser thread's event writer.</summary>
    internal static partial class HostEventInterop
    {
        /// <summary>Imports host-events.js. Must be awaited once before any other call.</summary>
        public static Task ImportAsync()
        {
            return JSHost.ImportAsync("hostevents", "../host-events.js");
        }

        /// <summary>Points the browser thread's writer at the shared ring.</summary>
        [JSImport("attach", "hostevents")]
        public static partial void Attach(int address, int threadId);
    }
}
