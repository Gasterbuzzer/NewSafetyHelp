using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using MelonLoader;
using NewSafetyHelp.CustomCampaign.Modifier.Data;
using NewSafetyHelp.CustomCampaign.Themes;
using NewSafetyHelp.EntryManager.EntryData;

namespace NewSafetyHelp.CustomCampaign
{
    public static class CustomCampaignGlobal
    {
        public static readonly List<CustomCampaignModel.CustomCampaign> CustomCampaignsAvailable = new List<CustomCampaignModel.CustomCampaign>();

        // ReSharper disable once RedundantDefaultMemberInitializer
        public static bool InCustomCampaign = false;

        public static string CurrentCustomCampaignName = "";

        /// <summary>
        /// Activates the custom campaign values.
        /// </summary>
        /// <param name="customCampaignName">Name of the custom campaign to set as the current one.</param>
        public static void ActivateCustomCampaign(string customCampaignName)
        {
            InCustomCampaign = true;
            CurrentCustomCampaignName = customCampaignName;
        }

        /// <summary>
        /// Deactivates the custom campaign and sets values as if it were the main campaign.
        /// </summary>
        public static void DeactivateCustomCampaign()
        {
            InCustomCampaign = false;
            CurrentCustomCampaignName = "";
        }

        /// <summary>
        /// Returns the current campaign as CustomCampaign.
        /// </summary>
        /// <returns>CustomCampaign Object of the current activate custom campaign.</returns>
        public static CustomCampaignModel.CustomCampaign GetActiveCustomCampaign()
        {
            return CustomCampaignsAvailable.Find(scannedCampaign =>
                scannedCampaign.CampaignName == CurrentCustomCampaignName);
        }

        /// <summary>
        /// Gets the custom caller by its order id provided. 
        /// </summary>
        /// <param name="orderID">Order number in the current custom campaign.</param>
        /// <returns>CustomCaller Object with the returned object. If not found, default. </returns>
        [CanBeNull]
        public static CallerPatches.CallerModel.CustomCaller GetCustomCallerFromActiveCampaign(int orderID)
        {
            return GetActiveCustomCampaign().CustomCallersInCampaign
                .Find(customCaller => customCaller.OrderInCampaign == orderID);
        }

        /// <summary>
        /// Gets the custom entry by its name.
        /// </summary>
        /// <param name="entryName"> Name of the entry to find. </param>
        /// <returns>EntryMetadata Object with the returned object. If not found, default. </returns>
        public static EntryMetadata GetEntryFromActiveCampaign(string entryName)
        {
            return GetActiveCustomCampaign().EntriesOnlyInCampaign.Find(customEntry => customEntry.Name == entryName);
        }

        /// <summary>
        /// Finds the ID for a given custom theme.
        /// </summary>
        /// <param name="theme">Theme to get the ID from.</param>
        /// <returns>ID of the theme if found. -1 if not found or if something went wrong.</returns>
        private static int GetThemeID(CustomTheme theme)
        {
            CustomCampaignModel.CustomCampaign customCampaign = GetActiveCustomCampaign();

            if (customCampaign == null)
            {
                MelonLogger.Error("ERROR: customCampaignExtraInfo is null! Unable of getting theme ID!");
                return -1;
            }

            if (customCampaign.CustomThemesGeneral != null)
            {
                int generalIDSearch = customCampaign.CustomThemesGeneral.IndexOf(theme);
                if (generalIDSearch != -1)
                {
                    return generalIDSearch + 4;
                }
            }

            if (customCampaign.CustomThemesDays != null)
            {
                int conditionalIDSearch = customCampaign.CustomThemesDays.IndexOf(theme);
                if (conditionalIDSearch != -1)
                {
                    if (customCampaign.CustomThemesGeneral != null)
                    {
                        conditionalIDSearch += customCampaign.CustomThemesGeneral.Count;
                    }
                    
                    return conditionalIDSearch + 4;
                }
            }

            return -1;
        }
        
