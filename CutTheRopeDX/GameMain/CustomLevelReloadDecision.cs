using System.Collections.Generic;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// How a custom level change should be applied to the running game.
    /// </summary>
    internal enum CustomLevelReloadKind
    {
        /// <summary>Every required resource is already resident; restart the scene in place.</summary>
        Instant,

        /// <summary>New resources are required; load them through the loading screen first.</summary>
        LoadingScreen
    }

    /// <summary>
    /// Chooses how to apply a custom level change based on resource availability.
    /// </summary>
    internal static class CustomLevelReloadDecision
    {
        /// <summary>
        /// Decides whether a reloaded level can restart in place.
        /// </summary>
        /// <param name="requiredResources">Resources the newly read level needs.</param>
        /// <param name="loadedResources">Resources currently resident for this session.</param>
        /// <returns>
        /// <see cref="CustomLevelReloadKind.Instant"/> when every required resource is already loaded;
        /// otherwise <see cref="CustomLevelReloadKind.LoadingScreen"/>.
        /// </returns>
        /// <remarks>
        /// The comparison is one-way. Resources that are loaded but no longer required do not force a
        /// loading pass; they are released during the next pass that happens for other reasons.
        /// </remarks>
        public static CustomLevelReloadKind Decide(
            IEnumerable<string> requiredResources,
            ISet<string> loadedResources)
        {
            if (requiredResources == null)
            {
                return CustomLevelReloadKind.Instant;
            }

            foreach (string resourceName in requiredResources)
            {
                if (string.IsNullOrWhiteSpace(resourceName))
                {
                    continue;
                }

                if (loadedResources == null || !loadedResources.Contains(resourceName))
                {
                    return CustomLevelReloadKind.LoadingScreen;
                }
            }

            return CustomLevelReloadKind.Instant;
        }
    }
}
