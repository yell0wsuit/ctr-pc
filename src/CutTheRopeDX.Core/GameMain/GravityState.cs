using System.Collections.Generic;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Physics;
using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Single owner of authored gravity, live orientation, toggle-touch ownership, and gravity
    /// presentation. Every orientation transition synchronizes physics and visuals atomically.
    /// </summary>
    internal sealed class GravityState
    {
        private readonly List<Image> earthAnimations = [];
        private readonly List<ToggleButton> buttons = [];
        private int toggleTouchIndex = -1;

        /// <summary>Authored gravity before orientation is applied.</summary>
        public Vector BaseVector { get; private set; }

        /// <summary>Whether gravity currently points opposite its authored vertical direction.</summary>
        public bool IsInverted { get; private set; }

        /// <summary>Gravity vector derived from <see cref="BaseVector"/> and <see cref="IsInverted"/>.</summary>
        public Vector CurrentVector => new(BaseVector.X, IsInverted ? -BaseVector.Y : BaseVector.Y);

        /// <summary>Clears level-owned configuration and presentation before loading another map.</summary>
        public void BeginLoad()
        {
            BaseVector = default;
            IsInverted = false;
            toggleTouchIndex = -1;
            buttons.Clear();
            earthAnimations.Clear();
        }

        /// <summary>Stores the authored gravity vector without changing live physics yet.</summary>
        public void ConfigureBase(Vector baseVector)
        {
            BaseVector = baseVector;
        }

        /// <summary>Attaches the toggle whose face is controlled by this state.</summary>
        public void AttachButton(ToggleButton button)
        {
            if (button != null && !buttons.Contains(button))
            {
                buttons.Add(button);
            }
        }

        /// <summary>Adds one earth image whose timeline is controlled by this state.</summary>
        public void AddEarthAnimation(Image earthAnimation)
        {
            if (earthAnimation != null)
            {
                earthAnimations.Add(earthAnimation);
            }
        }

        /// <summary>Starts live play in the authored orientation and synchronizes every representation.</summary>
        public void Activate()
        {
            IsInverted = false;
            toggleTouchIndex = -1;
            Synchronize(animateEarth: false);
        }

        /// <summary>Atomically flips orientation, live physics, and visual presentation.</summary>
        public void Toggle()
        {
            IsInverted = !IsInverted;
            Synchronize(animateEarth: true);
        }

        /// <summary>Records the pointer that pressed the gravity toggle.</summary>
        public void CaptureToggleTouch(int pointerIndex)
        {
            toggleTouchIndex = pointerIndex;
        }

        /// <summary>
        /// Releases toggle ownership when <paramref name="pointerIndex"/> owns it.
        /// </summary>
        /// <returns><see langword="true"/> only for the owning pointer's first release.</returns>
        public bool ReleaseToggleTouch(int pointerIndex)
        {
            if (toggleTouchIndex != pointerIndex)
            {
                return false;
            }

            toggleTouchIndex = -1;
            return true;
        }

        /// <summary>Checks the active toggle face against a world-space pointer position.</summary>
        public bool IsInToggleTouchZone(float x, float y)
        {
            foreach (ToggleButton button in buttons)
            {
                if (((Button)button.GetChild(button.On() ? 1 : 0)).IsInTouchZoneXYforTouchDown(x, y, true))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Draws every gravity toggle provided by the loaded level.</summary>
        public void DrawButtons()
        {
            foreach (ToggleButton button in buttons)
            {
                button.Draw();
            }
        }

        /// <summary>Removes every gravity toggle from its scene parent.</summary>
        public void RemoveButtonsFrom(BaseElement parent)
        {
            foreach (ToggleButton button in buttons)
            {
                parent.RemoveChild(button);
            }
        }

        /// <summary>Advances all gravity-owned earth animations.</summary>
        public void UpdateEarthAnimations(float delta)
        {
            foreach (Image earthAnimation in earthAnimations)
            {
                earthAnimation.Update(delta);
            }
        }

        /// <summary>Draws all gravity-owned earth animations.</summary>
        public void DrawEarthAnimations()
        {
            foreach (Image earthAnimation in earthAnimations)
            {
                earthAnimation.Draw();
            }
        }

        private void Synchronize(bool animateEarth)
        {
            Vector current = CurrentVector;
            MaterialPoint.globalGravity = current;
            MaterialPoint.globalDisableGravity = current.X == 0f && current.Y == 0f;

            foreach (ToggleButton button in buttons)
            {
                if (button.On() != IsInverted)
                {
                    button.Toggle();
                }
            }

            foreach (Image earthAnimation in earthAnimations)
            {
                if (animateEarth)
                {
                    earthAnimation.PlayTimeline(IsInverted ? 1 : 0);
                }
                else
                {
                    if (earthAnimation.GetCurrentTimeline() != null)
                    {
                        earthAnimation.StopCurrentTimeline();
                    }
                    earthAnimation.rotation = IsInverted ? 180f : 0f;
                }
            }
        }
    }
}
