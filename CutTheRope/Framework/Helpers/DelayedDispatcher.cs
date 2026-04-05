using System;
using System.Collections.Generic;

namespace CutTheRope.Framework.Helpers
{
    /// <summary>
    /// Queues callbacks for later execution after a specified delay.
    /// </summary>
    internal sealed class DelayedDispatcher : FrameworkTypes
    {
        /// <summary>
        /// Initializes an empty delayed-dispatch queue.
        /// </summary>
        public DelayedDispatcher()
        {
            dispatchers = [];
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                dispatchers?.Clear();
                dispatchers = null;
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Queues a callback to be invoked after the specified delay.
        /// </summary>
        /// <param name="s">Callback to invoke.</param>
        /// <param name="p">Parameter passed to the callback.</param>
        /// <param name="d">Delay in seconds before invocation.</param>
        public void CallObjectSelectorParamafterDelay(DispatchFunc s, FrameworkTypes p, float d)
        {
            DispatchClass item = new DispatchClass().InitWithObjectSelectorParamafterDelay(s, p, d);
            dispatchers.Add(item);
        }

        /// <summary>
        /// Advances queued dispatch timers and invokes callbacks whose delay has expired.
        /// </summary>
        /// <param name="d">Elapsed frame time in seconds.</param>
        public void Update(float d)
        {
            int count = dispatchers.Count;
            for (int i = 0; i < count; i++)
            {
                DispatchClass dispatch = dispatchers[i];
                dispatch.delay -= d;
                if (dispatch.delay <= 0)
                {
                    dispatch.Dispatch();
                    _ = dispatchers.Remove(dispatch);
                    i--;
                    count = dispatchers.Count;
                }
            }
        }

        /// <summary>
        /// Removes all pending dispatches without invoking them.
        /// </summary>
        public void CancelAllDispatches()
        {
            dispatchers.Clear();
        }

        /// <summary>
        /// Cancels a queued dispatch that matches the specified callback and parameter.
        /// </summary>
        /// <param name="s">Callback to match.</param>
        /// <param name="p">Parameter to match.</param>
        /// <exception cref="NotImplementedException">This cancellation mode is not currently implemented.</exception>
        public void CancelDispatchWithObjectSelectorParam(DispatchFunc s, FrameworkTypes p)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Pending delayed dispatch records.
        /// </summary>
        private List<DispatchClass> dispatchers;

        /// <summary>
        /// Delegate invoked by <see cref="DelayedDispatcher"/> when a queued delay expires.
        /// </summary>
        /// <param name="param">Framework object parameter supplied at queue time.</param>
        public delegate void DispatchFunc(FrameworkTypes param);
    }
}
