using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using NewSafetyHelp.Audio.Music.Data;
using NewSafetyHelp.Callers.CallerModel;
using NewSafetyHelp.CustomCampaignSystem.CustomCampaignModel;
using NewSafetyHelp.CustomCampaignSystem.Modifier.Data;
using NewSafetyHelp.CustomCampaignSystem.Themes;
using NewSafetyHelp.CustomVideos;
using NewSafetyHelp.Emails;
using NewSafetyHelp.EntryManager.EntryData;
using NewSafetyHelp.LoggingSystem;
using UnityEngine;

namespace NewSafetyHelp.CustomCampaignSystem
{
    public static class CustomCampaignGlobal
    {
        public static readonly List<CustomCampaign> CustomCampaignsAvailable = new List<CustomCampaign>();
        
        public static bool InCustomCampaign => currentCustomCampaign != null;
        
        private static CustomCampaign currentCustomCampaign;

        /// <summary>
        /// Activates the custom campaign values.
        /// </summary>
        /// <param name="customCampaignName">Name of the custom campaign to set as the current one.</param>
        public static void ActivateCustomCampaign(string customCampaignName)
        {
            currentCustomCampaign = CustomCampaignsAvailable.Find(
                customCampaign =>
                    customCampaign.CampaignName == customCampaignName
                );
        }

        /// <summary>
        /// Deactivates the custom campaign and sets values as if it were the main campaign.
        /// </summary>
        public static void DeactivateCustomCampaign()
        {
            currentCustomCampaign = null;
        }

        /// <summary>
        /// Returns the current campaign as CustomCampaign.
        /// </summary>
        /// <returns>CustomCampaign Object of the current activate custom campaign.</returns>
        public static CustomCampaign GetActiveCustomCampaign()
        {
            if (currentCustomCampaign == null)
            {
                LoggingHelper.CampaignNullError();
            }
            
            return currentCustomCampaign;
        }
        
        /// <summary>
        /// Returns the campaign of the given name.
        /// </summary>
        /// <param name="customCampaignName">Name of the custom campaign to find.</param>
        /// <returns>CustomCampaign Object of the current activate custom campaign.</returns>
        public static CustomCampaign GetNamedCustomCampaign(string customCampaignName)
        {
            return CustomCampaignsAvailable.Find(
                customCampaignSearch => customCampaignSearch.CampaignName == customCampaignName
                );
        }

        /// <summary>
        /// Gets the custom caller by its order id provided. 
        /// </summary>
        /// <param name="orderID">Order number in the current custom campaign.</param>
        /// <returns>CustomCCaller Object with the returned object. If not found, default. </returns>
        [CanBeNull]
        public static CustomCCaller GetCustomCallerFromActiveCampaign(int orderID)
        {
            CustomCampaign customCampaign = GetActiveCustomCampaign();
            
            if (customCampaign == null)
            {
                return null;
            }
            
            return customCampaign.CustomCallersInCampaign.Find(
                customCaller => customCaller.OrderInCampaign == orderID);
        }
        
        /// <summary>
        /// Gets the custom music from custom campaign.
        /// </summary>
        /// <param name="musicToFind">RichAudioClip of the music to find.</param>
        /// <returns>CustomMusic object of the found object. If not found, default.</returns>
        [CanBeNull]
        public static CustomMusic GetCustomMusicFromActiveCampaign(RichAudioClip musicToFind)
        {
            CustomCampaign customCampaign = GetActiveCustomCampaign();
            
            if (customCampaign == null)
            {
                return null;
            }
            
            return customCampaign.CustomMusic.Find(customMusic => customMusic.MusicClip == musicToFind);
        }
        
        /// <summary>
        /// Gets the custom video from custom campaign.
        /// </summary>
        /// <param name="videoGameObject">Video GameObject to find the custom video for.</param>
        /// <returns>CustomVideo object of the found object. If not found, default.</returns>
        [CanBeNull]
        public static CustomVideo GetCustomVideoFromActiveCampaign(GameObject videoGameObject)
        {
            CustomCampaign customCampaign = GetActiveCustomCampaign();
            
            if (customCampaign == null)
            {
                return null;
            }
            
            return customCampaign.CustomVideos.Find(
                customVideo => customVideo.ReferenceToCreatedVideo == videoGameObject);
        }
        
        /// <summary>
        /// Gets the custom email from custom campaign.
        /// </summary>
        /// <param name="emailToFind"> Email one wishes to find in the custom campaign.</param>
        /// <returns>CustomEmail object of the found object. If not found, default.</returns>
        [CanBeNull]
        public static CustomEmail GetCustomEmailFromActiveCampaign(Email emailToFind)
        {
            CustomCampaign customCampaign = GetActiveCustomCampaign();
            
            if (customCampaign == null)
            {
                return null;
            }
            
            return customCampaign.Emails.Find(customEmail => customEmail.ReferenceToEmailObject == emailToFind);
        }

