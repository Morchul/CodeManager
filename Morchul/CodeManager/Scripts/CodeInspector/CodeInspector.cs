using System.Collections.Generic;
using UnityEngine;

namespace Morchul.CodeManager
{
    /// <summary>
    /// With the CodeInspector you can start inspect files or texts.
    /// The CodeInspector also Handles Read and Write permissions for files.
    /// </summary>
    public static class CodeInspector
    {
        private static readonly List<FileBlocking> fileBlockings = new List<FileBlocking>();
        private static uint inspectionIDCounter = 0;

        /// <summary>
        /// Create CodeInspection for a string
        /// </summary>
        /// <param name="text">The string you want inspect</param>
        /// <returns>The created CodeInspection</returns>
        public static CodeInspection InspectText(string text)
        {
            return new CodeInspection(GetNextInspectionID(), InspectionMode.READ_WRITE, InspectionType.TEXT, text);
        }

        /// <summary>
        /// Create a CodeInspection to inspect a file
        /// </summary>
        /// <param name="path">The path to the file</param>
        /// <param name="mode">The mode in which the CodeInspection should inspect the file</param>
        /// <returns>The CodeInspection if it was created successfully</returns>
        public static CodeInspection InspectFile(string path, InspectionMode mode)
        {
            if (TryFindFileBlockings(path, out FileBlocking[] foundFileBlockings))
            {
                InspectionMode currentMode = GetHighestBlockingMode(foundFileBlockings);
                if (currentMode == InspectionMode.READ_WRITE)
                {
                    //Someone is already writing to it
                    Debug.LogError("A CodeInspection is blocking file: " + path);
                }
                else if (currentMode == InspectionMode.READ)
                {
                    if (mode == InspectionMode.READ)
                    {
                        //Currenty only readers so additional readers are ok
                        return CreateNewFileCodeInspection(mode, path);
                    }
                    else if (mode == InspectionMode.READ_WRITE)
                    {
                        //Others are currently reading so you can not write to it
                        Debug.LogError("A CodeInspection is currenty reading from file: " + path + ". You can only write to a file if no one is reading or writing to it");
                    }
                }
            }
            else
            {
                //No one is reading or writing from this file
                return CreateNewFileCodeInspection(mode, path);
            }

            return null;
        }

        /// <summary>
        /// Stops a CodeInspection from inspecting a File so other CodeInspection can start write to it.
        /// This sets the CodeInspection inactive and can not be used anymore
        /// </summary>
        /// <param name="codeInspection">The CodeInspection which should stop inspecting a File</param>
        public static void StopFileInspection(CodeInspection codeInspection)
        {
            if (codeInspection.Type == InspectionType.TEXT) return;
            if (codeInspection.CodeInspectionID == 0) return;

            if (fileBlockings.RemoveAll(fb => fb.CodeInspectionID == codeInspection.CodeInspectionID) > 0)
                codeInspection.SetInactive();
            else
                Debug.LogError("The CodeInspection + [" + codeInspection.CodeInspectionID + "] could not be removed.");
        }

        /// <summary>
        /// Tries to grant write permission to a CodeInspection
        /// </summary>
        /// <param name="codeInspection">The CodeInspection who wants write permissions</param>
        /// <returns>True if the write permission was successully given</returns>
        public static bool GrantWritePermission(CodeInspection codeInspection)
        {
            if (codeInspection.Mode == InspectionMode.READ_WRITE) return true;

            int countOfOtherFileBlockings = fileBlockings.FindAll(fb => fb.Path == codeInspection.Path && fb.CodeInspectionID != codeInspection.CodeInspectionID).Count;

            if(countOfOtherFileBlockings > 0)
            {
                //Others are currently reading so you can not write to it
                Debug.LogError("A CodeInspection is currenty reading from file: " + codeInspection.Path + ". You can only write to a file if no one is reading or writing to it");
                return false;
            }
            else
            {
                return codeInspection.GrantWritePermission();
            }
        }

        #region private
        /// <summary>
        /// Return the next InspectionID
        /// </summary>
        /// <returns>Next inspectionID</returns>
        private static uint GetNextInspectionID()
        {
            return ++inspectionIDCounter;
        }

        /// <summary>
        /// Creates a new CodeInspection for a file
        /// </summary>
        /// <param name="mode">The mode of the CodeInspection</param>
        /// <param name="path">The path to the inspected file</param>
        /// <returns>The CodeInspection</returns>
        private static CodeInspection CreateNewFileCodeInspection(InspectionMode mode, string path)
        {
            uint nextInspectionID = GetNextInspectionID();
            fileBlockings.Add(CreateFileBlocking(nextInspectionID, mode, path));
            return new CodeInspection(nextInspectionID, mode, InspectionType.FILE, path);
        }

        /// <summary>
        /// Creates a new file blocking
        /// </summary>
        /// <param name="codeInspectionID">The codeInspectionID of the CodeInspection who blocks the file</param>
        /// <param name="mode">The mode of the blocking</param>
        /// <param name="path">The path to the file which gets blocked</param>
        /// <returns>The FileBlocking</returns>
        private static FileBlocking CreateFileBlocking(uint codeInspectionID, InspectionMode mode, string path)
        {
            return new FileBlocking
            {
                Path = path,
                CodeInspectionID = codeInspectionID,
                Mode = mode
            };
        }

        /// <summary>
        /// Returns the Mode of the first FileBlocking. If someone already writes to the file he is the only one other there are only readers
        /// </summary>
        /// <param name="fileBlockings">The blockings on the File</param>
        /// <returns>The highest BlockingMode (READ_WRITE > READ)</returns>
        private static InspectionMode GetHighestBlockingMode(FileBlocking[] fileBlockings)
        {
            return fileBlockings[0].Mode;
        }

        /// <summary>
        /// Tries to find FileBlockings for a specific file
        /// </summary>
        /// <param name="path">The file for which file blockings will be searched</param>
        /// <param name="foundFileBlockings">All found file blockings will be assigned to this array</param>
        /// <returns>True if there were any file blockings for this file</returns>
        private static bool TryFindFileBlockings(string path, out FileBlocking[] foundFileBlockings)
        {
            foundFileBlockings = fileBlockings.FindAll(fb => fb.Path == path).ToArray();

            return foundFileBlockings.Length > 0;
        }

        /// <summary>
        /// Saves all important information of a FileBlocking
        /// </summary>
        private struct FileBlocking
        {
            internal uint CodeInspectionID;
            internal string Path;
            internal InspectionMode Mode;
        }
        #endregion
    }
}
