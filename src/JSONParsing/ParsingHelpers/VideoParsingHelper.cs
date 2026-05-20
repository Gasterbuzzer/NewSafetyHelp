using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using NewSafetyHelp.HelperFunctions;
using NewSafetyHelp.LoggingSystem;
using Newtonsoft.Json.Linq;

namespace NewSafetyHelp.JSONParsing.ParsingHelpers
{
    public static class VideoParsingHelper
    {
        /// <summary>
        /// All available video types supported by both Unity and our dynamic import.
        /// </summary>
        public enum VideoType
        {
            AVI,
            MOV,
            MP4,
            MPEG,
            ASF,
            NONE
        }

        /// <summary>
        /// Checks for a given video path, if the provided file is supported by unity and the video parser.
        /// </summary>
        /// <param name="filePath">Path to the video.</param>
        /// <returns>(Bool) True => Known file extension. False => Unknown file extension.</returns>
        public static bool IsKnownVideoExtension(string filePath)
        {
            if (!File.Exists(filePath))
            {
                LoggingHelper.ErrorLog($"Provided video '{filePath}' could not be found.");
                return false;
            }

            string fileExtension = Path.GetExtension(filePath).ToLowerInvariant().Trim();

            if (string.IsNullOrEmpty(fileExtension))
            {
                LoggingHelper.WarningLog(
                    $"Provided video file extension for video '{filePath}' is empty. Will attempt to find correct format.");
                return false;
            }

            switch (fileExtension)
            {
                case ".asf":
                case ".avi":
                case ".dv":
                case ".m4v":
                case ".mov":
                case ".mp4":
                case ".mpg":
                case ".mpeg":
                case ".ogv":
                case ".vp8":
                case ".webm":
                case ".wmv":
                    return true;
            }

            LoggingHelper.InfoLog(
                $"Provided video file extension for video '{filePath}' is unknown. Will attempt to find correct format.");
            return false;
        }

        /// <summary>
        /// Attempts to find out video format from the given file.
        /// </summary>
        /// <param name="filePath">Path to the file.</param>
        /// <returns>(VideoType) If found => VideoType that is associated with file. If not => VideoType.None</returns>
        public static VideoType TryFindingVideoType(string filePath)
        {
            // How many bytes we read from the header.
            const int headerByteCount = 16;

            // Open the file as read:
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (BinaryReader binaryReader = new BinaryReader(fileStream))
                {
                    byte[] headerBytes = binaryReader.ReadBytes(headerByteCount);

                    if (headerBytes.Length < headerByteCount)
                    {
                        LoggingHelper.ErrorLog(
                            $"Provided video file at '{filePath}' is too small to be considered a video file. Unable of assigning video.");
                        return VideoType.NONE;
                    }

                    return MatchSignature(headerBytes);
                }
            }
        }

