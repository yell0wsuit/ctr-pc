using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Physics;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;

using Xunit;

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

        /// <summary>All ropes currently registered with the scene.</summary>
        /// <param name="scene">Scene to read.</param>
        /// <returns>The registered rope entries.</returns>
        public static IReadOnlyList<RopeEntry> RegisteredRopes(this GameScene scene)
        {
            return Field<RopeRegistry>(scene, "ropes").All;
        }

        /// <summary>All free bubbles loaded into the scene.</summary>
        /// <param name="scene">Scene to read.</param>
        /// <returns>The bubble list.</returns>
        public static List<Bubble> Bubbles(this GameScene scene)
        {
            return Field<List<Bubble>>(scene, "bubbles");
        }

        /// <summary>All ghosts.</summary>
        /// <param name="scene">Scene to read.</param>
        /// <returns>The ghost list.</returns>
        public static List<Ghost> Ghosts(this GameScene scene)
        {
            return Field<List<Ghost>>(scene, "ghosts");
        }

        /// <summary>All bouncers.</summary>
        /// <param name="scene">Scene to read.</param>
        /// <returns>The bouncer list.</returns>
        public static List<Bouncer> Bouncers(this GameScene scene)
        {
            return Field<List<Bouncer>>(scene, "bouncers");
        }

        /// <summary>All air pumps.</summary>
        /// <param name="scene">Scene to read.</param>
        /// <returns>The pump list.</returns>
        public static List<Pump> Pumps(this GameScene scene)
        {
            return Field<List<Pump>>(scene, "pumps");
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

        /// <summary>
        /// The rest length of the leash a rocket created when it bound a candy - the reach it will
        /// reel in. It has to be measured to the candy the rocket actually bound; a rocket that took
        /// its reach from whichever candy was primary gets a leash that is slack or snaps.
        /// </summary>
        /// <param name="rocket">Rocket to inspect.</param>
        /// <param name="candy">Candy the rocket is expected to have bound.</param>
        /// <returns>The rest length, or <see langword="null"/> when the rocket holds no leash on it.</returns>
        public static float? BindReach(this Rocket rocket, CandyContext candy)
        {
            foreach (Constraint constraint in rocket.point.constraints)
            {
                if (constraint.cp == candy.WholeBody.Point)
                {
                    return constraint.restLength;
                }
            }

            return null;
        }

        /// <summary>
        /// The grab whose hook sits closest to a world position. Shipped levels are addressed by
        /// where their hooks are authored, so a test names the rope it cuts the way a player sees it
        /// rather than by an index into load order.
        /// </summary>
        /// <param name="scene">Scene to read.</param>
        /// <param name="world">World position to search near.</param>
        /// <returns>The nearest grab.</returns>
        public static Grab GrabNearestTo(this GameScene scene, Vector world)
        {
            Grab nearest = null;
            float best = float.MaxValue;
            foreach (Grab grab in scene.Grabs())
            {
                float dx = grab.x - world.X;
                float dy = grab.y - world.Y;
                float distance = (dx * dx) + (dy * dy);
                if (distance < best)
                {
                    best = distance;
                    nearest = grab;
                }
            }

            return nearest;
        }

        /// <summary>
        /// The rope a grab currently holds, or <see langword="null"/> when it holds none. Exists so a
        /// test can read a hook's rope without naming the field, which the axis refactor renames.
        /// </summary>
        /// <param name="grab">Grab to inspect.</param>
        /// <returns>The held rope, or <see langword="null"/>.</returns>
        public static Bungee RopeOf(this Grab grab)
        {
            return grab.Rope;
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
            return scene.AttachedRopeCount(candy.WholeBody);
        }

        /// <summary>Ropes still attached (uncut) to the exact candy body.</summary>
        /// <param name="scene">Scene to read.</param>
        /// <param name="body">Body to inspect.</param>
        /// <returns>The number of uncut ropes whose tail is this body's point.</returns>
        public static int AttachedRopeCount(this GameScene scene, CandyBody body)
        {
            int count = 0;
            foreach (Grab grab in scene.Grabs())
            {
                if (grab.Rope != null && grab.Rope.tail == body.Point && grab.Rope.cut == -1)
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
            return scene.ActiveSnailCountForPoint(candy.WholeBody.Point);
        }

        /// <summary>Whether the mouse system currently carries the given candy.</summary>
        /// <param name="scene">Scene to read.</param>
        /// <param name="candy">Candy to inspect.</param>
        /// <returns><see langword="true"/> when the active mouse holds this candy.</returns>
        public static bool MouseCarries(this GameScene scene, CandyContext candy)
        {
            MiceObject mice = Field<MiceObject>(scene, "miceManager");
            return mice?.CarriesCandy(candy.WholeBody.Point) == true;
        }

        /// <summary>Asserts that a retired whole candy has no observable live attachment owner.</summary>
        /// <param name="scene">Scene that owned the candy.</param>
        /// <param name="candy">Retired candy to inspect.</param>
        public static void AssertNoLiveAttachments(this GameScene scene, CandyContext candy)
        {
            Assert.Equal(0, scene.AttachedRopeCount(candy));
            Assert.Null(candy.WholeBody.Bubble);
            Assert.Null(candy.Lifecycle.Attachments.Rocket);
            Assert.Null(candy.Lifecycle.Attachments.Hand);
            Assert.False(scene.MouseCarries(candy));
            Assert.Null(candy.Lifecycle.Attachments.AntSegment);
            Assert.Null(candy.Lifecycle.Attachments.LastAntSegment);
            Assert.Equal(0, scene.SnailCount(candy));
            Assert.False(candy.Lifecycle.Attachments.InLantern);
            Assert.False(candy.Lifecycle.Attachments.HasAny);
            Assert.False(candy.Lifecycle.IsGravitySuppressed);
            Assert.False(candy.WholeBody.Point.disableGravity);
        }

        /// <summary>Invokes the scene's hazard entry point to model repeated overlap in one update.</summary>
        /// <param name="scene">Scene whose hazard path is invoked.</param>
        /// <param name="body">Body intersecting the hazard.</param>
        public static void BreakCandyBody(this GameScene scene, CandyBody body)
        {
            MethodInfo method = typeof(GameScene).GetMethod("BreakCandyBody", Instance)
                ?? throw new MissingMethodException(nameof(GameScene), "BreakCandyBody");
            _ = method.Invoke(scene, [body]);
        }

        /// <summary>Number of live candy-break particle systems in the scene animation pool.</summary>
        /// <param name="scene">Scene to inspect.</param>
        /// <returns>The live candy-break effect count.</returns>
        public static int CandyBreakEffectCount(this GameScene scene)
        {
            AnimationsPool pool = Field<AnimationsPool>(scene, "aniPool");
            return pool.GetChilds().Values.OfType<CandyBreak>().Count();
        }

        /// <summary>Number of live bubble-pop animations in the scene animation pool.</summary>
        /// <param name="scene">Scene to inspect.</param>
        /// <returns>The live bubble-pop effect count.</returns>
        public static int BubblePopEffectCount(this GameScene scene)
        {
            AnimationsPool pool = Field<AnimationsPool>(scene, "aniPool");
            CTRTexture2D bubbleTexture = Application.GetTexture(Resources.Img.ObjBubble);
            return pool.GetChilds().Values
                .OfType<Animation>()
                .Count(animation => ReferenceEquals(animation.texture, bubbleTexture));
        }

        /// <summary>Number of live spider victory images in the scene animation pool.</summary>
        /// <param name="scene">Scene to inspect.</param>
        /// <returns>The live spider victory-image count.</returns>
        public static int SpiderVictoryEffectCount(this GameScene scene)
        {
            AnimationsPool pool = Field<AnimationsPool>(scene, "aniPool");
            CTRTexture2D spiderTexture = Application.GetTexture(Resources.Img.ObjSpider);
            return pool.GetChilds().Values
                .OfType<Image>()
                .Count(effect => ReferenceEquals(effect.texture, spiderTexture) && effect.GetChilds().Count > 0);
        }

        /// <summary>
        /// Parks a second merged ghost bubble for an owner. Scenario XML cannot directly author the
        /// transient two-ghost post-merge state, so this probe establishes only that private state.
        /// </summary>
        public static void ParkSecondGhostBubble(this GameScene scene, CandyBody owner, GameObject bubble)
        {
            FieldInfo field = typeof(GameScene).GetField("parkedGhostBubble", Instance)
                ?? throw new MissingFieldException(nameof(GameScene), "parkedGhostBubble");
            field.SetValue(scene, new ParkedGhostBubble(owner, bubble));
        }

        /// <summary>Gets the ghost bubble parked behind a merged candy's active ghost bubble.</summary>
        public static GameObject PendingSecondGhostBubble(this GameScene scene)
        {
            return Field<ParkedGhostBubble>(scene, "parkedGhostBubble")?.Bubble;
        }

        /// <summary>Installs an exact lantern ticket for delayed-callback identity tests.</summary>
        public static void SetPendingLanternCapture(
            this GameScene scene,
            PendingLanternCapture capture)
        {
            FieldInfo field = typeof(GameScene).GetField("pendingLanternCapture", Instance)
                ?? throw new MissingFieldException(nameof(GameScene), "pendingLanternCapture");
            field.SetValue(scene, capture);
        }

        /// <summary>Gets the scene's current delayed lantern-capture ticket.</summary>
        public static PendingLanternCapture PendingLanternCapture(this GameScene scene)
        {
            return Field<PendingLanternCapture>(scene, "pendingLanternCapture");
        }

        /// <summary>Invokes the real delayed lantern completion callback with an exact ticket.</summary>
        public static void CompletePendingLanternCapture(
            this GameScene scene,
            PendingLanternCapture capture)
        {
            MethodInfo method = typeof(GameScene).GetMethod(
                "CompletePendingLanternCapture",
                Instance)
                ?? throw new MissingMethodException(nameof(GameScene), "CompletePendingLanternCapture");
            _ = method.Invoke(scene, [capture]);
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
