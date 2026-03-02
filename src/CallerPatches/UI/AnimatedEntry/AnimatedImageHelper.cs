using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace NewSafetyHelp.CallerPatches.UI.AnimatedEntry
{
    public static class AnimatedImageHelper
    {
        public static GameObject CreateAnimatedPortrait(GameObject animatedPortrait)
        {
            GameObject portraitAnimated = Object.Instantiate(animatedPortrait, animatedPortrait.transform);

            portraitAnimated.transform.localPosition = Vector3.zero;

            // Add updated texture
            Object.DestroyImmediate(portraitAnimated.GetComponent<Image>());
            
            RawImage rawImageComponent = portraitAnimated.AddComponent<RawImage>();
            rawImageComponent.raycastTarget = false;

            // Add video player.
            VideoPlayer videoPlayerComponent = portraitAnimated.AddComponent<VideoPlayer>();

            videoPlayerComponent.playOnAwake = true;
            videoPlayerComponent.waitForFirstFrame = true;
            videoPlayerComponent.isLooping = true;
            videoPlayerComponent.renderMode = VideoRenderMode.RenderTexture;
            videoPlayerComponent.aspectRatio = VideoAspectRatio.FitInside;
            videoPlayerComponent.audioOutputMode = VideoAudioOutputMode.None;
            
            AspectRatioFitter aspectFitter = portraitAnimated.AddComponent<AspectRatioFitter>();
            aspectFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            
            // Make render texture be the RawImage texture.

            videoPlayerComponent.prepareCompleted += 
                videoPlayer =>
            {   
                RenderTexture renderTexture = new RenderTexture((int) videoPlayer.width, (int) videoPlayer.height, 0);
                renderTexture.Create();
                
                videoPlayer.targetTexture = renderTexture;
                rawImageComponent.texture = renderTexture;
                
                float ratio = (float)videoPlayer.width / videoPlayer.height;
                aspectFitter.aspectRatio = ratio;
            };

            return portraitAnimated;
        }
        
        public static void SetVideoUrl(string url, GameObject animatedPortrait)
        {
            VideoPlayer videoPlayerComponent = animatedPortrait.GetComponent<VideoPlayer>();
            
            videoPlayerComponent.Stop();
            
            if(videoPlayerComponent.targetTexture != null)
            {
                videoPlayerComponent.targetTexture.Release();
                Object.Destroy(videoPlayerComponent.targetTexture);
            }
            
            videoPlayerComponent.url = url;
            
            // Activate the portrait
            animatedPortrait.SetActive(true);
        }
    }
}