        /// <summary>
        /// Gets the custom entry by its name.
        /// </summary>
        /// <param name="entryName"> Name of the entry to find. </param>
        /// <returns>EntryMetadata Object with the returned object. If not found, default. </returns>
        public static EntryMetadata GetEntryFromActiveCampaign(string entryName)
        {
            CustomCampaign customCampaign = GetActiveCustomCampaign();
            
            if (customCampaign == null)
            {
                return null;
            }
            
            return customCampaign.EntriesOnlyInCampaign.Find(customEntry => customEntry.Name == entryName);
        }

        /// <summary>
        /// Finds the ID for a given custom theme.
        /// </summary>
        /// <param name="theme">Theme to get the ID from.</param>
        /// <returns>ID of the theme if found. -1 if not found or if something went wrong.</returns>
        private static int GetThemeID(CustomTheme theme)
        {
            CustomCampaign customCampaign = GetActiveCustomCampaign();

            if (customCampaign == null)
            {
                LoggingHelper.CampaignNullError();
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
            CustomCampaign customCampaign = GetActiveCustomCampaign();

            if (customCampaign == null)
            {
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
            CustomCampaign customCampaign = GetActiveCustomCampaign();

            if (customCampaign == null)
            {
                return -1;
            }

            int currentThemeID = 3;

            if (customCampaign.CustomThemesGeneral != null && customCampaign.CustomThemesGeneral.Count > 0)
            {
                foreach (CustomTheme theme in customCampaign.CustomThemesGeneral)
                {
                    currentThemeID++;

                    if (theme != null && theme.ThemeName.Equals(themeName, StringComparison.OrdinalIgnoreCase))
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

                    if (theme != null && theme.ThemeName.Equals(themeName, StringComparison.OrdinalIgnoreCase))
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
            CustomCampaign customCampaign = GetActiveCustomCampaign();

            if (customCampaign == null)
            {
                LoggingHelper.CampaignNullError();
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
                    if (theme != null && theme.AttachedToTheme.Equals(currentTheme.ThemeName))
                    {
                        if (theme.UnlockDays.Contains(GlobalVariables.currentDay))
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
        /// <returns>Returns the actual active theme.
        /// Null if we failed or the theme is a default theme from the game.</returns>
        [CanBeNull]
        public static CustomTheme GetActiveTheme(ref bool isCustomTheme)
        {
            CustomCampaign customCampaign = GetActiveCustomCampaign();

            if (customCampaign == null)
            {
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
        /// Iterates through all modifier types and tries finding a valid value to use.
        /// </summary>
        /// <returns>(Tuple) Returns as the first parameter, if we found any value.
        /// The second value is the actual picked value.</returns>
        public static DesktopModifierSnapshot GetModifierDesktopSnapshot()
        {
            CustomCampaign customCampaign = GetActiveCustomCampaign();

            if (customCampaign == null)
            {
                return null;
            }

            DesktopModifierSnapshot newModifierSnapshot = new DesktopModifierSnapshot();

            // Loops through each modifier source and selects the value that fits the criteria.
            // If multiple exist, it will pick the first valid value of that modifier source type.
            // The list is ordered based on priority, elements that come first have the smallest priority, while
            // elements that come later have higher priority.
            foreach (ModifierSource modifierSource in customCampaign.ModifierSources)
            {
                if (modifierSource == null
                    || modifierSource.Modifiers == null
                    || modifierSource.Modifiers.Count == 0)
                {
                    continue;
                }

                if (!modifierSource.SourceCondition(customCampaign))
                {
                    continue;
                }

                foreach (CustomModifier modifier in modifierSource.Modifiers)
                {
                    /*
                     * Username Section
                     */

                    if (modifier.UsernameText.HasChanged)
                    {
                        newModifierSnapshot.UsernameText = (true, modifier.UsernameText);
                    }
                    
                    
                    /*
                     * Main Game Desktop program
                     */

                    if (modifier.RenameMainGameDesktopIcon.HasChanged)
                    {
                        newModifierSnapshot.RenameMainGameDesktopIcon = (true, modifier.RenameMainGameDesktopIcon);
                    }

                    if (modifier.MainGameDesktopIcon.HasChanged)
                    {
                        newModifierSnapshot.MainGameDesktopIcon = (true, modifier.MainGameDesktopIcon);
                    }
                    
                    /*
                     * Desktop Logo Section
                     */
                    if (modifier.DisableDesktopLogo.HasChanged)
                    {
                        newModifierSnapshot.DisableDesktopLogo = (true, modifier.DisableDesktopLogo);
                    }

                    if (modifier.CustomBackgroundLogo.HasChanged)
                    {
                        newModifierSnapshot.CustomBackgroundLogo = (true, modifier.CustomBackgroundLogo);
                    }

                    if (modifier.BackgroundLogoTransparency.HasChanged)
                    {
                        newModifierSnapshot.BackgroundLogoTransparency = (true, modifier.BackgroundLogoTransparency);
                    }
                    
                    /*
                     * Video Player Section
                     */

                    if (modifier.VideoPlayerDesktopIsWideMode.HasChanged)
                    {
                        newModifierSnapshot.VideoPlayerDesktopIsWideMode = (true, modifier.VideoPlayerDesktopIsWideMode);
                    }
                    
                    /*
                     * Mailbox Section
                     */

                    if (modifier.MailboxIcon.HasChanged)
                    {
                        newModifierSnapshot.MailboxIcon = (true, modifier.MailboxIcon);
                    }

                    if (modifier.MailboxRename.HasChanged)
                    {
                        newModifierSnapshot.MailboxRename = (true, modifier.MailboxRename);
                    }

                    if (modifier.ApplicationMailboxTitle.HasChanged)
                    {
                        newModifierSnapshot.ApplicationMailboxTitle = (true, modifier.ApplicationMailboxTitle);
                    }
                    
                    if (modifier.ApplicationMailboxIcon.HasChanged)
                    {
                        newModifierSnapshot.ApplicationMailboxIcon = (true, modifier.ApplicationMailboxIcon);
                    }
                    
                    if (modifier.DisplayMailboxOnDesktop.HasChanged)
                    {
                        newModifierSnapshot.DisplayMailboxOnDesktop = (true, modifier.DisplayMailboxOnDesktop);
                    }
                    
                    /*
                     * Entry Browser Section
                     */

                    if (modifier.EntryBrowserIcon.HasChanged)
                    {
                        newModifierSnapshot.EntryBrowserIcon = (true, modifier.EntryBrowserIcon);
                    }

                    if (modifier.EntryBrowserRename.HasChanged)
                    {
                        newModifierSnapshot.EntryBrowserRename = (true, modifier.EntryBrowserRename);
                    }

                    if (modifier.ApplicationEntryBrowserTitle.HasChanged)
                    {
                        newModifierSnapshot.ApplicationEntryBrowserTitle = (true, modifier.ApplicationEntryBrowserTitle);
                    }
                    
                    if (modifier.ApplicationEntryBrowserIcon.HasChanged)
                    {
                        newModifierSnapshot.ApplicationEntryBrowserIcon = (true, modifier.ApplicationEntryBrowserIcon);
                    }
                    

                    /*
                     * Options Section
                     */

                    if (modifier.OptionsIcon.HasChanged)
                    {
                        newModifierSnapshot.OptionsIcon = (true, modifier.OptionsIcon);
                    }

                    if (modifier.OptionsRename.HasChanged)
                    {
                        newModifierSnapshot.OptionsRename = (true, modifier.OptionsRename);
                    }

                    if (modifier.ApplicationOptionsTitle.HasChanged)
                    {
                        newModifierSnapshot.ApplicationOptionsTitle = (true, modifier.ApplicationOptionsTitle);
                    }
                    
                    if (modifier.ApplicationOptionsIcon.HasChanged)
                    {
                        newModifierSnapshot.ApplicationOptionsIcon = (true, modifier.ApplicationOptionsIcon);
                    }

                    /*
                     * Artbook Section
                     */

                    if (modifier.ArtbookIcon.HasChanged)
                    {
                        newModifierSnapshot.ArtbookIcon = (true, modifier.ArtbookIcon);
                    }

                    if (modifier.ArtbookRename.HasChanged)
                    {
                        newModifierSnapshot.ArtbookRename = (true, modifier.ArtbookRename);
                    }

                    if (modifier.ApplicationArtbookTitle.HasChanged)
                    {
                        newModifierSnapshot.ApplicationArtbookTitle = (true, modifier.ApplicationArtbookTitle);
                    }
                    
                    if (modifier.ApplicationArtbookIcon.HasChanged)
                    {
                        newModifierSnapshot.ApplicationArtbookIcon = (true, modifier.ApplicationArtbookIcon);
                    }

                    if (modifier.ArtbookPages != null
                        && modifier.ArtbookPages.Count > 0)
                    {
                        newModifierSnapshot.ArtbookPages = (true, modifier.ArtbookPages);
                    }
                    
                    /*
                     * Arcade Section
                     */

                    if (modifier.ArcadeIcon.HasChanged)
                    {
                        newModifierSnapshot.ArcadeIcon = (true, modifier.ArcadeIcon);
                    }

                    if (modifier.ArcadeRename.HasChanged)
                    {
                        newModifierSnapshot.ArcadeRename = (true, modifier.ArcadeRename);
                    }

                    /*
                     * Scorecard Section
                     */

                    if (modifier.ScorecardIcon.HasChanged)
                    {
                        newModifierSnapshot.ScorecardIcon = (true, modifier.ScorecardIcon);
                    }

                    if (modifier.ScorecardRename.HasChanged)
                    {
                        newModifierSnapshot.ScorecardRename = (true, modifier.ScorecardRename);
                    }

                    if (modifier.ApplicationScorecardTitle.HasChanged)
                    {
                        newModifierSnapshot.ApplicationScorecardTitle = (true, modifier.ApplicationScorecardTitle);
                    }
                    
                    if (modifier.ApplicationScorecardIcon.HasChanged)
                    {
                        newModifierSnapshot.ApplicationScorecardIcon = (true, modifier.ApplicationScorecardIcon);
                    }

                    /*
                     * Credits Section
                     */

                    if (modifier.DesktopCredits.HasChanged)
                    {
                        newModifierSnapshot.DesktopCredits = (true, modifier.DesktopCredits);
                    }

                    if (modifier.CreditsRename.HasChanged)
                    {
                        newModifierSnapshot.CreditsRename = (true, modifier.CreditsRename);
                    }

                    if (modifier.CreditsIcon.HasChanged)
                    {
                        newModifierSnapshot.CreditsIcon = (true, modifier.CreditsIcon);
                    }

                    if (modifier.HideDesktopCredits.HasChanged)
                    {
                        newModifierSnapshot.HideDesktopCredits = (true, modifier.HideDesktopCredits);
                    }

                    /*
                     * Discord Section
                     */

                    if (modifier.HideDiscordProgram.HasChanged)
                    {
                        newModifierSnapshot.HideDiscordProgram = (true, modifier.HideDiscordProgram);
                    }
                }
            }

            return newModifierSnapshot;
        }

        /// <summary>
        /// Iterates through all modifier types and tries finding a valid value to use.
        /// </summary>
        /// <param name="selector">Function that selects the value from the modifier.</param>
        /// <param name="predicate">Requirement for the picked value.</param>
        /// <param name="specialPredicate">Requirement for the modifier.</param>
        /// <typeparam name="TValue">Value of return value.</typeparam>
        /// <returns>(Tuple) Returns as the first parameter, if we found any value.
        /// The second value is the actual picked value.</returns>
        public static (bool foundModifier, TValue value) GetActiveModifierValue<TValue>(Func<CustomModifier,
                TValue> selector, Func<TValue, bool> predicate = null,
            Func<CustomModifier, bool> specialPredicate = null)
        {
            CustomCampaign customCampaign = GetActiveCustomCampaign();

            if (customCampaign == null)
            {
                return (false, default);
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
            bool foundModifier = false;

            // Loops through each modifier source and selects the value that fits the criteria.
            // If multiple exist, it will pick the first valid value of that modifier source type.
            // The list is ordered based on priority, elements that come first have the smallest priority, while
            // elements that come later have higher priority.
            foreach (ModifierSource modifierSource in customCampaign.ModifierSources)
            {
                if (modifierSource == null 
                    || modifierSource.Modifiers == null
                    || modifierSource.Modifiers.Count == 0)
                {
                    continue;
                }

                if (!modifierSource.SourceCondition(customCampaign))
                {
                    continue;
                }
                
                (bool found, TValue value) modifierResult = GetModifierValueFromList(
                    modifierSource.Modifiers, selector, predicate, 
                    modifier => modifierSource.ModifierExtraSelectionCondition(modifier)
                                && specialPredicate(modifier));

                if (modifierResult.found)
                {
                    foundModifier = true;
                    selectedValue = modifierResult.value;
                }
            }
            
            return (foundModifier, selectedValue);
        }

        /// <summary>
        /// Gets the first valid modifier value from a given list of modifiers. Returns first valid or default value.
        /// </summary>
        /// <param name="modifierList">List with all modifiers to be checked.</param>
        /// <param name="selector">Lambda function to select the value out of the modifier.</param>
        /// <param name="predicate">Predicate to check if the value is valid.</param>
        /// <param name="specialPredicate">Special predicate that only works for modifiers.</param>
        /// <typeparam name="TValue">Variable type of the selected parameter.</typeparam>
        /// <returns>First valid result or default if not found.</returns>
        private static (bool found, TValue value) GetModifierValueFromList<TValue>(
            List<CustomModifier> modifierList,
            Func<CustomModifier, TValue> selector,
            Func<TValue, bool> predicate, Func<CustomModifier, bool> specialPredicate)
        {
            if (modifierList == null)
            {
                return (false, default);
            }
            
            foreach (CustomModifier modifier in modifierList)
            {
                if (modifier == null)
                {
                    continue;
                }

                TValue value = selector(modifier);

                if (predicate(value) && specialPredicate(modifier))
                {
                    return (true, value);
                }
            }
            
            return (false, default);
        }

        /// <summary>
        /// Adds all entries of a custom campaign to the array of entries.
        /// </summary>
        /// <param name="monsterProfileList">MonsterProfileList to add the entries to.</param>
        public static void AddAllCustomCampaignEntriesToArray(ref MonsterProfileList monsterProfileList)
        {
            CustomCampaign customCampaign = GetActiveCustomCampaign();

            if (customCampaign == null)
            {
                return;
            }

            LoggingHelper.DebugLog(() => 
                $"Now adding all {customCampaign.EntriesOnlyInCampaign.Count} entries to the custom campaign.");

            // Add all entries.
            foreach (EntryMetadata entryInCustomCampaign in customCampaign.EntriesOnlyInCampaign)
            {
                LoggingHelper.DebugLog(() => 
                    $"Adding entry {entryInCustomCampaign.Name} to custom campaign.");

                EntryManager.EntryManager.AddMonsterToTheProfile(entryInCustomCampaign.ReferenceCopyEntry,
                    ref monsterProfileList.monsterProfiles, "allEntries");
            }

            // Sort afterward
            EntryManager.EntryManager.SortMonsterProfiles(ref monsterProfileList.monsterProfiles);
        }

        /// <summary>
        /// Replaces all the main campaign entries with the provided entries. Only if a match was found.
        /// </summary>
        /// <param name="monsterProfileList">List of entries that want to replace something.</param>
        public static void ReplaceAllProvidedCampaignEntries(ref MonsterProfileList monsterProfileList)
        {
            CustomCampaign customCampaign = GetActiveCustomCampaign();
            
            if (!InCustomCampaign || customCampaign == null)
            {
                return;
            }

            LoggingHelper.DebugLog(() =>
                $"Now replacing all {customCampaign.EntryReplaceOnlyInCampaign.Count} entries to the custom campaign.");

            if (monsterProfileList.monsterProfiles.Length <= 0)
            {
                return;
            }

            for (int i = 0; i < monsterProfileList.monsterProfiles.Length; i++)
            {
                MonsterProfile realEntry = monsterProfileList.monsterProfiles[i];

                if (realEntry == null)
                {
                    LoggingHelper.WarningLog("realEntry is null! Unable of replacing entry for this entry!");
                    return;
                }

                // Find matching entry to replace
                EntryMetadata entryFound = customCampaign.EntryReplaceOnlyInCampaign.Find(replaceEntry =>
                    replaceEntry.Name.Equals(realEntry.monsterName) || replaceEntry.ID.Equals(realEntry.monsterID)
                );

                // If we delete the entry or replace it.
                if (entryFound != null) // Delete
                {
                    if (entryFound.DeleteEntry)
                    {
                        if (string.IsNullOrEmpty(entryFound.Name))
                        {
                            LoggingHelper.WarningLog("Monster entry was not found. Is is the correct name?");
                            continue;
                        }

                        // Delete by name.
                        EntryManager.EntryManager.DeleteMonsterProfile(ref monsterProfileList.monsterProfiles,
                            null, entryFound.Name);

                        LoggingHelper.DebugLog(() => 
                            $"Deleting entry '{entryFound.Name}' in custom campaign.");
                    }
                    else // It exists, so replace it.
                    {
                        if (entryFound.ReferenceCopyEntry == null)
                        {
                            // I am too lazy to implement this.
                            // But if ever returns errors or problems, I will implement it this way.
                            LoggingHelper.WarningLog("referenceCopyEntry of EntryFound is null. " +
                                                     "Was the entry initialized?");
                            continue;
                        }

                        monsterProfileList.monsterProfiles[i] = entryFound.ReferenceCopyEntry;

                        LoggingHelper.DebugLog(() => 
                            $"Replacing entry {entryFound.Name} with custom entry in custom campaign.");
                    }
                    
                }
            }
        }
    }
}