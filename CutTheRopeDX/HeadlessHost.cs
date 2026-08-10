using CutTheRopeDX.Commons;
using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.GameMain;

namespace CutTheRopeDX
{
    /// <summary>
    /// Runs the real game loop with no window and no graphics device. This is the shipping game
    /// minus rendering: <see cref="CtrRenderer.OnDrawFrame"/> is never called.
    /// </summary>
    internal static class HeadlessHost
    {
        /// <summary>Default logical surface width, matching the engine's master resolution.</summary>
        public const int DefaultWidth = 2560;

        /// <summary>Default logical surface height.</summary>
        public const int DefaultHeight = 1440;

        /// <summary>Brings the engine up headless. Call once per process.</summary>
        /// <param name="width">Logical surface width.</param>
        /// <param name="height">Logical surface height.</param>
        /// <param name="language">Language to initialize with.</param>
        public static void Boot(int width, int height, Language language)
        {
            ScreenPresentation.Instance.SetSurfaceSize(width, height);
            CtrBootstrap.Initialize(new HeadlessAssetPlatform(), null, width, height, language);
        }

        /// <summary>
        /// Advances the game by one frame of logic. Mirrors the desktop host's per-frame tick
        /// with rendering omitted.
        /// </summary>
        /// <param name="deltaSeconds">Frame delta in seconds.</param>
        public static void Tick(float deltaSeconds)
        {
            CtrRenderer.Java_com_zeptolab_ctr_CtrRenderer_nativeTick(deltaSeconds * 1000f);
        }

        /// <summary>Returns whether the root controller has reached the gameplay child.</summary>
        /// <returns><see langword="true"/> when gameplay is the active child.</returns>
        public static bool IsInGameplay()
        {
            return Application.SharedRootController().activeChildID == CTRRootController.CHILD_GAME;
        }

        /// <summary>Names the active root child, for smoke-run reporting.</summary>
        /// <returns>A human-readable name for the active child controller.</returns>
        public static string ActiveControllerName()
        {
            return Application.SharedRootController().activeChildID switch
            {
                CTRRootController.CHILD_START => "startup",
                CTRRootController.CHILD_MENU => "menu",
                CTRRootController.CHILD_LOADING => "loading",
                CTRRootController.CHILD_GAME => "game",
                _ => "none",
            };
        }
    }
}
