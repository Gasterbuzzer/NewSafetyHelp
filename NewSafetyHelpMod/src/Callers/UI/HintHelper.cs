using System.Collections.Generic;
using UnityEngine;

namespace NewSafetyHelp.Callers.UI
{
    public static class HintHelper
    {
        public static string GetRandomHint()
        {
            List<string> hints = new List<string>
            {
                "Did you know that you can pause the game?",
                "Did you know that you can replay the audio of the caller if you missed it?",
                "Did you know that you can fail every caller on the first day?",
                "Did you know that your score for the final day won't get stored?",
                "Did you know that you can adjust your experience in the settings?",
                "Did you know that the game is in 32-bit?"
            };

            return hints[Random.Range(0, hints.Count)];
        }
    }
}