namespace CutTheRope.Framework.Helpers
{
    /// <summary>
    /// Stores a delayed callback together with its parameter and remaining delay time.
    /// </summary>
    internal sealed class DispatchClass : FrameworkTypes
    {
        /// <summary>
        /// Initializes this dispatch record with a callback, parameter, and delay.
        /// </summary>
        /// <param name="callThisFunc">Callback to invoke when the delay expires.</param>
        /// <param name="p">Parameter passed to the callback.</param>
        /// <param name="d">Initial delay in seconds.</param>
        /// <returns>The initialized dispatch record.</returns>
        public DispatchClass InitWithObjectSelectorParamafterDelay(DelayedDispatcher.DispatchFunc callThisFunc, FrameworkTypes p, float d)
        {
            callThis = callThisFunc;
            param = p;
            delay = d;
            return this;
        }

        /// <summary>
        /// Invokes the stored callback with the stored parameter.
        /// </summary>
        public void Dispatch()
        {
            callThis?.Invoke(param);
        }

        /// <summary>
        /// Remaining delay before dispatch, in seconds.
        /// </summary>
        public float delay;

        /// <summary>
        /// Callback to invoke when the delay expires.
        /// </summary>
        public DelayedDispatcher.DispatchFunc callThis;

        /// <summary>
        /// Parameter passed to <see cref="callThis"/> during dispatch.
        /// </summary>
        public FrameworkTypes param;
    }
}
