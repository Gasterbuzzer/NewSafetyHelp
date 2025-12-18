using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using MelonLoader;
using NewSafetyHelp.CallerPatches.CallerModel;
using NewSafetyHelp.CustomCampaign.CustomCampaignModel;
using NewSafetyHelp.CustomCampaign.Modifier.Data;
using NewSafetyHelp.CustomCampaign.Themes;
using NewSafetyHelp.EntryManager.EntryData;

namespace NewSafetyHelp.CustomCampaign
{
    public static class CustomCampaignGlobal
    {
        public static List<CustomCampaignExtraInfo> customCampaignsAvailable = new List<CustomCampaignExtraInfo>();

        // ReSharper disable once RedundantDefaultMemberInitializer
        public static bool inCustomCampaign = false;

        public static string currentCustomCampaignName = "";

        /// <summary>
        /// Activates the custom campaign values.
        /// </summary>
        /// <param name="customCampaignName">Name of the custom campaign to set as the current one.</param>
        public static void activateCustomCampaign(string customCampaignName)
        {
            inCustomCampaign = true;
            currentCustomCampaignName = customCampaignName;
        }

        /// <summary>
        /// Deactivates the custom campaign and sets values as if it were the main campaign.
        /// </summary>
        public static void deactivateCustomCampaign()
        {
            inCustomCampaign = false;
            currentCustomCampaignName = "";
        }

        /// <summary>
        /// Returns the current campaign as CustomCampaignExtraInfo.
        /// </summary>
        /// <returns>CustomCampaignExtraInfo Object of the current activate custom campaign.</returns>
        public static CustomCampaignExtraInfo getActiveCustomCampaign()
        {
            return customCampaignsAvailable.Find(scannedCampaign =>
                scannedCampaign.campaignName == currentCustomCampaignName);
        }

        /// <summary>
        /// Gets the custom caller by its order id provided. 
        /// </summary>
        /// <param name="orderID">Order number in the current custom campaign.</param>
        /// <returns>CustomCallerExtraInfo Object with the returned object. If not found, default. </returns>
        public static CustomCallerExtraInfo getCustomCallerFromActiveCampaign(int orderID)
        {
            return getActiveCustomCampaign().customCallersInCampaign
                .Find(customCaller => customCaller.orderInCampaign == orderID);
        }

        /// <summary>
        /// Gets the custom entry by its name.
        /// </summary>
        /// <param name="entryName"> Name of the entry to find. </param>
        /// <returns>EntryExtraInfo Object with the returned object. If not found, default. </returns>
        public static EntryExtraInfo getEntryFromActiveCampaign(string entryName)
        {
            return getActiveCustomCampaign().entriesOnlyInCampaign.Find(customEntry => customEntry.Name == entryName);
        }

