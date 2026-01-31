namespace CutTheRope.Framework.Media
{
    internal enum VideoPlayerBackend
    {
        AVFoundation,
        Ffmpeg,
        Vlc,
        MonoGame
    }

    internal static class VideoPlayerBackendSelector
    {
        public static VideoPlayerBackend Select(bool isMac, bool isMac26OrLater, bool hasAvFoundation, bool hasFfmpeg, bool hasVlc)
        {
            if (isMac)
            {
                if (isMac26OrLater && hasAvFoundation)
                {
                    return VideoPlayerBackend.AVFoundation;
                }

                if (hasFfmpeg)
                {
                    return VideoPlayerBackend.Ffmpeg;
                }
            }

            return hasVlc ? VideoPlayerBackend.Vlc : VideoPlayerBackend.MonoGame;
        }
    }
}
