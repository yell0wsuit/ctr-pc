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
        public static VideoPlayerBackend Select(bool isMac, bool hasAvFoundation, bool hasFfmpeg, bool hasVlc)
        {
            if (isMac && hasAvFoundation)
            {
                return VideoPlayerBackend.AVFoundation;
            }

            if (isMac && hasFfmpeg)
            {
                return VideoPlayerBackend.Ffmpeg;
            }

            if (!isMac && hasVlc)
            {
                return VideoPlayerBackend.Vlc;
            }

            return VideoPlayerBackend.MonoGame;
        }
    }
}
