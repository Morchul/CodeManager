using UnityEngine;

namespace Morchul.CodeManager
{
    /// <summary>
    /// A CleanCodeViolation is created if a CleanCodeRule was violated.
    /// CleanCodeViolations will be shown in the CleanCode console.
    /// </summary>
    public class CleanCodeViolation
    {
        public readonly Object Script;
        public readonly int LineIndex;
        public readonly string Description;
        public readonly string ErrorMessage;

        public readonly Texture2D Image;

        public CleanCodeViolation(int lineIndex, Object scriptObject, string description, string errorMessage, Texture2D image)
        {
            LineIndex = lineIndex;
            Script = scriptObject;
            Description = description;
            Image = image;
            ErrorMessage = errorMessage;
        }

        /// <summary>
        /// Creates a CleanCodeViolation occured through a violation of a UnwantedCode rule
        /// </summary>
        /// <param name="lineIndex">The line where the violation occured</param>
        /// <param name="script">The script in which the violation occured</param>
        /// <param name="description">The description of the rule</param>
        /// <param name="scriptName">The name of the script where the violation occured</param>
        /// <param name="image">An UnwantedCode violation image</param>
        /// <returns>The CleanCodeViolation</returns>
        public static CleanCodeViolation CreateUnwantedCodeMessage(int lineIndex, Object script, string description, string scriptName, Texture2D image)
        {
            return new CleanCodeViolation(lineIndex, script, description, "Unwanted Code found in: " + scriptName + ": " + lineIndex, image);
        }

        /// <summary>
        /// Creates a CleanCodeViolation occured through a violation of a Code Documentation rule
        /// </summary>
        /// <param name="lineIndex">The line where the violation occured</param>
        /// <param name="script">The script in which the violation occured</param>
        /// <param name="description">The description of the rule</param>
        /// <param name="scriptName">The name of the script where the violation occured</param>
        /// <param name="image">An CodeDocumentation violation image</param>
        /// <returns>The CleanCodeViolation</returns>
        public static CleanCodeViolation CreateCodeDocumentationMessage(int lineIndex, Object script, string description, string scriptName, Texture2D image)
        {
            return new CleanCodeViolation(lineIndex, script, description, "Code not documented in: " + scriptName + ": " + lineIndex, image);
        }

        /// <summary>
        /// Creates a CleanCodeViolation occured through a violation of a Code Guideline rule
        /// </summary>
        /// <param name="lineIndex">The line where the violation occured</param>
        /// <param name="script">The script in which the violation occured</param>
        /// <param name="description">The description of the rule</param>
        /// <param name="scriptName">The name of the script where the violation occured</param>
        /// <param name="image">An CodeGuideline violation image</param>
        /// <returns>The CleanCodeViolation</returns>
        public static CleanCodeViolation CreateCodeGuidelineMessage(int lineIndex, Object script, string description, string scriptName, Texture2D image)
        {
            return new CleanCodeViolation(lineIndex, script, description, "Codeguideline violation in: " + scriptName + ": " + lineIndex, image);
        }
    }
}
