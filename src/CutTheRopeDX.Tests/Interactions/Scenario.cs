using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.GameMain;

namespace CutTheRopeDX.Tests.Interactions
{
    /// <summary>
    /// Builds an interaction scenario in code and boots it headless. Tests describe only the
    /// objects a matrix cell needs - no shipping level is loaded, so a cell is never at the mercy
    /// of some level's incidental layout.
    /// </summary>
    /// <remarks>
    /// Positions are level-space (the level-editor grid, same units the shipping maps use) and
    /// integral, because the loaders truncate coordinates to int. The scene's only entry point is
    /// <see cref="GameScene.Show"/>, which reads the level element, so the builder feeds the real
    /// loader path rather than hand-placing objects: a scenario is loaded exactly like a level.
    /// </remarks>
    internal sealed class Scenario
    {
        /// <summary>Level scale the engine applies to every authored coordinate.</summary>
        public const float Scale = 3f;

        private const int DefaultWidth = 320;
        private const int DefaultHeight = 480;

        private readonly List<XElement> objects = [];
        private readonly List<XAttribute> design = [];
        private int width = DefaultWidth;
        private int height = DefaultHeight;
        private int special = 1;
        private bool splitCandy;

        internal Scenario()
        {
        }

        /// <summary>Number of hats added so far, so tests can assert a pair was built.</summary>
        public int HatCount { get; private set; }

        /// <summary>Starts an empty scenario.</summary>
        /// <returns>The new scenario.</returns>
        public static Scenario New()
        {
            return new Scenario();
        }

        /// <summary>Converts a level-space X to the world X the scene will use.</summary>
        /// <param name="x">Level-space X.</param>
        /// <returns>World X.</returns>
        public float WorldX(int x)
        {
            return (x * Scale) + ((2560f - (width * Scale)) / 2f);
        }

        /// <summary>Converts a level-space Y to the world Y the scene will use.</summary>
        /// <param name="y">Level-space Y.</param>
        /// <returns>World Y.</returns>
        public static float WorldY(int y)
        {
            return y * Scale;
        }

        /// <summary>Adds a candy. The first one becomes the primary candy (candies[0]).</summary>
        /// <param name="x">Level-space X.</param>
        /// <param name="y">Level-space Y.</param>
        /// <param name="number">Optional candy key, for ropes that bind by <c>candyNumber</c>.</param>
        /// <returns>This scenario.</returns>
        public Scenario Candy(int x, int y, string number = null)
        {
            XElement candy = Node("candy", x, y);
            if (number != null)
            {
                candy.SetAttributeValue("candyNumber", number);
            }

            return Add(candy);
        }

        /// <summary>Adds a numbered light-emitting candy body.</summary>
        /// <param name="x">Level-space X.</param>
        /// <param name="y">Level-space Y.</param>
        /// <param name="number">Bulb key used by authored ropes.</param>
        /// <param name="litRadius">Illumination radius in level units.</param>
        /// <returns>This scenario.</returns>
        public Scenario LightBulb(int x, int y, string number = "first", int litRadius = 100)
        {
            XElement bulb = Node("lightBulb", x, y);
            bulb.SetAttributeValue("bulbNumber", number);
            bulb.SetAttributeValue("litRadius", Num(litRadius));
            return Add(bulb);
        }

        /// <summary>
        /// Makes the primary candy a split candy: turns on the level's <c>twoParts</c> design flag
        /// and emits the <c>&lt;candyL&gt;</c>/<c>&lt;candyR&gt;</c> halves that the loader hands to
        /// <c>candies[0]</c>'s lifecycle as one <see cref="SplitCandyState"/>.
        /// </summary>
        /// <remarks>
        /// Only the primary candy can be split - that is the shipped maps' one topology, and the
        /// loader keeps it: a split level reserves <c>candies[0]</c>, so every later
        /// <see cref="Candy"/> call builds an additional whole candy context beside the split one.
        /// Call this once per scenario; a second call would leave the loader holding only the last
        /// pair of halves. The design flag is set on the scenario rather than through
        /// <see cref="Design"/>, so <c>twoParts</c> is never written twice onto one element.
        /// </remarks>
        /// <param name="leftX">Level-space X of the left half.</param>
        /// <param name="leftY">Level-space Y of the left half.</param>
        /// <param name="rightX">Level-space X of the right half.</param>
        /// <param name="rightY">Level-space Y of the right half.</param>
        /// <returns>This scenario.</returns>
        public Scenario SplitCandy(int leftX, int leftY, int rightX, int rightY)
        {
            splitCandy = true;
            _ = Add(Node("candyL", leftX, leftY));
            return Add(Node("candyR", rightX, rightY));
        }

