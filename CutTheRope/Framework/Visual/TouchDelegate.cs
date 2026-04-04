using System.Collections.Generic;

using Microsoft.Xna.Framework.Input.Touch;

namespace CutTheRope.Framework.Visual
{
    /// <summary>
    /// Receives touch and button input events from the framework.
    /// </summary>
    internal interface ITouchDelegate
    {
        /// <summary>Called when one or more touches begin.</summary>
        bool TouchesBeganwithEvent(IList<TouchLocation> touches);

        /// <summary>Called when one or more touches end.</summary>
        bool TouchesEndedwithEvent(IList<TouchLocation> touches);

        /// <summary>Called when one or more active touches move.</summary>
        bool TouchesMovedwithEvent(IList<TouchLocation> touches);

        /// <summary>Called when one or more touches are cancelled by the system.</summary>
        bool TouchesCancelledwithEvent(IList<TouchLocation> touches);

        /// <summary>Called when the back/escape button is pressed.</summary>
        bool BackButtonPressed();

        /// <summary>Called when the menu button is pressed.</summary>
        bool MenuButtonPressed();
    }
}