        /// <summary>
        /// Attempts to parse the header of a given file to try and figure out the file type.
        /// </summary>
        /// <param name="headerByteArray">Array 16 bytes that contain first 16 bytes of the header.</param>
        /// <returns>(VideoType) Video type that was recognized.
        /// If none found or not supported it will return "VideoType.NONE".</returns>
        private static VideoType MatchSignature(byte[] headerByteArray)
        {
            if (headerByteArray.Length < 16)
            {
                LoggingHelper.ErrorLog(
                    "Provided header bytes are less than 16, unable of properly checking for video type.");
                return VideoType.NONE;
            }

            /*
             * MP4 and MOV.
             *
             * The first 4 bytes are just stating how big the box size is.
             * We mostly wish to first check if the after the 4th index if we have "ftyp" in ascii. (0x66 0x74 0x79 0x70)
             * Then we check the "brand".
             */
            string headerMP4Mov = Encoding.ASCII.GetString(headerByteArray, 4, 4).ToLower().Trim();

            if (headerMP4Mov == "ftyp")
            {
                // We now check the major brand of the file (8-11 bytes):
                string majorBrand = Encoding.ASCII.GetString(headerByteArray, 8, 4).ToLower().Trim();

                // QT: QuickTime => Mov
                if (majorBrand == "qt")
                {
                    return VideoType.MOV;
                }
                else // For now simply assume if not quick time, it is an MP4.
                {
                    return VideoType.MP4;
                }
            }

            /*
             * AVI.
             *
             * The first 4 bytes should contain "RIFF" in ASCII.
             * Then the next 4 bytes are the file size.
             * And then next 4 bytes (3) contain the string "AVI" in ASCII:
             */

            string headerRiff = Encoding.ASCII.GetString(headerByteArray, 0, 4).ToLower().Trim();
            string headerAVI = Encoding.ASCII.GetString(headerByteArray, 8, 4).ToLower().Trim();

            if (headerRiff == "riff" &&
                headerAVI == "avi")
            {
                return VideoType.AVI;
            }

            /*
             * MPEG
             * First 4 bytes are the "start code".
             * The first three bytes are just (00 00 01) (0 0 1)
             * The 4th byte is (either BA or B3).
             * B3: MPEG-1
             * BA: MPEG-2 PS
             */
            if (headerByteArray[0] == 0x00 && headerByteArray[1] == 0x00 && headerByteArray[2] == 0x01)
            {
                if (headerByteArray[3] == 0xBA || headerByteArray[3] == 0xB3) // Either MPEG-1 or MPEG-2 PS
                {
                    return VideoType.MPEG;
                }
            }

            /*
             * WMV or ASF:
             * 16-byte GUID: 30 26 B2 75 8E 66 CF 11 A6 D9 00 AA 00 62 CE 6C
             */

            byte[] asfHeaderGUID =
            {
                0x30, 0x26, 0xB2, 0x75,
                0x8E, 0x66, 0xCF, 0x11,
                0xA6, 0xD9, 0x00, 0xAA,
                0x00, 0x62, 0xCE, 0x6C
            };

            // We check if the sequence is equal to the GUID of WMV / ASF
            if (headerByteArray.Take(16).SequenceEqual(asfHeaderGUID))
            {
                return VideoType.ASF;
            }

            return VideoType.NONE;
        }

        /// <summary>
        /// Gets the video file extension as a string.
        /// </summary>
        /// <param name="videoType">Video type to get the file extension from.</param>
        /// <returns>(string) If found => The file extension; If not found => null.</returns>
        [CanBeNull]
        public static string GetVideoTypeExtension(VideoType videoType)
        {
            switch (videoType)
            {
                case VideoType.AVI:
                    return ".avi";

                case VideoType.MOV:
                    return ".mov";

                case VideoType.MP4:
                    return ".mp4";

                case VideoType.MPEG:
                    return ".mpeg";

                case VideoType.ASF:
                    return ".wmv";
            }

            LoggingHelper.ErrorLog($"Unsupported video type '{videoType.ToString()}' provided. " +
                                   "Could not get file extension.");
            return null;
        }

