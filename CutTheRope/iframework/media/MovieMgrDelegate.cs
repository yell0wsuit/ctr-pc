using CutTheRope.ios;

namespace CutTheRope.iframework.media
{
    internal interface IMovieMgrDelegate
    {
        void MoviePlaybackFinished(NSString url);
    }
}
