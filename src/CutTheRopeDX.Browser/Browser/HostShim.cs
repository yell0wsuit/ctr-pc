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

        /// <summary>Returns whether this thread's scope exposes animation frames.</summary>
        [LibraryImport(Library, EntryPoint = "ctrdx_supports_animation_frame")]
        internal static partial int SupportsAnimationFrame();

        /// <summary>Registers the function each animation frame invokes.</summary>
        [LibraryImport(Library, EntryPoint = "ctrdx_set_frame_callback")]
        internal static partial void SetFrameCallback(
            delegate* unmanaged<double, void> callback);

        /// <summary>Schedules one animation frame on this thread.</summary>
        [LibraryImport(Library, EntryPoint = "ctrdx_request_frame")]
        internal static partial void RequestFrame();

        /// <summary>Returns how many times the frame entry point has run.</summary>
        [LibraryImport(Library, EntryPoint = "ctrdx_frame_callback_hits")]
        internal static partial int FrameCallbackHits();

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

        /// <summary>Returns whether the current context exposes a usable GL object.</summary>
        [LibraryImport(Library, EntryPoint = "ctrdx_context_usable")]
        internal static partial int ContextUsable();

        /// <summary>Drains stale errors from the current context.</summary>
        [LibraryImport(Library, EntryPoint = "ctrdx_clear_gl_errors")]
        internal static partial int ClearGlErrors();

        /// <summary>Reads the center pixel packed as 0xRRGGBBAA, or -1 when unavailable.</summary>
        [LibraryImport(Library, EntryPoint = "ctrdx_read_center_pixel")]
        internal static partial int ReadCenterPixel(int width, int height);

        /// <summary>Returns the error captured by the last readback.</summary>
        [LibraryImport(Library, EntryPoint = "ctrdx_last_gl_error")]
        internal static partial int LastGlError();
    }
}
