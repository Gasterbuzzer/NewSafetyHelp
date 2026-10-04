using JetBrains.Annotations;
using MelonLoader;
using UnityEngine;

namespace NewSafetyHelp.ARG.Achievements
{
    public class CustomAchievement
    {
        public readonly string AchievementName;

        public readonly string AchievementDescription;

        [CanBeNull] public readonly Sprite AchievementIcon;

        private readonly bool AchievementHidden;

        public MelonPreferences_Entry<bool> AchievementPreference;

        /// <summary>
        /// Constructor for custom achievement.
        /// </summary>
        /// <param name="achievementName">Name of the achievement.</param>
        /// <param name="achievementDescription">Description of the achievement.</param>
        /// <param name="achievementIcon">Icon of the achievement.</param>
        /// <param name="achievementHidden">If the achievement is a hidden achievement.</param>
        public CustomAchievement(string achievementName, string achievementDescription, Sprite achievementIcon,
            bool achievementHidden)
        {
            AchievementName = achievementName;
            AchievementDescription = achievementDescription;
            AchievementHidden = achievementHidden;
            AchievementIcon = achievementIcon;
        }

        /// <summary>
        /// Constructor for custom achievement.
        /// </summary>
        /// <param name="achievementName">Name of the achievement.</param>
        /// <param name="achievementDescription">Description of the achievement.</param>
        /// <param name="achievementIcon">Icon of the achievement.</param>
        /// <param name="ARGAchievementCategory">Category to add the achievement to.</param>
        /// <param name="achievementHidden">If the achievement is a hidden achievement.</param>
        public CustomAchievement(string achievementName, string achievementDescription, Sprite achievementIcon,
            MelonPreferences_Category ARGAchievementCategory, bool achievementHidden = false)
        {
            AchievementName = achievementName;
            AchievementDescription = achievementDescription;
            AchievementHidden = achievementHidden;
            AchievementIcon = achievementIcon;

            InitializeAchievementPreference(ARGAchievementCategory);
        }

        /// <summary>
        /// Initializes the achievement preference to be able to be used.
        /// </summary>
        /// <param name="ARGAchievementCategory">Category to add the achievement to.</param>
        private void InitializeAchievementPreference(MelonPreferences_Category ARGAchievementCategory)
        {
            string nameToCheck = AchievementName.Trim().Replace(" ", "").Replace(".", "").Replace(",", "")
                .Replace("=", "").Replace("[", "").Replace("]", "").Replace("#", "").ToLower();

            if (ARGAchievementCategory == null)
            {
                return;
            }

            if (ARGAchievementCategory.GetEntry<bool>(nameToCheck) == null)
            {
                AchievementPreference = ARGAchievementCategory.CreateEntry(nameToCheck, false);
            }
            else
            {
                AchievementPreference = ARGAchievementCategory.GetEntry<bool>(nameToCheck);
            }
        }
    }
}