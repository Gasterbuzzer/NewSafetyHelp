using NewSafetyHelp.ARG.ARGLogic;
using NewSafetyHelp.ImportFiles;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NewSafetyHelp.ARG.ARGGUI
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
             * Create Keypad Window
             */

            GameObject achievementPopup = Object
                .Instantiate(mainMenuCanvas.transform.GetChild(4).gameObject, mainMenuCanvas.transform).gameObject;

            ARGAchievementLogic.SetKeypadPopup(achievementPopup);

            // Rename Program
            achievementPopup.name = "AchievementPopup";

            GameObject programTitle = achievementPopup.transform.GetChild(0).GetChild(3).gameObject;

            programTitle.GetComponent<TextMeshProUGUI>().text = "ACHIEVEMENT POPUP";

            GameObject programLogo = achievementPopup.transform.GetChild(0).GetChild(2).gameObject;

            programLogo.GetComponent<Image>().sprite = EmbedLoader.AdminIcon;

            // Resize the Window
            RectTransform keypadRectTransform = achievementPopup.GetComponent<RectTransform>();

            keypadRectTransform.offsetMax = new Vector2(200, 127.645f);
            keypadRectTransform.offsetMin = new Vector2(-200, -159.165f);

            // Exit Button
            GameObject closeButton = achievementPopup.transform.GetChild(0).GetChild(0).gameObject;

            Button[] buttonComponents = closeButton.GetComponents<Button>();

            // Destroy first unused button
            Object.Destroy(buttonComponents[0]);

            buttonComponents[1].onClick.RemoveAllListeners();
            buttonComponents[1].onClick.AddListener(ARGAchievementLogic.CloseAchievementPopup);

            // Resize the Window
            GameObject scrollView = achievementPopup.transform.GetChild(1).gameObject;

            RectTransform scrollViewRectTransform = scrollView.GetComponent<RectTransform>();

            scrollViewRectTransform.offsetMax = new Vector2(190, 104.965f);
            scrollViewRectTransform.offsetMin = new Vector2(-190.515f, -133.125f);
        }
    }
}