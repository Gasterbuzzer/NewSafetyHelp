using System.Collections;
using UnityEngine;

namespace NewSafetyHelp.ARG.Achievements.GUI.AchievementPopup
{
    public static class AchievementPopupGUI
    {
        private static bool isShowingPopup;

        private static GameObject achievementPopup;

        private static readonly Vector3 TopPosition = new Vector3(285, -260, 0);

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

            RectTransform achievementPopupRectTransform = achievementPopup.GetComponent<RectTransform>();

            achievementPopupRectTransform.offsetMax = new Vector3(230, 0, 0);
            achievementPopupRectTransform.offsetMin = new Vector3(0, -80, 0);

            achievementPopupRectTransform.localPosition = TopPosition;
        }

        public static IEnumerator TriggerAchievementPopup()
        {
            while (isShowingPopup)
            {
                yield return new WaitForEndOfFrame();
            }

            if (achievementPopup != null)
            {
                yield return new WaitForSeconds(2);

                isShowingPopup = true;
                achievementPopup.SetActive(true);

                yield return new WaitForSeconds(5);

                isShowingPopup = false;
                achievementPopup.SetActive(false);
            }
        }
    }
}