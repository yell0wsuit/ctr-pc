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
        /// Returns the string resource name for a legacy identifier, or <c>null</c> when no mapping exists.
        /// </summary>
        public static string TranslateLegacyId(int resourceId)
        {
            string resourceName = ResDataPhoneFull.GetResourceName(resourceId);
            return string.IsNullOrEmpty(resourceName) ? null : resourceName;
        }
    }
}
