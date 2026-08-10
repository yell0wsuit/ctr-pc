using System.IO;
using System.Xml.Linq;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.GameMain;
using CutTheRopeDX.Helpers;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Boots the real game headless and drives it frame by frame. One instance per test;
    /// the engine's statics are process-wide, so the suite must stay serial.
    /// </summary>
    internal sealed class HeadlessGame
    {
        private static bool booted;

        private HeadlessGame()
        {
        }

        /// <summary>Boots the engine headless, once per process.</summary>
        /// <returns>A harness handle.</returns>
        public static HeadlessGame Boot()
        {
            if (!booted)
            {
                // Headless has no graphics device, but installing a throwing backend (rather than
                // leaving PlatformServices.Render null) turns "we believe no test's logic path
                // reaches rendering code" into a verified fact: see ThrowingRenderBackend for why.
                PlatformServices.Render = new ThrowingRenderBackend();
                HeadlessHost.Boot(HeadlessHost.DefaultWidth, HeadlessHost.DefaultHeight, Language.LANGEN);
                booted = true;
            }

            return new HeadlessGame();
        }

        /// <summary>Loads a level into a fresh scene and returns it.</summary>
        /// <param name="pack">Zero-based pack index.</param>
        /// <param name="level">Zero-based level index.</param>
        /// <returns>The loaded scene.</returns>
        public static GameScene LoadLevel(int pack, int level)
        {
            CTRRootController root = (CTRRootController)Application.SharedRootController();
            root.SetPicker(false);
            root.SetPack(pack);
            root.SetLevel(level);
            string mapPath = Path.Combine(ContentPaths.MapsDirectory, LevelsList.LEVEL_NAMES[pack, level]);
            root.SetMap(ContentPaths.LoadXml(mapPath));
            root.SetMapName(mapPath);

            GameScene scene = new();
            scene.Show();
            return scene;
        }

        /// <summary>
        /// Boots a full GameController for a level - view hierarchy, HUD buttons, pause menu -
        /// so button-driven flows can be exercised headlessly.
        /// </summary>
        /// <param name="pack">Zero-based pack index.</param>
        /// <param name="level">Zero-based level index.</param>
        /// <returns>An activated controller. Its scene is <c>GetView(0).GetChild(0)</c>.</returns>
        public static GameController LoadLevelWithController(int pack, int level)
        {
            CTRRootController root = (CTRRootController)Application.SharedRootController();
            root.SetPicker(false);
            root.SetPack(pack);
            root.SetLevel(level);
            string mapPath = Path.Combine(ContentPaths.MapsDirectory, LevelsList.LEVEL_NAMES[pack, level]);
            root.SetMap(ContentPaths.LoadXml(mapPath));
            root.SetMapName(mapPath);

            GameController controller = new(root);
            controller.Activate();
            return controller;
        }

        /// <summary>
        /// Loads a scenario built in code (see <c>Interactions/Scenario</c>) instead of a shipping
        /// level. The map element goes through the same resource-ensure and <see cref="GameScene.Show"/>
        /// path a real level takes, so loaded objects are wired exactly as in the game.
        /// </summary>
        /// <param name="map">The scenario's level element.</param>
        /// <param name="pack">Zero-based pack index, for pack-dependent visuals.</param>
        /// <param name="level">Zero-based level index.</param>
        /// <returns>The loaded scene.</returns>
        public static GameScene LoadScenarioMap(XElement map, int pack = 0, int level = 0)
        {
            CTRRootController root = (CTRRootController)Application.SharedRootController();
            root.SetPack(pack);
            root.SetLevel(level);
            root.PrepareMapAndEnsureResources(map, "scenario.xml");

            GameScene scene = new();
            scene.Show();
            return scene;
        }

        /// <summary>Advances a scene by whole frames at the engine's fixed delta.</summary>
        /// <param name="scene">Scene to advance.</param>
        /// <param name="frames">Number of frames.</param>
        public static void StepFrames(GameScene scene, int frames)
        {
            for (int i = 0; i < frames; i++)
            {
                scene.Update(0.016f);
            }
        }
    }
}
