using System.IO;
using MelonLoader;

namespace NewSafetyHelp.ImportFiles
{
    public static class FileImporter
    {
        /// <summary>
        /// Returns the UserData Directory folder for the mod. (Example: ...\GameDirectory\UserData\NewSafetyHelp\)
        /// </summary>
        public static string GetUserDataFolderPath()
        {
            string modFolder = Path.Combine(MelonUtils.UserDataDirectory, "NewSafetyHelp");

            if (!Directory.Exists(modFolder)) // Our cute folder.
            {
                MelonLogger.Msg("INFO: No UserData Folder found, creating one.");
                Directory.CreateDirectory(modFolder);
            }

            return modFolder;
        }
    }
}