        /// <summary>Adds an Om Nom. Every scenario needs one; the scene assumes a target exists.</summary>
        /// <param name="x">Level-space X.</param>
        /// <param name="y">Level-space Y.</param>
        /// <returns>This scenario.</returns>
        public Scenario OmNom(int x, int y)
        {
            return Add(Node("target", x, y));
        }

        /// <summary>Adds a plain fixed hook with a rope to the candy.</summary>
        /// <param name="x">Level-space X.</param>
        /// <param name="y">Level-space Y.</param>
        /// <param name="length">Rope rest length in level units.</param>
        /// <param name="candyNumber">Optional candy key to bind to.</param>
        /// <returns>This scenario.</returns>
        public Scenario Rope(int x, int y, int length, string candyNumber = null)
        {
            return Grab(x, y, length: length, candyNumber: candyNumber);
        }

        /// <summary>Adds a grab with full control over its variant attributes.</summary>
        /// <param name="x">Level-space X.</param>
        /// <param name="y">Level-space Y.</param>
        /// <param name="length">Rope rest length in level units; ignored when <paramref name="radius"/> is set.</param>
        /// <param name="radius">Auto-attach radius, or -1 for a fixed hook.</param>
        /// <param name="wheel">Wheel (rotating) grab.</param>
        /// <param name="gun">Gun grab.</param>
        /// <param name="spider">Carries a spider.</param>
        /// <param name="moveLength">Drag-rail length in level units, or -1 for none.</param>
        /// <param name="moveVertical">Drag rail runs vertically.</param>
        /// <param name="kickable">Suction cup that can be kicked loose.</param>
        /// <param name="kicked">Starts kicked (detached and free-falling).</param>
        /// <param name="path">Mover path string (makes it a bee), e.g. "L,60,0".</param>
        /// <param name="moveSpeed">Mover speed for <paramref name="path"/>.</param>
        /// <param name="candyNumber">Optional candy key to bind the rope to.</param>
        /// <param name="bindBulb">Whether this rope binds a numbered light emitter.</param>
        /// <param name="bulbNumber">Light-emitter key when <paramref name="bindBulb"/> is true.</param>
        /// <param name="breakable">Whether the rope can be cut by an ordinary swipe.</param>
        /// <returns>This scenario.</returns>
        public Scenario Grab(
            int x,
            int y,
            int length = 100,
            float radius = -1f,
            bool wheel = false,
            bool gun = false,
            bool spider = false,
            float moveLength = float.NaN,
            bool moveVertical = false,
            bool kickable = false,
            bool kicked = false,
            string path = null,
            float moveSpeed = 0f,
            string candyNumber = null,
            bool bindBulb = false,
            string bulbNumber = null,
            bool breakable = true)
        {
            XElement grab = Node("grab", x, y);
            grab.SetAttributeValue("length", Num(length));
            grab.SetAttributeValue("radius", Num(radius));
            grab.SetAttributeValue("wheel", Flag(wheel));
            grab.SetAttributeValue("gun", Flag(gun));
            grab.SetAttributeValue("spider", Flag(spider));
            if (!float.IsNaN(moveLength))
            {
                grab.SetAttributeValue("moveLength", Num(moveLength));
            }
            grab.SetAttributeValue("moveVertical", Flag(moveVertical));
            grab.SetAttributeValue("moveOffset", Num(0));
            grab.SetAttributeValue("part", "L");
            grab.SetAttributeValue("breakable", Flag(breakable));
            grab.SetAttributeValue("bindBulb", Flag(bindBulb));
            if (bulbNumber != null)
            {
                grab.SetAttributeValue("bulbNumber", bulbNumber);
            }
            if (kickable)
            {
                grab.SetAttributeValue("kickable", Flag(true));
            }

            if (kicked)
            {
                grab.SetAttributeValue("kicked", Flag(true));
            }

            if (path != null)
            {
                grab.SetAttributeValue("path", path);
                grab.SetAttributeValue("moveSpeed", Num(moveSpeed));
                grab.SetAttributeValue("hidePath", Flag(true));
            }

            if (candyNumber != null)
            {
                grab.SetAttributeValue("candyNumber", candyNumber);
            }

            return Add(grab);
        }

