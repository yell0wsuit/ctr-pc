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
            mice.Add(mouse);

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
            scene.DetachActiveHands();
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
        /// Forces the active mouse to drop candy and retreat back into its hole.
        /// </summary>
        public void ForceDropCandy()
        {
            if (activeMouse == null || !activeMouse.HasCandy)
            {
                return;
            }

            activeMouse.DropCandyAndRetreat();
            carriedStar = null;
            carriedCandy = null;
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
        private static Mouse.SharedMouseSprites CreateSharedSprites()
        {
            BaseElement container = new()
            {
                anchor = 18,
                parentAnchor = 18
            };

            Animation body = Animation.Animation_createWithResID(Resources.Img.ObjMouse);
            body.anchor = body.parentAnchor = 18;
            body.DoRestoreCutTransparency();

            // ID 0: Entry empty — frames 1, 2, 3, 14
            body.AddAnimationWithIDDelayLoopCountSequence(
                (int)MouseAnimationId.EntryEmpty,
                0.05f,
                Timeline.LoopType.TIMELINE_NO_LOOP,
                4,
                1,
                [2, 3, 14]);

            // ID 1: Entry with candy — frames 17, 19, 21, 24
            body.AddAnimationWithIDDelayLoopCountSequence(
                (int)MouseAnimationId.EntryWithCandy,
                0.05f,
                Timeline.LoopType.TIMELINE_NO_LOOP,
                4,
                17,
                [19, 21, Mouse.CandyInMouthQuad]);

            // ID 3: Idle — single frame 24 (candy in mouth pose, plays after entry-with-candy)
            body.AddAnimationWithIDDelayLoopFirstLast(
                (int)MouseAnimationId.Idle,
                0.05f,
                Timeline.LoopType.TIMELINE_NO_LOOP,
                Mouse.CandyInMouthQuad,
                Mouse.CandyInMouthQuad);

            // ID 2: Idle empty — single frame 4 (mouse body, no candy)
            body.AddAnimationWithIDDelayLoopCountSequence(
                (int)MouseAnimationId.IdleEmpty,
                0.05f,
                Timeline.LoopType.TIMELINE_NO_LOOP,
                1,
                Mouse.IdleQuad,
                []);

            // ID 4: Exit empty — frames 14, 15, 16, 18
            body.AddAnimationWithIDDelayLoopCountSequence(
                (int)MouseAnimationId.ExitEmpty,
                0.05f,
                Timeline.LoopType.TIMELINE_NO_LOOP,
                4,
                14,
                [15, 16, Mouse.BlankQuad]);

            // ID 5: Exit with candy — frames 24, 26, 28, 18
            body.AddAnimationWithIDDelayLoopCountSequence(
                (int)MouseAnimationId.ExitWithCandy,
                0.05f,
                Timeline.LoopType.TIMELINE_NO_LOOP,
                4,
                Mouse.CandyInMouthQuad,
                [26, 28, Mouse.BlankQuad]);

            _ = container.AddChild(body);

            // Eye blink animation — frames 5-13
            Animation eyes = Animation.Animation_createWithResID(Resources.Img.ObjMouse);
            eyes.anchor = eyes.parentAnchor = 18;
            eyes.DoRestoreCutTransparency();
            _ = eyes.AddAnimationDelayLoopFirstLast(
                0.05f,
                Timeline.LoopType.TIMELINE_NO_LOOP,
                Mouse.EyesStartQuad,
                Mouse.EyesEndQuad);
            _ = container.AddChild(eyes);
            eyes.visible = false;

            return new Mouse.SharedMouseSprites
            {
                Container = container,
                Body = body,
                Eyes = eyes
            };
        }

        private enum MouseAnimationId
        {
            EntryEmpty = 0,
            EntryWithCandy = 1,
            IdleEmpty = 2,
            Idle = 3,
            ExitEmpty = 4,
            ExitWithCandy = 5,
            Bounce = 6
        }

        private readonly GameScene scene = scene;
        private readonly List<Mouse> mice = [];
        private Mouse activeMouse;
        private int activeIndex = -1;
        private Mouse.SharedMouseSprites? sharedSpriteContainer;
        private bool advanceLocked;
        private ConstraintedPoint carriedStar;
        private GameObject carriedCandy;
    }
}
