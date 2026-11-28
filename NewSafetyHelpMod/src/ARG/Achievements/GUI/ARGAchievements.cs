using System.Reflection;
using JetBrains.Annotations;
using NewSafetyHelp.ImportFiles;
using NewSafetyHelp.LoggingSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NewSafetyHelp.ARG.Achievements.GUI
{
    public static class ARGAchievements
    {
        private static readonly FieldInfo MyImage = typeof(AdhereToPalette).GetField("myImage",
            BindingFlags.NonPublic | BindingFlags.Instance);

        private const float AchievementBoxHeight = 80f;
        private const float AchievementBoxWidth = 345f;

        [CanBeNull] private static GameObject contentBox;

        public static void CreateAchievementsIcon()
        {
            GameObject mainMenuCanvas = GameObject.Find("MainMenuCanvas");

            /*
             * Add Achievement Box
             */

            GameObject leftHandSide = mainMenuCanvas.transform.Find("Desktop/Programs").gameObject;

            GameObject achievementProgram = Object.Instantiate(leftHandSide.transform.Find("HSH-Executable"),
                leftHandSide.transform).gameObject;

            achievementProgram.name = "AchievementProgram";

            Object.Destroy(achievementProgram.GetComponent<HSHExecutableBehavior>());

            // Change Executable name.
            achievementProgram.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = "Achievement";

            // Change Icon.
            achievementProgram.transform.GetComponent<Image>().sprite = EmbedLoader.AdminIcon;

            ARGAchievementClickEvent argAchievementClickEventComponent =
                achievementProgram.AddComponent<ARGAchievementClickEvent>();

            Button doubleClickButton = achievementProgram.GetComponent<Button>();

            doubleClickButton.onClick.RemoveAllListeners(); // Remove all previous on click events.

            doubleClickButton.onClick.AddListener(argAchievementClickEventComponent.OpenAchievementPopup);

            achievementProgram.SetActive(true);

            /*
             * Create Achievement Window
             */

            GameObject achievementWindow = Object
                .Instantiate(mainMenuCanvas.transform.GetChild(4).gameObject, mainMenuCanvas.transform).gameObject;

            ARGAchievementLogic.SetAchievementPopup(achievementWindow);

            // Rename Program
            achievementWindow.name = "AchievementWindow";

            GameObject programTitle = achievementWindow.transform.GetChild(0).GetChild(3).gameObject;

            programTitle.GetComponent<TextMeshProUGUI>().text = "ACHIEVEMENT WINDOW";

            GameObject programLogo = achievementWindow.transform.GetChild(0).GetChild(2).gameObject;

            programLogo.GetComponent<Image>().sprite = EmbedLoader.AdminIcon;

            // Resize the Window
            RectTransform achievementWindowRectTransform = achievementWindow.GetComponent<RectTransform>();

            achievementWindowRectTransform.offsetMax = new Vector2(200, 127.645f);
            achievementWindowRectTransform.offsetMin = new Vector2(-200, -159.165f);

            // Exit Button
            GameObject closeButton = achievementWindow.transform.GetChild(0).GetChild(0).gameObject;

            Button[] buttonComponents = closeButton.GetComponents<Button>();

            // Destroy first unused button
            Object.Destroy(buttonComponents[0]);

            buttonComponents[1].onClick.RemoveAllListeners();
            buttonComponents[1].onClick.AddListener(ARGAchievementLogic.CloseAchievementPopup);

            // Resize the Window
            GameObject scrollView = achievementWindow.transform.GetChild(1).gameObject;

            RectTransform scrollViewRectTransform = scrollView.GetComponent<RectTransform>();

            scrollViewRectTransform.offsetMax = new Vector2(190, 104.965f);
            scrollViewRectTransform.offsetMin = new Vector2(-190.515f, -133.125f);

            // Add achievement parts

            // Remove text in content
            contentBox = scrollView.transform.GetChild(0).GetChild(0).gameObject;

            Object.Destroy(contentBox.transform.GetChild(0).gameObject);

            VerticalLayoutGroup contentBoxVerticalLayoutGroup = contentBox.GetComponent<VerticalLayoutGroup>();
            contentBoxVerticalLayoutGroup.spacing = 10;

            contentBoxVerticalLayoutGroup.padding = new RectOffset(10, 10, 10, 10);
            contentBoxVerticalLayoutGroup.childControlHeight = true;
            contentBoxVerticalLayoutGroup.childForceExpandHeight = false;
        }

        /// <summary>
        /// Adds an achievement element to be rendered in the achievement list.
        /// </summary>
        /// <param name="achievement">Achievement to be added.</param>
        private static void CreateAchievementInAchievementList(CustomAchievement achievement)
        {
            if (contentBox == null)
            {
                LoggingHelper.ErrorLog(
                    "Attempted to add achievement element to achievement window while it is not created. " +
                    "Unable to add achievement.");
                return;
            }

            GameObject achievementBox = new GameObject(achievement.AchievementName, typeof(RectTransform));
            achievementBox.transform.SetParent(contentBox.transform, false);

            achievementBox.AddComponent<CanvasRenderer>();
            achievementBox.AddComponent<CanvasGroup>().blocksRaycasts = false;

            // Add for enable
            achievementBox.AddComponent<ARGAchievementListElement>().Initialize(achievement);

            RectTransform achievementBoxRectTransform = achievementBox.GetComponent<RectTransform>();
            achievementBoxRectTransform.sizeDelta = new Vector2(AchievementBoxWidth, AchievementBoxHeight);

            LayoutElement layoutElement = achievementBox.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = AchievementBoxWidth;
            layoutElement.preferredHeight = AchievementBoxHeight;

            // Background
            GameObject backgroundCorner = new GameObject("backgroundCorner", typeof(RectTransform));
            backgroundCorner.transform.SetParent(achievementBox.transform, false);

            RectTransform backgroundCornerRectTransform = backgroundCorner.GetComponent<RectTransform>();
            backgroundCornerRectTransform.anchorMin = new Vector2(0, 0);
            backgroundCornerRectTransform.anchorMax = new Vector2(1, 1);
            backgroundCornerRectTransform.offsetMin = Vector2.zero;
            backgroundCornerRectTransform.offsetMax = Vector2.zero;

            Image backgroundCornerImage = backgroundCorner.AddComponent<Image>();
            backgroundCornerImage.color = new Color32(125, 125, 255, 255);
            backgroundCornerImage.sprite = null;

            AdhereToPalette backgroundCornerAdhereToPalette = backgroundCorner.AddComponent<AdhereToPalette>();
            backgroundCornerAdhereToPalette.colorSwatchInt = 2;
            MyImage.SetValue(backgroundCornerAdhereToPalette, backgroundCornerImage);
            backgroundCornerAdhereToPalette.ChangeColors();

            // Inner Background
            GameObject innerBackground = new GameObject("innerBackgroundImage", typeof(RectTransform));
            innerBackground.transform.SetParent(backgroundCorner.transform, false);

            RectTransform innerBackgroundImageRectTransform = innerBackground.GetComponent<RectTransform>();
            innerBackgroundImageRectTransform.anchorMin = new Vector2(0.01f, 0.05f);
            innerBackgroundImageRectTransform.anchorMax = new Vector2(0.99f, 0.95f);
            innerBackgroundImageRectTransform.offsetMin = Vector2.zero;
            innerBackgroundImageRectTransform.offsetMax = Vector2.zero;

            Image innerBackgroundImage = innerBackground.AddComponent<Image>();
            innerBackgroundImage.color = new Color32(125, 125, 255, 255);
            innerBackgroundImage.sprite = null;

            AdhereToPalette innerBackgroundAdhereToPalette = innerBackground.AddComponent<AdhereToPalette>();
            innerBackgroundAdhereToPalette.colorSwatchInt = 3;
            MyImage.SetValue(innerBackgroundAdhereToPalette, innerBackgroundImage);
            innerBackgroundAdhereToPalette.ChangeColors();

            // Text
            GameObject titlePart = new GameObject("titlePart", typeof(RectTransform));
            titlePart.transform.SetParent(achievementBox.transform, false);

            TextMeshProUGUI textPartTextMeshProUGUI = titlePart.AddComponent<TextMeshProUGUI>();
            textPartTextMeshProUGUI.color = Color.black;

            RectTransform textPartRectTransform = titlePart.GetComponent<RectTransform>();
            textPartRectTransform.anchorMin = new Vector2(0, 0);
            textPartRectTransform.anchorMax = new Vector2(1, 1);

            if (achievement.AchievementIcon == null)
            {
                textPartRectTransform.offsetMin = new Vector2(10, 0);
                textPartRectTransform.offsetMax = new Vector2(-10, 0);

                textPartTextMeshProUGUI.fontSize = 20;
                textPartTextMeshProUGUI.alignment = TextAlignmentOptions.MidlineLeft;
            }
            else
            {
                // Add Achievement Icon
                GameObject achievementIcon = new GameObject("achievementIcon", typeof(RectTransform));
                achievementIcon.transform.SetParent(achievementBox.transform, false);

                RectTransform achievementIconRectTransform = achievementIcon.GetComponent<RectTransform>();
                //achievementIconRectTransform.anchorMin = new Vector2(0, 0);
                //achievementIconRectTransform.anchorMax = new Vector2(1, 1);

                achievementIconRectTransform.offsetMin = Vector2.zero;
                achievementIconRectTransform.offsetMax = Vector2.zero;

                achievementIconRectTransform.anchorMin = new Vector2(0, 0.5f);
                achievementIconRectTransform.anchorMax = new Vector2(0, 0.5f);

                achievementIconRectTransform.sizeDelta = new Vector2(60, 60);

                achievementIconRectTransform.anchoredPosition = new Vector2(40, 0);

                Image achievementIconImage = achievementIcon.AddComponent<Image>();
                achievementIconImage.color = new Color32(255, 255, 255, 255);
                achievementIconImage.sprite = achievement.AchievementIcon;

                // Adjust the Title
                textPartRectTransform.offsetMin = new Vector2(70, 40);
                textPartRectTransform.offsetMax = new Vector2(-170, 0);

                textPartTextMeshProUGUI.fontSize = 18;
                textPartTextMeshProUGUI.alignment = TextAlignmentOptions.Center;

                // Adding Description
                GameObject descriptionPart = new GameObject("descriptionPart", typeof(RectTransform));
                descriptionPart.transform.SetParent(achievementBox.transform, false);

                TextMeshProUGUI descriptionPartTextMeshProUGUI = descriptionPart.AddComponent<TextMeshProUGUI>();
                descriptionPartTextMeshProUGUI.color = Color.black;

                descriptionPartTextMeshProUGUI.fontSize = 13;
                descriptionPartTextMeshProUGUI.alignment = TextAlignmentOptions.Left;

                descriptionPartTextMeshProUGUI.enableWordWrapping = true;
                descriptionPartTextMeshProUGUI.overflowMode = TextOverflowModes.Truncate;
                descriptionPartTextMeshProUGUI.verticalAlignment = VerticalAlignmentOptions.Top;

                RectTransform descriptionPartRectTransform = descriptionPart.GetComponent<RectTransform>();
                descriptionPartRectTransform.anchorMin = new Vector2(0, 0);
                descriptionPartRectTransform.anchorMax = new Vector2(1, 1);

                descriptionPartRectTransform.pivot = new Vector2(0, 1f);
                descriptionPartRectTransform.sizeDelta = new Vector2(-75, -25);
                descriptionPartRectTransform.anchoredPosition = new Vector2(77, -27);
            }

            titlePart.transform.SetAsLastSibling();
        }

        /// <summary>
        /// Adds all ARG achievements to the achievement list.
        /// </summary>
        public static void AddAllARGAchievements()
        {
            CreateAchievementInAchievementList(ARGAchievementSave.TheStart);
            CreateAchievementInAchievementList(ARGAchievementSave.TheReturn);
            CreateAchievementInAchievementList(ARGAchievementSave.TheRespite);
            CreateAchievementInAchievementList(ARGAchievementSave.TheChorus);
            CreateAchievementInAchievementList(ARGAchievementSave.TheClimax);

            CreateAchievementInAchievementList(ARGAchievementSave.TheEndOfEverything);
            CreateAchievementInAchievementList(ARGAchievementSave.TheDeathOfDave);

            CreateAchievementInAchievementList(ARGAchievementSave.TheGoldStandard);
            CreateAchievementInAchievementList(ARGAchievementSave.TheWorstOfTheWorst);
            CreateAchievementInAchievementList(ARGAchievementSave.TheBestOfTheBest);

            CreateAchievementInAchievementList(ARGAchievementSave.Critter101);
            CreateAchievementInAchievementList(ARGAchievementSave.ClockedIn);
            CreateAchievementInAchievementList(ARGAchievementSave.DrSafety);

            CreateAchievementInAchievementList(ARGAchievementSave.FMinus);
            CreateAchievementInAchievementList(ARGAchievementSave.GoAway);
            CreateAchievementInAchievementList(ARGAchievementSave.GoodRiddance);

            CreateAchievementInAchievementList(ARGAchievementSave.EmployeeOfTheMonth);

            CreateAchievementInAchievementList(ARGAchievementSave.TWFuIG9uIHRoZSBTY3JlZW4);
            CreateAchievementInAchievementList(ARGAchievementSave.ArcanaMachinae);

            CreateAchievementInAchievementList(ARGAchievementSave.EmployeeOfTheCentury);
        }
    }
}