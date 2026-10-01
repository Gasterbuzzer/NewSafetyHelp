using NewSafetyHelp.ImportFiles;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NewSafetyHelp.ARG.Achievements.GUI
{
    public static class ARGAchievements
    {
        public static void CreateAchievementsIcon()
        {
            GameObject mainMenuCanvas = GameObject.Find("MainMenuCanvas");

            /*
             * Add Keypad, for inputting the code.
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
            RectTransform keypadRectTransform = achievementWindow.GetComponent<RectTransform>();

            keypadRectTransform.offsetMax = new Vector2(200, 127.645f);
            keypadRectTransform.offsetMin = new Vector2(-200, -159.165f);

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
            GameObject contentBox = scrollView.transform.GetChild(0).GetChild(0).gameObject;

            Object.Destroy(contentBox.transform.GetChild(0).gameObject);

            VerticalLayoutGroup contentBoxVerticalLayoutGroup = contentBox.GetComponent<VerticalLayoutGroup>();
            contentBoxVerticalLayoutGroup.spacing = 100;

            // Create example box?
            for (int i = 0; i < 9; i++)
            {
                GameObject achievementBox = new GameObject($"achievementBox{i}", typeof(RectTransform));
                achievementBox.transform.SetParent(contentBox.transform, false);

                achievementBox.AddComponent<CanvasRenderer>();
                achievementBox.AddComponent<CanvasGroup>().blocksRaycasts = false;

                RectTransform achievementBoxRectTransform = achievementBox.GetComponent<RectTransform>();
                achievementBoxRectTransform.sizeDelta = new Vector2(300, 10);

                LayoutElement layoutElement = achievementBox.AddComponent<LayoutElement>();
                layoutElement.preferredWidth = 300;
                layoutElement.preferredHeight = 10;

                // Images
                GameObject backgroundImage = new GameObject("backgroundImage", typeof(RectTransform));
                backgroundImage.transform.SetParent(achievementBox.transform, false);

                RectTransform backgroundImageRectTransform = backgroundImage.GetComponent<RectTransform>();
                backgroundImageRectTransform.anchorMin = new Vector2(0, 0);
                backgroundImageRectTransform.anchorMax = new Vector2(1, 1);

                Image achievementBoxImage = backgroundImage.AddComponent<Image>();
                achievementBoxImage.color = new Color32(125, 125, 255, 255);
                achievementBoxImage.sprite = null;

                // Text
                GameObject textPart = new GameObject("textPart", typeof(RectTransform));
                textPart.transform.SetParent(achievementBox.transform, false);

                RectTransform textPartRectTransform = textPart.GetComponent<RectTransform>();
                textPartRectTransform.anchorMin = new Vector2(0, 0);
                textPartRectTransform.anchorMax = new Vector2(1, 1);
                textPartRectTransform.offsetMin = new Vector2(0, 0);
                textPartRectTransform.offsetMax = new Vector2(0, 0);

                TextMeshProUGUI textPartTextMeshProUGUI = textPart.AddComponent<TextMeshProUGUI>();
                textPartTextMeshProUGUI.fontSize = 36;
                textPartTextMeshProUGUI.color = Color.black;
                textPartTextMeshProUGUI.text = $"achievementBox{i}";

                textPart.transform.SetAsLastSibling();
            }
        }
    }
}