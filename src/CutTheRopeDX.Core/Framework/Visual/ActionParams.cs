using System.Collections.Generic;

namespace CutTheRopeDX.Framework.Visual
{
    /// <summary>
    /// Contains the set of <see cref="CTRAction"/> instances associated with an action keyframe.
    /// </summary>
    internal sealed class ActionParams
    {
        /// <summary>
        /// List of actions to execute when this keyframe is reached.
        /// </summary>
        public List<CTRAction> actionSet;
    }
}
