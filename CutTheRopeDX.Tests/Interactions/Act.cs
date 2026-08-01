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

        /// <summary>Binds the scene's rocket to the candy.</summary>
        /// <param name="scene">Scene under test.</param>
        /// <param name="candy">Candy to bind.</param>
        /// <returns>The bound rocket.</returns>
        public static Rocket BindRocket(GameScene scene, CandyContext candy)
        {
            Rocket rocket = scene.Rockets()[0];
            Chase(
                scene,
                candy,
                position =>
                {
                    rocket.x = position.X;
                    rocket.y = position.Y;
                    rocket.point.pos = position;
                    rocket.point.prevPos = position;
                },
                () => candy.HasActiveRocket,
                "the rocket never bound to the candy");
            return rocket;
        }

        /// <summary>Captures the candy in the scene's bubble.</summary>
        /// <param name="scene">Scene under test.</param>
        /// <param name="candy">Candy to capture.</param>
        /// <returns>The capturing bubble.</returns>
        public static Bubble CaptureInBubble(GameScene scene, CandyContext candy)
        {
            Bubble bubble = scene.Bubbles()[0];
            Chase(scene, candy, position => MoveTo(bubble, position), () => candy.bubble != null, "the bubble never captured the candy");
            return bubble;
        }

        /// <summary>Attaches the scene's snail to the candy.</summary>
        /// <param name="scene">Scene under test.</param>
        /// <param name="candy">Candy to ride.</param>
        /// <returns>The riding snail.</returns>
        public static Snail RideSnail(GameScene scene, CandyContext candy)
        {
            Snail snail = scene.Snails()[0];
            Chase(scene, candy, position => MoveTo(snail, position), () => scene.SnailCount(candy) == 1, "the snail never attached to the candy");
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
            Chase(scene, candy, position => MoveClawTo(hand, position), () => candy.capturingHand == hand, "the hand never grabbed the candy");
            return hand;
        }

        /// <summary>Has the active mouse steal the candy.</summary>
        /// <param name="scene">Scene under test.</param>
        /// <param name="candy">Candy to steal.</param>
        /// <returns>The carrying mouse.</returns>
        public static Mouse CarryByMouse(GameScene scene, CandyContext candy)
        {
            Mouse mouse = scene.Mice()[0];
            Chase(scene, candy, position => MoveTo(mouse, position), () => candy.carriedByMouse, "the mouse never took the candy");
            return mouse;
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
                Interaction.StepUntil(scene, () => candy.antSegment != null),
                "the ants never picked up the candy");
        }

        /// <summary>Feeds the candy to the nearest Om Nom.</summary>
        /// <param name="scene">Scene under test.</param>
        /// <param name="candy">Candy to feed.</param>
        public static void Eat(GameScene scene, CandyContext candy)
        {
            TargetContext target = scene.Targets()[0];
            Chase(
                scene,
                candy,
                position => MoveMouthTo(target.targetObject, position),
                // Falling asleep is what only eating does; candy-gone alone would also be true of a
                // candy that drifted off screen while Om Nom was chasing it.
                () => target.asleep,
                "Om Nom never ate the candy");
        }

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
                () => candy.noCandy || scene.PrimaryCandyGone(),
                "the spikes never broke the candy");

            // Losing is a two-stage event: the break tears the candy down, and the scheduled
            // GameLost that follows finishes the job (hands, mice). The matrix row describes the
            // finished state, so wait for the loss itself.
            Assert.True(
                Interaction.StepUntil(scene, () => scene.Outcomes().LostCount > 0),
                "the broken candy never lose the level");
        }

        /// <summary>Captures the candy in the scene's lantern.</summary>
        /// <param name="scene">Scene under test.</param>
        /// <param name="candy">Candy to capture.</param>
        public static void CaptureInLantern(GameScene scene, CandyContext candy)
        {
            Lantern lantern = Lantern.GetAllLanterns()[0];
            Chase(scene, candy, position => MoveTo(lantern, position), () => candy.inLantern, "the lantern never captured the candy");
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
                    hat.rotation = MouthAngleFacing(candy.point.posDelta, hat.rotation);
                    hat.UpdateRotation();
                },
                () => candy.targetSock != null,
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
                () => candy.targetBambooTube != null,
                "the bamboo tube never took the candy");
        }

        /// <summary>Moves a scene element so it sits on a world position.</summary>
        /// <param name="element">Element to move.</param>
        /// <param name="position">Target world position.</param>
        public static void MoveTo(BaseElement element, Vector position)
        {
            element.x = position.X;
            element.y = position.Y;
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
            place(candy.point.pos);
            Assert.True(
                Interaction.StepUntil(scene, () => place(candy.point.pos), done),
                message);

            // The matrix describes the settled state, not the state mid-frame: several teardowns
            // (the mouse's carry flag, hand release, rope hiding) land on the frame after the one
            // that fired the event.
            HeadlessGame.StepFrames(scene, SettleFrames);
        }
    }
}
