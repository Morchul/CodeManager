using System;

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

        #region Regexes
        public const string FieldRegex = @"";
        public const string MethodRegex = @"\b(?<access>public|private|protected|internal)\s*\b(async)?\s*(?<modifier>static|virtual|abstract)?\s*\b(?<return>[A-Za-z0-9_\[\]\<\>]+)\s*\b(?<method>[A-Za-z_][A-Za-z_0-9]*\s*)(?<generic>\<[A-Z]\>)?\(\s*((params)?\s*(\b[A-Za-z_][A-Za-z_0-9\[\]\<\>]*)\s*(\b[A-Za-z_][A-Za-z_0-9]*)\s*\,?\s*)*\s*\)";
        public const string ClassRegex = @"";
        #endregion

        public static string ConvertToOpertingSystemPath(string str)
        {
            if (str[str.Length - 1] == '/')
                return str.Remove(str.Length - 1);
            else
                return str;
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
    }
}