        /// <summary>Adds a rocket.</summary>
        /// <param name="x">Level-space X.</param>
        /// <param name="y">Level-space Y.</param>
        /// <param name="angle">Nozzle angle in degrees.</param>
        /// <param name="impulse">Thrust impulse.</param>
        /// <param name="time">Burn time in seconds, or -1 to burn until it leaves.</param>
        /// <param name="impulseFactor">
        /// Thrust multiplier applied while the candy's rope is relaxed; the loader substitutes
        /// <c>0.6</c> when a map leaves it at zero, so that is the default here too.
        /// </param>
        /// <param name="isRotatable">Whether the player can spin the rocket before it fires.</param>
        /// <returns>This scenario.</returns>
        public Scenario Rocket(
            int x,
            int y,
            float angle = 0f,
            float impulse = 200f,
            float time = -1f,
            float impulseFactor = 0.6f,
            bool isRotatable = false)
        {
            XElement rocket = Node("rocket", x, y);
            rocket.SetAttributeValue("angle", Num(angle));
            rocket.SetAttributeValue("impulse", Num(impulse));
            rocket.SetAttributeValue("time", Num(time));
            rocket.SetAttributeValue("impulseFactor", Num(impulseFactor));
            rocket.SetAttributeValue("isRotatable", Flag(isRotatable));
            return Add(rocket);
        }

        /// <summary>Adds a free-floating bubble.</summary>
        /// <param name="x">Level-space X.</param>
        /// <param name="y">Level-space Y.</param>
        /// <returns>This scenario.</returns>
        public Scenario Bubble(int x, int y)
        {
            return Add(Node("bubble", x, y));
        }

        /// <summary>Adds a bouncer.</summary>
        /// <param name="x">Level-space X.</param>
        /// <param name="y">Level-space Y.</param>
        /// <param name="size">Width class: 1 is the small bouncer, 2 the large one.</param>
        /// <param name="angle">Bouncer angle in degrees.</param>
        /// <returns>This scenario.</returns>
        public Scenario Bouncer(int x, int y, int size = 2, float angle = 0f)
        {
            XElement bouncer = Node(size == 1 ? "bouncer1" : "bouncer2", x, y);
            bouncer.SetAttributeValue("size", Num(size));
            bouncer.SetAttributeValue("angle", Num(angle));
            return Add(bouncer);
        }

        /// <summary>Adds a ghost that can morph into a bubble and a grab.</summary>
        /// <param name="x">Level-space X.</param>
        /// <param name="y">Level-space Y.</param>
        /// <param name="radius">Grab radius, or -1 for an automatically attached rope.</param>
        /// <param name="bouncer">Whether the ghost can also morph into a bouncer.</param>
        /// <returns>This scenario.</returns>
        public Scenario Ghost(int x, int y, int radius = 80, bool bouncer = false)
        {
            XElement ghost = Node("ghost", x, y);
            ghost.SetAttributeValue("bubble", "true");
            ghost.SetAttributeValue("grab", "true");
            ghost.SetAttributeValue("bouncer", Flag(bouncer));
            ghost.SetAttributeValue("radius", Num(radius));
            return Add(ghost);
        }

        /// <summary>Adds a directional air pump.</summary>
        /// <param name="x">Level-space X.</param>
        /// <param name="y">Level-space Y.</param>
        /// <param name="angle">Authored pump angle in degrees; -90 blows upward.</param>
        /// <returns>This scenario.</returns>
        public Scenario Pump(int x, int y, float angle = -90f)
        {
            XElement pump = Node("pump", x, y);
            pump.SetAttributeValue("angle", Num(angle));
            return Add(pump);
        }

        /// <summary>Adds a snail.</summary>
        /// <param name="x">Level-space X.</param>
        /// <param name="y">Level-space Y.</param>
        /// <returns>This scenario.</returns>
        public Scenario Snail(int x, int y)
        {
            return Add(Node("load", x, y));
        }

