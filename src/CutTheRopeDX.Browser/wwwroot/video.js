// Cutscene playback, as a browser can do it.
//
// The desktop backends decode frames themselves and hand Core a texture to draw in place of
// the scene. A browser already has a decoder wired to a compositor and an audio clock, so
// the faithful port is a <video> element over the canvas: same letterboxed box, same
// stop-on-click, and audio that cannot drift because nothing here is keeping it in sync.

const video = document.getElementById("movie");

let finished = true;
let started = false;

/** Ends the current playback, whatever ended it. */
function finish() {
    finished = true;
    video.hidden = true;
}

video.addEventListener("ended", finish);
// A missing file lands here, which is the case where the content build's video step was
// skipped. Reporting it as finished makes that build behave exactly like the no-op stub
// the web host shipped with, rather than hanging on a black screen.
video.addEventListener("error", finish);
// The desktop host stops the movie on a left click anywhere in the window. Use stop rather
// than only hiding the element so the skipped cutscene's audio cannot keep playing.
video.addEventListener("pointerdown", stop);

/**
 * Prepares a cutscene. Playback waits for start().
 *
 * @param {string} url
 * @param {boolean} mute
 */
export function load(url, mute) {
    finished = false;
    started = false;
    video.muted = mute;
    video.src = url;
    video.hidden = false;
    video.load();
}

/** Begins playback. Repeat calls are ignored. */
export function start() {
    if (started || finished) {
        return;
    }
    started = true;
    // The player pressed Play on the splash screen and has been clicking menu buttons, so
    // the document has user activation and sound is allowed. A browser that disagrees
    // still gets to show the cutscene, just silently; one that refuses even that is told
    // the playback is over, so the game moves on instead of waiting forever.
    video.play().catch(() => {
        video.muted = true;
        video.play().catch(finish);
    });
}

/** Holds playback while the page is inactive. */
export function pause() {
    video.pause();
}

/** Resumes a playback that pause() held. */
export function resume() {
    if (finished || !started) {
        return;
    }
    video.play().catch(() => {
        video.muted = true;
        video.play().catch(finish);
    });
}

/** Ends playback early, as a skip does. */
export function stop() {
    video.pause();
    finish();
}

/** Whether this playback has ended, failed, or been skipped. */
export function isFinished() {
    return finished;
}
