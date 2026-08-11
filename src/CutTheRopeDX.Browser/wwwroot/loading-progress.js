export function formatLoadingProgress(type, loaded, total) {
    return `Loading ${type}: ${loaded} of ${total}…`;
}

export function setLoadingProgress(type, loaded, total) {
    const progress = document.getElementById("splash-progress");
    if (progress !== null) {
        progress.textContent = formatLoadingProgress(type, loaded, total);
    }
}
