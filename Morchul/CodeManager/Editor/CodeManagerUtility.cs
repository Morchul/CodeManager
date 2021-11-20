
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Morchul.CodeManager
{
    /// <summary>
    /// Utility class for CodeManager
    /// </summary>
    public static class CodeManagerUtility
    {
        #region const Paths and FolderNames
        public const string CodeManagerPath = "Assets/Plugins/Morchul/CodeManager/";
        public const string CodeManagerResourcePath = CodeManagerPath + "Resources/";
        public const string ScriptTemplatePath = ScriptTemplateFolderPath + "/";
        public const string ScriptTemplateSettingsObject = CodeManagerResourcePath + "ScriptTemplateSettings.asset";
        public const string CleanCodeSettingsObject = CodeManagerResourcePath + "CleanCodeSettings.asset";

        public const string ScriptTemplateFolder = "ScriptTemplates";

        public const string ScriptTemplateFolderPath = "Assets/" + ScriptTemplateFolder;
        #endregion

        /// <summary>
        /// Returns the folder name of the path
        /// Assets/Scripts/Controller.cs => Assets/Scripts/
        /// </summary>
        /// <param name="path">The path</param>
        /// <returns>the folder path</returns>
        public static string GetFolderNameInPath(string path)
        {
            return path.Substring(0, path.LastIndexOf('/') + 1);
        }

        /// <summary>
        /// Returns file name with extension
        /// Assets/Scripts/Controller.cs => Controller.cs
        /// </summary>
        /// <param name="path">The path to file</param>
        /// <returns>Script name with extension</returns>
        public static string GetFileNameInPathWithExtension(string path)
        {
            return path.Substring(path.LastIndexOf('/') + 1);
        }

        /// <summary>
        /// Returns file name without extension
        /// Assets/Scripts/Controller.cs => Controller
        /// </summary>
        /// <param name="path">The path to file</param>
        /// <returns>Script name without extension</returns>
        public static string GetFileNameInPathWithoutExtension(string path)
        {
            string nameWithExtension = GetFileNameInPathWithExtension(path);
            return nameWithExtension.Substring(0, nameWithExtension.LastIndexOf('.'));
        }

        /// <summary>
        /// Tests if path points to a folder under Assets which already exists
        /// </summary>
        /// <param name="path">The path to folder</param>
        /// <returns>True if the folder is under Assets and exists</returns>
        public static bool IsValidAssetsFolderPath(string path)
        {
            if(Regex.IsMatch(path, @"^Assets/.*/$"))
            {
                return AssetDatabase.IsValidFolder(path.Remove(path.Length - 1));
            }
            return false;
        }

        /// <summary>
        /// Tests if path points to a folder under Assets/ScriptTemplates which already exists
        /// </summary>
        /// <param name="path">The path to folder</param>
        /// <returns>True if the folder is under Assets/ScriptTemplates and exists</returns>
        public static bool IsValidScriptTemplateFolderPath(string path)
        {
            if (Regex.IsMatch(path, @"^Assets\/ScriptTemplates.*\/$"))
            {
                return AssetDatabase.IsValidFolder(path.Remove(path.Length - 1));
            }
            return false;
        }

        /// <summary>
        /// Opens the explorer and let the user select a folder
        /// Folder has to be under Assets/ or a Error message will be shown and the defaultPath will be returned instead of the selected.
        /// </summary>
        /// <param name="defaultPath">The defaultPath if no valid path is selected by user</param>
        /// <returns>The selected folder path or defaultPath if no valid path is selected</returns>
        public static string SelectFolderInAssets(string defaultPath)
        {
            string path = EditorUtility.OpenFolderPanel("Select Folder", "", "");
            CodeInspection pathInspection = CodeInspector.InspectText(path);
            if(pathInspection.Find(@"Assets\/", out InspectionPart part))
            {
                pathInspection.DeleteBefore(part);
                pathInspection.Commit();
                path = pathInspection.CompleteCode;
                return path + "/";
            }
            else
            {
                Debug.LogError("Please select a folder under Assets/...");
                return defaultPath;
            }
        }

        /// <summary>
        /// Checks if a regex is valid or not
        /// </summary>
        /// <param name="regex">The regex which should be tested</param>
        /// <returns>True if the regex is valid</returns>
        public static bool IsValidRegex(string regex)
        {
            if (string.IsNullOrEmpty(regex)) return false;
            try
            {
                Regex.IsMatch("", regex);
                return true;
            }
            catch (System.ArgumentException)
            {
                return false;
            }
        }
    }
}

