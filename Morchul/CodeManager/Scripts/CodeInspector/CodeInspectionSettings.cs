using System;
using System.Text.RegularExpressions;

namespace Morchul.CodeManager
{
    /// <summary>
    /// Settings for CodeInspection
    /// </summary>
    public struct CodeInspectionSettings
    {
        /// <summary>
        /// Regex options applied in Find and FindAll of CodeInspection
        /// </summary>
        public RegexOptions RegexOptions;

        /// <summary>
        /// Regex timeout applied in Find and FindAll of CodeInspection
        /// </summary>
        public TimeSpan RegexTimeout;

        /// <summary>
        /// If set to true you can calculate the LineIndex of a CodePiece with codeInspection.GetLineIndex(CodePiece)
        /// And CodePiece has an additional field LineCount
        /// Set to false if you do not need it for better performance
        /// </summary>
        public bool AddLineIndex;

        public static CodeInspectionSettings Default = new CodeInspectionSettings()
            {
                RegexOptions = RegexOptions.None,
                RegexTimeout = TimeSpan.Zero,
                AddLineIndex = false
            };
    }
}
