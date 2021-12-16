using System.Text.RegularExpressions;

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
        public const string CodeManagerSettingsObject = CodeManagerResourcePath + "CodeManagerSettings.asset";

        public const string ScriptTemplateFolder = "ScriptTemplates";

        public const string ScriptTemplateFolderPath = "Assets/" + ScriptTemplateFolder;

        //public const string CodeManagerManualPath = CodeManagerPath + "Documentation/CodeManagerManual.pdf";
        //public const string CodeManagerAPIPath = CodeManagerPath + "Documentation/API/html/index.html";
        #endregion

        /// <summary>
        /// Converts a path to a operating system Path (Removes the / at the end of the path)
        /// </summary>
        /// <param name="str">The path used in CodeManager</param>
        /// <returns>returns the path without / at the end</returns>
        public static string ConvertToOpertingSystemPath(string str)
        {
            if (str[str.Length - 1] == '/')
                return str.Remove(str.Length - 1);
            else
                return str;
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

        /// <summary>
        /// Returns the LineCount of a string and if the string ends with new line or not
        /// </summary>
        /// <param name="str">the string</param>
        /// <param name="lineCount">out the Count of lines in string</param>
        /// <returns>true if the string ends with a new Line</returns>
        public static bool GetLineCount(string str, out int lineCount)
        {
            bool endWithNewLine = true;
            int count = 0;
            if (!string.IsNullOrEmpty(str))
            {
                count = str.Length - str.Replace("\n", string.Empty).Length;

                // if the last char of the string is not a newline, make sure to count that line too
                if (str[str.Length - 1] != '\n')
                {
                    endWithNewLine = false;
                    ++count;
                }
            }

            lineCount = count;
            return endWithNewLine;
        }

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
            return path.Substring(path.Replace("\\", "/").LastIndexOf('/') + 1);
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
    }
}
