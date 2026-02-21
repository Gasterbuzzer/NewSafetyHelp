using System.IO;
using MelonLoader.Utils;
using NewSafetyHelp.LoggingSystem;

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
                LoggingHelper.InfoLog("No UserData Folder found, creating one.");
                Directory.CreateDirectory(modFolder);
            }

            return modFolder;
        }
    }
}
