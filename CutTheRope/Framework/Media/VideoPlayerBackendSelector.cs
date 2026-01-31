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
            return isMac && hasAvFoundation
                ? VideoPlayerBackend.AVFoundation
                : isMac && hasFfmpeg ? VideoPlayerBackend.Ffmpeg : !isMac && hasVlc ? VideoPlayerBackend.Vlc : VideoPlayerBackend.MonoGame;
        }
    }
}
