#if UNITY_EDITOR

using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Morchul.CodeManager
{
    /// <summary>
    /// Utility class for CodeManager using UnityEditor
    /// </summary>
    public static class CodeManagerEditorUtility
    {

        private static CodeManagerSettings instance;

        #region Image Paths
        public const string UnwantedCodeImagePath = CodeManagerUtility.CodeManagerResourcePath + "UnwantedCodeImage.PNG";
        public const string CodeGuidelineImagePath = CodeManagerUtility.CodeManagerResourcePath + "CodeGuidelineImage.png";
        public const string DocumentationImagePath = CodeManagerUtility.CodeManagerResourcePath + "CodeDocumentationImage.png";
        #endregion

        /// <summary>
        /// Loads the current CodemanagerSettings.
        /// </summary>
        /// <param name="createNewIfNotExist">If set to true new CodeManagerSettings will be created if they do not exist. If false the return value can be null</param>
        /// <returns>The loaded CodeManagerSettings (can be null if not found and createNewIfNotExist is false)</returns>
        public static CodeManagerSettings LoadSettings(bool createNewIfNotExist = false)
        {
            if (instance == null)
            {
                instance = AssetDatabase.LoadAssetAtPath<CodeManagerSettings>(CodeManagerUtility.CodeManagerSettingsObject);
                if (createNewIfNotExist && instance == null) //Settings do not exist create new
                {
                    instance = ScriptableObject.CreateInstance<CodeManagerSettings>();
                    DefaultSettings.SetDefaultSettings(instance);
                    AssetDatabase.CreateAsset(instance, CodeManagerUtility.CodeManagerSettingsObject);
                    instance.UpdateRules();
                }
            }
            return instance;
        }

        /// <summary>
        /// Tests if path points to a folder under Assets which already exists
        /// </summary>
        /// <param name="path">The path to folder</param>
        /// <returns>True if the folder is under Assets, exists and ends with /</returns>
        public static bool IsValidAssetsFolderPath(string path)
        {
            if(Regex.IsMatch(path, @"^Assets/.*/$"))
            {
                return AssetDatabase.IsValidFolder(path.Remove(path.Length - 1));
            }
            return false;
        }

        /// <summary>
        /// Tests if path points to a valid folder and ends with /
        /// </summary>
        /// <param name="path">The path to folder</param>
        /// <returns>True if the folder exists and ends with /</returns>
        public static bool IsValidFolderPath(string path)
        {
            if (Regex.IsMatch(path, @"/$"))
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
            string folderPath = "Assets/Scripts";
            if (!AssetDatabase.IsValidFolder(folderPath))
                folderPath = "Assets";
            string path = EditorUtility.OpenFolderPanel("Select Folder", folderPath, "");
            CodeInspection pathInspection = CodeInspector.InspectText(path);

            if (string.IsNullOrEmpty(path)) return defaultPath;

            if(pathInspection.Find(@"Assets\/", out LinkedListNode<CodePiece> part))
            {
                part.Previous.Value.Code = "";
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
    }
}

#endif