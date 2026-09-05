using NewSafetyHelp.LoggingSystem;
using UnityEngine;

namespace NewSafetyHelp.ARG
{
    public static class ARGKeypadLogic
    {
        // GameObject References
        private static GameObject keypadPopup;

        /// <summary>
        /// Opens the keypad popup prompt.
        /// </summary>
        public static void OpenKeyPadPopup()
        {
            if (keypadPopup != null)
            {
                keypadPopup.SetActive(true);
            }
        }

        /// <summary>
        /// Closes the keypad popup prompt.
        /// </summary>
        public static void CloseKeyPadPopup()
        {
            if (keypadPopup != null)
            {
                keypadPopup.SetActive(false);
            }
        }

        /// <summary>
        /// Sets the keypad popup correctly.
        /// </summary>
        /// <param name="keypad"></param>
        public static void SetKeypadPopup(GameObject keypad)
        {
            keypadPopup = keypad;
        }
    }
}