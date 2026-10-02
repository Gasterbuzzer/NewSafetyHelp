using System.Collections;
using NewSafetyHelp.ImportFiles;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NewSafetyHelp.ARG.Achievements.GUI.AchievementPopup
{
    public static class AchievementPopupGUI
    {
        private static bool isShowingPopup;

        private static GameObject achievementPopup;
        private static AudioSource achievementPopupAudioSource;

        private static readonly Vector3 TopPosition = new Vector3(285, -260, 0);
        private static readonly Vector3 BottomPosition = new Vector3(285, -345, 0);

        /// <summary>
        /// Creates the desktop achievement popup on the desktop.
        /// </summary>
        public static void CreateDesktopAchievementPopup()
        {
            achievementPopup = Object.Instantiate(
                GameObject.Find("MainMenuCanvas").transform.Find("TextPopup").gameObject,
                GameObject.Find("MainMenuCanvas").transform, true);

            achievementPopup.SetActive(false);

            achievementPopup.name = "AchievementPopup";

            Object.Destroy(achievementPopup.GetComponent<DisableInputBlockerOnDisable>());

            Object.Destroy(achievementPopup.transform.GetChild(0).gameObject);
            Object.Destroy(achievementPopup.transform.GetChild(1).gameObject);

            // Adjust position and size
            RectTransform achievementPopupRectTransform = achievementPopup.GetComponent<RectTransform>();

            achievementPopupRectTransform.offsetMax = new Vector3(230, 0, 0);
            achievementPopupRectTransform.offsetMin = new Vector3(0, -80, 0);

            achievementPopupRectTransform.localPosition = BottomPosition;

            // Add sound effect
            achievementPopupAudioSource = achievementPopup.AddComponent<AudioSource>();
            achievementPopupAudioSource.volume = 0.1f;

            if (EmbedLoader.AchievementSound != null)
            {
                achievementPopupAudioSource.clip = EmbedLoader.AchievementSound.clip;
            }
        }

        public static IEnumerator TriggerAchievementPopup(CustomAchievement customAchievement)
        {
            while (isShowingPopup)
            {
                yield return new WaitForEndOfFrame();
            }

            if (achievementPopup != null)
            {
                RectTransform achievementPopupRectTransform = achievementPopup.GetComponent<RectTransform>();

                achievementPopupRectTransform.localPosition = BottomPosition;

                yield return new WaitForSeconds(2);

                isShowingPopup = true;
                achievementPopup.SetActive(true);

                // Play sound
                achievementPopupAudioSource.Play();

                // Animate appearing.
                int index = 0;
                float animationDuration = 50; // In ms
                while (true)
                {
                    achievementPopupRectTransform.localPosition = new Vector3(TopPosition.x,
                        Mathf.Lerp(BottomPosition.y, TopPosition.y, index / animationDuration), TopPosition.z);

                    yield return new WaitForSeconds(0.01f);

                    if (index >= animationDuration)
                    {
                        break;
                    }

                    index++;
                }

                achievementPopupRectTransform.localPosition = TopPosition;

                yield return new WaitForSeconds(4);

                // Animate disappearing.
                index = 0;
                while (true)
                {
                    achievementPopupRectTransform.localPosition = new Vector3(TopPosition.x,
                        Mathf.Lerp(TopPosition.y, BottomPosition.y, index / animationDuration), TopPosition.z);

                    yield return new WaitForSeconds(0.01f);

                    if (index >= animationDuration)
                    {
                        break;
                    }

                    index++;
                }

                achievementPopupRectTransform.localPosition = BottomPosition;

                isShowingPopup = false;
                achievementPopup.SetActive(false);
            }
        }
    }
}