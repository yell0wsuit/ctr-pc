using System.Collections.Generic;
using System.Threading;


namespace CutTheRope.GameMain
{
    internal class ResDataPhoneFull
    {
        // String-based resource ID system with auto-assignment
        private static readonly Lock resourceIdLock_ = new();
        private static Dictionary<string, int> stringToIntMap_;
        private static Dictionary<int, string> intToStringMap_;
        private static int nextAutoId_;  // Auto-assign sequential IDs

        /// <summary>
        /// Gets the integer ID for a resource name. If the resource name doesn't have an ID yet,
        /// one will be automatically assigned.
        /// </summary>
        public static int GetResourceId(string resourceName)
        {
            if (string.IsNullOrEmpty(resourceName))
            {
                return -1;
            }

            EnsureResourceIdMapsLoaded();

            lock (resourceIdLock_)
            {
                if (stringToIntMap_.TryGetValue(resourceName, out int existingId))
                {
                    return existingId;
                }

                // Auto-assign a new ID for this resource
                int newId = nextAutoId_++;
                stringToIntMap_[resourceName] = newId;
                intToStringMap_[newId] = resourceName;
                return newId;
            }
        }

        /// <summary>
        /// Gets the resource name for an integer ID. Returns null if not found.
        /// </summary>
        public static string GetResourceName(int resourceId)
        {
            EnsureResourceIdMapsLoaded();

            lock (resourceIdLock_)
            {
                _ = intToStringMap_.TryGetValue(resourceId, out string name);
                return name;
            }
        }

        private static void EnsureResourceIdMapsLoaded()
        {
            if (stringToIntMap_ != null)
            {
                return;
            }

            lock (resourceIdLock_)
            {
                if (stringToIntMap_ != null)
                {
                    return;
                }

                // Initialize empty maps - IDs will be auto-assigned on first use
                stringToIntMap_ = [];
                intToStringMap_ = [];
            }
        }
    }
}
