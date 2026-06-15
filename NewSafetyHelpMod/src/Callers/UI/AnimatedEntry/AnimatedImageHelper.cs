using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace NewSafetyHelp.Callers.UI.AnimatedEntry
{
    public static class AnimatedImageHelper
    {
        /// <summary>
        /// Creates a given animated portrait that contains a video player to show a video with.
        /// </summary>
        /// <param name="animatedPortrait">Original portrait to use as reference.</param>
        /// <param name="deleteChildren">If to remove all children after copy.</param>
        /// <param name="deleteDaySpriteSwapper">If to delete the component DayNumSpriteSwapper.</param>
        /// <param name="setAsFirstChild">If order matters, this will set the animated portrait as first child.</param>
        /// <param name="disableVideoClicking">If to disable clicking on the animated portrait.</param>
        /// <returns>Newly created GameObject that represents the animated portrait.</returns>
        public static GameObject CreateAnimatedPortrait(GameObject animatedPortrait, bool deleteChildren = false,
            bool deleteDaySpriteSwapper = false, bool setAsFirstChild = false, bool disableVideoClicking = true)
        {
            GameObject portraitAnimated = Object.Instantiate(animatedPortrait, animatedPortrait.transform);

            portraitAnimated.transform.localPosition = Vector3.zero;

            portraitAnimated.name = "Animated-Image-VideoPlayer";

            // Add updated texture
            Object.DestroyImmediate(portraitAnimated.GetComponent<Image>());
            
            RawImage rawImageComponent = portraitAnimated.AddComponent<RawImage>();

            if (disableVideoClicking)
            {
                rawImageComponent.raycastTarget = false;
            }
            
            if (deleteChildren)
            {
                foreach (Transform child in portraitAnimated.transform)
                {
                    Object.Destroy(child.gameObject);
                }
            }

            if (deleteDaySpriteSwapper)
            {
                Object.Destroy(portraitAnimated.GetComponent<DayNumSpriteSwapper>());
            }

            if (setAsFirstChild)
            {
                portraitAnimated.transform.SetAsFirstSibling();
            }
            
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
            
            portraitAnimated.SetActive(false);

            return portraitAnimated;
        }
        
        /// <summary>
        /// Sets the URL to play in given animated portrait that contains a video player.
        /// </summary>
        /// <param name="url">Video to play (Local file)</param>
        /// <param name="animatedPortrait">Animated portrait with video player.</param>
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
        
        /// <summary>
        /// Disables the video player from looping.
        /// </summary>
        /// <param name="animatedPortrait">Animated portrait with video player.</param>
        public static void DisableVideoLoop(GameObject animatedPortrait)
        {
            VideoPlayer videoPlayerComponent = animatedPortrait.GetComponent<VideoPlayer>();

            videoPlayerComponent.isLooping = false;
        }
        
        /// <summary>
        /// Sets the video play to loop.
        /// </summary>
        /// <param name="animatedPortrait">Animated portrait with video player.</param>
        public static void EnableVideoLoop(GameObject animatedPortrait)
        {
            VideoPlayer videoPlayerComponent = animatedPortrait.GetComponent<VideoPlayer>();

            videoPlayerComponent.isLooping = true;
        }
    }
}