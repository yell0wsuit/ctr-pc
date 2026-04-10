using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Physics;

namespace CutTheRopeDX.GameMain
{
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Per-frame driver for the ant-conveyor system. Updates all paths, manages the
        /// wait-before-attach flag, drains the segment cooldown, handles detach when the candy
        /// leaves a segment's internal rectangle, and runs the priority search for a new segment
        /// to carry the candy.
        /// </summary>
        /// <param name="delta">Elapsed time in seconds since the last frame.</param>
        private void UpdateAntConveyor(float delta)
        {
            if (antsPaths == null || antsPaths.Count == 0)
            {
                return;
            }

            foreach (AntsPath antsPath in antsPaths)
            {
                antsPath.Update(delta);
            }

            if (star == null || antsPathsSegments == null || antsPathsSegments.Count == 0)
            {
                return;
            }

            if (candyWaitForFlyBeforeAttachingToConveyor)
            {
                candyWaitForFlyBeforeAttachingToConveyor = false;
                foreach (AntsPathSegment segment in antsPathsSegments)
                {
                    if (segment.ContainsPoint(star.pos, external: true))
                    {
                        candyWaitForFlyBeforeAttachingToConveyor = true;
                        break;
                    }
                }
            }

            if (lastAntsPathSegmentWithCandy != null
                && antsPathSegmentWithCandy == null
                && Mover.MoveVariableToTarget(ref antsPathSegmentCooldown, 0f, 1f, 0.01f))
            {
                lastAntsPathSegmentWithCandy = null;
            }

            for (int i = 0; i < antsPathsSegments.Count; i++)
            {
                AntsPathSegment segment = antsPathsSegments[i];
                if (!segment.interacting || segment.interactionTime <= AntConveyorLogic.CarrierSnapTimeThreshold)
                {
                    continue;
                }

                if (segment.ContainsPoint(star.pos))
                {
                    continue;
                }

                bool shouldSlowStop = true;
                for (int j = 0; j < antsPathsSegments.Count; j++)
                {
                    if (j == i)
                    {
                        continue;
                    }

                    if (antsPathsSegments[j].ContainsPoint(star.pos, external: true))
                    {
                        shouldSlowStop = false;
                        break;
                    }
                }

                star.disableGravity = activeRocket != null;
                segment.StopInteractionWithCandySlow(shouldSlowStop);
                antsPathSegmentWithCandy = null;

                if (shouldSlowStop)
                {
                    PlayAntConveyorDetachSound();
                }
            }

            bool hasInteractingSegment = false;
            foreach (AntsPathSegment segment in antsPathsSegments)
            {
                hasInteractingSegment |= segment.interacting;
            }

            if (!hasInteractingSegment)
            {
                foreach (AntsPathSegment segment in antsPathsSegments)
                {
                    if (TryStartAntInteraction(segment, useExternalBounds: false))
                    {
                        hasInteractingSegment = true;
                        break;
                    }
                }
            }

            if (!hasInteractingSegment)
            {
                foreach (AntsPathSegment segment in antsPathsSegments)
                {
                    if (TryStartAntInteraction(segment, useExternalBounds: true))
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Overwrites the candy's world position with the carrier follow position computed by
        /// <see cref="AntConveyorLogic.ComputeCarrierFollowPosition"/>. No-op if no segment is
        /// currently carrying the candy.
        /// </summary>
        private void ApplyAntCarryToCandyPosition()
        {
            if (antsPathSegmentWithCandy == null || star == null)
            {
                return;
            }

            float scale = GetAntConveyorScale();
            float snapDistance = AntConveyorLogic.GetCarrierSnapDistance(scale);

            Vector nextPos = AntConveyorLogic.ComputeCarrierFollowPosition(
                star.pos,
                antsPathSegmentWithCandy.interactionPoint,
                antsPathSegmentWithCandy.container?.InteractionTime ?? 0f,
                snapDistance);

            star.pos = nextPos;

            if (activeRocket?.point != null)
            {
                activeRocket.point.pos = nextPos;
            }
        }

        /// <summary>
        /// Handles a touch event on the candy while it is being carried by the conveyor.
        /// If the touch lands inside the carrier touch zone the candy is detached and released to physics.
        /// </summary>
        /// <param name="point">The candy's constraint point.</param>
        /// <param name="tx">Touch X coordinate in screen space.</param>
        /// <param name="ty">Touch Y coordinate in screen space.</param>
        /// <returns><see langword="true"/> if the touch was consumed; otherwise, <see langword="false"/>.</returns>
        private bool HandleConveyorTouchConstraintedPointXY(ConstraintedPoint point, float tx, float ty)
        {
            if (point == null || antsPathSegmentWithCandy == null)
            {
                return false;
            }

            Vector touchWorld = Vect(tx + camera.pos.X, ty + camera.pos.Y);
            float halfSize = AntConveyorLogic.GetCarrierTouchHalfSize(GetAntConveyorScale());

            if (!AntConveyorLogic.IsPointInCarrierTouchZone(touchWorld, point.pos, halfSize))
            {
                return false;
            }

            candyWaitForFlyBeforeAttachingToConveyor = true;
            antsPathSegmentWithCandy.StopInteractionWithCandySlow(true);
            antsPathSegmentWithCandy = null;
            PlayAntConveyorDetachSound();
            star.disableGravity = activeRocket != null;
            return true;
        }

        /// <summary>
        /// Immediately detaches the candy from any active segment and restores all segments to the
        /// interactable state. Used when the level resets.
        /// </summary>
        private void ResetConveyor()
        {
            if (antsPathsSegments == null || antsPathsSegments.Count == 0)
            {
                return;
            }

            foreach (AntsPathSegment segment in antsPathsSegments)
            {
                if (segment.interacting)
                {
                    segment.StopInteractionWithCandySlow(false);
                    star.disableGravity = activeRocket != null;
                    PlayAntConveyorDetachSound();
                }

                segment.canInteract = true;
            }

            candyWaitForFlyBeforeAttachingToConveyor = false;
            antsPathSegmentWithCandy = null;
            lastAntsPathSegmentWithCandy = null;
            antsPathSegmentCooldown = 0f;
        }

        /// <summary>Prevents all segments from attaching to the candy. Used during certain game-state transitions.</summary>
        private void BlockConveyor()
        {
            if (antsPathsSegments == null)
            {
                return;
            }

            foreach (AntsPathSegment segment in antsPathsSegments)
            {
                segment.canInteract = false;
            }
        }

        /// <summary>Re-enables all segments to attach to the candy after a <see cref="BlockConveyor"/> call.</summary>
        private void UnblockConveyor()
        {
            if (antsPathsSegments == null)
            {
                return;
            }

            foreach (AntsPathSegment segment in antsPathsSegments)
            {
                segment.canInteract = true;
            }
        }

        /// <summary>
        /// Attempts to attach the candy to <paramref name="segment"/>. Returns <see langword="true"/> and starts the
        /// interaction if all preconditions pass: the segment is idle and interactable, the candy is
        /// not in the wait-before-attach state, and the candy lies inside the segment's bounding rectangle.
        /// </summary>
        /// <param name="segment">The segment to test.</param>
        /// <param name="useExternalBounds">Whether to use the wider external bounding rectangle.</param>
        /// <returns><see langword="true"/> if the candy was attached to the segment; otherwise, <see langword="false"/>.</returns>
        private bool TryStartAntInteraction(AntsPathSegment segment, bool useExternalBounds)
        {
            if (segment == null
                || segment.interacting
                || !segment.canInteract
                || candyWaitForFlyBeforeAttachingToConveyor
                || segment == lastAntsPathSegmentWithCandy)
            {
                return false;
            }

            bool contains = segment.ContainsPoint(star.pos, useExternalBounds);
            if (!contains)
            {
                return false;
            }

            star.disableGravity = true;
            antsPathSegmentWithCandy = segment;
            lastAntsPathSegmentWithCandy = segment;
            antsPathSegmentCooldown = 0.3f;

            if ((segment.container?.InteractionTime ?? 0f) == 0f)
            {
                PlayAntConveyorAttachSound();
            }

            segment.StartInteractionWithConstraitedPoint(star);

            if (candyBubble != null)
            {
                PopCandyBubble(false);
            }

            if (star.weight > 1f)
            {
                star.SetWeight(1f);
                DetachActiveSnails();
            }

            return true;
        }

        /// <summary>Returns the scale factor for ant-conveyor sizing. Always 1 on PC.</summary>
        /// <returns>The device scale multiplier.</returns>
        private static float GetAntConveyorScale()
        {
            return 1f;
        }

        /// <summary>
        /// Calls <see cref="Bungee.Update(float)"/> for <paramref name="rope"/> while preventing rope tension
        /// from displacing the candy when it is being carried by the ant conveyor.
        /// The candy position is locked before the rope physics step and restored afterward.
        /// </summary>
        /// <param name="rope">The bungee rope to update.</param>
        /// <param name="delta">Elapsed time in seconds since the last frame.</param>
        private void UpdateRopeWithAntCarryOverride(Bungee rope, float delta)
        {
            if (rope == null)
            {
                return;
            }

            if (antsPathSegmentWithCandy == null || star == null || rope.tail != star)
            {
                rope.Update(delta * ropePhysicsSpeed);
                return;
            }

            // Keep rope simulation running, but don't let it displace candy while ants carry it.
            Vector lockedCandyPos = star.pos;
            rope.Update(delta * ropePhysicsSpeed);
            star.pos = lockedCandyPos;
        }

        /// <summary>Plays the sound effect for the candy attaching to the ant conveyor.</summary>
        private static void PlayAntConveyorAttachSound()
        {
            CTRSoundMgr.PlaySound(Resources.Snd.ExpAntsTakeCandy);
        }

        /// <summary>Plays the sound effect for the candy detaching from the ant conveyor.</summary>
        private static void PlayAntConveyorDetachSound()
        {
            CTRSoundMgr.PlaySound(Resources.Snd.ExpAntsDropCandy);
        }
    }
}
