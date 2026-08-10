using System;
using System.Reflection;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests.Interactions
{
    /// <summary>
    /// Forms an attachment on a candy, or fires one of the matrix's events at it, by driving the
    /// real update loop. Every helper moves the *object* onto the candy and keeps it there each
    /// frame, so a candy that is swinging on a rope or pinned by a holder (hand, mouse, ants) can
    /// still be acted on. Each helper asserts the interaction actually happened, so a matrix test
    /// that reaches its own assertions is known to have started from the state it claims.
    /// </summary>
    internal static class Act
    {
        /// <summary>Frames run after an interaction fires, so its consequences land.</summary>
        private const int SettleFrames = 3;

        /// <summary>World Y well past the kill line of any scenario-sized map.</summary>
        private const float OffScreenY = 4000f;

        /// <summary>Binds the scene's rocket to the candy.</summary>
        /// <param name="scene">Scene under test.</param>
        /// <param name="candy">Candy to bind.</param>
        /// <returns>The bound rocket.</returns>
        public static Rocket BindRocket(GameScene scene, CandyContext candy, int rocketIndex = 0)
        {
            Rocket rocket = scene.Rockets()[rocketIndex];
            Chase(
                scene,
                candy,
                position =>
                {
                    MoveTo(rocket, position);
                    rocket.point.pos = position;
                    rocket.point.prevPos = position;
                },
                () => candy.Lifecycle.Attachments.Rocket == rocket,
                "the rocket never bound to the candy");
            return rocket;
        }

        /// <summary>Captures the candy in the scene's bubble.</summary>
        /// <param name="scene">Scene under test.</param>
        /// <param name="candy">Candy to capture.</param>
        /// <returns>The capturing bubble.</returns>
        public static Bubble CaptureInBubble(GameScene scene, CandyContext candy, int bubbleIndex = 0)
        {
            Bubble bubble = scene.Bubbles()[bubbleIndex];
            Chase(scene, candy, position => MoveTo(bubble, position), () => candy.WholeBody.Bubble == bubble, "the bubble never captured the candy");
            return bubble;
        }

        /// <summary>
        /// Pushes a bubble onto the candy without requiring it to capture. Used where the matrix
        /// says the bubble loses the exchange - a rocket-bound or snail-ridden candy pops it.
        /// </summary>
        /// <param name="scene">Scene under test.</param>
        /// <param name="candy">Candy to touch.</param>
        /// <param name="bubbleIndex">Index of the bubble in the scene.</param>
        /// <returns>The bubble that was pushed.</returns>
        public static Bubble PushBubbleAgainst(GameScene scene, CandyContext candy, int bubbleIndex = 0)
        {
            Bubble bubble = scene.Bubbles()[bubbleIndex];
            Chase(scene, candy, position => MoveTo(bubble, position), () => bubble.popped, "the bubble never reached the candy");
            return bubble;
        }

        /// <summary>Attaches the scene's snail to the candy.</summary>
        /// <param name="scene">Scene under test.</param>
        /// <param name="candy">Candy to ride.</param>
        /// <returns>The riding snail.</returns>
        public static Snail RideSnail(GameScene scene, CandyContext candy, int snailIndex = 0)
        {
            Snail snail = scene.Snails()[snailIndex];
            Chase(
                scene,
                candy,
                position => MoveTo(snail, position),
                () => snail.state == Snail.SNAIL_STATE_ACTIVE && snail.AttachedPoint() == candy.WholeBody.Point,
                "the snail never attached to the candy");
            return snail;
        }

        /// <summary>Has the given hand grab the candy.</summary>
        /// <param name="scene">Scene under test.</param>
        /// <param name="candy">Candy to grab.</param>
        /// <param name="handIndex">Index of the hand in the scene.</param>
        /// <returns>The grabbing hand.</returns>
        public static MechanicalHand GrabWithHand(GameScene scene, CandyContext candy, int handIndex = 0)
        {
            MechanicalHand hand = scene.Hands()[handIndex];
            Chase(scene, candy, position => MoveClawTo(hand, position), () => candy.Lifecycle.Attachments.Hand == hand, "the hand never grabbed the candy");
            return hand;
        }

        /// <summary>Has the active mouse steal the candy.</summary>
        /// <param name="scene">Scene under test.</param>
        /// <param name="candy">Candy to steal.</param>
        /// <returns>The carrying mouse.</returns>
        public static Mouse CarryByMouse(GameScene scene, CandyContext candy)
        {
            Mouse mouse = scene.Mice()[0];
            Chase(scene, candy, position => MoveTo(mouse, position), () => scene.MouseCarries(candy), "the mouse never took the candy");
            return mouse;
        }

        /// <summary>
        /// Has the active mouse take the candy from where its hole already is, instead of moving the
        /// hole onto the candy. Use this when the point of the test is that the mouse drags the
        /// candy somewhere else - off an ant lane, for instance.
        /// </summary>
        /// <param name="scene">Scene under test.</param>
        /// <param name="candy">Candy to steal.</param>
        /// <returns>The carrying mouse.</returns>
        public static Mouse CarryByMouseWithoutMovingIt(GameScene scene, CandyContext candy)
        {
            Assert.True(
                Interaction.StepUntil(scene, () => scene.MouseCarries(candy)),
                "the mouse never took the candy");
            HeadlessGame.StepFrames(scene, SettleFrames);
            return scene.Mice()[0];
        }

        /// <summary>
        /// Waits for the ant conveyor to pick the candy up. Unlike the other helpers this one does
        /// not move anything: an ant segment is a fixed lane, and the candy has to be on it.
        /// </summary>
        /// <param name="scene">Scene under test.</param>
        /// <param name="candy">Candy to hand to the ants.</param>
        public static void CarryByAnts(GameScene scene, CandyContext candy)
        {
            Assert.True(
                Interaction.StepUntil(scene, () => candy.Lifecycle.Attachments.AntSegment != null),
                "the ants never picked up the candy");
        }

        /// <summary>
        /// Feeds the candy to one of the scene's Om Noms. An Om Nom falls asleep once it has been
        /// fed and never reacts again, so a scenario that feeds two candies needs two of them and
        /// must name which one eats this time.
        /// </summary>
        /// <param name="scene">Scene under test.</param>
        /// <param name="candy">Candy to feed.</param>
        /// <param name="targetIndex">Index of the Om Nom in the scene.</param>
        public static void Eat(GameScene scene, CandyContext candy, int targetIndex = 0)
        {
            TargetContext target = scene.Targets()[targetIndex];
            Chase(
                scene,
                candy,
                position => MoveMouthTo(target.targetObject, position),
                // Becoming fed is what only eating does; candy-gone alone would also be true of a
                // candy that drifted off screen while Om Nom was chasing it.
                () => target.Feeding.IsFed,
                "Om Nom never ate the candy");
        }

        /// <summary>
        /// Cuts a grab's rope by swiping across it, the way a player does. The swipe crosses the
        /// rope's first segment square-on, so it lands whatever angle the rope is hanging at.
        /// </summary>
        /// <param name="scene">Scene under test.</param>
        /// <param name="grab">Grab whose rope to cut.</param>
        public static void CutRope(GameScene scene, Grab grab)
        {
            Bungee rope = grab.Rope;
            Assert.NotNull(rope);
            Assert.Equal(-1, rope.cut);

            Vector from = rope.parts[0].pos;
            Vector to = rope.parts[1].pos;
            Vector midpoint = new((from.X + to.X) / 2f, (from.Y + to.Y) / 2f);
            float dx = to.X - from.X;
            float dy = to.Y - from.Y;
            float length = MathF.Sqrt((dx * dx) + (dy * dy));
            Assert.True(length > 0f, "the rope segment being cut has no length");

            // Unit normal to the segment, so the swipe crosses it rather than running along it.
            float nx = -dy / length;
            float ny = dx / length;
            Vector swipeStart = new(midpoint.X - (nx * SwipeReach), midpoint.Y - (ny * SwipeReach));
            Vector swipeEnd = new(midpoint.X + (nx * SwipeReach), midpoint.Y + (ny * SwipeReach));

            int cuts = scene.CutWithRazorOrLine1Line2Immediate(null, swipeStart, swipeEnd, true);
            Assert.True(cuts > 0, "the swipe cut no rope");
            HeadlessGame.StepFrames(scene, 1);
        }

        /// <summary>How far to either side of a rope a cutting swipe extends, in world units.</summary>
        private const float SwipeReach = 40f;

        /// <summary>Destroys the candy on the scene's spikes.</summary>
        /// <param name="scene">Scene under test.</param>
        /// <param name="candy">Candy to destroy.</param>
        public static void BreakOnSpikes(GameScene scene, CandyContext candy)
        {
            Spikes spikes = scene.SpikeStrips()[0];
            Chase(
                scene,
                candy,
                position =>
                {
                    spikes.x = position.X;
                    spikes.y = position.Y;
                    spikes.UpdateRotation();
                },
                () => candy.HasNoWholeBodyInPlay,
                "the spikes never broke the candy");

            // Losing is a two-stage event: the break tears the candy down, and the scheduled
            // GameLost that follows finishes the job (hands, mice). The matrix row describes the
            // finished state, so wait for the loss itself.
            Assert.True(
                Interaction.StepUntil(scene, () => scene.Outcomes().LostCount > 0),
                "the broken candy never lose the level");
        }

        /// <summary>
        /// Loses the candy off the bottom of the screen - the matrix's other "Lost" trigger, which
        /// the engine handles on its own path rather than through the hazard break.
        /// </summary>
        /// <param name="scene">Scene under test.</param>
        /// <param name="candy">Candy to lose.</param>
        public static void LoseOffScreen(GameScene scene, CandyContext candy)
        {
            Interaction.Drop(candy);
            Vector belowTheMap = new(candy.WholeBody.Point.pos.X, OffScreenY);
            Interaction.PlaceCandyAt(candy, belowTheMap);

            Assert.True(
                Interaction.StepUntil(scene, () => scene.Outcomes().LostCount > 0),
                "the candy that left the screen never lost the level");
            HeadlessGame.StepFrames(scene, SettleFrames);
        }

        /// <summary>Captures the candy in the scene's lantern.</summary>
        /// <param name="scene">Scene under test.</param>
        /// <param name="candy">Candy to capture.</param>
        public static void CaptureInLantern(GameScene scene, CandyContext candy)
        {
            Lantern lantern = Lantern.GetAllLanterns()[0];
            Chase(scene, candy, position => MoveTo(lantern, position), () => candy.Lifecycle.Attachments.InLantern, "the lantern never captured the candy");
        }

        /// <summary>
        /// Sends the candy into the scene's first magic hat. The hat is turned to face however the
        /// candy is travelling, because a hat only swallows a candy moving into its mouth - a
        /// bubbled candy rises and an ant-carried one slides sideways.
        /// </summary>
        /// <param name="scene">Scene under test.</param>
        /// <param name="candy">Candy to swallow.</param>
        public static void EnterHat(GameScene scene, CandyContext candy)
        {
            Sock hat = scene.Hats()[0];
            Chase(
                scene,
                candy,
                position =>
                {
                    MoveTo(hat, position);
                    hat.rotation = MouthAngleFacing(candy.WholeBody.Point.posDelta, hat.rotation);
                    hat.UpdateRotation();
                },
                () => candy.Lifecycle.Transport?.Sock != null,
                "the magic hat never took the candy");
        }

        /// <summary>
        /// Sends the candy into the scene's bamboo tube. <paramref name="mouth"/> must match the way
        /// the scenario authored the tube, and the way the candy is travelling.
        /// </summary>
        /// <param name="scene">Scene under test.</param>
        /// <param name="candy">Candy to swallow.</param>
        /// <param name="mouth">Direction the tube's catching hole faces.</param>
        public static void EnterBambooTube(GameScene scene, CandyContext candy, TubeMouth mouth)
        {
            BambooTube tube = scene.BambooTubes()[0];
            Vector toCentre = TubeGeometry.CentreDirection(mouth);
            float halfBody = tube.bb.w * 0.5f;
            Chase(
                scene,
                candy,
                position =>
                {
                    MoveTo(tube, new Vector(position.X + (toCentre.X * halfBody), position.Y + (toCentre.Y * halfBody)));
                    RefreshTubeHoles(tube);
                },
                () => candy.Lifecycle.Transport?.BambooTube != null,
                "the bamboo tube never took the candy");
        }

        /// <summary>
        /// Moves a scene element so it sits on a world position, hitbox included. Collision runs off
        /// the cached draw position, which the update loop recomputes for the candy but not for
        /// scenery, so a moved object would otherwise keep colliding where it was loaded.
        /// </summary>
        /// <param name="element">Element to move.</param>
        /// <param name="position">Target world position.</param>
        public static void MoveTo(BaseElement element, Vector position)
        {
            element.x = position.X;
            element.y = position.Y;
            BaseElement.CalculateTopLeft(element);
        }

        /// <summary>
        /// Shifts Om Nom so the mouth hitbox - a thin slit well below the sprite's origin - lands on
        /// a world position. Placing the origin there instead would hold the candy above the mouth,
        /// where it is never eaten.
        /// </summary>
        /// <param name="omNom">Om Nom's game object.</param>
        /// <param name="position">Target mouth position.</param>
        public static void MoveMouthTo(GameObject omNom, Vector position)
        {
            float mouthX = omNom.drawX + omNom.bb.x + (omNom.bb.w / 2f);
            float mouthY = omNom.drawY + omNom.bb.y + (omNom.bb.h / 2f);
            float dx = position.X - mouthX;
            float dy = position.Y - mouthY;
            omNom.x += dx;
            omNom.y += dy;
            omNom.drawX += dx;
            omNom.drawY += dy;
        }

        /// <summary>Shifts a hand so its claw lands on a world position.</summary>
        /// <param name="hand">Hand to move.</param>
        /// <param name="position">Target claw position.</param>
        public static void MoveClawTo(MechanicalHand hand, Vector position)
        {
            Vector claw = hand.ClawPosition();
            float dx = position.X - claw.X;
            float dy = position.Y - claw.Y;
            hand.x += dx;
            hand.y += dy;
            hand.drawX += dx;
            hand.drawY += dy;
            hand.cPoint.pos = hand.ClawPosition();
        }

        /// <summary>Taps a hand's claw, which is how a player makes it drop the candy.</summary>
        /// <param name="scene">Scene under test.</param>
        /// <param name="hand">Hand whose claw is tapped.</param>
        public static void TapClaw(GameScene scene, MechanicalHand hand)
        {
            Vector claw = hand.ClawPosition();
            Assert.True(scene.TouchDownXYIndex(claw.X, claw.Y, 0), "the claw tap was not handled");
            HeadlessGame.StepFrames(scene, 1);
        }

        /// <summary>
        /// Starts a segment rotation and makes it the hand's rotating segment, as a button press
        /// would. The segment must have been created rotatable.
        /// </summary>
        /// <param name="scene">Scene under test.</param>
        /// <param name="hand">Hand to rotate.</param>
        /// <param name="segmentIndex">Segment to rotate.</param>
        public static void RotateSegment(GameScene scene, MechanicalHand hand, int segmentIndex = 0)
        {
            MechanicalHandSegment segment = hand.SegmentAtIndex(segmentIndex);
            segment.Rotate();
            Assert.NotNull(segment.GetCurrentTimeline());
            hand.rotatingSegment = segment;
            HeadlessGame.StepFrames(scene, 2);
        }

        /// <summary>
        /// Makes a hand eligible to clap, as pressing one of its rotate buttons does.
        /// </summary>
        /// <param name="hand">Hand to arm.</param>
        public static void ArmClap(MechanicalHand hand)
        {
            hand.ArmClap();
        }

        /// <summary>
        /// Settles a releasing hand to idle and reports whether that settle still owed the drop
        /// sound. Sounds are not observable headlessly, so this seam is the only way to pin that
        /// axis; its body is rewritten when the settle transition lands, leaving every caller's
        /// assertions untouched.
        /// </summary>
        /// <param name="hand">Hand to settle.</param>
        /// <returns><see langword="true"/> when the settle owed the drop sound.</returns>
        public static bool SettleOwedDropSound(MechanicalHand hand)
        {
            Assert.Equal(MechanicalHandState.Releasing, hand.State);
            HandSettle settle = hand.TrySettleToIdle(MechanicalHand.MH_RELEASE_DISTANCE + 1f);
            Assert.NotEqual(HandSettle.Stayed, settle);
            return settle == HandSettle.SettledOwingDropSound;
        }

        /// <summary>
        /// Recomputes a tube's hole positions after it has been moved. The tube caches them and
        /// only refreshes on rotation, so a moved tube would otherwise keep catching candy at the
        /// place it was loaded.
        /// </summary>
        /// <param name="tube">Tube that moved.</param>
        private static void RefreshTubeHoles(BambooTube tube)
        {
            MethodInfo refresh = typeof(BambooTube).GetMethod(
                "UpdateBambooRotation",
                BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new MissingMethodException(nameof(BambooTube), "UpdateBambooRotation");
            _ = refresh.Invoke(tube, null);
        }

        /// <summary>
        /// Rotation that puts a mouth square across <paramref name="motion"/>. The engine tests the
        /// candy's travel in the mouth's own frame and demands a non-negative downward component,
        /// which a mouth turned to motion angle minus 90 degrees always satisfies.
        /// </summary>
        /// <param name="motion">The candy's per-frame travel.</param>
        /// <param name="current">Rotation to keep when the candy is not moving.</param>
        /// <returns>The rotation in degrees.</returns>
        private static float MouthAngleFacing(Vector motion, float current)
        {
            const float stillThreshold = 1e-4f;
            return ((motion.X * motion.X) + (motion.Y * motion.Y)) < stillThreshold
                ? current
                : (float)(Math.Atan2(motion.Y, motion.X) * 180d / Math.PI) - 90f;
        }

        private static void Chase(GameScene scene, CandyContext candy, Action<Vector> place, Func<bool> done, string message)
        {
            place(candy.WholeBody.Point.pos);
            Assert.True(
                Interaction.StepUntil(scene, () => place(candy.WholeBody.Point.pos), done),
                message);

            // The matrix describes the settled state, not the state mid-frame: several teardowns
            // (the mouse's carry flag, hand release, rope hiding) land on the frame after the one
            // that fired the event.
            HeadlessGame.StepFrames(scene, SettleFrames);
        }
    }
}
