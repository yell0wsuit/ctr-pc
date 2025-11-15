namespace CutTheRope.Framework.Core
{
    internal interface IResourceMgrDelegate
    {
        void ResourceLoaded(int res);

        void AllResourcesLoaded();
    }
}
