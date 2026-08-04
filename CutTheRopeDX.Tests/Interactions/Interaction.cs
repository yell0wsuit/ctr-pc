using System;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.GameMain;

namespace CutTheRopeDX.Tests.Interactions
{
    /// <summary>
    /// Choreography shared by the interaction-matrix tests: run the real update loop until a
    /// condition holds, and park a candy where an object can act on it.
    /// </summary>
    internal static class Interaction
    {
        /// <summary>Frames a setup step may take before the test treats it as failed.</summary>
        public const int DefaultMaxFrames = 600;

        /// <summary>Steps the scene until <paramref name="condition"/> holds or the budget runs out.</summary>
        /// <param name="scene">Scene to advance.</param>
        /// <param name="condition">Condition to wait for.</param>
        /// <param name="maxFrames">Frame budget.</param>
        /// <returns><see langword="true"/> when the condition held within the budget.</returns>
        public static bool StepUntil(GameScene scene, Func<bool> condition, int maxFrames = DefaultMaxFrames)
        {
            return StepUntil(scene, null, condition, maxFrames);
        }

        /// <summary>
        /// Steps the scene until <paramref name="condition"/> holds, running
        /// <paramref name="beforeEachFrame"/> first every frame. The hook is how a trigger object is
        /// kept on a candy that is being swung by a rope or carried by a holder.
        /// </summary>
        /// <param name="scene">Scene to advance.</param>
        /// <param name="beforeEachFrame">Run before every frame, including the first.</param>
        /// <param name="condition">Condition to wait for.</param>
        /// <param name="maxFrames">Frame budget.</param>
        /// <returns><see langword="true"/> when the condition held within the budget.</returns>
        public static bool StepUntil(GameScene scene, Action beforeEachFrame, Func<bool> condition, int maxFrames = DefaultMaxFrames)
        {
            for (int frame = 0; frame < maxFrames; frame++)
            {
                if (condition())
                {
                    return true;
                }

                beforeEachFrame?.Invoke();
                HeadlessGame.StepFrames(scene, 1);
            }

            return condition();
        }

        /// <summary>
        /// Teleports a candy (physics point and visual) to a world position and kills its velocity,
        /// so an interaction is tested at a known place instead of wherever gravity took it.
        /// </summary>
        /// <param name="candy">Candy to move.</param>
        /// <param name="position">World position.</param>
        public static void PlaceCandyAt(CandyContext candy, Vector position)
        {
            PlaceBodyAt(candy.WholeBody, position);
        }

        /// <summary>
        /// Teleports one physical body to a world position and kills its velocity. The
        /// <see cref="CandyContext"/> overload only ever reaches a whole body, so this is how a split
        /// half - which its logical candy has no single point for - is parked where a test wants it.
        /// </summary>
        /// <param name="body">Body to move.</param>
        /// <param name="position">World position.</param>
        public static void PlaceBodyAt(CandyBody body, Vector position)
        {
            body.Point.pos = position;
            body.Point.prevPos = position;
            body.Point.v = new Vector(0f, 0f);
            body.Point.posDelta = new Vector(0f, 0f);
            if (body.Visual != null)
            {
                body.Visual.x = position.X;
                body.Visual.y = position.Y;
            }
        }

        /// <summary>Holds one body in place so a slow interaction is not outrun by gravity.</summary>
        /// <param name="body">Body to hold.</param>
        public static void Hover(CandyBody body)
        {
            body.Point.disableGravity = true;
        }

        /// <summary>World position of a scene object that carries x/y (rocket, hat, disc, ...).</summary>
        /// <param name="x">Object X.</param>
        /// <param name="y">Object Y.</param>
        /// <returns>The position as a vector.</returns>
        public static Vector At(float x, float y)
        {
            return new Vector(x, y);
        }

        /// <summary>Holds a candy in place so a slow interaction is not outrun by gravity.</summary>
        /// <param name="candy">Candy to hold.</param>
        public static void Hover(CandyContext candy)
        {
            Hover(candy.WholeBody);
        }

        /// <summary>Lets a held candy fall again.</summary>
        /// <param name="candy">Candy to release.</param>
        public static void Drop(CandyContext candy)
        {
            candy.WholeBody.Point.disableGravity = false;
        }
    }
}
