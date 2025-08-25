using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MelonLoader;
using Newtonsoft.Json.Linq;

namespace NewSafetyHelp.VersionChecker
{
    public static class AsyncVersionChecker
    {
        /// <summary>
        /// Gets the current mod version.
        /// </summary>
        /// <returns>Version of the mod.</returns>
        public static Version getCurrentVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version;
        }

        /// <summary>
        /// Checks if the current version is out of date.
        /// </summary>
        /// <param name="currentVersion"> Current version. </param>
        /// <param name="newVersion"> New version to check against. </param>
        /// <returns>If the newer version is newer.</returns>
        public static bool isOutDatedVersion(Version currentVersion, Version newVersion)
        {
            #if DEBUG
                MelonLogger.Msg($"Checking for outdated version with current version '{currentVersion}' and the new version '{newVersion}'. " +
                                $"(Equal?: {currentVersion == newVersion}) (Newer available? {currentVersion < newVersion}) (Current version newer?: {currentVersion > newVersion})");
            #endif
            
            return currentVersion < newVersion;
        }

        /// <summary>
        /// Parses the tag to a version number.
        /// </summary>
        /// <param name="tag">Tag to convert to version.</param>
        /// <returns></returns>
        [CanBeNull]
        public static Version parseVersionTag(string tag)
        {
            if (string.IsNullOrEmpty(tag) || string.IsNullOrWhiteSpace(tag) || tag == "NO_VERSION_FOUND")
            {
                return null;
            }
            
            // We prepare the parsing
            tag = tag.Trim();

            if (tag.StartsWith("v", StringComparison.OrdinalIgnoreCase)) // Second parameter means it ignores if upper case or lower case.
            {
                tag = tag.Substring(1); // remove the "v".
            }

            Version.TryParse(tag, out var parsedVersion); // Try parsing as version number.
            
            return parsedVersion;
        }
        
        /// <summary>
        /// Attempts to find any new GitHub Mod Version.
        /// </summary>
        /// <returns> Found version on GitHub latest. </returns>
        public static async Task<Version> GetLatestReleaseVersionAsync()
        {
            using (HttpClient http = new HttpClient())
            {
                // Provide flags.
                http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("NewSafetyHelp-Mod", getCurrentVersion().ToString() ));
                http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

                HttpResponseMessage resp = await http.GetAsync("https://api.github.com/repos/Gasterbuzzer/NewSafetyHelp/releases/latest");

                if (!resp.IsSuccessStatusCode) // We failed connecting.
                {
                    MelonLogger.Warning($"WARNING: GitHub API returned error code {resp.StatusCode}. Could not check for updates.");
                    return null;
                }
                
                // Parse response body.
                string responseBody = await resp.Content.ReadAsStringAsync();
                
                JObject jsonObjectParsed = JObject.Parse(responseBody);
                
                // Read off the version.
                string tag = "NO_VERSION_FOUND";
                if (jsonObjectParsed["tag_name"] != null)
                {
                    tag = (string) jsonObjectParsed["tag_name"];
                }
                else if (jsonObjectParsed["name"] != null)
                {
                    tag = (string) jsonObjectParsed["name"];
                }
                
                Version parsedVersion = parseVersionTag(tag);

                if (parsedVersion == null)
                {
                    MelonLogger.Warning($"WARNING: Was unable of parsing version number. Failed check.");
                    return null;
                }
                
                return parsedVersion;
            }
        }

        /// <summary>
        /// Shows the update message in the console.
        /// </summary>
        public static void showUpdateMessage()
        {
            string updateMessage = "NEW VERSION AVAILABLE! Download through your preferred method.";
                    
            MelonLogger.Msg(ConsoleColor.White, "------------------------");
            MelonLogger.Msg(ConsoleColor.Blue, "------------------------");
            MelonLogger.Msg(ConsoleColor.Yellow, "------------------------");
                    
            MelonLogger.Msg(ConsoleColor.Magenta, "\n\n\n");
                    
            MelonLogger.Msg(ConsoleColor.Magenta, updateMessage);
            MelonLogger.Msg(ConsoleColor.Red, updateMessage);
            MelonLogger.Msg(ConsoleColor.Magenta, updateMessage);
                    
            MelonLogger.Msg(ConsoleColor.Magenta, "\n\n\n");
                    
            MelonLogger.Msg(ConsoleColor.Yellow, "------------------------");
            MelonLogger.Msg(ConsoleColor.Blue, "------------------------");
            MelonLogger.Msg(ConsoleColor.White, "------------------------");
        }

        /// <summary>
        /// Checks if there are any updates available for the mod.
        /// </summary>
        public static async Task checkForUpdates()
        {
            Version newestVersion = await GetLatestReleaseVersionAsync();

            if (newestVersion == null)
            {
                MelonLogger.Warning("WARNING: Unable of checking if there is a new version available. Check your internet connection.");
            }
            else
            {
                bool newerVersionAvailable = isOutDatedVersion(getCurrentVersion(), newestVersion);

                if (newerVersionAvailable)
                {
                    MainClassForMonsterEntries._showUpdateMessage = true; // Show the message after loading in.
                    showUpdateMessage();
                }
            }
            
        }

    }
}