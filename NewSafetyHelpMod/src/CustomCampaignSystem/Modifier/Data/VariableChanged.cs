namespace NewSafetyHelp.CustomCampaignSystem.Modifier.Data
{
    /// <summary>
    /// An encapsulation for any variable to include as a HasChanged variable.
    /// This describes if the value has been changed.
    /// </summary>
    /// <typeparam name="T">Any type to be stored.</typeparam>
    public class VariableChanged<T>
    {
        public bool HasChanged = false;
        public T Data = default;
    }
}