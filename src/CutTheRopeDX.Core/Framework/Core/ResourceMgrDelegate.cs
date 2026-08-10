namespace CutTheRopeDX.Framework.Core
{
    /// <summary>
    /// Receives notification when the resource manager finishes loading all queued resources.
    /// </summary>
    internal interface IResourceMgrDelegate
    {
        /// <summary>
        /// Called when all queued resources have been loaded.
        /// </summary>
        void AllResourcesLoaded();
    }
}
