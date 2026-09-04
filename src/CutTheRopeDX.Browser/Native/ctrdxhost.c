// Bodies are EM_ASM because it runs in the JavaScript scope of the calling
// thread. The runtime proxies [JSImport] to the browser thread, so managed
// interop cannot reach the owner thread's own scope, and the owner thread is
// where the WebGL context and the animation frame have to live.

#include <emscripten.h>
#include <emscripten/threading.h>
#include <pthread.h>
#include <stdint.h>
#include <stdlib.h>

static void (*frame_callback)(double) = NULL;
static void *event_buffer = NULL;

EMSCRIPTEN_KEEPALIVE
int ctrdx_thread_id(void)
{
    return (int)(intptr_t)pthread_self();
}

EMSCRIPTEN_KEEPALIVE
int ctrdx_is_main_runtime_thread(void)
{
    return emscripten_is_main_runtime_thread();
}

EMSCRIPTEN_KEEPALIVE
void ctrdx_set_frame_callback(void (*callback)(double))
{
    frame_callback = callback;
}

EMSCRIPTEN_KEEPALIVE
void ctrdx_frame_entry(double timestamp)
{
    if (frame_callback != NULL)
    {
        frame_callback(timestamp);
    }
}

EMSCRIPTEN_KEEPALIVE
void ctrdx_request_frame(void)
{
    EM_ASM({
        var token = (globalThis.ctrdxFrameToken | 0) + 1;
        globalThis.ctrdxFrameToken = token;
        requestAnimationFrame(function (timestamp) {
            if (globalThis.ctrdxFrameToken !== token) {
                return;
            }
            _ctrdx_frame_entry(timestamp);
        });
    });
}

// Coexists with the runtime worker's own onmessage. Messages here carry no `cmd`
// field on purpose: that handler ends in `else if (e.data.cmd)` and reports any
// command it does not recognize twice, so a `cmd` would make every wake noisy.
EMSCRIPTEN_KEEPALIVE
int ctrdx_install_canvas_listener(void)
{
    return EM_ASM_INT({
        if (globalThis.ctrdxCanvasListener) {
            return 1;
        }
        globalThis.ctrdxCanvasListener = true;
        globalThis.ctrdxCanvas = null;
        addEventListener('message', function (event) {
            var data = event.data;
            if (data && data.ctrdxTransferCanvas) {
                globalThis.ctrdxCanvas = data.ctrdxTransferCanvas;
            } else if (data && data.ctrdxWake) {
                // Invalidate the animation frame that may have been suspended when
                // the page became hidden, then process lifecycle state immediately.
                globalThis.ctrdxFrameToken = (globalThis.ctrdxFrameToken | 0) + 1;
                _ctrdx_frame_entry(performance.now());
            }
        });
        return 1;
    });
}

EMSCRIPTEN_KEEPALIVE
int ctrdx_canvas_received(void)
{
    return EM_ASM_INT({
        return globalThis.ctrdxCanvas ? 1 : 0;
    });
}

EMSCRIPTEN_KEEPALIVE
int ctrdx_create_worker_context(int width, int height)
{
    return EM_ASM_INT({
        var surface = globalThis.ctrdxCanvas;
        if (!surface) {
            return 0;
        }
        surface.width = $0;
        surface.height = $1;
        var context = surface.getContext('webgl2', {
            alpha: true,
            depth: true,
            stencil: true,
            antialias: false,
            premultipliedAlpha: true,
            preserveDrawingBuffer: false
        });
        if (!context) {
            return 0;
        }
        var handle = GL.registerContext(context, {
            majorVersion: 2,
            minorVersion: 0,
            enableExtensionsByDefault: 1,
            alpha: 1,
            depth: 1,
            stencil: 8,
            antialias: 0,
            premultipliedAlpha: 1,
            preserveDrawingBuffer: 0
        });
        if (!handle) {
            return 0;
        }
        GL.makeContextCurrent(handle);
        globalThis.ctrdxContextLost = 0;
        surface.addEventListener('webglcontextlost', function (event) {
            event.preventDefault();
            globalThis.ctrdxContextLost = 1;
            postMessage({ ctrdxContextLost: 1 });
        });
        return handle;
    }, width, height);
}

EMSCRIPTEN_KEEPALIVE
int ctrdx_resize_canvas(int width, int height)
{
    return EM_ASM_INT({
        var surface = globalThis.ctrdxCanvas;
        if (!surface) {
            return 0;
        }
        surface.width = $0;
        surface.height = $1;
        return 1;
    }, width, height);
}

EMSCRIPTEN_KEEPALIVE
int ctrdx_context_lost(void)
{
    return EM_ASM_INT({
        return globalThis.ctrdxContextLost | 0;
    });
}

EMSCRIPTEN_KEEPALIVE
void *ctrdx_event_buffer(int bytes)
{
    if (event_buffer == NULL)
    {
        event_buffer = calloc(1, (size_t)bytes);
    }
    return event_buffer;
}