        /// <summary>
        /// Gets the custom theme of a given custom theme ID.
        /// </summary>
        /// <returns>(Int) null = No valid theme found for the given ID; Otherwise: Theme with the given ID.</returns>
        [CanBeNull]
        public static CustomTheme GetThemeFromID(int themeID)
        {
            CustomCampaignModel.CustomCampaign customCampaign = GetActiveCustomCampaign();

            if (customCampaign == null)
            {
                MelonLogger.Error("ERROR: customCampaignExtraInfo is null! Unable of setting conditional theme!");
                return null;
            }

            // Default them, just return null.
            if (themeID <= 3)
            {
                return null;
            }

            int currentThemeID = 3;

            if (customCampaign.CustomThemesGeneral != null && customCampaign.CustomThemesGeneral.Count > 0)
            {
                foreach (CustomTheme theme in customCampaign.CustomThemesGeneral)
                {
                    currentThemeID++;

                    if (theme != null && currentThemeID == themeID)
                    {
                        return theme;
                    }
                }
            }

            if (customCampaign.CustomThemesDays != null && customCampaign.CustomThemesDays.Count > 0)
            {
                foreach (CustomTheme theme in customCampaign.CustomThemesDays)
                {
                    currentThemeID++;

                    if (theme != null && currentThemeID == themeID)
                    {
                        return theme;
                    }
                }
            }

            return null;
        }
        
