using MelonLoader;
using System.IO;

namespace NewSafetyHelp.src.ImportFiles
{
    public static class FileImporter
    {
        public static string getUserDataFolderPath()
        {
            string modFolder = Path.Combine(MelonUtils.UserDataDirectory, "NewSafetyHelp");

            if (!Directory.Exists(modFolder)) // Our cute folder.
            {
                Directory.CreateDirectory(modFolder);
            }

            return modFolder;
        }
    }
}
