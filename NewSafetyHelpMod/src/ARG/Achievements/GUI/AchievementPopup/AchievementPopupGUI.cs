using System.Collections;
using NewSafetyHelp.ImportFiles;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace NewSafetyHelp.ARG.Achievements.GUI.AchievementPopup
{
    public static class AchievementPopupGUI
    {
        private static bool isShowingPopup;

        // Parent Popup
        private static GameObject achievementPopup;
        private static RectTransform achievementPopupRectTransform;

        private static readonly Vector3 TopPosition = new Vector3(285, -260, 0);
        private static readonly Vector3 BottomPosition = new Vector3(285, -345, 0);

        // Audio
        private static AudioSource achievementPopupAudioSource;

        // Icon
        private static GameObject achievementIcon;
        private static Image achievementIconImage;

        // Title
        private static RectTransform achievementTitleRectTransform;
        private static TextMeshProUGUI achievementTitleText;

        private static readonly Vector2 TitleTopPosition = new Vector2(115, 26);
        private static readonly Vector2 TitleRightTopPosition = new Vector2(145, 26);

        // Description
        private static RectTransform achievementDescriptionRectTransform;
        private static TextMeshProUGUI achievementDescriptionText;

        private static readonly Vector2 DescriptionNormalPosition = new Vector2(115, 12);
        private static readonly Vector2 DescriptionRightPosition = new Vector2(145, 12);

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

            // Create Text before we delete the children.
            GameObject achievementTitle = Object
                .Instantiate(achievementPopup.transform.GetChild(0).GetChild(3), achievementPopup.transform).gameObject;
            achievementTitle.name = "AchievementTitle";

            GameObject achievementDescription = Object
                .Instantiate(achievementPopup.transform.GetChild(0).GetChild(3), achievementPopup.transform).gameObject;
            achievementDescription.name = "achievementDescription";

            // Destroy old content
            Object.Destroy(achievementPopup.transform.GetChild(0).gameObject);
            Object.Destroy(achievementPopup.transform.GetChild(1).gameObject);

            // Adjust position and size
            achievementPopupRectTransform = achievementPopup.GetComponent<RectTransform>();

            achievementPopupRectTransform.offsetMax = new Vector3(230, 0, 0);
            achievementPopupRectTransform.offsetMin = new Vector3(0, -80, 0);

            achievementPopupRectTransform.localPosition = BottomPosition;

            // Add sound effect
            achievementPopupAudioSource = achievementPopup.AddComponent<AudioSource>();
            achievementPopupAudioSource.volume = 0.1f;
            achievementPopupAudioSource.playOnAwake = false;

            if (EmbedLoader.AchievementSound != null)
            {
                achievementPopupAudioSource.clip = EmbedLoader.AchievementSound.clip;
            }

            // Title
            achievementTitleText = achievementTitle.GetComponent<TextMeshProUGUI>();

            achievementTitleText.text = "Achievement Title";
            achievementTitleText.fontStyle = FontStyles.Bold;
            achievementTitleText.color = new Color(0, 0, 0);
            achievementTitleText.fontSize = 15;
            achievementTitleText.alignment = TextAlignmentOptions.Center;

            achievementTitleRectTransform = achievementTitle.GetComponent<RectTransform>();

            achievementTitleRectTransform.localPosition = Vector3.zero;
            achievementTitleRectTransform.anchoredPosition = TitleTopPosition;

            // Achievement Icon
            achievementIcon = new GameObject("AchievementIcon");
            achievementIcon.transform.SetParent(achievementPopup.transform, false);

            RectTransform achievementIconRectTransform = achievementIcon.AddComponent<RectTransform>();
            achievementIconRectTransform.anchoredPosition = new Vector2(-70, 0);
            achievementIconRectTransform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            achievementIcon.AddComponent<CanvasRenderer>();
            achievementIconImage = achievementIcon.AddComponent<Image>();
            achievementIconImage.sprite = EmbedLoader.AdminIcon;

            achievementIcon.SetActive(false);

            // Achievement Description
            achievementDescriptionText = achievementDescription.GetComponent<TextMeshProUGUI>();

            achievementDescriptionText.text = "Lorem Cat Ipsum";
            achievementDescriptionText.fontStyle = FontStyles.Bold;
            achievementDescriptionText.color = new Color(0, 0, 0);
            achievementDescriptionText.alignment = TextAlignmentOptions.Center;

            achievementDescriptionRectTransform = achievementDescription.GetComponent<RectTransform>();
            achievementDescriptionRectTransform.localPosition = Vector3.zero;
            achievementDescriptionRectTransform.anchoredPosition = DescriptionNormalPosition;
            achievementDescriptionRectTransform.anchorMax = new Vector2(0, 0.5f);

            achievementDescriptionRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 200);
        }

        /// <summary>
        /// Creates the desktop achievement popup inside the game.
        /// </summary>
        public static void CreateInGameAchievementPopup()
        {
            achievementPopup = Object.Instantiate(
                GameObject.Find("MainCanvas").transform.Find("OptionsPopup").gameObject,
                GameObject.Find("MainCanvas").transform, true);

            achievementPopup.SetActive(false);

            achievementPopup.name = "AchievementPopup";

            Object.Destroy(achievementPopup.GetComponent<DisableInputBlockerOnDisable>());
            Object.Destroy(achievementPopup.GetComponent<OptionsMenuBehavior>());

            // Create Text before we delete the children.
            GameObject achievementTitle = Object
                .Instantiate(achievementPopup.transform.GetChild(0).GetChild(3), achievementPopup.transform).gameObject;
            achievementTitle.name = "AchievementTitle";

            GameObject achievementDescription = Object
                .Instantiate(achievementPopup.transform.GetChild(0).GetChild(3), achievementPopup.transform).gameObject;
            achievementDescription.name = "achievementDescription";

            // Destroy old content
            Object.Destroy(achievementPopup.transform.GetChild(0).gameObject);
            Object.Destroy(achievementPopup.transform.GetChild(1).gameObject);

            // Adjust position and size
            achievementPopupRectTransform = achievementPopup.GetComponent<RectTransform>();

            achievementPopupRectTransform.offsetMax = new Vector3(230, 0, 0);
            achievementPopupRectTransform.offsetMin = new Vector3(0, -80, 0);

            achievementPopupRectTransform.localPosition = BottomPosition;

            // Add sound effect
            achievementPopupAudioSource = achievementPopup.AddComponent<AudioSource>();
            achievementPopupAudioSource.volume = 0.1f;
            achievementPopupAudioSource.playOnAwake = false;

            if (EmbedLoader.AchievementSound != null)
            {
                achievementPopupAudioSource.clip = EmbedLoader.AchievementSound.clip;
            }

            // Title
            achievementTitleText = achievementTitle.GetComponent<TextMeshProUGUI>();

            achievementTitleText.text = "Achievement Title";
            achievementTitleText.fontStyle = FontStyles.Bold;
            achievementTitleText.color = new Color(0, 0, 0);
            achievementTitleText.fontSize = 15;
            achievementTitleText.alignment = TextAlignmentOptions.Center;

            achievementTitleRectTransform = achievementTitle.GetComponent<RectTransform>();

            achievementTitleRectTransform.localPosition = Vector3.zero;
            achievementTitleRectTransform.anchoredPosition = TitleTopPosition;

            // Achievement Icon
            achievementIcon = new GameObject("AchievementIcon");
            achievementIcon.transform.SetParent(achievementPopup.transform, false);

            RectTransform achievementIconRectTransform = achievementIcon.AddComponent<RectTransform>();
            achievementIconRectTransform.anchoredPosition = new Vector2(-70, 0);
            achievementIconRectTransform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            achievementIcon.AddComponent<CanvasRenderer>();
            achievementIconImage = achievementIcon.AddComponent<Image>();
            achievementIconImage.sprite = EmbedLoader.AdminIcon;

            achievementIcon.SetActive(false);

            // Achievement Description
            achievementDescriptionText = achievementDescription.GetComponent<TextMeshProUGUI>();

            achievementDescriptionText.text = "Lorem Cat Ipsum";
            achievementDescriptionText.fontStyle = FontStyles.Bold;
            achievementDescriptionText.color = new Color(0, 0, 0);
            achievementDescriptionText.alignment = TextAlignmentOptions.Center;

            achievementDescriptionRectTransform = achievementDescription.GetComponent<RectTransform>();
            achievementDescriptionRectTransform.localPosition = Vector3.zero;
            achievementDescriptionRectTransform.anchoredPosition = DescriptionNormalPosition;
            achievementDescriptionRectTransform.anchorMax = new Vector2(0, 0.5f);

            achievementDescriptionRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 200);
        }

        /// <summary>
        /// Triggers the popup for the achievement.
        /// </summary>
        /// <param name="customAchievement">Custom Achievement to be displayed.</param>
        /// <param name="appearDelay">Delay until the popup appears.</param>
        /// <returns>Coroutine to be run.</returns>
        public static IEnumerator TriggerAchievementPopupDesktop(CustomAchievement customAchievement,
            float appearDelay = 1)
        {
            while (isShowingPopup)
            {
                yield return new WaitForEndOfFrame();
            }

            isShowingPopup = true;

            if (achievementPopup != null)
            {
                achievementPopupRectTransform.localPosition = BottomPosition;

                // Setup popup
                achievementTitleText.text = customAchievement.AchievementName;
                achievementDescriptionText.text = customAchievement.AchievementDescription;

                if (customAchievement.AchievementIcon != null)
                {
                    achievementIcon.SetActive(true);
                    achievementIconImage.sprite = customAchievement.AchievementIcon;

                    achievementTitleRectTransform.anchoredPosition = TitleRightTopPosition;

                    achievementDescriptionRectTransform.anchoredPosition = DescriptionRightPosition;
                    achievementDescriptionRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 150);
                }

                yield return new WaitForSeconds(appearDelay);

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

                achievementPopup.SetActive(false);
            }

            isShowingPopup = false;
        }
    }
}