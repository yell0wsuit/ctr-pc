using System.Collections.Generic;

namespace CutTheRopeDX.Framework.Platform
{
    /// <summary>
    /// Tracks which content paths have been asked for, which have arrived, and which
    /// still need fetching.
    /// </summary>
    /// <remarks>
    /// Pure bookkeeping with no IO, so the browser host's gating rule is testable by the
    /// headless suite. "Pending" means requested but not yet resident - work handed out
    /// by <see cref="TakePending"/> stays pending until it actually arrives, so the host
    /// cannot mistake dispatch for completion.
    /// </remarks>
    internal sealed class ResidencyTracker
    {
        private readonly HashSet<string> _resident = [];
        private readonly HashSet<string> _requested = [];
        private readonly List<string> _undispatched = [];

        /// <summary>Number of requested paths that have not arrived yet.</summary>
        public int PendingCount => _requested.Count - _resident.Count;

        /// <summary>Whether every requested path is readable.</summary>
        public bool AllResident => PendingCount == 0;

        /// <summary>Whether one path is readable.</summary>
        /// <param name="path">Content-relative path.</param>
        public bool IsResident(string path)
        {
            return _resident.Contains(path);
        }

        /// <summary>Records that these paths will be needed.</summary>
        /// <param name="paths">Content-relative paths.</param>
        public void Request(IEnumerable<string> paths)
        {
            foreach (string path in paths)
            {
                if (_resident.Contains(path) || !_requested.Add(path))
                {
                    continue;
                }
                _undispatched.Add(path);
            }
        }

        /// <summary>Returns the paths not yet handed out for fetching, and clears that list.</summary>
        public IReadOnlyCollection<string> TakePending()
        {
            if (_undispatched.Count == 0)
            {
                return [];
            }
            string[] batch = [.. _undispatched];
            _undispatched.Clear();
            return batch;
        }

        /// <summary>Records that a path has arrived.</summary>
        /// <param name="path">Content-relative path.</param>
        /// <returns><see langword="true"/> when this call changed the tracker's state.</returns>
        public bool MarkResident(string path)
        {
            _ = _requested.Add(path);
            return _resident.Add(path);
        }
    }
}
