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
    }
}
