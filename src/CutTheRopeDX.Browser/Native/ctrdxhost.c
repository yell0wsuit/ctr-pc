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
static int frame_callback_hits = 0;
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
int ctrdx_supports_animation_frame(void)
{
    return EM_ASM_INT({
        return typeof requestAnimationFrame === 'function' ? 1 : 0;
    });
}

EMSCRIPTEN_KEEPALIVE
void ctrdx_set_frame_callback(void (*callback)(double))
{
    frame_callback = callback;
}

EMSCRIPTEN_KEEPALIVE
void ctrdx_frame_entry(double timestamp)
{
    frame_callback_hits++;
    if (frame_callback != NULL)
    {
        frame_callback(timestamp);
    }
}

EMSCRIPTEN_KEEPALIVE
int ctrdx_frame_callback_hits(void)
{
    return frame_callback_hits;
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

// Coexists with the runtime worker's own onmessage, which logs unrecognized
// commands rather than throwing.
EMSCRIPTEN_KEEPALIVE
int ctrdx_install_canvas_listener(void)
{
    return EM_ASM_INT({
        if (globalThis.ctrdxCanvasListener) {
            return 1;
        }
        globalThis.ctrdxCanvasListener = true;
        globalThis.ctrdxCanvas = null;
        globalThis.ctrdxLastGlError = 0;
        addEventListener('message', function (event) {
            var data = event.data;
            if (data && data.cmd === 'ctrdx-transfer-canvas') {
                globalThis.ctrdxCanvas = data.canvas;
            } else if (data && data.cmd === 'ctrdx-host-wake') {
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
            postMessage({ cmd: 'ctrdx-context-lost' });
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
int ctrdx_context_usable(void)
{
    return EM_ASM_INT({
        var current = GL.currentContext;
        return (current && current.GLctx &&
            typeof current.GLctx.readPixels === 'function') ? 1 : 0;
    });
}

EMSCRIPTEN_KEEPALIVE
int ctrdx_clear_gl_errors(void)
{
    return EM_ASM_INT({
        var gl = GL.currentContext && GL.currentContext.GLctx;
        if (!gl) {
            return 0;
        }
        while (gl.getError() !== gl.NO_ERROR) {
            // Drain stale errors so the readback owns the next reported error.
        }
        return 1;
    });
}

// Returns the center pixel packed as 0xRRGGBBAA, or -1 when unavailable.
EMSCRIPTEN_KEEPALIVE
int ctrdx_read_center_pixel(int width, int height)
{
    return EM_ASM_INT({
        var gl = GL.currentContext && GL.currentContext.GLctx;
        if (!gl) {
            return -1;
        }
        var pixel = new Uint8Array(4);
        gl.finish();
        gl.readPixels(
            $0 >> 1, $1 >> 1, 1, 1, gl.RGBA, gl.UNSIGNED_BYTE, pixel);
        globalThis.ctrdxLastGlError = gl.getError();
        return ((pixel[0] << 24) | (pixel[1] << 16) |
            (pixel[2] << 8) | pixel[3]) >>> 0;
    }, width, height);
}

EMSCRIPTEN_KEEPALIVE
int ctrdx_last_gl_error(void)
{
    return EM_ASM_INT({
        return globalThis.ctrdxLastGlError | 0;
    });
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
