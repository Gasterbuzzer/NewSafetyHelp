using System.Collections;
using MelonLoader;
using NewSafetyHelp.CustomCampaignSystem.Modifier.Data;
using UnityEngine;
using UnityEngine.UI;

namespace NewSafetyHelp.CustomCampaignSystem.Hint
{
    public class HintStartScript : MonoBehaviour
    {
        public void Start()
        {
            MelonCoroutines.Start(UpdatedColors());
        }

        /// <summary>
        /// Updates the colors of the hint object.
        /// </summary>
        /// <returns>Coroutine to be run.</returns>
        private IEnumerator UpdatedColors()
        {
            yield return new WaitForSeconds(0.05f);

            // Update Colors
            (bool foundModifier, VariableChanged<bool> value) hintIncreaseColor =
                CustomCampaignGlobal.GetActiveModifierValue(c => c.HintIncreaseColor,
                    vCs => vCs.HasChanged);

            if (hintIncreaseColor.foundModifier
                && hintIncreaseColor.value.Data)
            {
                Image hintGameObjectImage = gameObject.GetComponent<Image>();

                if (hintGameObjectImage.color.b <= 0.56636)
                {
                    hintGameObjectImage.color = new Color(hintGameObjectImage.color.r,
                        hintGameObjectImage.color.g, 0.56636f);
                }
                else if (hintGameObjectImage.color.r <= 0.5783334f)
                {
                    hintGameObjectImage.color = new Color(0.5783334f,
                        hintGameObjectImage.color.g, hintGameObjectImage.color.b);
                }
                else if (hintGameObjectImage.color.g <= 0.5986274f)
                {
                    hintGameObjectImage.color = new Color(hintGameObjectImage.color.r,
                        0.5986274f, hintGameObjectImage.color.b);
                }
            }
        }
    }
}