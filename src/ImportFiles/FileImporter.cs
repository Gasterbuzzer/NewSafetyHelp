using System.IO;
using MelonLoader;
using MelonLoader.Utils;

namespace NewSafetyHelp.ImportFiles
{
    public static class FileImporter
    {
        /// <summary>
        /// Returns the UserData Directory folder for the mod. (Example: ...\GameDirectory\UserData\NewSafetyHelp\)
        /// </summary>
        public static string GetUserDataFolderPath()
        {
            string modFolder = Path.Combine(MelonEnvironment.UserDataDirectory, "NewSafetyHelp");

            if (!Directory.Exists(modFolder)) // Our cute folder.
            {
                MelonLogger.Msg("INFO: No UserData Folder found, creating one.");
                Directory.CreateDirectory(modFolder);
            }

            return modFolder;
        }
    }
}
