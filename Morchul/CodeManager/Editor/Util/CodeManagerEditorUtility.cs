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
        /// <returns>True if the folder is under Assets and exists</returns>
        public static bool IsValidAssetsFolderPath(string path)
        {
            if(Regex.IsMatch(path, @"^Assets/.*/$"))
            {
                return AssetDatabase.IsValidFolder(path.Remove(path.Length - 1));
            }
            return false;
        }

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

        /// <summary>
        /// Calculates the Width of a text in pixel.
        /// </summary>
        /// <param name="text">The text which width should be calculated</param>
        /// <param name="font">The font used by the text</param>
        /// <param name="fontSize">The fontsize of the text</param>
        /// <param name="fontstyle">The fontstyle of the text default = Normal</param>
        /// <returns></returns>
        public static int CalcTextWidth(string text, Font font, int fontSize, FontStyle fontstyle = FontStyle.Normal)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            int w = 0;
            font.RequestCharactersInTexture(text, fontSize, fontstyle);

            foreach (char c in text)
            {
                font.GetCharacterInfo(c, out CharacterInfo cInfo, fontSize);
                w += cInfo.advance;
            }

            return w;
        }

    }
}

#endif