        /// <summary>
        /// This function creates a temporary video file with the correct video extension.
        /// This is mainly used when the provided video file hid its extension.
        /// The temporary files are either deleted by the OS (not guruanteed) or by startup of this mod,
        /// so need to manually delete it.
        /// </summary>
        /// <param name="currentFilePath">Path to the current video file.</param>
        /// <param name="videoType">Type of the video that was detected.</param>
        /// <returns>(bool, string) First value is if the creation worked and second value is the path to the new video file.</returns>
        public static (bool createdTemptFile, string pathToTempFile) CreateTempVideoFile(string currentFilePath,
            VideoType videoType)
        {
            if (string.IsNullOrEmpty(currentFilePath))
            {
                LoggingHelper.ErrorLog("Provided file path is empty. Unable of creating temporary copy.");
                return (false, "");
            }

            if (videoType == VideoType.NONE)
            {
                LoggingHelper.ErrorLog("Video type is not known. Unable of creating temporary copy.");
                return (false, "");
            }

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(currentFilePath);
            string correctFileExtension = GetVideoTypeExtension(videoType);

            if (string.IsNullOrEmpty(correctFileExtension))
            {
                LoggingHelper.ErrorLog("Failed getting a file extension. Unable of creating temporary copy.");
                return (false, "");
            }

            if (string.IsNullOrEmpty(fileNameWithoutExtension))
            {
                LoggingHelper.ErrorLog($"Failed getting file path for '{currentFilePath}' without extension. " +
                                       "Unable of creating temporary copy.");
                return (false, "");
            }

            // We now attempt to create the copy at the given path:

            string tempFilePath = Path.Combine(
                Path.GetTempPath(),
                $"{EmbedHelpers.NewSafetyHelpPrefix}{fileNameWithoutExtension}_{Guid.NewGuid()}{correctFileExtension}");

            try
            {
                File.Copy(currentFilePath, tempFilePath, true);

                LoggingHelper.DebugLog($"Created copy of the video file at '{tempFilePath}'.");
            }
            catch (Exception e)
            {
                LoggingHelper.DebugLog($"Failec creating a copy at '{tempFilePath}'. Reason: \n'\n{e}\n'.");
                return (false, tempFilePath);
            }

            return (true, tempFilePath);
        }

