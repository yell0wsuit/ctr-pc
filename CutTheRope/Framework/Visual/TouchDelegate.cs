using System.Collections.Generic;

using Microsoft.Xna.Framework.Input.Touch;

namespace CutTheRope.Framework.Visual
{
    /// <summary>
    /// Receives touch and button input events from the framework.
    /// </summary>
    internal interface ITouchDelegate
    {
        /// <summary>
        /// Called when one or more <paramref name="touches"/> begin.
        /// </summary>
        /// <param name="touches">Touch locations that began.</param>
        /// <returns><see langword="true"/> when the event was handled; otherwise <see langword="false"/>.</returns>
        bool TouchesBeganwithEvent(IList<TouchLocation> touches);

        /// <summary>
        /// Called when one or more <paramref name="touches"/> end.
        /// </summary>
        /// <param name="touches">Touch locations that ended.</param>
        /// <returns><see langword="true"/> when the event was handled; otherwise <see langword="false"/>.</returns>
        bool TouchesEndedwithEvent(IList<TouchLocation> touches);

        /// <summary>
        /// Called when one or more active <paramref name="touches"/> move.
        /// </summary>
        /// <param name="touches">Touch locations that moved.</param>
        /// <returns><see langword="true"/> when the event was handled; otherwise <see langword="false"/>.</returns>
        bool TouchesMovedwithEvent(IList<TouchLocation> touches);

        /// <summary>
        /// Called when one or more <paramref name="touches"/> are cancelled by the system.
        /// </summary>
        /// <param name="touches">Touch locations that were cancelled.</param>
        /// <returns><see langword="true"/> when the event was handled; otherwise <see langword="false"/>.</returns>
        bool TouchesCancelledwithEvent(IList<TouchLocation> touches);

        /// <summary>
        /// Called when the back/escape button is pressed.
        /// </summary>
        /// <returns><see langword="true"/> when the event was handled; otherwise <see langword="false"/>.</returns>
        bool BackButtonPressed();

        /// <summary>
        /// Called when the menu button is pressed.
        /// </summary>
        /// <returns><see langword="true"/> when the event was handled; otherwise <see langword="false"/>.</returns>
        bool MenuButtonPressed();
    }
}
