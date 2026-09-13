using NewSafetyHelp.CustomDesktop.Utils;
using UnityEngine;
using UnityEngine.Video;

namespace NewSafetyHelp.ARG.ARGGUI
{
    public static class ARGDesktopVideo
    {
        private static GameObject fullscreenVideoPlayer;

        public static void CreateFullScreenVideoPlayer()
        {
            fullscreenVideoPlayer = new GameObject("FullscreenVideoPlayer");

            fullscreenVideoPlayer.SetActive(false);

            // Add video player.
            VideoPlayer videoPlayerComponent = fullscreenVideoPlayer.AddComponent<VideoPlayer>();

            videoPlayerComponent.playOnAwake = true;
            videoPlayerComponent.waitForFirstFrame = true;
            videoPlayerComponent.skipOnDrop = false;
            videoPlayerComponent.isLooping = false;
            videoPlayerComponent.renderMode = VideoRenderMode.CameraNearPlane;
            videoPlayerComponent.targetCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
            videoPlayerComponent.aspectRatio = VideoAspectRatio.FitInside;
            videoPlayerComponent.audioOutputMode = VideoAudioOutputMode.Direct;

            videoPlayerComponent.url = ARGLoadedFiles.ARGVideo;

            videoPlayerComponent.loopPointReached += source => { StopFullScreenVideo(); };
        }

        public static void PlayFullScreenVideo()
        {
            fullscreenVideoPlayer.SetActive(true);
        }

        private static void StopFullScreenVideo()
        {
            fullscreenVideoPlayer.SetActive(false);
            CustomCampaignSceneSwitcher.BackToMainGame();
        }
    }
}