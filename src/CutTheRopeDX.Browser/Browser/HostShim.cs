using System.Runtime.InteropServices;

namespace CutTheRopeDX.Browser
{
    /// <summary>
    /// Native boundary that executes in the calling thread's own JavaScript scope,
    /// which managed interop cannot reach because it is proxied to the browser thread.
    /// </summary>
    internal static unsafe partial class HostShim
    {
        private const string Library = "ctrdxhost";

        /// <summary>Returns the calling thread's pthread pointer.</summary>
        [LibraryImport(Library, EntryPoint = "ctrdx_thread_id")]
        internal static partial int ThreadId();

        /// <summary>Returns whether the caller is the browser runtime thread.</summary>
        [LibraryImport(Library, EntryPoint = "ctrdx_is_main_runtime_thread")]
        internal static partial int IsMainRuntimeThread();

        /// <summary>Registers the function each animation frame invokes.</summary>
        [LibraryImport(Library, EntryPoint = "ctrdx_set_frame_callback")]
        internal static partial void SetFrameCallback(
            delegate* unmanaged<double, void> callback);

        /// <summary>Schedules one animation frame on this thread.</summary>
        [LibraryImport(Library, EntryPoint = "ctrdx_request_frame")]
        internal static partial void RequestFrame();

        /// <summary>Starts listening for the transferred canvas on this thread.</summary>
        [LibraryImport(Library, EntryPoint = "ctrdx_install_canvas_listener")]
        internal static partial int InstallCanvasListener();

        /// <summary>Returns whether the transferred canvas has arrived.</summary>
        [LibraryImport(Library, EntryPoint = "ctrdx_canvas_received")]
        internal static partial int CanvasReceived();

        /// <summary>Registers and makes current a WebGL2 context this thread owns.</summary>
        [LibraryImport(Library, EntryPoint = "ctrdx_create_worker_context")]
        internal static partial int CreateWorkerContext(int width, int height);

        /// <summary>Resizes the transferred canvas backing store.</summary>
        [LibraryImport(Library, EntryPoint = "ctrdx_resize_canvas")]
        internal static partial int ResizeCanvas(int width, int height);

        /// <summary>Returns whether this thread's WebGL context has been lost.</summary>
        [LibraryImport(Library, EntryPoint = "ctrdx_context_lost")]
        internal static partial int ContextLost();

        /// <summary>
        /// Returns the shared event buffer, allocating it on first use. Its address is
        /// stable for the process, which is what lets the browser thread keep writing to
        /// it across a memory growth that replaces every typed-array view.
        /// </summary>
        [LibraryImport(Library, EntryPoint = "ctrdx_event_buffer")]
        internal static partial nint EventBuffer(int bytes);
    }
}
