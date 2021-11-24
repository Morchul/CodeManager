using UnityEngine;

public struct CleanCodeViolation
{
    public Object Script;
    public int LineIndex;
    public string ErrorMessage;

    public static CleanCodeViolation CreateDocumentationMissing(int lineIndex, Object script, string codeType, string codeName)
    {
        return new CleanCodeViolation()
        {
            LineIndex = lineIndex,
            Script = script,
            ErrorMessage = "Missing Documentation for " + codeType + ": " + codeName
        };
    }

    public static CleanCodeViolation CreateUnwantedCodeMessage(int lineIndex, Object script, string fileName)
    {
        return new CleanCodeViolation()
        {
            LineIndex = lineIndex,
            Script = script,
            ErrorMessage = "Unwanted Code in: " + fileName + " on line: " + lineIndex
        };
    }

    public static CleanCodeViolation CreateCodeGuidelineViolation(int lineIndex, Object script, string fileName)
    {
        return new CleanCodeViolation()
        {
            LineIndex = lineIndex,
            Script = script,
            ErrorMessage = "Code guideline violation: " + fileName + " on line: " + lineIndex
        };
    }
}
