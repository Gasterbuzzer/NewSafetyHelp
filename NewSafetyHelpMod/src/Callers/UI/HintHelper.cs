using System.Collections.Generic;
using NewSafetyHelp.CustomCampaignSystem.Modifier.Data;
using NewSafetyHelp.LoggingSystem;
using Random = UnityEngine.Random;

namespace NewSafetyHelp.Callers.UI
{
    public static class HintHelper
    {
        /// <summary>
        /// Picks a random hint if none was provided.
        /// </summary>
        /// <returns></returns>
        public static string GetRandomHint()
        {
            List<string> hints = new List<string>
            {
                "Did you know that you can pause the game?",
                "Did you know that you can replay the audio of the caller if you missed it?",
                "Did you know that you can fail every caller on the first day?",
                "Did you know that your score for the final day won't get stored?",
                "Did you know that you can adjust your experience in the settings?",
                "Did you know that the game is in 32-bit?",
                "Did you know this game was made in Unity?"
            };

            return hints[Random.Range(0, hints.Count)];
        }

        /// <summary>
        /// Gets a random hint from the modifier hint list. It also accounts for random chances.
        /// </summary>
        /// <param name="hintForTheDay">Modifier found values for the hints for the day.</param>
        /// <param name="hintChances">Chances of all hints. If none provided, all hints have the same chance.</param>
        /// <returns>(String) Hint to be displayed.</returns>
        public static string GetHintFromList((bool foundModifier, VariableChanged<List<string>> value) hintForTheDay,
            (bool foundModifier, VariableChanged<List<float>> value) hintChances)
        {
            string randomHint = "";

            if (hintChances.foundModifier)
            {
                if (hintChances.value.Data.Count == hintForTheDay.value.Data.Count)
                {
                    float totalHintChance = 0;

                    foreach (float hintChance in hintChances.value.Data)
                    {
                        totalHintChance += hintChance;
                    }

                    float randomHintPicked = Random.Range(0, totalHintChance);

                    float summedHintChance = 0;
                    for (int i = 0; i < hintChances.value.Data.Count; i++)
                    {
                        summedHintChance += hintChances.value.Data[i];

                        if (randomHintPicked <= summedHintChance)
                        {
                            randomHint = hintForTheDay.value.Data[i];
                            break;
                        }
                    }
                }
                else
                {
                    LoggingHelper.WarningLog("Hint chances and hints must have the same count. Ignoring hint chances.");
                    randomHint = hintForTheDay.value.Data[Random.Range(0, hintForTheDay.value.Data.Count)];
                }
            }
            else
            {
                randomHint = hintForTheDay.value.Data[Random.Range(0, hintForTheDay.value.Data.Count)];
            }

            if (randomHint.Equals(""))
            {
                LoggingHelper.WarningLog("Hint returned empty. Possibly no hints provided?");
            }

            return randomHint;
        }
    }
}