using System;

namespace NewSafetyHelp.CustomThemes
{
    public static class ColorHelper
    {
        /// <summary>
        /// Converts the given float into the correct format.
        /// A possible format is, if the color is given in 0-255 format. This will get converted into a float (0-1).
        /// </summary>
        /// <param name="currentColorValue">The color component value to normalize, expressed either in the 0–1 or 0–255 range.</param>
        /// <returns> Converted to a float if in wrong format or simply returned if no change is needed. </returns>
        public static float GetConvertedColorFloat(float currentColorValue)
        {
            currentColorValue = Math.Abs(currentColorValue);
            
            if (currentColorValue <= 1.0f) // 0-1 Format
            {
                return currentColorValue;
            }
            
            return currentColorValue / 255.0f; // 0-255 format
        }
    }
}