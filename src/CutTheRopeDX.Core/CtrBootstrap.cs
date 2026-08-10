using CutTheRopeDX.Commons;
using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Media;
using CutTheRopeDX.Framework.Platform;

namespace CutTheRopeDX
{
    /// <summary>
    /// The single device-independent boot sequence. Both the desktop host (<c>Game1</c>)
    /// and the headless host call this, so the two cannot drift — if they did, headless tests
    /// would stop reflecting the real game.
    /// </summary>
    internal static class CtrBootstrap
    {
        /// <summary>
        /// Installs the asset platform and brings the engine up to the point where the root
        /// controller can be ticked.
        /// </summary>
        /// <param name="platform">Asset platform to install before any asset load.</param>
        /// <param name="audioBackend">Audio backend for sound and music, or <see langword="null"/> for silent runs.</param>
        /// <param name="surfaceWidth">Logical surface width.</param>
        /// <param name="surfaceHeight">Logical surface height.</param>
        /// <param name="language">Language to initialize the engine with.</param>
        public static void Initialize(
            IAssetPlatform platform,
            IAudioBackend audioBackend,
            int surfaceWidth,
            int surfaceHeight,
            Language language)
        {
            AssetPlatform.Current = platform;
            SoundMgr.SetBackend(audioBackend);
            Preferences.LoadPreferences();
            CtrRenderer.Java_com_zeptolab_ctr_CtrRenderer_nativeInit(language);
            CtrRenderer.OnSurfaceCreated();
            CtrRenderer.OnSurfaceChanged(surfaceWidth, surfaceHeight);
        }
    }
}
