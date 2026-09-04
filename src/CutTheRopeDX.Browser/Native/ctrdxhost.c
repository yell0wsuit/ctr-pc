// Bodies are EM_ASM because it runs in the JavaScript scope of the calling
// thread. The runtime proxies [JSImport] to the browser thread, so managed
// interop cannot reach the owner thread's own scope, and the owner thread is
// where the WebGL context and the animation frame have to live.

#include <emscripten.h>
#include <emscripten/threading.h>
#include <pthread.h>
#include <stdint.h>

static void (*frame_callback)(double) = NULL;
static int frame_callback_hits = 0;

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
        requestAnimationFrame(function (timestamp) {
            _ctrdx_frame_entry(timestamp);
        });
    });
}
