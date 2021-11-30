using System;
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

        #region Image Paths
        public const string UnwantedCodeImage = CodeManagerUtility.CodeManagerResourcePath + "UnwantedCodeImage.PNG";
        public const string CodeGuidelineImage = CodeManagerUtility.CodeManagerResourcePath + "CodeGuidelineImage.png";
        public const string DocumentationImage = CodeManagerUtility.CodeManagerResourcePath + "CodeDocumentationImage.png";
        #endregion

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
            string path = EditorUtility.OpenFolderPanel("Select Folder", "Assets/Scripts", "");
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