        /// <summary>
        /// Gets the theme's ID from the theme's name.
        /// </summary>
        /// <returns>(Int) -1 = No theme found; Otherwise: ID of Theme.</returns>
        public static int GetThemeIDFromName(string themeName)
        {
            CustomCampaignModel.CustomCampaign customCampaign = GetActiveCustomCampaign();

            if (customCampaign == null)
            {
                MelonLogger.Error("ERROR: customCampaignExtraInfo is null! Unable of setting conditional theme!");
                return -1;
            }

            int currentThemeID = 3;

            if (customCampaign.CustomThemesGeneral != null && customCampaign.CustomThemesGeneral.Count > 0)
            {
                foreach (CustomTheme theme in customCampaign.CustomThemesGeneral)
                {
                    currentThemeID++;

                    if (theme != null && theme.themeName.Equals(themeName, StringComparison.OrdinalIgnoreCase))
                    {
                        return currentThemeID;
                    }
                }
            }

            if (customCampaign.CustomThemesDays != null && customCampaign.CustomThemesDays.Count > 0)
            {
                foreach (CustomTheme theme in customCampaign.CustomThemesDays)
                {
                    currentThemeID++;

                    if (theme != null && theme.themeName.Equals(themeName, StringComparison.OrdinalIgnoreCase))
                    {
                        return currentThemeID;
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// Checks if for the current day there is supposed to be a conditional theme active.
        /// </summary>
        /// <returns>(Int) -1 = No theme to be activated; Otherwise: ID of Theme to be activated.</returns>
        public static int CheckIfConditionalTheme()
        {
            CustomCampaignModel.CustomCampaign customCampaign = GetActiveCustomCampaign();

            if (customCampaign == null)
            {
                MelonLogger.Error("ERROR: customCampaignExtraInfo is null! Unable of setting conditional theme!");
                return -1;
            }

            bool themeFound = false;
            CustomTheme currentTheme = GetActiveTheme(ref themeFound);

            if (currentTheme == null) // Theme is default or not set. No conditional theme can be applied.
            {
                return -1;
            }

            if (customCampaign.CustomThemesDays != null && customCampaign.CustomThemesDays.Count > 0)
            {
                foreach (CustomTheme theme in customCampaign.CustomThemesDays)
                {
                    if (theme != null && theme.attachedToTheme.Equals(currentTheme.themeName))
                    {
                        if (theme.unlockDays.Contains(GlobalVariables.currentDay))
                        {
                            int foundThemeID = GetThemeID(theme);

                            if (foundThemeID >= 0)
                            {
                                return foundThemeID;
                            }
                        }
                    }
                }
            }
            
            return -1;
        }

        /// <summary>
        /// Gets the Theme that is current active.
        /// </summary>
        /// <returns>Returns the actual active theme. Null if we failed or the theme is a default theme from the game.</returns>
        [CanBeNull]
        public static CustomTheme GetActiveTheme(ref bool isCustomTheme)
        {
            CustomCampaignModel.CustomCampaign customCampaign = GetActiveCustomCampaign();

            if (customCampaign == null)
            {
                MelonLogger.Error("ERROR: customCampaignExtraInfo is null! Unable of getting the active theme!");
                return null;
            }
            
            isCustomTheme = false;

            if (customCampaign.ActiveTheme <= 3) // Default Theme
            {
                return null;
            }

            int indexAsGeneral = customCampaign.ActiveTheme - 4;

            if (indexAsGeneral >= 0 
                && customCampaign.CustomThemesGeneral != null
                && indexAsGeneral < customCampaign.CustomThemesGeneral.Count) // We have a general theme.
            {
                isCustomTheme = true;
                return customCampaign.CustomThemesGeneral[indexAsGeneral];
            }

            int indexAsDays = customCampaign.ActiveTheme - 4;

            if (customCampaign.CustomThemesGeneral != null)
            {
                indexAsDays -= customCampaign.CustomThemesGeneral.Count;
            }

            if (indexAsDays >= 0
                && customCampaign.CustomThemesDays != null
                && indexAsDays < customCampaign.CustomThemesDays.Count) // We have a (conditional) days theme.
            {
                isCustomTheme = true;
                return customCampaign.CustomThemesDays[indexAsDays];
            }

            return null;
        }

        /// <summary>
        /// From the provided selected value it returns the value that fits all the criteria.
        /// </summary>
        /// <returns>Returns default value if we no value is set. If set, it returns the requested value.</returns>
        [CanBeNull]
        public static TValue GetActiveModifierValue<TValue>(Func<CustomModifier, TValue> selector,
            ref bool foundModifier, Func<TValue, bool> predicate = null,
            Func<CustomModifier, bool> specialPredicate = null)
        {
            CustomCampaignModel.CustomCampaign customCampaign = GetActiveCustomCampaign();

            if (customCampaign == null)
            {
                MelonLogger.Error("ERROR: customCampaign is null! Unable of getting modifier!");
                return default;
            }

            if (predicate == null)
            {
                predicate = _ => true;
            }

            if (specialPredicate == null)
            {
                specialPredicate = _ => true;
            }

            TValue selectedValue = default(TValue);
            foundModifier = false;

            // For each general modifier, we check if it contains the value and add these to the "valid modifier general list".
            // We then pick the first in the list.
            // Note: General modifiers do not require to check for days, since they are always "valid", only the predicate.

            if (customCampaign.CustomModifiersGeneral != null)
            {
                foreach (CustomModifier modifierGeneral in customCampaign.CustomModifiersGeneral)
                {
                    if (modifierGeneral == null)
                    {
                        continue;
                    }

                    TValue value = selector(modifierGeneral);

                    if (predicate(value) && specialPredicate(modifierGeneral))
                    {
                        foundModifier = true;
                        selectedValue = value;
                        break;
                    }
                }
            }

            // Now we check for conditional modifiers. Same work as with the general modifiers.
            // We only need to check if valid for the current day.

            if (customCampaign.CustomModifiersDays != null)
            {
                foreach (CustomModifier modifierDay in customCampaign.CustomModifiersDays)
                {
                    if (modifierDay == null)
                    {
                        continue;
                    }

                    TValue value = selector(modifierDay);

                    if (predicate(value) && modifierDay.unlockDays != null
                                         && modifierDay.unlockDays.Contains(GlobalVariables.currentDay)
                                         && specialPredicate(modifierDay))
                    {
                        foundModifier = true;
                        return value;
                    }
                }
            }
            
            return selectedValue;
        }

        /// <summary>
        /// Adds all entries of a custom campaign to the array of entries.
        /// </summary>
        /// <param name="monsterProfileList">MonsterProfileList to add the entries to.</param>
        public static void AddAllCustomCampaignEntriesToArray(ref MonsterProfileList monsterProfileList)
        {
            CustomCampaignModel.CustomCampaign customCampaign = GetActiveCustomCampaign();

            if (customCampaign == null)
            {
                MelonLogger.Error(
                    "ERROR: customCampaign is null! Unable of adding entries to custom campaign!");
                return;
            }

            #if DEBUG
            MelonLogger.Msg(
                $"DEBUG: Now adding all {customCampaign.EntriesOnlyInCampaign.Count} entries to the custom campaign.");
            #endif

            // Add all entries.
            foreach (EntryMetadata entryInCustomCampaign in customCampaign.EntriesOnlyInCampaign)
            {
                #if DEBUG
                MelonLogger.Msg($"DEBUG: Adding entry {entryInCustomCampaign.Name} to custom campaign.");
                #endif

                EntryManager.EntryManager.AddMonsterToTheProfile(entryInCustomCampaign.referenceCopyEntry,
                    ref monsterProfileList.monsterProfiles, "allEntries");
            }

            // Sort afterward
            EntryManager.EntryManager.SortMonsterProfiles(ref monsterProfileList.monsterProfiles);
        }

        public static void ReplaceAllProvidedCampaignEntries(ref MonsterProfileList monsterProfileList)
        {
            CustomCampaignModel.CustomCampaign customCampaign = GetActiveCustomCampaign();

            if (customCampaign == null || !InCustomCampaign)
            {
                MelonLogger.Error(
                    "ERROR: customCampaign is null! Unable of adding entries to custom campaign!");
                return;
            }

            #if DEBUG
            MelonLogger.Msg(
                $"DEBUG: Now replacing all {customCampaign.EntryReplaceOnlyInCampaign.Count} entries to the custom campaign.");
            #endif

            if (monsterProfileList.monsterProfiles.Length <= 0)
            {
                #if DEBUG
                MelonLogger.Msg($"DEBUG: Monster Profile ");
                #endif
                return;
            }

            for (int i = 0; i < monsterProfileList.monsterProfiles.Length; i++)
            {
                MonsterProfile realEntry = monsterProfileList.monsterProfiles[i];

                if (realEntry == null)
                {
                    MelonLogger.Warning("WARNING: realEntry is null! Unable of replacing entry for this entry!");
                    return;
                }

                // Find matching entry to replace
                EntryMetadata entryFound = customCampaign.EntryReplaceOnlyInCampaign.Find(replaceEntry =>
                    replaceEntry.Name.Equals(realEntry.monsterName) || replaceEntry.ID.Equals(realEntry.monsterID)
                );

                // If we delete the entry or replace it.
                if (entryFound != null && entryFound.deleteEntry) // Delete
                {
                    if (string.IsNullOrEmpty(entryFound.Name))
                    {
                        MelonLogger.Warning("WARNING: Monster entry was not found. Is is the correct name?");
                        continue;
                    }

                    // Delete by name.
                    EntryManager.EntryManager.DeleteMonsterProfile(ref monsterProfileList.monsterProfiles, null,
                        entryFound.Name);

                    #if DEBUG
                    MelonLogger.Msg($"DEBUG: Deleting entry '{entryFound.Name}' in custom campaign.");
                    #endif
                }
                else if (entryFound != null) // It exists, so replace it.
                {
                    if (entryFound.referenceCopyEntry == null)
                    {
                        // I am too lazy to implement this. But if ever returns errors or problems, I will implement it this way.
                        MelonLogger.Warning(
                            "WARNING: referenceCopyEntry of EntryFound is null. Was the entry initialized?");
                        continue;
                    }

                    monsterProfileList.monsterProfiles[i] = entryFound.referenceCopyEntry;

                    #if DEBUG
                    MelonLogger.Msg($"DEBUG: Replacing entry {entryFound.Name} with custom entry in custom campaign.");
                    #endif
                }
            }
        }
    }
}