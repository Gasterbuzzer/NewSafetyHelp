using TMPro;
using UnityEngine;

namespace NewSafetyHelp.ARG.Achievements.GUI
{
    public class ARGAchievementListElement : MonoBehaviour
    {
        private TextMeshProUGUI descriptionTextMeshPro;
        private TextMeshProUGUI titleTextMeshPro;

        private CustomAchievement customAchievementToCheck;

        /// <summary>
        /// Initializes the ARG Achievements variables.
        /// </summary>
        /// <param name="customAchievement">Custom Achievement that this element is associated with.</param>
        public void Initialize(CustomAchievement customAchievement)
        {
            customAchievementToCheck = customAchievement;
        }

        /// <summary>
        /// Find all necessary GameObjects.
        /// </summary>
        private void Awake()
        {
            Transform descriptionPartTransform = gameObject.transform.Find("descriptionPart");

            if (descriptionPartTransform != null)
            {
                descriptionTextMeshPro = descriptionPartTransform.gameObject.GetComponent<TextMeshProUGUI>();
            }

            Transform titlePartTransform = gameObject.transform.Find("titlePart");

            if (titlePartTransform != null)
            {
                titleTextMeshPro = titlePartTransform.gameObject.GetComponent<TextMeshProUGUI>();
            }
        }

        /// <summary>
        /// On each enable / opening of the achievement window.
        /// </summary>
        private void OnEnable()
        {
            // Description
            if (descriptionTextMeshPro != null)
            {
                if (!customAchievementToCheck.AchievementHidden)
                {
                    descriptionTextMeshPro.text = customAchievementToCheck.AchievementDescription;
                }
                else
                {
                    descriptionTextMeshPro.text = "???";
                }

                descriptionTextMeshPro.ForceMeshUpdate(true);

                if (descriptionTextMeshPro.textInfo.lineCount <= 1)
                {
                    descriptionTextMeshPro.verticalAlignment = VerticalAlignmentOptions.Baseline;
                }
                else
                {
                    descriptionTextMeshPro.verticalAlignment = VerticalAlignmentOptions.Top;
                }
            }

            // Title
            if (titleTextMeshPro != null)
            {
                if (!customAchievementToCheck.AchievementHidden)
                {
                    titleTextMeshPro.text = customAchievementToCheck.AchievementName;
                }
                else
                {
                    titleTextMeshPro.text = "???";
                }
            }
        }
    }
}