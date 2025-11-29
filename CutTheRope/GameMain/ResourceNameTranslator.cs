using System.Collections.Generic;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Provides mapping helpers between legacy numeric resource identifiers and their string-based names.
    /// </summary>
    internal static class ResourceNameTranslator
    {
        /// <summary>
        /// Converts a string resource name into its legacy numeric identifier.
        /// </summary>
        public static int ToResourceId(string resourceName)
        {
            return ResDataPhoneFull.GetResourceId(resourceName);
        }

        /// <summary>
        /// Attempts to resolve a string resource name to its legacy numeric identifier without auto-registering.
        /// </summary>
        public static bool TryGetResourceId(string resourceName, out int resourceId)
        {
            return ResDataPhoneFull.TryGetResourceId(resourceName, out resourceId);
        }

        /// <summary>
        /// Attempts to get the string resource name for a legacy numeric identifier.
        /// </summary>
        public static bool TryGetResourceName(int resourceId, out string resourceName)
        {
            resourceName = ResDataPhoneFull.GetResourceName(resourceId);
            return !string.IsNullOrEmpty(resourceName);
        }

        /// <summary>
        /// Returns the string resource name for a legacy identifier, or <c>null</c> when no mapping exists.
        /// </summary>
        public static string TranslateLegacyId(int resourceId)
        {
            return TryGetResourceName(resourceId, out string resourceName) ? resourceName : null;
        }

        /// <summary>
        /// Translates a legacy pack array into string resource names while preserving the terminal sentinel.
        /// </summary>
        public static string[] TranslateLegacyPack(IEnumerable<int> pack)
        {
            List<string> results = [];

            foreach (int resourceId in pack)
            {
                if (resourceId < 0)
                {
                    break;
                }

                string resourceName = TranslateLegacyId(resourceId);
                if (!string.IsNullOrEmpty(resourceName))
                {
                    results.Add(resourceName);
                }
            }

            results.Add(null);
            return [.. results];
        }
    }
}
