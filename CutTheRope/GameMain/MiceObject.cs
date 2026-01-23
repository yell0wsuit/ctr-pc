using System.Collections.Generic;
using System.Linq;

using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Sfe;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Manages all mouse entities within a game scene, including activation,
    /// rendering, candy handoff, and mouse switching logic.
    /// </summary>
    internal sealed class MiceObject(GameScene scene)
    {
        /// <summary>
        /// Updates all registered mice.
        /// </summary>
        /// <param name="delta">Elapsed time since the last update, in seconds.</param>
        public void Update(float delta)
        {
            foreach (Mouse mouse in mice)
            {
                mouse?.Update(delta);
            }
        }

        /// <summary>
        /// Draws the mouse holes for all registered mice.
        /// </summary>
        public void DrawHoles()
        {
            foreach (Mouse mouse in mice)
            {
                mouse?.DrawHole();
            }
        }

        /// <summary>
        /// Iterates through all registered mice and draws only the currently active mouse.
        /// </summary>
        public void DrawMice()
        {
            foreach (Mouse mouse in mice)
            {
                if (mouse == activeMouse)
                {
                    mouse.DrawMouse();
                }
            }
        }

        /// <summary>
        /// Registers a mouse with a given index and initializes shared sprite resources
        /// if needed. May activate and spawn the mouse depending on index rules.
        /// </summary>
        /// <param name="mouse">The mouse instance to register.</param>
        /// <param name="index">Logical index used for ordering and activation.</param>
        public void RegisterMouse(Mouse mouse, int index)
        {
            mouse.index = index;
            _ = mice.AddObject(mouse);

            sharedSpriteContainer ??= CreateSharedSprites();

            bool hasIndexOne = mice.Any(m => m != null && m.index == 1);
            if (sharedSpriteContainer.HasValue && (index == 1 || (activeMouse == null && !hasIndexOne)))
            {
                activeMouse = mouse;
                activeIndex = index;
                mouse.Spawn(sharedSpriteContainer.Value, carriedCandy, carriedStar);
            }
        }

        /// <summary>
        /// Checks whether the active mouse is within grab range of a target point.
        /// </summary>
        /// <param name="target">The constrained point to test against.</param>
        /// <returns>
        /// <c>true</c> if the active mouse exists, is active, and within grab radius;
        /// otherwise <c>false</c>.
        /// </returns>
        public bool IsActiveMouseInRange(ConstraintedPoint target)
        {
            Mouse active = activeMouse;
            return active != null && active.IsActive && active.IsWithinGrabRadius(target);
        }

        /// <summary>
        /// Commands the active mouse to grab a candy from a star point.
        /// </summary>
        /// <param name="star">The constrained star point.</param>
        /// <param name="candy">The candy game object.</param>
        /// <param name="isLeft">
        /// Indicates whether the interaction originates from the left side
        /// (used for rope release logic).
        /// </param>
        public void GrabWithActiveMouse(ConstraintedPoint star, GameObject candy, bool isLeft)
        {
            if (activeMouse == null || activeMouse.HasCandy)
            {
                return;
            }

            scene.ReleaseAllRopes(isLeft);
            carriedStar = star;
            carriedCandy = candy;
            activeMouse.GrabCandy(star, candy);
        }

        /// <summary>
        /// Indicates whether the active mouse is currently holding candy.
        /// </summary>
        /// <returns><c>true</c> if the active mouse has candy; otherwise <c>false</c>.</returns>
        public bool ActiveMouseHasCandy()
        {
            return activeMouse?.HasCandy ?? false;
        }

        /// <summary>
        /// Handles click interaction for dropping candy from the active mouse.
        /// </summary>
        /// <param name="x">X coordinate of the click.</param>
        /// <param name="y">Y coordinate of the click.</param>
        /// <returns>
        /// <c>true</c> if the click was handled and candy was dropped;
        /// otherwise <c>false</c>.
        /// </returns>
        public bool HandleClick(float x, float y)
        {
            if (activeMouse == null || !activeMouse.HasCandy)
            {
                return false;
            }

            if (activeMouse.IsClickable(x, y))
            {
                activeMouse.DropCandyAndRetreat();
                carriedStar = null;
                carriedCandy = null;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Advances control to the next mouse in index order, transferring
        /// any carried candy and star state.
        /// </summary>
        public void AdvanceToNextMouse()
        {
            if (advanceLocked || !sharedSpriteContainer.HasValue || activeMouse == null)
            {
                return;
            }

            List<Mouse> ordered = [.. mice.Where(m => m != null).OrderBy(m => m.index)];
            Mouse currentMouse = ordered.FirstOrDefault(mouse => mouse.index == activeIndex);
            if (currentMouse == null || ordered.Count == 0)
            {
                return;
            }

            int currentIdx = ordered.IndexOf(currentMouse);
            int nextIdx = (currentIdx + 1) % ordered.Count;
            Mouse nextMouse = ordered[nextIdx];

            (ConstraintedPoint star, GameObject candy) = currentMouse.DetachCarriedCandy();
            carriedStar = star;
            carriedCandy = candy;

            activeIndex = nextMouse.index;
            activeMouse = nextMouse;
            nextMouse.Spawn(sharedSpriteContainer.Value, carriedCandy, carriedStar);
        }

        /// <summary>
        /// Locks the active mouse, preventing further advancement
        /// to other mice.
        /// </summary>
        public void LockActiveMouse()
        {
            advanceLocked = true;
            activeMouse?.Lock();
        }

        /// <summary>
        /// Creates and configures the shared sprite container used by all mice.
        /// </summary>
        /// <returns>A populated <see cref="Mouse.SharedMouseSprites"/> instance.</returns>
        private static Mouse.SharedMouseSprites CreateSharedSprites()
        {
            BaseElement container = new()
            {
                anchor = 18,
                parentAnchor = 18
            };

            Animation body = Animation.Animation_createWithResID(Resources.Img.ObjGap);
            body.anchor = body.parentAnchor = 18;
            body.DoRestoreCutTransparency();

            body.AddAnimationWithIDDelayLoopCountSequence(
                (int)MouseAnimationId.EntryEmpty,
                0.05f,
                Timeline.LoopType.TIMELINE_NO_LOOP,
                3,
                Mouse.ImgObjGapMouseStartQuad,
                [
                    Mouse.ImgObjGapMouseStartQuad + 1,
                    Mouse.ImgObjGapMouseStartQuad + 2
                ]);

            body.AddAnimationWithIDDelayLoopCountSequence(
                (int)MouseAnimationId.EntryWithCandy,
                0.05f,
                Timeline.LoopType.TIMELINE_NO_LOOP,
                3,
                Mouse.ImgObjGapMouseStartQuad + 3,
                [
                    Mouse.ImgObjGapMouseStartQuad + 4,
                    Mouse.ImgObjGapMouse0008Quad
                ]);

            body.AddAnimationWithIDDelayLoopCountSequence(
                (int)MouseAnimationId.ExitEmpty,
                0.05f,
                Timeline.LoopType.TIMELINE_NO_LOOP,
                4,
                Mouse.ImgObjGapMouseStartQuad + 2,
                [
                    Mouse.ImgObjGapMouseStartQuad + 6,
                    Mouse.ImgObjGapMouseStartQuad + 7,
                    Mouse.ImgObjGapMouseEndQuad
                ]);

            body.AddAnimationWithIDDelayLoopCountSequence(
                (int)MouseAnimationId.ExitWithCandy,
                0.05f,
                Timeline.LoopType.TIMELINE_NO_LOOP,
                4,
                Mouse.ImgObjGapMouse0008Quad,
                [
                    Mouse.ImgObjGapMouseStartQuad + 9,
                    Mouse.ImgObjGapMouseStartQuad + 10,
                    Mouse.ImgObjGapMouseEndQuad
                ]);

            body.AddAnimationWithIDDelayLoopFirstLast(
                (int)MouseAnimationId.Idle,
                0.05f,
                Timeline.LoopType.TIMELINE_NO_LOOP,
                Mouse.ImgObjGapIdleQuad,
                Mouse.ImgObjGapIdleQuad);

            _ = container.AddChild(body);

            Animation eyes = Animation.Animation_createWithResID(Resources.Img.ObjGap);
            eyes.anchor = eyes.parentAnchor = 18;
            eyes.DoRestoreCutTransparency();
            _ = eyes.AddAnimationDelayLoopFirstLast(
                0.05f,
                Timeline.LoopType.TIMELINE_NO_LOOP,
                Mouse.ImgObjGapEyesStartQuad,
                Mouse.ImgObjGapEyesEndQuad);

            _ = container.AddChild(eyes);
            eyes.visible = false;

            return new Mouse.SharedMouseSprites
            {
                Container = container,
                Body = body,
                Eyes = eyes
            };
        }

        /// <summary>
        /// Identifiers for mouse animation states.
        /// </summary>
        private enum MouseAnimationId
        {
            EntryEmpty = 0,
            EntryWithCandy = 1,
            ExitEmpty = 2,
            ExitWithCandy = 3,
            Retreat = 4,
            Idle = 5
        }

        private readonly GameScene scene = scene;
        private readonly DynamicArray<Mouse> mice = new();
        private Mouse activeMouse;
        private int activeIndex = -1;
        private Mouse.SharedMouseSprites? sharedSpriteContainer;
        private bool advanceLocked;
        private ConstraintedPoint carriedStar;
        private GameObject carriedCandy;
    }
}
