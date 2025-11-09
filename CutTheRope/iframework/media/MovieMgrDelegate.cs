using CutTheRope.ios;
using System;

namespace CutTheRope.iframework.media
{
    internal interface MovieMgrDelegate
    {
        void moviePlaybackFinished(NSString url);
    }
}
