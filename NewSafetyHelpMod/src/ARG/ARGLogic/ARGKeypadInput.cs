using System.Collections.Generic;
using NewSafetyHelp.ARG.ARGGUI;
using UnityEngine;

namespace NewSafetyHelp.ARG.ARGLogic
{
    public static class ARGKeypadInput
    {
        /// <summary>
        /// MonoBehaviour class for capturing input for the ARG.
        /// </summary>
        public class ARGCaptureKeypadInput : MonoBehaviour
        {
            private Dictionary<KeyCode, int> validKeys = new Dictionary<KeyCode, int>
            {
                [KeyCode.Alpha0] = 0,
                [KeyCode.Alpha1] = 1,
                [KeyCode.Alpha2] = 2,
                [KeyCode.Alpha3] = 3,
                [KeyCode.Alpha4] = 4,
                [KeyCode.Alpha5] = 5,
                [KeyCode.Alpha6] = 6,
                [KeyCode.Alpha7] = 7,
                [KeyCode.Alpha8] = 8,
                [KeyCode.Alpha9] = 9,
            };

            private void Update()
            {
                if (ARGKeypadLogic.KeyPadIsOpen)
                {
                    if (Input.GetKeyDown(KeyCode.Backspace))
                    {
                        ARGKeypad.RemoveLastDigit();
                    }
                    else if (Input.anyKeyDown)
                    {
                        foreach (KeyCode numberKey in validKeys.Keys)
                        {
                            if (Input.GetKeyDown(numberKey))
                            {
                                ARGKeypad.AddDigit(validKeys[numberKey]);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}