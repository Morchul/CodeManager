namespace Morchul.CodeManager
{
    public static class DefaultSettings
    {

        #region Regexes
        public const string AccessRegex = @"\b(public\s*|private\s*|protected\s*|internal\s*)?";
        public const string ModifierRegex = @"\b(static\s*|virtual\s*|abstract\s*|readonly\s*|const\s*)?";
        public const string TypeRegex = @"\b([A-Za-z0-9_\[\]\<\>]+)";
        public const string IdentifierRegex = @"\b(?<identifier>[A-Za-z_][A-Za-z_0-9]*)";
        public const string GenericRegex = @"(\<[A-Z]\>)?";
        public const string ParameterRegex = @"\(\s*((params)?\s*(\b[A-Za-z_][A-Za-z_0-9\[\]\<\>]*)\s*(\b[A-Za-z_][A-Za-z_0-9]*)\s*\,?\s*)*\s*\)";


        public const string AccessRegex_Private = @"\b(private\s*)";

        public const string FieldRegex = AccessRegex + ModifierRegex + TypeRegex + @"\s*" + IdentifierRegex;
        public const string MethodRegex = AccessRegex + @"\b(async\s*)?" + ModifierRegex + TypeRegex + @"\s*" + IdentifierRegex + @"\s*" + GenericRegex + @"\s*" + ParameterRegex;
        public const string ClassRegex = @".*class.*" + IdentifierRegex;


        public const string DefaultDocumentationRegex = @"[^\S][\/]{2,3}.*\n?$";
        #endregion

        public static void SetDefaultCleanCodeSettings(CleanCodeSettings cleanCodeSettings)
        {
            //Init all arrays
            cleanCodeSettings.Regexes = new CodeManagerRegex[7];
            cleanCodeSettings.UnwantedCodes = new UnwantedCode[1];
            cleanCodeSettings.CodeGuidelines = new CodeGuideline[2];
            cleanCodeSettings.CodeDocumentations = new CodeDocumentation[1];

            //Set default regexes
            cleanCodeSettings.Regexes[0] = new CodeManagerRegex { Name = "Empty Codeblock regex", Regex = @"\{\s*\}" };
            cleanCodeSettings.Regexes[1] = new CodeManagerRegex { Name = "Method regex", Regex = MethodRegex };
            cleanCodeSettings.Regexes[2] = new CodeManagerRegex { Name = "Class regex", Regex = ClassRegex };
            cleanCodeSettings.Regexes[3] = new CodeManagerRegex { Name = "Field regex", Regex = FieldRegex };
            cleanCodeSettings.Regexes[4] = new CodeManagerRegex { Name = "Private Field regex", Regex = AccessRegex_Private + ModifierRegex + TypeRegex + @"\s*" + IdentifierRegex };
            cleanCodeSettings.Regexes[5] = new CodeManagerRegex { Name = "Method Name regex", Regex = @"\b[A-Z][a-zA-Z_0-9]*" };
            cleanCodeSettings.Regexes[6] = new CodeManagerRegex { Name = "Private field name regex", Regex = @"\b[a-z][a-zA-Z_0-9]*" };
            cleanCodeSettings.DocumentationRegex = new CodeManagerRegex() { Regex = DefaultDocumentationRegex };

            //Set default unwanted Codes
            cleanCodeSettings.UnwantedCodes[0] = new UnwantedCode { Name = "Empty Codeblock", Description = "No empty code blocks allowed.", RegexIndex = 0 };

            //Set default code guidelines
            cleanCodeSettings.CodeGuidelines[0] = new CodeGuideline
            {
                Name = "Method Name",
                Description = "Methods have to start with a capital letter",
                GroupName = "identifier",
                SearchRegexIndex = 1,
                MatchRegexIndex = 5
            };

            cleanCodeSettings.CodeGuidelines[1] = new CodeGuideline
            {
                Name = "Private field name",
                Description = "private fields have to start with a lowercase letter",
                GroupName = "identifier",
                SearchRegexIndex = 4,
                MatchRegexIndex = 6
            };

            //Set default code documentations
            cleanCodeSettings.CodeDocumentations[0] = new CodeDocumentation { Name = "Documentation on class", Description = "Classes must be documented", RegexIndex = 2 };
        }
    }
}
