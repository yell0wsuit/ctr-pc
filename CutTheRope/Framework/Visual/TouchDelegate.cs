using System.Collections.Generic;

using Microsoft.Xna.Framework.Input.Touch;

namespace CutTheRope.Framework.Visual
{
    internal interface ITouchDelegate
    {
        bool TouchesBeganwithEvent(IList<TouchLocation> touches);

        bool TouchesEndedwithEvent(IList<TouchLocation> touches);

        bool TouchesMovedwithEvent(IList<TouchLocation> touches);

        bool TouchesCancelledwithEvent(IList<TouchLocation> touches);

        bool BackButtonPressed();

        bool MenuButtonPressed();
    }
}
