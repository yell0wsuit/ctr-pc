using System;
using System.Collections.Generic;
using System.Reflection;

using CutTheRopeDX.Framework.Physics;
using CutTheRopeDX.GameMain;

namespace CutTheRopeDX.Tests.Interactions
{
    /// <summary>
    /// Reads the scene's own state so a matrix cell can be asserted against what the game actually
    /// holds, not against a predicate's return value. The scene keeps its collections private, so
    /// this reaches them by reflection; a renamed field fails loudly here instead of silently
    /// weakening every test.
    /// </summary>
    internal static class SceneProbe
    {
        private const BindingFlags Instance = BindingFlags.Instance | BindingFlags.NonPublic;

        /// <summary>All candy contexts, primary first.</summary>
        /// <param name="scene">Scene to read.</param>
        /// <returns>The candy list.</returns>
        public static List<CandyContext> Candies(this GameScene scene)
        {
            return Field<List<CandyContext>>(scene, "candies");
        }

        /// <summary>The primary candy (candies[0]).</summary>
        /// <param name="scene">Scene to read.</param>
        /// <returns>The primary candy context.</returns>
        public static CandyContext Candy(this GameScene scene)
        {
            return scene.Candies()[0];
        }

        /// <summary>All grabs (rope hooks, wheels, guns, bees, suction cups).</summary>
        /// <param name="scene">Scene to read.</param>
        /// <returns>The grab list.</returns>
        public static List<Grab> Grabs(this GameScene scene)
        {
            return Field<List<Grab>>(scene, "bungees");
        }

        /// <summary>All free bubbles loaded into the scene.</summary>
        /// <param name="scene">Scene to read.</param>
        /// <returns>The bubble list.</returns>
        public static List<Bubble> Bubbles(this GameScene scene)
        {
            return Field<List<Bubble>>(scene, "bubbles");
        }

        /// <summary>All rockets.</summary>
        /// <param name="scene">Scene to read.</param>
        /// <returns>The rocket list.</returns>
        public static List<Rocket> Rockets(this GameScene scene)
        {
            return Field<List<Rocket>>(scene, "rockets");
        }

        /// <summary>All mechanical hands.</summary>
        /// <param name="scene">Scene to read.</param>
        /// <returns>The hand list.</returns>
        public static List<MechanicalHand> Hands(this GameScene scene)
        {
            return Field<List<MechanicalHand>>(scene, "hands");
        }

        /// <summary>All snails.</summary>
        /// <param name="scene">Scene to read.</param>
        /// <returns>The snail list.</returns>
        public static List<Snail> Snails(this GameScene scene)
        {
            return Field<List<Snail>>(scene, "snailobjects");
        }

        /// <summary>All magic hats.</summary>
        /// <param name="scene">Scene to read.</param>
        /// <returns>The hat list.</returns>
        public static List<Sock> Hats(this GameScene scene)
        {
            return Field<List<Sock>>(scene, "socks");
        }

        /// <summary>All DJ discs.</summary>
        /// <param name="scene">Scene to read.</param>
        /// <returns>The disc list.</returns>
        public static List<RotatedCircle> Discs(this GameScene scene)
        {
            return Field<List<RotatedCircle>>(scene, "rotatedCircles");
        }

        /// <summary>All Om Noms.</summary>
        /// <param name="scene">Scene to read.</param>
        /// <returns>The target list.</returns>
        public static List<TargetContext> Targets(this GameScene scene)
        {
            return Field<List<TargetContext>>(scene, "targets");
        }

        /// <summary>All spike strips.</summary>
        /// <param name="scene">Scene to read.</param>
        /// <returns>The spike list.</returns>
        public static List<Spikes> SpikeStrips(this GameScene scene)
        {
            return Field<List<Spikes>>(scene, "spikes");
        }

        /// <summary>All mice.</summary>
        /// <param name="scene">Scene to read.</param>
        /// <returns>The mouse list.</returns>
        public static List<Mouse> Mice(this GameScene scene)
        {
            return Field<List<Mouse>>(scene, "mice");
        }

        /// <summary>All bamboo tubes.</summary>
        /// <param name="scene">Scene to read.</param>
        /// <returns>The tube list.</returns>
        public static List<BambooTube> BambooTubes(this GameScene scene)
        {
            return Field<List<BambooTube>>(scene, "bambooTubes");
        }

        /// <summary>The conveyor-belt system.</summary>
        /// <param name="scene">Scene to read.</param>
        /// <returns>The conveyor object.</returns>
        public static ConveyorBeltObject Conveyors(this GameScene scene)
        {
            return Field<ConveyorBeltObject>(scene, "conveyors");
        }

