namespace CutTheRope.Framework.Visual
{
    /// <summary>
    /// Holds the name and parameters for a timeline action dispatched to a <see cref="BaseElement"/>.
    /// </summary>
    internal sealed class ActionData
    {
        /// <summary>
        /// Name identifying the action to perform.
        /// </summary>
        public string actionName;

        /// <summary>
        /// Primary integer parameter for the action.
        /// </summary>
        public int actionParam;

        /// <summary>
        /// Secondary integer parameter for the action.
        /// </summary>
        public int actionSubParam;

        /// <summary>
        /// Primary float parameter for the action.
        /// </summary>
        public float actionParamFloat;

        /// <summary>
        /// Secondary float parameter for the action.
        /// </summary>
        public float actionSubParamFloat;
    }
}
