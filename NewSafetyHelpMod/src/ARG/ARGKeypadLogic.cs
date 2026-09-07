using UnityEngine;

namespace NewSafetyHelp.ARG
{
    public static class ARGKeypadLogic
    {
        // GameObject References
        private static GameObject keypadPopup;

        public static bool KeyPadIsOpen;

        /// <summary>
        /// Opens the keypad popup prompt.
        /// </summary>
        public static void OpenKeyPadPopup()
        {
            if (keypadPopup != null)
            {
                keypadPopup.SetActive(true);
                KeyPadIsOpen = true;
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
                KeyPadIsOpen = false;
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