        /// <summary>Adds a mechanical hand with a single straight segment.</summary>
        /// <param name="x">Level-space X of the shoulder.</param>
        /// <param name="y">Level-space Y of the shoulder.</param>
        /// <param name="segmentLength">Segment length in level units.</param>
        /// <param name="segmentAngle">Segment angle in degrees (0 points right, 90 down).</param>
        /// <param name="rotatable">Whether the segment carries a rotate button.</param>
        /// <returns>This scenario.</returns>
        public Scenario Hand(int x, int y, int segmentLength, float segmentAngle, bool rotatable = false)
        {
            XElement hand = Node("hand", x, y);
            hand.SetAttributeValue("segmentsCount", Num(1));
            hand.SetAttributeValue("segment1Length", Num(segmentLength));
            hand.SetAttributeValue("segment1Angle", Num(segmentAngle));
            hand.SetAttributeValue("segment1Rotatable", Flag(rotatable));
            return Add(hand);
        }

        /// <summary>Adds a mouse hole and its mouse.</summary>
        /// <param name="x">Level-space X.</param>
        /// <param name="y">Level-space Y.</param>
        /// <param name="radius">Grab radius in level units.</param>
        /// <param name="index">Mouse index; index 1 is the one that starts active.</param>
        /// <param name="activeTime">Seconds before the mouse retreats.</param>
        /// <returns>This scenario.</returns>
        public Scenario Mouse(int x, int y, int radius = 80, int index = 1, float activeTime = 600f)
        {
            XElement mouse = Node("mouse", x, y);
            mouse.SetAttributeValue("angle", Num(0));
            mouse.SetAttributeValue("radius", Num(radius));
            mouse.SetAttributeValue("activeTime", Num(activeTime));
            mouse.SetAttributeValue("index", Num(index));
            return Add(mouse);
        }

        /// <summary>Adds a lantern.</summary>
        /// <param name="x">Level-space X.</param>
        /// <param name="y">Level-space Y.</param>
        /// <returns>This scenario.</returns>
        public Scenario Lantern(int x, int y)
        {
            return Add(Node("lantern", x, y));
        }

        /// <summary>
        /// Adds one magic hat. A hat teleports to another hat of the same group, so a working
        /// pair needs two calls with the same <paramref name="group"/>.
        /// </summary>
        /// <param name="x">Level-space X.</param>
        /// <param name="y">Level-space Y.</param>
        /// <param name="angle">Mouth angle in degrees.</param>
        /// <param name="group">Teleport group.</param>
        /// <returns>This scenario.</returns>
        public Scenario Hat(int x, int y, float angle = 0f, int group = 1)
        {
            XElement sock = Node("sock", x, y);
            sock.SetAttributeValue("angle", Num(angle));
            sock.SetAttributeValue("group", Num(group));
            HatCount++;
            return Add(sock);
        }

        /// <summary>Adds a bamboo tube whose mouth faces the given way.</summary>
        /// <param name="x">Level-space X.</param>
        /// <param name="y">Level-space Y.</param>
        /// <param name="mouth">Direction the catching hole faces.</param>
        /// <returns>This scenario.</returns>
        public Scenario BambooTube(int x, int y, TubeMouth mouth)
        {
            XElement pipe = Node("pipe", x, y);
            pipe.SetAttributeValue("angle", Num(TubeGeometry.AngleFor(mouth)));
            return Add(pipe);
        }

        /// <summary>Adds an ant path.</summary>
        /// <param name="x">Level-space X of the path origin.</param>
        /// <param name="y">Level-space Y of the path origin.</param>
        /// <param name="path">Comma-separated X,Y vertex offsets from the origin.</param>
        /// <param name="moveSpeed">March speed.</param>
        /// <returns>This scenario.</returns>
        public Scenario Ants(int x, int y, string path, float moveSpeed = 30f)
        {
            XElement ants = Node("ants", x, y);
            ants.SetAttributeValue("path", path);
            ants.SetAttributeValue("moveSpeed", Num(moveSpeed));
            return Add(ants);
        }

        /// <summary>Adds a spike strip.</summary>
        /// <param name="x">Level-space X.</param>
        /// <param name="y">Level-space Y.</param>
        /// <param name="angle">Rotation in degrees.</param>
        /// <param name="size">Strip size.</param>
        /// <returns>This scenario.</returns>
        public Scenario Spikes(int x, int y, float angle = 0f, int size = 4)
        {
            XElement spike = Node("spike4", x, y);
            spike.SetAttributeValue("angle", Num(angle));
            spike.SetAttributeValue("size", Num(size));
            return Add(spike);
        }