        /// <summary>The win/loss recorder a scenario scene is built with.</summary>
        /// <param name="scene">Scene to read.</param>
        /// <returns>The recording delegate.</returns>
        public static RecordingSceneDelegate Outcomes(this GameScene scene)
        {
            return (RecordingSceneDelegate)scene.gameSceneDelegate;
        }

        /// <summary>Every physical candy body the scene currently offers to its systems.</summary>
        /// <param name="scene">Scene to read.</param>
        /// <returns>The active bodies, in scene enumeration order.</returns>
        public static List<CandyBody> ActiveBodies(this GameScene scene)
        {
            return [.. scene.ActiveCandyBodies()];
        }

        /// <summary>
        /// The active bodies one scene system is allowed to act on. Reads the scene's own filtered
        /// enumerator, so a system that stopped consulting <see cref="CandyBodyEligibility"/> fails
        /// here rather than being re-derived by the test.
        /// </summary>
        /// <param name="scene">Scene to read.</param>
        /// <param name="interaction">The scene system asking for candidates.</param>
        /// <returns>The eligible active bodies, in scene enumeration order.</returns>
        public static List<CandyBody> ActiveBodies(this GameScene scene, CandyInteraction interaction)
        {
            return [.. scene.ActiveCandyBodies(interaction)];
        }

        /// <summary>The active body standing on a physics point, or null when no body owns it.</summary>
        /// <param name="scene">Scene to read.</param>
        /// <param name="point">Physics point to resolve.</param>
        /// <returns>The owning body, or <see langword="null"/>.</returns>
        public static CandyBody BodyForPoint(this GameScene scene, ConstraintedPoint point)
        {
            return scene.CandyBodyForPointOrNull(point);
        }

        /// <summary>Whether the primary candy's lifecycle leaves no whole body in play.</summary>
        /// <param name="scene">Scene to read.</param>
        /// <returns><see langword="true"/> when the primary candy is removed, hidden, or split.</returns>
        public static bool PrimaryCandyGone(this GameScene scene)
        {
            return scene.Candies()[0].HasNoWholeBodyInPlay;
        }

        /// <summary>Ropes still attached (uncut) to the given candy.</summary>
        /// <param name="scene">Scene to read.</param>
        /// <param name="candy">Candy to inspect.</param>
        /// <returns>The number of uncut ropes whose tail is this candy.</returns>
        public static int AttachedRopeCount(this GameScene scene, CandyContext candy)
        {
            int count = 0;
            foreach (Grab grab in scene.Grabs())
            {
                if (grab.rope != null && grab.rope.tail == candy.point && grab.rope.cut == -1)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>Snails riding the given candy.</summary>
        /// <param name="scene">Scene to read.</param>
        /// <param name="candy">Candy to inspect.</param>
        /// <returns>The count of active snails on this candy.</returns>
        public static int SnailCount(this GameScene scene, CandyContext candy)
        {
            return scene.ActiveSnailCountForPoint(candy.point);
        }

        /// <summary>Whether the mouse system currently carries the given candy.</summary>
        /// <param name="scene">Scene to read.</param>
        /// <param name="candy">Candy to inspect.</param>
        /// <returns><see langword="true"/> when the active mouse holds this candy.</returns>
        public static bool MouseCarries(this GameScene scene, CandyContext candy)
        {
            MiceObject mice = Field<MiceObject>(scene, "miceManager");
            ConstraintedPoint carried = mice?.ActiveMouseCarriedStar();
            return carried != null && carried == candy.point;
        }

        /// <summary>Whether any conveyor belt has bound the given grab.</summary>
        /// <param name="scene">Scene to read.</param>
        /// <param name="grab">Grab to inspect.</param>
        /// <returns><see langword="true"/> when a belt holds this grab.</returns>
        public static bool BeltHolds(this GameScene scene, Grab grab)
        {
            foreach (ConveyorBelt belt in scene.Conveyors().Iterator())
            {
                if (belt.HasItem(grab))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Whether the first DJ disc has captured the given grab.</summary>
        /// <param name="scene">Scene to read.</param>
        /// <param name="grab">Grab to inspect.</param>
        /// <returns><see langword="true"/> when the disc rotates this grab.</returns>
        public static bool DiscHolds(this GameScene scene, Grab grab)
        {
            foreach (RotatedCircle disc in scene.Discs())
            {
                if (disc.containedObjects.IndexOf(grab) != -1)
                {
                    return true;
                }
            }

            return false;
        }

        private static T Field<T>(GameScene scene, string name)
        {
            FieldInfo field = typeof(GameScene).GetField(name, Instance)
                ?? throw new MissingFieldException(nameof(GameScene), name);
            return (T)field.GetValue(scene);
        }
    }
}
