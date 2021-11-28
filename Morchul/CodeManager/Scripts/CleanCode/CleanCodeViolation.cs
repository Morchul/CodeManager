using UnityEngine;

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

    public static CleanCodeViolation CreateUnwantedCodeMessage(int lineIndex, Object script, string description, string fileName, Texture2D image)
    {
        return new CleanCodeViolation(lineIndex, script, description, "Unwanted Code found in: " + fileName + ": " + lineIndex, image);
    }

    public static CleanCodeViolation CreateCodeDocumentationMessage(int lineIndex, Object script, string description, string fileName, Texture2D image)
    {
        return new CleanCodeViolation(lineIndex, script, description, "Code not documented in: " + fileName + ": " + lineIndex, image);
    }

    public static CleanCodeViolation CreateCodeGuidelineMessage(int lineIndex, Object script, string description, string fileName, Texture2D image)
    {
        return new CleanCodeViolation(lineIndex, script, description, "Codeguideline violation in: " + fileName + ": " + lineIndex, image);
    }
}