        /// <summary>Adds a conveyor belt.</summary>
        /// <param name="x">Level-space X of the belt centre.</param>
        /// <param name="y">Level-space Y of the belt centre.</param>
        /// <param name="length">Belt length in level units.</param>
        /// <param name="width">Belt width (thickness) in level units.</param>
        /// <param name="velocity">Belt speed.</param>
        /// <param name="angle">Belt rotation in degrees.</param>
        /// <returns>This scenario.</returns>
        public Scenario Conveyor(int x, int y, int length = 120, int width = 20, float velocity = 40f, float angle = 0f)
        {
            XElement belt = Node("conveyorBelt", x, y);
            belt.SetAttributeValue("length", Num(length));
            belt.SetAttributeValue("width", Num(width));
            belt.SetAttributeValue("angle", Num(angle));
            belt.SetAttributeValue("velocity", Num(velocity));
            belt.SetAttributeValue("direction", "forward");
            belt.SetAttributeValue("type", "auto");
            return Add(belt);
        }

        /// <summary>Adds a DJ disc (rotated circle).</summary>
        /// <param name="x">Level-space X of the centre.</param>
        /// <param name="y">Level-space Y of the centre.</param>
        /// <param name="size">Disc radius in level units.</param>
        /// <returns>This scenario.</returns>
        public Scenario Disc(int x, int y, int size = 40)
        {
            XElement circle = Node("rotatedCircle", x, y);
            circle.SetAttributeValue("size", Num(size));
            circle.SetAttributeValue("handleAngle", Num(0));
            circle.SetAttributeValue("oneHandle", Flag(false));
            return Add(circle);
        }

        /// <summary>Sets a gameDesign attribute, for scenarios that need a non-default level setting.</summary>
        /// <param name="name">Attribute name.</param>
        /// <param name="value">Attribute value.</param>
        /// <returns>This scenario.</returns>
        public Scenario Design(string name, string value)
        {
            design.Add(new XAttribute(name, value));
            return this;
        }

        /// <summary>Sets the authored map dimensions.</summary>
        public Scenario MapSize(int mapWidth, int mapHeight)
        {
            width = mapWidth;
            height = mapHeight;
            return this;
        }

        /// <summary>Sets the authored special-tutorial identifier.</summary>
        public Scenario Special(int specialId)
        {
            special = specialId;
            return this;
        }

        /// <summary>Boots the scenario and returns the live scene.</summary>
        /// <returns>The loaded scene, ready to be stepped.</returns>
        public GameScene Build()
        {
            _ = HeadlessGame.Boot();

            XElement settings = new(
                "layer",
                new XAttribute("name", "settings"),
                new XElement(
                    "map",
                    new XAttribute("gridSize", 32),
                    new XAttribute("width", width),
                    new XAttribute("height", height)),
                new XElement(
                    "gameDesign",
                    new XAttribute("ropePhysicsSpeed", "1.0"),
                    new XAttribute("special", special),
                    new XAttribute("twoParts", Flag(splitCandy)),
                    design));

            XElement map = new(
                "map",
                settings,
                new XElement("layer", new XAttribute("name", "Objects"), objects));

            GameScene scene = HeadlessGame.LoadScenarioMap(map);

            // Win/loss both call straight into the delegate; a scenario that ends must not die on a
            // null one. Tests read the counts back through SceneProbe.Outcomes.
            scene.gameSceneDelegate = new RecordingSceneDelegate();
            return scene;
        }

        private static XElement Node(string name, int x, int y)
        {
            return new XElement(
                name,
                new XAttribute("x", Num(x)),
                new XAttribute("y", Num(y)));
        }

        private static string Num(float value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        private static string Flag(bool value)
        {
            return value ? "true" : "false";
        }

        private Scenario Add(XElement node)
        {
            objects.Add(node);
            return this;
        }

        /// <summary>Level-space position converted to the world vector the scene uses.</summary>
        /// <param name="x">Level-space X.</param>
        /// <param name="y">Level-space Y.</param>
        /// <returns>World-space vector.</returns>
        public Vector World(int x, int y)
        {
            return new Vector(WorldX(x), WorldY(y));
        }
    }
}