        /// <summary>
        /// Finds the ID for a given custom theme.
        /// </summary>
        /// <param name="theme">Theme to get the ID from.</param>
        /// <returns>ID of the theme if found. -1 if not found or if something went wrong.</returns>
        private static int getThemeID(ThemesExtraInfo theme)
        {
            CustomCampaignExtraInfo customCampaign = getActiveCustomCampaign();

            if (customCampaign == null)
            {
                MelonLogger.Error("ERROR: customCampaignExtraInfo is null! Unable of getting theme ID!");
                return -1;
            }

            if (customCampaign.customThemesGeneral != null)
            {
                int generalIDSearch = customCampaign.customThemesGeneral.IndexOf(theme);
                if (generalIDSearch != -1)
                {
                    return generalIDSearch + 4;
                }
            }

            if (customCampaign.customThemesDays != null)
            {
                int conditionalIDSearch = customCampaign.customThemesDays.IndexOf(theme);
                if (conditionalIDSearch != -1)
                {
                    if (customCampaign.customThemesGeneral != null)
                    {
                        conditionalIDSearch += customCampaign.customThemesGeneral.Count;
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
        public static ThemesExtraInfo getThemeFromID(int themeID)
        {
            CustomCampaignExtraInfo customCampaign = getActiveCustomCampaign();

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

            if (customCampaign.customThemesGeneral != null && customCampaign.customThemesGeneral.Count > 0)
            {
                foreach (ThemesExtraInfo theme in customCampaign.customThemesGeneral)
                {
                    currentThemeID++;

                    if (theme != null && currentThemeID == themeID)
                    {
                        return theme;
                    }
                }
            }

            if (customCampaign.customThemesDays != null && customCampaign.customThemesDays.Count > 0)
            {
                foreach (ThemesExtraInfo theme in customCampaign.customThemesDays)
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
        public static int getThemeIDFromName(string themeName)
        {
            CustomCampaignExtraInfo customCampaign = getActiveCustomCampaign();

            if (customCampaign == null)
            {
                MelonLogger.Error("ERROR: customCampaignExtraInfo is null! Unable of setting conditional theme!");
                return -1;
            }

            int currentThemeID = 3;

            if (customCampaign.customThemesGeneral != null && customCampaign.customThemesGeneral.Count > 0)
            {
                foreach (ThemesExtraInfo theme in customCampaign.customThemesGeneral)
                {
                    currentThemeID++;

                    if (theme != null && theme.themeName.Equals(themeName, StringComparison.OrdinalIgnoreCase))
                    {
                        return currentThemeID;
                    }
                }
            }

            if (customCampaign.customThemesDays != null && customCampaign.customThemesDays.Count > 0)
            {
                foreach (ThemesExtraInfo theme in customCampaign.customThemesDays)
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
        public static int checkIfConditionalTheme()
        {
            CustomCampaignExtraInfo customCampaign = getActiveCustomCampaign();

            if (customCampaign == null)
            {
                MelonLogger.Error("ERROR: customCampaignExtraInfo is null! Unable of setting conditional theme!");
                return -1;
            }

            bool themeFound = false;
            ThemesExtraInfo currentTheme = getActiveTheme(ref themeFound);

            if (currentTheme == null) // Theme is default or not set. No conditional theme can be applied.
            {
                return -1;
            }

            if (customCampaign.customThemesDays != null && customCampaign.customThemesDays.Count > 0)
            {
                foreach (ThemesExtraInfo theme in customCampaign.customThemesDays)
                {
                    if (theme != null && theme.attachedToTheme.Equals(currentTheme.themeName))
                    {
                        if (theme.unlockDays.Contains(GlobalVariables.currentDay))
                        {
                            int foundThemeID = getThemeID(theme);

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
        public static ThemesExtraInfo getActiveTheme(ref bool isCustomTheme)
        {
            CustomCampaignExtraInfo customCampaign = getActiveCustomCampaign();

            if (customCampaign == null)
            {
                MelonLogger.Error("ERROR: customCampaignExtraInfo is null! Unable of getting the active theme!");
                return null;
            }
            
            isCustomTheme = false;

            if (customCampaign.activeTheme <= 3) // Default Theme
            {
                return null;
            }

            int indexAsGeneral = customCampaign.activeTheme - 4;

            if (indexAsGeneral >= 0 
                && customCampaign.customThemesGeneral != null
                && indexAsGeneral < customCampaign.customThemesGeneral.Count) // We have a general theme.
            {
                isCustomTheme = true;
                return customCampaign.customThemesGeneral[indexAsGeneral];
            }

            int indexAsDays = customCampaign.activeTheme - 4;

            if (customCampaign.customThemesGeneral != null)
            {
                indexAsDays -= customCampaign.customThemesGeneral.Count;
            }

            if (indexAsDays >= 0
                && customCampaign.customThemesDays != null
                && indexAsDays < customCampaign.customThemesDays.Count) // We have a (conditional) days theme.
            {
                isCustomTheme = true;
                return customCampaign.customThemesDays[indexAsDays];
            }

            return null;
        }

        /// <summary>
        /// From the provided selected value it returns the value that fits all the criteria.
        /// </summary>
        /// <returns>Returns default value if we no value is set. If set, it returns the requested value.</returns>
        [CanBeNull]
        public static TValue getActiveModifierValue<TValue>(Func<ModifierExtraInfo, TValue> selector,
            ref bool foundModifier, Func<TValue, bool> predicate = null,
            Func<ModifierExtraInfo, bool> specialPredicate = null)
        {
            CustomCampaignExtraInfo customCampaignExtraInfo = getActiveCustomCampaign();

            if (customCampaignExtraInfo == null)
            {
                MelonLogger.Error("ERROR: customCampaignExtraInfo is null! Unable of getting modifier!");
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

            if (customCampaignExtraInfo.customModifiersGeneral != null)
            {
                foreach (ModifierExtraInfo modifierGeneral in customCampaignExtraInfo.customModifiersGeneral)
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

            if (customCampaignExtraInfo.customModifiersDays != null)
            {
                foreach (ModifierExtraInfo modifierDay in customCampaignExtraInfo.customModifiersDays)
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
        /// <param name="_monsterProfileList">MonsterProfileList to add the entries to.</param>
        public static void addAllCustomCampaignEntriesToArray(ref MonsterProfileList _monsterProfileList)
        {
            CustomCampaignExtraInfo customCampaignExtraInfo = getActiveCustomCampaign();

            if (customCampaignExtraInfo == null)
            {
                MelonLogger.Error(
                    "ERROR: customCampaignExtraInfo is null! Unable of adding entries to custom campaign!");
                return;
            }

            #if DEBUG
            MelonLogger.Msg(
                $"DEBUG: Now adding all {customCampaignExtraInfo.entriesOnlyInCampaign.Count} entries to the custom campaign.");
            #endif

            // Add all entries.
            foreach (EntryExtraInfo entryInCustomCampaign in customCampaignExtraInfo.entriesOnlyInCampaign)
            {
                #if DEBUG
                MelonLogger.Msg($"DEBUG: Adding entry {entryInCustomCampaign.Name} to custom campaign.");
                #endif

                EntryManager.EntryManager.AddMonsterToTheProfile(entryInCustomCampaign.referenceCopyEntry,
                    ref _monsterProfileList.monsterProfiles, "allEntries");
            }

            // Sort afterward
            EntryManager.EntryManager.SortMonsterProfiles(ref _monsterProfileList.monsterProfiles);
        }

        public static void replaceAllProvidedCampaignEntries(ref MonsterProfileList _monsterProfileList)
        {
            CustomCampaignExtraInfo customCampaignExtraInfo = getActiveCustomCampaign();

            if (customCampaignExtraInfo == null || !inCustomCampaign)
            {
                MelonLogger.Error(
                    "ERROR: customCampaignExtraInfo is null! Unable of adding entries to custom campaign!");
                return;
            }

            #if DEBUG
            MelonLogger.Msg(
                $"DEBUG: Now replacing all {customCampaignExtraInfo.entryReplaceOnlyInCampaign.Count} entries to the custom campaign.");
            #endif

            if (_monsterProfileList.monsterProfiles.Length <= 0)
            {
                #if DEBUG
                MelonLogger.Msg($"DEBUG: Monster Profile ");
                #endif
                return;
            }

            for (int i = 0; i < _monsterProfileList.monsterProfiles.Length; i++)
            {
                MonsterProfile realEntry = _monsterProfileList.monsterProfiles[i];

                if (realEntry == null)
                {
                    MelonLogger.Warning("WARNING: realEntry is null! Unable of replacing entry for this entry!");
                    return;
                }

                // Find matching entry to replace
                EntryExtraInfo entryFound = customCampaignExtraInfo.entryReplaceOnlyInCampaign.Find(replaceEntry =>
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
                    EntryManager.EntryManager.DeleteMonsterProfile(ref _monsterProfileList.monsterProfiles, null,
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

                    _monsterProfileList.monsterProfiles[i] = entryFound.referenceCopyEntry;

                    #if DEBUG
                    MelonLogger.Msg($"DEBUG: Replacing entry {entryFound.Name} with custom entry in custom campaign.");
                    #endif
                }
            }
        }
    }
}