        /// <summary>
        /// Attempts to parse the key for a list.
        /// </summary>
        /// <param name="jObjectParsed">JSON Object where the key is found.</param>
        /// <param name="key">Key to be found.</param>
        /// <param name="target">Targets to write the value to.</param>
        /// <param name="jsonFolderPath"> Folder path where the JSON is located. </param>
        /// <param name="usermodFolderPath"> Folder path where the usermod is located. </param>
        /// <returns>(Bool) If the parsed value was an array (false) or a single element (true).
        /// Null if we failed parsing.</returns>
        public static bool? TryAssignUrlListOrSingleUrl(JObject jObjectParsed, string key, ref List<string> target,
            string jsonFolderPath, string usermodFolderPath)
        {
            bool? result = ParsingHelper.TryAssignListOrSingleElement(jObjectParsed, key, ref target);

            for (int i = 0; i < target.Count; i++)
            {
                if (string.IsNullOrEmpty(target[i]))
                {
                    LoggingHelper.WarningLog("Provided video path is empty. Unable to show video.");
                }
                else
                {
                    string firstFilePath = jsonFolderPath + "\\" + target[i];
                    string videoFileAlternativePath = usermodFolderPath + "\\" + target[i];

                    string correctFilePath = "";
                    
                    if (File.Exists(target[i]))
                    {
                        correctFilePath = target[i];
                    }
                    else if (File.Exists(firstFilePath))
                    {
                        correctFilePath = firstFilePath;
                    }
                    else if (File.Exists(videoFileAlternativePath))
                    {
                        correctFilePath = videoFileAlternativePath;
                    }
                    else if (!File.Exists(firstFilePath) && !File.Exists(videoFileAlternativePath))
                    {
                        LoggingHelper.ErrorLog(
                            $"Could not find video '{target[i]}' in either: '{firstFilePath}' or " +
                            $"'{videoFileAlternativePath}'.");

                        correctFilePath = "";
                    }

                    bool validVideoExtension = IsKnownVideoExtension(correctFilePath);

                    // We check if we have a valid video extension, if not, we try to figure out the video type.
                    if (validVideoExtension)
                    {
                        target[i] = correctFilePath;
                    }
                    else
                    {
                        VideoType videoTypeDiscovered = TryFindingVideoType(correctFilePath);

                        bool createVideoFile = false;

                        switch (videoTypeDiscovered)
                        {
                            // We couldn't find it out, so we failed.
                            case VideoType.NONE:
                                LoggingHelper.ErrorLog(
                                    $"Provided video file at '{correctFilePath}' could not be understood. Possible unsupported file format.");
                                break;

                            default:
                                createVideoFile = true;
                                LoggingHelper.InfoLog(
                                    $"Provided video file '{correctFilePath}' was interpreted as a '{videoTypeDiscovered.ToString()}'.");
                                break;
                        }

                        // Create temp copy from the video we took a peek in.
                        if (createVideoFile)
                        {
                            (bool wasSuccessfull, string newFilePath) =
                                CreateTempVideoFile(correctFilePath, videoTypeDiscovered);

                            if (wasSuccessfull)
                            {
                                target[i] = newFilePath;
                            }
                            else
                            {
                                LoggingHelper.ErrorLog(
                                    "Failed creating a temporary copy of the video. Not showing video.");
                                target[i] = "";
                            }
                        }
                        else
                        {
                            LoggingHelper.InfoLog($"Video type at '{correctFilePath}' seems to be unsupported. " +
                                                  "Attempting to still use it.");
                            target[i] = correctFilePath;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Attempts to assign the video file path to the target string. But only if the video file exists.
        /// </summary>
        /// <param name="jObjectParsed">JSON Object where the key is found.</param>
        /// <param name="key">Key to be found.</param>
        /// <param name="target">Target to write the value to.</param>
        /// <param name="jsonFolderPath">Path to where the JSON is located.</param>
        /// <param name="usermodFolderPath">Path to the parent usermod folder.</param>
        public static bool TryAssignVideoPath(JObject jObjectParsed, string key, ref string target,
            string jsonFolderPath, string usermodFolderPath)
        {
            if (!jObjectParsed.TryGetValue(key, out var token))
            {
                return false;
            }

            string videoFilePath = token.Value<string>();

            string updatedFilePath = jsonFolderPath + "\\" + videoFilePath;
            string videoFileAlternativePath = usermodFolderPath + "\\" + videoFilePath;

            if (string.IsNullOrEmpty(videoFilePath))
            {
                LoggingHelper.WarningLog("Provided video path is empty. Unable to show video.");
                target = "";
            }
            else
            {
                string correctFilePath;
                
                if (File.Exists(videoFilePath))
                {
                    correctFilePath = videoFilePath;
                }
                else if (File.Exists(updatedFilePath))
                {
                    correctFilePath = updatedFilePath;
                }
                else if (File.Exists(videoFileAlternativePath))
                {
                    correctFilePath = videoFileAlternativePath;
                }
                else
                {
                    LoggingHelper.ErrorLog($"Provided video '{videoFilePath}' could not be found in either " +
                                           $"'{updatedFilePath}' " +
                                           $"or '{videoFileAlternativePath}'.");
                    target = "";
                    return true;
                }

                bool validVideoExtension = IsKnownVideoExtension(correctFilePath);

                // We check if we have a valid video extension, if not, we try to figure out the video type.
                if (validVideoExtension)
                {
                    target = correctFilePath;
                }
                else
                {
                    VideoType videoTypeDiscovered = TryFindingVideoType(correctFilePath);

                    bool createVideoFile = false;

                    switch (videoTypeDiscovered)
                    {
                        // We couldn't find it out, so we failed.
                        case VideoType.NONE:
                            LoggingHelper.ErrorLog(
                                $"Provided video file at '{correctFilePath}' could not be understood. Possible unsupported file format.");
                            break;

                        default:
                            createVideoFile = true;
                            LoggingHelper.InfoLog(
                                $"Provided video file '{correctFilePath}' was interpreted as a '{videoTypeDiscovered.ToString()}'.");
                            break;
                    }

                    // Create temp copy from the video we took a peek in.
                    if (createVideoFile)
                    {
                        (bool wasSuccessfull, string newFilePath) =
                            CreateTempVideoFile(correctFilePath, videoTypeDiscovered);

                        if (wasSuccessfull)
                        {
                            target = newFilePath;
                        }
                        else
                        {
                            LoggingHelper.ErrorLog("Failed creating a temporary copy of the video. Not showing video.");
                            target = "";
                        }
                    }
                    else
                    {
                        LoggingHelper.InfoLog($"Video type at '{correctFilePath}' seems to be unsupported. " +
                                              "Attempting to still use it.");
                        target = correctFilePath;
                    }
                }
            }

            return true;
        }
    }
}