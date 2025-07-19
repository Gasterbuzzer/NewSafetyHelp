using UnityEngine.Video;

namespace NewSafetyHelp.ImportFiles
{
    public static class VideoHelper
    {
        public static double videoLength(VideoPlayer vPlayer)
        {
            return (vPlayer.frameCount / vPlayer.frameRate);
        }
        
        public static void videoLengthLambda(VideoPlayer vPlayer, ref double videoLength)
        {
            videoLength = (vPlayer.frameCount / vPlayer.frameRate);
        }
    }
}